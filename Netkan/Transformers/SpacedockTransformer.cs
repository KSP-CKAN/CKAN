﻿using System;
using System.Text.RegularExpressions;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Spacedock;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from Spacedock.
    /// </summary>
    internal sealed class SpacedockTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SpacedockTransformer));

        private readonly ISpacedockApi _api;

        public SpacedockTransformer(ISpacedockApi api)
        {
            _api = api;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "spacedock")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Spacedock transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                // Look up our mod on SD by its Id.
                var sdMod = _api.GetMod(Convert.ToInt32(metadata.Kref.Id));
                var latestVersion = sdMod.Latest();

                Log.InfoFormat("Found Spacedock Mod: {0} {1}", sdMod.name, latestVersion.friendly_version);

                // Only pre-fill version info if there's none already. GH #199
                if (json["ksp_version_min"] == null && json["ksp_version_max"] == null && json["ksp_version"] == null)
                {
                    Log.DebugFormat("Writing ksp_version from Spacedock: {0}", latestVersion.KSP_version);
                    json["ksp_version"] = latestVersion.KSP_version.ToString();
                }

                json.SafeAdd("name", sdMod.name);
                json.SafeAdd("abstract", sdMod.short_description);
                json.SafeAdd("version", latestVersion.friendly_version.ToString());
                json.SafeAdd("author", sdMod.author);
                json.SafeAdd("download", latestVersion.download_path.OriginalString);

                // SD provides users with the following default selection of licenses. Let's convert them to CKAN
                // compatible license strings if possible.
                //
                // "MIT" - OK
                // "BSD" - Specific version is indeterminate
                // "GPLv2" - Becomes "GPL-2.0"
                // "GPLv3" - Becomes "GPL-3.0"
                // "LGPL" - Specific version is indeterminate

                var sdLicense = sdMod.license.Trim();

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

                resourcesJson.SafeAdd("homepage", Escape(sdMod.website));
                resourcesJson.SafeAdd("repository", Escape(sdMod.source_code));
                resourcesJson.SafeAdd("spacedock", sdMod.GetPageUrl().OriginalString);

                if (sdMod.background != null)
                {
                    resourcesJson.SafeAdd("x_screenshot", Escape(sdMod.background));
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
