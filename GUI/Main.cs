using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CKAN.Versioning;
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

    public partial class Main : Form
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

        public MainModList mainModList { get; }

        public NavigationHistory<GUIMod> navHistory;

        public string[] commandLineArgs;

        public GUIUser currentUser;

        private Timer filterTimer;

        private bool enableTrayIcon;
        private bool minimizeToTray;

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
                if (!ReferenceEquals(orig, value))
                    ChangeSetUpdated();
            }
        }

        private Dictionary<GUIMod, string> Conflicts
        {
            get { return conflicts; }
            set
            {
                var orig = conflicts;
                conflicts = value;
                if (orig != value)
                    ConflictsUpdated();
            }
        }

        private void ConflictsUpdated()
        {
            if (Conflicts == null) {
                // Clear status bar if no conflicts
                AddStatusMessage("");
            }
            foreach (DataGridViewRow row in ModList.Rows)
            {
                GUIMod module = (GUIMod)row.Tag;
                string value;

                if (Conflicts != null && Conflicts.TryGetValue(module, out value))
                {
                    string conflict_text = value;
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
                else if (row.DefaultCellStyle.BackColor != Color.Empty)
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

        public Main(string[] cmdlineArgs, KSPManager mgr, GUIUser user, bool showConsole)
        {
            log.Info("Starting the GUI");
            commandLineArgs = cmdlineArgs;

            // These are used by KSPManager's constructor to show messages about directory creation
            user.displayMessage = AddStatusMessage;
            user.displayError   = ErrorDialog;

            manager = mgr ?? new KSPManager(user);
            currentUser = user;

            controlFactory = new ControlFactory();
            Instance = this;
            mainModList = new MainModList(source => UpdateFilters(this), TooManyModsProvide, user);

            // History is read-only until the UI is started. We switch
            // out of it at the end of OnLoad() when we call NavInit().
            navHistory = new NavigationHistory<GUIMod> { IsReadOnly = true };

            InitializeComponent();

            // Replace mono's broken, ugly toolstrip renderer
            menuStrip1.Renderer = new FlatToolStripRenderer();
            menuStrip2.Renderer = new FlatToolStripRenderer();
            fileToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
            settingsToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
            helpToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
            FilterToolButton.DropDown.Renderer = new FlatToolStripRenderer();

            // We need to initialize the error dialog first to display errors.
            errorDialog = controlFactory.CreateControl<ErrorDialog>();

            // We want to check if our current instance is null first,
            // as it may have already been set by a command-line option.
            if (CurrentInstance == null && manager.GetPreferredInstance() == null)
            {
                Hide();

                var result = new ChooseKSPInstance(!actuallyVisible).ShowDialog();
                if (result == DialogResult.Cancel || result == DialogResult.Abort)
                {
                    Application.Exit();
                    return;
                }
            }

            configuration = Configuration.LoadOrCreateConfiguration
                (
                    Path.Combine(CurrentInstance.CkanDir(), "GUIConfig.xml"),
                    CKAN.Repository.default_ckan_repo_uri.ToString()
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
                Util.HideConsoleWindow();

            // Disable the modinfo controls until a mod has been choosen.
            ModInfoTabControl.SelectedModule = null;

            // WinForms on Mac OS X has a nasty bug where the UI thread hogs the CPU,
            // making our download speeds really slow unless you move the mouse while
            // downloading. Yielding periodically addresses that.
            // https://bugzilla.novell.com/show_bug.cgi?id=663433
            if (Platform.IsMac)
            {
                var timer = new Timer { Interval = 2 };
                timer.Tick += (sender, e) => { Thread.Yield(); };
                timer.Start();
            }

            Application.Run(this);

            var registry = RegistryManager.Instance(Manager.CurrentInstance);
            registry?.Dispose();
        }

        private void ModList_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            ModList_CellContentClick(sender, null);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.F:
                    ActiveControl = FilterByNameTextBox;
                    return true;

                case Keys.Control | Keys.S:
                    if (ChangeSet != null && ChangeSet.Any())
                        ApplyToolButton_Click(null, null);

                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public static Main Instance { get; private set; }

        /// <summary>
        /// Form.Visible says true even when the form hasn't shown yet.
        /// This value will tell the truth.
        /// </summary>
        private static bool actuallyVisible = false;

        protected override void OnShown(EventArgs e)
        {
            actuallyVisible = true;
            base.OnShown(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            actuallyVisible = false;
            base.OnFormClosed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Location = configuration.WindowLoc;
            Size = configuration.WindowSize;
            WindowState = configuration.IsWindowMaximised ? FormWindowState.Maximized : FormWindowState.Normal;
            try
            {
                splitContainer1.SplitterDistance = configuration.PanelPosition;
            }
            catch
            {
                // SplitContainer is mis-designed to throw exceptions
                // if the min/max limits are exceeded rather than simply obeying them.
            }
            ModInfoTabControl.ModMetaSplitPosition = configuration.ModInfoPosition;

            if (!configuration.CheckForUpdatesOnLaunchNoNag && AutoUpdate.CanUpdate)
            {
                log.Debug("Asking user if they wish for auto-updates");
                if (new AskUserForAutoUpdatesDialog().ShowDialog() == DialogResult.OK)
                    configuration.CheckForUpdatesOnLaunch = true;

                configuration.CheckForUpdatesOnLaunchNoNag = true;
                configuration.Save();
            }

            bool autoUpdating = false;

            if (configuration.CheckForUpdatesOnLaunch && AutoUpdate.CanUpdate)
            {
                try
                {
                    log.Info("Making auto-update call");
                    AutoUpdate.Instance.FetchLatestReleaseInfo();
                    var latest_version = AutoUpdate.Instance.latestUpdate.Version;
                    var current_version = new ModuleVersion(Meta.GetVersion());

                    if (AutoUpdate.Instance.IsFetched() && latest_version.IsGreaterThan(current_version))
                    {
                        log.Debug("Found higher ckan version");
                        var release_notes = AutoUpdate.Instance.latestUpdate.ReleaseNotes;
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
                    currentUser.RaiseError($"Error in auto-update:\n\t{exception.Message}");
                    log.Error("Error in auto-update", exception);
                }
            }

            CheckTrayState();
            InitRefreshTimer();

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

            // We would like to refresh if we're configured to refresh on startup,
            // or if we have no currently available modules.
            bool repoUpdateNeeded = configuration.RefreshOnStartup
                || !RegistryManager.Instance(CurrentInstance).registry.HasAnyAvailable();
            // If we're auto-updating the client then we shouldn't interfere with the progress tab
            if (!autoUpdating && repoUpdateNeeded)
            {
                UpdateRepo();
            }

            Text = $"CKAN {Meta.GetVersion()} - KSP {CurrentInstance.Version()}  --  {CurrentInstance.GameDir()}";

            if (commandLineArgs.Length >= 2)
            {
                var identifier = commandLineArgs[1];
                if (identifier.StartsWith("//"))
                    identifier = identifier.Substring(2);
                else if (identifier.StartsWith("ckan://"))
                    identifier = identifier.Substring(7);

                if (identifier.EndsWith("/"))
                    identifier = identifier.Substring(0, identifier.Length - 1);

                log.Debug("Attempting to select mod from startup parameters");
                FocusMod(identifier, true, true);
                ModList.Refresh();
                log.Debug("Failed to select mod from startup parameters");
            }

            var pluginsPath = Path.Combine(CurrentInstance.CkanDir(), "Plugins");
            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);

            pluginController = new PluginController(pluginsPath);

            CurrentInstance.RebuildKSPSubDir();

            // Initialize navigation. This should be called as late as
            // possible, once the UI is "settled" from its initial load.
            NavInit();

            log.Info("GUI started");
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Only close the window, when the user has access to the "Exit" of the menu.
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
            configuration.IsWindowMaximised = WindowState == FormWindowState.Maximized;

            // Copy panel position to app settings
            configuration.PanelPosition = splitContainer1.SplitterDistance;

            // Copy metadata panel split height to app settings
            configuration.ModInfoPosition = ModInfoTabControl.ModMetaSplitPosition;

            // Save the active filter
            configuration.ActiveFilter = (int)mainModList.ModFilter;

            // Save settings.
            configuration.Save();
            base.OnFormClosing(e);
        }

        private void CurrentInstanceUpdated()
        {
            Util.Invoke(this, () =>
            {
                Text = $"CKAN {Meta.GetVersion()} - KSP {CurrentInstance.Version()}    --    {CurrentInstance.GameDir()}";
            });

            configuration = Configuration.LoadOrCreateConfiguration(
                Path.Combine(CurrentInstance.CkanDir(), "GUIConfig.xml"),
                CKAN.Repository.default_ckan_repo_uri.ToString()
            );

            if (CurrentInstance.CompatibleVersionsAreFromDifferentKsp)
            {
                new CompatibleKspVersionsDialog(CurrentInstance, !actuallyVisible)
                    .ShowDialog();
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
            SetDescription($"Upgrading CKAN to {AutoUpdate.Instance.latestUpdate.Version}");

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
                var mod = (GUIMod)row.Tag;
                if (mod.HasUpdate)
                {
                    MarkModForUpdate(mod.Identifier);
                }
            }

            // only sort by Update column if checkbox in settings checked
            if (Main.Instance.configuration.AutoSortByUpdate)
            {
                // set new sort column
                var new_sort_column = ModList.Columns[1];
                var current_sort_column = ModList.Columns[configuration.SortByColumnIndex];

                // Reset the glyph.
                current_sort_column.HeaderCell.SortGlyphDirection = SortOrder.None;
                configuration.SortByColumnIndex = new_sort_column.Index;
                UpdateFilters(this);

                // Selects the top row and scrolls the list to it.
                DataGridViewCell cell = ModList.Rows[0].Cells[2];
                ModList.CurrentCell = cell;

            }

            ModList.Refresh();
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
                // Delay updating to improve typing performance on OS X.
                RunFilterUpdateTimer();
            }
            else
                mainModList.ModNameFilter = FilterByNameTextBox.Text;
        }

        private void FilterByAuthorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Platform.IsMac)
            {
                // Delay updating to improve typing performance on OS X.
                RunFilterUpdateTimer();
            }
            else
                mainModList.ModAuthorFilter = FilterByAuthorTextBox.Text;
        }

        private void FilterByDescriptionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Platform.IsMac)
            {
                // Delay updating to improve typing performance on OS X.
                RunFilterUpdateTimer();
            }
            else
                mainModList.ModDescriptionFilter = FilterByDescriptionTextBox.Text;
        }

        /// <summary>
        /// Start or restart a timer to update the filter after an interval since the last keypress.
        /// On Mac OS X, this prevents the search field from locking up due to DataGridViews being
        /// slow and key strokes being interpreted incorrectly when slowed down:
        /// http://mono.1490590.n4.nabble.com/Incorrect-missing-and-duplicate-keypress-events-td4658863.html
        /// </summary>
        private void RunFilterUpdateTimer()
        {
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
        /// Updates the filter after an interval of time has passed since the last keypress.
        /// </summary>
        private void OnFilterUpdateTimer(object source, EventArgs e)
        {
            mainModList.ModNameFilter = FilterByNameTextBox.Text;
            mainModList.ModAuthorFilter = FilterByAuthorTextBox.Text;
            mainModList.ModDescriptionFilter = FilterByDescriptionTextBox.Text;
            filterTimer.Stop();
        }

        private async Task UpdateChangeSetAndConflicts(IRegistryQuerier registry)
        {
            IEnumerable<ModChange> full_change_set = null;
            Dictionary<GUIMod, string> new_conflicts = null;

            bool too_many_provides_thrown = false;
            var user_change_set = mainModList.ComputeUserChangeSet();
            try
            {
                var module_installer = ModuleInstaller.GetInstance(CurrentInstance, Manager.Cache, GUI.user);
                full_change_set = await mainModList.ComputeChangeSetFromModList(registry, user_change_set, module_installer, CurrentInstance.VersionCriteria());
            }
            catch (InconsistentKraken k)
            {
                // Need to be recomputed due to ComputeChangeSetFromModList possibly changing it with too many provides handling.
                AddStatusMessage(k.ShortDescription);
                user_change_set = mainModList.ComputeUserChangeSet();
                new_conflicts = MainModList.ComputeConflictsFromModList(registry, user_change_set, CurrentInstance.VersionCriteria());
                full_change_set = null;
            }
            catch (TooManyModsProvideKraken)
            {
                // Can be thrown by ComputeChangeSetFromModList if the user cancels out of it.
                // We can just rerun it as the ModInfoTabControl has been removed.
                too_many_provides_thrown = true;
            }
            catch (DependencyNotSatisfiedKraken k)
            {
                GUI.user.RaiseError(
                    "{0} depends on {1}, which is not compatible with the currently installed version of KSP",
                    k.parent,
                    k.module
                );

                // Uncheck the box
                MarkModForInstall(k.parent.identifier, true);
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
                return null;

            DataGridViewRow selected_item = ModList.SelectedRows[0];

            var module = (GUIMod)selected_item?.Tag;
            return module;
        }

        private void launchKSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var split = configuration.CommandLineArguments.Split(' ');
            if (split.Length == 0)
                return;

            var binary = split[0];
            var args = string.Join(" ", split.Skip(1));

            try
            {
                Directory.SetCurrentDirectory(CurrentInstance.GameDir());
                Process.Start(binary, args);
            }
            catch (Exception exception)
            {
                GUI.user.RaiseError($"Couldn't start KSP.\r\n{exception.Message}.");
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
            OpenFileDialog open_file_dialog = new OpenFileDialog { Filter = Resources.CKANFileFilter };

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

                menuStrip1.Enabled = false;

                InstallModuleDriver(registry_manager.registry, module);
            }
        }

        /// <summary>
        /// Exports installed mods to a .ckan file.
        /// </summary>
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

                    // Save, just to be certain that the installed-*.ckan metapackage is generated.
                    RegistryManager mgr = RegistryManager.Instance(CurrentInstance);
                    mgr.Save(true);
                    mgr.ExportInstalled(dlg.FileName, recommends, versions);
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
            var result = new ChooseKSPInstance(!actuallyVisible).ShowDialog();
            if (result == DialogResult.OK && !Equals(old_instance, Instance.CurrentInstance))
                Instance.CurrentInstanceUpdated();
        }

        private void openKspDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Instance.manager.CurrentInstance.GameDir());
        }

        private void CompatibleKspVersionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompatibleKspVersionsDialog dialog = new CompatibleKspVersionsDialog(
                Instance.manager.CurrentInstance,
                !actuallyVisible
            );
            if (dialog.ShowDialog() != DialogResult.Cancel)
            {
                // This takes a while, so don't do it if they cancel out
                UpdateModsList();
            }
        }

        public void ResetFilterAndSelectModOnList(string key)
        {
            FilterByNameTextBox.Text = string.Empty;
            mainModList.ModNameFilter = string.Empty;
            FocusMod(key, true);
        }

        private void FocusMod(string key, bool exactMatch, bool showAsFirst = false)
        {
            DataGridViewRow current_row = ModList.CurrentRow;
            int currentIndex = current_row?.Index ?? 0;
            DataGridViewRow first_match = null;

            var does_name_begin_with_key = new Func<DataGridViewRow, bool>(row =>
            {
                GUIMod mod = row.Tag as GUIMod;
                bool row_match;
                if (exactMatch)
                    row_match = mod.Name == key || mod.Identifier == key;
                else
                    row_match = mod.Name.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                                mod.Abbrevation.StartsWith(key, StringComparison.OrdinalIgnoreCase) ||
                                mod.Identifier.StartsWith(key, StringComparison.OrdinalIgnoreCase);

                if (row_match && first_match == null)
                {
                    // Remember the first match to allow cycling back to it if necessary.
                    first_match = row;
                }

                if (key.Length == 1 && row_match && row.Index <= currentIndex)
                {
                    // Keep going forward if it's a single key match and not ahead of the current row.
                    return false;
                }

                return row_match;
            });

            ModList.ClearSelection();
            var rows = ModList.Rows.Cast<DataGridViewRow>().Where(row => row.Visible);
            DataGridViewRow match = rows.FirstOrDefault(does_name_begin_with_key);
            if (match == null && first_match != null)
            {
                // If there were no matches after the first match, cycle over to the beginning.
                match = first_match;
            }

            if (match != null)
            {
                match.Selected = true;

                // Setting this to the 'Name' cell prevents the checkbox from being toggled
                // by pressing 'Space' while the row is not indicated as active.
                ModList.CurrentCell = match.Cells[2];
                if (showAsFirst)
                    ModList.FirstDisplayedScrollingRowIndex = match.Index;
            }
            else
            {
                AddStatusMessage("Not found.");
            }
        }

        private void RecommendedModsToggleCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            var state = ((CheckBox)sender).Checked;
            foreach (ListViewItem item in RecommendedModsListView.Items)
            {
                if (item.Checked != state)
                    item.Checked = state;
            }

            RecommendedModsListView.Refresh();
        }

        private void reportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/KSP-CKAN/NetKAN/issues/new");
        }

        private void ModList_MouseDown(object sender, MouseEventArgs e)
        {
            var rowIndex = ModList.HitTest(e.X, e.Y).RowIndex;

            // Ignore header column to prevent errors.
            if (rowIndex != -1 && e.Button == MouseButtons.Right)
            {
                // Detect the clicked cell and select the row.
                ModList.ClearSelection();
                ModList.Rows[rowIndex].Selected = true;

                // Show the context menu.
                ModListContextMenuStrip.Show(ModList, new Point(e.X, e.Y));

                // Set the menu options.
                var guiMod = (GUIMod)ModList.Rows[rowIndex].Tag;

                downloadContentsToolStripMenuItem.Enabled = !guiMod.IsCached;
                reinstallToolStripMenuItem.Enabled = guiMod.IsInstalled;
            }
        }

        private void reinstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GUIMod module = ModInfoTabControl.SelectedModule;
            if (module == null || !module.IsCKAN)
                return;

            YesNoDialog reinstallDialog = new YesNoDialog();
            string confirmationText = $"Do you want to reinstall {module.Name}?";
            if (reinstallDialog.ShowYesNoDialog(confirmationText) == DialogResult.No)
                return;

            IRegistryQuerier registry = RegistryManager.Instance(CurrentInstance).registry;

            // Build the list of changes, first the mod to remove:
            List<ModChange> toReinstall = new List<ModChange>()
            {
                new ModChange(module, GUIModChangeType.Remove, null)
            };
            // Then everything we need to re-install:
            HashSet<string> goners = registry.FindReverseDependencies(
                new List<string>() { module.Identifier }
            );
            foreach (string id in goners)
            {
                toReinstall.Add(new ModChange(
                    mainModList.full_list_of_mod_rows[id]?.Tag as GUIMod,
                    GUIModChangeType.Install,
                    null
                ));
            }
            // Hand off to centralized [un]installer code
            installWorker.RunWorkerAsync(
                new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                    toReinstall,
                    RelationshipResolver.DefaultOpts()
                )
            );
        }

        private void downloadContentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var module = ModInfoTabControl.SelectedModule;
            if (module == null || !module.IsCKAN)
                return;

            Instance.ResetProgress();
            Instance.ShowWaitDialog(false);
            ModInfoTabControl.CacheWorker.RunWorkerAsync(module.ToCkanModule());
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            UpdateTrayState();
        }

        private void minimizeNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenWindow();
        }

        private void updatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow();
            MarkAllUpdatesToolButton_Click(sender, e);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateRepo();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configuration.RefreshPaused = !configuration.RefreshPaused;
            if (configuration.RefreshPaused)
            {
                refreshTimer.Stop();
                pauseToolStripMenuItem.Text = "Resume";
            }
            else
            {
                refreshTimer.Start();
                pauseToolStripMenuItem.Text = "Pause";
            }
        }

        private void openCKANToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWindow();
        }

        private void cKANSettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenWindow();
            new SettingsDialog().ShowDialog();
        }

        #region Tray Behaviour

        public void CheckTrayState()
        {
            enableTrayIcon = configuration.EnableTrayIcon;
            minimizeToTray = configuration.MinimizeToTray;
            UpdateTrayState();
        }

        private void UpdateTrayState()
        {
            if (enableTrayIcon)
            {
                minimizeNotifyIcon.Visible = true;

                if (minimizeToTray && WindowState == FormWindowState.Minimized)
                {
                    Hide();
                }
                else
                {
                    // Save the window state
                    configuration.IsWindowMaximised = WindowState == FormWindowState.Maximized;
                    configuration.Save();
                }
            }
            else
            {
                minimizeNotifyIcon.Visible = false;
            }
        }

        public void UpdateTrayInfo()
        {
            var count = mainModList.CountModsByFilter(GUIModFilter.InstalledUpdateAvailable);

            if (count == 0)
            {
                updatesToolStripMenuItem.Enabled = false;
                updatesToolStripMenuItem.Text = "No available updates";
            }
            else
            {
                updatesToolStripMenuItem.Enabled = true;
                updatesToolStripMenuItem.Text = $"{count} available update" + (count == 1 ? "" : "s");
            }
        }

        /// <summary>
        /// Open the GUI and set it to the correct state.
        /// </summary>
        public void OpenWindow()
        {
            Show();
            WindowState = configuration.IsWindowMaximised ? FormWindowState.Maximized : FormWindowState.Normal;
        }

        #endregion

        #region Navigation History

        private void NavInit()
        {
            navHistory.OnHistoryChange += NavOnHistoryChange;
            navHistory.IsReadOnly = false;
            var currentMod = GetSelectedModule();
            if (currentMod != null)
                navHistory.AddToHistory(currentMod);
        }

        private void NavUpdateUI()
        {
            NavBackwardToolButton.Enabled = navHistory.CanNavigateBackward;
            NavForwardToolButton.Enabled = navHistory.CanNavigateForward;
        }

        private void NavSelectMod(GUIMod module)
        {
            navHistory.AddToHistory(module);
        }

        private void NavGoBackward()
        {
            if (navHistory.CanNavigateBackward)
                NavGoToMod(navHistory.NavigateBackward());
        }

        private void NavGoForward()
        {
            if (navHistory.CanNavigateForward)
                NavGoToMod(navHistory.NavigateForward());
        }

        private void NavGoToMod(GUIMod module)
        {
            // Focussing on a mod also causes navigation, but we don't want
            // this to affect the history. so we switch to read-only mode.
            navHistory.IsReadOnly = true;
            FocusMod(module.Name, true);
            navHistory.IsReadOnly = false;
        }

        private void NavOnHistoryChange()
        {
            NavUpdateUI();
        }

        private void NavBackwardToolButton_Click(object sender, EventArgs e)
        {
            NavGoBackward();
        }

        private void NavForwardToolButton_Click(object sender, EventArgs e)
        {
            NavGoForward();
        }

        #endregion
    }
}
