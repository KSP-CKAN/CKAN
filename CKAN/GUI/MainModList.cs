using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CKAN
{

    public partial class Main : Form
    {

        private GUIModFilter m_ModFilter = GUIModFilter.All;
        private string m_ModNameFilter = "";

        // this functions computes a changeset from the user's choices in the GUI
        private List<KeyValuePair<CkanModule, GUIModChangeType>> ComputeChangeSetFromModList() // this probably needs to be refactored
        {
            HashSet<KeyValuePair<CkanModule, GUIModChangeType>> changeset = new HashSet<KeyValuePair<CkanModule, GUIModChangeType>>();

            // these are the lists
            var modulesToInstall = new HashSet<string>();
            var modulesToRemove = new HashSet<string>();

            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = (CkanModule)row.Tag;
                if (mod == null)
                {
                    continue;
                }

                var isInstalled = RegistryManager.Instance().registry.IsInstalled(mod.identifier);
                var isInstalledCell = row.Cells[0] as DataGridViewCheckBoxCell;
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

            RelationshipResolver resolver = null;
            try
            {
                resolver = new RelationshipResolver(modulesToInstall.ToList(), options);
            }
            catch (Exception)
            {
                return null;
            }

            foreach (CkanModule mod in resolver.ModList())
            {
                changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Install));
            }

            var installer = new ModuleInstaller();

            List<string> reverseDependencies = new List<string>();

            foreach (var moduleName in modulesToRemove)
            {
                reverseDependencies = installer.FindReverseDependencies(moduleName);
                foreach (var reverseDependency in reverseDependencies)
                {
                    var mod = RegistryManager.Instance().registry.available_modules[reverseDependency].Latest();
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>((CkanModule)mod, GUIModChangeType.Remove));
                }
            }

            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = (CkanModule)row.Tag;
                if (mod == null)
                {
                    continue;
                }

                var isInstalled = RegistryManager.Instance().registry.IsInstalled(mod.identifier);
                var isInstalledCell = row.Cells[0] as DataGridViewCheckBoxCell;
                var isInstalledChecked = (bool)isInstalledCell.Value;

                if (isInstalled && !isInstalledChecked)
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Remove));
                }
                else if (isInstalled && isInstalledChecked && mod.version.IsGreaterThan(RegistryManager.Instance().registry.InstalledVersion(mod.identifier)))
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Update));
                }
            }

            return changeset.ToList();
        }

        private int CountModsByFilter(GUIModFilter filter)
        {
            List<CkanModule> modules = RegistryManager.Instance().registry.Available();

            int count = modules.Count();

            // filter by left menu selection
            switch (filter)
            {
                case GUIModFilter.All:
                    break;
                case GUIModFilter.Installed:
                    count -= modules.Count(m => !RegistryManager.Instance().registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.InstalledUpdateAvailable:
                    count -= modules.Count
                    (
                        m => !(RegistryManager.Instance().registry.IsInstalled(m.identifier) &&
                            m.version.IsGreaterThan(RegistryManager.Instance().registry.InstalledVersion(m.identifier)))
                    );
                    break;
                case GUIModFilter.NewInRepository:
                    break;
                case GUIModFilter.NotInstalled:
                    count -= modules.RemoveAll(m => RegistryManager.Instance().registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.Incompatible:
                    count = RegistryManager.Instance().registry.Incompatible().Count();
                    break;
            }

            return count;
        }

        private List<CkanModule> GetModsByFilter(GUIModFilter filter)
        {
            List<CkanModule> modules = RegistryManager.Instance().registry.Available();

            // filter by left menu selection
            switch (m_ModFilter)
            {
                case GUIModFilter.All:
                    break;
                case GUIModFilter.Installed:
                    modules.RemoveAll(m => !RegistryManager.Instance().registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.InstalledUpdateAvailable:
                    modules.RemoveAll
                    (
                        m => !(RegistryManager.Instance().registry.IsInstalled(m.identifier) &&
                            m.version.IsGreaterThan(RegistryManager.Instance().registry.InstalledVersion(m.identifier)))
                    );
                    break;
                case GUIModFilter.NewInRepository:
                    break;
                case GUIModFilter.NotInstalled:
                    modules.RemoveAll(m => RegistryManager.Instance().registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.Incompatible:
                    modules = RegistryManager.Instance().registry.Incompatible();
                    break;
            }

            return modules;
        }

        private void UpdateModFilterList()
        {
            FilterToolButton.DropDownItems[0].Text = String.Format("All ({0})", CountModsByFilter(GUIModFilter.All));
            FilterToolButton.DropDownItems[1].Text = String.Format("Installed ({0})", CountModsByFilter(GUIModFilter.Installed));
            FilterToolButton.DropDownItems[2].Text = String.Format("Updated ({0})", CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));
            FilterToolButton.DropDownItems[3].Text = String.Format("New in repository ({0})", CountModsByFilter(GUIModFilter.NewInRepository));
            FilterToolButton.DropDownItems[4].Text = String.Format("Not installed ({0})", CountModsByFilter(GUIModFilter.NotInstalled));
            FilterToolButton.DropDownItems[5].Text = String.Format("Incompatible ({0})", CountModsByFilter(GUIModFilter.Incompatible));
        }

        public void UpdateModsList(bool markUpdates = false)
        {
            if (ModList.InvokeRequired)
            {
                ModList.Invoke(new MethodInvoker(delegate
                {
                    _UpdateModsList(markUpdates);
                }));
            }
            else
            {
                _UpdateModsList(markUpdates);
            }
        }

        private void _UpdateModsList(bool markUpdates)
        {
            ModList.Rows.Clear();

            var modules = GetModsByFilter(m_ModFilter);

            // filter by left menu selection
            switch (m_ModFilter)
            {
                case GUIModFilter.All:
                    break;
                case GUIModFilter.Installed:
                    modules.RemoveAll(m => !RegistryManager.Instance().registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.InstalledUpdateAvailable:
                    modules.RemoveAll
                    (
                        m => !(RegistryManager.Instance().registry.IsInstalled(m.identifier) &&
                            m.version.IsGreaterThan(RegistryManager.Instance().registry.InstalledVersion(m.identifier)))
                    );
                    break;
                case GUIModFilter.NewInRepository:
                    break;
                case GUIModFilter.NotInstalled:
                    modules.RemoveAll(m => RegistryManager.Instance().registry.IsInstalled(m.identifier));
                    break;
                case GUIModFilter.Incompatible:
                    break;
            }

            // filter by name
            modules.RemoveAll(m => !m.name.ToLowerInvariant().Contains(m_ModNameFilter.ToLowerInvariant()));

            foreach (CkanModule mod in modules)
            {
                DataGridViewRow item = new DataGridViewRow();
                item.Tag = mod;

                var isInstalled = RegistryManager.Instance().registry.IsInstalled(mod.identifier);

                // installed
                if (m_ModFilter != GUIModFilter.Incompatible)
                {
                    var installedCell = new DataGridViewCheckBoxCell();
                    installedCell.Value = isInstalled;
                    item.Cells.Add(installedCell);
                }
                else
                {
                    var installedCell = new DataGridViewTextBoxCell();
                    installedCell.Value = "-";
                    item.Cells.Add(installedCell);
                }

                // want update
                if (!isInstalled)
                {
                    var updateCell = new DataGridViewTextBoxCell();
                    item.Cells.Add(updateCell);
                    updateCell.ReadOnly = true;
                    updateCell.Value = "-";
                }
                else
                {
                    var isUpToDate = !RegistryManager.Instance().registry.InstalledVersion(mod.identifier).IsLessThan(mod.version);
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
                    var authors = "";
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
                var installedVersion = RegistryManager.Instance().registry.InstalledVersion(mod.identifier);
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
                var latestVersion = mod.version;
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
                var kspVersion = mod.ksp_version;
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

                try
                {
                    homepageCell.Value = mod.resources["homepage"];

                }
                catch (Exception)
                {
                    homepageCell.Value = "N/A";
                }
                item.Cells.Add(homepageCell);

                ModList.Rows.Add(item);
            }
        }

    }

}
