using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CKAN
{
    public partial class Main : Form
    {
        private GUIModFilter m_ModFilter = GUIModFilter.All;
        private string m_ModNameFilter = "";
        private List<CkanModule> _modules = new List<CkanModule>();


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
                var isInstalledChecked = (bool) isInstalledCell.Value;
                DataGridViewCell shouldBeUpdatedCell = row.Cells[1];
                bool shouldBeUpdated = false;
                if (shouldBeUpdatedCell is DataGridViewCheckBoxCell && shouldBeUpdatedCell.Value != null)
                {
                    shouldBeUpdated = (bool) shouldBeUpdatedCell.Value;
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

        private static int CountModsByFilter(Registry registry, IEnumerable<CkanModule> modules, GUIModFilter filter)
        {
            switch (filter)                {
                    case GUIModFilter.All:
                        return modules.Count();                        
                    case GUIModFilter.Installed:
                        return modules.Count(m => registry.IsInstalled(m.identifier));                        
                    case GUIModFilter.InstalledUpdateAvailable:
                        return modules.Count
                            (
                                m => registry.IsInstalled(m.identifier) &&
                                       m.version.IsGreaterThan(registry.InstalledVersion(m.identifier))
                            );                        
                    case GUIModFilter.NewInRepository:
                        return modules.Count();                        
                    case GUIModFilter.NotInstalled:
                        return modules.Count(m => !registry.IsInstalled(m.identifier));                        
                    case GUIModFilter.Incompatible:
                        return registry.Incompatible().Count;
                        
            }
            throw new Kraken("Attempted to filter by unknown filter in CountModsByFilter");          
        }

        private List<CkanModule> GetModsByFilter(GUIModFilter filter)
        {

            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            List<CkanModule> modules = registry.Available();

            // filter by left menu selection
            switch (m_ModFilter)
            {
                case GUIModFilter.All:
                    break;
                case GUIModFilter.Installed:
                    modules.RemoveAll(m => !registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.InstalledUpdateAvailable:
                    modules.RemoveAll
                        (
                            m => !(registry.IsInstalled(m.identifier) &&
                                   m.version.IsGreaterThan(
                                       registry.InstalledVersion(m.identifier)))
                        );
                    break;
                case GUIModFilter.NewInRepository:
                    break;
                case GUIModFilter.NotInstalled:
                    modules.RemoveAll(m => registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.Incompatible:
                    modules = registry.Incompatible();
                    break;
            }

            return modules;
        }

        private void UpdateModFilterList()
        {
            if (menuStrip2.InvokeRequired)
            {
                menuStrip2.Invoke(new MethodInvoker(delegate { _UpdateModFilterList(); }));
            }
            else
            {
                _UpdateModFilterList();
            }
        }

        private void _UpdateModFilterList()
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;
            var modules = registry.Available();            
            FilterToolButton.DropDownItems[0].Text = String.Format("All ({0})",
                CountModsByFilter(registry, modules, GUIModFilter.All));

            FilterToolButton.DropDownItems[1].Text = String.Format("Installed ({0})",
                CountModsByFilter(registry, modules, GUIModFilter.Installed));

            FilterToolButton.DropDownItems[2].Text = String.Format("Updated ({0})",
                CountModsByFilter(registry, modules, GUIModFilter.InstalledUpdateAvailable));

            FilterToolButton.DropDownItems[3].Text = String.Format("New in repository ({0})",
                CountModsByFilter(registry, modules, GUIModFilter.NewInRepository));

            FilterToolButton.DropDownItems[4].Text = String.Format("Not installed ({0})",
                CountModsByFilter(registry, modules, GUIModFilter.NotInstalled));

            FilterToolButton.DropDownItems[5].Text = String.Format("Incompatible ({0})",
                CountModsByFilter(registry, modules, GUIModFilter.Incompatible));
        }

        public void UpdateModsList(bool markUpdates = false)
        {
            Util.Invoke(this, () => _UpdateModsList(markUpdates));
        }

        private void ChangeFilterText()
        {

            Util.Invoke(this, () =>
            {
                var registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;
                var ckanModules =
                    _modules.Where(
                        m => m.name.IndexOf(m_ModNameFilter, StringComparison.InvariantCultureIgnoreCase) >= 0);
                SetModlistToModules(ckanModules, registry);
            });
        }
    

    private void _UpdateModsList(bool markUpdates)
        {
            Registry registry = RegistryManager.Instance(KSPManager.CurrentInstance).registry;

            ModList.Rows.Clear();

            _modules = GetModsByFilter(m_ModFilter);

            // filter by left menu selection
            switch (m_ModFilter)
            {
                case GUIModFilter.All:
                    break;
                case GUIModFilter.Installed:
                    _modules.RemoveAll(m => !registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.InstalledUpdateAvailable:
                    _modules.RemoveAll
                        (
                            m => !(registry.IsInstalled(m.identifier) &&
                                   m.version.IsGreaterThan(
                                       registry.InstalledVersion(m.identifier)))
                        );
                    break;
                case GUIModFilter.NewInRepository:
                    break;
                case GUIModFilter.NotInstalled:
                    _modules.RemoveAll(m => registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.Incompatible:
                    break;
            }

            // filter by name            
            SetModlistToModules(_modules.Where(m => m.name.IndexOf(m_ModNameFilter, StringComparison.InvariantCultureIgnoreCase) >= 0), registry);

        }
        private void SetModlistToModules(IEnumerable<CkanModule> modules, Registry registry)
        {
            ModList.Rows.Clear();
            foreach (CkanModule mod in modules)
            {
                var item = new DataGridViewRow { Tag = mod };

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

                if (m_ModFilter != GUIModFilter.Incompatible)
                {
                    if (!isAutodetected)
                    {
                        var installedCell = new DataGridViewCheckBoxCell { Value = isInstalled };
                        item.Cells.Add(installedCell);
                    }
                    else
                    {
                        var installedCell = new DataGridViewTextBoxCell { Value = "AD" };
                        item.Cells.Add(installedCell);
                    }
                }
                else
                {
                    var installedCell = new DataGridViewTextBoxCell { Value = "-" };
                    item.Cells.Add(installedCell);
                }

                // want update
                if (!isInstalled || isAutodetected)
                {
                    var updateCell = new DataGridViewTextBoxCell { Value = "-" };
                    item.Cells.Add(updateCell);
                    updateCell.ReadOnly = true;
                }
                else
                {
                    bool isUpToDate = !registry.InstalledVersion(mod.identifier).IsLessThan(mod.version);
                    if (!isUpToDate)
                    {
                        var updateCell = new DataGridViewCheckBoxCell();
                        item.Cells.Add(updateCell);
                        updateCell.ReadOnly = false;
                    }
                    else
                    {
                        var updateCell = new DataGridViewTextBoxCell { Value = "-" };
                        item.Cells.Add(updateCell);
                        updateCell.ReadOnly = true;
                    }
                }

                // name
                var nameCell = new DataGridViewTextBoxCell { Value = mod.name };
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
                var installedVersionCell = new DataGridViewTextBoxCell
                {
                    Value = installedVersion != null ? installedVersion.ToString() : "-"
                };

                item.Cells.Add(installedVersionCell);

                // latest version
                Version latestVersion = mod.version;
                var latestVersionCell = new DataGridViewTextBoxCell
                {
                    Value = latestVersion != null ? latestVersion.ToString() : "-"
                };
                item.Cells.Add(latestVersionCell);

                // KSP version
                KSPVersion kspVersion = mod.ksp_version;
                var kspVersionCell = new DataGridViewTextBoxCell
                {
                    Value = kspVersion != null ? kspVersion.ToString() : "-"
                };
                item.Cells.Add(kspVersionCell);

                // description
                var descriptionCell = new DataGridViewTextBoxCell { Value = mod.@abstract };
                item.Cells.Add(descriptionCell);

                // homepage
                var homepageCell = new DataGridViewLinkCell
                {
                    Value = mod.resources != null && mod.resources.homepage != null
                        ? (object)mod.resources.homepage
                        : "N/A"
                };
                item.Cells.Add(homepageCell);

                ModList.Rows.Add(item);
            }
            // sort by name
            ModList.Sort(ModList.Columns[2], ListSortDirection.Ascending);
            ModList.Refresh();
        }
    }

}