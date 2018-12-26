using System;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Extensions;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Jenkins;

namespace CKAN.NetKAN.Transformers
{
    /// <summary>
    /// An <see cref="ITransformer"/> that looks up data from a Jenkins build server.
    /// </summary>
    internal sealed class JenkinsTransformer : ITransformer
    {
        public JenkinsTransformer(IJenkinsApi api)
        {
            _api = api;
        }

        public string Name { get { return "jenkins"; } }

        public Metadata Transform(Metadata metadata)
        {
            if (metadata.Kref != null && metadata.Kref.Source == "jenkins")
            {
                var json = metadata.Json();

                Log.InfoFormat("Executing Jenkins transformation with {0}", metadata.Kref);
                Log.DebugFormat("Input metadata:{0}{1}", Environment.NewLine, json);

                JenkinsOptions options = json["x_netkan_jenkins"]?.ToObject<JenkinsOptions>()
                    ?? new JenkinsOptions();

                JenkinsBuild build = _api.GetLatestBuild(
                    new JenkinsRef(metadata.Kref),
                    options
                );

                JenkinsArtifact[] artifacts = build.Artifacts
                    .Where(a => options.AssetMatchPattern.IsMatch(a.FileName))
                    .ToArray();

                switch (artifacts.Length)
                {
                    case 1:
                        JenkinsArtifact artifact = artifacts.Single();

                        string download = Uri.EscapeUriString(
                            $"{build.Url}artifact/{artifact.RelativePath}"
                        );
                        Log.DebugFormat("Using download URL: {0}", download);
                        json.SafeAdd("download", download);

                        if (options.UseFilenameVersion)
                        {
                            Log.DebugFormat("Using filename as version: {0}", artifact.FileName);
                            json.SafeAdd("version", artifact.FileName);
                        }

                        // Make sure resources exist.
                        if (json["resources"] == null)
                        {
                            json["resources"] = new JObject();
                        }

                        var resourcesJson = (JObject)json["resources"];
                        resourcesJson.SafeAdd("ci", Uri.EscapeUriString(metadata.Kref.Id));

                        Log.DebugFormat("Transformed metadata:{0}{1}", Environment.NewLine, json);

                        return new Metadata(json);
                        break;

                    case 0:
                        throw new Exception("Could not find any matching artifacts");

                    default:
                        throw new Exception("Found too many matching artifacts");
                }
            }
            return metadata;
        }

        private readonly IJenkinsApi _api;
        private static readonly ILog Log = LogManager.GetLogger(typeof(JenkinsTransformer));
    }
}
