using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace CKAN
{

    public class AutoUpdate
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));

        private static readonly Uri latestCKANReleaseApiUrl = new Uri("https://api.github.com/repos/KSP-CKAN/CKAN/releases/latest");

        private static readonly Uri latestUpdaterReleaseApiUrl = new Uri(
            "https://api.github.com/repos/KSP-CKAN/CKAN-autoupdate/releases/latest");

        public static Version FetchLatestCkanVersion()
        {
            var response = MakeRequest(latestCKANReleaseApiUrl);
            return new Version(response.tag_name.ToString());
        }

        public static void StartUpdateProcess()
        {
            var pid = Process.GetCurrentProcess().Id;
            
            // download updater app
            string updaterFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";

            var web = new WebClient();
            web.Headers.Add("user-agent", Net.UserAgentString);
            web.DownloadFile(FetchUpdaterUrl(), updaterFilename);

            // download new ckan.exe
            string ckanFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            web.DownloadFile(FetchCkanUrl(), ckanFilename);

            // run updater
            var args = String.Format("{0} \"{1}\" \"{2}\"", pid, Assembly.GetExecutingAssembly().Location, ckanFilename);
            Process.Start(updaterFilename, args);

            // exit this ckan instance
            Environment.Exit(0);
        }

        private static Uri FetchUpdaterUrl()
        {
            var response = MakeRequest(latestUpdaterReleaseApiUrl);
            var assets = response.assets[0];
            return new Uri(assets.browser_download_url.ToString());
        }

        private static Uri FetchCkanUrl()
        {
            var response = MakeRequest(latestCKANReleaseApiUrl);
            var assets = response.assets[0];
            return new Uri(assets.browser_download_url.ToString());
        }

        private static dynamic MakeRequest(Uri url)
        {
            var web = new WebClient();
            web.Headers.Add("user-agent", Net.UserAgentString);

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
