﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CKAN.CmdLine
{
    public class Upgrade : ICommand
    {
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
                User.RaiseMessage("\nUnsupported option at this time.");
                return Exit.BADOPT;
            }

            if (options.modules.Count == 0)
            {
                // What? No files specified?
                User.RaiseMessage("Usage: ckan upgrade Mod [Mod2, ...]");
                return Exit.BADOPT;
            }

            var to_upgrade = new List<CkanModule> ();

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


            User.RaiseMessage("\nUpgrading modules...\n");
            // TODO: These instances all need to go.
            ModuleInstaller.GetInstance(ksp, User).Upgrade(to_upgrade,new NetAsyncDownloader(User));
            User.RaiseMessage("\nDone!\n");
            return Exit.OK;

        }
    }
}

