using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CKAN.Versioning;
using log4net;

namespace CKAN
{
    public enum GUIModFilter
    {
        Compatible               = 0,
        Installed                = 1,
        InstalledUpdateAvailable = 2,
        NewInRepository          = 3,
        NotInstalled             = 4,
        Incompatible             = 5,
        All                      = 6,
        Cached                   = 7,
        Replaceable              = 8,
        Uncached                 = 9,
        CustomLabel              = 10,
        Tag                      = 11,
    }

    public partial class Main
    {
        private IEnumerable<DataGridViewRow> _SortRowsByColumn(IEnumerable<DataGridViewRow> rows)
        {
            switch (this.configuration.SortByColumnIndex)
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
                this.ModList.Columns[this.configuration.SortByColumnIndex].HeaderCell;

            // The columns will be sorted by mod name in addition to whatever the current sorting column is
            if (this.configuration.SortDescending)
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
            var cellVal = row.Cells[configuration.SortByColumnIndex].Value as string;
            return string.IsNullOrWhiteSpace(cellVal) ? string.Empty : cellVal;
        }

        /// <summary>
        /// Transforms a DataGridViewRow's checkbox status into a value suitable for sorting.
        /// Uses this.m_Configuration.SortByColumnIndex to determine which
        /// field to sort on.
        /// </summary>
        private string CheckboxSorter(DataGridViewRow row)
        {
            var cell = row.Cells[this.configuration.SortByColumnIndex];
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

        private void UpdateFilters(Main control)
        {
            Util.Invoke(control, _UpdateFilters);
        }

        private void _UpdateFilters()
        {
            if (ModList == null || mainModList?.full_list_of_mod_rows == null)
                return;

            // Each time a row in DataGridViewRow is changed, DataGridViewRow updates the view. Which is slow.
            // To make the filtering process faster, Copy the list of rows. Filter out the hidden and replace the
            // rows in DataGridView.

            var rows = new DataGridViewRow[mainModList.full_list_of_mod_rows.Count];
            mainModList.full_list_of_mod_rows.Values.CopyTo(rows, 0);
            // Try to remember the current scroll position and selected mod
            var scroll_col = Math.Max(0, ModList.FirstDisplayedScrollingColumnIndex);
            GUIMod selected_mod = null;
            if (ModList.CurrentRow != null)
            {
                selected_mod = (GUIMod) ModList.CurrentRow.Tag;
            }

            ModList.Rows.Clear();
            foreach (var row in rows)
            {
                var mod = ((GUIMod) row.Tag);
                row.Visible = mainModList.IsVisible(mod, CurrentInstance.Name);
            }

            var sorted = this._SortRowsByColumn(rows.Where(row => row.Visible));

            ModList.Rows.AddRange(sorted.ToArray());

            // Find and select the previously selected row
            if (selected_mod != null)
            {
                var selected_row = ModList.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(row => selected_mod.Identifier.Equals(((GUIMod)row.Tag).Identifier));
                if (selected_row != null)
                {
                    ModList.CurrentCell = selected_row.Cells[scroll_col];
                }
            }
        }

        public async void UpdateModsList(IEnumerable<ModChange> mc = null, Dictionary<string, bool> old_modules = null)
        {
            // Run the update in the background so the UI thread can appear alive
            // Await it so potential (fatal) errors are thrown, not swallowed.
            await Task.Factory.StartNew(() =>
                _UpdateModsList(mc ?? new List<ModChange>(), old_modules)
            );
        }

        private void _UpdateModsList(IEnumerable<ModChange> mc, Dictionary<string, bool> old_modules = null)
        {
            log.Info("Updating the mod list");

            ResetProgress();
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainModListWaitTitle);
            ShowWaitDialog(false);
            tabController.SetTabLock(true);
            Util.Invoke(this, SwitchEnabledState);
            ClearLog();

            AddLogMessage(Properties.Resources.MainModListLoadingRegistry);
            KspVersionCriteria versionCriteria = CurrentInstance.VersionCriteria();
            IRegistryQuerier registry = RegistryManager.Instance(CurrentInstance).registry;

            AddLogMessage(Properties.Resources.MainModListLoadingInstalled);
            var gui_mods = new HashSet<GUIMod>();
            gui_mods.UnionWith(
                registry.InstalledModules
                    .Select(instMod => new GUIMod(instMod, registry, versionCriteria))
            );
            AddLogMessage(Properties.Resources.MainModListLoadingAvailable);
            gui_mods.UnionWith(
                registry.CompatibleModules(versionCriteria)
                    .Select(m => new GUIMod(m, registry, versionCriteria))
            );
            AddLogMessage(Properties.Resources.MainModListLoadingIncompatible);
            gui_mods.UnionWith(
                registry.IncompatibleModules(versionCriteria)
                    .Select(m => new GUIMod(m, registry, versionCriteria, true))
            );

            AddLogMessage(Properties.Resources.MainModListPreservingNew);
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
            LabelsAfterUpdate(toNotify);

            AddLogMessage(Properties.Resources.MainModListPopulatingList);
            // Update our mod listing
            mainModList.ConstructModList(gui_mods.ToList(), CurrentInstance.Name, mc, configuration.HideEpochs, configuration.HideV);
            mainModList.Modules = new ReadOnlyCollection<GUIMod>(
                mainModList.full_list_of_mod_rows.Values.Select(row => row.Tag as GUIMod).ToList());

            UpdateChangeSetAndConflicts(registry);

            AddLogMessage(Properties.Resources.MainModListUpdatingFilters);

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
            
            UpdateFilters(this);

            // Hide update and replacement columns if not needed.
            // Write it to the configuration, else they are hidden again after a filter change.
            // After the update / replacement, they are hidden again.
            Util.Invoke(ModList, () =>
            {
                ModList.Columns["UpdateCol"].Visible     = has_any_updates;
                ModList.Columns["AutoInstalled"].Visible = has_any_installed && !configuration.HiddenColumnNames.Contains("AutoInstalled");
                ModList.Columns["ReplaceCol"].Visible    = has_any_replacements;
            });

            AddLogMessage(Properties.Resources.MainModListUpdatingTray);
            UpdateTrayInfo();

            HideWaitDialog(true);
            tabController.HideTab("WaitTabPage");
            tabController.SetTabLock(false);
            Util.Invoke(this, SwitchEnabledState);
            Util.Invoke(this, () => Main.Instance.ModList.Focus());
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

        public void MarkModForUpdate(string identifier, bool value)
        {
            Util.Invoke(this, () => _MarkModForUpdate(identifier, value));
        }

        public void _MarkModForUpdate(string identifier, bool value)
        {
            DataGridViewRow row = mainModList.full_list_of_mod_rows[identifier];
            var mod = (GUIMod)row.Tag;
            mod.SetUpgradeChecked(row, UpdateCol, value);
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Skip if already disposed (i.e. after the form has been closed).
            // Needed for TransparentTextBoxes
            if (ModInfo.IsDisposed)
            {
                return;
            }

            var module = GetSelectedModule();

            ActiveModInfo = module;
            if (module == null)
                return;

            NavSelectMod(module);
        }

        /// <summary>
        /// Called when there's a click on the modlist header row.
        /// Handles sorting and the header right click context menu.
        /// </summary>
        private void ModList_HeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Left click -> sort by new column / change sorting direction.
            if (e.Button == MouseButtons.Left)
            {
                var new_sort_column = ModList.Columns [e.ColumnIndex];
                var current_sort_column = ModList.Columns [configuration.SortByColumnIndex];

                // Reverse the sort order if the current sorting column is clicked again.
                configuration.SortDescending = new_sort_column == current_sort_column && !configuration.SortDescending;

                // Reset the glyph.
                current_sort_column.HeaderCell.SortGlyphDirection = SortOrder.None;
                configuration.SortByColumnIndex = new_sort_column.Index;
                UpdateFilters(this);
            }
            // Right click -> Bring up context menu to change visibility of columns.
            else if (e.Button == MouseButtons.Right)
            {
                // Start from scrap: clear the entire item list, then add all options again.
                ModListHeaderContextMenuStrip.Items.Clear();

                // Add columns
                ModListHeaderContextMenuStrip.Items.AddRange(
                    ModList.Columns.Cast<DataGridViewColumn>()
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
                configuration.SetColumnVisibility(col.Name, !clickedItem.Checked);
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
                UpdateFilters(this);
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
                    ModList.CurrentCell = ModList.Rows[0].Cells[SelectableColumnIndex()];
                    e.Handled = true;
                    break;

                case Keys.End:
                    // Last row.
                    ModList.CurrentCell = ModList.Rows[ModList.Rows.Count - 1].Cells[SelectableColumnIndex()];
                    e.Handled = true;
                    break;

                case Keys.Space:
                    // If they've focused one of the checkbox columns, don't intercept
                    if (ModList.CurrentCell.ColumnIndex > 3)
                    {
                        DataGridViewRow row = ModList.CurrentRow;
                        // Toggle Update column if enabled, otherwise Install
                        for (int colIndex = 2; colIndex >= 0; --colIndex)
                        {
                            if (row?.Cells[colIndex] is DataGridViewCheckBoxCell)
                            {
                                // Need to change the state here, because the user hasn't clicked on a checkbox
                                row.Cells[colIndex].Value = !(bool)row.Cells[colIndex].Value;
                                ModList.CommitEdit(DataGridViewDataErrorContexts.Commit);
                                e.Handled = true;
                                break;
                            }
                        }
                    }
                    break;
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
            return ModList.CurrentCell?.ColumnIndex
                // If there's no currently active cell, use the first visible non-checkbox column
                ?? ModList.Columns.Cast<DataGridViewColumn>()
                    .FirstOrDefault(c => c is DataGridViewTextBoxColumn && c.Visible)?.Index
                // Otherwise use the Installed checkbox column since it can't be hidden
                ?? Installed.Index;
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
            ModList.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void ModList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = ModList.Rows[e.RowIndex];
            if (!(row.Cells[0] is DataGridViewCheckBoxCell))
                return;

            // Need to change the state here, because the user hasn't clicked on a checkbox.
            row.Cells[0].Value = !(bool)row.Cells[0].Value;
            ModList.CommitEdit(DataGridViewDataErrorContexts.Commit);
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
                    switch (ModList.Columns[column_index].Name)
                    {
                        case "Installed":
                            gui_mod.SetInstallChecked(row, Installed);
                            if (gui_mod.IsInstallChecked)
                                last_mod_to_have_install_toggled.Push(gui_mod);
                            // The above will call UpdateChangeSetAndConflicts, so we don't need to.
                            return;
                        case "AutoInstalled":
                            gui_mod.SetAutoInstallChecked(row, AutoInstalled);
                            needRegistrySave = true;
                            break;
                        case "UpdateCol":
                            gui_mod.SetUpgradeChecked(row, UpdateCol);
                            break;
                        case "ReplaceCol":
                            gui_mod.SetReplaceChecked(row, ReplaceCol);
                            break;
                    }
                    await UpdateChangeSetAndConflicts(
                        RegistryManager.Instance(CurrentInstance).registry
                    );
                }
            }
        }

        private void ModList_GotFocus(object sender, EventArgs e)
        {
            Util.Invoke(this, () =>
            {
                // Give the selected row the standard highlight color
                ModList.RowsDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                ModList.RowsDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            });
        }

        private void ModList_LostFocus(object sender, EventArgs e)
        {
            Util.Invoke(this, () =>
            {
                // Gray out the selected row so you can tell the mod list is not focused
                ModList.RowsDefaultCellStyle.SelectionBackColor = SystemColors.Control;
                ModList.RowsDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
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
    }

    /// <summary>
    /// The main list of mods. Currently used to work around mono issues.
    /// </summary>
    public class MainModListGUI : DataGridView
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                // Hacky workaround for https://bugzilla.xamarin.com/show_bug.cgi?id=24372
                if (Platform.IsMono && !Platform.IsMonoFourOrLater)
                {
                    var first_row_index = typeof (MainModListGUI).BaseType
                        .GetField("first_row_index", BindingFlags.NonPublic | BindingFlags.Instance);
                    var value = (int) first_row_index.GetValue(this);
                    if (value < 0 || value >= Rows.Count)
                    {
                        first_row_index.SetValue(this, 0);
                    }
                }
            }
            catch
            {
                // Never throw exceptions in OnPaint, or WinForms might decide to replace our control with a big red X
                // https://blogs.msdn.microsoft.com/shawnhar/2010/11/22/winforms-and-the-big-red-x-of-doom/
            }
            base.OnPaint(e);
        }

        //Hacky workaround for https://bugzilla.xamarin.com/show_bug.cgi?id=24372
        protected override void SetSelectedRowCore(int rowIndex, bool selected)
        {
            if (rowIndex < 0 || rowIndex >= Rows.Count)
                return;
            base.SetSelectedRowCore(rowIndex, selected);
        }

        //ImageList for Update/Changes Column
        public System.Windows.Forms.ImageList ModChangesImageList { get; set; }

    }

    public class MainModList
    {
        //identifier, row
        internal Dictionary<string, DataGridViewRow> full_list_of_mod_rows;

        public MainModList(ModFiltersUpdatedEvent onModFiltersUpdated, HandleTooManyProvides too_many_provides,
            IUser user = null)
        {
            this.too_many_provides = too_many_provides;
            this.user = user ?? new NullUser();
            Modules = new ReadOnlyCollection<GUIMod>(new List<GUIMod>());
            ModFiltersUpdated += onModFiltersUpdated ?? (source => { });
            ModFiltersUpdated(this);
        }

        public delegate void ModFiltersUpdatedEvent(MainModList source);

        //TODO Move to relationship resolver and have it use this.
        public delegate Task<CkanModule> HandleTooManyProvides(TooManyModsProvideKraken kraken);

        public event ModFiltersUpdatedEvent ModFiltersUpdated;
        public ReadOnlyCollection<GUIMod> Modules { get; set; }

        public GUIModFilter ModFilter
        {
            get { return _modFilter; }
            set
            {
                var old = _modFilter;
                _modFilter = value;
                if (!old.Equals(value)) ModFiltersUpdated(this);
            }
        }

        public readonly ModuleLabelList ModuleLabels = ModuleLabelList.Load(ModuleLabelList.DefaultPath)
            ?? ModuleLabelList.GetDefaultLabels();
            
        public readonly ModuleTagList ModuleTags = ModuleTagList.Load(ModuleTagList.DefaultPath)
            ?? new ModuleTagList();

        private ModuleTag _tagFilter;
        public ModuleTag TagFilter
        {
            get { return _tagFilter; }
            set
            {
                var old = _tagFilter;
                _tagFilter = value;
                if (!old?.Equals(value) ?? !value?.Equals(old) ?? false)
                {
                    ModFiltersUpdated(this);
                }
            }
        }

        private ModuleLabel _customLabelFilter;
        public ModuleLabel CustomLabelFilter
        {
            get { return _customLabelFilter; }
            set
            {
                var old = _customLabelFilter;
                _customLabelFilter = value;
                if (!old?.Equals(value) ?? !value?.Equals(old) ?? false)
                {
                    ModFiltersUpdated(this);
                }
            }
        }

        public string ModNameFilter
        {
            get { return _modNameFilter; }
            set
            {
                var old = _modNameFilter;
                _modNameFilter = value;
                if (!old.Equals(value)) ModFiltersUpdated(this);
            }
        }

        public string ModAuthorFilter
        {
            get { return _modAuthorFilter; }
            set
            {
                var old = _modAuthorFilter;
                _modAuthorFilter = value;
                if (!old.Equals(value)) ModFiltersUpdated(this);
            }
        }

        public string ModDescriptionFilter
        {
            get { return _modDescriptionFilter; }
            set
            {
                var old = _modDescriptionFilter;
                _modDescriptionFilter = value;
                if (!old.Equals(value)) ModFiltersUpdated(this);
            }
        }

        private GUIModFilter _modFilter = GUIModFilter.Compatible;
        private string _modNameFilter = String.Empty;
        private string _modAuthorFilter = String.Empty;
        private string _modDescriptionFilter = String.Empty;
        private IUser user;

        private readonly HandleTooManyProvides too_many_provides;

        /// <summary>
        /// This function returns a changeset based on the selections of the user.
        /// Currently returns null if a conflict is detected.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="changeSet"></param>
        /// <param name="installer">A module installer for the current KSP install</param>
        /// <param name="version">The version of the current KSP install</param>
        public IEnumerable<ModChange> ComputeChangeSetFromModList(
            IRegistryQuerier registry, HashSet<ModChange> changeSet, ModuleInstaller installer,
            KspVersionCriteria version)
        {
            var modules_to_install = new HashSet<CkanModule>();
            var modules_to_remove = new HashSet<CkanModule>();

            foreach (var change in changeSet)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.None:
                        break;
                    case GUIModChangeType.Update:
                    case GUIModChangeType.Install:
                        modules_to_install.Add(change.Mod);
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Mod);
                        break;
                    case GUIModChangeType.Replace:
                        ModuleReplacement repl = registry.GetReplacement(change.Mod, version);
                        if (repl != null)
                        {
                            modules_to_remove.Add(repl.ToReplace);
                            modules_to_install.Add(repl.ReplaceWith);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var installed_modules =
                registry.InstalledModules.Select(imod => imod.Module).ToDictionary(mod => mod.identifier, mod => mod);
            foreach (var dependency in registry.FindReverseDependencies(
                modules_to_remove
                    .Select(mod => mod.identifier)
                    .Except(modules_to_install.Select(m => m.identifier))
            ))
            {
                //TODO This would be a good place to have an event that alters the row's graphics to show it will be removed
                CkanModule depMod;
                if (installed_modules.TryGetValue(dependency, out depMod))
                {
                    CkanModule module_by_version = registry.GetModuleByVersion(depMod.identifier,
                    depMod.version)
                        ?? registry.InstalledModule(dependency).Module;
                    changeSet.Add(new ModChange(module_by_version, GUIModChangeType.Remove, null));
                    modules_to_remove.Add(module_by_version);
                }
            }
            foreach (var im in registry.FindRemovableAutoInstalled(
                registry.InstalledModules.Where(im => !modules_to_remove.Any(m => m.identifier == im.identifier) || modules_to_install.Any(m => m.identifier == im.identifier))
            ))
            {
                changeSet.Add(new ModChange(im.Module, GUIModChangeType.Remove, new SelectionReason.NoLongerUsed()));
                modules_to_remove.Add(im.Module);
            }

            // Get as many dependencies as we can, but leave decisions and prompts for installation time
            RelationshipResolverOptions opts = RelationshipResolver.DependsOnlyOpts();
            opts.without_toomanyprovides_kraken = true;
            opts.without_enforce_consistency    = true;

            var resolver = new RelationshipResolver(
                modules_to_install,
                modules_to_remove,
                opts, registry, version);
            changeSet.UnionWith(
                resolver.ModList()
                    .Select(m => new ModChange(m, GUIModChangeType.Install, resolver.ReasonFor(m))));

            return changeSet;
        }

        public bool IsVisible(GUIMod mod, string instanceName)
        {
            return IsNameInNameFilter(mod)
                && IsAuthorInAuthorFilter(mod)
                && IsAbstractInDescriptionFilter(mod)
                && IsModInFilter(ModFilter, TagFilter, CustomLabelFilter, mod)
                && !HiddenByTagsOrLabels(ModFilter, TagFilter, CustomLabelFilter, mod, instanceName);
        }

        private bool HiddenByTagsOrLabels(GUIModFilter filter, ModuleTag tag, ModuleLabel label, GUIMod m, string instanceName)
        {
            if (filter != GUIModFilter.CustomLabel)
            {
                // "Hide" labels apply to all non-custom filters
                if (ModuleLabels?.LabelsFor(instanceName)
                    .Where(l => l != label && l.Hide)
                    .Any(l => l.ModuleIdentifiers.Contains(m.Identifier))
                    ?? false)
                {
                    return true;
                }
                if (ModuleTags?.Tags?.Values
                    .Where(t => t != tag && t.Visible == false)
                    .Any(t => t.ModuleIdentifiers.Contains(m.Identifier))
                    ?? false)
                {
                    return true;
                }
            }
            return false;
        }

        public int CountModsByFilter(GUIModFilter filter)
        {
            if (filter == GUIModFilter.All)
            {
                // Don't check each one
                return Modules.Count;
            }
            // Tags and Labels are not counted here
            return Modules.Count(m => IsModInFilter(filter, null, null, m));
        }

        /// <summary>
        /// Constructs the mod list suitable for display to the user.
        /// Manipulates <c>full_list_of_mod_rows</c>.
        /// </summary>
        /// <param name="modules">A list of modules that may require updating</param>
        /// <param name="mc">Changes the user has made</param>
        /// <param name="hideEpochs">If true, remove epochs from the displayed versions</param>
        /// <param name="hideV">If true, strip 'v' prefix from versions</param>
        /// <returns>The mod list</returns>
        public IEnumerable<DataGridViewRow> ConstructModList(
            IEnumerable<GUIMod> modules, string instanceName, IEnumerable<ModChange> mc = null,
            bool hideEpochs = false, bool hideV = false)
        {
            List<ModChange> changes = mc?.ToList();
            full_list_of_mod_rows = modules.ToDictionary(
                gm => gm.Identifier,
                gm => MakeRow(gm, changes, instanceName, hideEpochs, hideV)
            );
            return full_list_of_mod_rows.Values;
        }

        private DataGridViewRow MakeRow(GUIMod mod, List<ModChange> changes, string instanceName, bool hideEpochs = false, bool hideV = false)
        {
            DataGridViewRow item = new DataGridViewRow() {Tag = mod};

            Color? myColor = ModuleLabels.LabelsFor(instanceName)
                .FirstOrDefault(l => l.ModuleIdentifiers.Contains(mod.Identifier))
                ?.Color;
            if (myColor.HasValue)
            {
                item.DefaultCellStyle.BackColor = myColor.Value;
            }

            ModChange myChange = changes?.FindLast((ModChange ch) => ch.Mod.Equals(mod));

            var selecting = mod.IsInstallable()
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = myChange == null ? mod.IsInstalled
                        : myChange.ChangeType == GUIModChangeType.Install ? true
                        : myChange.ChangeType == GUIModChangeType.Remove  ? false
                        : mod.IsInstalled
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = mod.IsAutodetected ? Properties.Resources.MainModListAutoDetected : "-"
                };

            var autoInstalled = mod.IsInstalled && !mod.IsAutodetected
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = mod.IsAutoInstalled
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = "-"
                };

            var updating = mod.IsInstallable() && mod.HasUpdate
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = myChange == null ? false
                        : myChange.ChangeType == GUIModChangeType.Update ? true
                        : false
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = "-"
                };

            var replacing = IsModInFilter(GUIModFilter.Replaceable, null, null, mod)
                ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                {
                    Value = myChange == null ? false
                        : myChange.ChangeType == GUIModChangeType.Replace ? true
                        : false
                }
                : new DataGridViewTextBoxCell()
                {
                    Value = "-"
                };

            var name   = new DataGridViewTextBoxCell() {Value = mod.Name};
            var author = new DataGridViewTextBoxCell() {Value = mod.Authors};

            var installVersion = new DataGridViewTextBoxCell()
            {
                Value = hideEpochs
                    ? (hideV
                        ? ModuleInstaller.StripEpoch(ModuleInstaller.StripV(mod.InstalledVersion ?? ""))
                        : ModuleInstaller.StripEpoch(mod.InstalledVersion ?? ""))
                    : (hideV
                        ? ModuleInstaller.StripV(mod.InstalledVersion ?? "")
                        : mod.InstalledVersion ?? "")
            };

            var latestVersion = new DataGridViewTextBoxCell()
            {
                Value =
                    hideEpochs ?
                        (hideV ? ModuleInstaller.StripEpoch(ModuleInstaller.StripV(mod.LatestVersion))
                        : ModuleInstaller.StripEpoch(mod.LatestVersion))
                    : (hideV ? ModuleInstaller.StripV(mod.LatestVersion)
                        : mod.LatestVersion)
            };

            var downloadCount = new DataGridViewTextBoxCell() { Value = String.Format("{0:N0}", mod.DownloadCount) };
            var compat        = new DataGridViewTextBoxCell() { Value = mod.KSPCompatibility                       };
            var size          = new DataGridViewTextBoxCell() { Value = mod.DownloadSize                           };
            var installDate   = new DataGridViewTextBoxCell() { Value = mod.InstallDate                            };
            var desc          = new DataGridViewTextBoxCell() { Value = mod.Abstract                               };

            item.Cells.AddRange(selecting, autoInstalled, updating, replacing, name, author, installVersion, latestVersion, compat, size, installDate, downloadCount, desc);

            selecting.ReadOnly     = selecting     is DataGridViewTextBoxCell;
            autoInstalled.ReadOnly = autoInstalled is DataGridViewTextBoxCell;
            updating.ReadOnly      = updating      is DataGridViewTextBoxCell;

            return item;
        }
        
        public Color GetRowBackground(GUIMod mod, bool conflicted, string instanceName)
        {
            if (conflicted)
            {
                return Color.LightCoral;
            }
            DataGridViewRow row;
            if (full_list_of_mod_rows.TryGetValue(mod.Identifier, out row))
            {
                Color? myColor = ModuleLabels.LabelsFor(instanceName)
                    .FirstOrDefault(l => l.ModuleIdentifiers.Contains(mod.Identifier))
                    ?.Color;
                if (myColor.HasValue)
                {
                    return myColor.Value;
                }
            }
            return Color.Empty;
        }

        /// <summary>
        /// Update the color and visible state of the given row
        /// after it has been added to or removed from a label group
        /// </summary>
        /// <param name="mod">The mod that needs an update</param>
        public void ReapplyLabels(GUIMod mod, bool conflicted, string instanceName)
        {
            DataGridViewRow row;
            if (full_list_of_mod_rows.TryGetValue(mod.Identifier, out row))
            {
                row.DefaultCellStyle.BackColor = GetRowBackground(mod, conflicted, instanceName);
                row.Visible = IsVisible(mod, instanceName);
            }
        }

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        public string StripEpoch(string version)
        {
            // If our version number starts with a string of digits, followed by
            // a colon, and then has no more colons, we're probably safe to assume
            // the first string of digits is an epoch
            //return Regex.IsMatch(version, @"^[0-9][0-9]*:[^:]+$") ? Regex.Replace(version, @"^([^:]+):([^:]+)$", @"$2") : version;
            return ContainsEpoch.IsMatch(version) ? RemoveEpoch.Replace(version, @"$2") : version;
        }

        private static readonly Regex ContainsEpoch = new Regex(@"^[0-9][0-9]*:[^:]+$", RegexOptions.Compiled);
        private static readonly Regex RemoveEpoch   = new Regex(@"^([^:]+):([^:]+)$",   RegexOptions.Compiled);


        private bool IsNameInNameFilter(GUIMod mod)
        {
            string sanitisedModNameFilter = CkanModule.nonAlphaNums.Replace(ModNameFilter, "");

            return mod.Abbrevation.IndexOf(ModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1
                || mod.SearchableName.IndexOf(sanitisedModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1
                || mod.SearchableIdentifier.IndexOf(sanitisedModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private bool IsAuthorInAuthorFilter(GUIMod mod)
        {
            string sanitisedModAuthorFilter = CkanModule.nonAlphaNums.Replace(ModAuthorFilter, "");

            return mod.SearchableAuthors.Any((author) => author.IndexOf(sanitisedModAuthorFilter, StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        private bool IsAbstractInDescriptionFilter(GUIMod mod)
        {
            string sanitisedModDescriptionFilter = CkanModule.nonAlphaNums.Replace(ModDescriptionFilter, "");

            return mod.SearchableAbstract.IndexOf(sanitisedModDescriptionFilter, StringComparison.InvariantCultureIgnoreCase) != -1
                || mod.SearchableDescription.IndexOf(sanitisedModDescriptionFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private bool IsModInFilter(GUIModFilter filter, ModuleTag tag, ModuleLabel label, GUIMod m)
        {
            switch (filter)
            {
                case GUIModFilter.Compatible:               return !m.IsIncompatible;
                case GUIModFilter.Installed:                return m.IsInstalled;
                case GUIModFilter.InstalledUpdateAvailable: return m.IsInstalled && m.HasUpdate;
                case GUIModFilter.Cached:                   return m.IsCached;
                case GUIModFilter.Uncached:                 return !m.IsCached;
                case GUIModFilter.NewInRepository:          return m.IsNew;
                case GUIModFilter.NotInstalled:             return !m.IsInstalled;
                case GUIModFilter.Incompatible:             return m.IsIncompatible;
                case GUIModFilter.Replaceable:              return m.IsInstalled && m.HasReplacement;
                case GUIModFilter.All:                      return true;
                case GUIModFilter.Tag:                      return tag?.ModuleIdentifiers.Contains(m.Identifier)
                    ?? ModuleTags.Untagged.Contains(m.Identifier);
                case GUIModFilter.CustomLabel:              return label?.ModuleIdentifiers?.Contains(m.Identifier) ?? false;
                default:                                    throw new Kraken(string.Format(Properties.Resources.MainModListUnknownFilter, filter));
            }
        }

        public static Dictionary<GUIMod, string> ComputeConflictsFromModList(IRegistryQuerier registry,
            IEnumerable<ModChange> change_set, KspVersionCriteria ksp_version)
        {
            var modules_to_install = new HashSet<string>();
            var modules_to_remove = new HashSet<string>();
            var options = new RelationshipResolverOptions
            {
                without_toomanyprovides_kraken = true,
                proceed_with_inconsistencies = true,
                without_enforce_consistency = true,
                with_recommends = false
            };

            foreach (var change in change_set)
            {
                switch (change.ChangeType)
                {
                    case GUIModChangeType.None:
                        break;
                    case GUIModChangeType.Install:
                        modules_to_install.Add(change.Mod.identifier);
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Mod.identifier);
                        break;
                    case GUIModChangeType.Update:
                        break;
                    case GUIModChangeType.Replace:
                        ModuleReplacement repl = registry.GetReplacement(change.Mod, ksp_version);
                        if (repl != null)
                        {
                            modules_to_remove.Add(repl.ToReplace.identifier);
                            modules_to_install.Add(repl.ReplaceWith.identifier);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Only check mods that would exist after the changes are made.
            IEnumerable<CkanModule> installed = registry.InstalledModules.Where(
                im => !modules_to_remove.Contains(im.Module.identifier)
            ).Select(im => im.Module);

            // Convert ONLY modules_to_install with CkanModule.FromIDandVersion,
            // because it may not find already-installed modules.
            IEnumerable<CkanModule> mods_to_check = installed.Union(
                modules_to_install.Except(modules_to_remove).Select(
                    name => CkanModule.FromIDandVersion(registry, name, ksp_version)
                )
            );
            var resolver = new RelationshipResolver(
                mods_to_check,
                change_set.Where(ch => ch.ChangeType == GUIModChangeType.Remove)
                    .Select(ch => ch.Mod),
                options, registry, ksp_version
            );
            return resolver.ConflictList.ToDictionary(item => new GUIMod(item.Key, registry, ksp_version),
                item => item.Value);
        }

        public HashSet<ModChange> ComputeUserChangeSet(IRegistryQuerier registry)
        {
            var removableAuto = registry?.FindRemovableAutoInstalled(registry?.InstalledModules)
                ?? new InstalledModule[] {};
            return new HashSet<ModChange>(
                Modules
                    .SelectMany(mod => mod.GetModChanges())
                    .Union(removableAuto.Select(im => new ModChange(
                        im.Module,
                        GUIModChangeType.Remove,
                        new SelectionReason.NoLongerUsed())))
            );
        }
    }
}
