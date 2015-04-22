using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main
    {
        private void UpdateFilters(Main control)
        {
            Util.Invoke(control, _UpdateFilters);
        }

        private void _UpdateFilters()
        {
            if (ModList == null) return;

            // Each time a row in DataGridViewRow is changed, DataGridViewRow updates the view. Which is slow.
            // To make the filtering process faster, Copy the list of rows. Make all row changes then add it back to the DataGridView.
            var rows = new DataGridViewRow[ModList.Rows.Count];
            ModList.Rows.CopyTo(rows, 0);
            ModList.Rows.Clear();

            foreach (DataGridViewRow row in rows)
            {
                var mod = ((GUIMod)row.Tag);
                var isVisible = mainModList.IsVisible(mod);
                row.Visible = isVisible;
            }

            ModList.Rows.AddRange(rows);
        }

        private void UpdateModsList()
        {
            Util.Invoke(this, _UpdateModsList);
        }


        private void _UpdateModsList()
        {
            Registry registry = RegistryManager.Instance(CurrentInstance).registry;

            var ckanModules = registry.Available(CurrentInstance.Version()).Concat(
                registry.Incompatible(CurrentInstance.Version())).ToList();
            var gui_mods = ckanModules.Select(m => new GUIMod(m, registry, CurrentInstance.Version())).ToList();
            mainModList.Modules = new ReadOnlyCollection<GUIMod>(gui_mods);
            var rows = MainModList.ConstructModList(mainModList.Modules);
            ModList.Rows.Clear();
            ModList.Rows.AddRange(rows.ToArray());
            ModList.Sort(ModList.Columns[2], ListSortDirection.Ascending);

            //TODO Consider using smart enum patten so stuff like this is easier
            FilterToolButton.DropDownItems[0].Text = String.Format("All ({0})",
                mainModList.CountModsByFilter(GUIModFilter.All));
            FilterToolButton.DropDownItems[1].Text = String.Format("Installed ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Installed));
            FilterToolButton.DropDownItems[2].Text = String.Format("Updated ({0})",
                mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));
            FilterToolButton.DropDownItems[3].Text = String.Format("New in repository ({0})",
                mainModList.CountModsByFilter(GUIModFilter.NewInRepository));
            FilterToolButton.DropDownItems[4].Text = String.Format("Not installed ({0})",
                mainModList.CountModsByFilter(GUIModFilter.NotInstalled));
            FilterToolButton.DropDownItems[5].Text = String.Format("Incompatible ({0})",
                mainModList.CountModsByFilter(GUIModFilter.Incompatible));

            var has_any_updates = gui_mods.Any(mod => mod.HasUpdate);
            UpdateAllToolButton.Enabled = has_any_updates;
            UpdateFilters(this);
        }

        public void MarkModForInstall(string identifier, bool uninstall = false)
        {
            Util.Invoke(this, () => _MarkModForInstall(identifier));
        }

        private void _MarkModForInstall(string identifier, bool uninstall = false)
        {
            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = (GUIMod)row.Tag;
                if (mod.Identifier == identifier)
                {
                    mod.IsInstallChecked = true;
                    //TODO Fix up MarkMod stuff when I commit the GUIConflict
                    (row.Cells[0] as DataGridViewCheckBoxCell).Value = !uninstall;
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
                var mod = (GUIMod)row.Tag;
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

        public MainModList(ModFiltersUpdatedEvent onModFiltersUpdated)
        {
            Modules = new ReadOnlyCollection<GUIMod>(new List<GUIMod>());
            ModFiltersUpdated += onModFiltersUpdated;
            ModFiltersUpdated(this);
        }

        public delegate void ModFiltersUpdatedEvent(MainModList source);

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

        private GUIModFilter _modFilter = GUIModFilter.All;
        private string _modNameFilter = String.Empty;

        /// <summary>
        /// This function returns a changeset based on the selections of the user. 
        /// Currently returns null if a conflict is detected.        
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="current_instance"></param>
        public static IEnumerable<KeyValuePair<CkanModule, GUIModChangeType>> ComputeChangeSetFromModList(Registry registry, HashSet<KeyValuePair<CkanModule, GUIModChangeType>> changeSet, ModuleInstaller installer, KSPVersion version)
        {

            var modules_to_install = new HashSet<string>();
            var modules_to_remove = new HashSet<string>();
            var options = new RelationshipResolverOptions()
            {
                without_toomanyprovides_kraken = true,
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

            //May throw InconsistentKraken
            var resolver = new RelationshipResolver(modules_to_install.ToList(), options, registry,version);
            changeSet.UnionWith(resolver.ModList().Select(mod => new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Install)));

            foreach (var reverse_dependencies in modules_to_remove.Select(installer.FindReverseDependencies))
            {
                //TODO This would be a good place to have a event that alters the row's graphics to show it will be removed
                var modules = reverse_dependencies.Select(rDep => registry.LatestAvailable(rDep, version));
                changeSet.UnionWith(modules.Select(mod => new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Remove)));
            }
            return changeSet;

        
    }

        public bool IsVisible(GUIMod mod)
        {

            var nameMatchesFilter = IsNameInNameFilter(mod);
            var modMatchesType = IsModInFilter(mod);
            var isVisible = nameMatchesFilter && modMatchesType;
            return isVisible;
        }



        public int CountModsByFilter(GUIModFilter filter)
        {

            switch (filter)
            {
                case GUIModFilter.All:
                    return Modules.Count(m => !m.IsIncompatible);
                case GUIModFilter.Installed:
                    return Modules.Count(m => m.IsInstalled);
                case GUIModFilter.InstalledUpdateAvailable:
                    return Modules.Count(m => m.HasUpdate);
                case GUIModFilter.NewInRepository:
                    return Modules.Count();
                case GUIModFilter.NotInstalled:
                    return Modules.Count(m => !m.IsInstalled);
                case GUIModFilter.Incompatible:
                    return Modules.Count(m => m.IsIncompatible);
            }
            throw new Kraken("Unknown filter type in CountModsByFilter");
        }

        public static IEnumerable<DataGridViewRow> ConstructModList(IEnumerable<GUIMod> modules)
        {
            var output = new List<DataGridViewRow>();
            foreach (var mod in modules)
            {
                var item = new DataGridViewRow {Tag = mod};

                var installedCell = mod.IsInstallable()
                    ? (DataGridViewCell) new DataGridViewCheckBoxCell()
                    : new DataGridViewTextBoxCell();
                installedCell.Value = mod.IsIncompatible
                    ? "-"
                    : (!mod.IsAutodetected ? (object) mod.IsInstalled : "AD");

                var updateCell = !mod.IsInstallable() || !mod.HasUpdate
                    ? (DataGridViewCell) new DataGridViewTextBoxCell()
                    : new DataGridViewCheckBoxCell();
                updateCell.Value = !mod.IsInstallable() || !mod.HasUpdate
                    ? "-"
                    : (object) false;

                var nameCell = new DataGridViewTextBoxCell {Value = mod.Name};
                var authorCell = new DataGridViewTextBoxCell {Value = mod.Authors};
                var installedVersionCell = new DataGridViewTextBoxCell {Value = mod.InstalledVersion};
                var latestVersionCell = new DataGridViewTextBoxCell {Value = mod.LatestVersion};
                var descriptionCell = new DataGridViewTextBoxCell {Value = mod.Abstract};
                var homepageCell = new DataGridViewLinkCell {Value = mod.Homepage};

                item.Cells.AddRange(installedCell, updateCell,
                    nameCell, authorCell,
                    installedVersionCell, latestVersionCell,
                    descriptionCell, homepageCell);

                installedCell.ReadOnly = !mod.IsInstallable();
                updateCell.ReadOnly = !mod.IsInstallable() || !mod.HasUpdate;

                output.Add(item);
            }
            return output;
        }

        private bool IsNameInNameFilter(GUIMod mod)
        {
            return mod.Name.IndexOf(ModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private bool IsModInFilter(GUIMod m)
        {
            switch (ModFilter)
            {
                case GUIModFilter.All:
                    return !m.IsIncompatible;
                case GUIModFilter.Installed:
                    return m.IsInstalled;
                case GUIModFilter.InstalledUpdateAvailable:
                    return m.IsInstalled && m.HasUpdate;
                case GUIModFilter.NewInRepository:
                    return true;
                case GUIModFilter.NotInstalled:
                    return !m.IsInstalled;
                case GUIModFilter.Incompatible:
                    return m.IsIncompatible;
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