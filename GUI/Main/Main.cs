using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using log4net;
using Autofac;

using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN.GUI
{
    public partial class Main : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Main));

        public delegate void ModChangedCallback(CkanModule module, GUIModChangeType change);

        public static event ModChangedCallback modChangedCallback;

        public GUIConfiguration configuration;

        public ControlFactory controlFactory;

        public TabController tabController;

        public PluginController pluginController;

        public volatile GameInstanceManager manager;

        public GameInstance CurrentInstance
        {
            get { return manager.CurrentInstance; }
        }

        public GameInstanceManager Manager
        {
            get { return manager; }
            set { manager = value; }
        }

        private bool needRegistrySave = false;

        public string[] commandLineArgs;

        public GUIUser currentUser;

        private bool enableTrayIcon;
        private bool minimizeToTray;

        public static Main Instance { get; private set; }

        public Main(string[] cmdlineArgs, GameInstanceManager mgr, bool showConsole)
        {
            log.Info("Starting the GUI");
            commandLineArgs = cmdlineArgs;

            Configuration.IConfiguration mainConfig = ServiceLocator.Container.Resolve<Configuration.IConfiguration>();

            // If the language is not set yet in the config, try to save the current language.
            // If it isn't supported, it'll still be null afterwards. Doesn't matter, .NET handles the resource selection.
            // Once the user chooses a language in the settings, the string will be no longer null, and we can change
            // CKAN's language here before any GUI components are initialized.
            if (string.IsNullOrEmpty(mainConfig.Language))
            {
                string runtimeLanguage = Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;
                mainConfig.Language = runtimeLanguage;
            }
            else
            {
                CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(mainConfig.Language);
            }

            InitializeComponent();

            Instance = this;

            currentUser = new GUIUser(this, this.Wait);
            if (mgr != null)
            {
                // With a working GUI, assign a GUIUser to the GameInstanceManager to replace the ConsoleUser
                mgr.User = currentUser;
                manager = mgr;
            }
            else
            {
                manager = new GameInstanceManager(currentUser);
            }

            controlFactory = new ControlFactory();

            // React when the user clicks a tag or filter link in mod info
            ModInfo.OnChangeFilter += ManageMods.Filter;

            // Replace mono's broken, ugly toolstrip renderer
            if (Platform.IsMono)
            {
                menuStrip1.Renderer = new FlatToolStripRenderer();
                fileToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                settingsToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                helpToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                minimizedContextMenuStrip.Renderer = new FlatToolStripRenderer();
            }

            // Initialize all user interaction dialogs.
            RecreateDialogs();

            // Make sure we have an instance
            if (CurrentInstance == null)
            {
                // Maybe we can find an instance automatically (e.g., portable, only, default)
                manager.GetPreferredInstance();
            }
            // A loop that ends when we have a valid instance or the user gives up
            do
            {
                if (CurrentInstance == null && !InstancePromptAtStart())
                {
                    // User cancelled, give up
                    return;
                }
                // We now have a tentative instance. Check if it's locked.
                try
                {
                    // This will throw RegistryInUseKraken if locked by another process
                    var regMgr = RegistryManager.Instance(CurrentInstance);
                    // Tell the user their registry was reset if it was corrupted
                    if (!string.IsNullOrEmpty(regMgr.previousCorruptedMessage)
                        && !string.IsNullOrEmpty(regMgr.previousCorruptedPath))
                    {
                        errorDialog.ShowErrorDialog(Properties.Resources.MainCorruptedRegistry,
                            regMgr.previousCorruptedPath, regMgr.previousCorruptedMessage,
                            Path.Combine(Path.GetDirectoryName(regMgr.previousCorruptedPath) ?? "", regMgr.LatestInstalledExportFilename()));
                        regMgr.previousCorruptedMessage = null;
                        regMgr.previousCorruptedPath    = null;
                        // But the instance is actually fine because a new registry was just created
                    }
                }
                catch (RegistryInUseKraken kraken)
                {
                    errorDialog.ShowErrorDialog(kraken.ToString());
                    // Couldn't get the lock, there is no current instance
                    manager.CurrentInstance = null;
                    if (manager.Instances.All(inst => !inst.Value.Valid || inst.Value.IsMaybeLocked))
                    {
                        // Everything's invalid or locked, give up
                        Application.Exit();
                        return;
                    }
                }
            } while (CurrentInstance == null);
            // We can only reach this point if CurrentInstance is not null
            // AND we acquired the lock for it successfully

            // Get the instance's GUI onfig
            configuration = GUIConfiguration.LoadOrCreateConfiguration(
                Path.Combine(CurrentInstance.CkanDir(), "GUIConfig.xml"));

            tabController = new TabController(MainTabControl);
            tabController.ShowTab("ManageModsTabPage");

            if (!showConsole)
            {
                Util.HideConsoleWindow();
            }

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

            if (CurrentInstance != null)
            {
                var registry = RegistryManager.Instance(Manager.CurrentInstance);
                registry?.Dispose();
            }
        }

        private bool InstancePromptAtStart()
        {
            Hide();
            var result = new ManageGameInstancesDialog(!actuallyVisible, currentUser).ShowDialog();
            if (result != DialogResult.OK)
            {
                Application.Exit();
                return false;
            }
            return true;
        }

        private void manageGameInstancesMenuItem_Click(object sender, EventArgs e)
        {
            var old_instance = CurrentInstance;
            var result = new ManageGameInstancesDialog(!actuallyVisible, currentUser).ShowDialog();
            if (result == DialogResult.OK && !Equals(old_instance, CurrentInstance))
            {
                try
                {
                    ManageMods.ModGrid.ClearSelection();
                    CurrentInstanceUpdated(true);
                }
                catch (RegistryInUseKraken kraken)
                {
                    // Couldn't get the lock, revert to previous instance
                    errorDialog.ShowErrorDialog(kraken.ToString());
                    manager.CurrentInstance = old_instance;
                    CurrentInstanceUpdated(false);
                }
            }
        }

        private void UpdateStatusBar()
        {
            StatusInstanceLabel.Text = string.Format(
                CurrentInstance.playTime.Time > TimeSpan.Zero
                    ? Properties.Resources.StatusInstanceLabelTextWithPlayTime
                    : Properties.Resources.StatusInstanceLabelText,
                CurrentInstance.Name,
                CurrentInstance.game.ShortName,
                CurrentInstance.Version()?.ToString(),
                CurrentInstance.playTime.ToString());
        }

        /// <summary>
        /// React to switching to a new game instance
        /// </summary>
        /// <param name="allowRepoUpdate">true if a repo update is allowed if needed (e.g. on initial load), false otherwise</param>
        private void CurrentInstanceUpdated(bool allowRepoUpdate)
        {
            log.Debug("Current instance updated, scanning");
            CurrentInstance.Scan();
            Util.Invoke(this, () =>
            {
                Text = $"CKAN {Meta.GetVersion()} - {CurrentInstance.game.ShortName} {CurrentInstance.Version()}    --    {CurrentInstance.GameDir().Replace('/', Path.DirectorySeparatorChar)}";
                UpdateStatusBar();
            });

            if (CurrentInstance.CompatibleVersionsAreFromDifferentGameVersion)
            {
                new CompatibleGameVersionsDialog(CurrentInstance, !actuallyVisible)
                    .ShowDialog();
            }

            // This will throw RegistryInUseKraken if locked by another process
            var regMgr   = RegistryManager.Instance(CurrentInstance);
            var registry = regMgr.registry;
            if (!string.IsNullOrEmpty(regMgr.previousCorruptedMessage)
                                                && !string.IsNullOrEmpty(regMgr.previousCorruptedPath))
            {
                errorDialog.ShowErrorDialog(Properties.Resources.MainCorruptedRegistry,
                    regMgr.previousCorruptedPath, regMgr.previousCorruptedMessage,
                    Path.Combine(Path.GetDirectoryName(regMgr.previousCorruptedPath) ?? "", regMgr.LatestInstalledExportFilename()));
                regMgr.previousCorruptedMessage = null;
                regMgr.previousCorruptedPath = null;
            }
            registry.BuildTagIndex(ManageMods.mainModList.ModuleTags);

            configuration = GUIConfiguration.LoadOrCreateConfiguration(
                Path.Combine(CurrentInstance.CkanDir(), "GUIConfig.xml"));

            bool repoUpdateNeeded = configuration.RefreshOnStartup
                || !RegistryManager.Instance(CurrentInstance).registry.HasAnyAvailable();
            if (allowRepoUpdate)
            {
                // If not allowing, don't do anything
                if (repoUpdateNeeded)
                {
                    // Update the filters after UpdateRepo() completed.
                    // Since this happens with a backgroundworker, Filter() is added as callback for RunWorkerCompleted.
                    // Remove it again after it ran, else it stays there and is added again and again.
                    void filterUpdate(object sender, RunWorkerCompletedEventArgs e)
                    {
                        SetupDefaultSearch();
                        m_UpdateRepoWorker.RunWorkerCompleted -= filterUpdate;
                    }

                    m_UpdateRepoWorker.RunWorkerCompleted += filterUpdate;

                    ManageMods.ModGrid.Rows.Clear();
                    UpdateRepo();
                }
                else
                {
                    SetupDefaultSearch();
                    ManageMods.UpdateModsList();
                }
            }
            ManageMods.InstanceUpdated(CurrentInstance);
        }

        /// <summary>
        /// Form.Visible says true even when the form hasn't shown yet.
        /// This value will tell the truth.
        /// </summary>
        public bool actuallyVisible { get; private set; } = false;

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
            ModInfo.ModMetaSplitPosition = configuration.ModInfoPosition;

            base.OnShown(e);
        }

        /// <summary>
        /// Open the user guide when the user presses F1
        /// </summary>
        protected override void OnHelpRequested(HelpEventArgs evt)
        {
            evt.Handled = Util.TryOpenWebPage(HelpURLs.UserGuide);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Stop all running play time timers
            foreach (var inst in manager.Instances.Values)
            {
                if (inst.Valid)
                {
                    inst.playTime.Stop(inst.CkanDir());
                }
            }
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

            bool autoUpdating = CheckForCKANUpdate();
            CheckTrayState();
            InitRefreshTimer();

            m_UpdateRepoWorker = new BackgroundWorker { WorkerReportsProgress = false, WorkerSupportsCancellation = true };

            m_UpdateRepoWorker.RunWorkerCompleted += PostUpdateRepo;
            m_UpdateRepoWorker.DoWork += UpdateRepo;

            installWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            installWorker.RunWorkerCompleted += PostInstallMods;
            installWorker.DoWork += InstallMods;

            URLHandlers.RegisterURLHandler(configuration, currentUser);

            if (CurrentInstance != null)
            {
                CurrentInstanceUpdated(!autoUpdating);
            }

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
                ManageMods.FocusMod(identifier, true, true);
                ManageMods.ModGrid.Refresh();
                log.Debug("Failed to select mod from startup parameters");
            }

            if (CurrentInstance != null)
            {
                var pluginsPath = Path.Combine(CurrentInstance.CkanDir(), "Plugins");
                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);

                pluginController = new PluginController(pluginsPath);

                CurrentInstance.game.RebuildSubdirectories(CurrentInstance);
            }

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

            if (!ManageMods.AllowClose())
            {
                e.Cancel = true;
                return;
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
            configuration.ModInfoPosition = ModInfo.ModMetaSplitPosition;

            // Save settings.
            configuration.Save();

            if (needRegistrySave)
            {
                using (var transaction = CkanTransaction.CreateTransactionScope())
                {
                    // Save registry
                    RegistryManager.Instance(CurrentInstance).Save(false);
                    transaction.Complete();
                }
            }

            base.OnFormClosing(e);
        }

        private void SetupDefaultSearch()
        {
            var def = configuration.DefaultSearches;
            if (def == null || def.Count < 1)
            {
                // Fall back to old setting
                ManageMods.Filter(ModList.FilterToSavedSearch(
                    (GUIModFilter)configuration.ActiveFilter,
                    ManageMods.mainModList.ModuleTags.Tags.GetOrDefault(configuration.TagFilter),
                    ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                        .FirstOrDefault(l => l.Name == configuration.CustomLabelFilter)
                ));
                // Clear the old filter so it doesn't get pulled forward again
                configuration.ActiveFilter = (int)GUIModFilter.All;
            }
            else
            {
                var labels = ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name).ToList();
                var searches = def.Select(s => ModSearch.Parse(s, labels)).ToList();
                ManageMods.SetSearches(searches);
            }
        }

        public void UpdateModContentsTree(CkanModule module, bool force = false)
        {
            ModInfo.UpdateModContentsTree(module, force);
        }

        private void ExitToolButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void GameCommandlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new GameCommandLineOptionsDialog();
            if (dialog.ShowGameCommandLineOptionsDialog(configuration.CommandLineArguments) == DialogResult.OK)
            {
                configuration.CommandLineArguments = dialog.GetResult();
                configuration.Save();
            }
        }

        private void CKANSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Flipping enabled here hides the main form itself.
            Enabled = false;
            new SettingsDialog(currentUser).ShowDialog();
            Enabled = true;
        }

        private void pluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Enabled = false;
            pluginsDialog.ShowDialog();
            Enabled = true;
        }

        private void installFiltersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Enabled = false;
            var dlg = new InstallFiltersDialog(ServiceLocator.Container.Resolve<Configuration.IConfiguration>(), CurrentInstance);
            dlg.ShowDialog(this);
            Enabled = true;
        }

        private void installFromckanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog()
            {
                Filter      = Properties.Resources.CKANFileFilter,
                Multiselect = true,
            };

            if (open_file_dialog.ShowDialog() == DialogResult.OK)
            {
                // We'll need to make some registry changes to do this.
                RegistryManager registry_manager = RegistryManager.Instance(CurrentInstance);

                foreach (string path in open_file_dialog.FileNames)
                {
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

                        continue;
                    }
                    catch (Exception ex)
                    {
                        currentUser.RaiseError(ex.Message);
                        continue;
                    }

                    if (module.IsDLC)
                    {
                        currentUser.RaiseError(Properties.Resources.MainCantInstallDLC, module);
                        continue;
                    }

                    menuStrip1.Enabled = false;

                    InstallModuleDriver(registry_manager.registry, module);
                }
                registry_manager.Save(true);
            }
        }

        private void CompatibleGameVersionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompatibleGameVersionsDialog dialog = new CompatibleGameVersionsDialog(
                Instance.manager.CurrentInstance,
                !actuallyVisible
            );
            if (dialog.ShowDialog() != DialogResult.Cancel)
            {
                // This takes a while, so don't do it if they cancel out
                ManageMods.UpdateModsList();
            }
        }

        private void ManageMods_OnSelectedModuleChanged(GUIMod m)
        {
            ActiveModInfo = m;
        }

        private GUIMod ActiveModInfo
        {
            set {
                if (value?.ToModule() == null)
                {
                    splitContainer1.Panel2Collapsed = true;
                }
                else
                {
                    if (splitContainer1.Panel2Collapsed)
                    {
                        splitContainer1.Panel2Collapsed = false;
                    }
                    ModInfo.SelectedModule = value;
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

        private void ManageMods_OnChangeSetChanged(IEnumerable<ModChange> changeset)
        {
            if (changeset != null && changeset.Any())
            {
                tabController.ShowTab("ChangesetTabPage", 1, false);
                UpdateChangesDialog(changeset.ToList());
                auditRecommendationsMenuItem.Enabled = false;
            }
            else
            {
                tabController.HideTab("ChangesetTabPage");
                Wait.RetryEnabled = false;
                auditRecommendationsMenuItem.Enabled = true;
            }
        }

        private void ManageMods_OnRegistryChanged()
        {
            needRegistrySave = true;
        }

        private void MainTabControl_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            switch (MainTabControl.SelectedTab?.Name)
            {
                case "ManageModsTabPage":
                    // TODO: Call public func on ManageMods
                    //ModList_SelectedIndexChanged(sender, e);
                    break;

                case "ChangesetTabPage":
                    ShowSelectionModInfo(Changeset.SelectedItems);
                    break;

                case "ChooseRecommendedModsTabPage":
                    ShowSelectionModInfo(ChooseRecommendedMods.SelectedItems);
                    break;

                case "ChooseProvidedModsTabPage":
                    ShowSelectionModInfo(ChooseProvidedMods.SelectedItems);
                    break;

                default:
                    ShowSelectionModInfo(null);
                    break;
            }
        }

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL(HelpURLs.UserGuide);
        }

        private void discordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL(HelpURLs.Discord);
        }

        private void reportClientIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL("https://github.com/KSP-CKAN/CKAN/issues/new/choose");
        }

        private void reportMetadataIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL("https://github.com/KSP-CKAN/NetKAN/issues/new/choose");
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            UpdateTrayState();
        }

        private void openGameDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL(Instance.manager.CurrentInstance.GameDir());
        }

        private void openGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentInstance.LaunchGame(currentUser, launchAnyWay);
        }

        public Tuple<bool, bool> launchAnyWay(string text, string suppressText)
        {
            var result = SuppressableYesNoDialog(text, suppressText,
            Properties.Resources.MainLaunch, Properties.Resources.MainGoBack);
            
            return new Tuple<bool, bool>(result.Item1 == DialogResult.Yes, result.Item2);
        }
        private void ManageMods_StartChangeSet(List<ModChange> changeset)
        {
            // Hand off to centralized [un]installer code
            installWorker.RunWorkerAsync(
                new KeyValuePair<List<ModChange>, RelationshipResolverOptions>(
                    changeset,
                    RelationshipResolver.DependsOnlyOpts()
                )
            );
        }

        private void ManageMods_OpenProgressTab()
        {
            ResetProgress();
            ShowWaitDialog(false);
            tabController.SetTabLock(true);
            Util.Invoke(this, SwitchEnabledState);
            Wait.ClearLog();
        }

        private void ManageMods_CloseProgressTab()
        {
            Util.Invoke(this, UpdateTrayInfo);
            HideWaitDialog(true);
            tabController.HideTab("WaitTabPage");
            tabController.SetTabLock(false);
            tabController.ShowTab("ManageModsTabPage");
            Util.Invoke(this, SwitchEnabledState);
        }
    }
}
