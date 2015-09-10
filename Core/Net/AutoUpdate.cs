using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using CKAN.Types;


namespace CKAN
{

    public class AutoUpdate
    {

        private readonly ILog log = LogManager.GetLogger(typeof(AutoUpdate));

        private readonly Uri latestCKANReleaseApiUrl = new Uri("https://api.github.com/repos/KSP-CKAN/CKAN/releases/latest");

        private readonly Uri latestUpdaterReleaseApiUrl = new Uri(
            "https://api.github.com/repos/KSP-CKAN/CKAN-autoupdate/releases/latest");

        private Uri fetchedUpdaterUrl;
        private Uri fetchedCkanUrl;

        public Version LatestVersion { get; private set; }
        public string ReleaseNotes { get; private set; }

        private static AutoUpdate instance;

        public static AutoUpdate Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AutoUpdate();
                }
                return instance;
            }
            private set { }
        }

        private AutoUpdate() { }

        public static void ClearCache()
        {
            instance = new AutoUpdate();
        }

        public bool IsFetched()
        {
            return LatestVersion != null && fetchedUpdaterUrl != null &&
                fetchedCkanUrl != null && ReleaseNotes != null;
        }

        public void FetchLatestReleaseInfo()
        {
            var response = MakeRequest(latestCKANReleaseApiUrl);

            try
            {
                fetchedUpdaterUrl = RetrieveUrl(MakeRequest(latestUpdaterReleaseApiUrl));
                fetchedCkanUrl = RetrieveUrl(response);
            }
            catch (Kraken)
            {
                LatestVersion = new Version(Meta.Version());
                return;
            }

            string body = response.body.ToString();
            ReleaseNotes = body.Split(new string[] { "\r\n---\r\n" }, StringSplitOptions.None)[1];
            LatestVersion = new CKANVersion(response.tag_name.ToString(), response.name.ToString());
        }

        /// <summary>
        /// Downloads the new ckan.exe version, as well as the updater helper,
        /// and then launches the helper allowing us to upgrade.
        /// </summary>
        /// <param name="launchCKANAfterUpdate">If set to <c>true</c> launch CKAN after update.</param>
        public void StartUpdateProcess(bool launchCKANAfterUpdate)
        {
            if (!IsFetched())
            {
                throw new Kraken("We have not fetched the release info yet. Can't update.");
            }

            var pid = Process.GetCurrentProcess().Id;

            // download updater app
            string updaterFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";

            var web = new WebClient();
            web.Headers.Add("user-agent", Net.UserAgentString);
            web.DownloadFile(fetchedUpdaterUrl, updaterFilename);

            // download new ckan.exe
            string ckanFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            web.DownloadFile(fetchedCkanUrl, ckanFilename);

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

        /// <summary>
        /// Extracts the first downloadable asset (either the ckan.exe or its updater)
        /// from the provided github API response
        /// </summary>
        /// <returns>The URL to the downloadable asset.</returns>
        internal Uri RetrieveUrl(dynamic response)
        {
            if (response.assets.Count == 0)
            {
                throw new Kraken("The latest release isn't uploaded yet.");
            }
            var assets = response.assets[0];
            return new Uri(assets.browser_download_url.ToString());
        }

        /// <summary>
        /// Fetches the URL provided, and de-serialises the returned JSON
        /// data structure into a dynamic object.
        /// 
        /// May throw an exception (especially a WebExeption) on failure.
        /// </summary>
        /// <returns>A dynamic object representing the JSON we fetched.</returns>
        internal dynamic MakeRequest(Uri url)
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
