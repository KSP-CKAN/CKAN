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
            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = ((GUIMod)row.Tag);
                var isVisible = mainModList.IsVisible(mod);
                row.Visible = isVisible;
            }
        }

        public void UpdateModsList(bool markUpdates = false)
        {
            Util.Invoke(this, () => _UpdateModsList(markUpdates, mainModList));
        }

        private void _UpdateModsList(bool markUpdates, MainModList mainModList)
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            var ckanModules = registry.Available().Concat(registry.Incompatible());
            var guiMods = ckanModules.Select(m=>new GUIMod(m,registry)).ToList();
            mainModList.Modules = new ReadOnlyCollection<GUIMod>(guiMods);
            
            var rows = MainModList.ConstructModList(mainModList.Modules);
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

    public class GUIMod
    {
        private CkanModule Mod { get; set; }
        public string Name{ get { return Mod.name; }}
        public bool IsInstalled { get; private set; }
        public bool HasUpdate { get; private set; }
        public bool IsIncompatible { get; private set; }
        public bool IsAutodetected { get; private set; }
        public string Authors { get; private set; }
        public string InstalledVersion { get; private set; }
        public string LatestVersion { get; private set; }
        public string KSPversion { get; private set; }
        public string Abstract { get; private set; }
        public object Homepage { get; private set; }
        public string Identifier { get; private set; }
        public bool IsInstallChecked { get; set; }
        public bool IsUpgradeChecked { get; set; }


        public GUIMod(CkanModule mod, Registry registry)
        {
            Mod = mod;
            IsInstalled = registry.IsInstalled(mod.identifier);
            IsInstallChecked = IsInstalled;
            HasUpdate = IsInstalled && Mod.version.IsGreaterThan(registry.InstalledVersion(mod.identifier));
            IsIncompatible = !registry.IsCompatible(mod.identifier);
            //TODO Remove magic values
            IsAutodetected = IsInstalled && registry.InstalledVersion(mod.identifier).ToString().Equals("autodetected dll");
            Authors = mod.author == null ? "N/A" : String.Join(",", mod.author);
            
            var installedVersion = registry.InstalledVersion(mod.identifier);
            var latestVersion = mod.version;
            var kspVersion = mod.ksp_version;

            InstalledVersion = installedVersion != null ? installedVersion.ToString() : "-";            
            LatestVersion = latestVersion != null ? latestVersion.ToString() : "-";            
            KSPversion = kspVersion != null ? kspVersion.ToString() : "-";

            Abstract = mod.@abstract;
            Homepage = mod.resources != null && mod.resources.homepage != null
                ? (object) mod.resources.homepage
                : "N/A";

            Identifier = mod.identifier;
        }

        public CkanModule ToCkanModule()
        {
            return Mod;
        }
    }
    public class MainModList
    {
        public delegate void ModFiltersUpdatedEvent(MainModList source);
        public event ModFiltersUpdatedEvent ModFiltersUpdated;

        public MainModList(ModFiltersUpdatedEvent onModFiltersUpdated)
        {                        
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

        public ReadOnlyCollection<GUIMod> Modules
        {
            set { _modules = value; }
            get { return _modules; }
        }

        private GUIModFilter _modFilter = GUIModFilter.All;
        private string _modNameFilter = "";
        private ReadOnlyCollection<GUIMod> _modules;

        // this functions computes a changeset from the user's choices in the GUI
        public List<KeyValuePair<CkanModule, GUIModChangeType>> ComputeChangeSetFromModList()
        {
            var changeset = new HashSet<KeyValuePair<CkanModule, GUIModChangeType>>();

            // these are the lists
            var modulesToInstall = new HashSet<string>();
            var modulesToRemove = new HashSet<string>();

            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            foreach (var mod in _modules.Where(mod => mod.IsInstallable()))
            {
                if (mod.IsInstalled)
                {
                    if (!mod.IsInstallChecked)
                    {
                        modulesToRemove.Add(mod.Identifier);
                        changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod.ToCkanModule(), GUIModChangeType.Remove));
                    }
                    else if (mod.IsInstallChecked && mod.HasUpdate && mod.IsUpgradeChecked)
                    {
                        changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod.ToCkanModule(), GUIModChangeType.Update));
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
                resolver = new RelationshipResolver(modulesToInstall.ToList(), options, registry);
            }
            catch (Exception)
            {                
                return null;
            }

            changeset.UnionWith(resolver.ModList().Select(mod => new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Install)));
            
            var installer = ModuleInstaller.Instance;
            foreach (var reverseDependencies in modulesToRemove.Select(installer.FindReverseDependencies))
            {
                //TODO This would be a good place to have a event that alters the row's graphics to show it will be removed
                var modules = reverseDependencies.Select(rDep => registry.LatestAvailable(rDep));
                changeset.UnionWith(modules.Select(mod => new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Remove)));                
            }

            return changeset.ToList();
        }

        public int CountModsByFilter(GUIModFilter filter)
        {            
            switch (filter)
            {
                case GUIModFilter.All:
                    return _modules.Count(m=>!m.IsIncompatible);
                case GUIModFilter.Installed:
                    return _modules.Count(m => m.IsInstalled);
                case GUIModFilter.InstalledUpdateAvailable:
                    return _modules.Count(m=>m.HasUpdate);
                case GUIModFilter.NewInRepository:
                    return _modules.Count();
                case GUIModFilter.NotInstalled:
                    return _modules.Count(m => !m.IsInstalled);
                case GUIModFilter.Incompatible:
                    return _modules.Count(m => m.IsIncompatible);
            }
            throw new Kraken("Unknown filter type in CountModsByFilter");
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
                
                
                // want update
                DataGridViewCell updateCell = !mod.IsInstallable() || !mod.HasUpdate
                    ? (DataGridViewCell) new DataGridViewTextBoxCell()
                    : new DataGridViewCheckBoxCell();
                updateCell.Value = !mod.IsInstallable() || !mod.HasUpdate
                    ? "-"
                    : (object) false;                
                
                var nameCell = new DataGridViewTextBoxCell {Value = mod.Name};
                var authorCell = new DataGridViewTextBoxCell {Value = mod.Authors};
                var installedVersionCell = new DataGridViewTextBoxCell{Value = mod.InstalledVersion};
                var latestVersionCell = new DataGridViewTextBoxCell{Value = mod.LatestVersion};
                var kspVersionCell = new DataGridViewTextBoxCell{Value = mod.KSPversion};
                var descriptionCell = new DataGridViewTextBoxCell {Value = mod.Abstract};
                var homepageCell = new DataGridViewLinkCell { Value = mod.Homepage};

                item.Cells.AddRange(installedCell, updateCell,
                    nameCell, authorCell, 
                    installedVersionCell, latestVersionCell, kspVersionCell, 
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

        public bool IsVisible(GUIMod mod)
        {
            var nameMatchesFilter = IsNameInNameFilter(mod);
            var modMatchesType = IsModInFilter(mod);
            var isVisible = nameMatchesFilter && modMatchesType;
            return isVisible;
        }
    }     
}