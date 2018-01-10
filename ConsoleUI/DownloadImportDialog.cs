using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// A popup to let the user import manually downloaded zip files into the mod cache.
    /// </summary>
    public static class DownloadImportDialog {

        /// <summary>
        /// Let the user choose some zip files, then import them to the mod cache.
        /// </summary>
        /// <param name="gameInst">Game instance to import into</param>
        /// <param name="cp">Change plan object for marking things to be installed</param>
        public static void ImportDownloads(KSP gameInst, ChangePlan cp)
        {
            ConsoleFileMultiSelectDialog cfmsd = new ConsoleFileMultiSelectDialog(
                "Import Downloads",
                FindDownloadsPath(gameInst),
                "*.zip",
                "Import"
            );
            HashSet<FileInfo> files = cfmsd.Run();

            if (files.Count > 0) {
                ProgressScreen  ps   = new ProgressScreen("Importing Downloads", "Calculating...");
                ModuleInstaller inst = ModuleInstaller.GetInstance(gameInst, ps);
                ps.Run(() => inst.ImportFiles(files, ps,
                    (string identifier) => cp.Install.Add(identifier)));
                // Don't let the installer re-use old screen references
                inst.User = null;
            }
        }

        private static readonly string[] downloadPaths = new string[] {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        private static string FindDownloadsPath(KSP gameInst)
        {
            foreach (string p in downloadPaths) {
                if (!string.IsNullOrEmpty(p) && Directory.Exists(p)) {
                    return p;
                }
            }
            return gameInst.GameDir();
        }

    }

}
