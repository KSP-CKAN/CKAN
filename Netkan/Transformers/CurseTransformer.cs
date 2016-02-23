using System;
using System.Text.RegularExpressions;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Curse;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from Curse.
    /// </summary>
    internal sealed class CurseTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CurseTransformer));

        private readonly ICurseApi _api;

        public CurseTransformer(ICurseApi api)
        {
            _api = api;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "curse")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing SpaceDock transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                // Look up our mod on SD by its Id.
                var curseMod = _api.GetMod(Convert.ToInt32(metadata.Kref.Id));
                var latestVersion = curseMod.Latest();

                Log.InfoFormat("Found SpaceDock Mod: {0} {1}", curseMod.title);

                // Only pre-fill version info if there's none already. GH #199
                if (json["ksp_version_min"] == null && json["ksp_version_max"] == null && json["ksp_version"] == null)
                {
                    Log.DebugFormat("Writing ksp_version from SpaceDock: {0}", latestVersion.version);
                    json["ksp_version"] = latestVersion.version;
                }

                json.SafeAdd("name", curseMod.title);
                //json.SafeAdd("abstract", cMod.short_description);
                json.SafeAdd("version", "0");
                json.SafeAdd("author", curseMod.authors[0]);
                json.SafeAdd("download", curseMod.GetDownloadUrl(latestVersion));

                // SD provides users with the following default selection of licenses. Let's convert them to CKAN
                // compatible license strings if possible.
                //
                // "MIT" - OK
                // "BSD" - Specific version is indeterminate
                // "GPLv2" - Becomes "GPL-2.0"
                // "GPLv3" - Becomes "GPL-3.0"
                // "LGPL" - Specific version is indeterminate

                var sdLicense = curseMod.license.Trim();

                switch (sdLicense)
                {
                    case "GPLv2":
                        json.SafeAdd("license", "GPL-2.0");
                        break;
                    case "GPLv3":
                        json.SafeAdd("license", "GPL-3.0");
                        break;
                    default:
                        json.SafeAdd("license", sdLicense);
                        break;
                }

                // Make sure resources exist.
                if (json["resources"] == null)
                {
                    json["resources"] = new JObject();
                }

                var resourcesJson = (JObject)json["resources"];

                //resourcesJson.SafeAdd("homepage", Escape(cMod.website));
                //resourcesJson.SafeAdd("repository", Escape(cMod.source_code));
                resourcesJson.SafeAdd("curse", curseMod.project_url);

                if (curseMod.thumbnail != null)
                {
                    resourcesJson.SafeAdd("x_screenshot", Escape(new Uri(curseMod.thumbnail)));
                }

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
            // just have crazy ideas as to what should be in a URL, and SD doesn't
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
