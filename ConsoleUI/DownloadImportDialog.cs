using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
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
                ps.Run(() => ImportFiles(gameInst, files, ps, inst,
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

        /// <summary>
        /// Import a list of files into the download cache, with progress bar and
        /// interactive prompts for installation and deletion.
        /// </summary>
        /// <param name="gameInst">Game instance to install into</param>
        /// <param name="files">Set of files to import</param>
        /// <param name="user">Object for user interaction</param>
        /// <param name="inst">Module installer object</param>
        /// <param name="installMod">Function to call to mark a mod for installation</param>
        public static void ImportFiles(KSP gameInst, HashSet<FileInfo> files,
                IUser user, ModuleInstaller inst, Action<string> installMod)
        {
            Registry         registry    = RegistryManager.Instance(gameInst).registry;
            HashSet<string>  installable = new HashSet<string>();
            List<FileInfo>   deletable   = new List<FileInfo>();
            // Get the mapping of known hashes to modules
            Dictionary<string, List<CkanModule>> index = registry.GetSha1Index();
            int i = 0;
            foreach (FileInfo f in files) {
                int percent = i * 100 / files.Count;
                user.RaiseProgress($"Importing {f.Name}... ({percent}%)", percent);
                // Calc SHA-1 sum
                string sha1 = GetFileHashSha1(f.FullName);
                // Find SHA-1 sum in registry (potentially multiple)
                if (index.ContainsKey(sha1)) {
                    deletable.Add(f);
                    List<CkanModule> matches = index[sha1];
                    foreach (CkanModule mod in matches) {
                        if (mod.IsCompatibleKSP(gameInst.VersionCriteria())) {
                            installable.Add(mod.identifier);
                        }
                        if (inst.Cache.IsCachedZip(mod.download)) {
                            user.RaiseMessage("Already cached: {0}", f.Name);
                        } else {
                            user.RaiseMessage($"Importing {mod.identifier} {Formatting.StripEpoch(mod.version)}...");
                            inst.Cache.Store(mod.download, f.FullName);
                        }
                    }
                } else {
                    user.RaiseMessage("Not found in index: {0}", f.Name);
                }
                ++i;
            }
            if (installable.Count > 0 && user.RaiseYesNoDialog($"Install {installable.Count} compatible imported mods?")) {
                // Install the imported mods
                foreach (string identifier in installable) {
                    installMod(identifier);
                }
            }
            if (user.RaiseYesNoDialog($"Import complete. Delete {deletable.Count} old files?")) {
                // Delete old files
                foreach (FileInfo f in deletable) {
                    f.Delete();
                }
            }
        }

        private static string GetFileHashSha1(string filePath)
        {
            using (FileStream     fs   = new FileStream(filePath, FileMode.Open))
            using (BufferedStream bs   = new BufferedStream(fs))
            using (SHA1Cng        sha1 = new SHA1Cng())
            {
                return BitConverter.ToString(sha1.ComputeHash(bs)).Replace("-", "");
            }
        }

    }

}
