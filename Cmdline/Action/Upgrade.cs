using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using System.Diagnostics.CodeAnalysis;

using CommandLine;

using CKAN.IO;
using CKAN.Versioning;

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
                options.modules?.Add(CkanModule.FromFile(options.ckan_file).identifier);
            }

            if (options.modules?.Count == 0 && !options.upgrade_all)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                foreach (var h in Actions.GetHelp("upgrade"))
                {
                    user.RaiseError("{0}", h);
                }
                return Exit.BADOPT;
            }

            if (!options.upgrade_all
                //&& options.modules is ["ckan"]
                && options.modules != null
                && options.modules.Count > 0
                && options.modules[0] is "ckan"
                && AutoUpdate.CanUpdate)
            {
                return UpgradeCkan(options);
            }

            try
            {
                var regMgr = RegistryManager.Instance(instance, repoData);
                var registry = regMgr.registry;
                var deduper = new InstalledFilesDeduplicator(instance,
                                                             manager.Instances.Values,
                                                             repoData);
                if (options.upgrade_all)
                {
                    var to_upgrade = registry
                                     .CheckUpgradeable(instance, new HashSet<string>())
                                     [true];
                    if (to_upgrade.Count == 0)
                    {
                        user.RaiseMessage(Properties.Resources.UpgradeAllUpToDate);
                    }
                    else if (manager.Cache != null)
                    {
                        UpgradeModules(manager.Cache, options.NetUserAgent, user, instance, deduper, to_upgrade);
                    }
                }
                else
                {
                    if (options.modules != null && manager.Cache != null)
                    {
                        Search.AdjustModulesCase(instance, registry, options.modules);
                        UpgradeModules(manager.Cache, options.NetUserAgent, user, instance, deduper, options.modules);
                    }
                }
                user.RaiseMessage("");
            }
            catch (CancelledActionKraken k)
            {
                user.RaiseMessage(Properties.Resources.UpgradeAborted, k.Message);
                return Exit.ERROR;
            }
            catch (Kraken kraken)
            {
                user.RaiseMessage("{0}", kraken.Message);
                return Exit.ERROR;
            }
            catch (Exception exc)
            {
                user.RaiseMessage("{0}", exc.ToString());
                return Exit.ERROR;
            }

            return Exit.OK;
        }

        [ExcludeFromCodeCoverage]
        private int UpgradeCkan(UpgradeOptions options)
        {
            if (options.dev_build && options.stable_release)
            {
                user.RaiseMessage(Properties.Resources.UpgradeCannotCombineFlags);
                return Exit.BADOPT;
            }
            var config = manager.Configuration;
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
                var update = upd.GetUpdate(config.DevBuilds ?? false,
                                           options.NetUserAgent);
                if (update.Version is CkanModuleVersion latestVersion
                    && !latestVersion.SameClientVersion(Meta.ReleaseVersion))
                {
                    user.RaiseMessage(Properties.Resources.UpgradeNewCKANAvailable,
                                      latestVersion?.ToString() ?? "");
                    if (update.ReleaseNotes != null)
                    {
                        user.RaiseMessage("{0}", update.ReleaseNotes);
                    }
                    user.RaiseMessage("");
                    user.RaiseMessage("");

                    if (user.RaiseYesNoDialog(Properties.Resources.UpgradeProceed))
                    {
                        user.RaiseMessage(Properties.Resources.UpgradePleaseWait);
                        upd.StartUpdateProcess(false, options.NetUserAgent, config.DevBuilds ?? false, user);
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
                user.RaiseError(Properties.Resources.UpgradeFailed, exc.Message);
                return Exit.ERROR;
            }
        }

        /// <summary>
        /// Upgrade some modules by their CkanModules
        /// </summary>
        /// <param name="manager">Game instance manager to use</param>
        /// <param name="user">IUser object for output</param>
        /// <param name="instance">Game instance to use</param>
        /// <param name="modules">List of modules to upgrade</param>
        private void UpgradeModules(NetModuleCache             cache,
                                    string?                    userAgent,
                                    IUser                      user,
                                    CKAN.GameInstance          instance,
                                    InstalledFilesDeduplicator deduper,
                                    List<CkanModule>           modules)
        {
            UpgradeModules(
                cache, userAgent, user, instance, repoData,
                (ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string>? possibleConfigOnlyDirs) =>
                    installer.Upgrade(modules, downloader,
                                      ref possibleConfigOnlyDirs,
                                      regMgr, deduper, true, true),
                modules.Add);
        }

        /// <summary>
        /// Upgrade some modules by their identifier and (optional) version
        /// </summary>
        /// <param name="manager">Game instance manager to use</param>
        /// <param name="user">IUser object for output</param>
        /// <param name="instance">Game instance to use</param>
        /// <param name="identsAndVersions">List of identifier[=version] to upgrade</param>
        private void UpgradeModules(NetModuleCache             cache,
                                    string?                    userAgent,
                                    IUser                      user,
                                    CKAN.GameInstance          instance,
                                    InstalledFilesDeduplicator deduper,
                                    List<string>               identsAndVersions)
        {
            UpgradeModules(
                cache, userAgent, user, instance, repoData,
                (ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string>? possibleConfigOnlyDirs) =>
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
                                                                       () => registry.LatestAvailable(req, instance.StabilityToleranceConfig, crit))
                                                                   ?? registry.GetInstalledVersion(req))
                                                    .Concat(heldIdents.Select(ident => registry.GetInstalledVersion(ident)))
                                                    .OfType<CkanModule>()
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
                                     ?? (upgradeable.TryGetValue(request, out CkanModule? m)
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
                        installer.Upgrade(to_upgrade, downloader,
                                          ref possibleConfigOnlyDirs, regMgr, deduper, true);
                    }
                },
                m => identsAndVersions.Add(m.identifier));
        }

        private static string UpToFirst(string orig, char toFind)
            => UpTo(orig, orig.IndexOf(toFind));

        private static string UpTo(string orig, int pos)
            => pos >= 0 && pos < orig.Length ? orig[..pos]
                                             : orig;

        // Action<ref T> isn't allowed
        private delegate void AttemptUpgradeAction(ModuleInstaller installer, NetAsyncModulesDownloader downloader, RegistryManager regMgr, ref HashSet<string>? possibleConfigOnlyDirs);

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
        private void UpgradeModules(NetModuleCache        cache,
                                    string?               userAgent,
                                    IUser                 user,
                                    CKAN.GameInstance     instance,
                                    RepositoryDataManager repoData,
                                    AttemptUpgradeAction  attemptUpgradeCallback,
                                    Action<CkanModule>    addUserChoiceCallback)
        {
            using (TransactionScope transact = CkanTransaction.CreateTransactionScope()) {
                var installer  = new ModuleInstaller(instance, cache,
                                                     manager.Configuration, user);
                var downloader = new NetAsyncModulesDownloader(user, cache, userAgent);
                var regMgr     = RegistryManager.Instance(instance, repoData);
                HashSet<string>? possibleConfigOnlyDirs = null;
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
        public string? ckan_file { get; set; }

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
        public List<string>? modules { get; set; }
    }

}
