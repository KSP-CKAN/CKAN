using System;
﻿using System.IO;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using CKAN.Types;
using System.Linq;

namespace CKAN
{

    /// <summary>
    /// CKAN client auto-updating routines. This works in conjunction with the
    /// auto-update helper to allow users to upgrade.
    /// </summary>
    public class AutoUpdate
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(AutoUpdate));

        /// <summary>
        /// The list of releases containing ckan.exe and AutoUpdater.exe
        /// </summary>
        private static readonly Uri latestCKANReleaseApiUrl = new Uri("https://api.github.com/repos/KSP-CKAN/CKAN/releases/latest");

        /// <summary>
        /// Old release list that just contains the auto updater,
        /// used as a fallback when missing from main release
        /// </summary>
        private static readonly Uri oldLatestUpdaterReleaseApiUrl = new Uri("https://api.github.com/repos/KSP-CKAN/CKAN-autoupdate/releases/latest");

        private Tuple<Uri, long> fetchedUpdaterUrl;
        private Tuple<Uri, long> fetchedCkanUrl;

        public Version LatestVersion { get; private set; }
        public string  ReleaseNotes  { get; private set; }

        public static readonly AutoUpdate Instance = new AutoUpdate();

        // This is private so we can enforce our class being a singleton.
        private AutoUpdate() { }

        private static bool CanWrite(string path)
        {
            try
            {
                // Try to open the file for writing.
                // We won't actually write, but we expect the OS to stop us if we don't have permissions.
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite)) { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // This is null when running tests, seemingly.
        private static readonly string exePath = Assembly.GetEntryAssembly()?.Location ?? "";

        /// <summary>
        /// Report whether it's possible to run the auto-updater.
        /// Checks whether we can overwrite the running ckan.exe.
        /// Windows doesn't let us check this because it locks the EXE
        /// for a running process, so assume we can always overwrite on Windows.
        /// </summary>
        public static readonly bool CanUpdate = Platform.IsWindows || CanWrite(exePath);

        /// <summary>
        /// Our metadata is considered fetched if we have a latest version, release notes,
        /// and download URLs for the ckan executable and helper.
        /// </summary>
        public bool IsFetched()
        {
            return LatestVersion != null && fetchedUpdaterUrl != null &&
                fetchedCkanUrl != null && ReleaseNotes != null;
        }

        /// <summary>
        /// Fetches all the latest release info, populating our attributes in
        /// the process.
        /// </summary>
        public void FetchLatestReleaseInfo()
        {
            var response = MakeRequest(latestCKANReleaseApiUrl);

            try
            {
                // Check whether the release includes the auto updater
                foreach (var asset in response.assets)
                {
                    string url = asset.browser_download_url.ToString();
                    if (url.EndsWith("ckan.exe"))
                    {
                        fetchedCkanUrl    = new Tuple<Uri, long>(new Uri(url), (long)asset.size);
                    }
                    else if (url.EndsWith("AutoUpdater.exe"))
                    {
                        fetchedUpdaterUrl = new Tuple<Uri, long>(new Uri(url), (long)asset.size);
                    }
                }
                if (fetchedUpdaterUrl == null)
                {
                    // Older releases don't include the auto updater
                    fetchedUpdaterUrl = RetrieveUrl(MakeRequest(oldLatestUpdaterReleaseApiUrl), 0);
                }
            }
            catch (Kraken)
            {
                LatestVersion = new Version(Meta.GetVersion());
                return;
            }

            ReleaseNotes  = ExtractReleaseNotes(response.body.ToString());
            LatestVersion = new CKANVersion(response.tag_name.ToString(), response.name.ToString());
        }

        /// <summary>
        /// Extracts release notes from the body of text provided by the github API.
        /// By default this is everything after the first three dashes on a line by
        /// itself, but as a fallback we'll use the whole body if not found.
        /// </summary>
        /// <returns>The release notes.</returns>
        public static string ExtractReleaseNotes(string releaseBody)
        {
            const string divider = "\r\n---\r\n";
            // Get at most two pieces, the first is the image, the second is the release notes
            string[] notesArray = releaseBody.Split(new string[] { divider }, 2, StringSplitOptions.None);
            return notesArray.Length > 1 ? notesArray[1] : notesArray[0];
        }

        /// <summary>
        /// Downloads the new ckan.exe version, as well as the updater helper,
        /// and then launches the helper allowing us to upgrade.
        /// </summary>
        /// <param name="launchCKANAfterUpdate">If set to <c>true</c> launch CKAN after update.</param>
        public void StartUpdateProcess(bool launchCKANAfterUpdate, IUser user = null)
        {
            if (!IsFetched())
            {
                throw new Kraken("We have not fetched the release info yet. Can't update.");
            }

            var pid = Process.GetCurrentProcess().Id;

            // download updater app and new ckan.exe
            string updaterFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            string ckanFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            Net.DownloadWithProgress(
                new[]
                {
                    new Net.DownloadTarget(fetchedUpdaterUrl.Item1, null, updaterFilename, fetchedUpdaterUrl.Item2),
                    new Net.DownloadTarget(fetchedCkanUrl.Item1,    null, ckanFilename,    fetchedCkanUrl.Item2),
                },
                user
            );

            // run updater
            SetExecutable(updaterFilename);
            Process.Start(new ProcessStartInfo
            {
                Verb      = "runas",
                FileName  = updaterFilename,
                Arguments = String.Format(@"{0} ""{1}"" ""{2}"" {3}", pid, exePath, ckanFilename, launchCKANAfterUpdate ? "launch" : "nolaunch"),
                UseShellExecute = false
            });

            // exit this ckan instance
            Environment.Exit(0);
        }

        /// <summary>
        /// Extracts the first downloadable asset (either the ckan.exe or its updater)
        /// from the provided github API response
        /// </summary>
        /// <returns>The URL to the downloadable asset.</returns>
        internal Tuple<Uri, long> RetrieveUrl(dynamic response, int whichOne)
        {
            if (response.assets.Count == 0)
            {
                throw new Kraken("The latest release isn't uploaded yet.");
            }
            else if (whichOne >= response.assets.Count)
            {
                throw new Kraken($"Asset index {whichOne} does not exist.");
            }
            var asset = response.assets[whichOne];
            string url = asset.browser_download_url.ToString();
            return new Tuple<Uri, long>(new Uri(url), (long)asset.size);
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
            web.Headers.Add("User-Agent", Net.UserAgentString);

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

        public static void SetExecutable(string fileName)
        {
            // mark as executable if on Linux or Mac
            if (Platform.IsUnix || Platform.IsMac)
            {
                // TODO: It would be really lovely (and safer!) to use the native system
                // call here: http://docs.go-mono.com/index.aspx?link=M:Mono.Unix.Native.Syscall.chmod

                string command = string.Format("+x \"{0}\"", fileName);

                ProcessStartInfo permsinfo = new ProcessStartInfo("chmod", command);
                permsinfo.UseShellExecute = false;
                Process permsprocess = Process.Start(permsinfo);
                permsprocess.WaitForExit();
            }
        }
    }
}
