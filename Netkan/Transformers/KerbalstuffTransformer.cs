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
                json.SafeAdd("license", ksMod.license);
                json.SafeAdd("download", latestVersion.download_path.OriginalString);
                json.SafeAdd("x_screenshot", Escape(ksMod.background));

                // Make sure resources exist.
                if (json["resources"] == null)
                {
                    json["resources"] = new JObject();
                }

                var resourcesJson = (JObject)json["resources"];

                resourcesJson.SafeAdd("homepage", Escape(ksMod.website));
                resourcesJson.SafeAdd("repository", Escape(ksMod.source_code));
                resourcesJson.SafeAdd("kerbalstuff", ksMod.GetPageUrl().OriginalString);

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }

        /// <summary>
        /// Provide an escaped version of the given URL, including converting
        /// square brackets to their escaped forms.
        /// </summary>
        private static string Escape(Uri url)
        {
            if (url == null)
            {
                return null;
            }

            Log.DebugFormat("Escaping {0}", url);

            var escaped = Uri.EscapeUriString(url.ToString());

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

            Log.DebugFormat("Escaped to {0}", escaped);

            return escaped;
        }
    }
}
