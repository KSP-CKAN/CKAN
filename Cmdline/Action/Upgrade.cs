using System;
using System.Linq;
﻿using System.Collections.Generic;
using log4net;
using CKAN.Versioning;

namespace CKAN.CmdLine
{
    public class Upgrade : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Upgrade));

        public IUser User { get; set; }
        private GameInstanceManager manager;

        /// <summary>
        /// Initialize the upgrade command object
        /// </summary>
        /// <param name="mgr">GameInstanceManager containing our instances</param>
        /// <param name="user">IUser object for interaction</param>
        public Upgrade(GameInstanceManager mgr, IUser user)
        {
            manager   = mgr;
            User = user;
        }

        /// <summary>
        /// Upgrade an installed module
        /// </summary>
        /// <param name="ksp">Game instance from which to remove</param>
        /// <param name="raw_options">Command line options object</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunCommand(CKAN.GameInstance ksp, object raw_options)
        {
            UpgradeOptions options = (UpgradeOptions) raw_options;

            if (options.ckan_file != null)
            {                
                options.modules.Add(MainClass.LoadCkanFromFile(ksp, options.ckan_file).identifier);
            }

            if (options.modules.Count == 0 && !options.upgrade_all)
            {
                // What? No files specified?
                User.RaiseMessage("Usage: ckan upgrade Mod [Mod2, ...]");
                User.RaiseMessage("  or   ckan upgrade --all");
                if (AutoUpdate.CanUpdate)
                {
                    User.RaiseMessage("  or   ckan upgrade ckan");
                }
                return Exit.BADOPT;
            }

            if (!options.upgrade_all && options.modules[0] == "ckan" && AutoUpdate.CanUpdate)
            {
                User.RaiseMessage("Querying the latest CKAN version");
                AutoUpdate.Instance.FetchLatestReleaseInfo();
                var latestVersion = AutoUpdate.Instance.latestUpdate.Version;
                var currentVersion = new ModuleVersion(Meta.GetVersion(VersionFormat.Short));

                if (latestVersion.IsGreaterThan(currentVersion))
                {
                    User.RaiseMessage("New CKAN version available - " + latestVersion);
                    var releaseNotes = AutoUpdate.Instance.latestUpdate.ReleaseNotes;
                    User.RaiseMessage(releaseNotes);
                    User.RaiseMessage("\r\n");

                    if (User.RaiseYesNoDialog("Proceed with install?"))
                    {
                        User.RaiseMessage("Upgrading CKAN, please wait..");
                        AutoUpdate.Instance.StartUpdateProcess(false);
                    }
                }
                else
                {
                    User.RaiseMessage("You already have the latest version.");
                }

                return Exit.OK;
            }

            try
            {
                HashSet<string> possibleConfigOnlyDirs = null;
                var regMgr = RegistryManager.Instance(ksp);
                var registry = regMgr.registry;
                if (options.upgrade_all)
                {
                    var to_upgrade = new List<CkanModule>();

                    foreach (KeyValuePair<string, ModuleVersion> mod in registry.Installed(false))
                    {
                        try
                        {
                            // Check if upgrades are available
                            var latest = registry.LatestAvailable(mod.Key, ksp.VersionCriteria());

                            // This may be an unindexed mod. If so,
                            // skip rather than crash. See KSP-CKAN/CKAN#841.
                            if (latest == null || latest.IsDLC)
                            {
                                continue;
                            }

                            if (latest.version.IsGreaterThan(mod.Value))
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

                    ModuleInstaller.GetInstance(ksp, manager.Cache, User).Upgrade(to_upgrade, new NetAsyncModulesDownloader(User, manager.Cache), ref possibleConfigOnlyDirs, regMgr, true, true);
                }
                else
                {
                    Search.AdjustModulesCase(ksp, options.modules);
                    ModuleInstaller.GetInstance(ksp, manager.Cache, User).Upgrade(options.modules, new NetAsyncModulesDownloader(User, manager.Cache), ref possibleConfigOnlyDirs, regMgr);
                }
                User.RaiseMessage("");
            }
            catch (CancelledActionKraken k)
            {
                User.RaiseMessage("Upgrade aborted: {0}", k.Message);
                return Exit.ERROR;
            }
            catch (ModuleNotFoundKraken kraken)
            {
                User.RaiseMessage("Module {0} not found", kraken.module);
                return Exit.ERROR;
            }
            catch (InconsistentKraken kraken)
            {
                User.RaiseMessage(kraken.ToString());
                return Exit.ERROR;
            }
            catch (ModuleIsDLCKraken kraken)
            {
                User.RaiseMessage($"CKAN can't upgrade expansion '{kraken.module.name}' for you.");
                var res = kraken?.module?.resources;
                var storePagesMsg = new Uri[] { res?.store, res?.steamstore }
                    .Where(u => u != null)
                    .Aggregate("", (a, b) => $"{a}\r\n- {b}");
                if (!string.IsNullOrEmpty(storePagesMsg))
                {
                    User.RaiseMessage($"To upgrade this expansion, download any updates from the store page from which you purchased it:\r\n{storePagesMsg}");
                }
                return Exit.ERROR;
            }

            return Exit.OK;
        }
    }
}
