using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using CKAN.Properties;

namespace CKAN
{
    public enum GUIModFilter
    {
        Compatible = 0,
        Installed = 1,
        InstalledUpdateAvailable = 2,
        NewInRepository = 3,
        NotInstalled = 4,
        Incompatible = 5,
        All = 6
    }

    public enum GUIModChangeType
    {
        None = 0,
        Install = 1,
        Remove = 2,
        Update = 3
    }

    public partial class Main
    {
        public delegate void ModChangedCallback(CkanModule module, GUIModChangeType change);

        public static event ModChangedCallback modChangedCallback;

        public Configuration m_Configuration;

        public ControlFactory controlFactory;

        private static readonly ILog log = LogManager.GetLogger(typeof (Main));
        public TabController m_TabController;
        public volatile KSPManager manager;

        public PluginController m_PluginController = null;

        public KSP CurrentInstance
        {
            get { return manager.CurrentInstance; }
        }

        public KSPManager Manager
        {
            get { return manager; }
            set { manager = value; }
        }

        public MainModList mainModList { get; private set; }

        public string[] m_CommandLineArgs = null;

        public GUIUser m_User = null;

        private Timer filterTimer = null;

        private IEnumerable<KeyValuePair<CkanModule, GUIModChangeType>> change_set;
        private Dictionary<Module, string> conflicts;

        private IEnumerable<KeyValuePair<CkanModule, GUIModChangeType>> ChangeSet
        {
            get { return change_set; }
            set
            {
                change_set = value;
                ChangeSetUpdated();
            }
        }

        private Dictionary<Module, string> Conflicts
        {
            get { return conflicts; }
            set
            {
                conflicts = value;
                ConflictsUpdated();
            }
        }

        private void ConflictsUpdated()
        {
            foreach (DataGridViewRow row in ModList.Rows)
            {
                var module = ((GUIMod) row.Tag).ToCkanModule();
                string value;

                if (Conflicts != null && Conflicts.TryGetValue(module, out value))
                {
                    var conflict_text = value;
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.ToolTipText = conflict_text;
                    }
                    if (row.DefaultCellStyle.BackColor != Color.LightCoral)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        ModList.InvalidateRow(row.Index);
                    }
                }
                else
                {
                    if (row.DefaultCellStyle.BackColor != Color.White)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.ToolTipText = null;
                        }

                        row.DefaultCellStyle.BackColor = Color.White;
                        ModList.InvalidateRow(row.Index);
                    }
                }
            }
        }

        private void ChangeSetUpdated()
        {
            if (ChangeSet != null && ChangeSet.Any())
            {
                UpdateChangesDialog(ChangeSet.ToList(), m_InstallWorker);
                m_TabController.ShowTab("ChangesetTabPage", 1, false);
                ApplyToolButton.Enabled = true;
            }
            else
            {
                m_TabController.HideTab("ChangesetTabPage");
                ApplyToolButton.Enabled = false;
            }
        }

        public Main(string[] cmdlineArgs, GUIUser User, bool showConsole)
        {
            log.Info("Starting the GUI");
            m_CommandLineArgs = cmdlineArgs;
            m_User = User;

            User.displayMessage = AddStatusMessage;
            User.displayError = ErrorDialog;

            controlFactory = new ControlFactory();
            Instance = this;
            mainModList = new MainModList(source => UpdateFilters(this));
            InitializeComponent();

            // We need to initialize error dialog first to display errors
            m_ErrorDialog = controlFactory.CreateControl<ErrorDialog>();

            // We want to check our current instance is null first, as it may
            // have already been set by a command-line option.
            Manager = new KSPManager(User);
            if (CurrentInstance == null && manager.GetPreferredInstance() == null)
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
                    Path.Combine(CurrentInstance.GameDir(), "CKAN/GUIConfig.xml"),
                    Repo.default_ckan_repo.ToString()
                );

            FilterToolButton.MouseHover += (sender, args) => FilterToolButton.ShowDropDown();
            launchKSPToolStripMenuItem.MouseHover += (sender, args) => launchKSPToolStripMenuItem.ShowDropDown();
            ApplyToolButton.MouseHover += (sender, args) => ApplyToolButton.ShowDropDown();

            ModList.CurrentCellDirtyStateChanged += ModList_CurrentCellDirtyStateChanged;
            ModList.CellValueChanged += ModList_CellValueChanged;

            m_TabController = new TabController(MainTabControl);
            m_TabController.ShowTab("ManageModsTabPage");

            RecreateDialogs();

            if (!showConsole)
            {
                Util.HideConsoleWindow();
            }

            // Disable the modinfo controls until a mod has been choosen.
            this.ModInfoTabControl.Enabled = false;

            // WinForms on Mac OS X has a nasty bug where the UI thread hogs the CPU,
            // making our download speeds really slow unless you move the mouse while 
            // downloading. Yielding periodically addresses that.
            // https://bugzilla.novell.com/show_bug.cgi?id=663433
            if (Platform.IsMac)
            {
                System.Windows.Forms.Timer yieldTimer = new System.Windows.Forms.Timer();
                yieldTimer.Interval = 2;
                yieldTimer.Tick += YieldTimer_Tick;
                yieldTimer.Start();
            }

            Application.Run(this);
        }

        /// <summary>
        /// Used above to fix the UI thread from hogging all the CPU on OS X,
        /// slowing down downloads.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void YieldTimer_Tick (object sender, EventArgs e)
        {
            System.Threading.Thread.Yield();
        }

        private void ModList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            ModList_CellContentClick(sender, null);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.Control | Keys.F):
                    ActiveControl = FilterByNameTextBox;
                    return true;
                case (Keys.Control | Keys.S):
                    if (ChangeSet != null && ChangeSet.Any())
                    {
                        ApplyToolButton_Click(null, null);
                    }

                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public static Main Instance { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            Location = m_Configuration.WindowLoc;
            Size = m_Configuration.WindowSize;
            if (!m_Configuration.CheckForUpdatesOnLaunchNoNag)
            {
                log.Debug("Asking user if they wish for autoupdates");
                if (new AskUserForAutoUpdatesDialog().ShowDialog() == DialogResult.OK)
                {
                    m_Configuration.CheckForUpdatesOnLaunch = true;
                }

                m_Configuration.CheckForUpdatesOnLaunchNoNag = true;
                m_Configuration.Save();
            }

            if (m_Configuration.CheckForUpdatesOnLaunch)
            {
                try
                {
                    log.Info("Making autoupdate call");
                    var latest_version = AutoUpdate.FetchLatestCkanVersion();
                    var current_version = new Version(Meta.Version());

                    if (latest_version.IsGreaterThan(current_version))
                    {
                        log.Debug("Found higher ckan version");
                        var release_notes = AutoUpdate.FetchLatestCkanVersionReleaseNotes();
                        var dialog = new NewUpdateDialog(latest_version.ToString(), release_notes);
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            log.Info("Start ckan update");
                            AutoUpdate.StartUpdateProcess(true);
                        }
                    }
                }
                catch (Exception exception)
                {
                    m_User.RaiseError("Error in autoupdate: \n\t" + exception.Message + "");
                    log.Error("Error in autoupdate", exception);
                }
            }

            m_UpdateRepoWorker = new BackgroundWorker { WorkerReportsProgress = false, WorkerSupportsCancellation = true };

            m_UpdateRepoWorker.RunWorkerCompleted += PostUpdateRepo;
            m_UpdateRepoWorker.DoWork += UpdateRepo;

            m_InstallWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            m_InstallWorker.RunWorkerCompleted += PostInstallMods;
            m_InstallWorker.DoWork += InstallMods;

            UpdateModsList();

            m_User.displayYesNo = YesNoDialog;
            URLHandlers.RegisterURLHandler(m_Configuration, m_User);
            m_User.displayYesNo = null;

            ApplyToolButton.Enabled = false;

            CurrentInstanceUpdated();

            ModList.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            if (m_CommandLineArgs.Length >= 2)
            {
                var identifier = m_CommandLineArgs[1];
                if (identifier.StartsWith("//"))
                {
                    identifier = identifier.Substring(2);
                }
                else if (identifier.StartsWith("ckan://"))
                {
                    identifier = identifier.Substring(7);
                }

                if (identifier.EndsWith("/"))
                {
                    identifier = identifier.Substring(0, identifier.Length - 1);
                }

                int i = 0;
                log.Debug("Attempting to select mod from startup parameters");
                foreach (DataGridViewRow row in ModList.Rows)
                {
                    var module = ((GUIMod)row.Tag).ToCkanModule();
                    if (identifier == module.identifier)
                    {
                        ModList.FirstDisplayedScrollingRowIndex = i;
                        row.Selected = true;
                        break;
                    }

                    i++;
                }
                log.Debug("Failed to select mod from startup parameters");
            }

            var pluginsPath = Path.Combine(CurrentInstance.CkanDir(), "Plugins");
            if (!Directory.Exists(pluginsPath))
            {
                Directory.CreateDirectory(pluginsPath);
            }

            m_PluginController = new PluginController(pluginsPath, true);

            log.Info("GUI started");
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            m_Configuration.WindowLoc = Location;

            // Copy window size to app settings
            m_Configuration.WindowSize = WindowState == FormWindowState.Normal ? Size : RestoreBounds.Size;

            // Save settings
            m_Configuration.Save();
            base.OnFormClosing(e);
        }

        public void CurrentInstanceUpdated()
        {
            Util.Invoke(this, () =>
            {
                Text = String.Format("CKAN {0} - KSP {1}    --    {2}", Meta.Version(), CurrentInstance.Version(),
                CurrentInstance.GameDir());
                KSPVersionLabel.Text = String.Format("Kerbal Space Program {0}", CurrentInstance.Version());
            });

            // Update the settings dialog to reflect the changes made.
            Util.Invoke(this.m_SettingsDialog, () =>
            {
                this.m_SettingsDialog.UpdateDialog();
            });

            m_Configuration = Configuration.LoadOrCreateConfiguration
            (
                Path.Combine(CurrentInstance.GameDir(), "CKAN/GUIConfig.xml"),
                Repo.default_ckan_repo.ToString()
            );
            UpdateModsList();
            ChangeSet = null;
            Conflicts = null;
        }

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            UpdateRepo();
        }

        private void MarkAllUpdatesToolButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in ModList.Rows)
            {
                var mod = ((GUIMod) row.Tag);
                if (mod.HasUpdate && row.Cells[1] is DataGridViewCheckBoxCell)
                {
                    mod.SetUpgradeChecked(row, true);
                    ApplyToolButton.Enabled = true;
                }
            }

            ModList.Refresh();
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var module = GetSelectedModule();
            
            this.ModInfoTabControl.Enabled = module!=null;
            if (module == null) return;

            this.ModInfoTabControl.Enabled = true;

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
            if (Platform.IsMac)
            {
                // Delay updating to improve typing performance on OS X
                RunFilterUpdateTimer();
            }
            else
            {
                mainModList.ModNameFilter = FilterByNameTextBox.Text;
            }
        }

        private void FilterByAuthorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Platform.IsMac)
            {
                // Delay updating to improve typing performance on OS X
                RunFilterUpdateTimer();
            }
            else
            {
                mainModList.ModAuthorFilter = FilterByAuthorTextBox.Text;
            }
        }

        /// <summary>
        /// Start or restart a timer to update the filter after an interval 
        /// since the last keypress. On Mac OS X, this prevents the search 
        /// field from locking up due to DataGridViews being slow and
        /// key strokes being interpreted incorrectly when slowed down:
        /// http://mono.1490590.n4.nabble.com/Incorrect-missing-and-duplicate-keypress-events-td4658863.html
        /// </summary>
        private void RunFilterUpdateTimer() {
            if (filterTimer == null)
            {
                filterTimer = new Timer();
                filterTimer.Tick += OnFilterUpdateTimer;
                filterTimer.Interval = 700;
                filterTimer.Start();
            }
            else
            {
                filterTimer.Stop();
                filterTimer.Start();
            }
        }

        /// <summary>
        /// Updates the filter after an interval of time has passed since the 
        /// last keypress.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="e">EventArgs</param>
        private void OnFilterUpdateTimer(Object source, EventArgs e)
        {
            mainModList.ModNameFilter = FilterByNameTextBox.Text;
            mainModList.ModAuthorFilter = FilterByAuthorTextBox.Text;
            filterTimer.Stop();
        }

        /// <summary>
        /// Called on key press when the mod is focused. Scrolls to the first mod 
        /// with name begining with the key pressed. If space is pressed, the checkbox
        /// at the current row is toggled.
        /// </summary>        
        private void ModList_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check the key. If it is space, mark the current mod as selected.
            if (e.KeyChar.ToString() == " ")
            {
                var selected_row = ModList.CurrentRow;

                if (selected_row != null)
                {
                    // Get the checkbox.
                    var selected_row_check_box = selected_row.Cells["Installed"] as DataGridViewCheckBoxCell;

                    // Invert the value.
                    if (selected_row_check_box != null)
                    {
                        bool selected_value = (bool)selected_row_check_box.Value;
                        selected_row_check_box.Value = !selected_value;
                    }                    
                }
                e.Handled = true;
                return;
            }

            var rows = ModList.Rows.Cast<DataGridViewRow>().Where(row => row.Visible);
            var does_name_begin_with_char = new Func<DataGridViewRow, bool>(row =>
            {
                var modname = ((GUIMod) row.Tag).ToCkanModule().name;
                var key = e.KeyChar.ToString();
                return modname.StartsWith(key, StringComparison.OrdinalIgnoreCase);
            });
            ModList.ClearSelection();
            DataGridViewRow match = rows.FirstOrDefault(does_name_begin_with_char);
            if (match != null)
            {
                match.Selected = true;
                ModList.CurrentCell = match.Cells[0];
            }
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
            var row_index = e.RowIndex;
            var column_index = e.ColumnIndex;

            if (row_index < 0 || column_index < 0)
            {
                return;
            }
            var registry_manager = RegistryManager.Instance(CurrentInstance);

            var grid = sender as DataGridView;

            var row = grid.Rows[row_index];
            var grid_view_cell = row.Cells[column_index];

            if (grid_view_cell is DataGridViewLinkCell)
            {
                var cell = grid_view_cell as DataGridViewLinkCell;
                Process.Start(cell.Value.ToString());
            }
            else if (column_index < 2)
            {
                var gui_mod = ((GUIMod) row.Tag);
                switch (column_index)
                {
                    case 0:
                        gui_mod.SetInstallChecked(row);
                        break;
                    case 1:
                        gui_mod.SetUpgradeChecked(row);
                        break;
                }

                var registry = registry_manager.registry;
                UpdateChangeSetAndConflicts(registry);
            }
        }

        private void UpdateChangeSetAndConflicts(Registry registry)
        {
            IEnumerable<KeyValuePair<CkanModule, GUIModChangeType>> full_change_set;
            Dictionary<Module, string> conflicts;

            var user_change_set = mainModList.ComputeUserChangeSet();
            try
            {
                var module_installer = ModuleInstaller.GetInstance(CurrentInstance, GUI.user);
                full_change_set = MainModList.ComputeChangeSetFromModList(registry, user_change_set, module_installer,
                    CurrentInstance.Version());
                conflicts = null;
            }
            catch (InconsistentKraken)
            {
                conflicts = MainModList.ComputeConflictsFromModList(registry, user_change_set, CurrentInstance.Version());
                full_change_set = null;
            }

            Conflicts = conflicts;
            ChangeSet = full_change_set;
        }

        private void FilterCompatibleButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.Compatible;
            FilterToolButton.Text = "Filter (Compatible)";
        }

        private void FilterInstalledButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.Installed;
            FilterToolButton.Text = "Filter (Installed)";
        }

        private void FilterInstalledUpdateButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.InstalledUpdateAvailable;
            FilterToolButton.Text = "Filter (Upgradeable)";
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

        private void FilterAllButton_Click(object sender, EventArgs e)
        {
            mainModList.ModFilter = GUIModFilter.All;
            FilterToolButton.Text = "Filter (All)";
        }

        private void ContentsDownloadButton_Click(object sender, EventArgs e)
        {
            var module = GetSelectedModule();
            if (module == null) return;
            ResetProgress();
            ShowWaitDialog(false);
            ModuleInstaller.GetInstance(CurrentInstance, GUI.user).CachedOrDownload(module);
            HideWaitDialog(true);

            UpdateModContentsTree(module);
            RecreateDialogs();
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.OpenLinkFromLinkLabel(sender as LinkLabel);
        }

        private void ModuleRelationshipType_SelectedIndexChanged(object sender, EventArgs e)
        {
            CkanModule module = GetSelectedModule();
            if (module == null) return;
            UpdateModDependencyGraph(module);
        }

        private CkanModule GetSelectedModule()
        {
            if (ModList.SelectedRows.Count == 0)
            {
                return null;
            }

            DataGridViewRow selected_item = ModList.SelectedRows[0];
            if (selected_item == null)
            {
                return null;
            }

            var module = ((GUIMod) selected_item.Tag).ToCkanModule();            
            return module;
        }


        private void launchKSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var split = m_Configuration.CommandLineArguments.Split(' ');
            if (split.Length == 0)
            {
                return;
            }

            var binary = split[0];
            var args = string.Join(" ",split.Skip(1));
            
            try
            {
                Directory.SetCurrentDirectory(CurrentInstance.GameDir());
                Process.Start(binary, args);
            }
            catch (Exception exception)
            {
                GUI.user.RaiseError("Couldn't start KSP. {0}.", exception.Message);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog();
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

        private void pluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Enabled = false;
            m_PluginsDialog.ShowDialog();
            Enabled = true;
        }

        

        private void installFromckanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog {Filter = Resources.CKANFileFilter};

            if (open_file_dialog.ShowDialog() == DialogResult.OK)
            {
                var path = open_file_dialog.FileName;
                CkanModule module = null;

                try
                {
                    module = CkanModule.FromFile(path);
                }
                catch (Kraken kraken)
                {
                    m_User.RaiseError(kraken.Message + ": " + kraken.InnerException.Message);
                    return;
                }
                catch (Exception ex)
                {
                    m_User.RaiseError(ex.Message);
                    return;
                }

                // We'll need to make some registry changes to do this.
                RegistryManager registry_manager = RegistryManager.Instance(CurrentInstance);

                // Remove this version of the module in the registry, if it exists.
                registry_manager.registry.RemoveAvailable(module);

                // Sneakily add our version in...
                registry_manager.registry.AddAvailable(module);

                var changeset = new List<KeyValuePair<CkanModule, GUIModChangeType>>();
                changeset.Add(new KeyValuePair<CkanModule, GUIModChangeType>(module, GUIModChangeType.Install));

                menuStrip1.Enabled = false;

                RelationshipResolverOptions install_ops = RelationshipResolver.DefaultOpts();
                install_ops.with_recommends = false;

                m_InstallWorker.RunWorkerAsync(
                    new KeyValuePair<List<KeyValuePair<CkanModule, GUIModChangeType>>, RelationshipResolverOptions>(
                        changeset, install_ops));
                m_Changeset = null;

                UpdateChangesDialog(null, m_InstallWorker);
                ShowWaitDialog();
            }
        }

        /// <summary>
        /// Exports installed mods to a .ckan file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportModListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = Resources.CKANFileFilter;
            dlg.Title = Resources.ExportInstalledModsDialogTitle;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                // Save, just to be certain that the installed-*.ckan metapackage is generated
                RegistryManager.Instance(CurrentInstance).Save();

                // TODO: The core might eventually save as something other than 'installed-default.ckan'
                File.Copy(Path.Combine(CurrentInstance.CkanDir(), "installed-default.ckan"), dlg.FileName);
            }
        }
    }

    public class GUIUser : NullUser
    {
        public delegate bool DisplayYesNo(string message);

        public Action<string, object[]> displayMessage;
        public Action<string, object[]> displayError;
        public DisplayYesNo displayYesNo;


        protected override bool DisplayYesNoDialog(string message)
        {
            if (displayYesNo == null)
            {
                return true;
            }

            return displayYesNo(message);
        }

        protected override void DisplayMessage(string message, params object[] args)
        {
            displayMessage(message, args);
        }

        protected override void DisplayError(string message, params object[] args)
        {
            displayError(message, args);
        }

        protected override void ReportProgress(string format, int percent)
        {
            Main.Instance.SetDescription(format + " - " + percent + "%");
            Main.Instance.SetProgress(percent);
        }

        public override int WindowWidth
        {
            get { return -1; }
        }
    }
}