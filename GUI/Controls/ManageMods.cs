using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using log4net;
using CKAN.Versioning;

namespace CKAN
{
    public partial class ManageMods : UserControl
    {
        public ManageMods()
        {
            InitializeComponent();

            mainModList = new ModList(source => UpdateFilters());
            FilterToolButton.MouseHover += (sender, args) => FilterToolButton.ShowDropDown();
            launchKSPToolStripMenuItem.MouseHover += (sender, args) => launchKSPToolStripMenuItem.ShowDropDown();
            ApplyToolButton.MouseHover += (sender, args) => ApplyToolButton.ShowDropDown();
            ApplyToolButton.Enabled = false;

            // History is read-only until the UI is started. We switch
            // out of it at the end of OnLoad() when we call NavInit().
            navHistory = new NavigationHistory<GUIMod> { IsReadOnly = true };

            // Initialize navigation. This should be called as late as
            // possible, once the UI is "settled" from its initial load.
            NavInit();

            if (Platform.IsMono)
            {
                menuStrip2.Renderer = new FlatToolStripRenderer();
                FilterToolButton.DropDown.Renderer = new FlatToolStripRenderer();
                FilterTagsToolButton.DropDown.Renderer = new FlatToolStripRenderer();
                FilterLabelsToolButton.DropDown.Renderer = new FlatToolStripRenderer();
                ModListContextMenuStrip.Renderer = new FlatToolStripRenderer();
                ModListHeaderContextMenuStrip.Renderer = new FlatToolStripRenderer();
                LabelsContextMenuStrip.Renderer = new FlatToolStripRenderer();
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(ManageMods));
        private DateTime lastSearchTime;
        private string lastSearchKey;
        private NavigationHistory<GUIMod> navHistory;

        private IEnumerable<ModChange> currentChangeSet;
        private Dictionary<GUIMod, string> conflicts;

        public readonly ModList mainModList;

        public event Action<GUIMod> OnSelectedModuleChanged;
        public event Action<IEnumerable<ModChange>> OnChangeSetChanged;
        public event Action OnRegistryChanged;

        public event Action<List<ModChange>> StartChangeSet;
        public event Action OpenProgressTab;
        public event Action CloseProgressTab;
        public event Action<IEnumerable<GUIMod>> LabelsAfterUpdate;

        private IEnumerable<ModChange> ChangeSet
        {
            get { return currentChangeSet; }
            set
            {
                var orig = currentChangeSet;
                currentChangeSet = value;
                if (!ReferenceEquals(orig, value))
                    ChangeSetUpdated();
            }
        }

        private void ChangeSetUpdated()
        {
            if (ChangeSet != null && ChangeSet.Any())
            {
                ApplyToolButton.Enabled = true;
            }
            else
            {
                ApplyToolButton.Enabled = false;
                InstallAllCheckbox.Checked = true;
            }
            if (OnChangeSetChanged != null)
            {
                OnChangeSetChanged(ChangeSet);
            }
        }

        private Dictionary<GUIMod, string> Conflicts
        {
            get { return conflicts; }
            set
            {
                var orig = conflicts;
                conflicts = value;
                if (orig != value)
                    ConflictsUpdated(orig);
            }
        }

        private void ConflictsUpdated(Dictionary<GUIMod, string> prevConflicts)
        {
            if (Conflicts == null)
            {
                // Clear status bar if no conflicts
                Main.Instance.AddStatusMessage("");
            }

            if (prevConflicts != null)
            {
                // Mark old conflicts as non-conflicted
                // (rows that are _still_ conflicted will be marked as such in the next loop)
                foreach (GUIMod guiMod in prevConflicts.Keys)
                {
                    DataGridViewRow row = mainModList.full_list_of_mod_rows[guiMod.Identifier];

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.ToolTipText = null;
                    }
                    mainModList.ReapplyLabels(guiMod, false, Main.Instance.CurrentInstance.Name);
                    if (row.Visible)
                    {
                        ModGrid.InvalidateRow(row.Index);
                    }
                }
            }
            if (Conflicts != null)
            {
                // Mark current conflicts as conflicted
                foreach (var kvp in Conflicts)
                {
                    GUIMod          guiMod = kvp.Key;
                    DataGridViewRow row    = mainModList.full_list_of_mod_rows[guiMod.Identifier];
                    string conflict_text = kvp.Value;

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.ToolTipText = conflict_text;
                    }
                    row.DefaultCellStyle.BackColor = mainModList.GetRowBackground(guiMod, true, Main.Instance.CurrentInstance.Name);
                    if (row.Visible)
                    {
                        ModGrid.InvalidateRow(row.Index);
                    }
                }
            }
        }

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            Main.Instance.UpdateRepo();
        }

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
            foreach (ModuleLabel mlbl in mainModList.ModuleLabels.LabelsFor(Main.Instance.CurrentInstance.Name))
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

        #region Filter right click menu

        private void LabelsContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LabelsContextMenuStrip.Items.Clear();

            var module = SelectedModule;
            foreach (ModuleLabel mlbl in mainModList.ModuleLabels.LabelsFor(Main.Instance.CurrentInstance.Name))
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
            var module = SelectedModule;
            if (item.Checked)
            {
                mlbl.Add(module.Identifier);
            }
            else
            {
                mlbl.Remove(module.Identifier);
            }
            mainModList.ReapplyLabels(module, Conflicts?.ContainsKey(module) ?? false, Main.Instance.CurrentInstance.Name);
            mainModList.ModuleLabels.Save(ModuleLabelList.DefaultPath);
        }

        private void editLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditLabelsDialog eld = new EditLabelsDialog(Main.Instance.currentUser, Main.Instance.Manager, mainModList.ModuleLabels);
            eld.ShowDialog(this);
            eld.Dispose();
            mainModList.ModuleLabels.Save(ModuleLabelList.DefaultPath);
            foreach (GUIMod module in mainModList.Modules)
            {
                mainModList.ReapplyLabels(module, Conflicts?.ContainsKey(module) ?? false, Main.Instance.CurrentInstance.Name);
            }
        }

        #endregion

        private void FilterCompatibleButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Compatible);
        }

        private void FilterInstalledButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Installed);
        }

        private void FilterInstalledUpdateButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.InstalledUpdateAvailable);
        }

        private void FilterReplaceableButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Replaceable);
        }

        private void FilterCachedButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Cached);
        }

        private void FilterUncachedButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Uncached);
        }

        private void FilterNewButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.NewInRepository);
        }

        private void FilterNotInstalledButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.NotInstalled);
        }

        private void FilterIncompatibleButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Incompatible);
        }

        private void FilterAllButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.All);
        }

        /// <summary>
        /// Called when the ModGrid filter (all, compatible, incompatible...) is changed.
        /// </summary>
        /// <param name="filter">Filter.</param>
        public void Filter(GUIModFilter filter, ModuleTag tag = null, ModuleLabel label = null)
        {
            // Triggers mainModList.ModFiltersUpdated()
            mainModList.TagFilter = tag;
            mainModList.CustomLabelFilter = label;
            mainModList.ModFilter = filter;

            // Save new filter to the configuration.
            Main.Instance.configuration.ActiveFilter = (int)mainModList.ModFilter;
            Main.Instance.configuration.CustomLabelFilter = label?.Name;
            Main.Instance.configuration.TagFilter = tag?.Name;
            Main.Instance.configuration.Save();

            // Ask the configuration which columns to show.
            foreach (DataGridViewColumn col in ModGrid.Columns)
            {
                // Some columns are always shown, and others are handled by UpdateModsList()
                if (col.Name != "Installed" && col.Name != "UpdateCol" && col.Name != "ReplaceCol")
                {
                    col.Visible = !Main.Instance.configuration.HiddenColumnNames.Contains(col.Name);
                }
            }

            switch (filter)
            {
                // Some columns really do / don't make sense to be visible on certain filter settings.
                // Hide / Show them, without writing to config, so once the user changes tab again,
                // they are shown / hidden again, as before.
                case GUIModFilter.All:                      FilterToolButton.Text = Properties.Resources.MainFilterAll;          break;
                case GUIModFilter.Incompatible:             FilterToolButton.Text = Properties.Resources.MainFilterIncompatible; break;
                case GUIModFilter.Installed:                FilterToolButton.Text = Properties.Resources.MainFilterInstalled;    break;
                case GUIModFilter.InstalledUpdateAvailable: FilterToolButton.Text = Properties.Resources.MainFilterUpgradeable;  break;
                case GUIModFilter.Replaceable:              FilterToolButton.Text = Properties.Resources.MainFilterReplaceable;  break;
                case GUIModFilter.Cached:                   FilterToolButton.Text = Properties.Resources.MainFilterCached;       break;
                case GUIModFilter.Uncached:                 FilterToolButton.Text = Properties.Resources.MainFilterUncached;     break;
                case GUIModFilter.NewInRepository:          FilterToolButton.Text = Properties.Resources.MainFilterNew;          break;
                case GUIModFilter.NotInstalled:             ModGrid.Columns["InstalledVersion"].Visible = false;
                                                            ModGrid.Columns["InstallDate"].Visible      = false;
                                                            ModGrid.Columns["AutoInstalled"].Visible    = false;
                                                            FilterToolButton.Text = Properties.Resources.MainFilterNotInstalled; break;
                case GUIModFilter.CustomLabel:              FilterToolButton.Text = string.Format(Properties.Resources.MainFilterLabel, label?.Name ?? "CUSTOM"); break;
                case GUIModFilter.Tag:
                    FilterToolButton.Text = tag == null
                        ? Properties.Resources.MainFilterUntagged
                        : string.Format(Properties.Resources.MainFilterTag, tag.Name);
                    break;
                default:                                    FilterToolButton.Text = Properties.Resources.MainFilterCompatible;   break;
            }
        }

        public void MarkAllUpdates()
        {
            foreach (DataGridViewRow row in ModGrid.Rows)
            {
                var mod = (GUIMod)row.Tag;
                if (mod.HasUpdate)
                {
                    MarkModForUpdate(mod.Identifier, true);
                }
            }

            // only sort by Update column if checkbox in settings checked
            if (Main.Instance.configuration.AutoSortByUpdate)
            {
                // set new sort column
                var new_sort_column = ModGrid.Columns[UpdateCol.Index];
                var current_sort_column = ModGrid.Columns[Main.Instance.configuration.SortByColumnIndex];

                // Reset the glyph.
                current_sort_column.HeaderCell.SortGlyphDirection = SortOrder.None;
                Main.Instance.configuration.SortByColumnIndex = new_sort_column.Index;
                UpdateFilters();

                // Select the top row and scroll the list to it.
                ModGrid.CurrentCell = ModGrid.Rows[0].Cells[SelectableColumnIndex()];
            }

            ModGrid.Refresh();
        }

        private void MarkAllUpdatesToolButton_Click(object sender, EventArgs e)
        {
            MarkAllUpdates();
        }

        private void ApplyToolButton_Click(object sender, EventArgs e)
        {
            Main.Instance.tabController.ShowTab("ChangesetTabPage", 1);
        }

        public void MarkModForUpdate(string identifier, bool value)
        {
            Util.Invoke(this, () => _MarkModForUpdate(identifier, value));
        }

        private void _MarkModForUpdate(string identifier, bool value)
        {
            DataGridViewRow row = mainModList.full_list_of_mod_rows[identifier];
            var mod = (GUIMod)row.Tag;
            mod.SetUpgradeChecked(row, UpdateCol, value);
        }

        private void launchKSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.Instance.LaunchKSP();
        }

        private void NavBackwardToolButton_Click(object sender, EventArgs e)
        {
            NavGoBackward();
        }

        private void NavForwardToolButton_Click(object sender, EventArgs e)
        {
            NavGoForward();
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Skip if already disposed (i.e. after the form has been closed).
            // Needed for TransparentTextBoxes
            if (IsDisposed)
            {
                return;
            }

            var module = SelectedModule;
            if (OnSelectedModuleChanged != null)
            {
                OnSelectedModuleChanged(module);
            }
            if (module != null)
            {
                NavSelectMod(module);
            }
        }

        /// <summary>
        /// Called when there's a click on the ModGrid header row.
        /// Handles sorting and the header right click context menu.
        /// </summary>
        private void ModList_HeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Left click -> sort by new column / change sorting direction.
            if (e.Button == MouseButtons.Left)
            {
                var new_sort_column = ModGrid.Columns [e.ColumnIndex];
                var current_sort_column = ModGrid.Columns [Main.Instance.configuration.SortByColumnIndex];

                // Reverse the sort order if the current sorting column is clicked again.
                Main.Instance.configuration.SortDescending = new_sort_column == current_sort_column && !Main.Instance.configuration.SortDescending;

                // Reset the glyph.
                current_sort_column.HeaderCell.SortGlyphDirection = SortOrder.None;
                Main.Instance.configuration.SortByColumnIndex = new_sort_column.Index;
                UpdateFilters();
            }
            // Right click -> Bring up context menu to change visibility of columns.
            else if (e.Button == MouseButtons.Right)
            {
                // Start from scrap: clear the entire item list, then add all options again.
                ModListHeaderContextMenuStrip.Items.Clear();

                // Add columns
                ModListHeaderContextMenuStrip.Items.AddRange(
                    ModGrid.Columns.Cast<DataGridViewColumn>()
                    .Where(col => col.Name != "Installed" && col.Name != "UpdateCol" && col.Name != "ReplaceCol")
                    .Select(col => new ToolStripMenuItem()
                    {
                        Name    = col.Name,
                        Text    = col.HeaderText,
                        Checked = col.Visible,
                        Tag     = col
                    })
                    .ToArray()
                );

                // Separator
                ModListHeaderContextMenuStrip.Items.Add(new ToolStripSeparator());

                // Add tags
                ModListHeaderContextMenuStrip.Items.AddRange(
                    mainModList.ModuleTags.Tags.OrderBy(kvp => kvp.Key)
                    .Select(kvp => new ToolStripMenuItem()
                    {
                        Name    = kvp.Key,
                        Text    = kvp.Key,
                        Checked = kvp.Value.Visible,
                        Tag     = kvp.Value,
                    })
                    .ToArray()
                );

                // Show the context menu on cursor position.
                ModListHeaderContextMenuStrip.Show(Cursor.Position);
            }
        }

        /// <summary>
        /// Called if a ToolStripButton of the header context menu is pressed.
        /// </summary>
        private void ModListHeaderContextMenuStrip_ItemClicked(object sender, System.Windows.Forms.ToolStripItemClickedEventArgs e)
        {
            // ClickedItem is of type ToolStripItem, we need ToolStripButton.
            ToolStripMenuItem  clickedItem = e.ClickedItem    as ToolStripMenuItem;
            DataGridViewColumn col         = clickedItem?.Tag as DataGridViewColumn;
            ModuleTag          tag         = clickedItem?.Tag as ModuleTag;

            if (col != null)
            {
                col.Visible = !clickedItem.Checked;
                Main.Instance.configuration.SetColumnVisibility(col.Name, !clickedItem.Checked);
                if (col.Index == 0)
                {
                    InstallAllCheckbox.Visible = col.Visible;
                }
            }
            else if (tag != null)
            {
                tag.Visible = !clickedItem.Checked;
                if (tag.Visible)
                {
                    mainModList.ModuleTags.HiddenTags.Remove(tag.Name);
                }
                else
                {
                    mainModList.ModuleTags.HiddenTags.Add(tag.Name);
                }
                mainModList.ModuleTags.Save(ModuleTagList.DefaultPath);
                UpdateFilters();
            }
        }

        /// <summary>
        /// Called on key down when the mod list is focused.
        /// Makes the Home/End keys go to the top/bottom of the list respectively.
        /// </summary>
        private void ModList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Home:
                    // First row.
                    ModGrid.CurrentCell = ModGrid.Rows[0].Cells[SelectableColumnIndex()];
                    e.Handled = true;
                    break;

                case Keys.End:
                    // Last row.
                    ModGrid.CurrentCell = ModGrid.Rows[ModGrid.Rows.Count - 1].Cells[SelectableColumnIndex()];
                    e.Handled = true;
                    break;

                case Keys.Space:
                    // If they've focused one of the checkbox columns, don't intercept
                    if (ModGrid.CurrentCell.ColumnIndex > 3)
                    {
                        DataGridViewRow row = ModGrid.CurrentRow;
                        // Toggle Update column if enabled, otherwise Install
                        for (int colIndex = 2; colIndex >= 0; --colIndex)
                        {
                            if (row?.Cells[colIndex] is DataGridViewCheckBoxCell)
                            {
                                // Need to change the state here, because the user hasn't clicked on a checkbox
                                row.Cells[colIndex].Value = !(bool)row.Cells[colIndex].Value;
                                ModGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                                e.Handled = true;
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Called on key press when the mod is focused. Scrolls to the first mod with name
        /// beginning with the key pressed. If more than one unique keys are pressed in under
        /// a second, it searches for the combination of the keys pressed. If the same key is
        /// being pressed repeatedly, it cycles through mods names beginning with that key.
        /// If space is pressed, the checkbox at the current row is toggled.
        /// </summary>
        private void ModList_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Don't search for spaces or newlines
            if (e.KeyChar == (char)Keys.Space || e.KeyChar == (char)Keys.Enter)
            {
                return;
            }

            var key = e.KeyChar.ToString();
            // Determine time passed since last key press.
            TimeSpan interval = DateTime.Now - lastSearchTime;
            if (interval.TotalSeconds < 1)
            {
                // Last keypress was < 1 sec ago, so combine the last and current keys.
                key = lastSearchKey + key;
            }

            // Remember the current time and key.
            lastSearchTime = DateTime.Now;
            lastSearchKey = key;

            if (key.Distinct().Count() == 1)
            {
                // Treat repeating and single keypresses the same.
                key = key.Substring(0, 1);
            }

            FocusMod(key, false);
            e.Handled = true;
        }

        /// <summary>
        /// I'm pretty sure this is what gets called when the user clicks on a ticky in the mod list.
        /// </summary>
        private void ModList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ModGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void ModList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = ModGrid.Rows[e.RowIndex];
            if (!(row.Cells[0] is DataGridViewCheckBoxCell))
                return;

            // Need to change the state here, because the user hasn't clicked on a checkbox.
            row.Cells[0].Value = !(bool)row.Cells[0].Value;
            ModGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private async void ModList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int row_index    = e.RowIndex;
            int column_index = e.ColumnIndex;

            if (row_index < 0 || column_index < 0)
                return;

            DataGridView     grid     = sender as DataGridView;
            DataGridViewRow  row      = grid?.Rows[row_index];
            DataGridViewCell gridCell = row?.Cells[column_index];

            if (gridCell is DataGridViewLinkCell)
            {
                // Launch URLs if found in grid
                DataGridViewLinkCell cell = gridCell as DataGridViewLinkCell;
                string cmd = cell?.Value.ToString();
                if (!string.IsNullOrEmpty(cmd))
                    Utilities.ProcessStartURL(cmd);
            }
            else
            {
                GUIMod gui_mod = row?.Tag as GUIMod;
                if (gui_mod != null)
                {
                    switch (ModGrid.Columns[column_index].Name)
                    {
                        case "Installed":
                            gui_mod.SetInstallChecked(row, Installed);
                            // The above will call UpdateChangeSetAndConflicts, so we don't need to.
                            return;
                        case "AutoInstalled":
                            gui_mod.SetAutoInstallChecked(row, AutoInstalled);
                            if (OnRegistryChanged != null)
                            {
                                OnRegistryChanged();
                            }
                            break;
                        case "UpdateCol":
                            gui_mod.SetUpgradeChecked(row, UpdateCol);
                            break;
                        case "ReplaceCol":
                            gui_mod.SetReplaceChecked(row, ReplaceCol);
                            break;
                    }
                    await UpdateChangeSetAndConflicts(
                        Main.Instance.CurrentInstance,
                        RegistryManager.Instance(Main.Instance.CurrentInstance).registry
                    );
                }
            }
        }

        private void ModList_GotFocus(object sender, EventArgs e)
        {
            Util.Invoke(this, () =>
            {
                // Give the selected row the standard highlight color
                ModGrid.RowsDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                ModGrid.RowsDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            });
        }

        private void ModList_LostFocus(object sender, EventArgs e)
        {
            Util.Invoke(this, () =>
            {
                // Gray out the selected row so you can tell the mod list is not focused
                ModGrid.RowsDefaultCellStyle.SelectionBackColor = SystemColors.Control;
                ModGrid.RowsDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
            });
        }

        private void InstallAllCheckbox_CheckChanged(object sender, EventArgs e)
        {
            if (InstallAllCheckbox.Checked)
            {
                // Reset changeset
                ClearChangeSet();
            }
            else
            {
                // Uninstall all
                foreach (DataGridViewRow row in mainModList.full_list_of_mod_rows.Values)
                {
                    GUIMod mod = row.Tag as GUIMod;
                    if (mod.IsInstallChecked)
                    {
                        mod.SetInstallChecked(row, Installed, false);
                    }
                }
            }
        }

        public void ClearChangeSet()
        {
            foreach (DataGridViewRow row in mainModList.full_list_of_mod_rows.Values)
            {
                GUIMod mod = row.Tag as GUIMod;
                if (mod.IsInstallChecked != mod.IsInstalled)
                {
                    mod.SetInstallChecked(row, Installed, mod.IsInstalled);
                }
                mod.SetUpgradeChecked(row, UpdateCol, false);
                mod.SetReplaceChecked(row, ReplaceCol, false);
            }
        }

        /// <summary>
        /// Find a column of the grid that can contain the CurrentCell.
        /// Can't be hidden or an exception is thrown.
        /// Shouldn't be a checkbox because we don't want the space bar to toggle.
        /// </summary>
        /// <returns>
        /// Index of the column to use.
        /// </returns>
        private int SelectableColumnIndex()
        {
            // First try the currently active cell's column
            return ModGrid.CurrentCell?.ColumnIndex
                // If there's no currently active cell, use the first visible non-checkbox column
                ?? ModGrid.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => c is DataGridViewTextBoxColumn && c.Visible)?.Index
                // Otherwise use the Installed checkbox column since it can't be hidden
                ?? Installed.Index;
        }

        public void FocusMod(string key, bool exactMatch, bool showAsFirst = false)
        {
            DataGridViewRow current_row = ModGrid.CurrentRow;
            int currentIndex = current_row?.Index ?? 0;
            DataGridViewRow first_match = null;

            var does_name_begin_with_key = new Func<DataGridViewRow, bool>(row =>
            {
                GUIMod mod = row.Tag as GUIMod;
                bool row_match;
                if (exactMatch)
                    row_match = mod.Name == key || mod.Identifier == key;
                else
                    row_match = mod.Name.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                                mod.Abbrevation.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                                mod.Identifier.StartsWith(key, StringComparison.OrdinalIgnoreCase);

                if (row_match && first_match == null)
                {
                    // Remember the first match to allow cycling back to it if necessary.
                    first_match = row;
                }

                if (key.Length == 1 && row_match && row.Index <= currentIndex)
                {
                    // Keep going forward if it's a single key match and not ahead of the current row.
                    return false;
                }

                return row_match;
            });

            ModGrid.ClearSelection();
            var rows = ModGrid.Rows.Cast<DataGridViewRow>().Where(row => row.Visible);
            DataGridViewRow match = rows.FirstOrDefault(does_name_begin_with_key);
            if (match == null && first_match != null)
            {
                // If there were no matches after the first match, cycle over to the beginning.
                match = first_match;
            }

            if (match != null)
            {
                match.Selected = true;

                ModGrid.CurrentCell = match.Cells[SelectableColumnIndex()];
                if (showAsFirst)
                    ModGrid.FirstDisplayedScrollingRowIndex = match.Index;
            }
            else
            {
                Main.Instance.AddStatusMessage(Properties.Resources.MainNotFound);
            }
        }

        private void ModList_MouseDown(object sender, MouseEventArgs e)
        {
            var rowIndex = ModGrid.HitTest(e.X, e.Y).RowIndex;

            // Ignore header column to prevent errors.
            if (rowIndex != -1 && e.Button == MouseButtons.Right)
            {
                // Detect the clicked cell and select the row.
                ModGrid.ClearSelection();
                ModGrid.Rows[rowIndex].Selected = true;

                // Show the context menu.
                ModListContextMenuStrip.Show(ModGrid, new Point(e.X, e.Y));

                // Set the menu options.
                var guiMod = (GUIMod)ModGrid.Rows[rowIndex].Tag;

                downloadContentsToolStripMenuItem.Enabled = !guiMod.IsCached;
                purgeContentsToolStripMenuItem.Enabled = guiMod.IsCached;
                reinstallToolStripMenuItem.Enabled = guiMod.IsInstalled && !guiMod.IsAutodetected;
            }
        }

        private void reinstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GUIMod module = SelectedModule;
            if (module == null || !module.IsCKAN)
                return;

            YesNoDialog reinstallDialog = new YesNoDialog();
            string confirmationText = string.Format(Properties.Resources.MainReinstallConfirm, module.Name);
            if (reinstallDialog.ShowYesNoDialog(Main.Instance, confirmationText) == DialogResult.No)
                return;

            IRegistryQuerier registry = RegistryManager.Instance(Main.Instance.CurrentInstance).registry;

            // Build the list of changes, first the mod to remove:
            List<ModChange> toReinstall = new List<ModChange>()
            {
                new ModChange(module.ToModule(), GUIModChangeType.Remove, null)
            };
            // Then everything we need to re-install:
            var revdep = registry.FindReverseDependencies(new List<string>() { module.Identifier });
            var goners = revdep.Union(
                registry.FindRemovableAutoInstalled(
                    registry.InstalledModules.Where(im => !revdep.Contains(im.identifier))
                ).Select(im => im.Module.identifier)
            );
            foreach (string id in goners)
            {
                toReinstall.Add(new ModChange(
                    (mainModList.full_list_of_mod_rows[id]?.Tag as GUIMod).ToModule(),
                    GUIModChangeType.Install,
                    null
                ));
            }
            if (StartChangeSet != null)
            {
                StartChangeSet(toReinstall);
            }
        }

        private void purgeContentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Purge other versions as well since the user is likely to want that
            // and has no other way to achieve it
            var selected = SelectedModule;
            if (selected != null)
            {
                IRegistryQuerier registry = RegistryManager.Instance(Main.Instance.CurrentInstance).registry;
                var allAvail = registry.AvailableByIdentifier(selected.Identifier);
                foreach (CkanModule mod in allAvail)
                {
                    Main.Instance.Manager.Cache.Purge(mod);
                }
                selected.UpdateIsCached();
                Main.Instance.UpdateModContentsTree(selected.ToCkanModule(), true);
            }
        }

        private void downloadContentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.Instance.StartDownload(SelectedModule);
        }

        private void EditModSearch_ApplySearch(ModSearch search)
        {
            mainModList.SetSearch(search);
        }

        private void EditModSearch_SurrenderFocus()
        {
            Util.Invoke(this, () => ModGrid.Focus());
        }

        private void UpdateFilters()
        {
            Util.Invoke(this, _UpdateFilters);
        }

        private void _UpdateFilters()
        {
            if (ModGrid == null || mainModList?.full_list_of_mod_rows == null)
                return;

            // Each time a row in DataGridViewRow is changed, DataGridViewRow updates the view. Which is slow.
            // To make the filtering process faster, Copy the list of rows. Filter out the hidden and replace the
            // rows in DataGridView.

            var rows = new DataGridViewRow[mainModList.full_list_of_mod_rows.Count];
            mainModList.full_list_of_mod_rows.Values.CopyTo(rows, 0);
            // Try to remember the current scroll position and selected mod
            var scroll_col = Math.Max(0, ModGrid.FirstDisplayedScrollingColumnIndex);
            GUIMod selected_mod = null;
            if (ModGrid.CurrentRow != null)
            {
                selected_mod = (GUIMod) ModGrid.CurrentRow.Tag;
            }

            ModGrid.Rows.Clear();
            foreach (var row in rows)
            {
                var mod = ((GUIMod) row.Tag);
                row.Visible = mainModList.IsVisible(mod, Main.Instance.CurrentInstance.Name);
            }

            var sorted = this._SortRowsByColumn(rows.Where(row => row.Visible));

            ModGrid.Rows.AddRange(sorted.ToArray());

            // Find and select the previously selected row
            if (selected_mod != null)
            {
                var selected_row = ModGrid.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(row => selected_mod.Identifier.Equals(((GUIMod)row.Tag).Identifier));
                if (selected_row != null)
                {
                    ModGrid.CurrentCell = selected_row.Cells[scroll_col];
                }
            }
        }

        public async void UpdateModsList(Dictionary<string, bool> old_modules = null)
        {
            // Run the update in the background so the UI thread can appear alive
            // Await it so potential (fatal) errors are thrown, not swallowed.
            // Need to be on the GUI thread to get the translated strings
            Main.Instance.tabController.RenameTab("WaitTabPage", Properties.Resources.MainModListWaitTitle);
            await Task.Factory.StartNew(() =>
                _UpdateModsList(old_modules)
            );
        }

        private void _UpdateModsList(Dictionary<string, bool> old_modules = null)
        {
            log.Info("Updating the mod list");

            if (OpenProgressTab != null)
            {
                OpenProgressTab();
            }

            Main.Instance.Wait.AddLogMessage(Properties.Resources.MainModListLoadingRegistry);
            KspVersionCriteria versionCriteria = Main.Instance.CurrentInstance.VersionCriteria();
            IRegistryQuerier registry = RegistryManager.Instance(Main.Instance.CurrentInstance).registry;

            Main.Instance.Wait.AddLogMessage(Properties.Resources.MainModListLoadingInstalled);
            var gui_mods = new HashSet<GUIMod>();
            gui_mods.UnionWith(
                registry.InstalledModules
                    .Where(instMod => !instMod.Module.IsDLC)
                    .Select(instMod => new GUIMod(instMod, registry, versionCriteria))
            );
            Main.Instance.Wait.AddLogMessage(Properties.Resources.MainModListLoadingAvailable);
            gui_mods.UnionWith(
                registry.CompatibleModules(versionCriteria)
                    .Where(m => !m.IsDLC)
                    .Select(m => new GUIMod(m, registry, versionCriteria))
            );
            Main.Instance.Wait.AddLogMessage(Properties.Resources.MainModListLoadingIncompatible);
            gui_mods.UnionWith(
                registry.IncompatibleModules(versionCriteria)
                    .Where(m => !m.IsDLC)
                    .Select(m => new GUIMod(m, registry, versionCriteria, true))
            );

            Main.Instance.Wait.AddLogMessage(Properties.Resources.MainModListPreservingNew);
            var toNotify = new HashSet<GUIMod>();
            if (old_modules != null)
            {
                foreach (GUIMod gm in gui_mods)
                {
                    if (old_modules.TryGetValue(gm.Identifier, out bool oldIncompat))
                    {
                        // Found it; check if newly compatible
                        if (!gm.IsIncompatible && oldIncompat)
                        {
                            gm.IsNew = true;
                            toNotify.Add(gm);
                        }
                    }
                    else
                    {
                        // Newly indexed, show regardless of compatibility
                        gm.IsNew = true;
                    }
                }
            }
            else
            {
                // Copy the new mod flag from the old list.
                var old_new_mods = new HashSet<GUIMod>(
                    mainModList.Modules.Where(m => m.IsNew));
                foreach (var gui_mod in gui_mods.Where(m => old_new_mods.Contains(m)))
                {
                    gui_mod.IsNew = true;
                }
            }
            if (LabelsAfterUpdate != null)
            {
                LabelsAfterUpdate(toNotify);
            }

            Main.Instance.Wait.AddLogMessage(Properties.Resources.MainModListPopulatingList);
            // Update our mod listing
            mainModList.ConstructModList(gui_mods.ToList(), Main.Instance.CurrentInstance.Name, ChangeSet, Main.Instance.configuration.HideEpochs, Main.Instance.configuration.HideV);
            mainModList.Modules = new ReadOnlyCollection<GUIMod>(
                mainModList.full_list_of_mod_rows.Values.Select(row => row.Tag as GUIMod).ToList());

            UpdateChangeSetAndConflicts(Main.Instance.CurrentInstance, registry);

            Main.Instance.Wait.AddLogMessage(Properties.Resources.MainModListUpdatingFilters);

            var has_any_updates      = gui_mods.Any(mod => mod.HasUpdate);
            var has_any_installed    = gui_mods.Any(mod => mod.IsInstalled);
            var has_any_replacements = gui_mods.Any(mod => mod.IsInstalled && mod.HasReplacement);

            Util.Invoke(menuStrip2, () =>
            {
                FilterCompatibleButton.Text = String.Format(Properties.Resources.MainModListCompatible,
                    mainModList.CountModsByFilter(GUIModFilter.Compatible));
                FilterInstalledButton.Text = String.Format(Properties.Resources.MainModListInstalled,
                    mainModList.CountModsByFilter(GUIModFilter.Installed));
                FilterInstalledUpdateButton.Text = String.Format(Properties.Resources.MainModListUpgradeable,
                    mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));
                FilterReplaceableButton.Text = String.Format(Properties.Resources.MainModListReplaceable,
                    mainModList.CountModsByFilter(GUIModFilter.Replaceable));
                FilterCachedButton.Text = String.Format(Properties.Resources.MainModListCached,
                    mainModList.CountModsByFilter(GUIModFilter.Cached));
                FilterUncachedButton.Text = String.Format(Properties.Resources.MainModListUncached,
                    mainModList.CountModsByFilter(GUIModFilter.Uncached));
                FilterNewButton.Text = String.Format(Properties.Resources.MainModListNewlyCompatible,
                    mainModList.CountModsByFilter(GUIModFilter.NewInRepository));
                FilterNotInstalledButton.Text = String.Format(Properties.Resources.MainModListNotInstalled,
                    mainModList.CountModsByFilter(GUIModFilter.NotInstalled));
                FilterIncompatibleButton.Text = String.Format(Properties.Resources.MainModListIncompatible,
                    mainModList.CountModsByFilter(GUIModFilter.Incompatible));
                FilterAllButton.Text = String.Format(Properties.Resources.MainModListAll,
                    mainModList.CountModsByFilter(GUIModFilter.All));

                UpdateAllToolButton.Enabled = has_any_updates;
            });

            (registry as Registry)?.BuildTagIndex(mainModList.ModuleTags);

            UpdateFilters();

            // Hide update and replacement columns if not needed.
            // Write it to the configuration, else they are hidden again after a filter change.
            // After the update / replacement, they are hidden again.
            Util.Invoke(ModGrid, () =>
            {
                ModGrid.Columns["UpdateCol"].Visible     = has_any_updates;
                ModGrid.Columns["AutoInstalled"].Visible = has_any_installed && !Main.Instance.configuration.HiddenColumnNames.Contains("AutoInstalled");
                ModGrid.Columns["ReplaceCol"].Visible    = has_any_replacements;
            });

            Main.Instance.Wait.AddLogMessage(Properties.Resources.MainModListUpdatingTray);

            if (CloseProgressTab != null)
            {
                CloseProgressTab();
            }
            Util.Invoke(this, () => ModGrid.Focus());
        }

        public void MarkModForInstall(string identifier, bool uncheck = false)
        {
            Util.Invoke(this, () => _MarkModForInstall(identifier, uncheck));
        }

        private void _MarkModForInstall(string identifier, bool uninstall)
        {
            DataGridViewRow row = mainModList?.full_list_of_mod_rows?[identifier];
            var mod = (GUIMod)row?.Tag;
            if (mod?.Identifier == identifier)
            {
                mod.SetInstallChecked(row, Installed, !uninstall);
            }
        }

        private void ModList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            ModList_CellContentClick(sender, null);
        }

        private IEnumerable<DataGridViewRow> _SortRowsByColumn(IEnumerable<DataGridViewRow> rows)
        {
            switch (Main.Instance.configuration.SortByColumnIndex)
            {
                // XXX: There should be a better way to identify checkbox columns than hardcoding their indices here
                case 0: case 1:
                case 2: case 3: return Sort(rows, CheckboxSorter);
                case 9:         return Sort(rows, DownloadSizeSorter);
                case 10:        return Sort(rows, InstallDateSorter);
                case 11:        return Sort(rows, r => (r.Tag as GUIMod)?.DownloadCount ?? 0);
            }
            return Sort(rows, DefaultSorter);
        }

        private IEnumerable<DataGridViewRow> Sort<T>(IEnumerable<DataGridViewRow> rows, Func<DataGridViewRow, T> sortFunction)
        {
            var get_row_mod_name = new Func<DataGridViewRow, string>(row => ((GUIMod)row.Tag).Name);
            DataGridViewColumnHeaderCell header =
                this.ModGrid.Columns[Main.Instance.configuration.SortByColumnIndex].HeaderCell;

            // The columns will be sorted by mod name in addition to whatever the current sorting column is
            if (Main.Instance.configuration.SortDescending)
            {
                header.SortGlyphDirection = SortOrder.Descending;
                return rows.OrderByDescending(sortFunction).ThenBy(get_row_mod_name);
            }

            header.SortGlyphDirection = SortOrder.Ascending;
            return rows.OrderBy(sortFunction).ThenBy(get_row_mod_name);
        }

        /// <summary>
        /// Transforms a DataGridViewRow's into a generic value suitable for sorting.
        /// Uses this.m_Configuration.SortByColumnIndex to determine which
        /// field to sort on.
        /// </summary>
        private string DefaultSorter(DataGridViewRow row)
        {
            // changed so that it never returns null
            var cellVal = row.Cells[Main.Instance.configuration.SortByColumnIndex].Value as string;
            return string.IsNullOrWhiteSpace(cellVal) ? string.Empty : cellVal;
        }

        /// <summary>
        /// Transforms a DataGridViewRow's checkbox status into a value suitable for sorting.
        /// Uses this.m_Configuration.SortByColumnIndex to determine which
        /// field to sort on.
        /// </summary>
        private string CheckboxSorter(DataGridViewRow row)
        {
            var cell = row.Cells[Main.Instance.configuration.SortByColumnIndex];
            if (cell.ValueType == typeof(bool))
            {
                return (bool)cell.Value ? "a" : "c";
            }
            else
            {
                // If it's a "-" cell, let it be ordered last
                // Otherwise put it after the checked boxes
                return (string)cell.Value == "-" ? "d" : "b";
            }
        }

        /// <summary>
        /// Transforms a DataGridViewRow into a long representing the download size,
        /// suitable for sorting.
        /// </summary>
        private long DownloadSizeSorter(DataGridViewRow row)
        {
            return (row.Tag as GUIMod)?.ToCkanModule()?.download_size ?? 0;
        }

        /// <summary>
        /// Transforms a DataGridViewRow into a long representing the install date,
        /// suitable for sorting.
        /// The grid's default on first click is ascending, and sorting uninstalled mods to
        /// the top is kind of useless, so we'll make this negative so ascending is useful.
        /// </summary>
        private long InstallDateSorter(DataGridViewRow row)
        {
            return -(row.Tag as GUIMod)?.InstallDate?.Ticks ?? 0;
        }

        public void ResetFilterAndSelectModOnList(string key)
        {
            EditModSearch.Clear();
            FocusMod(key, true);
        }

        public GUIMod SelectedModule
        {
            get
            {
                return ModGrid.SelectedRows.Count == 0
                    ? null
                    : ModGrid.SelectedRows[0]?.Tag as GUIMod;
            }
        }

        #region Navigation History

        private void NavInit()
        {
            navHistory.OnHistoryChange += NavOnHistoryChange;
            navHistory.IsReadOnly = false;
            var currentMod = SelectedModule;
            if (currentMod != null)
                navHistory.AddToHistory(currentMod);
        }

        private void NavUpdateUI()
        {
            NavBackwardToolButton.Enabled = navHistory.CanNavigateBackward;
            NavForwardToolButton.Enabled = navHistory.CanNavigateForward;
        }

        private void NavSelectMod(GUIMod module)
        {
            navHistory.AddToHistory(module);
        }

        private void NavGoBackward()
        {
            if (navHistory.CanNavigateBackward)
                NavGoToMod(navHistory.NavigateBackward());
        }

        private void NavGoForward()
        {
            if (navHistory.CanNavigateForward)
                NavGoToMod(navHistory.NavigateForward());
        }

        private void NavGoToMod(GUIMod module)
        {
            // Focussing on a mod also causes navigation, but we don't want
            // this to affect the history. so we switch to read-only mode.
            navHistory.IsReadOnly = true;
            FocusMod(module.Name, true);
            navHistory.IsReadOnly = false;
        }

        private void NavOnHistoryChange()
        {
            NavUpdateUI();
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.F:
                    ActiveControl = EditModSearch;
                    return true;

                case Keys.Control | Keys.Shift | Keys.F:
                    EditModSearch.ExpandCollapse();
                    ActiveControl = EditModSearch;
                    return true;

                case Keys.Control | Keys.S:
                    if (ChangeSet != null && ChangeSet.Any())
                        ApplyToolButton_Click(null, null);

                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public bool AllowClose()
        {
            if (Conflicts != null)
            {
                if (Conflicts.Any())
                {
                    // Ask if they want to resolve conflicts
                    string confDescrip = Conflicts
                        .Select(kvp => kvp.Value)
                        .Aggregate((a, b) => $"{a}, {b}");
                    if (!Main.Instance.YesNoDialog(string.Format(Properties.Resources.MainQuitWithConflicts, confDescrip),
                        Properties.Resources.MainQuit,
                        Properties.Resources.MainGoBack))
                    {
                        return false;
                    }
                }
                else
                {
                    // The Conflicts dictionary is empty even when there are unmet dependencies.
                    if (!Main.Instance.YesNoDialog(Properties.Resources.MainQuitWithUnmetDeps,
                        Properties.Resources.MainQuit,
                        Properties.Resources.MainGoBack))
                    {
                        return false;
                    }
                }
            }
            else if (ChangeSet?.Any() ?? false)
            {
                // Ask if they want to discard the change set
                string changeDescrip = ChangeSet
                    .GroupBy(ch => ch.ChangeType, ch => ch.Mod.name)
                    .Select(grp => $"{grp.Key}: "
                        + grp.Aggregate((a, b) => $"{a}, {b}"))
                    .Aggregate((a, b) => $"{a}\r\n{b}");
                if (!Main.Instance.YesNoDialog(string.Format(Properties.Resources.MainQuitWIthUnappliedChanges, changeDescrip),
                    Properties.Resources.MainQuit,
                    Properties.Resources.MainGoBack))
                {
                    return false;
                }
            }
            return true;
        }

        public void InstanceUpdated(KSP ksp)
        {
            ChangeSet = null;
            Conflicts = null;
        }

        public async Task UpdateChangeSetAndConflicts(KSP ksp, IRegistryQuerier registry)
        {
            IEnumerable<ModChange> full_change_set = null;
            Dictionary<GUIMod, string> new_conflicts = null;

            bool too_many_provides_thrown = false;
            var user_change_set = mainModList.ComputeUserChangeSet(registry);
            try
            {
                var module_installer = ModuleInstaller.GetInstance(ksp, Main.Instance.Manager.Cache, Main.Instance.currentUser);
                full_change_set = mainModList.ComputeChangeSetFromModList(registry, user_change_set, module_installer, ksp.VersionCriteria());
            }
            catch (InconsistentKraken k)
            {
                // Need to be recomputed due to ComputeChangeSetFromModList possibly changing it with too many provides handling.
                Main.Instance.AddStatusMessage(k.ShortDescription);
                user_change_set = mainModList.ComputeUserChangeSet(registry);
                new_conflicts = ModList.ComputeConflictsFromModList(registry, user_change_set, ksp.VersionCriteria());
                full_change_set = null;
            }
            catch (TooManyModsProvideKraken)
            {
                // Can be thrown by ComputeChangeSetFromModList if the user cancels out of it.
                // We can just rerun it as the ModInfo has been removed.
                too_many_provides_thrown = true;
            }
            catch (DependencyNotSatisfiedKraken k)
            {
                Main.Instance.currentUser.RaiseError(
                    Properties.Resources.MainDepNotSatisfied,
                    k.parent,
                    k.module
                );

                // Uncheck the box
                MarkModForInstall(k.parent.identifier, true);
            }

            if (too_many_provides_thrown)
            {
                await UpdateChangeSetAndConflicts(ksp, registry);
                new_conflicts = Conflicts;
                full_change_set = ChangeSet;
            }

            Conflicts = new_conflicts;
            ChangeSet = full_change_set;
        }

    }
}
