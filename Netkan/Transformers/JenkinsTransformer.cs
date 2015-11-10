using System;
using System.Collections.Generic;
using System.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from a Jenkins build server.
    /// </summary>
    internal sealed class JenkinsTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(JenkinsTransformer));

        private static readonly Dictionary<string, string> BuildTypeToProperty = new Dictionary<string, string>
        {
            { "any", "lastBuild" },
            { "compeleted", "lastCompletedBuild" },
            { "failed", "lastFailedBuild" },
            { "stable", "lastStableBuild" },
            { "successful", "lastSuccessfulBuild" },
            { "unstable", "lastUnstableBuild" },
            { "unsuccessful", "lastUnsuccessfulBuild" }
        };

        private readonly IHttpService _http;

        public JenkinsTransformer(IHttpService http)
        {
            _http = http;
        }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "jenkins")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Jenkins transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                var buildType = "stable";
                var useFilenameVersion = true;

                var jenkinsMetadata = (JObject)json["x_netkan_jenkins"];
                if (jenkinsMetadata != null)
                {
                    var jenkinsBuildMetadata = (string)jenkinsMetadata["build"];

                    if (jenkinsBuildMetadata != null)
                    {
                        buildType = jenkinsBuildMetadata;
                    }

                    var jenkinsUseFilenameVersionMetadata = (bool?)jenkinsMetadata["use_filename_version"];

                    if (jenkinsUseFilenameVersionMetadata != null)
                    {
                        useFilenameVersion = jenkinsUseFilenameVersionMetadata.Value;
                    }
                }

                Log.InfoFormat("Attempting to retrieve the last {0} build", buildType);

                // Get the job metadata
                var job = JsonConvert.DeserializeObject<JObject>(
                    _http.DownloadText(new Uri(metadata.Kref.Id + "api/json"))
                );

                // Get the build reference metadata
                var buildRef = (JObject)job[BuildTypeToProperty[buildType]];

                // Get the build number and url
                var buildNumber = (int)buildRef["number"];
                var buildUrl = (string)buildRef["url"];

                Log.InfoFormat("The last {0} build is #{1}", buildType, buildNumber);

                // Get the build metadata
                var build = JsonConvert.DeserializeObject<JObject>(
                    _http.DownloadText(new Uri(buildUrl + "api/json"))
                );

                // Get the artifact metadata
                // TODO: Support asset_matching
                var artifact = ((JArray)build["artifacts"])
                    .Select(i => (JObject)i)
                    .Single(i => ((string)i["fileName"]).ToLowerInvariant().EndsWith(".zip"));

                var artifactFileName = artifact["fileName"];
                var artifactRelativePath = artifact["relativePath"];

                // I'm not sure if 'relativePath' is the right property to use here
                var download = Uri.EscapeUriString(buildUrl + "artifact/" + artifactRelativePath);
                var version = artifactFileName;

                Log.DebugFormat("Using download URL: {0}", download);
                json.SafeAdd("download", download);

                if (useFilenameVersion)
                {
                    Log.DebugFormat("Using filename as version: {0}", version);
                    json.SafeAdd("version", version);
                }

                Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                return new Metadata(json);
            }

            return metadata;
        }
    }
}
