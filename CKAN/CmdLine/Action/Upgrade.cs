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

            var to_upgrade = new List<CkanModule> ();

            if (options.upgrade_all)
            {
                var installed = new Dictionary<string, Version>(ksp.Registry.Installed());

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
            }
            else
            {
                foreach (string mod in options.modules)
                {
                    Match match = Regex.Match(mod, @"^(?<mod>[^=]*)=(?<version>.*)$");

                    if (match.Success)
                    {
                        string ident = match.Groups["mod"].Value;
                        string version = match.Groups["version"].Value;

                        CkanModule module = ksp.Registry.GetModuleByVersion(ident, version);

                        if (module == null)
                        {
                            User.RaiseMessage("Cannot install {0}, version {1} not available", ident, version);
                            return Exit.ERROR;
                        }

                        to_upgrade.Add(module);
                    }
                    else
                    {
                        to_upgrade.Add(
                            ksp.Registry.LatestAvailable(mod, ksp.Version())
                        );
                    }
                }
            }


            var auto_detected_modules = to_upgrade.Where(module => ksp.Registry.IsAutodetected(module.identifier)).ToList();
            foreach (var module in auto_detected_modules.Select(mod => mod.identifier))
            {
                User.RaiseMessage("Cannot upgrade {0} as it was not installed by CKAN. \n Please remove manually before trying again", module);
            }
            //Exit if any are auto_detected
            if (auto_detected_modules.Any()) return Exit.ERROR;   

            User.RaiseMessage("\nUpgrading modules...\n");
            // TODO: These instances all need to go.
            ModuleInstaller.GetInstance(ksp, User).Upgrade(to_upgrade,new NetAsyncDownloader(User));
            User.RaiseMessage("\nDone!\n");
            return Exit.OK;

        }
    }
}

