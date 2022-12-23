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
    public partial class Main : Form, IMessageFilter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Main));

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
                CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(mainConfig.Language);
            }

            Application.AddMessageFilter(this);

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
                        regMgr.previousCorruptedPath = null;
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
            var regMgr = RegistryManager.Instance(CurrentInstance);
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
                    ManageMods.ModGrid.Rows.Clear();
                    UpdateRepo();
                }
                else
                {
                    SetupDefaultSearch();
                    RefreshModList();
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
            Application.RemoveMessageFilter(this);
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
            Console.CancelKeyPress += (sender, evt) =>
            {
                // Hide tray icon on Ctrl-C
                minimizeNotifyIcon.Visible = false;
            };
            InitRefreshTimer();

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

            // Save settings
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
                ), false);
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
            new SettingsDialog(currentUser).ShowDialog(this);
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
                Filter = Properties.Resources.CKANFileFilter,
                Multiselect = true,
            };

            if (open_file_dialog.ShowDialog() == DialogResult.OK)
            {
                // We'll need to make some registry changes to do this.
                RegistryManager registry_manager = RegistryManager.Instance(CurrentInstance);
                var crit = CurrentInstance.VersionCriteria();

                var installed = registry_manager.registry.InstalledModules.Select(inst => inst.Module).ToList();
                var toInstall = new List<CkanModule>();

                foreach (string path in open_file_dialog.FileNames)
                {
                    CkanModule module;

                    try
                    {
                        module = CkanModule.FromFile(path);
                        if (module.IsMetapackage && module.depends != null)
                        {
                            // Add metapackage dependencies to the changeset so we can skip compat checks for them
                            toInstall.AddRange(module.depends
                                .Where(rel => !rel.MatchesAny(installed, null, null))
                                .Select(rel =>
                                    // If there's a compatible match, return it
                                    // Metapackages aren't intending to prompt users to choose providing mods
                                    rel.ExactMatch(registry_manager.registry, crit, installed, toInstall)
                                    // Otherwise look for incompatible
                                    ?? rel.ExactMatch(registry_manager.registry, null, installed, toInstall))
                                .Where(mod => mod != null));
                        }
                        toInstall.Add(module);
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
                }
                // Get all recursively incompatible module identifiers (quickly)
                var allIncompat = registry_manager.registry.IncompatibleModules(crit)
                    .Select(mod => mod.identifier)
                    .ToHashSet();
                // Get incompatible mods we're installing
                var myIncompat = toInstall.Where(mod => allIncompat.Contains(mod.identifier)).ToList();
                if (!myIncompat.Any()
                    // Confirm installation of incompatible like the Versions tab does
                    || Main.Instance.YesNoDialog(
                        string.Format(Properties.Resources.ModpackInstallIncompatiblePrompt,
                            string.Join(Environment.NewLine, myIncompat),
                            crit.ToSummaryString(CurrentInstance.game)),
                        Properties.Resources.AllModVersionsInstallYes,
                        Properties.Resources.AllModVersionsInstallNo))
                {
                    InstallModuleDriver(registry_manager.registry, toInstall);
                }
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
                RefreshModList();
            }
        }

        private const int WM_XBUTTONDOWN = 0x20b;
        private const int MK_XBUTTON1 = 0x20;
        private const int MK_XBUTTON2 = 0x40;

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_XBUTTONDOWN:
                    switch (m.WParam.ToInt32() & 0xffff)
                    {
                        case MK_XBUTTON1:
                            ManageMods.NavGoBackward();
                            break;

                        case MK_XBUTTON2:
                            ManageMods.NavGoForward();
                            break;
                    }
                    break;
            }
            return false;
        }

        private void ManageMods_OnSelectedModuleChanged(GUIMod m)
        {
            ActiveModInfo = m;
        }

        private GUIMod ActiveModInfo
        {
            set
            {
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

        private void ManageMods_OnChangeSetChanged(List<ModChange> changeset)
        {
            if (changeset != null && changeset.Any())
            {
                tabController.ShowTab("ChangesetTabPage", 1, false);
                UpdateChangesDialog(changeset);
                auditRecommendationsMenuItem.Enabled = false;
            }
            else
            {
                tabController.HideTab("ChangesetTabPage");
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
                    ActiveModInfo = ManageMods.SelectedModule;
                    ManageMods.ModGrid.Focus();
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
            LaunchGame();
        }

        public void LaunchGame()
        {
            var split = configuration.CommandLineArguments.Split(' ');
            if (split.Length == 0)
                return;

            var registry = RegistryManager.Instance(CurrentInstance).registry;

            var suppressedIdentifiers = CurrentInstance.GetSuppressedCompatWarningIdentifiers;
            var incomp = registry.IncompatibleInstalled(CurrentInstance.VersionCriteria())
                .Where(m => !m.Module.IsDLC && !suppressedIdentifiers.Contains(m.identifier))
                .ToList();
            if (incomp.Any())
            {
                // Warn that it might not be safe to run Game with incompatible modules installed
                string incompatDescrip = incomp
                    .Select(m => $"{m.Module} ({registry.CompatibleGameVersions(CurrentInstance.game, m.Module)})")
                    .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");
                var ver = CurrentInstance.Version();
                var result = SuppressableYesNoDialog(
                    string.Format(Properties.Resources.MainLaunchWithIncompatible, incompatDescrip),
                    string.Format(Properties.Resources.MainLaunchDontShow,
                        CurrentInstance.game.ShortName,
                        new GameVersion(ver.Major, ver.Minor, ver.Patch)),
                    Properties.Resources.MainLaunch,
                    Properties.Resources.MainGoBack
                );
                if (result.Item1 != DialogResult.Yes)
                {
                    return;
                }
                else if (result.Item2)
                {
                    CurrentInstance.AddSuppressedCompatWarningIdentifiers(
                        incomp.Select(m => m.identifier).ToHashSet()
                    );
                }
            }

            split = CurrentInstance.game.AdjustCommandLine(split, CurrentInstance.Version());
            var binary = split[0];
            var args = string.Join(" ", split.Skip(1));

            try
            {

                Directory.SetCurrentDirectory(CurrentInstance.GameDir());

                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = binary,
                        Arguments = args
                    },
                    EnableRaisingEvents = true
                };

                GameInstance inst = CurrentInstance;
                p.Exited += (sender, e) => GameExit(inst);

                p.Start();
                CurrentInstance.playTime.Start();
            }
            catch (Exception exception)
            {
                currentUser.RaiseError(Properties.Resources.MainLaunchFailed, exception.Message);
            }
        }

        private void GameExit(GameInstance inst)
        {
            inst.playTime.Stop(inst.CkanDir());
            UpdateStatusBar();
        }

        // This is used by Reinstall
        private void ManageMods_StartChangeSet(List<ModChange> changeset)
        {
            UpdateChangesDialog(changeset);
            tabController.ShowTab("ChangesetTabPage", 1);
        }

        private void RefreshModList(Dictionary<string, bool> oldModules = null)
        {
            tabController.RenameTab("WaitTabPage", Properties.Resources.MainModListWaitTitle);
            ShowWaitDialog();
            DisableMainWindow();
            Wait.StartWaiting(
                ManageMods.Update,
                (sender, e) =>
                {
                    UpdateTrayInfo();
                    HideWaitDialog();
                    EnableMainWindow();
                    SetupDefaultSearch();
                },
                false,
                oldModules);
        }

        private void EnableMainWindow()
        {
            Util.Invoke(this, () =>
            {
                Enabled = true;
                menuStrip1.Enabled = true;
                tabController.SetTabLock(false);
                /* Windows (7 & 8 only?) bug #1548 has extra facets.
                 * parent.childcontrol.Enabled = false seems to disable the parent,
                 * if childcontrol had focus. Depending on optimization steps,
                 * parent.childcontrol.Enabled = true does not necessarily
                 * re-enable the parent.*/
                this.Focus();
            });
        }

        private void DisableMainWindow()
        {
            Util.Invoke(this, () =>
            {
                menuStrip1.Enabled = false;
                tabController.SetTabLock(true);
            });
        }

    }
}
