using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Sources.Jenkins
{
    internal class JenkinsApi : IJenkinsApi
    {
        public JenkinsApi(IHttpService http)
        {
            _http = http;
        }

        public JenkinsBuild? GetLatestBuild(JenkinsRef reference, JenkinsOptions? options)
        {
            options ??= new JenkinsOptions();
            string url = Regex.Replace(reference.BaseUri?.ToString() ?? "", @"/$", "");
            return Call<JenkinsBuild>(
                $"{url}/{BuildTypeToProperty[options.BuildType]}/api/json");
        }

        public IEnumerable<JenkinsBuild> GetAllBuilds(JenkinsRef reference, JenkinsOptions? options)
        {
            options ??= new JenkinsOptions();
            string url = Regex.Replace(reference.BaseUri?.ToString() ?? "", @"/$", "");
            var job = Call<JObject>($"{url}/api/json");
            var builds = (JArray?)job?["builds"];
            BuildTypeToResult.TryGetValue(options.BuildType, out string? resultVal);
            foreach (JObject buildEntry in builds?.OfType<JObject>() ?? Enumerable.Empty<JObject>())
            {
                Log.Info($"Processing {buildEntry["url"]}");
                var build = Call<JenkinsBuild>($"{buildEntry["url"]}api/json");
                // Make sure build status matches options.BuildType
                if (build != null && (resultVal == null || build.Result == resultVal))
                {
                    yield return build;
                }
            }
        }

        private T? Call<T>(string url)
            => Call<T>(new Uri(url));

        private T? Call<T>(Uri url)
            => Call(url) is string s
                ? JsonConvert.DeserializeObject<T>(s)
                : default;

        private string? Call(Uri url)
            => _http.DownloadText(url);

        private static readonly Dictionary<JenkinsBuildType, string> BuildTypeToProperty = new Dictionary<JenkinsBuildType, string>()
        {
            { JenkinsBuildType.any,          "lastBuild"             },
            { JenkinsBuildType.completed,    "lastCompletedBuild"    },
            { JenkinsBuildType.failed,       "lastFailedBuild"       },
            { JenkinsBuildType.stable,       "lastStableBuild"       },
            { JenkinsBuildType.successful,   "lastSuccessfulBuild"   },
            { JenkinsBuildType.unstable,     "lastUnstableBuild"     },
            { JenkinsBuildType.unsuccessful, "lastUnsuccessfulBuild" }
        };

        private static readonly Dictionary<JenkinsBuildType, string> BuildTypeToResult = new Dictionary<JenkinsBuildType, string>()
        {
            // "any" not listed so it will match everything
            { JenkinsBuildType.completed,    "SUCCESS" },
            { JenkinsBuildType.stable,       "SUCCESS" },
            { JenkinsBuildType.successful,   "SUCCESS" },
            { JenkinsBuildType.failed,       "FAILURE" },
            { JenkinsBuildType.unstable,     "FAILURE" },
            { JenkinsBuildType.unsuccessful, "FAILURE" }
        };

        private readonly IHttpService _http;
        private static readonly ILog Log = LogManager.GetLogger(typeof(JenkinsApi));
    }
}
