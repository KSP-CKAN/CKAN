using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CKAN.Exporters;
using CKAN.Types;
using log4net;

namespace CKAN.CmdLine
{
    public class Available : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(List));

        public IUser user { get; set; }

        public Available(IUser user)
        {
            this.user = user;
        }

        private int ConsoleWidth()
        {
            try
            {
                var width = user.WindowWidth;
                return width;
            }
            catch (Kraken)
            {
                var width = 80;
                log.InfoFormat("Could not find console width, defaulting to {0}", width);
                return width;
            }
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            IRegistryQuerier registry = RegistryManager.Instance(ksp).registry;

            var width = ConsoleWidth();

            List<CkanModule> available = registry.Available(ksp.VersionCriteria());

            user.RaiseMessage("Mods available for KSP {0}", ksp.Version());
            user.RaiseMessage("");

            foreach (CkanModule module in available)
            {
                    string entry = String.Format("* {0} ({1}) - {2}", module.identifier, module.version, module.name);
                    user.RaiseMessage(width > 0 ? entry.PadRight(width).Substring(0, width - 1) : entry);
            }

            return Exit.OK;
        }
    }
}

