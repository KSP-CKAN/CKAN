using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main : Form
    {
        public GUIModFilter ModFilter
        {
            get { return _modFilter; }
            set
            {
                var old = _modFilter;
                _modFilter = value;
                if(!old.Equals(value)) UpdateFilters();
            }
        }

        public string ModNameFilter
        {
            get { return _modNameFilter; }
            set
            {
                var old = _modNameFilter;
                _modNameFilter = value;
                if (!old.Equals(value)) UpdateFilters();                
            }
        }
        private GUIModFilter _modFilter = GUIModFilter.All;
        private string _modNameFilter = "";
        private ReadOnlyCollection<CkanModule> _availbleModules;
        private ReadOnlyCollection<CkanModule> _incompatableModules;

        // this functions computes a changeset from the user's choices in the GUI
        private List<KeyValuePair<CkanModule, GUIModChangeType>> ComputeChangeSetFromModList()
            // this probably needs to be refactored
        {
            var changeset = new HashSet<KeyValuePair<CkanModule, GUIModChangeType>>();

            // these are the lists
            var modulesToInstall = new HashSet<string>();
            var modulesToRemove = new HashSet<string>();

            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = (CkanModule) row.Tag;
                if (mod == null)
                {
                    continue;
                }

                bool isInstalled = registry.IsInstalled(mod.identifier);
                var isInstalledCell = row.Cells[0] as DataGridViewCheckBoxCell;
                if(isInstalledCell==null) continue; //Ignore Ad mods
                var isInstalledChecked = (bool) isInstalledCell.Value;

                if (!isInstalled && isInstalledChecked)
                {
                    modulesToInstall.Add(mod.identifier);
                }
                else if (isInstalled && !isInstalledChecked)
                {
                    modulesToRemove.Add(mod.identifier);
                }
            }

            RelationshipResolverOptions options = RelationshipResolver.DefaultOpts();
            options.with_recommends = false;
            options.without_toomanyprovides_kraken = true;
            options.without_enforce_consistency = true;

            RelationshipResolver resolver = null;
            try
            {
                resolver = new RelationshipResolver(modulesToInstall.ToList(), options, registry);
            }
            catch (Exception e)
            {
                return null;
            }

            foreach (CkanModule mod in resolver.ModList())
            {
                changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Install));
            }

            ModuleInstaller installer = ModuleInstaller.Instance;

            foreach (string moduleName in modulesToRemove)
            {
                var reverseDependencies = installer.FindReverseDependencies(moduleName);
                foreach (string reverseDependency in reverseDependencies)
                {
                    CkanModule mod = registry.LatestAvailable(reverseDependency);
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Remove));
                }
            }

            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = (CkanModule) row.Tag;
                if (mod == null)
                {
                    continue;
                }

                bool isInstalled = registry.IsInstalled(mod.identifier);
                var isInstalledCell = row.Cells[0] as DataGridViewCheckBoxCell;
                if (isInstalledCell == null) continue; //Ignore Ad mods
                var isInstalledChecked = (bool) isInstalledCell.Value;
                DataGridViewCell shouldBeUpdatedCell = row.Cells[1];
                bool shouldBeUpdated = false;
                if(shouldBeUpdatedCell is DataGridViewCheckBoxCell && shouldBeUpdatedCell.Value != null){
                    shouldBeUpdated = (bool)shouldBeUpdatedCell.Value;
                }

                if (isInstalled && !isInstalledChecked)
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Remove));
                }
                else if (isInstalled && isInstalledChecked &&
                         mod.version.IsGreaterThan(registry.InstalledVersion(mod.identifier)) && shouldBeUpdated)
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Update));
                }
            }

            return changeset.ToList();
        }

        private int CountModsByFilter(GUIModFilter filter)
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            switch (filter)
            {
                case GUIModFilter.All:
                    return _availbleModules.Count();
                case GUIModFilter.Installed:
                    return _availbleModules.Count(m => registry.IsInstalled(m.identifier));
                case GUIModFilter.InstalledUpdateAvailable:
                    return _availbleModules.Count
                        (
                            m => registry.IsInstalled(m.identifier) &&
                                   m.version.IsGreaterThan(registry.InstalledVersion(m.identifier))
                        );
                case GUIModFilter.NewInRepository:
                    return _availbleModules.Count();
                case GUIModFilter.NotInstalled:
                    return _availbleModules.Count(m => !registry.IsInstalled(m.identifier));                    
                case GUIModFilter.Incompatible:
                    return registry.Incompatible().Count;                    
            }
            throw new Kraken("Unknown filter type in CountModsByFilter");         
        }

        public void UpdateFilters()
        {
            Util.Invoke(this, _UpdateFilters);
        }

        private void _UpdateFilters()
        {

            foreach (DataGridViewRow row in ModList.Rows)
            {                
                var mod = (CkanModule) row.Tag;
                var nameMatchesFilter = mod.name.IndexOf(ModNameFilter,StringComparison.InvariantCultureIgnoreCase) != -1;               
                var modMatchesType = IsModInFilter(ModFilter, mod);
                row.Visible = nameMatchesFilter && modMatchesType;
            }
        }

        private bool IsModInFilter(GUIModFilter filter,Module m)
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;
            var id = m.identifier;
            switch (filter)
            {
                case GUIModFilter.All:
                    return registry.IsCompatible(dependencyGraphRootModule);                    
                case GUIModFilter.Installed:
                    return registry.IsInstalled(id);                    
                case GUIModFilter.InstalledUpdateAvailable:
                    return registry.IsInstalled(id) &&
                        m.version.IsGreaterThan(registry.InstalledVersion(id));                        
                case GUIModFilter.NewInRepository:
                    return true;                    
                case GUIModFilter.NotInstalled:
                    return !registry.IsInstalled(id);                    
                case GUIModFilter.Incompatible:
                    return _incompatableModules.Contains(m);
            }
            throw new Kraken("Unknown filter type in IsModInFilter");
        }        

        public void UpdateModsList(bool markUpdates = false)
        {
            Util.Invoke(this, () => _UpdateModsList(markUpdates));
        }

        private void _UpdateModsList(bool markUpdates)
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            _availbleModules = new ReadOnlyCollection<CkanModule>(registry.Available());
            _incompatableModules = new ReadOnlyCollection<CkanModule>(registry.Incompatible());
            ConstructModList(_availbleModules.Concat(_incompatableModules), registry);

            FilterToolButton.DropDownItems[0].Text = String.Format("All ({0})",
                CountModsByFilter(GUIModFilter.All));

            FilterToolButton.DropDownItems[1].Text = String.Format("Installed ({0})",
                CountModsByFilter(GUIModFilter.Installed));

            FilterToolButton.DropDownItems[2].Text = String.Format("Updated ({0})",
                CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));

            FilterToolButton.DropDownItems[3].Text = String.Format("New in repository ({0})",
                CountModsByFilter(GUIModFilter.NewInRepository));

            FilterToolButton.DropDownItems[4].Text = String.Format("Not installed ({0})",
                CountModsByFilter(GUIModFilter.NotInstalled));

            FilterToolButton.DropDownItems[5].Text = String.Format("Incompatible ({0})",
                _incompatableModules.Count);

            UpdateFilters();
        }

        private void ConstructModList(IEnumerable<CkanModule> modules, Registry registry)
        {
            ModList.Rows.Clear();
            foreach (CkanModule mod in modules)
            {
                var item = new DataGridViewRow();
                item.Tag = mod;

                bool isInstalled = registry.IsInstalled(mod.identifier);
                bool isAutodetected = false;
                if (isInstalled)
                {
                    isAutodetected = registry.InstalledVersion(mod.identifier).ToString() == "autodetected dll";
                }

                if (isAutodetected)
                {
                    item.DefaultCellStyle.BackColor = System.Drawing.SystemColors.InactiveCaption;
                }

                // installed
                if (registry.IsCompatible(mod))
                {
                    if (!isAutodetected)
                    {
                        var installedCell = new DataGridViewCheckBoxCell();
                        installedCell.Value = isInstalled;
                        item.Cells.Add(installedCell);
                    }
                    else
                    {
                        var installedCell = new DataGridViewTextBoxCell();
                        installedCell.Value = "AD";
                        item.Cells.Add(installedCell);
                        installedCell.ReadOnly = true;
                    }
                }
                else
                {
                    var installedCell = new DataGridViewTextBoxCell();
                    installedCell.Value = "-";
                    item.Cells.Add(installedCell);
                }

                // want update
                if (!isInstalled || isAutodetected)
                {
                    var updateCell = new DataGridViewTextBoxCell();
                    item.Cells.Add(updateCell);
                    updateCell.ReadOnly = true;
                    updateCell.Value = "-";
                }
                else
                {
                    bool isUpToDate =
                        !registry.InstalledVersion(mod.identifier).IsLessThan(mod.version);
                    if (!isUpToDate)
                    {
                        var updateCell = new DataGridViewCheckBoxCell();
                        item.Cells.Add(updateCell);
                        updateCell.ReadOnly = false;
                    }
                    else
                    {
                        var updateCell = new DataGridViewTextBoxCell();
                        item.Cells.Add(updateCell);
                        updateCell.ReadOnly = true;
                        updateCell.Value = "-";
                    }
                }

                // name
                var nameCell = new DataGridViewTextBoxCell();
                nameCell.Value = mod.name;
                item.Cells.Add(nameCell);

                // author
                var authorCell = new DataGridViewTextBoxCell();
                if (mod.author != null)
                {
                    string authors = "";
                    for (int i = 0; i < mod.author.Count(); i++)
                    {
                        authors += mod.author[i];
                        if (i != mod.author.Count() - 1)
                        {
                            authors += ", ";
                        }
                    }

                    authorCell.Value = authors;
                }
                else
                {
                    authorCell.Value = "N/A";
                }

                item.Cells.Add(authorCell);

                // installed version
                Version installedVersion = registry.InstalledVersion(mod.identifier);
                var installedVersionCell = new DataGridViewTextBoxCell();

                if (installedVersion != null)
                {
                    installedVersionCell.Value = installedVersion.ToString();
                }
                else
                {
                    installedVersionCell.Value = "-";
                }

                item.Cells.Add(installedVersionCell);

                // latest version
                Version latestVersion = mod.version;
                var latestVersionCell = new DataGridViewTextBoxCell();

                if (latestVersion != null)
                {
                    latestVersionCell.Value = latestVersion.ToString();
                }
                else
                {
                    latestVersionCell.Value = "-";
                }

                item.Cells.Add(latestVersionCell);

                // KSP version
                KSPVersion kspVersion = mod.ksp_version;
                var kspVersionCell = new DataGridViewTextBoxCell();

                if (kspVersion != null)
                {
                    kspVersionCell.Value = kspVersion.ToString();
                }
                else
                {
                    kspVersionCell.Value = "-";
                }

                item.Cells.Add(kspVersionCell);

                // description
                var descriptionCell = new DataGridViewTextBoxCell();
                descriptionCell.Value = mod.@abstract;
                item.Cells.Add(descriptionCell);

                // homepage
                var homepageCell = new DataGridViewLinkCell();

                if (mod.resources != null && mod.resources.homepage != null)
                {
                    homepageCell.Value = mod.resources.homepage;
                }
                else
                {
                    homepageCell.Value = "N/A";
                }

                item.Cells.Add(homepageCell);

                ModList.Rows.Add(item);                
            }
            // sort by name
            ModList.Sort(ModList.Columns[2], ListSortDirection.Ascending);
            ModList.Refresh();
        }
    }
}