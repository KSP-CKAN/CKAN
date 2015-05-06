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
            // To make the filtering process faster, Copy the list of rows. Filter out the hidden and replace t
            // rows in DataGridView.
            var rows = new DataGridViewRow[mainModList.FullListOfModRows.Count];
            mainModList.FullListOfModRows.CopyTo(rows, 0);
            ModList.Rows.Clear();

            foreach (var row in rows)
            {
                var mod = ((GUIMod)row.Tag);
                row.Visible = mainModList.IsVisible(mod);
            }
            
            ModList.Rows.AddRange(rows.Where(row=>row.Visible).OrderBy(row=>((GUIMod)row.Tag).Name).ToArray());
            
        }

        private void UpdateModsList(Boolean repo_updated=false)
        {
            Util.Invoke(this, ()=>_UpdateModsList(repo_updated));
        }


        private void _UpdateModsList(bool repo_updated)
        {
            Registry registry = RegistryManager.Instance(CurrentInstance).registry;
            
            var ckanModules = registry.Available(CurrentInstance.Version()).Concat(
                registry.Incompatible(CurrentInstance.Version())).ToList();
            var gui_mods = new HashSet<GUIMod>(ckanModules.Select(m => new GUIMod(m, registry, CurrentInstance.Version())));            
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
            var rows = mainModList.ConstructModList(mainModList.Modules);
            ModList.Rows.Clear();
            ModList.Rows.AddRange(rows.ToArray());
            ModList.Sort(ModList.Columns[2], ListSortDirection.Ascending);

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

        internal List<DataGridViewRow> FullListOfModRows;
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

        /// <summary>
        /// This function returns a changeset based on the selections of the user. 
        /// Currently returns null if a conflict is detected.        
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="current_instance"></param>
        public List<KeyValuePair<CkanModule, GUIModChangeType>> ComputeChangeSetFromModList(Registry registry, KSP current_instance)
        {
            var changeset = new HashSet<KeyValuePair<CkanModule, GUIModChangeType>>();
            var modulesToInstall = new HashSet<string>();
            var modulesToRemove = new HashSet<string>();

            foreach (var mod in Modules.Where(mod => mod.IsInstallable()))
            {
                if (mod.IsInstalled)
                {
                    if (!mod.IsInstallChecked)
                    {
                        modulesToRemove.Add(mod.Identifier);
                        changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod.ToCkanModule(),
                            GUIModChangeType.Remove));
                    }
                    else if (mod.IsInstallChecked && mod.HasUpdate && mod.IsUpgradeChecked)
                    {
                        changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod.ToCkanModule(),
                            GUIModChangeType.Update));
                    }
                }
                else if (mod.IsInstallChecked)
                {
                    modulesToInstall.Add(mod.Identifier);
                }
            }

            RelationshipResolverOptions options = RelationshipResolver.DefaultOpts();
            options.with_recommends = false;
            options.without_toomanyprovides_kraken = true;
            options.without_enforce_consistency = true;

            RelationshipResolver resolver;
            try
            {
                resolver = new RelationshipResolver(modulesToInstall.ToList(), options, registry, current_instance.Version());
            }
            catch (Exception)
            {
                //TODO FIX this so the UI reacts.
                return null;
            }

            changeset.UnionWith(
                resolver.ModList()
                    .Select(mod => new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Install)));


            ModuleInstaller installer = ModuleInstaller.GetInstance(current_instance, GUI.user);

            foreach (var reverseDependencies in modulesToRemove.Select(mod => installer.FindReverseDependencies(mod)))
            {
                //TODO This would be a good place to have a event that alters the row's graphics to show it will be removed

                //TODO This currently gets the latest version. This may cause the displayed version to wrong in the changset. 
                var modules = reverseDependencies.Select(rDep => registry.LatestAvailable(rDep, null));
                changeset.UnionWith(
                    modules.Select(mod => new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Remove)));
            }

            return changeset.ToList();
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
                    return Modules.Count(m=>m.IsNew);
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
            FullListOfModRows = new List<DataGridViewRow>();
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

                FullListOfModRows.Add(item);
            }
            return FullListOfModRows;
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
    }
}