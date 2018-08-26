using System;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Avc;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from a KSP-AVC URL.
    /// </summary>
    internal sealed class AvcKrefTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AvcKrefTransformer));

        public string Name { get { return "avc-kref"; } }

        public AvcKrefTransformer() { }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Kref?.Source == "ksp-avc")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing KSP-AVC $kref transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                AvcVersion remoteAvc = JsonConvert.DeserializeObject<AvcVersion>(
                    Net.DownloadText(new Uri(metadata.Kref.Id))
                );

                json.SafeAdd("name",     remoteAvc.Name);
                json.SafeAdd("download", remoteAvc.Download);

                // Set .resources.repository based on GITHUB properties
                if (remoteAvc.Github?.Username != null && remoteAvc.Github?.Repository != null)
                {
                    // Make sure resources exist.
                    if (json["resources"] == null)
                        json["resources"] = new JObject();
                    var resourcesJson = (JObject)json["resources"];
                    resourcesJson.SafeAdd("repository", $"https://github.com/{remoteAvc.Github.Username}/{remoteAvc.Github.Repository}");
                }

                // Use standard KSP-AVC logic to set version and the ksp_version_* properties
                AvcTransformer.ApplyVersions(json, remoteAvc);

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
