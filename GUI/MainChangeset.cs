using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {

        private List<ModChange> changeSet;

        public void UpdateChangesDialog(List<ModChange> changeset, BackgroundWorker installWorker)
        {
            changeSet = changeset;
            this.installWorker = installWorker;
            ChangesListView.Items.Clear();

            if (changeset == null)
            {
                return;
            }

            // We're going to split our change-set into two parts: updated/removed mods,
            // and everything else (which right now is installing mods, but we may have
            // other types in the future).

            changeSet = new List<ModChange>();
            changeSet.AddRange(changeset.Where(change => change.ChangeType == GUIModChangeType.Remove));
            changeSet.AddRange(changeset.Where(change => change.ChangeType == GUIModChangeType.Update));

            IEnumerable<ModChange> leftOver = changeset.Where(change => change.ChangeType != GUIModChangeType.Remove
                                                && change.ChangeType != GUIModChangeType.Update);

            // Now make our list more human-friendly (dependencies for a mod are listed directly
            // after it.)
            CreateSortedModList(leftOver);

            changeset = changeSet;

            foreach (var change in changeset)
            {
                if (change.ChangeType == GUIModChangeType.None)
                {
                    continue;
                }

                ListViewItem item = new ListViewItem()
                {
                    Text = CurrentInstance.Cache.IsCachedZip(change.Mod.ToModule())
                        ? $"{change.Mod.Name} {change.Mod.Version} (cached)"
                        : $"{change.Mod.Name} {change.Mod.Version} ({change.Mod.ToModule()?.download.Host ?? ""}, {change.Mod.DownloadSize})"
                };

                var sub_change_type = new ListViewItem.ListViewSubItem {Text = change.ChangeType.ToString()};

                ListViewItem.ListViewSubItem description = new ListViewItem.ListViewSubItem();
                description.Text = change.Reason.Reason.Trim();

                if (change.ChangeType == GUIModChangeType.Update)
                {
                    description.Text = String.Format("Update selected by user to version {0}.", change.Mod.LatestVersion);
                }

                if (change.ChangeType == GUIModChangeType.Install && change.Reason is SelectionReason.UserRequested)
                {
                    description.Text = "New mod install selected by user.";
                }

                item.SubItems.Add(sub_change_type);
                item.SubItems.Add(description);
                ChangesListView.Items.Add(item);
            }
        }

        /// <summary>
        /// This method creates the Install part of the changeset
        /// It arranges the changeset in a human-friendly order
        /// The requested mod is listed first, it's dependencies right after it
        /// So we get for example "ModuleRCSFX" directly after "USI Exploration Pack"
        ///
        /// It is very likely that this is forward-compatible with new ChangeTypes's,
        /// like a a "reconfigure" changetype, but only the future will tell
        /// </summary>
        /// <param name="changes">Every leftover ModChange that should be sorted</param>
        /// <param name="parent"></param>
        private void CreateSortedModList(IEnumerable<ModChange> changes, ModChange parent=null)
        {
            foreach (ModChange change in changes)
            {
                bool goDeeper = parent == null || change.Reason.Parent.identifier == parent.Mod.Identifier;

                if (goDeeper)
                {
                    if (!changeSet.Any(c => c.Mod.Identifier == change.Mod.Identifier))
                        changeSet.Add(change);
                    CreateSortedModList(changes.Where(c => !(c.Reason is SelectionReason.UserRequested)), change);
                }
            }
        }

        private void CancelChangesButton_Click(object sender, EventArgs e)
        {
            UpdateModsList();
            UpdateChangesDialog(null, installWorker);
            tabController.ShowTab("ManageModsTabPage");
            tabController.HideTab("ChangesetTabPage");
            ApplyToolButton.Enabled = false;
        }

        private void ConfirmChangesButton_Click(object sender, EventArgs e)
        {
            if (changeSet == null)
                return;

            menuStrip1.Enabled = false;
            RetryCurrentActionButton.Visible = false;

            RelationshipResolverOptions install_ops = RelationshipResolver.DefaultOpts();
            install_ops.with_recommends = false;
            //Using the changeset passed in can cause issues with versions.
            // An example is Mechjeb for FAR at 25/06/2015 with a 1.0.2 install.
            // TODO Work out why this is.
            installWorker.RunWorkerAsync(
                new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                    mainModList.ComputeUserChangeSet().ToList(),
                    install_ops
                )
            );
            ShowWaitDialog();
        }

    }
}
