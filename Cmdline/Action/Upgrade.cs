using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;

using CommandLine;
using Autofac;

using CKAN.Versioning;
using CKAN.Configuration;
using CKAN.Extensions;

namespace CKAN.CmdLine
{
    public class Upgrade : ICommand
    {
        /// <summary>
        /// Initialize the upgrade command object
        /// </summary>
        /// <param name="mgr">GameInstanceManager containing our instances</param>
        /// <param name="user">IUser object for interaction</param>
        public Upgrade(GameInstanceManager mgr, RepositoryDataManager repoData, IUser user)
        {
            manager       = mgr;
            this.repoData = repoData;
            this.user     = user;
        }

        /// <summary>
        /// Upgrade an installed module
        /// </summary>
        /// <param name="instance">Game instance from which to remove</param>
        /// <param name="raw_options">Command line options object</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunCommand(CKAN.GameInstance instance, object raw_options)
        {
            UpgradeOptions options = (UpgradeOptions) raw_options;

            if (options.ckan_file != null)
            {
                options.modules.Add(MainClass.LoadCkanFromFile(options.ckan_file).identifier);
            }

            if (options.modules.Count == 0 && !options.upgrade_all)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                foreach (var h in Actions.GetHelp("upgrade"))
                {
                    user.RaiseError(h);
                }
                return Exit.BADOPT;
            }

            if (!options.upgrade_all && options.modules[0] == "ckan" && AutoUpdate.CanUpdate)
            {
                if (options.dev_build && options.stable_release)
                {
                    user.RaiseMessage(Properties.Resources.UpgradeCannotCombineFlags);
                    return Exit.BADOPT;
                }
                var config = ServiceLocator.Container.Resolve<IConfiguration>();
                var devBuild = options.dev_build
                               || (!options.stable_release && (config.DevBuilds ?? false));
                if (devBuild != config.DevBuilds)
                {
                    if (!config.DevBuilds.HasValue && devBuild && Platform.IsWindows)
                    {
                        // Tell Windows users about malware scanner's false positives
                        // and how to disable it, if they feel safe doing it
                        Utilities.ProcessStartURL(HelpURLs.WindowsDevBuilds);
                    }
                    config.DevBuilds = devBuild;
                    user.RaiseMessage(
                        config.DevBuilds ?? false
                            ? Properties.Resources.UpgradeSwitchingToDevBuilds
                            : Properties.Resources.UpgradeSwitchingToStableReleases);
                }

                user.RaiseMessage(Properties.Resources.UpgradeQueryingCKAN);
                try
                {
                    var upd = new AutoUpdate();
                    var update = upd.GetUpdate(config.DevBuilds ?? false);
                    var latestVersion = update.Version;
                    var currentVersion = new ModuleVersion(Meta.GetVersion());

                    if (!latestVersion.Equals(currentVersion))
                    {
                        user.RaiseMessage(Properties.Resources.UpgradeNewCKANAvailable, latestVersion);
                        var releaseNotes = update.ReleaseNotes;
                        user.RaiseMessage(releaseNotes);
                        user.RaiseMessage("");
                        user.RaiseMessage("");

                        if (user.RaiseYesNoDialog(Properties.Resources.UpgradeProceed))
                        {
                            user.RaiseMessage(Properties.Resources.UpgradePleaseWait);
                            upd.StartUpdateProcess(false, config.DevBuilds ?? false, user);
                        }
                    }
                    else
                    {
                        user.RaiseMessage(Properties.Resources.UpgradeAlreadyHaveLatest);
                    }
                    return Exit.OK;
                }
                catch (Exception exc)
                {
                    user.RaiseError("Upgrade failed: {0}", exc.Message);
                    return Exit.ERROR;
                }
            }

            try
            {
                var regMgr = RegistryManager.Instance(instance, repoData);
                var registry = regMgr.registry;
                if (options.upgrade_all)
                {
                    var to_upgrade = registry
                                     .CheckUpgradeable(instance, new HashSet<string>())
                                     [true];
                    if (to_upgrade.Count == 0)
                    {
                        user.RaiseMessage(Properties.Resources.UpgradeAllUpToDate);
                    }
                    else
                    {
                        UpgradeModules(manager, user, instance, to_upgrade);
                    }
                }
                else
                {
                    Search.AdjustModulesCase(instance, registry, options.modules);
                    UpgradeModules(manager, user, instance, options.modules);
                }
                user.RaiseMessage("");
            }
            catch (CancelledActionKraken k)
            {
                user.RaiseMessage(Properties.Resources.UpgradeAborted, k.Message);
                return Exit.ERROR;
            }
            catch (ModuleNotFoundKraken kraken)
            {
                user.RaiseMessage(Properties.Resources.UpgradeNotFound, $"{kraken.module} {kraken.version}");
                return Exit.ERROR;
            }
            catch (InconsistentKraken kraken)
            {
                user.RaiseMessage(kraken.ToString());
                return Exit.ERROR;
            }
            catch (ModuleIsDLCKraken kraken)
            {
                user.RaiseMessage(Properties.Resources.UpgradeDLC, kraken.module.name);
                var res = kraken?.module?.resources;
                var storePagesMsg = new Uri[] { res?.store, res?.steamstore }
                    .Where(u => u != null)
                    .Aggregate("", (a, b) => $"{a}\r\n- {b}");
                if (!string.IsNullOrEmpty(storePagesMsg))
                {
                    user.RaiseMessage(Properties.Resources.UpgradeDLCStorePage, storePagesMsg);
                }
                return Exit.ERROR;
            }

            return Exit.OK;
        }

        /// <summary>
        /// Upgrade some modules by their CkanModules
        /// </summary>
        /// <param name="manager">Game instance manager to use</param>
        /// <param name="user">IUser object for output</param>
        /// <param name="instance">Game instance to use</param>
        /// <param name="modules">List of modules to upgrade</param>
        private void UpgradeModules(GameInstanceManager manager,
                                    IUser               user,
                                    CKAN.GameInstance   instance,
                                    List<CkanModule>    modules)
        {
            UpgradeModules(
                manager, user, instance, repoData,
                (ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string> possibleConfigOnlyDirs) =>
                    installer.Upgrade(modules, downloader,
                                      ref possibleConfigOnlyDirs,
                                      regMgr, true, true, true),
                m => modules.Add(m));
        }

        /// <summary>
        /// Upgrade some modules by their identifier and (optional) version
        /// </summary>
        /// <param name="manager">Game instance manager to use</param>
        /// <param name="user">IUser object for output</param>
        /// <param name="instance">Game instance to use</param>
        /// <param name="identsAndVersions">List of identifier[=version] to upgrade</param>
        private void UpgradeModules(GameInstanceManager manager,
                                    IUser               user,
                                    CKAN.GameInstance   instance,
                                    List<string>        identsAndVersions)
        {
            UpgradeModules(
                manager, user, instance, repoData,
                (ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string> possibleConfigOnlyDirs) =>
                {
                    var crit     = instance.VersionCriteria();
                    var registry = regMgr.registry;
                    // Installed modules we're NOT upgrading
                    var heldIdents = registry.Installed(false)
                                             .Keys
                                             .Except(identsAndVersions.Select(arg => UpToFirst(arg, '=')))
                                             .ToHashSet();
                    // The modules we'll have after upgrading as aggressively as possible
                    var limiters = identsAndVersions.Select(req => CkanModule.FromIDandVersion(registry, req, crit)
                                                                   ?? Utilities.DefaultIfThrows(
                                                                       () => registry.LatestAvailable(req, crit))
                                                                   ?? registry.GetInstalledVersion(req))
                                                    .Concat(heldIdents.Select(ident => registry.GetInstalledVersion(ident)))
                                                    .Where(m => m != null)
                                                    .ToList();
                    // Modules allowed by THOSE modules' relationships
                    var upgradeable = registry
                                      .CheckUpgradeable(instance, heldIdents, limiters)
                                      [true]
                                      .ToDictionary(m => m.identifier,
                                                    m => m);
                    // Substitute back in the ident=ver requested versions
                    var to_upgrade = new List<CkanModule>();
                    foreach (var request in identsAndVersions)
                    {
                        var module = CkanModule.FromIDandVersion(registry, request, crit)
                                     ?? (upgradeable.TryGetValue(request, out CkanModule m)
                                        ? m
                                        : null);
                        if (module == null)
                        {
                            user.RaiseMessage(Properties.Resources.UpgradeAlreadyUpToDate, request);
                        }
                        else
                        {
                            to_upgrade.Add(module);
                        }
                    }
                    if (to_upgrade.Count > 0)
                    {
                        installer.Upgrade(to_upgrade, downloader, ref possibleConfigOnlyDirs, regMgr, true);
                    }
                },
                m => identsAndVersions.Add(m.identifier));
        }

        private static string UpToFirst(string orig, char toFind)
            => UpTo(orig, orig.IndexOf(toFind));

        private static string UpTo(string orig, int pos)
            => pos >= 0 && pos < orig.Length ? orig.Substring(0, pos)
                                             : orig;

        // Action<ref T> isn't allowed
        private delegate void AttemptUpgradeAction(ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string> possibleConfigOnlyDirs);

        /// <summary>
        /// The core of the module upgrading logic, with callbacks to
        /// support different input formats managed by the calling code.
        /// Handles transactions, creating commonly required objects,
        /// looping logic, prompting for TooManyModsProvideKraken resolution.
        /// </summary>
        /// <param name="manager">Game instance manager to use</param>
        /// <param name="user">IUser object for output</param>
        /// <param name="instance">Game instance to use</param>
        /// <param name="attemptUpgradeCallback">Function to call to try to perform the actual upgrade, may throw TooManyModsProvideKraken</param>
        /// <param name="addUserChoiceCallback">Function to call when the user has requested a new module added to the change set in response to TooManyModsProvideKraken</param>
        private void UpgradeModules(GameInstanceManager   manager,
                                    IUser                 user,
                                    CKAN.GameInstance     instance,
                                    RepositoryDataManager repoData,
                                    AttemptUpgradeAction  attemptUpgradeCallback,
                                    Action<CkanModule>    addUserChoiceCallback)
        {
            using (TransactionScope transact = CkanTransaction.CreateTransactionScope()) {
                var installer  = new ModuleInstaller(instance, manager.Cache, user);
                var downloader = new NetAsyncModulesDownloader(user, manager.Cache);
                var regMgr     = RegistryManager.Instance(instance, repoData);
                HashSet<string> possibleConfigOnlyDirs = null;
                bool done = false;
                while (!done)
                {
                    try
                    {
                        attemptUpgradeCallback?.Invoke(installer, downloader, regMgr, ref possibleConfigOnlyDirs);
                        transact.Complete();
                        done = true;
                    }
                    catch (TooManyModsProvideKraken k)
                    {
                        int choice = user.RaiseSelectionDialog(
                            k.Message,
                            k.modules.Select(m => $"{m.identifier} ({m.name})").ToArray());
                        if (choice < 0)
                        {
                            return;
                        }
                        else
                        {
                            addUserChoiceCallback?.Invoke(k.modules[choice]);
                        }
                    }
                }
            }
        }

        private readonly IUser                 user;
        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
    }

    internal class UpgradeOptions : InstanceSpecificOptions
    {
        [Option('c', "ckanfile", HelpText = "Local CKAN file to process")]
        public string ckan_file { get; set; }

        [Option("no-recommends", DefaultValue = false, HelpText = "Do not install recommended modules")]
        public bool no_recommends { get; set; }

        [Option("with-suggests", DefaultValue = false, HelpText = "Install suggested modules")]
        public bool with_suggests { get; set; }

        [Option("with-all-suggests", DefaultValue = false, HelpText = "Install suggested modules all the way down")]
        public bool with_all_suggests { get; set; }

        [Option("all", DefaultValue = false, HelpText = "Upgrade all available updated modules")]
        public bool upgrade_all { get; set; }

        [Option("dev-build", DefaultValue = false,
                HelpText = "For `ckan` option only, use dev builds")]
        public bool dev_build { get; set; }

        [Option("stable-release", DefaultValue = false,
                HelpText = "For `ckan` option only, use stable releases")]
        public bool stable_release { get; set; }

        [ValueList(typeof (List<string>))]
        [InstalledIdentifiers]
        public List<string> modules { get; set; }
    }

}
