using System;
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

        public Main()
        {
            controlFactory = new ControlFactory();
            m_Instance = this;
            InitializeComponent();

            RecreateDialogs ();
        }

        public void RecreateDialogs()
        {
            m_ApplyChangesDialog = controlFactory.CreateControl<ApplyChangesDialog>();
            m_ErrorDialog = controlFactory.CreateControl<ErrorDialog>();
            m_RecommendsDialog = controlFactory.CreateControl<RecommendsDialog>();
            m_SettingsDialog = controlFactory.CreateControl<SettingsDialog>();
            m_WaitDialog = controlFactory.CreateControl<WaitDialog>();
            m_YesNoDialog = controlFactory.CreateControl<YesNoDialog>();
        }

        public static Main Instance
        {
            get { return m_Instance; }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            KSP.Init();

            m_Configuration = Configuration.LoadOrCreateConfiguration
            (
                Path.Combine(KSP.GameDir(), "CKAN/GUIConfig.xml"),
                Repo.default_ckan_repo
            );

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

            User.frontEnd = FrontEndType.UI;
            User.yesNoDialog = YesNoDialog;
            User.displayMessage = AddStatusMessage;
            User.displayError = ErrorDialog;

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

        private void UpdateModInfo(CkanModule module)
        {
            Util.Invoke(ModInfo, () => _UpdateModInfo(module));
        }

        private void _UpdateModInfo(CkanModule module)
        {
            ModInfo.Text = "";

            ModInfo.AppendText(String.Format("\"{0}\" - version {1}\r\n", module.name, module.version));

            ModInfo.AppendText(String.Format("Abstract: {0}\r\n", module.@abstract));

            if (module.author != null)
            {
                string authors = "";
                foreach (string auth in module.author)
                {
                    authors += auth + ", ";
                }

                ModInfo.AppendText(String.Format("Author: {0}\r\n", authors));
            }

            ModInfo.AppendText(String.Format("Comment: {0}\r\n", module.comment));
            ModInfo.AppendText(String.Format("Download: {0}\r\n", module.download));
            ModInfo.AppendText(String.Format("Identifier: {0}\r\n", module.identifier));

            if (module.ksp_version != null)
            {
                ModInfo.AppendText (String.Format ("KSP Version: {0}\r\n", module.ksp_version));
            }

            ModInfo.AppendText(String.Format("License: {0}\r\n", module.license.ToString()));
            ModInfo.AppendText(String.Format("Release status: {0}\r\n", module.release_status));

            ModInfo.AppendText("\r\n");

            string dependencies = "";
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

            string recommended = "";
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

            string suggested = "";
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

        private void UpdateModDependencyGraphRecursively(TreeNode node, CkanModule module)
        {
            int i = 0;

            node.Text = module.name;
            node.Nodes.Clear();

            if (module.depends != null)
            {
                foreach (dynamic dependency in module.depends)
                {
                    Registry registry = RegistryManager.Instance().registry;

                    try
                    {
                        dynamic dependencyModule = registry.LatestAvailable(dependency.name.ToString(), KSP.Version());

                        node.Nodes.Add("");
                        UpdateModDependencyGraphRecursively(node.Nodes[i], dependencyModule);
                        i++;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void UpdateModDependencyGraph(CkanModule module)
        {
            Util.Invoke(DependsGraphTree, () => _UpdateModDependencyGraph(module));
        }

        private void UpdateModContentsGraphRecursively(TreeNode node, CkanModule module)
        {
            
        }

        private void _UpdateModDependencyGraph(CkanModule module)
        {
            DependsGraphTree.Nodes.Clear();
            DependsGraphTree.Nodes.Add("");
            UpdateModDependencyGraphRecursively(DependsGraphTree.Nodes[0], module);
            DependsGraphTree.Nodes[0].ExpandAll();
        }

        private void UpdateModContentsTree(CkanModule module)
        {
            Util.Invoke(ContentsPreviewTree, () => _UpdateModContentsTree(module));
        }

        private void _UpdateModContentsTree(CkanModule module)
        {
            if (ModuleInstaller.IsCached(module))
            {
                NotCachedLabel.Text = "Module is cached, preview available";
                ContentsDownloadButton.Enabled = false;
            }
            else
            {
                NotCachedLabel.Text = "This mod is not in the cache, click 'Download' to preview contents";
                ContentsDownloadButton.Enabled = true;
            }

            ContentsPreviewTree.Nodes.Clear();
            ContentsPreviewTree.Nodes.Add(module.name);
            UpdateModContentsGraphRecursively(ContentsPreviewTree.Nodes[0], module);
            ContentsPreviewTree.Nodes[0].ExpandAll();
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
    }
}