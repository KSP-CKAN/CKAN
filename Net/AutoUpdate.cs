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
            
            // Download updater app (See CKAN-autoupdate repo).
            string updater_filename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";

            var web = new WebClient();
            web.Headers.Add("user-agent", Net.UserAgentString);
            web.DownloadFile(FetchUpdaterUrl(), updater_filename);

            // Download new ckan.exe.
            string ckan_filename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            web.DownloadFile(FetchCkanUrl(), ckan_filename);

            var path = Assembly.GetEntryAssembly().Location;

            // Mark as executable if on Linux or Mac.
            if (Platform.IsUnix || Platform.IsMac)
            {
                // TODO: It would be really lovely (and safer!) to use the native system
                // call here: http://docs.go-mono.com/index.aspx?link=M:Mono.Unix.Native.Syscall.chmod

                string command_updater = string.Format("+x \"{0}\"", updater_filename);
                string command_ckan = string.Format("+x \"{0}\"", ckan_filename);

                ProcessStartInfo permsinfo_updater = new ProcessStartInfo("chmod", command_updater);
                permsinfo_updater.UseShellExecute = false;
                Process permsprocess_updater = Process.Start(permsinfo_updater);
                permsprocess_updater.WaitForExit();

                ProcessStartInfo permsinfo_ckan = new ProcessStartInfo("chmod", command_ckan);
                permsinfo_ckan.UseShellExecute = false;
                Process permsprocess_ckan = Process.Start(permsinfo_ckan);
                permsprocess_ckan.WaitForExit();

                // Make sure we have the right exit status from both processes.
                if (permsprocess_ckan.ExitCode != 0 || permsprocess_updater.ExitCode != 0)
                {
                    throw new Kraken("Could not set permissions properly.");
                }
            }

            // Run updater.
            var args = String.Format("{0} \"{1}\" \"{2}\" {3}", pid, path, ckan_filename, launchCKANAfterUpdate ? "launch" : "nolaunch");

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = updater_filename;
            processInfo.Arguments = args;
            processInfo.UseShellExecute = false;
            Process.Start(processInfo);

            // Exit this CKAN instance.
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
