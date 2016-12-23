﻿using System;
using System.Collections.Generic;
using System.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Avc;
using CKAN.Versioning;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that populates version data from an AVC version file included in the
    /// distribution package.
    /// </summary>
    internal sealed class AvcTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AvcTransformer));

        private readonly IHttpService _http;
        private readonly IModuleService _moduleService;

        public string Name { get { return "avc"; } }

        public AvcTransformer(IHttpService http, IModuleService moduleService)
        {
            _http = http;
            _moduleService = moduleService;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Vref != null && metadata.Vref.Source == "ksp-avc")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Interal AVC transformation with {0}", metadata.Kref);
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

                var file = _http.DownloadPackage(metadata.Download, metadata.Identifier);
                var avc = _moduleService.GetInternalAvc(mod, file, metadata.Vref.Id);

                if (avc != null)
                {
                    Log.Info("Found internal AVC version file");

                    if (Uri.IsWellFormedUriString(avc.Url, UriKind.Absolute))
                    {
                        Log.InfoFormat("Found remote AVC version file at {0}", avc.Url);

                        try
                        {
                            var remoteJson = Net.DownloadText(avc.Url);
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

                    // Get the minimum and maximum KSP versions that already exist in the metadata.
                    // Use specific KSP version if min/max don't exist.
                    var existingKspMinStr = (string)json["ksp_version_min"] ?? (string)json["ksp_version"];
                    var existingKspMaxStr = (string)json["ksp_version_max"] ?? (string)json["ksp_version"];

                    var existingKspMin = existingKspMinStr == null ? null : KspVersion.Parse(existingKspMinStr);
                    var existingKspMax = existingKspMaxStr == null ? null : KspVersion.Parse(existingKspMaxStr);
                    
                    // Get the minimum and maximum KSP versions that are in the AVC file.
                    // Use specific KSP version if min/max don't exist.
                    var avcKspMin = avc.ksp_version_min ?? avc.ksp_version;
                    var avcKspMax = avc.ksp_version_max ?? avc.ksp_version;

                    // Now calculate the minimum and maximum KSP versions between both the existing metadata and the
                    // AVC file.
                    var kspMins = new List<KspVersion>();
                    var kspMaxes = new List<KspVersion>();

                    if (existingKspMin != null)
                        kspMins.Add(existingKspMin);

                    if (avcKspMin != null)
                        kspMins.Add(avcKspMin);

                    if (existingKspMax != null)
                        kspMaxes.Add(existingKspMax);

                    if (avcKspMax != null)
                        kspMaxes.Add(avcKspMax);

                    var kspMin = kspMins.Any() ? kspMins.Min() : null;
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
                            if (kspMin.CompareTo(kspMax) == 0)
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
                        // specified from another source such as SpaceDock, curse or a GitHub tag, don't overwrite it.
                        json.SafeAdd("version", avc.version.ToString());
                    }

                    // It's cool if we don't have version info at all, it's optional in the AVC spec.

                    Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);
                }

                return new Metadata(json);
            }
            else
            {
                return metadata;
            }
        }
    }
}
