using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Avc;
using CKAN.Versioning;
using CKAN.NetKAN.Sources.Github;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that populates version data from an AVC version file included in the
    /// distribution package.
    /// </summary>
    internal sealed class AvcTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AvcTransformer));

        private readonly IHttpService   _http;
        private readonly IModuleService _moduleService;
        private readonly IGithubApi     _github;

        public string Name { get { return "avc"; } }

        public AvcTransformer(IHttpService http, IModuleService moduleService, IGithubApi github)
        {
            _http          = http;
            _moduleService = moduleService;
            _github        = github;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Vref != null && metadata.Vref.Source == "ksp-avc")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing internal AVC transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var noVersion = metadata.Version == null;

                if (noVersion)
                {
                    json["version"] = "0"; // TODO: DBB: Dummy version necessary to the next statement doesn't throw
                }

                var mod = CkanModule.FromJson(json.ToString());

                if (noVersion)
                {
                    json.Remove("version");
                }

                var file = _http.DownloadPackage(metadata.Download, metadata.Identifier, metadata.RemoteTimestamp);
                var avc = _moduleService.GetInternalAvc(mod, file, metadata.Vref.Id);

                if (avc != null)
                {
                    Log.Info("Found internal AVC version file");

                    var remoteUri = GetRemoteAvcUri(avc);

                    if (remoteUri != null)
                    {
                        try
                        {
                            var remoteJson = _github?.DownloadText(remoteUri)
                                ?? _http.DownloadText(remoteUri);
                            var remoteAvc = JsonConvert.DeserializeObject<AvcVersion>(remoteJson);

                            if (avc.version.CompareTo(remoteAvc.version) == 0)
                            {
                                // Local AVC and Remote AVC describe the same version, prefer
                                Log.Info("Remote AVC version file describes same version as local AVC version file, using it preferrentially.");
                                avc = remoteAvc;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.InfoFormat("An error occured fetching the remote AVC version file, ignoring: {0}", e.Message);
                            Log.Debug(e);
                        }
                    }

                    ApplyVersions(json, avc);

                    // It's cool if we don't have version info at all, it's optional in the AVC spec.

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                }

                yield return new Metadata(json);
            }
            else
            {
                yield return metadata;
            }
        }

        public static void ApplyVersions(JObject json, AvcVersion avc)
        {
            // Get the minimum and maximum KSP versions that already exist in the metadata.
            // Use specific KSP version if min/max don't exist.
            var existingKspMinStr = (string)json["ksp_version_min"] ?? (string)json["ksp_version"];
            var existingKspMaxStr = (string)json["ksp_version_max"] ?? (string)json["ksp_version"];

            var existingKspMin = existingKspMinStr == null ? null : KspVersion.Parse(existingKspMinStr);
            var existingKspMax = existingKspMaxStr == null ? null : KspVersion.Parse(existingKspMaxStr);

            // Get the minimum and maximum KSP versions that are in the AVC file.
            // https://github.com/linuxgurugamer/KSPAddonVersionChecker/blob/master/KSP-AVC.schema.json
            // KSP-AVC allows KSP_VERSION to be set
            // when KSP_VERSION_MIN/_MAX are set, but CKAN treats
            // its equivalent properties as mutually exclusive.
            // Only fallback if neither min nor max are defined,
            // for open ranges.
            KspVersion avcKspMin, avcKspMax;
            if (avc.ksp_version_min == null && avc.ksp_version_max == null)
            {
                // Use specific KSP version if min/max don't exist
                avcKspMin = avcKspMax = avc.ksp_version;
            }
            else
            {
                avcKspMin = avc.ksp_version_min;
                avcKspMax = avc.ksp_version_max;
            }

            // Now calculate the minimum and maximum KSP versions between both the existing metadata and the
            // AVC file.
            var kspMins  = new List<KspVersion>();
            var kspMaxes = new List<KspVersion>();

            if (!KspVersion.IsNullOrAny(existingKspMin))
                kspMins.Add(existingKspMin);

            if (!KspVersion.IsNullOrAny(avcKspMin))
                kspMins.Add(avcKspMin);

            if (!KspVersion.IsNullOrAny(existingKspMax))
                kspMaxes.Add(existingKspMax);

            if (!KspVersion.IsNullOrAny(avcKspMax))
                kspMaxes.Add(avcKspMax);

            var kspMin = kspMins.Any()  ? kspMins.Min()  : null;
            var kspMax = kspMaxes.Any() ? kspMaxes.Max() : null;

            if (kspMin != null || kspMax != null)
            {
                // If we have either a minimum or maximum KSP version, remove all existing KSP version
                // information from the metadata.
                json.Remove("ksp_version");
                json.Remove("ksp_version_min");
                json.Remove("ksp_version_max");

                if (kspMin != null && kspMax != null)
                {
                    // If we have both a minimum and maximum KSP version...
                    if (kspMin.Equals(kspMax))
                    {
                        // ...and they are equal, then just set ksp_version
                        json["ksp_version"] = kspMin.ToString();
                    }
                    else
                    {
                        // ...otherwise set both ksp_version_min and ksp_version_max
                        json["ksp_version_min"] = kspMin.ToString();
                        json["ksp_version_max"] = kspMax.ToString();
                    }
                }
                else
                {
                    // If we have only one or the other then set which ever is applicable

                    if (kspMin != null)
                        json["ksp_version_min"] = kspMin.ToString();

                    if (kspMax != null)
                        json["ksp_version_max"] = kspMax.ToString();
                }
            }

            if (avc.version != null)
            {
                // In practice, the version specified in .version files tends to be unreliable, with authors
                // forgetting to update it when new versions are released. Therefore if we have a version
                // specified from another source such as SpaceDock, curse or a GitHub tag, don't overwrite it
                // unless x_netkan_trust_version_file is true.
                if ((bool?)json["x_netkan_trust_version_file"] ?? false)
                {
                    json.Remove("version");
                }
                json.SafeAdd("version", avc.version.ToString());
            }
        }

        private static Uri GetRemoteAvcUri(AvcVersion avc)
        {
            if (!Uri.IsWellFormedUriString(avc.Url, UriKind.Absolute))
                return null;

            var remoteUri = new Uri(avc.Url);

            Log.InfoFormat("Remote AVC version file at: {0}", remoteUri);

            return CKAN.Net.GetRawUri(remoteUri);
        }
    }
}
