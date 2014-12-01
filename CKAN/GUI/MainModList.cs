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
            Util.Invoke(control, () => _UpdateFilters(mainModList));
        }

        private void _UpdateFilters(MainModList mainModList)
        {
            foreach (DataGridViewRow row in mainModList.Modlist.Rows)
            {
                var mod = (CkanModule)row.Tag;
                var nameMatchesFilter = mainModList.IsNameInFilter(mod);
                var modMatchesType = mainModList.IsModInFilter(mod);
                row.Visible = nameMatchesFilter && modMatchesType;
            }
        }

        public void UpdateModsList(bool markUpdates = false)
        {
            Util.Invoke(this, () => _UpdateModsList(markUpdates, mainModList));
        }

        private void _UpdateModsList(bool markUpdates, MainModList mainModList)
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            mainModList.Modules = new ReadOnlyCollection<CkanModule>(registry.Available());
            
            var rows = mainModList.ConstructModList(mainModList.Modules, registry, mainModList.Modlist);
            //rows.Sort();
            ModList.Rows.Clear();
            ModList.Rows.AddRange(rows.ToArray());
            ModList.Sort(ModList.Columns[2],ListSortDirection.Ascending);
            
            FilterToolButton.DropDownItems[0].Text = String.Format("All ({0})", mainModList.CountModsByFilter(GUIModFilter.All));

            FilterToolButton.DropDownItems[1].Text = String.Format("Installed ({0})", mainModList.CountModsByFilter(GUIModFilter.Installed));

            FilterToolButton.DropDownItems[2].Text = String.Format("Updated ({0})", mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));

            FilterToolButton.DropDownItems[3].Text = String.Format("New in repository ({0})", mainModList.CountModsByFilter(GUIModFilter.NewInRepository));

            FilterToolButton.DropDownItems[4].Text = String.Format("Not installed ({0})", mainModList.CountModsByFilter(GUIModFilter.NotInstalled));

            FilterToolButton.DropDownItems[5].Text = String.Format("Incompatible ({0})", mainModList.CountModsByFilter(GUIModFilter.Incompatible));

            UpdateFilters(this);
        }
    }

    public class MainModList
    {        
        public DataGridView Modlist { get; private set; }        

        public delegate void ModFiltersUpdatedEvent(MainModList source);
        public event ModFiltersUpdatedEvent ModFiltersUpdated;

        public MainModList(DataGridView modlist, ModFiltersUpdatedEvent onModFiltersUpdated)
        {            
            Modlist = modlist;     
            ModFiltersUpdated += onModFiltersUpdated;
            ModFiltersUpdated(this);
        }

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
            private get { return _modNameFilter; }
            set
            {
                var old = _modNameFilter;
                _modNameFilter = value;
                if (!old.Equals(value)) ModFiltersUpdated(this);
            }
        }

        public ReadOnlyCollection<CkanModule> Modules
        {
            set { _modules = value; }
            get { return _modules; }
        }

        private GUIModFilter _modFilter = GUIModFilter.All;
        private string _modNameFilter = "";
        private ReadOnlyCollection<CkanModule> _modules;

        // this functions computes a changeset from the user's choices in the GUI
        public List<KeyValuePair<CkanModule, GUIModChangeType>> ComputeChangeSetFromModList()
        // this probably needs to be refactored
        {
            var changeset = new HashSet<KeyValuePair<CkanModule, GUIModChangeType>>();

            // these are the lists
            var modulesToInstall = new HashSet<string>();
            var modulesToRemove = new HashSet<string>();

            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            foreach (DataGridViewRow row in Modlist.Rows)
            {
                var mod = (CkanModule)row.Tag;
                if (mod == null)
                {
                    continue;
                }

                bool isInstalled = registry.IsInstalled(mod.identifier);
                var isInstalledCell = row.Cells[0] as DataGridViewCheckBoxCell;
                if (isInstalledCell == null) continue; //Ignore Ad mods
                var isInstalledChecked = (bool)isInstalledCell.Value;

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

            foreach (DataGridViewRow row in Modlist.Rows)
            {
                var mod = (CkanModule)row.Tag;
                if (mod == null)
                {
                    continue;
                }

                bool isInstalled = registry.IsInstalled(mod.identifier);
                var isInstalledCell = row.Cells[0] as DataGridViewCheckBoxCell;
                if (isInstalledCell == null) continue; //Ignore Ad mods
                var isInstalledChecked = (bool)isInstalledCell.Value;
                DataGridViewCell shouldBeUpdatedCell = row.Cells[1];
                bool shouldBeUpdated = false;
                if (shouldBeUpdatedCell is DataGridViewCheckBoxCell && shouldBeUpdatedCell.Value != null)
                {
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

        public int CountModsByFilter(GUIModFilter filter)
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            switch (filter)
            {
                case GUIModFilter.All:
                    return _modules.Count();
                case GUIModFilter.Installed:
                    return _modules.Count(m => registry.IsInstalled(m.identifier));
                case GUIModFilter.InstalledUpdateAvailable:
                    return _modules.Count
                        (
                            m => registry.IsInstalled(m.identifier) &&
                                   m.version.IsGreaterThan(registry.InstalledVersion(m.identifier))
                        );
                case GUIModFilter.NewInRepository:
                    return _modules.Count();
                case GUIModFilter.NotInstalled:
                    return _modules.Count(m => !registry.IsInstalled(m.identifier));
                case GUIModFilter.Incompatible:
                    return registry.Incompatible().Count;
            }
            throw new Kraken("Unknown filter type in CountModsByFilter");
        }

        public bool IsModInFilter(Module m)
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;
            var id = m.identifier;
            switch (ModFilter)
            {
                case GUIModFilter.All:
                    return true;
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
                    return !registry.IsCompatible(id);
            }
            throw new Kraken("Unknown filter type in IsModInFilter");
        }

        public IEnumerable<DataGridViewRow> ConstructModList(IEnumerable<CkanModule> modules, Registry registry, DataGridView dataGridView)
        {
            
            var output = new List<DataGridViewRow>();
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
                if (ModFilter != GUIModFilter.Incompatible)
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

                output.Add(item);
            }
            // sort by name
            return output;
        }

        public bool IsNameInFilter(CkanModule mod)
        {
            return mod.name.IndexOf(ModNameFilter, StringComparison.InvariantCultureIgnoreCase) != -1;
        }
    }     
}