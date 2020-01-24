using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using CKAN.Extensions;

namespace CKAN
{
    public partial class Main
    {
        #region Filter dropdown

        private void FilterToolButton_DropDown_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // The menu items' dropdowns can't be accessed if they're empty
            FilterTagsToolButton_DropDown_Opening(null, null);
            FilterLabelsToolButton_DropDown_Opening(null, null);
        }

        private void FilterTagsToolButton_DropDown_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FilterTagsToolButton.DropDownItems.Clear();
            foreach (var kvp in mainModList.ModuleTags.Tags.OrderBy(kvp => kvp.Key))
            {
                FilterTagsToolButton.DropDownItems.Add(new ToolStripMenuItem(
                    $"{kvp.Key} ({kvp.Value.ModuleIdentifiers.Count})",
                    null, tagFilterButton_Click
                )
                {
                    Tag = kvp.Value
                });
            }
            FilterTagsToolButton.DropDownItems.Add(untaggedFilterToolStripSeparator);
            FilterTagsToolButton.DropDownItems.Add(new ToolStripMenuItem(
                string.Format(Properties.Resources.MainLabelsUntagged, mainModList.ModuleTags.Untagged.Count),
                null, tagFilterButton_Click
            )
            {
                Tag = null
            });
        }

        private void FilterLabelsToolButton_DropDown_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FilterLabelsToolButton.DropDownItems.Clear();
            foreach (ModuleLabel mlbl in mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name))
            {
                FilterLabelsToolButton.DropDownItems.Add(new ToolStripMenuItem(
                    $"{mlbl.Name} ({mlbl.ModuleIdentifiers.Count})",
                    null, customFilterButton_Click
                )
                {
                    Tag = mlbl
                });
            }
        }

        private void tagFilterButton_Click(object sender, EventArgs e)
        {
            var clicked = sender as ToolStripMenuItem;
            Filter(GUIModFilter.Tag, clicked.Tag as ModuleTag, null);
        }

        private void customFilterButton_Click(object sender, EventArgs e)
        {
            var clicked = sender as ToolStripMenuItem;
            Filter(GUIModFilter.CustomLabel, null, clicked.Tag as ModuleLabel);
        }

        #endregion

        #region Right click menu

        private void LabelsContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LabelsContextMenuStrip.Items.Clear();

            var module = GetSelectedModule();
            foreach (ModuleLabel mlbl in mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name))
            {
                LabelsContextMenuStrip.Items.Add(
                    new ToolStripMenuItem(mlbl.Name, null, labelMenuItem_Click)
                    {
                        Checked      = mlbl.ModuleIdentifiers.Contains(module.Identifier),
                        CheckOnClick = true,
                        Tag          = mlbl,
                    }
                );
            }
            LabelsContextMenuStrip.Items.Add(labelToolStripSeparator);
            LabelsContextMenuStrip.Items.Add(editLabelsToolStripMenuItem);
            e.Cancel = false;
        }

        private void labelMenuItem_Click(object sender, EventArgs e)
        {
            var item   = sender   as ToolStripMenuItem;
            var mlbl   = item.Tag as ModuleLabel;
            var module = GetSelectedModule();
            if (item.Checked)
            {
                mlbl.Add(module.Identifier);
            }
            else
            {
                mlbl.Remove(module.Identifier);
            }
            mainModList.ReapplyLabels(module, Conflicts?.ContainsKey(module) ?? false, CurrentInstance.Name);
            mainModList.ModuleLabels.Save(ModuleLabelList.DefaultPath);
        }

        private void editLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditLabelsDialog eld = new EditLabelsDialog(currentUser, Manager, mainModList.ModuleLabels);
            eld.ShowDialog(this);
            eld.Dispose();
            mainModList.ModuleLabels.Save(ModuleLabelList.DefaultPath);
            foreach (GUIMod module in mainModList.Modules)
            {
                mainModList.ReapplyLabels(module, Conflicts?.ContainsKey(module) ?? false, CurrentInstance.Name);
            }
        }

        #endregion

        #region Notifications

        private void LabelsAfterUpdate(IEnumerable<GUIMod> mods)
        {
            Util.Invoke(Main.Instance, () =>
            {
                mods = mods.Memoize();
                var notifLabs = mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
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
                    foreach (ModuleLabel l in mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
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
            foreach (ModuleLabel l in mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                .Where(l => l.RemoveOnInstall && l.ModuleIdentifiers.Contains(mod.identifier)))
            {
                l.Remove(mod.identifier);
            }
        }

        #endregion
	}
}
