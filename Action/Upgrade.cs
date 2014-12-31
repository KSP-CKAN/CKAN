using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                options.modules.Add(MainClass.LoadCkanFromFile(ksp, options.ckan_file).identifier);
            }

            if (options.modules.Count == 0 && ! options.upgrade_all)
            {
                // What? No files specified?
                User.RaiseMessage("Usage: ckan upgrade Mod [Mod2, ...]");
                User.RaiseMessage("  or   ckan upgrade --all");
                return Exit.BADOPT;
            }

            User.RaiseMessage("\nUpgrading modules...\n");

            try
            {
                if (options.upgrade_all)
                {
                    var installed = new Dictionary<string, Version>(ksp.Registry.Installed());
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
                                CkanModule latest = ksp.Registry.LatestAvailable(mod.Key, ksp.Version());

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

                    ModuleInstaller.GetInstance(ksp, User).Upgrade(to_upgrade, new NetAsyncDownloader(User));
                }
                else
                {
                    // TODO: These instances all need to go.
                    ModuleInstaller.GetInstance(ksp, User).Upgrade(options.modules, new NetAsyncDownloader(User));
                }
            }
            catch (ModuleNotFoundKraken kraken)
            {
                User.RaiseMessage(kraken.Message);
                return Exit.ERROR;
            }
            User.RaiseMessage("\nDone!\n");
            return Exit.OK;

        }
    }
}

