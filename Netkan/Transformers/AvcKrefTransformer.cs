using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Avc;
using CKAN.NetKAN.Sources.Github;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from a KSP-AVC URL.
    /// </summary>
    internal sealed class AvcKrefTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AvcKrefTransformer));

        public string Name => "avc-kref";
        private readonly IHttpService httpSvc;
        private readonly IGithubApi   githubSrc;

        public AvcKrefTransformer(IHttpService http, IGithubApi github)
        {
            httpSvc   = http;
            githubSrc = github;
        }

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.Kref?.Source == "ksp-avc")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing KSP-AVC $kref transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var url = new Uri(metadata.Kref.Id);
                AvcVersion remoteAvc = JsonConvert.DeserializeObject<AvcVersion>(
                    githubSrc?.DownloadText(url)
                        ?? httpSvc.DownloadText(Net.GetRawUri(url))
                );

                json.SafeAdd("name",     remoteAvc.Name);
                json.Remove("$kref");
                json.SafeAdd("download", remoteAvc.Download);

                // Set .resources.repository based on GITHUB properties
                if (remoteAvc.Github?.Username != null && remoteAvc.Github?.Repository != null)
                {
                    // Make sure resources exist.
                    if (json["resources"] == null)
                    {
                        json["resources"] = new JObject();
                    }

                    var resourcesJson = (JObject)json["resources"];
                    resourcesJson.SafeAdd("repository", $"https://github.com/{remoteAvc.Github.Username}/{remoteAvc.Github.Repository}");
                }

                // Use standard KSP-AVC logic to set version and the ksp_version_* properties
                AvcTransformer.ApplyVersions(json, remoteAvc);

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                yield return new Metadata(json);
            }
            else
            {
                yield return metadata;
            }
        }
    }
}
