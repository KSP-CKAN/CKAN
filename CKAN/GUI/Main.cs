using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        Incompatible = 5,
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

        public Configuration m_Configuration = null;

        public Main()
        {
            m_Instance = this;
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            KSP.Init();

            m_Configuration = Configuration.LoadOrCreateConfiguration
            (
                System.IO.Path.Combine(KSP.GameDir(), "CKAN/GUIConfig.xml"),
                Repo.default_ckan_repo
            );

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

            User.yesNoDialog = YesNoDialog;
            User.displayMessage = AddStatusMessage;
            User.displayError = ErrorDialog;

            UpdateModsList();
            UpdateModFilterList();

            ApplyToolButton.Enabled = false;

            Text = "CKAN (" + Meta.Version() + ")";
        }

        private static Main m_Instance = null;
        public static Main Instance
        {
            get
            {
                return m_Instance;
            }
        }

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            UpdateRepo();
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

            ModInfo.AppendText("\r\n");

            var dependencies = "";
            if (module.depends != null)
            {
                for (int i = 0; i < module.depends.Count(); i++)
                {
                    dependencies += module.depends[i].name;
                    if (i != module.depends.Count() - 1)
                    {
                        dependencies += ", ";
                    }
                }
            }

            ModInfo.AppendText(String.Format("Dependencies: {0}\r\n", dependencies));
            ModInfo.AppendText("\r\n");

            var recommended = "";
            if (module.recommends != null)
            {
                for (int i = 0; i < module.recommends.Count(); i++)
                {
                    recommended += module.recommends[i].name;
                    if (i != module.recommends.Count() - 1)
                    {
                        recommended += ", ";
                    }
                }
            }

            ModInfo.AppendText(String.Format("Recommends: {0}\r\n", recommended));
            ModInfo.AppendText("\r\n");

            var suggested = "";
            if (module.suggests != null)
            {
                for (int i = 0; i < module.suggests.Count(); i++)
                {
                    suggested += module.suggests[i].name;
                    if (i != module.suggests.Count() - 1)
                    {
                        suggested += ", ";
                    }
                }
            }

            ModInfo.AppendText(String.Format("Suggested: {0}\r\n", suggested));
            ModInfo.AppendText("\r\n");
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

        private void FilterByNameTextBox_TextChanged(object sender, EventArgs e)
        {
            m_ModNameFilter = FilterByNameTextBox.Text;
            UpdateModsList();
        }

        private void ModList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (m_ModFilter == GUIModFilter.Incompatible)
            {
                return;
            }

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            if (ModList.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewLinkCell)
            {
                var cell = ModList.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewLinkCell;
                Process.Start((string)cell.Value.ToString());
            }

            if (e.ColumnIndex == 0 && ModList.Rows.Count > e.RowIndex) // if user clicked install
            {
                var row = ModList.Rows[e.RowIndex];
                var cell = row.Cells[0] as DataGridViewCheckBoxCell;
                var mod = (CkanModule)row.Tag;

                var isInstalled = RegistryManager.Instance().registry.IsInstalled(mod.identifier);
                if ((bool)cell.Value == false && !isInstalled)
                {
                    var options = new RelationshipResolverOptions();
                    options.with_all_suggests = false;
                    options.with_recommends = false;
                    options.with_suggests = false;

                    List<CkanModule> dependencies = GetInstallDependencies(mod, options);

                    if (dependencies == null)
                    {
                        return;
                    }

                    foreach (CkanModule dependency in dependencies)
                    {
                        foreach (DataGridViewRow depRow in ModList.Rows)
                        {
                            if ((CkanModule) depRow.Tag == dependency)
                            {
                                (depRow.Cells[0] as DataGridViewCheckBoxCell).Value = true;
                            }
                        }
                    }
                }
                else if ((bool) cell.Value == true && isInstalled)
                {
                    var installer = new ModuleInstaller();
                    List<string> reverseDependencies = installer.FindReverseDependencies(mod.identifier);
                    foreach (string dependency in reverseDependencies)
                    {
                        foreach (DataGridViewRow depRow in ModList.Rows)
                        {
                            if (((CkanModule)depRow.Tag).identifier == dependency)
                            {
                                (depRow.Cells[0] as DataGridViewCheckBoxCell).Value = false;
                            }
                        }
                    }
                }
            }

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
