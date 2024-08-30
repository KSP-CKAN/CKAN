using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Services;
using System.Linq;

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

        private static readonly Dictionary<string, string> BuildTypeToProperty = new Dictionary<string, string>()
        {
            { "any",          "lastBuild"             },
            { "completed",    "lastCompletedBuild"    },
            { "failed",       "lastFailedBuild"       },
            { "stable",       "lastStableBuild"       },
            { "successful",   "lastSuccessfulBuild"   },
            { "unstable",     "lastUnstableBuild"     },
            { "unsuccessful", "lastUnsuccessfulBuild" }
        };

        private static readonly Dictionary<string, string> BuildTypeToResult = new Dictionary<string, string>()
        {
            // "any" not listed so it will match everything
            { "completed",    "SUCCESS" },
            { "stable",       "SUCCESS" },
            { "successful",   "SUCCESS" },
            { "failed",       "FAILURE" },
            { "unstable",     "FAILURE" },
            { "unsuccessful", "FAILURE" }
        };

        private readonly IHttpService _http;
        private static readonly ILog Log = LogManager.GetLogger(typeof(JenkinsApi));
    }
}
