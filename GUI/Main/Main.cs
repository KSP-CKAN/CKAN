using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using log4net;
using Autofac;

using CKAN.IO;
using CKAN.Versioning;
using CKAN.GUI.Attributes;
using CKAN.Configuration;

// Don't warn if we use our own obsolete properties
#pragma warning disable 0618

namespace CKAN.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public partial class Main : Form, IMessageFilter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Main));

        // Stuff we set in the constructor and never change
        public readonly IUser currentUser;
        public readonly GameInstanceManager Manager;
        public GameInstance? CurrentInstance => Manager.CurrentInstance;
        private readonly RepositoryDataManager repoData;
        private readonly string? userAgent;
        private readonly AutoUpdate updater = new AutoUpdate();
        public bool Waiting => Wait.Busy;

        // Stuff we set when the game instance changes
        public GUIConfiguration? configuration;
        public PluginController? pluginController;

        private readonly TabController tabController;
        private string? focusIdent;

        private bool needRegistrySave = false;

        [Obsolete("Main.Instance is a global singleton. Find a better way to access this object.")]
        public static Main? Instance { get; private set; }

        /// <summary>
        /// Set up the main form's core properties quickly.
        /// This should have NO UI interactions!
        /// A constructor just initializes the object, it doesn't ask the user questions.
        /// That's the job of OnLoad or OnShown.
        /// </summary>
        /// <param name="cmdlineArgs">The strings from the command line that launched us</param>
        /// <param name="mgr">Game instance manager created by the cmdline handler</param>
        /// <param name="userAgent">User agent to use for web queries</param>
        public Main(string[]             cmdlineArgs,
                    GameInstanceManager? mgr,
                    string?              userAgent)
        {
            log.Info("Starting the GUI");
            if (//cmdlineArgs is [_, string focusIdent, ..]
                cmdlineArgs.Length > 1
                && cmdlineArgs[1] is string focusIdent)
            {
                if (//focusIdent is ['/', '/', .. var rest]
                    focusIdent.Length > 2)
                {
                    focusIdent = focusIdent[2..];
                }
                else if (//focusIdent is ['c', 'k', 'a', 'n', ':', '/', '/', .. var rest2]
                    focusIdent.StartsWith("ckan://"))
                {
                    focusIdent = focusIdent[7..];
                }
                if (//focusIdent is [.. var start, '/']
                    focusIdent.EndsWith("/"))
                {
                    focusIdent = focusIdent[..^1];
                }
            }

            var mainConfig = ServiceLocator.Container.Resolve<IConfiguration>();

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
            // React when the user clicks a tag or filter link in mod info
            ModInfo.OnChangeFilter += ManageMods.Filter;
            ModInfo.ModuleDoubleClicked += ManageMods.ResetFilterAndSelectModOnList;
            repoData = ServiceLocator.Container.Resolve<RepositoryDataManager>();
            this.userAgent = userAgent;

            Instance = this;

            // Replace mono's broken, ugly toolstrip renderer
            if (Platform.IsMono)
            {
                MainMenu.Renderer = new FlatToolStripRenderer();
                FileToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                settingsToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                helpToolStripMenuItem.DropDown.Renderer = new FlatToolStripRenderer();
                minimizedContextMenuStrip.Renderer = new FlatToolStripRenderer();
            }

            // Initialize all user interaction dialogs.
            RecreateDialogs();

            // WinForms on Mac OS X has a nasty bug where the UI thread hogs the CPU,
            // making our download speeds really slow unless you move the mouse while
            // downloading. Yielding periodically addresses that.
            // https://bugzilla.novell.com/show_bug.cgi?id=663433
            if (Platform.IsMac)
            {
                var timer = new Timer
                {
                    Interval = 2
                };
                timer.Tick += (sender, e) => Thread.Yield();
                timer.Start();
            }

            // Set the window name and class for X11
            if (Platform.IsX11)
            {
                HandleCreated += (sender, e) => X11.SetWMClass(Meta.ProductName,
                                                               Meta.ProductName, Handle);
            }

            currentUser = new GUIUser(this, Wait, StatusLabel, StatusProgress);
            if (mgr != null)
            {
                // With a working GUI, assign a GUIUser to the GameInstanceManager to replace the ConsoleUser
                mgr.User = currentUser;
                Manager = mgr;
            }
            else
            {
                Manager = new GameInstanceManager(currentUser, mainConfig);
            }

            Manager.CacheChanged += OnCacheChanged;
            OnCacheChanged(null);
            Manager.InstanceChanged += Manager_InstanceChanged;

            tabController = new TabController(MainTabControl);
            tabController.ShowTab(ManageModsTabPage.Name);

            // Disable the modinfo controls until a mod has been choosen. This has an effect if the modlist is empty.
            ActiveModInfo = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Try to get an instance
            if (CurrentInstance == null)
            {
                // Maybe we can find an instance automatically (e.g., portable, only, default)
                Manager.GetPreferredInstance();
            }

            // We need a config object to get the window geometry, but we don't need the registry lock yet
            configuration = GUIConfigForInstance(
                Manager.SteamLibrary,
                // Find the most recently used instance if no default instance
                CurrentInstance ?? InstanceWithNewestGUIConfig(Manager.Instances.Values));

            // This must happen before Shown, and it depends on the configuration
            SetStartPosition();
            Size = configuration.WindowSize;
            WindowState = configuration.IsWindowMaximised ? FormWindowState.Maximized : FormWindowState.Normal;

            URLHandlers.RegisterURLHandler(configuration, CurrentInstance, currentUser);

            Util.Invoke(this, () => Text = $"CKAN {Meta.GetVersion()}");

            splitContainer1.SplitterDistance = configuration.PanelPosition;

            log.Info("GUI started");
            base.OnLoad(e);
        }

        private static GUIConfiguration GUIConfigForInstance(SteamLibrary steamLib, GameInstance? inst)
            => inst == null ? new GUIConfiguration()
                            : GUIConfiguration.LoadOrCreateConfiguration(inst, steamLib);

        private static GameInstance? InstanceWithNewestGUIConfig(IEnumerable<GameInstance> instances)
            => instances.Where(inst => inst.Valid)
                        .OrderByDescending(GUIConfiguration.LastWriteTime)
                        .ThenBy(inst => inst.Name)
                        .FirstOrDefault();

        /// <summary>
        /// Form.Visible says true even when the form hasn't shown yet.
        /// This value will tell the truth.
        /// </summary>
        public bool actuallyVisible { get; private set; } = false;

        protected override void OnShown(EventArgs e)
        {
            actuallyVisible = true;

            tabController.RenameTab(WaitTabPage.Name, Properties.Resources.MainLoadingGameInstance);
            ShowWaitDialog();
            DisableMainWindow();
            Wait.StartWaiting(
                (sender, evt) =>
                {
                    if (evt != null)
                    {
                        currentUser.RaiseMessage(Properties.Resources.MainModListLoadingRegistry);
                        // Make sure we have a lockable instance
                        do
                        {
                            if (CurrentInstance == null && !InstancePromptAtStart())
                            {
                                // User cancelled, give up
                                evt.Result = false;
                                return;
                            }
                            for (RegistryManager? regMgr = null;
                                 CurrentInstance != null && regMgr == null;)
                            {
                                // We now have a tentative instance. Check if it's locked.
                                try
                                {
                                    // This will throw RegistryInUseKraken if locked by another process
                                    regMgr = RegistryManager.Instance(CurrentInstance, repoData);
                                    // Tell the user their registry was reset if it was corrupted
                                    if (regMgr.previousCorruptedMessage != null
                                        && regMgr.previousCorruptedPath != null)
                                    {
                                        errorDialog.ShowErrorDialog(this,
                                            Properties.Resources.MainCorruptedRegistry,
                                            regMgr.previousCorruptedPath, regMgr.previousCorruptedMessage,
                                            Path.Combine(Path.GetDirectoryName(regMgr.previousCorruptedPath) ?? "", regMgr.LatestInstalledExportFilename()));
                                        regMgr.previousCorruptedMessage = null;
                                        regMgr.previousCorruptedPath    = null;
                                        // But the instance is actually fine because a new registry was just created
                                    }
                                }
                                catch (RegistryInUseKraken kraken)
                                {
                                    if (YesNoDialog(
                                        kraken.Message,
                                        Properties.Resources.MainDeleteLockfileYes,
                                        Properties.Resources.MainDeleteLockfileNo))
                                    {
                                        // Delete it
                                        File.Delete(kraken.lockfilePath);
                                        // Loop back around to re-acquire the lock
                                    }
                                    else
                                    {
                                        // Couldn't get the lock, there is no current instance
                                        Manager.SetCurrentInstance((GameInstance?)null);
                                        if (Manager.Instances.Values.All(inst => !inst.Valid || inst.IsMaybeLocked))
                                        {
                                            // Everything's invalid or locked, give up
                                            evt.Result = false;
                                            return;
                                        }
                                    }
                                }
                                catch (RegistryVersionNotSupportedKraken kraken)
                                {
                                    currentUser.RaiseError("{0}", kraken.Message);
                                    if (CheckForCKANUpdate())
                                    {
                                        UpdateCKAN();
                                    }
                                    else
                                    {
                                        Manager.SetCurrentInstance((GameInstance?)null);
                                        if (Manager.Instances.Values.All(inst => !inst.Valid || inst.IsMaybeLocked))
                                        {
                                            // Everything's invalid or locked, give up
                                            evt.Result = false;
                                            return;
                                        }
                                    }
                                }
                            }
                        } while (CurrentInstance == null);
                        // We can only reach this point if CurrentInstance is not null
                        // AND we acquired the lock for it successfully
                        evt.Result = true;
                    }
                },
                (sender, evt) =>
                {
                    if (evt != null)
                    {
                        // Application.Exit doesn't work if the window is disabled!
                        EnableMainWindow();
                        if (evt.Error != null)
                        {
                            currentUser.RaiseError("{0}", evt.Error.Message);
                        }
                        else if (evt.Result is bool b && b)
                        {
                            HideWaitDialog();
                            CheckTrayState();
                            Console.CancelKeyPress += (sender2, evt2) =>
                            {
                                // Hide tray icon on Ctrl-C
                                minimizeNotifyIcon.Visible = false;
                            };
                            InitRefreshTimer();
                            CurrentInstanceUpdated();
                        }
                        else
                        {
                            Application.Exit();
                        }
                    }
                },
                false,
                null);

            base.OnShown(e);
        }

        private bool InstancePromptAtStart()
        {
            bool gotInstance = false;
            Util.Invoke(this, () =>
            {
                var result = new ManageGameInstancesDialog(Manager, !actuallyVisible, currentUser).ShowDialog(this);
                gotInstance = result == DialogResult.OK;
            });
            return gotInstance;
        }

        private void StabilityToleranceConfig_Changed(string? identifier, ReleaseStatus? relStat)
        {
            // null represents the overall setting, for which we'll refresh when the settings dialog closes
            if (identifier != null)
            {
                RefreshModList(false);
            }
        }

        private void UpdateStatusBar()
        {
            if (CurrentInstance != null)
            {
                StatusInstanceLabel.Text = string.Format(
                    CurrentInstance.playTime != null
                    && CurrentInstance.playTime.Time > TimeSpan.Zero
                        ? Properties.Resources.StatusInstanceLabelTextWithPlayTime
                        : Properties.Resources.StatusInstanceLabelText,
                    CurrentInstance.Name,
                    CurrentInstance.game.ShortName,
                    CurrentInstance.Version()?.ToString(),
                    CurrentInstance.playTime?.ToString() ?? "");
            }
        }

        private void Manager_InstanceChanged(GameInstance? previous, GameInstance? current)
        {
            if (previous != null)
            {
                if (needRegistrySave)
                {
                    using (var transaction = CkanTransaction.CreateTransactionScope())
                    {
                        // Save registry
                        RegistryManager.Instance(previous, repoData).Save(false);
                        transaction.Complete();
                        needRegistrySave = false;
                    }
                }
                configuration?.Save(previous);
            }
            configuration = GUIConfigForInstance(Manager.SteamLibrary, current);
        }

        /// <summary>
        /// React to switching to a new game instance
        /// </summary>
        private void CurrentInstanceUpdated()
        {
            if (CurrentInstance == null || configuration == null)
            {
                return;
            }
            CurrentInstance.StabilityToleranceConfig.Changed += StabilityToleranceConfig_Changed;
            // This will throw RegistryInUseKraken if locked by another process
            var regMgr = RegistryManager.Instance(CurrentInstance, repoData);
            log.Debug("Current instance updated, scanning");

            Util.Invoke(this, () =>
            {
                Text = $"{Meta.ProductName} {Meta.GetVersion()} - {CurrentInstance.game.ShortName} {CurrentInstance.Version()}    --    {Platform.FormatPath(CurrentInstance.GameDir())}";
                minimizeNotifyIcon.Text = $"{Meta.ProductName} - {CurrentInstance.Name}";
                UpdateStatusBar();
            });

            if (CurrentInstance.CompatibleVersionsAreFromDifferentGameVersion)
            {
                new CompatibleGameVersionsDialog(CurrentInstance, !actuallyVisible)
                    .ShowDialog(this);
            }

            var registry = regMgr.registry;
            if (regMgr.previousCorruptedMessage != null
                && regMgr.previousCorruptedPath != null)
            {
                errorDialog.ShowErrorDialog(this,
                    Properties.Resources.MainCorruptedRegistry,
                    regMgr.previousCorruptedPath, regMgr.previousCorruptedMessage,
                    Path.Combine(Path.GetDirectoryName(regMgr.previousCorruptedPath) ?? "", regMgr.LatestInstalledExportFilename()));
                regMgr.previousCorruptedMessage = null;
                regMgr.previousCorruptedPath = null;
            }

            var pluginsPath = Path.Combine(CurrentInstance.CkanDir(), "Plugins");
            if (!Directory.Exists(pluginsPath))
            {
                Directory.CreateDirectory(pluginsPath);
            }
            pluginController = new PluginController(pluginsPath, true);

            CurrentInstance.game.RebuildSubdirectories(CurrentInstance.GameDir());

            ManageMods.InstanceUpdated();

            AutoUpdatePrompts(ServiceLocator.Container
                                            .Resolve<IConfiguration>(),
                              configuration);

            if (configuration.CheckForUpdatesOnLaunch && CheckForCKANUpdate())
            {
                UpdateCKAN();
            }
            else
            {
                if (configuration.RefreshOnStartup)
                {
                    UpdateRepo(refreshWithoutChanges: true);
                }
                else
                {
                    RefreshModList(registry.Repositories.Count > 0);
                }
            }
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
            if (CurrentInstance != null)
            {
                RegistryManager.DisposeInstance(CurrentInstance);
            }

            // Stop all running play time timers
            foreach (var inst in Manager.Instances.Values)
            {
                if (inst.Valid)
                {
                    inst.playTime?.Stop(inst.CkanDir());
                }
            }
            Application.RemoveMessageFilter(this);
            actuallyVisible = false;
            base.OnFormClosed(e);
        }

        private void SetStartPosition()
        {
            if (configuration != null)
            {
                var screen = Util.FindScreen(configuration.WindowLoc, configuration.WindowSize);
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
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Only close the window, when the user has access to the "Exit" of the menu.
            if (!MainMenu.Enabled)
            {
                e.Cancel = true;
                return;
            }

            if (!ManageMods.AllowClose())
            {
                e.Cancel = true;
                return;
            }

            if (configuration != null)
            {
                // Copy window location to app settings
                configuration.WindowLoc = WindowState == FormWindowState.Normal ? Location : RestoreBounds.Location;

                // Copy window size to app settings if not maximized
                configuration.WindowSize = WindowState == FormWindowState.Normal ? Size : RestoreBounds.Size;

                //copy window maximized state to app settings
                configuration.IsWindowMaximised = WindowState == FormWindowState.Maximized;

                // Copy panel position to app settings
                configuration.PanelPosition = splitContainer1.SplitterDistance;

                // Save settings
                if (CurrentInstance != null)
                {
                    configuration.Save(CurrentInstance);
                }
            }

            if (needRegistrySave && CurrentInstance != null)
            {
                using (var transaction = CkanTransaction.CreateTransactionScope())
                {
                    // Save registry
                    RegistryManager.Instance(CurrentInstance, repoData).Save(false);
                    transaction.Complete();
                    needRegistrySave = false;
                }
            }

            // Remove the tray icon
            minimizeNotifyIcon.Visible = false;
            minimizeNotifyIcon.Dispose();

            base.OnFormClosing(e);
        }

        // https://learn.microsoft.com/en-us/windows/win32/winmsg/window-notifications
        private const int WM_PARENTNOTIFY = 0x210;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (Platform.IsWindows)
            {
                switch (m.Msg)
                {
                    // Windows sends us this when you click outside the search dropdown
                    // Mono closes the dropdown automatically
                    case WM_PARENTNOTIFY:
                        ManageMods?.CloseSearch(
                            PointToScreen(new Point(m.LParam.ToInt32() & 0xffff,
                                                    m.LParam.ToInt32() >> 16)));
                        break;
                }
            }
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            ManageMods?.ParentMoved();
        }

        private IEnumerable<T> VisibleControls<T>()
            => (MainTabControl?.SelectedTab
                              ?.Controls
                               .OfType<T>()
                              ?? Enumerable.Empty<T>())
                  .Concat(!splitContainer1.Panel2Collapsed
                          && ModInfo is T t
                              ? Enumerable.Repeat(t, 1)
                              : Enumerable.Empty<T>());

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Control | Keys.F:
                    VisibleControls<ISearchableControl>()
                        .FirstOrDefault()
                        ?.FocusSearch(false);
                    e.Handled = true;
                    break;
                case Keys.Control | Keys.Shift | Keys.F:
                    VisibleControls<ISearchableControl>()
                        .FirstOrDefault()
                        ?.FocusSearch(true);
                    e.Handled = true;
                    break;
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        private void SetupDefaultSearch()
        {
            if (CurrentInstance != null && configuration != null)
            {
                var registry = RegistryManager.Instance(CurrentInstance, repoData).registry;
                var def = configuration.DefaultSearches;
                if (def == null || def.Count < 1)
                {
                    // Fall back to old setting
                    ManageMods.Filter(
                        ModList.FilterToSavedSearch(
                            CurrentInstance,
                            (GUIModFilter)configuration.ActiveFilter, ModuleLabelList.ModuleLabels,
                            configuration.TagFilter == null
                                ? null
                                : registry.Tags.GetValueOrDefault(configuration.TagFilter),
                            ModuleLabelList.ModuleLabels.LabelsFor(CurrentInstance.Name)
                                .FirstOrDefault(l => l.Name == configuration.CustomLabelFilter)),
                        false);
                    // Clear the old filter so it doesn't get pulled forward again
                    configuration.ActiveFilter = (int)GUIModFilter.All;
                }
                else
                {
                    var searches = def.Select(s => ModSearch.Parse(ModuleLabelList.ModuleLabels, CurrentInstance, s))
                                      .OfType<ModSearch>()
                                      .ToList();
                    ManageMods.SetSearches(searches);
                }
            }
        }

        private void EditCommandLines()
        {
            if (CurrentInstance != null && configuration != null)
            {
                var dialog = new GameCommandLineOptionsDialog();
                var defaults = CurrentInstance.game.DefaultCommandLines(Manager.SteamLibrary,
                                                                        new DirectoryInfo(CurrentInstance.GameDir()));
                if (dialog.ShowGameCommandLineOptionsDialog(this, configuration.CommandLines, defaults) == DialogResult.OK)
                {
                    configuration.CommandLines = dialog.Results;
                }
            }
        }

        private void InstallFromCkanFiles(string[] files)
        {
            if (CurrentInstance == null)
            {
                return;
            }
            // We'll need to make some registry changes to do this.
            var registry_manager = RegistryManager.Instance(CurrentInstance, repoData);
            var stabilityTolerance = CurrentInstance.StabilityToleranceConfig;
            var crit = CurrentInstance.VersionCriteria();

            var installed = registry_manager.registry.InstalledModules.Select(inst => inst.Module).ToList();
            var toInstall = new List<CkanModule>();
            foreach (string path in files)
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
                                rel.ExactMatch(registry_manager.registry, stabilityTolerance, crit, installed, toInstall)
                                // Otherwise look for incompatible
                                ?? rel.ExactMatch(registry_manager.registry, stabilityTolerance, null, installed, toInstall))
                            .OfType<CkanModule>());
                    }
                    toInstall.Add(module);
                }
                catch (Kraken kraken)
                {
                    currentUser.RaiseError("{0}", kraken.InnerException == null
                        ? kraken.Message
                        : $"{kraken.Message}: {kraken.InnerException.Message}");

                    continue;
                }
                catch (Exception ex)
                {
                    currentUser.RaiseError("{0}", ex.Message);
                    continue;
                }

                if (module.IsDLC)
                {
                    currentUser.RaiseError(Properties.Resources.MainCantInstallDLC, module);
                    continue;
                }
            }

            var modpacks = toInstall.Where(m => m.IsMetapackage)
                                    .ToArray();
            if (modpacks.Any())
            {
                CkanModule.GetMinMaxVersions(modpacks,
                                             out _, out _,
                                             out GameVersion? minGame, out GameVersion? maxGame);
                var filesRange = new GameVersionRange(minGame ?? GameVersion.Any,
                                                      maxGame ?? GameVersion.Any);
                var instRanges = crit.Versions.Select(gv => gv.ToVersionRange())
                                              .ToList();
                var missing = CurrentInstance.game
                                             .KnownVersions
                                             .Where(gv => filesRange.Contains(gv)
                                                          && !instRanges.Any(ir => ir.Contains(gv)))
                                             // Use broad Major.Minor group for each specific version
                                             .Select(gv => new GameVersion(gv.Major, gv.Minor))
                                             .Distinct()
                                             .ToList();
                if (missing.Count != 0
                    && YesNoDialog(string.Format(Properties.Resources.MetapackageAddCompatibilityPrompt,
                                                 filesRange.ToSummaryString(CurrentInstance.game),
                                                 crit.ToSummaryString(CurrentInstance.game)),
                                   Properties.Resources.MetapackageAddCompatibilityYes,
                                   Properties.Resources.MetapackageAddCompatibilityNo))
                {
                    CurrentInstance.SetCompatibleVersions(crit.Versions
                                                              .Concat(missing)
                                                              .ToList());
                    crit = CurrentInstance.VersionCriteria();
                }
            }

            // Get all recursively incompatible module identifiers (quickly)
            var allIncompat = registry_manager.registry.IncompatibleModules(stabilityTolerance, crit)
                .Select(mod => mod.identifier)
                .ToHashSet();
            // Get incompatible mods we're installing
            var myIncompat = toInstall.Where(mod => allIncompat.Contains(mod.identifier)).ToList();
            if (myIncompat.Count == 0
                // Confirm installation of incompatible like the Versions tab does
                || YesNoDialog(string.Format(Properties.Resources.ModpackInstallIncompatiblePrompt,
                                             string.Join(Environment.NewLine, myIncompat),
                                             crit.ToSummaryString(CurrentInstance.game)),
                               Properties.Resources.AllModVersionsInstallYes,
                               Properties.Resources.AllModVersionsInstallNo))
            {
                UpdateChangesDialog(toInstall.Select(m => new ModChange(m, GUIModChangeType.Install,
                                                                        ServiceLocator.Container.Resolve<IConfiguration>()))
                                             .ToList(),
                                    null);
                tabController.ShowTab(ChangesetTabPage.Name, 1);
            }
        }

        private const int WM_XBUTTONDOWN = 0x20b;
        private const int MK_XBUTTON1    = 0x20;
        private const int MK_XBUTTON2    = 0x40;

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
            if (MainTabControl.SelectedTab == ManageModsTabPage)
            {
                ActiveModInfo = m;
            }
        }

        private GUIMod? ActiveModInfo
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
                }
                ModInfo.SelectedModule = value;
            }
        }

        private void ShowSelectionModInfo(CkanModule? module)
        {
            if (CurrentInstance != null && configuration != null)
            {
                ActiveModInfo = module == null ? null : new GUIMod(
                    module,
                    repoData,
                    RegistryManager.Instance(CurrentInstance, repoData).registry,
                    CurrentInstance.StabilityToleranceConfig,
                    CurrentInstance.VersionCriteria(),
                    null,
                    configuration.HideEpochs,
                    configuration.HideV);
            }
        }

        private void ShowSelectionModInfo(ListView.SelectedListViewItemCollection selection)
        {
            ShowSelectionModInfo(selection?.OfType<ListViewItem>()
                                           .FirstOrDefault()?.Tag as CkanModule);
        }

        private void ManageMods_OnChangeSetChanged(List<ModChange> changeset, Dictionary<GUIMod, string> conflicts)
        {
            if (changeset != null && changeset.Count != 0)
            {
                tabController.ShowTab(ChangesetTabPage.Name, 1, false);
                UpdateChangesDialog(
                    changeset,
                    conflicts.ToDictionary(item => item.Key.ToCkanModule(),
                                           item => item.Value));
                AuditRecommendationsToolStripMenuItem.Enabled = false;
            }
            else
            {
                tabController.HideTab(ChangesetTabPage.Name);
                AuditRecommendationsToolStripMenuItem.Enabled = true;
            }
        }

        private void ManageMods_OnRegistryChanged()
        {
            needRegistrySave = true;
        }

        private void ManageMods_RaiseMessage(string message)
        {
            currentUser.RaiseMessage("{0}", message);
        }

        private void ManageMods_RaiseError(string error)
        {
            currentUser.RaiseError("{0}", error);
        }

        private void ManageMods_SetStatusBar(string message)
        {
            StatusLabel.ToolTipText = StatusLabel.Text = message;
        }

        private void ManageMods_ClearStatusBar()
        {
            StatusLabel.ToolTipText = StatusLabel.Text = "";
        }

        private void MainTabControl_OnSelectedIndexChanged(object? sender, EventArgs? e)
        {
            switch (MainTabControl.SelectedTab?.Name)
            {
                case "ManageModsTabPage":
                    ActiveModInfo = ManageMods.SelectedModule;
                    ManageMods.ModGrid.Focus();
                    break;

                case "ChangesetTabPage":
                    ShowSelectionModInfo(Changeset.SelectedItem);
                    break;

                case "ChooseRecommendedModsTabPage":
                    ShowSelectionModInfo(ChooseRecommendedMods.SelectedItems);
                    break;

                case "ChooseProvidedModsTabPage":
                    ShowSelectionModInfo(ChooseProvidedMods.SelectedItems);
                    break;

                default:
                    ShowSelectionModInfo(null as CkanModule);
                    break;
            }
        }

        private void Main_Resize(object? sender, EventArgs? e)
        {
            UpdateTrayState();
        }

        private void LaunchGame(string command)
        {
            if (CurrentInstance != null)
            {
                var registry = RegistryManager.Instance(CurrentInstance, repoData).registry;
                var suppressedIdentifiers = CurrentInstance.GetSuppressedCompatWarningIdentifiers;
                var incomp = registry.IncompatibleInstalled(CurrentInstance.VersionCriteria())
                    .Where(m => !m.Module.IsDLC && !suppressedIdentifiers.Contains(m.identifier))
                    .ToList();
                if (incomp.Count != 0 && CurrentInstance.Version() is GameVersion gv)
                {
                    // Warn that it might not be safe to run game with incompatible modules installed
                    string incompatDescrip = incomp
                        .Select(m => $"{m.Module} ({m.Module.CompatibleGameVersions(CurrentInstance.game)})")
                        .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");
                    var result = SuppressableYesNoDialog(
                        string.Format(Properties.Resources.MainLaunchWithIncompatible,
                                      incompatDescrip),
                        string.Format(Properties.Resources.MainLaunchDontShow,
                                      CurrentInstance.game.ShortName,
                                      gv.WithoutBuild),
                        Properties.Resources.MainLaunch,
                        Properties.Resources.MainGoBack);
                    if (result.Item1 != DialogResult.Yes)
                    {
                        return;
                    }
                    else if (result.Item2)
                    {
                        CurrentInstance.AddSuppressedCompatWarningIdentifiers(
                            incomp.Select(m => m.identifier)
                                  .ToHashSet());
                    }
                }

                CurrentInstance.PlayGame(command,
                                         () =>
                                         {
                                             ManageMods.OnGameExit();
                                             UpdateStatusBar();
                                         });
            }
        }

        // This is used by Reinstall
        private void ManageMods_StartChangeSet(List<ModChange> changeset, Dictionary<GUIMod, string> conflicts)
        {
            UpdateChangesDialog(changeset,
                                conflicts?.ToDictionary(item => item.Key.ToCkanModule(),
                                                        item => item.Value));
            tabController.ShowTab(ChangesetTabPage.Name, 1);
        }

        private void RefreshModList(bool allowAutoUpdate, Dictionary<string, bool>? oldModules = null)
        {
            tabController.RenameTab(WaitTabPage.Name, Properties.Resources.MainModListWaitTitle);
            ShowWaitDialog();
            DisableMainWindow();
            ActiveModInfo = null;
            Wait.StartWaiting(
                ManageMods.Update,
                (sender, e) =>
                {
                    if (e != null)
                    {
                        if (allowAutoUpdate && e.Result is bool b && !b)
                        {
                            UpdateRepo();
                        }
                        else
                        {
                            UpdateTrayInfo();
                            HideWaitDialog();
                            EnableMainWindow();
                            SetupDefaultSearch();
                            if (focusIdent != null)
                            {
                                log.Debug("Attempting to select mod from startup parameters");
                                ManageMods.FocusMod(focusIdent, true, true);
                                // Only do it the first time
                                focusIdent = null;
                            }
                        }
                    }
                },
                false,
                oldModules);
        }

        [ForbidGUICalls]
        private void EnableMainWindow()
        {
            Util.Invoke(this, () =>
            {
                Enabled = true;
                MainMenu.Enabled = true;
                tabController.SetTabLock(false);
                /* Windows (7 & 8 only?) bug #1548 has extra facets.
                 * parent.childcontrol.Enabled = false seems to disable the parent,
                 * if childcontrol had focus. Depending on optimization steps,
                 * parent.childcontrol.Enabled = true does not necessarily
                 * re-enable the parent.*/
                Focus();
            });
        }

        [ForbidGUICalls]
        private void DisableMainWindow()
        {
            Util.Invoke(this, () =>
            {
                MainMenu.Enabled = false;
                tabController.SetTabLock(true);
            });
        }

    }
}
