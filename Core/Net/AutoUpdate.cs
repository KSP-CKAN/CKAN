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

        private static readonly ILog log = LogManager.GetLogger(typeof(AutoUpdate));

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

        public static void StartUpdateProcess(bool launchCKANAfterUpdate)
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

            // mark as executable if on Linux or Mac
            if (Platform.IsUnix || Platform.IsMac)
            {
                // TODO: It would be really lovely (and safer!) to use the native system
                // call here: http://docs.go-mono.com/index.aspx?link=M:Mono.Unix.Native.Syscall.chmod

                string command = string.Format("+x \"{0}\"", updaterFilename);

                ProcessStartInfo permsinfo = new ProcessStartInfo("chmod", command);
                permsinfo.UseShellExecute = false;
                Process permsprocess = Process.Start(permsinfo);
                permsprocess.WaitForExit();
            }

            var args = String.Format("{0} \"{1}\" \"{2}\" {3}", pid, path, ckanFilename, launchCKANAfterUpdate ? "launch" : "nolaunch");

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = updaterFilename;
            processInfo.Arguments = args;
            processInfo.UseShellExecute = false;
            Process.Start(processInfo);

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

            try
            {
                var result = web.DownloadString(url);
                return JsonConvert.DeserializeObject(result);
            }
            catch (WebException webEx)
            {
                log.ErrorFormat("WebException while accessing {0}: {1}", url, webEx);
                throw;
            }
        }

    }

}
