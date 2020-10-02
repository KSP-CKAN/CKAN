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
using CKAN.Extensions;
using CKAN.Properties;
using CKAN.Types;
using log4net;
using Timer = System.Windows.Forms.Timer;
using Autofac;

namespace CKAN
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

        public string[] commandLineArgs;

        public GUIUser currentUser;

        private bool enableTrayIcon;
        private bool minimizeToTray;

        public Main(string[] cmdlineArgs, KSPManager mgr, bool showConsole)
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
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(mainConfig.Language);
            }

            InitializeComponent();

            Instance = this;

            currentUser = new GUIUser(this, this.Wait);
            manager = mgr ?? new KSPManager(currentUser);

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

            // We want to check if our current instance is null first,
            // as it may have already been set by a command-line option.
            if (CurrentInstance == null && manager.GetPreferredInstance() == null)
            {
                Hide();

                var result = new ManageKspInstancesDialog(!actuallyVisible, currentUser).ShowDialog();
                if (result == DialogResult.Cancel || result == DialogResult.Abort)
                {
                    Application.Exit();
                    return;
                }
            }

            configuration = GUIConfiguration.LoadOrCreateConfiguration
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
            ModInfo.ModMetaSplitPosition = configuration.ModInfoPosition;

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
                ManageMods.FocusMod(identifier, true, true);
                ManageMods.ModGrid.Refresh();
                log.Debug("Failed to select mod from startup parameters");
            }

            var pluginsPath = Path.Combine(CurrentInstance.CkanDir(), "Plugins");
            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);

            pluginController = new PluginController(pluginsPath);

            CurrentInstance.RebuildKSPSubDir();

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

            // Save the active filter
            configuration.ActiveFilter = (int)ManageMods.mainModList.ModFilter;
            configuration.CustomLabelFilter = ManageMods.mainModList.CustomLabelFilter?.Name;

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
        /// <param name="allowRepoUpdate">true if a repo update is allowed if needed (e.g. on initial load), false otherwise</param>
        private void CurrentInstanceUpdated(bool allowRepoUpdate)
        {
            Util.Invoke(this, () =>
            {
                Text = $"CKAN {Meta.GetVersion()} - KSP {CurrentInstance.Version()}    --    {CurrentInstance.GameDir().Replace('/', Path.DirectorySeparatorChar)}";
                StatusInstanceLabel.Text = string.Format(
                    Properties.Resources.StatusInstanceLabelText,
                    CurrentInstance.Name,
                    CurrentInstance.Version()?.ToString()
                );
            });

            configuration = GUIConfiguration.LoadOrCreateConfiguration(
                Path.Combine(CurrentInstance.CkanDir(), "GUIConfig.xml")
            );

            if (CurrentInstance.CompatibleVersionsAreFromDifferentKsp)
            {
                new CompatibleKspVersionsDialog(CurrentInstance, !actuallyVisible)
                    .ShowDialog();
            }

            (RegistryManager.Instance(CurrentInstance).registry as Registry)
                ?.BuildTagIndex(ManageMods.mainModList.ModuleTags);

            bool repoUpdateNeeded = configuration.RefreshOnStartup
                || !RegistryManager.Instance(CurrentInstance).registry.HasAnyAvailable();
            if (allowRepoUpdate && repoUpdateNeeded)
            {
                // Update the filters after UpdateRepo() completed.
                // Since this happens with a backgroundworker, Filter() is added as callback for RunWorkerCompleted.
                // Remove it again after it ran, else it stays there and is added again and again.
                void filterUpdate(object sender, RunWorkerCompletedEventArgs e)
                {
                    ManageMods.Filter(
                        (GUIModFilter)configuration.ActiveFilter,
                        ManageMods.mainModList.ModuleTags.Tags.GetOrDefault(configuration.TagFilter),
                        ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                            .FirstOrDefault(l => l.Name == configuration.CustomLabelFilter)
                    );
                    m_UpdateRepoWorker.RunWorkerCompleted -= filterUpdate;
                }

                m_UpdateRepoWorker.RunWorkerCompleted += filterUpdate;

                ManageMods.ModGrid.Rows.Clear();
                UpdateRepo();
            }
            else
            {
                ManageMods.UpdateModsList();
                ManageMods.Filter(
                    (GUIModFilter)configuration.ActiveFilter,
                    ManageMods.mainModList.ModuleTags.Tags.GetOrDefault(configuration.TagFilter),
                    ManageMods.mainModList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                        .FirstOrDefault(l => l.Name == configuration.CustomLabelFilter)
                );
            }
            ManageMods.InstanceUpdated(CurrentInstance);
        }

        public void UpdateCKAN()
        {
            ResetProgress();
            ShowWaitDialog(false);
            SwitchEnabledState();
            Wait.ClearLog();
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainUpgradingWaitTitle);
            Wait.SetDescription(string.Format(Properties.Resources.MainUpgradingTo, AutoUpdate.Instance.latestUpdate.Version));

            log.Info("Start ckan update");
            BackgroundWorker updateWorker = new BackgroundWorker();
            updateWorker.DoWork += (sender, args) => AutoUpdate.Instance.StartUpdateProcess(true, currentUser);
            updateWorker.RunWorkerAsync();
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
            new SettingsDialog(currentUser).ShowDialog();
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
            OpenFileDialog open_file_dialog = new OpenFileDialog()
            {
                Filter      = Resources.CKANFileFilter,
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

        private void manageKspInstancesMenuItem_Click(object sender, EventArgs e)
        {
            var old_instance = Instance.CurrentInstance;
            var result = new ManageKspInstancesDialog(!actuallyVisible, currentUser).ShowDialog();
            if (result == DialogResult.OK && !Equals(old_instance, Instance.CurrentInstance))
            {
                ManageMods.ModGrid.ClearSelection();
                CurrentInstanceUpdated(true);
            }
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

        private void openKspDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utilities.ProcessStartURL(Instance.manager.CurrentInstance.GameDir());
        }

        private void openKSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchKSP();
        }

        public void LaunchKSP()
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

            // -single-instance crashes KSP 1.8 to KSP 1.9 on Linux
            // https://issuetracker.unity3d.com/issues/linux-segmentation-fault-when-running-a-built-project-with-single-instance-argument
            if (Platform.IsUnix)
            {
                var brokenVersionRange = new KspVersionRange(
                    new KspVersion(1, 8),
                    new KspVersion(1, 9)
                );
                split = filterCmdLineArgs(split, brokenVersionRange, "-single-instance");
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
                currentUser.RaiseError(Properties.Resources.MainLaunchFailed, exception.Message);
            }
        }

        /// <summary>
        /// If the installed game version is in the given range,
        /// return the given array without the given parameter,
        /// otherwise return the array as-is.
        /// </summary>
        /// <param name="args">Command line parameters to check</param>
        /// <param name="crashyKspRange">Game versions that should not use this parameter</param>
        /// <param name="parameter">The parameter to remove on version match</param>
        /// <returns>
        /// args or args minus parameter
        /// </returns>
        private string[] filterCmdLineArgs(string[] args, KspVersionRange crashyKspRange, string parameter)
        {
            var installedRange = Main.Instance.CurrentInstance.Version().ToVersionRange();
            if (crashyKspRange.IntersectWith(installedRange) != null
                && args.Contains(parameter))
            {
                log.DebugFormat(
                    "Parameter {0} found on incompatible KSP version {1}, pruning",
                    parameter,
                    Main.Instance.CurrentInstance.Version().ToString());
                return args.Where(s => s != parameter).ToArray();
            }
            return args;
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
