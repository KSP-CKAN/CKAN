using System;
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
        Replaceable              = 8
    }

    public partial class Main
    {
        private IEnumerable<DataGridViewRow> _SortRowsByColumn(IEnumerable<DataGridViewRow> rows)
        {
            switch (this.configuration.SortByColumnIndex)
            {
                // XXX: There should be a better way to identify checkbox columns than hardcoding their indices here
                case 0: case 1: case 2: return Sort(rows, CheckboxSorter);
                case 8:                 return Sort(rows, DownloadSizeSorter);
                case 9:                 return Sort(rows, InstallDateSorter);
                case 10:                return Sort(rows, r => (r.Tag as GUIMod)?.DownloadCount ?? 0);
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
                return (bool)cell.Value ? "a" : "b";
            }
            // It's a "-" cell so let it be ordered last
            return "c";
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
                row.Visible = mainModList.IsVisible(mod);
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

        public void UpdateModsList(Boolean repo_updated = false, IEnumerable<ModChange> mc = null)
        {
            // Run the update in the background so the UI thread can appear alive
            Task.Factory.StartNew(() =>
                _UpdateModsList(repo_updated, mc ?? new List<ModChange>())
            );
        }

        private void _UpdateModsList(bool repo_updated, IEnumerable<ModChange> mc)
        {
            log.Info("Updating the mod list");

            ResetProgress();
            tabController.RenameTab("WaitTabPage", "Loading modules");
            ShowWaitDialog(false);
            tabController.SetTabLock(true);
            Util.Invoke(this, SwitchEnabledState);
            ClearLog();

            AddLogMessage("Loading registry...");
            KspVersionCriteria versionCriteria = CurrentInstance.VersionCriteria();
            IRegistryQuerier registry = RegistryManager.Instance(CurrentInstance).registry;

            AddLogMessage("Loading installed modules...");
            var gui_mods = new HashSet<GUIMod>();
            gui_mods.UnionWith(
                registry.InstalledModules
                    .Select(instMod => new GUIMod(instMod, registry, versionCriteria))
            );
            AddLogMessage("Loading available modules...");
            gui_mods.UnionWith(
                registry.Available(versionCriteria)
                    .Select(m => new GUIMod(m, registry, versionCriteria))
            );
            AddLogMessage("Loading incompatible modules...");
            gui_mods.UnionWith(
                registry.Incompatible(versionCriteria)
                    .Select(m => new GUIMod(m, registry, versionCriteria, true))
            );

            if (mc != null)
            {
                AddLogMessage("Restoring change set...");
                foreach (ModChange change in mc)
                {
                    // Propagate IsInstallChecked and IsUpgradeChecked to the next generation
                    gui_mods.FirstOrDefault(
                        mod => mod.Identifier == change.Mod.Identifier
                    )?.SetRequestedChange(change.ChangeType);
                }
            }

            AddLogMessage("Preserving new flags...");
            var old_modules = mainModList.Modules.ToDictionary(m => m, m => m.IsIncompatible);
            if (repo_updated)
            {
                foreach (GUIMod gm in gui_mods)
                {
                    bool oldIncompat;
                    if (old_modules.TryGetValue(gm, out oldIncompat))
                    {
                        // Found it; check if newly compatible
                        if (!gm.IsIncompatible && oldIncompat)
                        {
                            gm.IsNew = true;
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
                //Copy the new mod flag from the old list.
                var old_new_mods = new HashSet<GUIMod>(old_modules.Keys.Where(m => m.IsNew));
                foreach (var gui_mod in gui_mods.Where(m => old_new_mods.Contains(m)))
                {
                    gui_mod.IsNew = true;
                }
            }

            AddLogMessage("Populating mod list...");
            // Update our mod listing
            mainModList.ConstructModList(gui_mods.ToList(), mc, configuration.HideEpochs, configuration.HideV);
            mainModList.Modules = new ReadOnlyCollection<GUIMod>(
                mainModList.full_list_of_mod_rows.Values.Select(row => row.Tag as GUIMod).ToList());

            AddLogMessage("Updating filters...");

            var has_any_updates = gui_mods.Any(mod => mod.HasUpdate);
            var has_any_replacements = gui_mods.Any(mod => mod.HasReplacement);

            //TODO Consider using smart enumeration pattern so stuff like this is easier
            Util.Invoke(menuStrip2, () =>
            {
                FilterToolButton.DropDownItems[0].Text = String.Format("Compatible ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.Compatible));
                FilterToolButton.DropDownItems[1].Text = String.Format("Installed ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.Installed));
                FilterToolButton.DropDownItems[2].Text = String.Format("Upgradeable ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));
                FilterToolButton.DropDownItems[3].Text = String.Format("Replaceable ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.Replaceable));
                FilterToolButton.DropDownItems[4].Text = String.Format("Cached ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.Cached));
                FilterToolButton.DropDownItems[5].Text = String.Format("Newly compatible ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.NewInRepository));
                FilterToolButton.DropDownItems[6].Text = String.Format("Not installed ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.NotInstalled));
                FilterToolButton.DropDownItems[7].Text = String.Format("Incompatible ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.Incompatible));
                FilterToolButton.DropDownItems[8].Text = String.Format("All ({0})",
                    mainModList.CountModsByFilter(GUIModFilter.All));

                UpdateAllToolButton.Enabled = has_any_updates;
            });

            UpdateFilters(this);

            // Hide update and replacement columns if not needed.
            // Write it to the configuration, else they are hidden agian after a filter change.
            // After the update / replacement, they are hidden again.
            ModList.Columns[1].Visible = has_any_updates;
            ModList.Columns[2].Visible = has_any_replacements;

            AddLogMessage("Updating tray...");
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
            if (!mainModList.full_list_of_mod_rows.ContainsKey(identifier))
            {
                return;
            }
            DataGridViewRow row = mainModList.full_list_of_mod_rows[identifier];

            var mod = (GUIMod)row.Tag;
            if (mod.Identifier == identifier)
            {
                mod.SetInstallChecked(row, !uninstall);
            }
        }

        public void MarkModForUpdate(string identifier)
        {
            Util.Invoke(this, () => _MarkModForUpdate(identifier));
        }

        public void _MarkModForUpdate(string identifier)
        {
            DataGridViewRow row = mainModList.full_list_of_mod_rows[identifier];
            var mod = (GUIMod)row.Tag;
            mod.SetUpgradeChecked(row, true);
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Skip if already disposed (i.e. after the form has been closed).
            // Needed for TransparentTextBoxes
            if (ModInfoTabControl.IsDisposed)
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

                ModListHeaderContextMenuStrip.Items.AddRange(
                    ModList.Columns.Cast<DataGridViewColumn>()
                    .Where(col => col.Index > 2)
                    .Select(col => new ToolStripMenuItem()
                    {
                        Name    = col.Name,
                        Text    = col.HeaderText,
                        Checked = col.Visible,
                        Tag     = col
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

            if (col != null)
            {
                col.Visible = !clickedItem.Checked;
                configuration.SetColumnVisibility(col.Name, !clickedItem.Checked);
                if (col.Index == 0)
                {
                    InstallAllCheckbox.Visible = col.Visible;
                }
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
                    ModList.CurrentCell = ModList.Rows[0].Cells[2];
                    e.Handled = true;
                    break;

                case Keys.End:
                    // Last row.
                    ModList.CurrentCell = ModList.Rows[ModList.Rows.Count - 1].Cells[2];
                    e.Handled = true;
                    break;

                case Keys.Space:
                    // If they've focused one of the checkbox columns, don't intercept
                    if (ModList.CurrentCell.ColumnIndex > 2)
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
                    Process.Start(cmd);
            }
            else if (column_index <= 2)
            {
                GUIMod gui_mod = row?.Tag as GUIMod;
                if (gui_mod != null)
                {
                    switch (column_index)
                    {
                        case 0:
                            gui_mod.SetInstallChecked(row);
                            if (gui_mod.IsInstallChecked)
                                last_mod_to_have_install_toggled.Push(gui_mod);
                            break;
                        case 1:
                            gui_mod.SetUpgradeChecked(row);
                            break;
                        case 2:
                            gui_mod.SetReplaceChecked(row);
                            break;
                    }
                    await UpdateChangeSetAndConflicts(
                        RegistryManager.Instance(CurrentInstance).registry
                    );
                }
            }
        }

        private void InstallAllCheckbox_CheckChanged(object sender, EventArgs e)
        {
            if (this.InstallAllCheckbox.Checked)
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
                        mod.SetInstallChecked(row, false);
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
        public async Task<IEnumerable<ModChange>> ComputeChangeSetFromModList(
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
                        //TODO: Fix
                        //This will give us a mod with a wrong version!
                        modules_to_install.Add(change.Mod.ToCkanModule());
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Mod);
                        break;
                    case GUIModChangeType.Replace:
                        ModuleReplacement repl = registry.GetReplacement(change.Mod.ToModule(), version);
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

            bool handled_all_too_many_provides = false;
            while (!handled_all_too_many_provides)
            {
                //Can't await in catch clause - doesn't seem to work in mono. Hence this flag
                TooManyModsProvideKraken kraken;
                try
                {
                    new RelationshipResolver(
                        modules_to_install,
                        null,
                        RelationshipResolver.DependsOnlyOpts(),
                        registry, version);
                    handled_all_too_many_provides = true;
                    continue;
                }
                catch (TooManyModsProvideKraken k)
                {
                    kraken = k;
                }
                //Shouldn't get here unless there is a kraken.
                var mod = await too_many_provides(kraken);
                if (mod != null)
                {
                    modules_to_install.Add(mod);
                }
                else
                {
                    //TODO Is could be a new type of Kraken.
                    throw kraken;
                }
            }

            foreach (var dependency in registry.FindReverseDependencies(
                modules_to_remove
                    .Select(mod => mod.identifier)
                    .Except(modules_to_install.Select(m => m.identifier))
            ))
            {
                //TODO This would be a good place to have a event that alters the row's graphics to show it will be removed
                CkanModule module_by_version = registry.GetModuleByVersion(installed_modules[dependency].identifier,
                    installed_modules[dependency].version) ?? registry.InstalledModule(dependency).Module;
                changeSet.Add(new ModChange(new GUIMod(module_by_version, registry, version), GUIModChangeType.Remove, null));
            }

            var resolver = new RelationshipResolver(
                modules_to_install,
                changeSet.Where(change => change.ChangeType.Equals(GUIModChangeType.Remove)).Select(m => m.Mod.ToModule()),
                RelationshipResolver.DependsOnlyOpts(), registry, version);
            changeSet.UnionWith(
                resolver.ModList()
                    .Select(m => new ModChange(new GUIMod(m, registry, version), GUIModChangeType.Install, resolver.ReasonFor(m))));

            return changeSet;
        }

        public bool IsVisible(GUIMod mod)
        {
            var nameMatchesFilter = IsNameInNameFilter(mod);
            var authorMatchesFilter = IsAuthorInauthorFilter(mod);
            var abstractMatchesFilter = IsAbstractInDescriptionFilter(mod);
            var modMatchesType = IsModInFilter(ModFilter, mod);
            var isVisible = nameMatchesFilter && modMatchesType && authorMatchesFilter && abstractMatchesFilter;
            return isVisible;
        }

        public int CountModsByFilter(GUIModFilter filter)
        {
            if (filter == GUIModFilter.All)
            {
                // Don't check each one
                return Modules.Count;
            }
            return Modules.Count(m => IsModInFilter(filter, m));
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
            IEnumerable<GUIMod> modules, IEnumerable<ModChange> mc = null,
            bool hideEpochs = false, bool hideV = false)
        {
            List<ModChange> changes = mc?.ToList();
            full_list_of_mod_rows = modules.ToDictionary(
                gm => gm.Identifier,
                gm => MakeRow(gm, changes, hideEpochs, hideV)
            );
            return full_list_of_mod_rows.Values;
        }

        private DataGridViewRow MakeRow(GUIMod mod, List<ModChange> changes, bool hideEpochs = false, bool hideV = false)
        {
            DataGridViewRow item = new DataGridViewRow() {Tag = mod};

            ModChange myChange = changes?.FindLast((ModChange ch) => ch.Mod.Identifier == mod.Identifier);

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
                    Value = mod.IsAutodetected ? "AD" : "-"
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

            var replacing = IsModInFilter(GUIModFilter.Replaceable, mod)
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

            item.Cells.AddRange(selecting, updating, replacing, name, author, installVersion, latestVersion, compat, size, installDate, downloadCount, desc);

            selecting.ReadOnly = selecting is DataGridViewTextBoxCell;
            updating.ReadOnly  = updating  is DataGridViewTextBoxCell;

            return item;
        }

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        public string StripEpoch(string version)
        {
            // If our version number starts with a string of digits, followed by
            // a colon, and then has no more colons, we're probably safe to assume
            // the first string of digits is an epoch
            return Regex.IsMatch(version, @"^[0-9][0-9]*:[^:]+$") ? Regex.Replace(version, @"^([^:]+):([^:]+)$", @"$2") : version;
        }

        private bool IsNameInNameFilter(GUIMod mod)
        {
            return mod.Name.IndexOf(ModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1
                || mod.Abbrevation.IndexOf(ModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1
                || mod.Identifier.IndexOf(ModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private bool IsAuthorInauthorFilter(GUIMod mod)
        {
            return mod.Authors.IndexOf(ModAuthorFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private bool IsAbstractInDescriptionFilter(GUIMod mod)
        {
            return mod.Abstract.IndexOf(ModDescriptionFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private static bool IsModInFilter(GUIModFilter filter, GUIMod m)
        {
            switch (filter)
            {
                case GUIModFilter.Compatible:               return !m.IsIncompatible;
                case GUIModFilter.Installed:                return m.IsInstalled;
                case GUIModFilter.InstalledUpdateAvailable: return m.IsInstalled && m.HasUpdate;
                case GUIModFilter.Cached:                   return m.IsCached;
                case GUIModFilter.NewInRepository:          return m.IsNew;
                case GUIModFilter.NotInstalled:             return !m.IsInstalled;
                case GUIModFilter.Incompatible:             return m.IsIncompatible;
                case GUIModFilter.Replaceable:              return m.IsInstalled && m.HasReplacement;
                case GUIModFilter.All:                      return true;
                default:                                    throw new Kraken($"Unknown filter type {filter} in IsModInFilter");
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
                        modules_to_install.Add(change.Mod.Identifier);
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Mod.Identifier);
                        break;
                    case GUIModChangeType.Update:
                        break;
                    case GUIModChangeType.Replace:
                        ModuleReplacement repl = registry.GetReplacement(change.Mod.ToModule(), ksp_version);
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
                    .Select(ch => ch.Mod.ToModule()),
                options, registry, ksp_version
            );
            return resolver.ConflictList.ToDictionary(item => new GUIMod(item.Key, registry, ksp_version),
                item => item.Value);
        }

        public HashSet<ModChange> ComputeUserChangeSet()
        {
            return new HashSet<ModChange>(
                Modules
                    .Where(mod => mod.IsInstallable())
                    .Select(mod => mod.GetRequestedChange())
                    .Where(change => change.HasValue)
                    .Select(change => change.Value)
                    .Select(change => new ModChange(change.Key, change.Value, null))
            );
        }
    }
}
