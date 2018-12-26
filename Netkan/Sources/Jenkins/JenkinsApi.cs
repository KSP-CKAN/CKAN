using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using CKAN.NetKAN.Services;

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

        private T Call<T>(string url)
        {
            return Call<T>(new Uri(url));
        }

        private T Call<T>(Uri url)
        {
            return JsonConvert.DeserializeObject<T>(Call(url));
        }

        private string Call(string url)
        {
            return Call(new Uri(url));
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

        private IHttpService _http;
        private static readonly ILog Log = LogManager.GetLogger(typeof(JenkinsApi));
    }
}
