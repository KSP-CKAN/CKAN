using System;

using CKAN.IO;

namespace CKAN.GUI
{
    public partial class Main
    {
        private void deduplicateToolstripMenuItem_Click(object? sender, EventArgs? evt)
        {
            // Show WaitTabPage (status page) and lock it.
            tabController.RenameTab(WaitTabPage.Name,
                                    Properties.Resources.MainDeduplicateWaitTitle);
            ShowWaitDialog();
            DisableMainWindow();
            Wait.StartWaiting(
                (sender, e) =>
                {
                    if (e != null)
                    {
                        currentUser.RaiseMessage(Properties.Resources.MainDeduplicateScanning);
                        var deduper = new InstalledFilesDeduplicator(Manager.Instances.Values,
                                                                     repoData);
                        deduper.DeduplicateAll(currentUser);
                        e.Result = true;
                    }
                },
                (sender, e) =>
                {
                    switch (e?.Error)
                    {
                        case CancelledActionKraken:
                            HideWaitDialog();
                            break;
                        case Exception exc:
                            currentUser.RaiseMessage("{0}", exc.Message);
                            break;
                    }
                    Wait.Finish();
                    EnableMainWindow();
                },
                false,
                null);
        }
    }
}
