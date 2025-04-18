using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Curse;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from Curse.
    /// </summary>
    internal sealed class CurseTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CurseTransformer));

        private readonly ICurseApi _api;
        private readonly string?   _userAgent;

        public string Name => "curse";

        public CurseTransformer(ICurseApi api, string? userAgent = null)
        {
            _api       = api;
            _userAgent = userAgent;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref?.Source == "curse" && metadata.Kref.Id != null)
            {
                Log.InfoFormat("Executing Curse transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                // Look up our mod on Curse by its Id.
                var curseMod = _api.GetMod(metadata.Kref.Id);
                if (curseMod == null)
                {
                    throw new Kraken("Failed to get mod from Curse!");
                }
                var versions = curseMod.All();
                if (opts.SkipReleases != null)
                {
                    versions = versions.Skip(opts.SkipReleases.Value);
                }
                if (opts.Releases != null)
                {
                    versions = versions.Take(opts.Releases.Value);
                }
                bool returnedAny = false;
                foreach (CurseFile f in versions)
                {
                    returnedAny = true;
                    yield return TransformOne(metadata.Json(), curseMod, f);
                }
                if (!returnedAny)
                {
                    Log.WarnFormat("No releases found for {0}", curseMod.ToString());
                    yield return metadata;
                }
            }
            else
            {
                yield return metadata;
            }
        }

        private Metadata TransformOne(JObject json, CurseMod curseMod, CurseFile latestVersion)
        {
            Log.InfoFormat("Found Curse mod: {0} {1}", curseMod.GetName(), latestVersion.GetFileVersion(_userAgent));

            // Only pre-fill version info if there's none already. GH #199
            if (json["ksp_version_min"] == null && json["ksp_version_max"] == null && json["ksp_version"] == null)
            {
                var minVer = latestVersion.versions?.Min();
                var maxVer = latestVersion.versions?.Max();
                if (minVer == maxVer)
                {
                    Log.DebugFormat("Writing ksp_version from Curse: {0}", latestVersion.version);
                    json["ksp_version"] = latestVersion.version?.ToString();
                }
                else
                {
                    Log.DebugFormat("Writing ksp_version_min,_max from Curse: {0}, {1}", minVer, maxVer);
                    json["ksp_version_min"] = minVer?.ToString();
                    json["ksp_version_max"] = maxVer?.ToString();
                }
            }

            var useDownloadNameVersion = false;
            var useFilenameVersion = false;
            var useCurseIdVersion = false;

            var curseMetadata = (JObject?)json["x_netkan_curse"];
            if (curseMetadata != null)
            {
                var useDownloadNameVersionMetadata = (bool?)curseMetadata["use_download_name_version"];
                if (useDownloadNameVersionMetadata != null)
                {
                    useDownloadNameVersion = useDownloadNameVersionMetadata.Value;
                }

                var useFilenameVersionMetadata = (bool?) curseMetadata["use_filename_version"];
                if (useFilenameVersionMetadata != null)
                {
                    useFilenameVersion = useFilenameVersionMetadata.Value;
                }

                var useCurseIdVersionMetadata = (bool?)curseMetadata["use_curse_id_version"];
                if (useCurseIdVersionMetadata != null)
                {
                    useCurseIdVersion = useCurseIdVersionMetadata.Value;
                }

                if ((useDownloadNameVersion ? 1 : 0) + (useFilenameVersion ? 1 : 0) + (useCurseIdVersion ? 1 : 0) > 1)
                {
                    throw new Kraken("Conflicting version options set in x_netkan_curse");
                }
            }

            json.SafeAdd("name", curseMod.GetName());
            json.SafeAdd("abstract", curseMod.description);

            if (useDownloadNameVersion)
            {
                json.SafeAdd("version", latestVersion.name);
            }
            else if (useFilenameVersion)
            {
                json.SafeAdd("version", latestVersion.GetFilename(_userAgent));
            }
            else if (useCurseIdVersion)
            {
                json.SafeAdd("version", latestVersion.GetCurseIdVersion());
            }
            else
            {
                json.SafeAdd("version", latestVersion.GetFileVersion(_userAgent));
            }

            json.SafeAdd("author",   () => JToken.FromObject(curseMod.authors));
            json.Remove("$kref");
            json.SafeAdd("download", Regex.Replace(latestVersion.GetDownloadUrl(_userAgent), " ", "%20"));

            // Curse provides users with the following default selection of licenses. Let's convert them to CKAN
            // compatible license strings if possible.
            //
            // "Academic Free License v3.0"                               - Becomes "AFL-3.0"
            // "Ace3 Style BSD"                                           - Becomes "restricted"
            // "All Rights Reserved"                                      - Becomes "restricted"
            // "Apache License version 2.0"                               - Becomes "Apache-2.0"
            // "Apple Public Source License version 2.0 (APSL)"           - Becomes "APSL-2.0"
            // "BSD License"                                              - Becomes "BSD-3-clause"
            // "Common Development and Distribution License (CDDL) "      - Becomes "CDDL"
            // "GNU Affero General Public License version 3 (AGPLv3)"     - Becomes "AGPL-3.0"
            // "GNU General Public License version 2 (GPLv2)"             - Becomes "GPL-2.0"
            // "GNU General Public License version 3 (GPLv3)"             - Becomes "GPL-3.0"
            // "GNU Lesser General Public License version 2.1 (LGPLv2.1)" - Becomes "LGPL-2.1"
            // "GNU Lesser General Public License version 3 (LGPLv3)"     - Becomes "LGPL-3.0"
            // "ISC License (ISCL)"                                       - Becomes "ISC"
            // "Microsoft Public License (Ms-PL)"                         - Becomes "Ms-PL"
            // "Microsoft Reciprocal License (Ms-RL)"                     - Becomes "Ms-RL"
            // "MIT License"                                              - Becomes "MIT"
            // "Mozilla Public License 1.0 (MPL)"                         - Becomes "MPL-1.0"
            // "Mozilla Public License 1.1 (MPL 1.1)"                     - Becomes "MPL-1.1"
            // "Public Domain"                                            - Becomes "public-domain"
            // "WTFPL"                                                    - Becomes "WTFPL"
            // "zlib/libpng License"                                      - Becomes "Zlib"
            // "Custom License"                                           - Becomes "unknown"

            var curseLicense = curseMod.license?.Trim();

            switch (curseLicense)
            {
                case "Academic Free License v3.0":
                    json.SafeAdd("license", "AFL-3.0");
                    break;
                case "Ace3 Style BSD":
                    json.SafeAdd("license", "restricted");
                    break;
                case "All Rights Reserved":
                    json.SafeAdd("license", "restricted");
                    break;
                case "Apache License version 2.0":
                    json.SafeAdd("license", "Apache-2.0");
                    break;
                case "Apple Public Source License version 2.0 (APSL)":
                    json.SafeAdd("license", "APSL-2.0");
                    break;
                case "BSD License":
                    json.SafeAdd("license", "BSD-3-clause");
                    break;
                case "Common Development and Distribution License (CDDL) ":
                    json.SafeAdd("license", "CDDL");
                    break;
                case "GNU Affero General Public License version 3 (AGPLv3)":
                    json.SafeAdd("license", "AGPL-3.0");
                    break;
                case "GNU General Public License version 2 (GPLv2)":
                    json.SafeAdd("license", "GPL-2.0");
                    break;
                case "GNU General Public License version 3 (GPLv3)":
                    json.SafeAdd("license", "GPL-3.0");
                    break;
                case "GNU Lesser General Public License version 2.1 (LGPLv2.1)":
                    json.SafeAdd("license", "LGPL-2.1");
                    break;
                case "GNU Lesser General Public License version 3 (LGPLv3)":
                    json.SafeAdd("license", "LGPL-3.0");
                    break;
                case "ISC License (ISCL)":
                    json.SafeAdd("license", "ISC");
                    break;
                case "Microsoft Public License (Ms-PL)":
                    json.SafeAdd("license", "Ms-PL");
                    break;
                case "Microsoft Reciprocal License (Ms-RL)":
                    json.SafeAdd("license", "Ms-RL");
                    break;
                case "MIT License":
                    json.SafeAdd("license", "MIT");
                    break;
                case "Mozilla Public License 1.0 (MPL)":
                    json.SafeAdd("license", "MPL-1.0");
                    break;
                case "Mozilla Public License 1.1 (MPL 1.1)":
                    json.SafeAdd("license", "MPL-1.1");
                    break;
                case "Public Domain":
                    json.SafeAdd("license", "public-domain");
                    break;
                case "WTFPL":
                    json.SafeAdd("license", "WTFPL");
                    break;
                case "zlib/libpng License":
                    json.SafeAdd("license", "Zlib");
                    break;
                default:
                    json.SafeAdd("license", "unknown");
                    break;
            }

            // Make sure resources exist.
            if (json["resources"] == null)
            {
                json["resources"] = new JObject();
            }

            var resourcesJson = (JObject?)json["resources"];

            //resourcesJson.SafeAdd("homepage", Normalize(curseMod.website));
            //resourcesJson.SafeAdd("repository", Normalize(curseMod.source_code));
            resourcesJson?.SafeAdd("curse", curseMod.GetProjectUrl());

            if (curseMod.thumbnail != null)
            {
                resourcesJson?.SafeAdd("x_screenshot", Normalize(new Uri(curseMod.thumbnail)));
            }

            Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

            return new Metadata(json);
        }

        private static string? Normalize(Uri uri)
            => Net.NormalizeUri(uri.ToString());
    }
}
