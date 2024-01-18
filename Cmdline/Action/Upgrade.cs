using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;

using Autofac;
using log4net;

using CKAN.Versioning;
using CKAN.Configuration;

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
                // What? No files specified?
                user.RaiseMessage("{0}: ckan upgrade Mod [Mod2, ...]", Properties.Resources.Usage);
                user.RaiseMessage("  or   ckan upgrade --all");
                if (AutoUpdate.CanUpdate)
                {
                    user.RaiseMessage("  or   ckan upgrade ckan [--stable-release|--dev-build]");
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
                    var to_upgrade = new List<CkanModule>();

                    foreach (KeyValuePair<string, ModuleVersion> mod in registry.Installed(false))
                    {
                        try
                        {
                            // Check if upgrades are available
                            var latest = registry.LatestAvailable(mod.Key, instance.VersionCriteria());

                            // This may be an unindexed mod. If so,
                            // skip rather than crash. See KSP-CKAN/CKAN#841.
                            if (latest == null || latest.IsDLC)
                            {
                                continue;
                            }

                            if (latest.version.IsGreaterThan(mod.Value) || registry.HasUpdate(mod.Key, instance.VersionCriteria()))
                            {
                                // Upgradable
                                log.InfoFormat("New version {0} found for {1}",
                                    latest.version, latest.identifier);
                                to_upgrade.Add(latest);
                            }

                        }
                        catch (ModuleNotFoundKraken)
                        {
                            log.InfoFormat("{0} is installed, but no longer in the registry",
                                mod.Key);
                        }
                    }
                    UpgradeModules(manager, user, instance, true, to_upgrade);
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
                                    bool                ConfirmPrompt,
                                    List<CkanModule>    modules)
        {
            UpgradeModules(
                manager, user, instance, repoData,
                (ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string> possibleConfigOnlyDirs) =>
                    installer.Upgrade(modules, downloader,
                        ref possibleConfigOnlyDirs, regMgr, true, true, ConfirmPrompt),
                m => modules.Add(m)
            );
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
                    installer.Upgrade(
                        identsAndVersions.Select(arg => CkanModule.FromIDandVersion(
                                                            regMgr.registry, arg,
                                                            instance.VersionCriteria()))
                                         .ToList(),
                        downloader,
                        ref possibleConfigOnlyDirs,
                        regMgr,
                        true),
                m => identsAndVersions.Add(m.identifier)
            );
        }

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

        private static readonly ILog log = LogManager.GetLogger(typeof(Upgrade));
    }
}
