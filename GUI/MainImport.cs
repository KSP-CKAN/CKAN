using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CKAN
{

    public partial class Main
    {

        private void importDownloadsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportModules();
        }

        private void ImportModules()
        {
            // Prompt the user to select one or more ZIP files
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Title            = "Import Mods",
                AddExtension     = true,
                CheckFileExists  = true,
                CheckPathExists  = true,
                InitialDirectory = FindDownloadsPath(CurrentInstance),
                DefaultExt       = "zip",
                Filter           = "Mods (*.zip)|*.zip",
                Multiselect      = true
            };
            if (dlg.ShowDialog() == DialogResult.OK
                    && dlg.FileNames.Length > 0)
            {

                // Set up GUI to respond to IUser calls...
                GUIUser.DisplayYesNo old_YesNoDialog = currentUser.displayYesNo;
                currentUser.displayYesNo = YesNoDialog;
                tabController.RenameTab("WaitTabPage", "Status log");
                tabController.ShowTab("WaitTabPage");
                tabController.SetTabLock(true);

                try
                {
                    ModuleInstaller.GetInstance(CurrentInstance, currentUser).ImportFiles(
                        GetFiles(dlg.FileNames),
                        currentUser,
                        (string id) => MarkModForInstall(id, false)
                    );
                }
                finally
                {
                    // Put GUI back the way we found it
                    currentUser.displayYesNo = old_YesNoDialog;
                    tabController.SetTabLock(false);
                    tabController.HideTab("WaitTabPage");
                }
            }
        }

        private HashSet<FileInfo> GetFiles(string[] filenames)
        {
            HashSet<FileInfo> files = new HashSet<FileInfo>();
            foreach (string fn in filenames)
            {
                files.Add(new FileInfo(fn));
            }
            return files;
        }

        private static readonly string[] downloadPaths = new string[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        private static string FindDownloadsPath(KSP gameInst)
        {
            foreach (string p in downloadPaths)
            {
                if (!string.IsNullOrEmpty(p) && Directory.Exists(p))
                {
                    return p;
                }
            }
            return gameInst.GameDir();
        }

    }

}
