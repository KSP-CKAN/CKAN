using System;
using System.IO;
using System.Linq;

using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// A popup to let the user import manually downloaded zip files into the mod cache.
    /// </summary>
    public static class InstallFromCkanDialog {

        /// <summary>
        /// Let the user choose some zip files, then import them to the mod cache.
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="gameInst">Game instance to import into</param>
        public static CkanModule[] ChooseCkanFiles(ConsoleTheme theme,
                                                   GameInstance gameInst)
        {
            var cfmsd = new ConsoleFileMultiSelectDialog(
                Properties.Resources.CkanFileSelectTitle,
                FindDownloadsPath(gameInst),
                "*.ckan",
                Properties.Resources.CkanFileSelectHeader,
                Properties.Resources.CkanFileSelectHeader);
            return cfmsd.Run(theme)
                        .Select(f => CkanModule.FromFile(f.FullName))
                        .ToArray();
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
