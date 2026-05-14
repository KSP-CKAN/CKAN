using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

using Autofac;

using CKAN.Configuration;
using CKAN.IO;

namespace CKAN.GUI
{
    public partial class Main
    {
        private void ImportDownloadsToolStripMenuItem_Click(object? sender, EventArgs? e)
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
                                var toInstall = new List<CkanModule>();
                                e.Result = (success: ModuleImporter.ImportFiles(
                                                         GetFiles(dlg.FileNames),
                                                         currentUser,
                                                         toInstall.Add,
                                                         RegistryManager.Instance(CurrentInstance, repoData).registry,
                                                         CurrentInstance, Manager.Cache),
                                            toInstall);
                            }
                        },
                        (sender, e) =>
                        {
                            EnableMainWindow();
                            switch (e)
                            {
                                case { Error: Kraken k }:
                                    log.Error(k.Message, k);
                                    currentUser.RaiseMessage("{0}", k.Message);
                                    Wait.Finish();
                                    break;

                                case { Error: Exception exc }:
                                    log.Error(exc.Message, exc);
                                    currentUser.RaiseMessage("{0}", exc.ToString());
                                    Wait.Finish();
                                    break;

                                case { Result: (false, _) }:
                                    // Failed, error already shown by importer
                                    Wait.Finish();
                                    break;

                                case { Result: (true, List<CkanModule> { Count: < 1 }) }:
                                    // Succeeded, nothing to import; put GUI back the way we found it
                                    HideWaitDialog();
                                    break;

                                case { Result: (true, List<CkanModule> { Count: > 0 } toInstall) }:
                                    var regMgr = RegistryManager.Instance(CurrentInstance, repoData);
                                    var tuple = ModList.ComputeFullChangeSetFromUserChangeSet(
                                        regMgr.registry,
                                        toInstall.Select(m => new ModChange(m, GUIModChangeType.Install,
                                                                            ServiceLocator.Container.Resolve<IConfiguration>()))
                                                 .ToHashSet(),
                                        ServiceLocator.Container.Resolve<IConfiguration>(), CurrentInstance);
                                    UpdateChangesDialog(tuple.Item1.ToList(), tuple.Item2);
                                    HideWaitDialog();
                                    tabController.ShowTab(ChangesetTabPage.Name, 1);
                                    break;
                            }
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
            => downloadPaths.FirstOrDefault(p => !string.IsNullOrEmpty(p)
                                                 && Directory.Exists(p))
                            ?? gameInst.GameDir;

    }
}
