using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace CKAN
{

    class AutoUpdate
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));

        private static readonly Uri latestCKANReleaseApiUrl = new Uri("https://api.github.com/repos/KSP-CKAN/CKAN/releases/latest");

        private static readonly Uri latestUpdaterReleaseApiUrl = new Uri(
            "https://api.github.com/repos/KSP-CKAN/CKAN-autoupdate/releases/latest");

        private static readonly WebClient web = new WebClient();

        public static Version FetchLatestCkanVersion()
        {
            return new Version(MakeRequest(latestCKANReleaseApiUrl).tag_name);
        }

        public static void StartUpdateProcess(string new_ckan_exe)
        {
            var pid = Process.GetCurrentProcess().Id;
            
            // download updater app
            string updaterFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            web.DownloadFile(FetchUpdaterUrl(), updaterFilename);

            // run updater
            Process.Start(updaterFilename, String.Format("{0} \"{1}\" \"{2}\"", pid, Assembly.GetExecutingAssembly().Location, new_ckan_exe));

            // exit this ckan instance
            System.Environment.Exit(0);
        }

        private static Uri FetchUpdaterUrl()
        {
            return new Uri(MakeRequest(latestUpdaterReleaseApiUrl).assets[0].browser_download_url);
        }

        private static dynamic MakeRequest(Uri url)
        {
            string result = "";
            try
            {
                result = web.DownloadString(latestCKANReleaseApiUrl);
            }
            catch (WebException webEx)
            {
                log.ErrorFormat("WebException while accessing {0}: {1}", url, webEx);
                throw;
            }

            return JsonConvert.DeserializeObject(result);
        }


    }

}
