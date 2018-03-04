using System.Collections.Generic;
using log4net;

namespace CKAN.CmdLine
{
    public class Upgrade : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Upgrade));

        public IUser User { get; set; }

        public Upgrade(IUser user)
        {
            User = user;
        }


        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            UpgradeOptions options = (UpgradeOptions) raw_options;

            if (options.ckan_file != null)
            {
                options.modules.Add(LoadCkanFromFile(ksp, options.ckan_file).identifier);
            }

            if (options.modules.Count == 0 && ! options.upgrade_all)
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
                var latestVersion = AutoUpdate.Instance.LatestVersion;
                var currentVersion = new Version(Meta.GetVersion(VersionFormat.Short));

                if (latestVersion.IsGreaterThan(currentVersion))
                {
                    User.RaiseMessage("New CKAN version available - " + latestVersion);
                    var releaseNotes = AutoUpdate.Instance.ReleaseNotes;
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

            User.RaiseMessage("\r\nUpgrading modules...\r\n");

            try
            {
                if (options.upgrade_all)
                {
                    var registry = RegistryManager.Instance(ksp).registry;
                    var installed = new Dictionary<string, Version>(registry.Installed());
                    var to_upgrade = new List<CkanModule>();

                    foreach (KeyValuePair<string, Version> mod in installed)
                    {
                        Version current_version = mod.Value;

                        if ((current_version is ProvidesVersion) || (current_version is DllVersion))
                        {
                            continue;
                        }
                        else
                        {
                            try
                            {
                                // Check if upgrades are available
                                var latest = registry.LatestAvailable(mod.Key, ksp.VersionCriteria());

                                // This may be an unindexed mod. If so,
                                // skip rather than crash. See KSP-CKAN/CKAN#841.
                                if (latest == null)
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

                    }

                    ModuleInstaller.GetInstance(ksp, User).Upgrade(to_upgrade, new NetAsyncModulesDownloader(User));
                }
                else
                {
                    // TODO: These instances all need to go.
                    Search.AdjustModulesCase(ksp, options.modules);
                    ModuleInstaller.GetInstance(ksp, User).Upgrade(options.modules, new NetAsyncModulesDownloader(User));
                }
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
            User.RaiseMessage("\r\nDone!\r\n");

            return Exit.OK;
        }

        internal static CkanModule LoadCkanFromFile(CKAN.KSP current_instance, string ckan_file)
        {
            CkanModule module = CkanModule.FromFile(ckan_file);

            // We'll need to make some registry changes to do this.
            RegistryManager registry_manager = RegistryManager.Instance(current_instance);

            // Remove this version of the module in the registry, if it exists.
            registry_manager.registry.RemoveAvailable(module);

            // Sneakily add our version in...
            registry_manager.registry.AddAvailable(module);

            return module;
        }
    }
}
