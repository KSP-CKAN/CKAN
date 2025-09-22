using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Validators;
using CKAN.Avc;
using CKAN.NetKAN.Sources.Github;
using CKAN.Versioning;

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
        private readonly IGithubApi?    _github;
        private readonly VrefValidator  _vrefValidator;

        public string Name => "avc";

        public AvcTransformer(IHttpService   http,
                              IModuleService moduleService,
                              IGithubApi?    github)
        {
            _http          = http;
            _moduleService = moduleService;
            _github        = github;
            _vrefValidator = new VrefValidator(_http, _moduleService);
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            _vrefValidator.Validate(metadata);

            if (metadata.Vref?.Source == "ksp-avc")
            {

                Log.InfoFormat("Executing internal AVC transformation with {0}", metadata.Vref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, metadata.AllJson);

                var noVersion = metadata.Version == null;

                var json = metadata.Json();
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
                if (file != null
                    && _moduleService.GetInternalAvc(mod, file, metadata.Vref.Id) is AvcVersion avc)
                {
                    Log.Info("Found internal AVC version file");

                    var resourcesJson = (JObject?)json["resources"];
                    var remoteUri = resourcesJson?["remote-avc"] != null
                        && (string?)resourcesJson["remote-avc"] is string s
                            ? new Uri(s)
                            : GetRemoteAvcUri(avc);

                    if (remoteUri != null && avc.version != null)
                    {
                        if (resourcesJson == null)
                        {
                            json["resources"] = resourcesJson = new JObject();
                        }
                        resourcesJson.SafeAdd("remote-avc", remoteUri.OriginalString);

                        try
                        {
                            if (BadHosts.Contains(remoteUri.Host))
                            {
                                Log.WarnFormat("AVC host does not contain version files: {0}",
                                               remoteUri.Host);
                            }
                            else if ((_github?.DownloadText(remoteUri)
                                      ?? _http.DownloadText(remoteUri))
                                     is string remoteJson
                                     && JsonConvert.DeserializeObject<AvcVersion>(remoteJson)
                                        is AvcVersion remoteAvc
                                     && avc.version.Equals(remoteAvc.version))
                            {
                                // Local AVC and Remote AVC describe the same version, prefer
                                Log.Info("Remote AVC version file describes same version as local AVC version file, using it preferentially.");
                                avc = remoteAvc;
                            }
                        }
                        catch (JsonReaderException e)
                        {
                            Log.WarnFormat("Error parsing remote version file {0}: {1}",
                                remoteUri, e.Message);
                            Log.Debug(e);
                        }
                        catch (RequestThrottledKraken)
                        {
                            // Treat rate limiting as a real error to avoid temporary metadata degradation
                            throw;
                        }
                        catch (Exception e)
                        {
                            Log.WarnFormat("Error fetching remote version file {0}: {1}", remoteUri, e.Message);
                            Log.Debug(e);
                        }
                    }

                    ApplyVersions(metadata, json, avc);

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

        public static void ApplyVersions(Metadata metadata, JObject json, AvcVersion avc)
        {
            GameVersion.SetJsonCompatibility(json, avc.ksp_version, avc.ksp_version_min, avc.ksp_version_max);

            if (avc.version != null)
            {
                // In practice, the version specified in .version files tends to be unreliable, with authors
                // forgetting to update it when new versions are released. Therefore if we have a version
                // specified from another source such as SpaceDock, curse or a GitHub tag, don't overwrite it
                // unless x_netkan_trust_version_file is true.
                if (metadata.TrustVersionFile)
                {
                    json.Remove("version");
                }
                json.SafeAdd("version", avc.version.ToString());
            }
        }

        private static Uri? GetRemoteAvcUri(AvcVersion avc)
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

            return Net.GetRawUri(remoteUri);
        }

        private static readonly HashSet<string> BadHosts = new HashSet<string>()
        {
            "forum.kerbalspaceprogram.com",
        };
    }
}
