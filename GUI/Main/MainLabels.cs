using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using CKAN.Extensions;

namespace CKAN.GUI
{
    public partial class Main
    {
        #region Notifications

        private void ManageMods_LabelsAfterUpdate(IEnumerable<GUIMod> mods)
        {
            Util.Invoke(this, () =>
            {
                mods = mods.Memoize();
                var notifLabs = ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                    .Where(l => l.NotifyOnChange)
                    .Memoize();
                var toNotif = mods
                    .Where(m =>
                        notifLabs.Any(l =>
                            l.ContainsModule(CurrentInstance.game, m.Identifier)))
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
                            && l.ContainsModule(CurrentInstance.game, mod.Identifier)))
                    {
                        l.Remove(CurrentInstance.game, mod.Identifier);
                    }
                }
            });
        }

        private void LabelsAfterInstall(CkanModule mod)
        {
            foreach (ModuleLabel l in ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                .Where(l => l.RemoveOnInstall && l.ContainsModule(CurrentInstance.game, mod.identifier)))
            {
                l.Remove(CurrentInstance.game, mod.identifier);
            }
        }

        public bool LabelsHeld(string identifier)
            => ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                .Any(l => l.HoldVersion && l.ContainsModule(CurrentInstance.game, identifier));

        #endregion
    }
}
