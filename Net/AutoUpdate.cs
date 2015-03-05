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

        public static string FetchLatestCkanVersionReleaseNotes()
        {
            var response = MakeRequest(latestCKANReleaseApiUrl);
            string body = response.body.ToString();
            return body.Split(new string[] {"\r\n---\r\n"}, StringSplitOptions.None)[1];
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

            var path = Assembly.GetEntryAssembly().Location;

            // run updater
            var args = String.Format("{0} \"{1}\" \"{2}\"", pid, path, ckanFilename);

            Process.Start(new ProcessStartInfo(updaterFilename, args) {UseShellExecute = false});

            // exit this ckan instance
            Environment.Exit(0);
        }

        public static bool IsLinux
        {
            get
            {
                // Magic numbers ahoy! This arcane incantation was found
                // in a Unity help-page, which was found on a scroll,
                // which was found in an urn that dated back to Mono 2.0.
                // It documents singular numbers of great power.
                //
                // "And lo! 'pon the 4, 6, and 128 the penguin shall
                // come, and it infiltrate dominate from the smallest phone to
                // the largest cloud."
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
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
                result = web.DownloadString(url);
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
