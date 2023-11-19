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

        public JenkinsBuild GetLatestBuild(JenkinsRef reference, JenkinsOptions options)
        {
            if (options == null)
            {
                options = new JenkinsOptions();
            }
            string url = Regex.Replace(reference.BaseUri.ToString(), @"/$", "");
            return Call<JenkinsBuild>(
                $"{url}/{BuildTypeToProperty[options.BuildType]}/api/json"
            );
        }

        public IEnumerable<JenkinsBuild> GetAllBuilds(JenkinsRef reference, JenkinsOptions options)
        {
            if (options == null)
            {
                options = new JenkinsOptions();
            }
            string url = Regex.Replace(reference.BaseUri.ToString(), @"/$", "");
            JObject job = Call<JObject>($"{url}/api/json");
            JArray builds = (JArray)job["builds"];
            BuildTypeToResult.TryGetValue(options.BuildType, out string resultVal);
            foreach (JObject buildEntry in builds.Cast<JObject>())
            {
                Log.Info($"Processing {buildEntry["url"]}");
                JenkinsBuild build = Call<JenkinsBuild>($"{buildEntry["url"]}api/json");
                // Make sure build status matches options.BuildType
                if (resultVal == null || build.Result == resultVal)
                {
                    yield return build;
                }
            }
        }

        private T Call<T>(string url)
        {
            return Call<T>(new Uri(url));
        }

        private T Call<T>(Uri url)
        {
            return JsonConvert.DeserializeObject<T>(Call(url));
        }

        private string Call(Uri url)
        {
            return _http.DownloadText(url);
        }

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
