using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using log4net;

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

        private static readonly ILog log = LogManager.GetLogger(typeof(Main));

        private TabController m_TabController = null;

        public MainModList mainModList { get; private set; }

        public Main()
        {
            User.frontEnd = FrontEndType.UI;
            User.yesNoDialog = YesNoDialog;
            User.displayMessage = AddStatusMessage;
            User.displayError = ErrorDialog;

            controlFactory = new ControlFactory();
            m_Instance = this;
            
            InitializeComponent();

            // We need to initialize error dialog first to display errors
            m_ErrorDialog = controlFactory.CreateControl<ErrorDialog>();

            // We want to check our current instance is null first, as it may
            // have already been set by a command-line option.
            if (KSPManager.CurrentInstance == null && KSPManager.GetPreferredInstance() == null)
            {
                Hide();

                var result = new ChooseKSPInstance().ShowDialog();
                if (result == DialogResult.Cancel || result == DialogResult.Abort)
                {
                    Application.Exit();
                    return;
                }
            }

            m_Configuration = Configuration.LoadOrCreateConfiguration
            (
                Path.Combine(KSPManager.CurrentInstance.GameDir(), "CKAN/GUIConfig.xml"),
                Repo.default_ckan_repo.ToString()
            );

            FilterToolButton.MouseHover += (sender, args) => FilterToolButton.ShowDropDown();
            launchKSPToolStripMenuItem.MouseHover += (sender, args) => launchKSPToolStripMenuItem.ShowDropDown();
            ApplyToolButton.MouseHover += (sender, args) => ApplyToolButton.ShowDropDown();

            ModList.CurrentCellDirtyStateChanged += ModList_CurrentCellDirtyStateChanged;
            ModList.CellValueChanged += ModList_CellValueChanged;

            m_TabController = new TabController(MainTabControl);
            m_TabController.ShowTab("ManageModsTabPage");
            
            mainModList = new MainModList(source => UpdateFilters(this));            

            RecreateDialogs();

            // We should run application only when we really sure.
            // System.Threading.Thread.CurrentThread.SetApartmentState(System.Threading.ApartmentState.STA);
            Util.HideConsoleWindow();
            Application.Run(this);
            
        }

        void ModList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {   
            ModList_CellContentClick(sender, null);
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
                if (mainModList.ComputeChangeSetFromModList(RegistryManager.Instance(KSPManager.CurrentInstance).registry).Any())
                {
                    ApplyToolButton_Click(null, null);
                }

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

            ApplyToolButton.Enabled = false;

            ModList.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            Text = String.Format("CKAN {0} - KSP {1}", Meta.Version(), KSPManager.CurrentInstance.Version());
            KSPVersionLabel.Text = String.Format("Kerbal Space Program {0}", KSPManager.CurrentInstance.Version());
        }

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            UpdateRepo();
        }

        private void MarkAllUpdatesToolButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = ((GUIMod)row.Tag).ToCkanModule();
                if (!RegistryManager.Instance(KSPManager.CurrentInstance).registry.IsInstalled(mod.identifier))
                {
                    continue;
                }

                bool isUpToDate =
                    !RegistryManager.Instance(KSPManager.CurrentInstance).registry.InstalledVersion(mod.identifier).IsLessThan(mod.version);
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

            var module = ((GUIMod) selectedItem.Tag).ToCkanModule();
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
            m_TabController.ShowTab("ChangesetTabPage", 1);
        }

        private void ExitToolButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FilterByNameTextBox_TextChanged(object sender, EventArgs e)
        {
            mainModList.ModNameFilter = FilterByNameTextBox.Text;
        }

        /// <summary>
        /// I'm pretty sure this is what gets called when the user clicks on a ticky
        /// in the mod list.
        /// </summary>
        private void ModList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
            ModList.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void ModList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (mainModList.ModFilter == GUIModFilter.Incompatible)
            {
                return;
            }
            var grid = sender as DataGridView;
            var row = grid.Rows[e.RowIndex];
            var columnIndex = e.ColumnIndex;
            var gridViewCell = row.Cells[columnIndex];
            if (columnIndex < 2)
            {
                var checkbox = (DataGridViewCheckBoxCell)gridViewCell;
                
                if (columnIndex==0)
                {
                    ((GUIMod) row.Tag).IsInstallChecked = (bool) checkbox.Value;
                }
                else if (columnIndex == 1)
                {
                    ((GUIMod)row.Tag).IsUpgradeChecked = (bool)checkbox.Value;
                }
            }
            var changeset = mainModList.ComputeChangeSetFromModList(RegistryManager.Instance(KSPManager.CurrentInstance).registry);


            if (changeset != null && changeset.Any())
            {
                UpdateChangesDialog(changeset, m_InstallWorker);
                m_TabController.ShowTab("ChangesetTabPage", 1, false);
                ApplyToolButton.Enabled = true;
            }
            else
            {
                m_TabController.HideTab("ChangesetTabPage");
                ApplyToolButton.Enabled = false;
            }

            if (e.RowIndex < 0 || columnIndex < 0)
            {
                return;
            }

            
            if (gridViewCell is DataGridViewLinkCell)
            {
                var cell = gridViewCell as DataGridViewLinkCell;
                Process.Start(cell.Value.ToString());
            }

            ModList.EndEdit();
        }

        private void FilterAllButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.All;
            FilterToolButton.Text = "Filter (All)";
        }

        private void FilterInstalledButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.Installed;
            FilterToolButton.Text = "Filter (Installed)";
        }

        private void FilterInstalledUpdateButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.InstalledUpdateAvailable;
            FilterToolButton.Text = "Filter (Updated)";
        }

        private void FilterNewButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.NewInRepository;
            FilterToolButton.Text = "Filter (New)";
        }

        private void FilterNotInstalledButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.NotInstalled;
            FilterToolButton.Text = "Filter (Not installed)";
        }

        private void FilterIncompatibleButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.Incompatible;
            FilterToolButton.Text = "Filter (Incompatible)";
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

            var module = ((GUIMod) selectedItem.Tag).ToCkanModule();
            if (module == null)
            {
                return;
            }

            ResetProgress();
            ShowWaitDialog(false);
            ModuleInstaller.Instance.CachedOrDownload(module);
            HideWaitDialog(true);

            UpdateModContentsTree(module);
            RecreateDialogs();
        }

        private void MetadataModuleHomePageLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MetadataModuleHomePageLinkLabel.Text == "N/A")
            {
                return;
            }

            Process.Start(MetadataModuleHomePageLinkLabel.Text);
        }

        private void MetadataModuleGitHubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MetadataModuleGitHubLinkLabel.Text == "N/A")
            {
                return;
            }

            Process.Start(MetadataModuleGitHubLinkLabel.Text);
        }

        private void ModuleRelationshipType_SelectedIndexChanged(object sender, EventArgs e)
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

            var module = ((GUIMod)selectedItem.Tag).ToCkanModule();
            if (module == null)
            {
                return;
            }

            UpdateModDependencyGraph(module);
        }

        private void exportInstalledModsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void installFromckanToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

      
        private void launchKSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lst = m_Configuration.CommandLineArguments.Split(' ');
            if (lst.Length == 0)
            {
                return;
            }

            string binary = lst[0];
            string args = String.Empty;

            for(int i = 1; i < lst.Length; i++)
            {
                args += lst[i] + " ";
            }
            
            try
            {
                Directory.SetCurrentDirectory(KSPManager.CurrentInstance.GameDir());
                Process.Start(binary, args);
            }
            catch(Exception exception)
            {
                User.Error("Couldn't start KSP. {0}.", exception.Message);
            }
        }

        private void setCommandlineOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void clearChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateModsList();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.ShowDialog();
        }

        private void KSPCommandlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new KSPCommandLineOptionsDialog();
            dialog.SetCommandLine(m_Configuration.CommandLineArguments);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                m_Configuration.CommandLineArguments = dialog.AdditionalArguments.Text;
                m_Configuration.Save();
            }
        }

        private void CKANSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Flipping enabled here hides the main form itself.
            Enabled = false;
            m_SettingsDialog.ShowDialog();
            Enabled = true;
        }

    }
}
