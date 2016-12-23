﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CKAN.Versioning;

namespace CKAN
{
    public partial class Main
    {
        private void UpdateFilters(Main control)
        {
            Util.Invoke(control, _UpdateFilters);
        }

        private IEnumerable<DataGridViewRow> _SortRowsByColumn(IEnumerable<DataGridViewRow> rows)
        {
            // XXX: There should be a better way to identify checkbox columns than hardcoding their indices here
            if (this.configuration.SortByColumnIndex < 2)
            {
                return Sort(rows, CheckboxSorter);
            }
            // XXX: Same for Integer columns
            else if (this.configuration.SortByColumnIndex == 7)
            {
                return Sort(rows, IntegerSorter);
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
        /// Transforms a DataGridViewRow into an integer suitable for sorting.
        /// Uses this.m_Configuration.SortByColumnIndex to determine which
        /// field to sort on.
        /// </summary>
        private int IntegerSorter(DataGridViewRow row)
        {
            var cell = row.Cells[this.configuration.SortByColumnIndex];

            if (cell.Value.ToString() == "N/A")
                return -1;
            else if (cell.Value.ToString() == "1<KB")
                return 0;

            int result = -2;
            int.TryParse(cell.Value as string, out result);
            return result;
        }

        private void _UpdateFilters()
        {
            if (ModList == null) return;

            // Each time a row in DataGridViewRow is changed, DataGridViewRow updates the view. Which is slow.
            // To make the filtering process faster, Copy the list of rows. Filter out the hidden and replace t
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

        public void UpdateModsList(Boolean repo_updated = false)
        {
            Util.Invoke(this, () => _UpdateModsList(repo_updated));
        }

        private void _UpdateModsList(bool repo_updated)
        {
            log.Debug("Updating the mod list");

            KspVersionCriteria versionCriteria = CurrentInstance.VersionCriteria();
            IRegistryQuerier registry = RegistryManager.Instance(CurrentInstance).registry;
            var gui_mods = new HashSet<GUIMod>(registry.Available(versionCriteria)
                .Select(m => new GUIMod(m, registry, versionCriteria)));
            gui_mods.UnionWith(registry.Incompatible(versionCriteria)
                .Select(m => new GUIMod(m, registry, versionCriteria)));
            var installed = registry.InstalledModules
                .Select(m => new GUIMod(m.Module, registry, versionCriteria));

            //Hashset does not define if add/unionwith replaces existing elements.
            //In this case that could cause a CkanModule to be replaced by a Module.
            //Hence the explicit checking
            foreach (var mod in installed.Where(mod => !gui_mods.Contains(mod)))
            {
                gui_mods.Add(mod);
            }
            var old_modules = new HashSet<GUIMod>(mainModList.Modules);
            if (repo_updated)
            {
                foreach (var gui_mod in gui_mods.Where(m => !old_modules.Contains(m)))
                {
                    gui_mod.IsNew = true;
                }
            }
            else
            {
                //Copy the new mod flag from the old list.
                var old_new_mods = new HashSet<GUIMod>(old_modules.Where(m => m.IsNew));
                foreach (var gui_mod in gui_mods.Where(m => old_new_mods.Contains(m)))
                {
                    gui_mod.IsNew = true;
                }
            }

            // Update our mod listing. If we're doing a repo update, then we don't refresh
            // all (in case the user has selected changes they wish to apply).
            mainModList.ConstructModList(gui_mods.ToList(), refreshAll: !repo_updated, hideEpochs: configuration.HideEpochs);
            mainModList.Modules = new ReadOnlyCollection<GUIMod>(
                mainModList.full_list_of_mod_rows.Values.Select(row => row.Tag as GUIMod).ToList());

            //TODO Consider using smart enum patten so stuff like this is easier
            FilterToolButton.DropDownItems[0].Text = String.Format("Compatible ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Compatible));
            FilterToolButton.DropDownItems[1].Text = String.Format("Installed ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Installed));
            FilterToolButton.DropDownItems[2].Text = String.Format("Upgradeable ({0})",
                mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));
            FilterToolButton.DropDownItems[3].Text = String.Format("Cached ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Cached));
            FilterToolButton.DropDownItems[4].Text = String.Format("New in repository ({0})",
                mainModList.CountModsByFilter(GUIModFilter.NewInRepository));
            FilterToolButton.DropDownItems[5].Text = String.Format("Not installed ({0})",
                mainModList.CountModsByFilter(GUIModFilter.NotInstalled));
            FilterToolButton.DropDownItems[6].Text = String.Format("Incompatible ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Incompatible));
            FilterToolButton.DropDownItems[7].Text = String.Format("All ({0})",
                mainModList.CountModsByFilter(GUIModFilter.All));
            var has_any_updates = gui_mods.Any(mod => mod.HasUpdate);
            UpdateAllToolButton.Enabled = has_any_updates;
            UpdateFilters(this);
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
            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = (GUIMod) row.Tag;
                if (mod.Identifier == identifier)
                {
                    (row.Cells[1] as DataGridViewCheckBoxCell).Value = true;
                    break;
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
            //Hacky workaround for https://bugzilla.xamarin.com/show_bug.cgi?id=24372
            if (Platform.IsMono && !Platform.IsMonoFour)
            {
                var first_row_index = typeof (MainModListGUI).BaseType
                    .GetField("first_row_index", BindingFlags.NonPublic | BindingFlags.Instance);
                var value = (int) first_row_index.GetValue(this);
                if (value < 0 || value >= Rows.Count)
                {
                    first_row_index.SetValue(this, 0);
                }
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
            var options = new RelationshipResolverOptions
            {
                without_toomanyprovides_kraken = false,
                with_recommends = false
            };

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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            var installed_modules =
                registry.InstalledModules.Select(imod => imod.Module).ToDictionary(mod => mod.identifier, mod => mod);

            bool handled_all_to_many_provides = false;
            while (!handled_all_to_many_provides)
            {
                //Can't await in catch clause - doesn't seem to work in mono. Hence this flag
                TooManyModsProvideKraken kraken;
                try
                {
                    new RelationshipResolver(modules_to_install.ToList(), options, registry, version);
                    handled_all_to_many_provides = true;
                    continue;
                }
                catch (TooManyModsProvideKraken k)
                {
                    kraken = k;
                }
                catch (ModuleNotFoundKraken k)
                {
                    //We shouldn't need this. However the relationship provider will throw TMPs with incompatible mods.
                    user.RaiseError("Module {0} has not been found. This may be because it is not compatible " +
                                    "with the currently installed version of KSP", k.module);
                    return null;
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

            foreach (var dependency in registry.FindReverseDependencies(modules_to_remove.Select(mod=>mod.identifier)))
            {
                //TODO This would be a good place to have a event that alters the row's graphics to show it will be removed
                CkanModule module_by_version = registry.GetModuleByVersion(installed_modules[dependency].identifier,
                    installed_modules[dependency].version) ?? registry.InstalledModule(dependency).Module;
                changeSet.Add(new ModChange(new GUIMod(module_by_version, registry, version), GUIModChangeType.Remove, null));
            }

            var resolver = new RelationshipResolver(options, registry, version);
            resolver.RemoveModsFromInstalledList(
                changeSet.Where(change => change.ChangeType.Equals(GUIModChangeType.Remove)).Select(m => m.Mod.ToModule()));
            resolver.AddModulesToInstall(modules_to_install.ToList());
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
            var modMatchesType = IsModInFilter(mod);
            var isVisible = nameMatchesFilter && modMatchesType && authorMatchesFilter && abstractMatchesFilter;
            return isVisible;
        }


        public int CountModsByFilter(GUIModFilter filter)
        {
            switch (filter)
            {
                case GUIModFilter.Compatible:
                    return Modules.Count(m => !m.IsIncompatible);
                case GUIModFilter.Installed:
                    return Modules.Count(m => m.IsInstalled);
                case GUIModFilter.InstalledUpdateAvailable:
                    return Modules.Count(m => m.HasUpdate);
                case GUIModFilter.Cached:
                    return Modules.Count(m => m.IsCached);
                case GUIModFilter.NewInRepository:
                    return Modules.Count(m => m.IsNew);
                case GUIModFilter.NotInstalled:
                    return Modules.Count(m => !m.IsInstalled);
                case GUIModFilter.Incompatible:
                    return Modules.Count(m => m.IsIncompatible);
                case GUIModFilter.All:
                    return Modules.Count();
            }
            throw new Kraken("Unknown filter type in CountModsByFilter");
        }

        /// <summary>
        /// Constructs the mod list suitable for display to the user.
        /// This manipulates <c>full_list_of_mod_rows</c> as it runs, and by default
        /// will only update entries which have changed or were previously missing.
        /// (Set <c>refreshAll</c> to force update everything.)
        /// </summary>
        /// <returns>The mod list.</returns>
        /// <param name="modules">A list of modules that may require updating</param>
        /// <param name="refreshAll">If set to <c>true</c> then always rebuild the list from scratch</param>
        /// <param name="hideEpochs">If true, remove epochs from the displayed versions</param>
        public IEnumerable<DataGridViewRow> ConstructModList(IEnumerable<GUIMod> modules, bool refreshAll = false, bool hideEpochs = false)
        {

            if (refreshAll || full_list_of_mod_rows == null)
            {
                full_list_of_mod_rows = new Dictionary<string, DataGridViewRow>();
            }

            // We're only going to update the status of rows that either don't already exist,
            // or which exist but have changed their latest version
            // or whose installation status has changed
            //
            // TODO: Will this catch a mod where the latest version number remains the same, but
            // another part of the metadata (eg: dependencies or description) has changed?
            IEnumerable<GUIMod> rowsToUpdate = modules.Where(
                mod => !full_list_of_mod_rows.ContainsKey(mod.Identifier) ||
                mod.LatestVersion != (full_list_of_mod_rows[mod.Identifier].Tag as GUIMod)?.LatestVersion ||
                mod.IsInstalled != (full_list_of_mod_rows[mod.Identifier].Tag as GUIMod)?.IsInstalled);

            // Let's update our list!
            foreach (var mod in rowsToUpdate)
            {
                full_list_of_mod_rows.Remove(mod.Identifier);
                var item = new DataGridViewRow {Tag = mod};

                var selecting = mod.IsInstallable()
                    ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                    : new DataGridViewTextBoxCell();

                selecting.Value = mod.IsInstallable()
                    ? (object) mod.IsInstalled
                    : (mod.IsAutodetected ? "AD" : "-");

                var updating = mod.HasUpdate && !mod.IsAutodetected
                    ? new DataGridViewCheckBoxCell()
                    : (DataGridViewCell) new DataGridViewTextBoxCell();

                updating.Value = !mod.IsInstallable() || !mod.HasUpdate
                    ? "-"
                    : (object) false;

                var name = new DataGridViewTextBoxCell {Value = mod.Name};
                var author = new DataGridViewTextBoxCell {Value = mod.Authors};
                var installVersion = new DataGridViewTextBoxCell {Value = hideEpochs ? StripEpoch(mod.InstalledVersion) : mod.InstalledVersion };
                var latestVersion = new DataGridViewTextBoxCell {Value = hideEpochs ? StripEpoch(mod.LatestVersion) : mod.LatestVersion };
                var desc = new DataGridViewTextBoxCell {Value = mod.Abstract};
                var compat = new DataGridViewTextBoxCell {Value = mod.KSPCompatibility};
                var size = new DataGridViewTextBoxCell {Value = mod.DownloadSize};

                item.Cells.AddRange(selecting, updating, name, author, installVersion, latestVersion, compat, size, desc);

                selecting.ReadOnly = !mod.IsInstallable();
                updating.ReadOnly = !mod.IsInstallable() || !mod.HasUpdate;

                full_list_of_mod_rows.Add(mod.Identifier, item);
            }
            return full_list_of_mod_rows.Values;
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


        private bool IsModInFilter(GUIMod m)
        {
            switch (ModFilter)
            {
                case GUIModFilter.Compatible:
                    return !m.IsIncompatible;
                case GUIModFilter.Installed:
                    return m.IsInstalled;
                case GUIModFilter.InstalledUpdateAvailable:
                    return m.IsInstalled && m.HasUpdate;
                case GUIModFilter.Cached:
                    return m.IsCached;
                case GUIModFilter.NewInRepository:
                    return m.IsNew;
                case GUIModFilter.NotInstalled:
                    return !m.IsInstalled;
                case GUIModFilter.Incompatible:
                    return m.IsIncompatible;
                case GUIModFilter.All:
                    return true;
            }
            throw new Kraken("Unknown filter type in IsModInFilter");
        }


        public static Dictionary<GUIMod, string> ComputeConflictsFromModList(IRegistryQuerier registry,
            IEnumerable<ModChange> change_set, KspVersionCriteria ksp_version)
        {
            var modules_to_install = new HashSet<string>();
            var modules_to_remove = new HashSet<string>();
            var options = new RelationshipResolverOptions
            {
                without_toomanyprovides_kraken = true,
                procede_with_inconsistencies = true,
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var installed =
                registry.Installed()
                    .Where(pair => pair.Value.CompareTo(new ProvidesVersion("")) != 0)
                    .Select(pair => pair.Key);

            //We wish to only check mods that would exist after the changes are made.
            var mods_to_check = installed.Union(modules_to_install).Except(modules_to_remove);
            var resolver = new RelationshipResolver(mods_to_check.ToList(), options, registry, ksp_version);
            return resolver.ConflictList.ToDictionary(item => new GUIMod(item.Key, registry, ksp_version),
                item => item.Value);
        }

        public HashSet<ModChange> ComputeUserChangeSet()
        {
            var changes = Modules.Where(mod => mod.IsInstallable()).Select(mod => mod.GetRequestedChange());
            var changeset = new HashSet<ModChange>(
                changes.Where(change => change.HasValue).
                Select(change => change.Value).
                Select(change => new ModChange(change.Key, change.Value, null))
                );
            return changeset;
        }
    }
}