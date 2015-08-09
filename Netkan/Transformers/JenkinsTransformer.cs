using System;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Jenkins;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from a Jenkins build server.
    /// </summary>
    internal sealed class JenkinsTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JenkinsTransformer));

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "jenkins")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Jenkins transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var versionBase = (string)json["x_ci_version_base"];
                var resources = (JObject)json["resources"];
                var baseUri = (string)resources["ci"] ?? (string)resources["x_ci"];

                var build = JenkinsAPI.GetLatestBuild(baseUri, versionBase);

                Log.DebugFormat("Found Jenkins Mod: {0} {1}", metadata.Kref.Id, build.version);

                json.SafeAdd("version", build.version.ToString());
                json.SafeAdd("download", Uri.EscapeUriString(build.download.ToString()));

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
