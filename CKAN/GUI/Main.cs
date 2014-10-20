﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private static Main m_Instance;
        public Configuration m_Configuration = null;

        public ControlFactory controlFactory = null;

        private FolderBrowserDialog m_FindKSPRootDialog = new FolderBrowserDialog();

        public Main()
        {
            User.frontEnd = FrontEndType.UI;
            User.yesNoDialog = YesNoDialog;
            User.displayMessage = AddStatusMessage;
            User.displayError = ErrorDialog;

            controlFactory = new ControlFactory();
            m_Instance = this;
            InitializeComponent();
            RecreateDialogs();

            try
            {
                KSP.Init();
            }
            catch (DirectoryNotFoundException)
            {
                User.Error("Failed to find KSP root directory, press OK to browse");
                DialogResult result = m_FindKSPRootDialog.ShowDialog();

                if (result != DialogResult.OK)
                {
                    Environment.Exit(1);
                }

                var path = m_FindKSPRootDialog.SelectedPath;

                if (Directory.Exists(path))
                {
                    try
                    {
                        KSP.PopulateGamedirRegistry(m_FindKSPRootDialog.SelectedPath);
                        KSP.Init();
                    }
                    catch (Exception)
                    {
                        User.Error("Invalid KSP directory");
                    }
                }
                else
                {
                    User.Error("Directory doesn't exist");
                }
            }

            m_Configuration = Configuration.LoadOrCreateConfiguration
            (
                Path.Combine(KSP.GameDir(), "CKAN/GUIConfig.xml"),
                Repo.default_ckan_repo
            );
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F))
            {
                ActiveControl = FilterByNameTextBox;
                return true;
            }
            else if (keyData == (Keys.Control | Keys.S))
            {
                ApplyToolButton_Click(null, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public static Main Instance
        {
            get { return m_Instance; }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            m_UpdateRepoWorker = new BackgroundWorker();
            m_UpdateRepoWorker.WorkerReportsProgress = false;
            m_UpdateRepoWorker.WorkerSupportsCancellation = true;
            m_UpdateRepoWorker.RunWorkerCompleted += PostUpdateRepo;
            m_UpdateRepoWorker.DoWork += UpdateRepo;

            m_InstallWorker = new BackgroundWorker();
            m_InstallWorker.WorkerReportsProgress = true;
            m_InstallWorker.WorkerSupportsCancellation = true;
            m_InstallWorker.RunWorkerCompleted += PostInstallMods;
            m_InstallWorker.DoWork += InstallMods;

            UpdateModsList();
            UpdateModFilterList();

            ApplyToolButton.Enabled = false;

            Text = "CKAN (" + Meta.Version() + ")";
        }

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            UpdateRepo();
        }

        private void MarkAllUpdatesToolButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = (CkanModule) row.Tag;
                if (!RegistryManager.Instance().registry.IsInstalled(mod.identifier))
                {
                    continue;
                }

                bool isUpToDate =
                    !RegistryManager.Instance().registry.InstalledVersion(mod.identifier).IsLessThan(mod.version);
                if (!isUpToDate)
                {
                    if (row.Cells[1] is DataGridViewCheckBoxCell)
                    {
                        var updateCell = row.Cells[1] as DataGridViewCheckBoxCell;
                        updateCell.Value = true;
                        ApplyToolButton.Enabled = true;
                    }
                }
            }

            ModList.Refresh();
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModList.SelectedRows.Count == 0)
            {
                return;
            }

            DataGridViewRow selectedItem = ModList.SelectedRows[0];
            if (selectedItem == null)
            {
                return;
            }

            var module = (CkanModule) selectedItem.Tag;
            if (module == null)
            {
                return;
            }

            UpdateModInfo(module);
            UpdateModDependencyGraph(module);
            UpdateModContentsTree(module);
        }

        private void ApplyToolButton_Click(object sender, EventArgs e)
        {
            List<KeyValuePair<CkanModule, GUIModChangeType>> changeset = ComputeChangeSetFromModList();
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
                Process.Start(cell.Value.ToString());
            }

            if (e.ColumnIndex == 0 && ModList.Rows.Count > e.RowIndex) // if user clicked install
            {
                DataGridViewRow row = ModList.Rows[e.RowIndex];
                var cell = row.Cells[0] as DataGridViewCheckBoxCell;
                var mod = (CkanModule) row.Tag;

                bool isInstalled = RegistryManager.Instance().registry.IsInstalled(mod.identifier);
                if ((bool) cell.Value == false && !isInstalled)
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
                            if (depRow.Tag == dependency)
                            {
                                (depRow.Cells[0] as DataGridViewCheckBoxCell).Value = true;
                            }
                        }
                    }
                }
                else if ((bool) cell.Value && isInstalled)
                {
                    var installer = ModuleInstaller.Instance;
                    List<string> reverseDependencies = installer.FindReverseDependencies(mod.identifier);
                    foreach (string dependency in reverseDependencies)
                    {
                        foreach (DataGridViewRow depRow in ModList.Rows)
                        {
                            if (((CkanModule) depRow.Tag).identifier == dependency)
                            {
                                (depRow.Cells[0] as DataGridViewCheckBoxCell).Value = false;
                            }
                        }
                    }
                }
            }

            ModList.EndEdit();

            var changeset = ComputeChangeSetFromModList();
            if (changeset != null && changeset.Any())
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

        private void FilterAllButton_Click(object sender, EventArgs e)
        {
            m_ModFilter = GUIModFilter.All;
            FilterToolButton.Text = "Filter (All)";
            UpdateModsList();
        }

        private void FilterInstalledButton_Click(object sender, EventArgs e)
        {
            m_ModFilter = GUIModFilter.Installed;
            FilterToolButton.Text = "Filter (Installed)";
            UpdateModsList();
        }

        private void FilterInstalledUpdateButton_Click(object sender, EventArgs e)
        {
            m_ModFilter = GUIModFilter.InstalledUpdateAvailable;
            FilterToolButton.Text = "Filter (Updated)";
            UpdateModsList();
        }

        private void FilterNewButton_Click(object sender, EventArgs e)
        {
            m_ModFilter = GUIModFilter.NewInRepository;
            FilterToolButton.Text = "Filter (New)";
            UpdateModsList();
        }

        private void FilterNotInstalledButton_Click(object sender, EventArgs e)
        {
            m_ModFilter = GUIModFilter.NotInstalled;
            FilterToolButton.Text = "Filter (Not installed)";
            UpdateModsList();
        }

        private void FilterIncompatibleButton_Click(object sender, EventArgs e)
        {
            m_ModFilter = GUIModFilter.Incompatible;
            FilterToolButton.Text = "Filter (Incompatible)";
            UpdateModsList();
        }

        private void ContentsDownloadButton_Click(object sender, EventArgs e)
        {
            if (ModList.SelectedRows.Count == 0)
            {
                return;
            }

            DataGridViewRow selectedItem = ModList.SelectedRows[0];
            if (selectedItem == null)
            {
                return;
            }

            var module = (CkanModule)selectedItem.Tag;
            if (module == null)
            {
                return;
            }

            m_WaitDialog.ResetProgress();
            m_WaitDialog.ShowWaitDialog(false);
            ModuleInstaller.Instance.CachedOrDownload(module);
            m_WaitDialog.HideWaitDialog();

            UpdateModContentsTree(module);
            RecreateDialogs();
        }
    }
}