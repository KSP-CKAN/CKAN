using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CKAN.CmdLine
{
    public class Upgrade : ICommand
    {
        public Upgrade()
        {
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            UpgradeOptions options = (UpgradeOptions) raw_options;

            if (options.ckan_file != null)
            {
                User.WriteLine("\nUnsupported option at this time.");
                return Exit.BADOPT;
            }

            if (options.modules.Count == 0)
            {
                // What? No files specified?
                User.WriteLine("Usage: ckan upgrade Mod [Mod2, ...]");
                return Exit.BADOPT;
            }

            var to_upgrade = new List<CkanModule> ();

            foreach (string mod in options.modules)
            {
                try
                {
                    to_upgrade.Add(CkanModule.FromIDandVersion(ksp, mod));
                }
                catch (ModuleNotFoundKraken k)
                {
                    User.displayError(k.Message);
                    return Exit.ERROR;
                }
            }

            User.WriteLine("\nUpgrading modules...\n");

            // TODO: These instances all need to go.
            ModuleInstaller.Instance.Upgrade(to_upgrade);

            User.WriteLine("\nDone!\n");

            return Exit.OK;

        }
    }
}

