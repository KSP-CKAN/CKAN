using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    public partial class Main
    {
        private void importDownloadsToolStripMenuItem_Click(object? sender, EventArgs? e)
        {
            ImportModules();
        }

        private void ImportModules()
        {
            if (CurrentInstance != null)
            {
                // Prompt the user to select one or more ZIP files
                var dlg = new OpenFileDialog()
                {
                    Title            = Properties.Resources.MainImportTitle,
                    AddExtension     = true,
                    CheckFileExists  = true,
                    CheckPathExists  = true,
                    InitialDirectory = FindDownloadsPath(CurrentInstance),
                    DefaultExt       = "zip",
                    Filter           = Properties.Resources.MainImportFilter,
                    Multiselect      = true
                };
                if (dlg.ShowDialog(this) == DialogResult.OK
                        && dlg.FileNames.Length > 0)
                {
                    // Show WaitTabPage (status page) and lock it.
                    tabController.RenameTab(WaitTabPage.Name, Properties.Resources.MainImportWaitTitle);
                    ShowWaitDialog();
                    DisableMainWindow();
                    Wait.StartWaiting(
                        (sender, e) =>
                        {
                            if (e != null && Manager?.Cache != null)
                            {
                                e.Result = ModuleImporter.ImportFiles(
                                    GetFiles(dlg.FileNames),
                                    currentUser,
                                    mod =>
                                    {
                                        if (ManageMods.mainModList
                                                      .full_list_of_mod_rows
                                                      .TryGetValue(mod.identifier,
                                                                   out DataGridViewRow? row)
                                            && row.Tag is GUIMod gmod)
                                        {
                                            gmod.SelectedMod = mod;
                                        }
                                    },
                                    RegistryManager.Instance(CurrentInstance, repoData).registry,
                                    CurrentInstance, Manager.Cache);
                            }
                        },
                        (sender, e) =>
                        {
                            if (e?.Error == null && e?.Result is bool result && result)
                            {
                                // Put GUI back the way we found it
                                HideWaitDialog();
                            }
                            else
                            {
                                if (e?.Error is Exception exc)
                                {
                                    log.Error(exc.Message, exc);
                                    currentUser.RaiseMessage("{0}", exc.Message);
                                }
                                Wait.Finish();
                            }
                            EnableMainWindow();
                        },
                        false,
                        null);
                }
            }
        }

        private static HashSet<FileInfo> GetFiles(string[] filenames)
            => filenames.Select(fn => new FileInfo(fn))
                        .ToHashSet();

        private static readonly string[] downloadPaths = new string[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        private static string FindDownloadsPath(GameInstance gameInst)
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
