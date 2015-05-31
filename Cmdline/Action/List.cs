﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class List : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(List));

        public IUser user { get; set; }

        public List(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            ListOptions options = (ListOptions) raw_options;

            Registry registry = RegistryManager.Instance(ksp).registry;

            if (!(options.porcelain))
            {
                user.RaiseMessage("\nKSP found at {0}\n", ksp.GameDir());
                user.RaiseMessage("KSP Version: {0}\n", ksp.Version());

                user.RaiseMessage("Installed Modules:\n");
            }

            var installed = new SortedDictionary<string, Version>(registry.Installed());

            foreach (KeyValuePair<string, Version> mod in installed)
            {
                Version current_version = mod.Value;

                string bullet = "*";

                if (current_version is ProvidesVersion)
                {
                    // Skip virtuals for now.
                    continue;
                }
                else if (current_version is DllVersion)
                {
                    // Autodetected dll
                    bullet = "-";
                }
                else
                {
                    try
                    {
                        // Check if upgrades are available, and show appropriately.
                        CkanModule latest = registry.LatestAvailable(mod.Key, ksp.Version());

                        log.InfoFormat("Latest {0} is {1}", mod.Key, latest);

                        if (latest == null)
                        {
                            // Not compatible!
                            bullet = "X";
                        }
                        else if (latest.version.IsEqualTo(current_version))
                        {
                            // Up to date
                            bullet = "-";
                        }
                        else if (latest.version.IsGreaterThan(mod.Value))
                        {
                            // Upgradable
                            bullet = "^";
                        }
                    }
                    catch (ModuleNotFoundKraken)
                    {
                        log.InfoFormat("{0} is installed, but no longer in the registry", mod.Key);
                        bullet = "?";
                    }
                }

                user.RaiseMessage("{0} {1} {2}", bullet, mod.Key, mod.Value);
            }

            if (!(options.porcelain))
            {
                user.RaiseMessage("\nLegend: -: Up to date. X: Incompatible. ^: Upgradable. ?: Unknown ");
            }

            return Exit.OK;
        }
    }
}

