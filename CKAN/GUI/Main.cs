using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CKAN
{

    public enum GUIModFilter
    {
        All = 0,
        Installed = 1,
        InstalledUpdateAvailable = 2,
        NewInRepository = 3,
        NotInstalled = 4,
    }

    public enum GUIModChangeType
    {
        None = 0,
        Install = 1,
        Remove = 2,
        Update = 3,
    }

    public partial class Main : Form
    {

        private WaitDialog m_WaitDialog = new WaitDialog();
        private ApplyChangesDialog m_ApplyChangesDialog = new ApplyChangesDialog();
        private SettingsDialog m_SettingsDialog = new SettingsDialog();

        private BackgroundWorker m_UpdateRepoWorker = null;
        private BackgroundWorker m_InstallWorker = null;

        private GUIModFilter m_ModFilter = GUIModFilter.All;
        private string m_ModNameFilter = "";

        public Main()
        {
            m_Instance = this;
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            m_UpdateRepoWorker = new BackgroundWorker();
            m_UpdateRepoWorker.WorkerReportsProgress = false;
            m_UpdateRepoWorker.WorkerSupportsCancellation = true;
            m_UpdateRepoWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PostUpdateRepo);
            m_UpdateRepoWorker.DoWork += new DoWorkEventHandler(UpdateRepo);

            m_InstallWorker = new BackgroundWorker();
            m_InstallWorker.WorkerReportsProgress = true;
            m_InstallWorker.WorkerSupportsCancellation = true;
            m_InstallWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PostInstallMods);
            m_InstallWorker.DoWork += new DoWorkEventHandler(InstallMods);

            KSP.Init();
            User.noConsole = true;

            UpdateModsList();
            UpdateModFilterList();

            ApplyToolButton.Enabled = false;

            var assemblies = (AssemblyInformationalVersionAttribute[])Assembly.GetAssembly(typeof(RegistryManager)).GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);

            if (assemblies.Length == 0 || assemblies[0].InformationalVersion == null)
            {
                // Dunno the version. Some dev probably built it. 
                Text = "CKAN (development)";
            }
            else
            {
                Text = String.Format("CKAN {0}", assemblies[0].InformationalVersion);
            }
        }

        private static Main m_Instance = null;
        public static Main Instance
        {
            get
            {
                return m_Instance;
            }
        }

        private void UpdateRepo(object sender, DoWorkEventArgs e)
        {
            Repo.Update();
        }

        private void PostUpdateRepo(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            UpdateModFilterList();

            m_WaitDialog.SetDescription("Scanning for manually installed mods");
            KSP.ScanGameData();

            m_WaitDialog.HideWaitDialog();
            Enabled = true;
        }

        private void InstallModsReportProgress(string message, int percent)
        {
            if (m_WaitDialog != null)
            {
                m_WaitDialog.SetDescription(message + " " + percent.ToString() + "%");
            }

        }

        private void InstallMods(object sender, DoWorkEventArgs e)
        {
            var opts = (KeyValuePair<List<KeyValuePair<CkanModule, GUIModChangeType>>, RelationshipResolverOptions>)e.Argument;

            ModuleInstaller installer = new ModuleInstaller();
            installer.onReportProgress += InstallModsReportProgress;

            // first we uninstall selected mods
            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Remove)
                {
                    installer.Uninstall(change.Key.identifier);
                }
            }

            // install everything else
            List<string> toInstall = new List<string>();
            foreach (var change in opts.Key)
            {
                if (change.Value == GUIModChangeType.Install || change.Value == GUIModChangeType.Update)
                {
                    toInstall.Add(change.Key.identifier);
                }
            }

            installer.InstallList(toInstall, opts.Value);
        }

        private void PostInstallMods(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateModsList();
            UpdateModFilterList();
            m_WaitDialog.Close();
            Enabled = true;
        }

        private List<KeyValuePair<CkanModule, GUIModChangeType>> ComputeChangeSetFromModList()
        {
            List<KeyValuePair<CkanModule, GUIModChangeType>> changeset = new List<KeyValuePair<CkanModule, GUIModChangeType>>();

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
                else if (!isInstalled && isInstalledChecked)
                {
                    changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(mod, GUIModChangeType.Install));
                }
            }

            return changeset;
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
            }

            return modules;
        }

        private void UpdateModFilterList()
        {
            ModFilter.Items[0] = String.Format("All ({0})", CountModsByFilter(GUIModFilter.All));
            ModFilter.Items[1] = String.Format("Installed ({0})", CountModsByFilter(GUIModFilter.Installed));
            ModFilter.Items[2] = String.Format("Updated ({0})", CountModsByFilter(GUIModFilter.InstalledUpdateAvailable));
            ModFilter.Items[3] = String.Format("New in repository ({0})", CountModsByFilter(GUIModFilter.NewInRepository));
            ModFilter.Items[4] = String.Format("Not installed ({0})", CountModsByFilter(GUIModFilter.NotInstalled));
        }

        private void UpdateModsList(bool markUpdates = false)
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
            }

            // filter by name
            modules.RemoveAll(m => !m.name.ToLowerInvariant().Contains(m_ModNameFilter.ToLowerInvariant()));

            foreach (CkanModule mod in modules)
            {
                DataGridViewRow item = new DataGridViewRow();
                item.Tag = mod;

                // installed
                var isInstalled = RegistryManager.Instance().registry.IsInstalled(mod.identifier);
                var installedCell = new DataGridViewCheckBoxCell();
                installedCell.Value = isInstalled;
                item.Cells.Add(installedCell);

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

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            m_UpdateRepoWorker.RunWorkerAsync();
            Enabled = false;
            m_WaitDialog.SetDescription("Contacting repository..");
            m_WaitDialog.ShowWaitDialog();
        }

        private void ModFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_ModFilter = (GUIModFilter)ModFilter.SelectedIndex;
            UpdateModsList();
        }

        private void MarkAllUpdatesToolButton_Click(object sender, EventArgs e)
        {
            foreach(DataGridViewRow row in ModList.Rows)
            {
                var mod = (CkanModule) row.Tag;
                if (!RegistryManager.Instance().registry.IsInstalled(mod.identifier))
                {
                    continue;
                }

                var isUpToDate = !RegistryManager.Instance().registry.InstalledVersion(mod.identifier).IsLessThan(mod.version);
                if (isUpToDate)
                {
                    if (row.Cells[1] is DataGridViewCheckBoxCell)
                    {
                        var updateCell = row.Cells[1] as DataGridViewCheckBoxCell;
                        updateCell.Value = true;
                    }
                }
            }

            UpdateModsList();
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModList.SelectedRows.Count == 0)
            {
                return;
            }

            var selectedItem = ModList.SelectedRows[0];
            if (selectedItem == null)
            {
                return;
            }

            var module = (CkanModule)selectedItem.Tag;
            if (module == null)
            {
                return;
            }

            ModInfo.Text = "";

            ModInfo.AppendText(String.Format("\"{0}\" - version {1}\r\n", module.name, module.version));

            ModInfo.AppendText(String.Format("Abstract: {0}\r\n", module.@abstract));

            if (module.author != null)
            {
                string authors = "";
                foreach (var auth in module.author)
                {
                    authors += auth + ", ";
                }

                ModInfo.AppendText(String.Format("Author: {0}\r\n", authors));
            }

            ModInfo.AppendText(String.Format("Comment: {0}\r\n", module.comment));
            ModInfo.AppendText(String.Format("Download: {0}\r\n", module.download.ToString()));
            ModInfo.AppendText(String.Format("Identifier: {0}\r\n", module.identifier));
            ModInfo.AppendText(String.Format("KSP Version: {0}\r\n", module.ksp_version.ToString()));
            ModInfo.AppendText(String.Format("License: {0}\r\n", module.license.ToString()));
            ModInfo.AppendText(String.Format("Release status: {0}\r\n", module.release_status));
        }

        private void ApplyToolButton_Click(object sender, EventArgs e)
        {
            var changeset = ComputeChangeSetFromModList();
            m_ApplyChangesDialog.ShowApplyChangesDialog(changeset, m_InstallWorker);
            ApplyToolButton.Enabled = false;
        }

        private void ExitToolButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void ShowWaitDialog()
        {
            Enabled = false;
            m_WaitDialog.ShowWaitDialog();
        }

        public void HideWaitDialog()
        {
            m_WaitDialog.Close();
            Enabled = true;
        }

        private void FilterByNameTextBox_TextChanged(object sender, EventArgs e)
        {
            m_ModNameFilter = FilterByNameTextBox.Text;
            UpdateModsList();
        }

        private void ModList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ModList.EndEdit();

            if (ComputeChangeSetFromModList().Any())
            {
                ApplyToolButton.Enabled = true;
            }
            else
            {
                ApplyToolButton.Enabled = false;
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_SettingsDialog.ShowDialog();
        }
    }
}
