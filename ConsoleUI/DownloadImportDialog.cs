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
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="gameInst">Game instance to import into</param>
        /// <param name="cache">Cache object to import into</param>
        /// <param name="cp">Change plan object for marking things to be installed</param>
        public static void ImportDownloads(ConsoleTheme theme, GameInstance gameInst, NetModuleCache cache, ChangePlan cp)
        {
            ConsoleFileMultiSelectDialog cfmsd = new ConsoleFileMultiSelectDialog(
                "Import Downloads",
                FindDownloadsPath(gameInst),
                "*.zip",
                "Import"
            );
            HashSet<FileInfo> files = cfmsd.Run(theme);

            if (files.Count > 0) {
                ProgressScreen  ps   = new ProgressScreen("Importing Downloads", "Calculating...");
                ModuleInstaller inst = new ModuleInstaller(gameInst, cache, ps);
                ps.Run(theme, (ConsoleTheme th) => inst.ImportFiles(files, ps,
                    (CkanModule mod) => cp.Install.Add(mod), RegistryManager.Instance(gameInst).registry));
                // Don't let the installer re-use old screen references
                inst.User = null;
            }
        }

        private static readonly string[] downloadPaths = new string[] {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        private static string FindDownloadsPath(GameInstance gameInst)
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
