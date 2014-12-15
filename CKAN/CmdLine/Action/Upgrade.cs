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
                    to_upgrade.Add(ParseModNameAndVersion(ksp, mod));
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

        /// <summary>
        /// Tries to parse an identifier in the format Modname=x.x.x
        /// If the module cannot be found in the registry, throws a ModuleNotFoundKraken.
        /// </summary>
        private CkanModule ParseModNameAndVersion(CKAN.KSP ksp, string mod)
        {
            Match match = Regex.Match(mod, @"^(?<mod>[^=]*)=(?<version>.*)$");

            if (match.Success)
            {
                string ident = match.Groups["mod"].Value;
                string version = match.Groups["version"].Value;

                CkanModule module = ksp.Registry.GetModuleByVersion(ident, version);

                if (module == null)                
                    throw new ModuleNotFoundKraken(string.Format("Cannot install {0}, version {1} not available", ident, version));                
                else
                    return module;
            }
            else
            {
                return ksp.Registry.LatestAvailable(mod, ksp.Version());
            }
        }
    }
}

