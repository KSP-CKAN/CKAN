using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            var get_row_mod_name = new Func<DataGridViewRow, string>(row => ((GUIMod)row.Tag).Name);
            Func<DataGridViewRow, string> sort_fn;

            // XXX: There should be a better way to identify checkbox columns than hardcoding their indices here
            if (this.m_Configuration.SortByColumnIndex < 2)
            {
                sort_fn = new Func<DataGridViewRow, string>(row => {
                    var cell = row.Cells[this.m_Configuration.SortByColumnIndex];
                    if (cell.ValueType == typeof(bool)) {
                        return (bool)cell.Value ? "a" : "b";
                    }
                    // It's a "-" cell so let it be ordered last
                    return "c";
                });
            }
            else
            {
                sort_fn = new Func<DataGridViewRow, string>(row => row.Cells[this.m_Configuration.SortByColumnIndex].Value.ToString());
            }
            // Update the column sort glyph
            this.ModList.Columns[this.m_Configuration.SortByColumnIndex].HeaderCell.SortGlyphDirection = this.m_Configuration.SortDescending ? SortOrder.Descending : SortOrder.Ascending;
            // The columns will be sorted by mod name in addition to whatever the current sorting column is
            if (this.m_Configuration.SortDescending)
            {
                return rows.OrderByDescending(sort_fn).ThenBy(get_row_mod_name);
            }
            return rows.OrderBy(sort_fn).ThenBy(get_row_mod_name);
        }

        private void _UpdateFilters()
        {
            if (ModList == null) return;

            // Each time a row in DataGridViewRow is changed, DataGridViewRow updates the view. Which is slow.
            // To make the filtering process faster, Copy the list of rows. Filter out the hidden and replace t
            // rows in DataGridView.

            var rows = new DataGridViewRow[mainModList.full_list_of_mod_rows.Count];
            mainModList.full_list_of_mod_rows.CopyTo(rows, 0);
            // Try to remember the current scroll position and selected mod
            var scroll_col = Math.Max(0, ModList.FirstDisplayedScrollingColumnIndex);
            CkanModule selected_mod = null;
            if (ModList.CurrentRow != null)
            {
                selected_mod = ((GUIMod) ModList.CurrentRow.Tag).ToCkanModule();
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
                    .FirstOrDefault(row => selected_mod.identifier == ((GUIMod)row.Tag).ToCkanModule().identifier);
                if (selected_row != null)
                {
                    ModList.CurrentCell = selected_row.Cells[scroll_col];
                }
            }
        }

        private void UpdateModsList(Boolean repo_updated = false)
        {
            Util.Invoke(this, () => _UpdateModsList(repo_updated));
        }


        private void _UpdateModsList(bool repo_updated)
        {
            log.Debug("Updating the mod list");
            Registry registry = RegistryManager.Instance(CurrentInstance).registry;

            var ckanModules = registry.Available(CurrentInstance.Version()).Concat(
                registry.Incompatible(CurrentInstance.Version())).ToList();
            var gui_mods =
                new HashSet<GUIMod>(ckanModules.Select(m => new GUIMod(m, registry, CurrentInstance.Version())));
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
            mainModList.Modules = new ReadOnlyCollection<GUIMod>(gui_mods.ToList());
            mainModList.ConstructModList(mainModList.Modules);

            //TODO Consider using smart enum patten so stuff like this is easier
            FilterToolButton.DropDownItems[0].Text = String.Format("Compatible ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Compatible));
            FilterToolButton.DropDownItems[1].Text = String.Format("Installed ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Installed));
            FilterToolButton.DropDownItems[2].Text = String.Format("Upgradeable ({0})",
                mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));
            FilterToolButton.DropDownItems[3].Text = String.Format("New in repository ({0})",
                mainModList.CountModsByFilter(GUIModFilter.NewInRepository));
            FilterToolButton.DropDownItems[4].Text = String.Format("Not installed ({0})",
                mainModList.CountModsByFilter(GUIModFilter.NotInstalled));
            FilterToolButton.DropDownItems[5].Text = String.Format("Incompatible ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Incompatible));
            FilterToolButton.DropDownItems[6].Text = String.Format("All ({0})",
                mainModList.CountModsByFilter(GUIModFilter.All));
            var has_any_updates = gui_mods.Any(mod => mod.HasUpdate);
            UpdateAllToolButton.Enabled = has_any_updates;
            UpdateFilters(this);
        }

        public void MarkModForInstall(string identifier, bool uninstall = false)
        {
            Util.Invoke(this, () => _MarkModForInstall(identifier, uninstall));
        }

        private void _MarkModForInstall(string identifier, bool uninstall)
        {
            foreach (DataGridViewRow row in mainModList.full_list_of_mod_rows)
            {
                var mod = (GUIMod) row.Tag;
                if (mod.Identifier == identifier)
                {
                    mod.IsInstallChecked = !uninstall;
                    //TODO Fix up MarkMod stuff when I commit the GUIConflict
                    (row.Cells[0] as DataGridViewCheckBoxCell).Value = !uninstall;
                    if (!uninstall) last_mod_to_have_install_toggled.Push(mod);
                    break;
                }
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

    public class MainModList
    {
        internal List<DataGridViewRow> full_list_of_mod_rows;

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

        private GUIModFilter _modFilter = GUIModFilter.Compatible;
        private string _modNameFilter = String.Empty;
        private string _modAuthorFilter = String.Empty;
        private IUser user;

        private readonly HandleTooManyProvides too_many_provides;

        /// <summary>
        /// This function returns a changeset based on the selections of the user.
        /// Currently returns null if a conflict is detected.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="current_instance"></param>

        public async Task<IEnumerable<KeyValuePair<CkanModule, GUIModChangeType>>> ComputeChangeSetFromModList(
            Registry registry, HashSet<KeyValuePair<CkanModule, GUIModChangeType>> changeSet, ModuleInstaller installer,
            KSPVersion version)
        {
            var modules_to_install = new HashSet<CkanModule>();
            var modules_to_remove = new HashSet<Module>();
            var options = new RelationshipResolverOptions
            {
                without_toomanyprovides_kraken = false,
                with_recommends = false
            };

            foreach (var change in changeSet)
            {
                switch (change.Value)
                {
                    case GUIModChangeType.None:
                        break;
                    case GUIModChangeType.Install:
                        modules_to_install.Add(change.Key);
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Key);
                        break;
                    case GUIModChangeType.Update:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            var installed_modules = registry.InstalledModules.Select(imod => imod.Module).ToDictionary(mod => mod.identifier,mod => mod);


            bool handled_all_to_many_provides = false;
            while (!handled_all_to_many_provides)
            {
                //Can't await in catch clause - doesn't seem to work in mono. Hence this flag
                TooManyModsProvideKraken kraken = null;
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

            
            foreach (var dependency in modules_to_remove.
                Select(mod=>installer.FindReverseDependencies(mod.identifier)).
                SelectMany(reverse_dependencies => reverse_dependencies))
            {
                //TODO This would be a good place to have a event that alters the row's graphics to show it will be removed
                changeSet.Add(
                    new KeyValuePair<CkanModule, GUIModChangeType>(
                        registry.GetModuleByVersion(installed_modules[dependency].identifier, installed_modules[dependency].version), GUIModChangeType.Remove));
            }
            //May throw InconsistentKraken
            var resolver = new RelationshipResolver(options, registry, version);
            resolver.RemoveModsFromInstalledList(changeSet.Where(change => change.Value.Equals(GUIModChangeType.Remove)).Select(m => m.Key));
            resolver.AddModulesToInstall(modules_to_install.ToList());
            changeSet.UnionWith(resolver.ModList().Select(mod => new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Install)));


            return changeSet;
        }

        public bool IsVisible(GUIMod mod)
        {
            var nameMatchesFilter = IsNameInNameFilter(mod);
            var authorMatchesFilter = IsAuthorInauthorFilter(mod);
            var modMatchesType = IsModInFilter(mod);
            var isVisible = nameMatchesFilter && modMatchesType && authorMatchesFilter;
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

        public IEnumerable<DataGridViewRow> ConstructModList(IEnumerable<GUIMod> modules)
        {
            full_list_of_mod_rows = new List<DataGridViewRow>();
            foreach (var mod in modules)
            {
                var item = new DataGridViewRow {Tag = mod};

                var installed_cell = mod.IsInstallable()
                    ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                    : new DataGridViewTextBoxCell();

                installed_cell.Value = mod.IsInstallable()
                    ? (object) mod.IsInstalled
                    : (mod.IsAutodetected ? "AD" : "-");

                var update_cell = mod.HasUpdate && !mod.IsAutodetected
                    ? new DataGridViewCheckBoxCell()
                    : (DataGridViewCell) new DataGridViewTextBoxCell();

                update_cell.Value = !mod.IsInstallable() || !mod.HasUpdate
                    ? "-"
                    : (object) false;

                var name_cell = new DataGridViewTextBoxCell {Value = mod.Name};
                var author_cell = new DataGridViewTextBoxCell {Value = mod.Authors};
                var installed_version_cell = new DataGridViewTextBoxCell {Value = mod.InstalledVersion};
                var latest_version_cell = new DataGridViewTextBoxCell {Value = mod.LatestVersion};
                var description_cell = new DataGridViewTextBoxCell {Value = mod.Abstract};
                var homepage_cell = new DataGridViewLinkCell {Value = mod.Homepage};

                item.Cells.AddRange(installed_cell, update_cell,
                    name_cell, author_cell,
                    installed_version_cell, latest_version_cell,
                    description_cell, homepage_cell);

                installed_cell.ReadOnly = !mod.IsInstallable();
                update_cell.ReadOnly = !mod.IsInstallable() || !mod.HasUpdate;

                full_list_of_mod_rows.Add(item);
            }
            return full_list_of_mod_rows;
        }

        private bool IsNameInNameFilter(GUIMod mod)
        {
            return mod.Name.IndexOf(ModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private bool IsAuthorInauthorFilter(GUIMod mod)
        {
            return mod.Authors.IndexOf(ModAuthorFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
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


        public static Dictionary<Module, string> ComputeConflictsFromModList(Registry registry, IEnumerable<KeyValuePair<CkanModule, GUIModChangeType>> changeSet, KSPVersion ksp_version)
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

            foreach (var change in changeSet)
            {
                switch (change.Value)
                {
                    case GUIModChangeType.None:
                        break;
                    case GUIModChangeType.Install:
                        modules_to_install.Add(change.Key.identifier);
                        break;
                    case GUIModChangeType.Remove:
                        modules_to_remove.Add(change.Key.identifier);
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
            return resolver.ConflictList;
        }

        public HashSet<KeyValuePair<CkanModule, GUIModChangeType>> ComputeUserChangeSet()
        {
            var changes = Modules.Where(mod => mod.IsInstallable()).Select(mod => mod.GetRequestedChange());
            var changeset = new HashSet<KeyValuePair<CkanModule, GUIModChangeType>>(
                changes.Where(change => change.HasValue).Select(change => change.Value)
                );
            return changeset;
        }
    }
}