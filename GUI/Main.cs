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
using System.Globalization;
using CKAN.Versioning;
using CKAN.Exporters;
using CKAN.Properties;
using CKAN.Types;
using log4net;
using Timer = System.Windows.Forms.Timer;
using CKAN.Win32Registry;
using Autofac;

namespace CKAN
{
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

        private bool needRegistrySave = false;

        public MainModList mainModList { get; }

        public NavigationHistory<GUIMod> navHistory;

        public string[] commandLineArgs;

        public GUIUser currentUser;

        private Timer filterTimer;

        private bool enableTrayIcon;
        private bool minimizeToTray;
        private IWin32Registry winReg = ServiceLocator.Container.Resolve<IWin32Registry>();

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
                    ConflictsUpdated(orig);
            }
        }

        private void ConflictsUpdated(Dictionary<GUIMod, string> prevConflicts)
        {
            if (Conflicts == null)
            {
                // Clear status bar if no conflicts
                AddStatusMessage("");
            }

            if (prevConflicts != null)
            {
                // Mark old conflicts as non-conflicted
                // (rows that are _still_ conflicted will be marked as such in the next loop)
                foreach (GUIMod guiMod in prevConflicts.Keys)
                {
                    DataGridViewRow row = mainModList.full_list_of_mod_rows[guiMod.Identifier];

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.ToolTipText = null;
                    }
                    row.DefaultCellStyle.BackColor = Color.Empty;
                    ModList.InvalidateRow(row.Index);
                }
            }
            if (Conflicts != null)
            {
                // Mark current conflicts as conflicted
                foreach (var kvp in Conflicts)
                {
                    GUIMod          guiMod = kvp.Key;
                    DataGridViewRow row    = mainModList.full_list_of_mod_rows[guiMod.Identifier];
                    string conflict_text = kvp.Value;

                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.ToolTipText = conflict_text;
                    }
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
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
                auditRecommendationsMenuItem.Enabled = false;
            }
            else
            {
                tabController.HideTab("ChangesetTabPage");
                ApplyToolButton.Enabled = false;
                auditRecommendationsMenuItem.Enabled = true;
            }
        }

        public Main(string[] cmdlineArgs, KSPManager mgr, GUIUser user, bool showConsole)
        {
            log.Info("Starting the GUI");
            commandLineArgs = cmdlineArgs;

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
            if (Platform.IsMono)
            {
                menuStrip1.Renderer = new FlatToolStripRenderer();
                menuStrip2.Renderer = new FlatToolStripRenderer();
                fileToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                settingsToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                helpToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                FilterToolButton.DropDown.Renderer = new FlatToolStripRenderer();
                minimizedContextMenuStrip.Renderer = new FlatToolStripRenderer();
                ModListContextMenuStrip.Renderer = new FlatToolStripRenderer();
                ModListHeaderContextMenuStrip.Renderer = new FlatToolStripRenderer();
            }

            // Initialize all user interaction dialogs.
            RecreateDialogs();

            // We want to check if our current instance is null first,
            // as it may have already been set by a command-line option.
            if (CurrentInstance == null && manager.GetPreferredInstance() == null)
            {
                Hide();

                var result = new ManageKspInstances(!actuallyVisible).ShowDialog();
                if (result == DialogResult.Cancel || result == DialogResult.Abort)
                {
                    Application.Exit();
                    return;
                }
            }

            configuration = Configuration.LoadOrCreateConfiguration
                (
                    Path.Combine(CurrentInstance.CkanDir(), "GUIConfig.xml")
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

            if (!showConsole)
                Util.HideConsoleWindow();

            // Disable the modinfo controls until a mod has been choosen. This has an effect if the modlist is empty.
            ActiveModInfo = null;

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

            // Set the window name and class for X11
            if (Platform.IsX11)
            {
                HandleCreated += (sender, e) => X11.SetWMClass("CKAN", "CKAN", Handle);
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

            base.OnShown(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            actuallyVisible = false;
            base.OnFormClosed(e);
        }

        private void SetStartPosition()
        {
            Screen screen = Util.FindScreen(configuration.WindowLoc, configuration.WindowSize);
            if (screen == null)
            {
                // Start at center of screen if we have an invalid location saved in the config
                // (such as -32000,-32000, which Windows uses when you're minimized)
                StartPosition = FormStartPosition.CenterScreen;
            }
            else if (configuration.WindowLoc.X == -1 && configuration.WindowLoc.Y == -1)
            {
                // Center on screen for first launch
                StartPosition = FormStartPosition.CenterScreen;
            }
            else if (Platform.IsMac)
            {
                // Make sure there's room at the top for the MacOSX menu bar
                Location = Util.ClampedLocationWithMargins(
                    configuration.WindowLoc, configuration.WindowSize,
                    new Size(0, 30), new Size(0, 0),
                    screen
                );
            }
            else
            {
                // Just make sure it's fully on screen
                Location = Util.ClampedLocation(configuration.WindowLoc, configuration.WindowSize, screen);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            SetStartPosition();
            Size = configuration.WindowSize;
            WindowState = configuration.IsWindowMaximised ? FormWindowState.Maximized : FormWindowState.Normal;

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
                    currentUser.RaiseError(Properties.Resources.MainAutoUpdateFailed, exception.Message);
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

            URLHandlers.RegisterURLHandler(configuration, currentUser);

            ApplyToolButton.Enabled = false;

            CurrentInstanceUpdated(!autoUpdating);

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

            if (Conflicts != null)
            {
                if (Conflicts.Any())
                {
                    // Ask if they want to resolve conflicts
                    string confDescrip = Conflicts
                        .Select(kvp => kvp.Value)
                        .Aggregate((a, b) => $"{a}, {b}");
                    if (!YesNoDialog(string.Format(Properties.Resources.MainQuitWithConflicts, confDescrip),
                        Properties.Resources.MainQuit,
                        Properties.Resources.MainGoBack))
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else
                {
                    // The Conflicts dictionary is empty even when there are unmet dependencies.
                    if (!YesNoDialog(Properties.Resources.MainQuitWithUnmetDeps,
                        Properties.Resources.MainQuit,
                        Properties.Resources.MainGoBack))
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            else if (ChangeSet?.Any() ?? false)
            {
                // Ask if they want to discard the change set
                string changeDescrip = ChangeSet
                    .GroupBy(ch => ch.ChangeType, ch => ch.Mod.Name)
                    .Select(grp => $"{grp.Key}: "
                        + grp.Aggregate((a, b) => $"{a}, {b}"))
                    .Aggregate((a, b) => $"{a}\r\n{b}");
                if (!YesNoDialog(string.Format(Properties.Resources.MainQuitWIthUnappliedChanges, changeDescrip),
                    Properties.Resources.MainQuit,
                    Properties.Resources.MainGoBack))
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Copy window location to app settings
            configuration.WindowLoc = WindowState == FormWindowState.Normal ? Location : RestoreBounds.Location;

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

            if (needRegistrySave)
            {
                // Save registry
                RegistryManager.Instance(CurrentInstance).Save(false);
            }

            base.OnFormClosing(e);
        }

        /// <summary>
        /// React to switching to a new game instance
        /// </summary>
        /// <param name="onStartup">true if this is the initial load and should trigger auto repo updates, false otherwise</param>
        private void CurrentInstanceUpdated(bool onStartup)
        {
            Util.Invoke(this, () =>
            {
                Text = $"CKAN {Meta.GetVersion()} - KSP {CurrentInstance.Version()}    --    {CurrentInstance.GameDir()}";
                StatusInstanceLabel.Text = string.Format(
                    Properties.Resources.StatusInstanceLabelText,
                    CurrentInstance.Name,
                    CurrentInstance.Version()?.ToString()
                );
            });

            configuration = Configuration.LoadOrCreateConfiguration(
                Path.Combine(CurrentInstance.CkanDir (), "GUIConfig.xml")
            );

            if (CurrentInstance.CompatibleVersionsAreFromDifferentKsp)
            {
                new CompatibleKspVersionsDialog(CurrentInstance, !actuallyVisible)
                    .ShowDialog();
            }

            bool repoUpdateNeeded = configuration.RefreshOnStartup
                || !RegistryManager.Instance(CurrentInstance).registry.HasAnyAvailable();
            if (onStartup && repoUpdateNeeded)
            {
                UpdateRepo();
            }
            else
            {
                UpdateModsList();
            }
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
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainUpgradingWaitTitle);
            SetDescription(string.Format(Properties.Resources.MainUpgradingTo, AutoUpdate.Instance.latestUpdate.Version));

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
                var new_sort_column = ModList.Columns[UpdateCol.Index];
                var current_sort_column = ModList.Columns[configuration.SortByColumnIndex];

                // Reset the glyph.
                current_sort_column.HeaderCell.SortGlyphDirection = SortOrder.None;
                configuration.SortByColumnIndex = new_sort_column.Index;
                UpdateFilters(this);

                // Select the top row and scroll the list to it.
                ModList.CurrentCell = ModList.Rows[0].Cells[SelectableColumnIndex()];
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
            var user_change_set = mainModList.ComputeUserChangeSet(registry);
            try
            {
                var module_installer = ModuleInstaller.GetInstance(CurrentInstance, Manager.Cache, GUI.user);
                full_change_set = await mainModList.ComputeChangeSetFromModList(registry, user_change_set, module_installer, CurrentInstance.VersionCriteria());
            }
            catch (InconsistentKraken k)
            {
                // Need to be recomputed due to ComputeChangeSetFromModList possibly changing it with too many provides handling.
                AddStatusMessage(k.ShortDescription);
                user_change_set = mainModList.ComputeUserChangeSet(registry);
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
                    Properties.Resources.MainDepNotSatisfied,
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

        private void FilterReplaceableButton_Click(object sender, EventArgs e)
        {
            Filter(GUIModFilter.Replaceable);
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

        /// <summary>
        /// Called when the modlist filter (all, compatible, incompatible...) is changed.
        /// </summary>
        /// <param name="filter">Filter.</param>
        private void Filter(GUIModFilter filter)
        {
            // Triggers mainModList.ModFiltersUpdated()
            mainModList.ModFilter = filter;

            // Ask the configuration which columns to show.
            foreach (DataGridViewColumn col in ModList.Columns)
            {
                // Some columns are always shown, and others are handled by UpdateModsList()
                if (col.Name != "Installed" && col.Name != "UpdateCol" && col.Name != "ReplaceCol")
                {
                    col.Visible = !configuration.HiddenColumnNames.Contains(col.Name);
                }
            }

            switch (filter)
            {
                // Some columns really do / don't make sense to be visible on certain filter settings.
                // Hide / Show them, without writing to config, so once the user changes tab again,
                // they are shown / hidden again, as before.
                case GUIModFilter.All:                      FilterToolButton.Text = Properties.Resources.MainFilterAll;          break;
                case GUIModFilter.Incompatible:             FilterToolButton.Text = Properties.Resources.MainFilterIncompatible; break;
                case GUIModFilter.Installed:                FilterToolButton.Text = Properties.Resources.MainFilterInstalled;    break;
                case GUIModFilter.InstalledUpdateAvailable: FilterToolButton.Text = Properties.Resources.MainFilterUpgradeable;  break;
                case GUIModFilter.Replaceable:              FilterToolButton.Text = Properties.Resources.MainFilterReplaceable;  break;
                case GUIModFilter.Cached:                   FilterToolButton.Text = Properties.Resources.MainFilterCached;       break;
                case GUIModFilter.NewInRepository:          FilterToolButton.Text = Properties.Resources.MainFilterNew;          break;
                case GUIModFilter.NotInstalled:             ModList.Columns["InstalledVersion"].Visible = false;
                                                            ModList.Columns["InstallDate"].Visible      = false;
                                                            ModList.Columns["AutoInstalled"].Visible    = false;
                                                            FilterToolButton.Text = Properties.Resources.MainFilterNotInstalled; break;
                default:                                    FilterToolButton.Text = Properties.Resources.MainFilterCompatible;   break;
            }
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

            var registry = RegistryManager.Instance(CurrentInstance).registry;
            var incomp   = registry.IncompatibleInstalled(CurrentInstance.VersionCriteria());
            if (incomp.Any())
            {
                // Warn that it might not be safe to run KSP with incompatible modules installed
                string incompatDescrip = incomp
                    .Select(m => $"{m.Module} ({registry.CompatibleGameVersions(m.Module)})")
                    .Aggregate((a, b) => $"{a}, {b}");
                if (!YesNoDialog(string.Format(Properties.Resources.MainLaunchWithIncompatible, incompatDescrip),
                    Properties.Resources.MainLaunch,
                    Properties.Resources.MainGoBack))
                {
                    return;
                }
            }

            var binary = split[0];
            var args = string.Join(" ", split.Skip(1));

            try
            {
                Directory.SetCurrentDirectory(CurrentInstance.GameDir());
                Process.Start(binary, args);
            }
            catch (Exception exception)
            {
                GUI.user.RaiseError(Properties.Resources.MainLaunchFailed, exception.Message);
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

                // Don't add metapacakges to the registry
                if (!module.IsMetapackage)
                {
                    // Remove this version of the module in the registry, if it exists.
                    registry_manager.registry.RemoveAvailable(module);

                    // Sneakily add our version in...
                    registry_manager.registry.AddAvailable(module);
                }

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
                new ExportOption(ExportFileType.CkanFavourite, Properties.Resources.MainFavouritesList, "ckan"),
                new ExportOption(ExportFileType.Ckan,          Properties.Resources.MainModPack,        "ckan"),
                new ExportOption(ExportFileType.PlainText,     Properties.Resources.MainPlainText,      "txt"),
                new ExportOption(ExportFileType.Markdown,      Properties.Resources.MainMarkdown,       "md"),
                new ExportOption(ExportFileType.BbCode,        Properties.Resources.MainBBCode,         "txt"),
                new ExportOption(ExportFileType.Csv,           Properties.Resources.MainCSV,            "csv"),
                new ExportOption(ExportFileType.Tsv,           Properties.Resources.MainTSV,            "tsv")
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

        private void manageKspInstancesMenuItem_Click(object sender, EventArgs e)
        {
            Instance.Manager.ClearAutoStart();
            var old_instance = Instance.CurrentInstance;
            var result = new ManageKspInstances(!actuallyVisible).ShowDialog();
            if (result == DialogResult.OK && !Equals(old_instance, Instance.CurrentInstance))
                Instance.CurrentInstanceUpdated(false);
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

                ModList.CurrentCell = match.Cells[SelectableColumnIndex()];
                if (showAsFirst)
                    ModList.FirstDisplayedScrollingRowIndex = match.Index;
            }
            else
            {
                AddStatusMessage(Properties.Resources.MainNotFound);
            }
        }

        private GUIMod ActiveModInfo
        {
            set {
                if (value == null)
                {
                    splitContainer1.Panel2Collapsed = true;
                }
                else
                {
                    if (splitContainer1.Panel2Collapsed)
                    {
                        splitContainer1.Panel2Collapsed = false;
                    }
                    ModInfoTabControl.SelectedModule = value;
                }
            }
        }

        private void ShowSelectionModInfo(ListView.SelectedListViewItemCollection selection)
        {
            CkanModule module = (CkanModule)selection?.Cast<ListViewItem>().FirstOrDefault()?.Tag;

            ActiveModInfo = module == null ? null : new GUIMod(
                module,
                RegistryManager.Instance(CurrentInstance).registry,
                CurrentInstance.VersionCriteria()
            );
        }

        private void ChangesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectionModInfo(ChangesListView.SelectedItems);
        }

        private void ChooseProvidedModsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectionModInfo(ChooseProvidedModsListView.SelectedItems);
        }

        private void MainTabControl_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            switch (MainTabControl.SelectedTab?.Name)
            {
                case "ManageModsTabPage":
                    ModList_SelectedIndexChanged(sender, e);
                    break;

                case "WaitTabPage":
                    ShowSelectionModInfo(null);
                    break;

                case "ChangesetTabPage":
                    ShowSelectionModInfo(ChangesListView.SelectedItems);
                    break;

                case "ChooseRecommendedModsTabPage":
                    ShowSelectionModInfo(RecommendedModsListView.SelectedItems);
                    break;

                case "ChooseProvidedModsTabPage":
                    ShowSelectionModInfo(ChooseProvidedModsListView.SelectedItems);
                    break;
            }
        }

        private void reportClientIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/KSP-CKAN/CKAN/issues/new/choose");
        }

        private void reportMetadataIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/KSP-CKAN/NetKAN/issues/new/choose");
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
            string confirmationText = string.Format(Properties.Resources.MainReinstallConfirm, module.Name);
            if (reinstallDialog.ShowYesNoDialog(confirmationText) == DialogResult.No)
                return;

            IRegistryQuerier registry = RegistryManager.Instance(CurrentInstance).registry;

            // Build the list of changes, first the mod to remove:
            List<ModChange> toReinstall = new List<ModChange>()
            {
                new ModChange(module, GUIModChangeType.Remove, null)
            };
            // Then everything we need to re-install:
            var revdep = registry.FindReverseDependencies(new List<string>() { module.Identifier });
            var goners = revdep.Union(
                registry.FindRemovableAutoInstalled(
                    registry.InstalledModules.Where(im => !revdep.Contains(im.identifier))
                ).Select(im => im.Module.identifier)
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
                    RelationshipResolver.DependsOnlyOpts()
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
