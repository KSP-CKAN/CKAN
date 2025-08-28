using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace CKAN
{
    /// <summary>
    /// CKAN client auto-updating routines. This works in conjunction with the
    /// auto-update helper to allow users to upgrade.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AutoUpdate
    {
        public AutoUpdate()
        {
        }

        public CkanUpdate GetUpdate(bool devBuild, string? userAgent = null)
        {
            if (updates.TryGetValue(devBuild, out CkanUpdate? update))
            {
                return update;
            }
            var newUpdate = devBuild
                ? new S3BuildCkanUpdate(null, userAgent) as CkanUpdate
                : new GitHubReleaseCkanUpdate(null, userAgent);
            updates.Add(devBuild, newUpdate);
            return newUpdate;
        }

        private readonly Dictionary<bool, CkanUpdate> updates = new Dictionary<bool, CkanUpdate>();

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
        /// Downloads the new ckan.exe version, as well as the updater helper,
        /// and then launches the helper allowing us to upgrade.
        /// </summary>
        /// <param name="launchCKANAfterUpdate">If set to <c>true</c> launch CKAN after update.</param>
        /// <param name="userAgent">The user agent to use for the request.</param>
        /// <param name="devBuild">If set to <c>true</c> use the dev build.</param>
        /// <param name="user">The user to use for the request.</param>
        public void StartUpdateProcess(bool launchCKANAfterUpdate, string? userAgent, bool devBuild, IUser? user = null)
        {
            var pid = Process.GetCurrentProcess().Id;

            var update = GetUpdate(devBuild, userAgent);

            // download updater app and new ckan.exe
            NetAsyncDownloader.DownloadWithProgress(update.Targets, userAgent, user);

            // run updater
            SetExecutable(update.updaterFilename);
            Process.Start(new ProcessStartInfo
            {
                Verb      = "runas",
                FileName  = update.updaterFilename,
                Arguments = string.Format(@"{0} ""{1}"" ""{2}"" {3}",
                                          -pid, exePath,
                                          update.ckanFilename,
                                          launchCKANAfterUpdate ? "launch" : "nolaunch"),
                // .NET ignores Verb without this
                UseShellExecute = true,
                CreateNoWindow = true,
            });

            // Caller should now exit. Let them do it safely.
        }

        public static void SetExecutable(string fileName)
        {
            // mark as executable if on Linux or Mac
            if (Platform.IsUnix || Platform.IsMac)
            {
                // TODO: It would be really lovely (and safer!) to use the native system
                // call here: http://docs.go-mono.com/index.aspx?link=M:Mono.Unix.Native.Syscall.chmod

                string command = string.Format("+x \"{0}\"", fileName);

                ProcessStartInfo permsinfo = new ProcessStartInfo("chmod", command)
                {
                    UseShellExecute = false
                };
                var permsprocess = Process.Start(permsinfo);
                permsprocess?.WaitForExit();
            }
        }

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
    }
}
