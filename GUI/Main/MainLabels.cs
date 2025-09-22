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
            if (CurrentInstance != null)
            {
                Util.Invoke(this, () =>
                {
                    mods = mods.Memoize();
                    var notifLabs = ModuleLabelList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                        .Where(l => l.NotifyOnChange)
                        .Memoize();
                    var toNotif = mods
                        .Where(m =>
                            notifLabs.Any(l =>
                                l.ContainsModule(CurrentInstance.Game, m.Identifier)))
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
                        foreach (ModuleLabel l in ModuleLabelList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                            .Where(l => l.RemoveOnChange
                                && l.ContainsModule(CurrentInstance.Game, mod.Identifier)))
                        {
                            l.Remove(CurrentInstance.Game, mod.Identifier);
                        }
                    }
                });
            }
        }

        private void LabelsAfterInstall(CkanModule mod)
        {
            if (CurrentInstance != null)
            {
                foreach (var l in ModuleLabelList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                    .Where(l => l.RemoveOnInstall && l.ContainsModule(CurrentInstance.Game, mod.identifier)))
                {
                    l.Remove(CurrentInstance.Game, mod.identifier);
                }
            }
        }

        public bool LabelsHeld(string identifier)
            => CurrentInstance != null
                && ModuleLabelList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                                               .Any(l => l.HoldVersion && l.ContainsModule(CurrentInstance.Game, identifier));

        public bool LabelsIgnoreMissing(string identifier)
            => CurrentInstance != null
                && ModuleLabelList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                                               .Any(l => l.IgnoreMissingFiles && l.ContainsModule(CurrentInstance.Game, identifier));

        #endregion
    }
}
