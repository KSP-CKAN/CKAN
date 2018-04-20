using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Object representing a CKAN release
    /// </summary>
    public class CkanUpdate
    {
        /// <summary>
        /// Initialize the Object
        /// </summary>
        /// <param name="json">JSON representation of release</param>
        public CkanUpdate(string json)
        {
            dynamic response = JsonConvert.DeserializeObject(json);

            Version = new CkanModuleVersion(
                response.tag_name.ToString(),
                response.name.ToString()
            );
            ReleaseNotes = ExtractReleaseNotes(response.body.ToString());

            foreach (var asset in response.assets)
            {
                string url = asset.browser_download_url.ToString();
                if (url.EndsWith("ckan.exe"))
                {
                    ReleaseDownload = asset.browser_download_url;
                    ReleaseSize     = (long)asset.size;
                }
                else if (url.EndsWith("AutoUpdater.exe"))
                {
                    UpdaterDownload = asset.browser_download_url;
                    UpdaterSize     = (long)asset.size;
                }
            }
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

        public readonly CkanModuleVersion Version;
        public readonly Uri    ReleaseDownload;
        public readonly long   ReleaseSize;
        public readonly Uri    UpdaterDownload;
        public readonly long   UpdaterSize;
        public readonly string ReleaseNotes;
    }

    /// <summary>
    /// CKAN client auto-updating routines. This works in conjunction with the
    /// auto-update helper to allow users to upgrade.
    /// </summary>
    public class AutoUpdate
    {
        /// <summary>
        /// The list of releases containing ckan.exe and AutoUpdater.exe
        /// </summary>
        private static readonly Uri latestCKANReleaseApiUrl = new Uri("https://api.github.com/repos/KSP-CKAN/CKAN/releases/latest");

        public static readonly AutoUpdate Instance = new AutoUpdate();

        public CkanUpdate latestUpdate;

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
            return latestUpdate != null;
        }

        /// <summary>
        /// Fetches all the latest release info, populating our attributes in
        /// the process.
        /// </summary>
        public void FetchLatestReleaseInfo()
        {
            latestUpdate = new CkanUpdate(Net.DownloadText(latestCKANReleaseApiUrl));
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
            string ckanFilename    = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".exe";
            Net.DownloadWithProgress(
                new[]
                {
                    new Net.DownloadTarget(
                        latestUpdate.UpdaterDownload,
                        null,
                        updaterFilename,
                        latestUpdate.UpdaterSize),
                    new Net.DownloadTarget(
                        latestUpdate.ReleaseDownload,
                        null,
                        ckanFilename,
                        latestUpdate.ReleaseSize),
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

        private static readonly ILog log = LogManager.GetLogger(typeof(AutoUpdate));
    }
}
