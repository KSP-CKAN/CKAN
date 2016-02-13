using System;
using System.Text.RegularExpressions;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Kerbalstuff;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from Kerbal Stuff.
    /// </summary>
    internal sealed class KerbalstuffTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KerbalstuffTransformer));

        private readonly IKerbalstuffApi _api;

        public KerbalstuffTransformer(IKerbalstuffApi api)
        {
            _api = api;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "kerbalstuff")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing KerbalStuff transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                // Look up our mod on KS by its Id.
                var ksMod = _api.GetMod(Convert.ToInt32(metadata.Kref.Id));
                var latestVersion = ksMod.Latest();

                Log.InfoFormat("Found KerbalStuff Mod: {0} {1}", ksMod.name, latestVersion.friendly_version);

                // Only pre-fill version info if there's none already. GH #199
                if (json["ksp_version_min"] == null && json["ksp_version_max"] == null && json["ksp_version"] == null)
                {
                    Log.DebugFormat("Writing ksp_version from Kerbal Stuff: {0}", latestVersion.KSP_version);
                    json["ksp_version"] = latestVersion.KSP_version.ToString();
                }

                json.SafeAdd("name", ksMod.name);
                json.SafeAdd("abstract", ksMod.short_description);
                json.SafeAdd("version", latestVersion.friendly_version.ToString());
                json.SafeAdd("author", ksMod.author);
                json.SafeAdd("download", latestVersion.download_path.OriginalString);

                // KS provides users with the following default selection of licenses. Let's convert them to CKAN
                // compatible license strings if possible.
                //
                // "MIT" - OK
                // "BSD" - Specific version is indeterminate
                // "GPLv2" - Becomes "GPL-2.0"
                // "GPLv3" - Becomes "GPL-3.0"
                // "LGPL" - Specific version is indeterminate

                var ksLicense = ksMod.license.Trim();

                switch (ksLicense)
                {
                    case "GPLv2":
                        json.SafeAdd("license", "GPL-2.0");
                        break;
                    case "GPLv3":
                        json.SafeAdd("license", "GPL-3.0");
                        break;
                    default:
                        json.SafeAdd("license", ksLicense);
                        break;
                }

                // Make sure resources exist.
                if (json["resources"] == null)
                {
                    json["resources"] = new JObject();
                }

                var resourcesJson = (JObject)json["resources"];

                resourcesJson.SafeAdd("homepage", Normalize(ksMod.website));
                resourcesJson.SafeAdd("repository", Normalize(ksMod.source_code));
                resourcesJson.SafeAdd("kerbalstuff", ksMod.GetPageUrl().OriginalString);

                if (ksMod.background != null)
                {
                    resourcesJson.SafeAdd("x_screenshot", Normalize(ksMod.background));
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }

        private static string Normalize(Uri uri)
        {
            return Normalize(uri.ToString());
        }

        /// <summary>
        /// Provide an escaped version of the given Uri string, including converting  square brackets to their escaped
        /// forms.
        /// </summary>
        /// <returns>
        /// <c>null</c> if the string is not a valid <see cref="Uri"/>, otherwise its normlized form.
        /// </returns>
        private static string Normalize(string uri)
        {
            if (uri == null)
            {
                return null;
            }

            Log.DebugFormat("Escaping {0}", uri);

            var escaped = Uri.EscapeUriString(uri);

            // Square brackets are "reserved characters" that should not appear
            // in strings to begin with, so C# doesn't try to escape them in case
            // they're being used in a special way. They're not; some mod authors
            // just have crazy ideas as to what should be in a URL, and KS doesn't
            // escape them in its API. There's probably more in RFC 3986.

            escaped = escaped.Replace("[", Uri.HexEscape('['));
            escaped = escaped.Replace("]", Uri.HexEscape(']'));

            // Make sure we have a "http://" or "https://" start.
            if (!Regex.IsMatch(escaped, "(?i)^(http|https)://"))
            {
                // Prepend "http://", as we do not know if the site supports https.
                escaped = "http://" + escaped;
            }

            if (Uri.IsWellFormedUriString(escaped, UriKind.Absolute))
            {
                Log.DebugFormat("Escaped to {0}", escaped);
                return escaped;
            }
            else
            {
                Log.WarnFormat("Could not normalize URL: {0}", uri);
                return null;
            }
        }
    }
}
