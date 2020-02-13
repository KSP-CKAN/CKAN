using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using CKAN.Extensions;

namespace CKAN
{
    public partial class Main
    {
        #region Notifications

        private void ManageMods_LabelsAfterUpdate(IEnumerable<GUIMod> mods)
        {
            Util.Invoke(Main.Instance, () =>
            {
                mods = mods.Memoize();
                var notifLabs = ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                    .Where(l => l.NotifyOnChange)
                    .Memoize();
                var toNotif = mods
                    .Where(m =>
                        notifLabs.Any(l =>
                            l.ModuleIdentifiers.Contains(m.Identifier)))
                    .Select(m => m.Name)
                    .Memoize();
                if (toNotif.Any())
                {
                    MessageBox.Show(
                        string.Format(
                            Properties.Resources.MainLabelsUpdateMessage,
                            string.Join("\r\n", toNotif)
                        ),
                        Properties.Resources.MainLabelsUpdateTitle,
                        MessageBoxButtons.OK
                    );
                }

                foreach (GUIMod mod in mods)
                {
                    foreach (ModuleLabel l in ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                        .Where(l => l.RemoveOnChange
                            && l.ModuleIdentifiers.Contains(mod.Identifier)))
                    {
                        l.Remove(mod.Identifier);
                    }
                }
            });
        }

        private void LabelsAfterInstall(CkanModule mod)
        {
            foreach (ModuleLabel l in ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                .Where(l => l.RemoveOnInstall && l.ModuleIdentifiers.Contains(mod.identifier)))
            {
                l.Remove(mod.identifier);
            }
        }

        #endregion
	}
}
