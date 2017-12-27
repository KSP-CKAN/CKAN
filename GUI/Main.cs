﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CKAN.Exporters;
using CKAN.Properties;
using CKAN.Types;
using log4net;
using Timer = System.Windows.Forms.Timer;

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
        All = 6,
        Cached = 7
    }

    public enum GUIModChangeType
    {
        None = 0,
        Install = 1,
        Remove = 2,
        Update = 3
    }

    public partial class Main: Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Main));

        public delegate void ModChangedCallback(CkanModule module, GUIModChangeType change);

        public static event ModChangedCallback modChangedCallback;

        public Configuration configuration;

        public ControlFactory controlFactory;

        public TabController tabController;

        public PluginController pluginController;

        public volatile KSPManager manager;

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

        public NavigationHistory<GUIMod> navHistory;

        public string[] commandLineArgs;

        public GUIUser currentUser;

        private Timer filterTimer;

        private DateTime lastSearchTime;
        private string lastSearchKey;

        private IEnumerable<ModChange> currentChangeSet;
        private Dictionary<GUIMod, string> conflicts;

        private IEnumerable<ModChange> ChangeSet
        {
            get { return currentChangeSet; }
            set
            {
                var orig = currentChangeSet;
                currentChangeSet = value;
                if(!ReferenceEquals(orig, value)) ChangeSetUpdated();
            }
        }

        private Dictionary<GUIMod, string> Conflicts
        {
            get { return conflicts; }
            set
            {
                var orig = conflicts;
                conflicts = value;
                if(orig != value) ConflictsUpdated();
            }
        }

        private void ConflictsUpdated()
        {
            foreach (DataGridViewRow row in ModList.Rows)
            {
                var module = ((GUIMod) row.Tag);
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
                    if (row.DefaultCellStyle.BackColor != Color.Empty)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.ToolTipText = null;
                        }

                        row.DefaultCellStyle.BackColor = Color.Empty;
                        ModList.InvalidateRow(row.Index);
                    }
                }
            }
        }

        private void ChangeSetUpdated()
        {
            if (ChangeSet != null && ChangeSet.Any())
            {
                UpdateChangesDialog(ChangeSet.ToList(), installWorker);
                tabController.ShowTab("ChangesetTabPage", 1, false);
                ApplyToolButton.Enabled = true;
            }
            else
            {
                tabController.HideTab("ChangesetTabPage");
                ApplyToolButton.Enabled = false;
            }
        }

        public Main(string[] cmdlineArgs, GUIUser user, bool showConsole)
        {
            log.Info("Starting the GUI");
            commandLineArgs = cmdlineArgs;
            currentUser = user;

            user.displayMessage = AddStatusMessage;
            user.displayError = ErrorDialog;

            controlFactory = new ControlFactory();
            Instance = this;
            mainModList = new MainModList(source => UpdateFilters(this), TooManyModsProvide, user);

            navHistory = new NavigationHistory<GUIMod>();
            navHistory.IsReadOnly = true; // read-only until the UI is started.
                                            // we switch out of it at the end of OnLoad()
                                            // when we call NavInit()

            InitializeComponent();

            // We need to initialize error dialog first to display errors
            errorDialog = controlFactory.CreateControl<ErrorDialog>();

            // We want to check our current instance is null first, as it may
            // have already been set by a command-line option.
            Manager = new KSPManager(user);
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

            configuration = Configuration.LoadOrCreateConfiguration
                (
                    Path.Combine(CurrentInstance.GameDir(), "CKAN/GUIConfig.xml"),
                    Repo.default_ckan_repo.ToString()
                );

            // Check if there is any other instances already running.
            // This is not entirely necessary, but we can show a nicer error message this way.
            try
            {
                #pragma warning disable 219
                var lockedReg = RegistryManager.Instance(CurrentInstance).registry;
                #pragma warning restore 219
            }
            catch (RegistryInUseKraken kraken)
            {
                errorDialog.ShowErrorDialog(kraken.ToString());

                return;
            }

            FilterToolButton.MouseHover += (sender, args) => FilterToolButton.ShowDropDown();
            launchKSPToolStripMenuItem.MouseHover += (sender, args) => launchKSPToolStripMenuItem.ShowDropDown();
            ApplyToolButton.MouseHover += (sender, args) => ApplyToolButton.ShowDropDown();

            ModList.CurrentCellDirtyStateChanged += ModList_CurrentCellDirtyStateChanged;
            ModList.CellValueChanged += ModList_CellValueChanged;

            tabController = new TabController(MainTabControl);
            tabController.ShowTab("ManageModsTabPage");

            RecreateDialogs();

            if (!showConsole)
            {
                Util.HideConsoleWindow();
            }

            // Disable the modinfo controls until a mod has been choosen.
            ModInfoTabControl.SelectedModule = null;

            // WinForms on Mac OS X has a nasty bug where the UI thread hogs the CPU,
            // making our download speeds really slow unless you move the mouse while
            // downloading. Yielding periodically addresses that.
            // https://bugzilla.novell.com/show_bug.cgi?id=663433
            if (Platform.IsMac)
            {
                var timer = new Timer { Interval = 2 };
                timer.Tick += (sender, e) => {
                    Thread.Yield();
                };
                timer.Start();
            }

            Application.Run(this);

            var registry = RegistryManager.Instance(Manager.CurrentInstance);
            if (registry != null)
            {
                registry.Dispose();
            }
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
            Location = configuration.WindowLoc;
            Size = configuration.WindowSize;
            WindowState = configuration.IsWindowMaximised ? FormWindowState.Maximized : FormWindowState.Normal;
            splitContainer1.SplitterDistance = configuration.PanelPosition;
            ModInfoTabControl.ModMetaSplitPosition = configuration.ModInfoPosition;

            if (!configuration.CheckForUpdatesOnLaunchNoNag && AutoUpdate.CanUpdate)
            {
                log.Debug("Asking user if they wish for autoupdates");
                if (new AskUserForAutoUpdatesDialog().ShowDialog() == DialogResult.OK)
                {
                    configuration.CheckForUpdatesOnLaunch = true;
                }

                configuration.CheckForUpdatesOnLaunchNoNag = true;
                configuration.Save();
            }

            bool autoUpdating = false;

            if (configuration.CheckForUpdatesOnLaunch && AutoUpdate.CanUpdate)
            {
                try
                {
                    log.Info("Making autoupdate call");
                    AutoUpdate.Instance.FetchLatestReleaseInfo();
                    var latest_version = AutoUpdate.Instance.LatestVersion;
                    var current_version = new Version(Meta.GetVersion());

                    if (AutoUpdate.Instance.IsFetched() && latest_version.IsGreaterThan(current_version))
                    {
                        log.Debug("Found higher ckan version");
                        var release_notes = AutoUpdate.Instance.ReleaseNotes;
                        var dialog = new NewUpdateDialog(latest_version.ToString(), release_notes);
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            UpdateCKAN();
                            autoUpdating = true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    currentUser.RaiseError("Error in autoupdate: \n\t" + exception.Message + "");
                    log.Error("Error in autoupdate", exception);
                }
            }

            m_UpdateRepoWorker = new BackgroundWorker { WorkerReportsProgress = false, WorkerSupportsCancellation = true };

            m_UpdateRepoWorker.RunWorkerCompleted += PostUpdateRepo;
            m_UpdateRepoWorker.DoWork += UpdateRepo;

            installWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            installWorker.RunWorkerCompleted += PostInstallMods;
            installWorker.DoWork += InstallMods;

            var old_YesNoDialog = currentUser.displayYesNo;
            currentUser.displayYesNo = YesNoDialog;
            URLHandlers.RegisterURLHandler(configuration, currentUser);
            currentUser.displayYesNo = old_YesNoDialog;

            ApplyToolButton.Enabled = false;

            CurrentInstanceUpdated();

            // if we're autoUpdating then we shouldn't interfere progress tab
            if (configuration.RefreshOnStartup && !autoUpdating)
            {
                UpdateRepo();
            }

            Text = String.Format("CKAN {0} - KSP {1}  --  {2}", Meta.GetVersion(), CurrentInstance.Version(),
                CurrentInstance.GameDir());

            if (commandLineArgs.Length >= 2)
            {
                var identifier = commandLineArgs[1];
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

                log.Debug("Attempting to select mod from startup parameters");
                FocusMod(identifier, true, true);
                ModList.Refresh();
                log.Debug("Failed to select mod from startup parameters");
            }

            var pluginsPath = Path.Combine(CurrentInstance.CkanDir(), "Plugins");
            if (!Directory.Exists(pluginsPath))
            {
                Directory.CreateDirectory(pluginsPath);
            }

            pluginController = new PluginController(pluginsPath, true);

            CurrentInstance.RebuildKSPSubDir();

            NavInit();  // initialize navigation. this should be called as late
                        // as possible, once the UI is "settled" from its initial
                        // load.

            log.Info("GUI started");
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //Only close the window, when the user has access to the "Exit" of the menu
            if (!menuStrip1.Enabled)
            {
                e.Cancel = true;
                return;
            }

            // Copy window location to app settings
            configuration.WindowLoc = Location;

            // Copy window size to app settings if not maximized
            configuration.WindowSize = WindowState == FormWindowState.Normal ? Size : RestoreBounds.Size;

            //copy window maximized state to app settings
            configuration.IsWindowMaximised = WindowState == FormWindowState.Maximized ? true : false;

            // Copy panel position to app settings
            configuration.PanelPosition = splitContainer1.SplitterDistance;

            // Copy metadata panel split height to app settings
            configuration.ModInfoPosition = ModInfoTabControl.ModMetaSplitPosition;

            // Save the active filter
            configuration.ActiveFilter = (int)mainModList.ModFilter;

            // Save settings
            configuration.Save();
            base.OnFormClosing(e);
        }

        public void CurrentInstanceUpdated()
        {
            Util.Invoke(this, () =>
            {
                Text = String.Format("CKAN {0} - KSP {1}    --    {2}", Meta.GetVersion(), CurrentInstance.Version(),
                CurrentInstance.GameDir());
            });

            configuration = Configuration.LoadOrCreateConfiguration
            (
                Path.Combine(CurrentInstance.GameDir(), "CKAN/GUIConfig.xml"),
                Repo.default_ckan_repo.ToString()
            );

            if (CurrentInstance.CompatibleVersionsAreFromDifferentKsp)
            {
                CompatibleKspVersionsDialog dialog = new CompatibleKspVersionsDialog(CurrentInstance);
                dialog.ShowDialog();
            }

            UpdateModsList();
            ChangeSet = null;
            Conflicts = null;

            Filter((GUIModFilter)configuration.ActiveFilter);
        }

        public void UpdateCKAN()
        {
            ResetProgress();
            ShowWaitDialog(false);
            SwitchEnabledState();
            ClearLog();
            tabController.RenameTab("WaitTabPage", "Updating CKAN");
            SetDescription("Upgrading CKAN to " + AutoUpdate.Instance.LatestVersion);

            log.Info("Start ckan update");
            BackgroundWorker updateWorker = new BackgroundWorker();
            updateWorker.DoWork += (sender, args) => AutoUpdate.Instance.StartUpdateProcess(true, GUI.user);
            updateWorker.RunWorkerAsync();
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
                    MarkModForUpdate(mod.Identifier);
                    mod.SetUpgradeChecked(row, true);
                    ApplyToolButton.Enabled = true;
                }
            }

            ModList.Refresh();
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var module = GetSelectedModule();

            this.AddStatusMessage("");

            this.ModInfoTabControl.SelectedModule = module;
            if (module == null) return;

            NavSelectMod(module);
        }

        public void UpdateModContentsTree(CkanModule module, bool force = false)
        {
            ModInfoTabControl.UpdateModContentsTree(module, force);
        }

        private void ApplyToolButton_Click(object sender, EventArgs e)
        {
            tabController.ShowTab("ChangesetTabPage", 1);
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

        private void FilterByDescriptionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Platform.IsMac)
            {
                // Delay updating to improve typing performance on OS X
                RunFilterUpdateTimer();
            }
            else
            {
                mainModList.ModDescriptionFilter = FilterByDescriptionTextBox.Text;
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
            mainModList.ModDescriptionFilter = FilterByDescriptionTextBox.Text;
            filterTimer.Stop();
        }

        /// <summary>
        /// Programmatic implementation of row sorting by columns.
        /// </summary>
        private void ModList_HeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var new_sort_column = this.ModList.Columns[e.ColumnIndex];
            var current_sort_column = this.ModList.Columns[this.configuration.SortByColumnIndex];
            // Reverse the sort order if the current sorting column is clicked again
            this.configuration.SortDescending = new_sort_column == current_sort_column ? !this.configuration.SortDescending : false;
            // Reset the glyph
            current_sort_column.HeaderCell.SortGlyphDirection = SortOrder.None;
            this.configuration.SortByColumnIndex = new_sort_column.Index;
            this.UpdateFilters(this);
        }

        /// <summary>
        /// Called on key down when the mod list is focused.
        /// Makes the Home/End keys go to the top/bottom of the list respectively.
        /// </summary>
        private void ModList_KeyDown(object sender, KeyEventArgs e)
        {
            DataGridViewCell cell = null;
            switch (e.KeyCode)
            {
                case Keys.Home:
                    // First row
                    cell = ModList.Rows [0].Cells [2];
                    break;
                case Keys.End:
                    // Last row
                    cell = ModList.Rows [ModList.Rows.Count - 1].Cells [2];
                    break;
            }
            if (cell != null)
            {
                e.Handled = true;
                // Selects the top/bottom row and scrolls the list to it
                ModList.CurrentCell = cell;
            }
        }

        /// <summary>
        /// Called on key press when the mod is focused. Scrolls to the first mod
        /// with name beginning with the key pressed. If more than one unique keys are pressed
        /// in under a second, it searches for the combination of the keys pressed.
        /// If the same key is being pressed repeatedly, it cycles through mods names
        /// beginning with that key. If space is pressed, the checkbox at the current row is toggled.
        /// </summary>
        private void ModList_KeyPress(object sender, KeyPressEventArgs e)
        {
            var current_row = ModList.CurrentRow;
            var key = e.KeyChar.ToString();
            // Check the key. If it is space and the current row is selected, mark the current mod as selected.
            if (key == " ")
            {
                if (current_row != null && current_row.Selected)
                {
                    var gui_mod = ((GUIMod)current_row.Tag);
                    if (gui_mod.IsInstallable())
                    {
                        MarkModForInstall(gui_mod.Identifier,uncheck:gui_mod.IsInstallChecked);
                    }
                }
                e.Handled = true;
                return;
            }
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Don't try to search for newlines
                return;
            }

            // Determine time passed since last key press
            TimeSpan interval = DateTime.Now - this.lastSearchTime;
            if (interval.TotalSeconds < 1)
            {
                // Last keypress was < 1 sec ago, so combine the last and current keys
                key = this.lastSearchKey + key;
            }
            // Remember the current time and key
            this.lastSearchTime = DateTime.Now;
            this.lastSearchKey = key;

            if (key.Distinct().Count() == 1)
            {
                // Treat repeating and single keypresses the same
                key = key.Substring(0, 1);
            }

            FocusMod(key, false);
            e.Handled = true;
        }

        /// <summary>
        /// I'm pretty sure this is what gets called when the user clicks on a ticky
        /// in the mod list.
        /// </summary>
        private void ModList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            ModList.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void ModList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            DataGridViewRow row = ModList.Rows[e.RowIndex];
            if (!(row.Cells[0] is DataGridViewCheckBoxCell))
            {
                return;
            }
            // Need to change the state here, because the user hasn't clicked on a checkbox
            row.Cells[0].Value = !(bool)row.Cells[0].Value;
            ModList.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private async void ModList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
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
                var gui_mod = ((GUIMod)row.Tag);
                switch (column_index)
                {
                    case 0:
                        gui_mod.SetInstallChecked(row);
                        if(gui_mod.IsInstallChecked)
                            last_mod_to_have_install_toggled.Push(gui_mod);
                        break;
                    case 1:
                        gui_mod.SetUpgradeChecked(row);
                        break;
                }

                var registry = registry_manager.registry;
                await UpdateChangeSetAndConflicts(registry);
            }
        }

        private async Task UpdateChangeSetAndConflicts(IRegistryQuerier registry)
        {
            IEnumerable<ModChange> full_change_set = null;
            Dictionary<GUIMod, string> new_conflicts = null;

            bool too_many_provides_thrown = false;
            var user_change_set = mainModList.ComputeUserChangeSet();
            try
            {
                var module_installer = ModuleInstaller.GetInstance(CurrentInstance, GUI.user);
                full_change_set =
                    await mainModList.ComputeChangeSetFromModList(registry, user_change_set, module_installer,
                    CurrentInstance.VersionCriteria());
            }
            catch (InconsistentKraken)
            {
                //Need to be recomputed due to ComputeChangeSetFromModList possibly changing it with too many provides handling.
                user_change_set = mainModList.ComputeUserChangeSet();
                new_conflicts = MainModList.ComputeConflictsFromModList(registry, user_change_set, CurrentInstance.VersionCriteria());
                full_change_set = null;
            }
            catch (TooManyModsProvideKraken)
            {
                //Can be thrown by ComputeChangeSetFromModList if the user cancels out of it.
                //We can just rerun it as the ModInfoTabControl has been removed.
                too_many_provides_thrown = true;
            }
            if (too_many_provides_thrown)
            {
                await UpdateChangeSetAndConflicts(registry);
                new_conflicts = Conflicts;
                full_change_set = ChangeSet;
            }
            last_mod_to_have_install_toggled.Clear();
            Conflicts = new_conflicts;
            ChangeSet = full_change_set;
        }

        private void FilterCompatibleButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Compatible);
        }

        private void FilterInstalledButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Installed);
        }

        private void FilterInstalledUpdateButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.InstalledUpdateAvailable);
        }

        private void cachedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Cached);
        }

        private void FilterNewButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.NewInRepository);
        }

        private void FilterNotInstalledButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.NotInstalled);
        }

        private void FilterIncompatibleButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Incompatible);
        }

        private void FilterAllButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.All);
        }

        private void Filter(GUIModFilter filter)
        {
            mainModList.ModFilter = filter;

            if (filter == GUIModFilter.All)
                FilterToolButton.Text = "Filter (All)";
            else if (filter == GUIModFilter.Incompatible)
                FilterToolButton.Text = "Filter (Incompatible)";
            else if (filter == GUIModFilter.Installed)
                FilterToolButton.Text = "Filter (Installed)";
            else if (filter == GUIModFilter.InstalledUpdateAvailable)
                FilterToolButton.Text = "Filter (Upgradeable)";
            else if (filter == GUIModFilter.Cached)
                FilterToolButton.Text = "Filter (Cached)";
            else if (filter == GUIModFilter.NewInRepository)
                FilterToolButton.Text = "Filter (New)";
            else if (filter == GUIModFilter.NotInstalled)
                FilterToolButton.Text = "Filter (Not installed)";
            else
                FilterToolButton.Text = "Filter (Compatible)";
        }

        private GUIMod GetSelectedModule()
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


            var module = ((GUIMod) selected_item.Tag);
            return module;
        }


        private void launchKSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var split = configuration.CommandLineArguments.Split(' ');
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
            if (dialog.ShowKSPCommandLineOptionsDialog(configuration.CommandLineArguments) == DialogResult.OK)
            {
                configuration.CommandLineArguments = dialog.GetResult();
                configuration.Save();
            }
        }

        private void CKANSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Flipping enabled here hides the main form itself.
            Enabled = false;
            settingsDialog.ShowDialog();
            Enabled = true;
        }

        private void pluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Enabled = false;
            pluginsDialog.ShowDialog();
            Enabled = true;
        }



        private void installFromckanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog {Filter = Resources.CKANFileFilter};

            if (open_file_dialog.ShowDialog() == DialogResult.OK)
            {
                var path = open_file_dialog.FileName;
                CkanModule module;

                try
                {
                    module = CkanModule.FromFile(path);
                }
                catch (Kraken kraken)
                {
                    currentUser.RaiseError(kraken.InnerException == null
                        ? kraken.Message
                        : $"{kraken.Message}: {kraken.InnerException.Message}");
                    return;
                }
                catch (Exception ex)
                {
                    currentUser.RaiseError(ex.Message);
                    return;
                }

                // We'll need to make some registry changes to do this.
                RegistryManager registry_manager = RegistryManager.Instance(CurrentInstance);

                // Remove this version of the module in the registry, if it exists.
                registry_manager.registry.RemoveAvailable(module);

                // Sneakily add our version in...
                registry_manager.registry.AddAvailable(module);

                var changeset = new List<ModChange>();
                changeset.Add(new ModChange(
                    new GUIMod(module,registry_manager.registry,CurrentInstance.VersionCriteria()),
                    GUIModChangeType.Install, null));

                menuStrip1.Enabled = false;

                RelationshipResolverOptions install_ops = RelationshipResolver.DefaultOpts();
                install_ops.with_recommends = false;

                installWorker.RunWorkerAsync(
                    new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                        changeset, install_ops));
                changeSet = null;

                UpdateChangesDialog(null, installWorker);
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
            var exportOptions = new List<ExportOption>
            {
                new ExportOption(ExportFileType.CkanFavourite, "CKAN favourites list (*.ckan)", "ckan"),
                new ExportOption(ExportFileType.Ckan, "CKAN modpack (enforces exact mod versions) (*.ckan)", "ckan"),
                new ExportOption(ExportFileType.PlainText, "Plain text (*.txt)", "txt"),
                new ExportOption(ExportFileType.Markdown, "Markdown (*.md)", "md"),
                new ExportOption(ExportFileType.BbCode, "BBCode (*.txt)", "txt"),
                new ExportOption(ExportFileType.Csv, "Comma-separated values (*.csv)", "csv"),
                new ExportOption(ExportFileType.Tsv, "Tab-separated values (*.tsv)", "tsv")
            };

            var filter = string.Join("|", exportOptions.Select(i => i.ToString()).ToArray());

            var dlg = new SaveFileDialog
            {
                Filter = filter,
                Title = Resources.ExportInstalledModsDialogTitle
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var exportOption = exportOptions[dlg.FilterIndex - 1]; // FilterIndex is 1-indexed

                if (exportOption.ExportFileType == ExportFileType.Ckan || exportOption.ExportFileType == ExportFileType.CkanFavourite)
                {
                    bool recommends = false;
                    bool versions = true;

                    if (exportOption.ExportFileType == ExportFileType.CkanFavourite)
                    {
                        recommends = true;
                        versions = false;
                    }

                    // Save, just to be certain that the installed-*.ckan metapackage is generated
                    RegistryManager.Instance(CurrentInstance).Save(true, recommends, versions);

                    // TODO: The core might eventually save as something other than 'installed-default.ckan'
                    File.Copy(Path.Combine(CurrentInstance.CkanDir(), "installed-default.ckan"), dlg.FileName, true);
                }
                else
                {
                    var fileMode = File.Exists(dlg.FileName) ? FileMode.Truncate : FileMode.CreateNew;

                    using (var stream = new FileStream(dlg.FileName, fileMode))
                    {
                        var registry = RegistryManager.Instance(CurrentInstance).registry;

                        new Exporter(exportOption.ExportFileType).Export(registry, stream);
                    }
                }
            }
        }

        private void selectKSPInstallMenuItem_Click(object sender, EventArgs e)
        {
            Instance.Manager.ClearAutoStart();
            var old_instance = Instance.CurrentInstance;
            var result = new ChooseKSPInstance().ShowDialog();
            if (result == DialogResult.OK && !Equals(old_instance, Instance.CurrentInstance))
                Instance.CurrentInstanceUpdated();
        }

        private void openKspDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Instance.manager.CurrentInstance.GameDir());
        }

        private void CompatibleKspVersionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var instanceSettingsDialog = new CompatibleKspVersionsDialog(Instance.manager.CurrentInstance);
            instanceSettingsDialog.ShowDialog();
            UpdateModsList(repo_updated: false);
        }

        public void ResetFilterAndSelectModOnList(string key)
        {
            FilterByNameTextBox.Text = "";
            mainModList.ModNameFilter = "";
            FocusMod(key, true);
        }

        private void FocusMod(string key, bool exactMatch, bool showAsFirst=false)
        {
            DataGridViewRow current_row = ModList.CurrentRow;
            int currentIndex = current_row != null ? current_row.Index : 0;
            DataGridViewRow first_match = null;

            var does_name_begin_with_key = new Func<DataGridViewRow, bool>(row =>
            {
                GUIMod mod = row.Tag as GUIMod;
                bool row_match = false;
                if (exactMatch)
                {
                    row_match = mod.Name == key || mod.Identifier == key;
                }
                else
                {
                    row_match = mod.Name.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                        mod.Abbrevation.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                        mod.Identifier.StartsWith(key, StringComparison.OrdinalIgnoreCase);
                }
                if (row_match && first_match == null)
                {
                    // Remember the first match to allow cycling back to it if necessary
                    first_match = row;
                }
                if (key.Length == 1 && row_match && row.Index <= currentIndex)
                {
                    // Keep going forward if it's a single key match and not ahead of the current row
                    return false;
                }
                return row_match;
            });
            ModList.ClearSelection();
            var rows = ModList.Rows.Cast<DataGridViewRow>().Where(row => row.Visible);
            DataGridViewRow match = rows.FirstOrDefault(does_name_begin_with_key);
            if (match == null && first_match != null)
            {
                // If there were no matches after the first match, cycle over to the beginning
                match = first_match;
            }
            if (match != null)
            {
                match.Selected = true;
                // Setting this to the Name cell prevents the checkbox from being toggled
                // by pressing Space while the row is not indicated as active
                ModList.CurrentCell = match.Cells[2];
                if (showAsFirst)
                    ModList.FirstDisplayedScrollingRowIndex = match.Index;
            }
            else
            {
                this.AddStatusMessage("Not found");
            }
        }

        private void RecommendedModsToggleCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var state = ((CheckBox)sender).Checked;
            foreach (ListViewItem item in RecommendedModsListView.Items)
            {
                if (item.Checked != state)
                {
                    item.Checked = state;
                }
            }
            RecommendedModsListView.Refresh();
        }

        #region Navigation History

        void NavInit()
        {
            navHistory.OnHistoryChange += NavOnHistoryChange;
            navHistory.IsReadOnly = false;
            var currentMod = GetSelectedModule();
            if (currentMod != null)
            {
                navHistory.AddToHistory(currentMod);
            }
        }

        void NavUpdateUI()
        {
            NavBackwardToolButton.Enabled = navHistory.CanNavigateBackward;
            NavForwardToolButton.Enabled = navHistory.CanNavigateForward;
        }

        void NavSelectMod(GUIMod module)
        {
            navHistory.AddToHistory(module);
        }

        void NavGoBackward()
        {
            if (navHistory.CanNavigateBackward)
            {
                NavGoToMod(navHistory.NavigateBackward());
            }
        }

        void NavGoForward()
        {
            if (navHistory.CanNavigateForward)
            {
                NavGoToMod(navHistory.NavigateForward());
            }
        }

        void NavGoToMod(GUIMod module)
        {
            // focussing on a mod also causes navigation, but we don't
            // want this to affect the history. so we switch to read-only
            // mode.
            navHistory.IsReadOnly = true;
            FocusMod(module.Name, true);
            navHistory.IsReadOnly = false;
        }

        void NavOnHistoryChange()
        {
            NavUpdateUI();
        }

        void NavBackwardToolButton_Click(object sender, EventArgs e)
        {
            NavGoBackward();
        }

        void NavForwardToolButton_Click(object sender, EventArgs e)
        {
            NavGoForward();
        }

        #endregion

        private void reportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/KSP-CKAN/NetKAN/issues/new");
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

    }
}
