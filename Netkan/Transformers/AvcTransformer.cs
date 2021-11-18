﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Validators;
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
        private readonly VrefValidator  _vrefValidator;

        public string Name { get { return "avc"; } }

        public AvcTransformer(IHttpService http, IModuleService moduleService, IGithubApi github)
        {
            _http          = http;
            _moduleService = moduleService;
            _github        = github;
            _vrefValidator = new VrefValidator(_http, _moduleService);
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            _vrefValidator.Validate(metadata);

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

                var file = _http.DownloadModule(metadata);
                var avc = _moduleService.GetInternalAvc(mod, file, metadata.Vref.Id);

                if (avc != null)
                {
                    Log.Info("Found internal AVC version file");

                    var remoteUri = GetRemoteAvcUri(avc);

                    if (remoteUri != null)
                    {
                        if (json["resources"] == null)
                            json["resources"] = new JObject();
                        var resourcesJson = (JObject)json["resources"];
                        resourcesJson.SafeAdd("remote-avc", remoteUri.OriginalString);

                        try
                        {
                            var remoteJson = _github?.DownloadText(remoteUri)
                                ?? _http.DownloadText(remoteUri);

                            var rootToken = JToken.Parse(remoteJson, new JsonLoadSettings { CommentHandling = CommentHandling.Ignore });

                            if (rootToken.Type == JTokenType.Object)
                            {
                                var remoteAvc = rootToken.ToObject<AvcVersion>();
                                if (avc.version.CompareTo(remoteAvc.version) == 0)
                                {
                                    // Local AVC and Remote AVC describe the same version, prefer
                                    Log.Info("Remote AVC version file describes same version as local AVC version file, using it preferrentially.");
                                    avc = remoteAvc;
                                }
                            }
                            else if (rootToken.Type == JTokenType.Array)
                            {
                                var remoteAvcs = rootToken.ToObject<AvcVersion[]>();
                                AvcVersion matchedVersion = null;
                                foreach (var remoteAvc in remoteAvcs)
                                {
                                    if (avc.version.CompareTo(remoteAvc.version) != 0) continue;
                                    if (matchedVersion != null) throw new InvalidOperationException("More than one matching version found in remote AVC file");
                                    matchedVersion = remoteAvc;
                                }
                                if (matchedVersion == null) throw new InvalidOperationException("No matching version found in remote AVC file");
                                avc = matchedVersion;
                            }
                            else
                                throw new InvalidOperationException("Invalid root token in remote AVC: " + rootToken.Type.ToString());
                        }
                        catch (JsonReaderException e)
                        {
                            Log.WarnFormat("Error parsing remote version file {0}: {1}",
                                remoteUri, e.Message);
                            Log.Debug(e);
                        }
                        catch (Exception e)
                        {
                            Log.WarnFormat("Error fetching remote version file {0}: {1}", remoteUri, e.Message);
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
            ModuleService.ApplyVersions(json, avc.ksp_version, avc.ksp_version_min, avc.ksp_version_max);

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
            {
                if (avc.Url != null)
                {
                    Log.WarnFormat("Version file URL property is invalid: {0}", avc.Url);
                }
                return null;
            }

            var remoteUri = new Uri(avc.Url);

            Log.InfoFormat("Remote AVC version file at: {0}", remoteUri);

            return CKAN.Net.GetRawUri(remoteUri);
        }
    }
}
