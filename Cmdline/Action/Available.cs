using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using CKAN.Exporters;
//using CKAN.Types;
//using log4net;

namespace CKAN.CmdLine
{
    public class Available : ICommand
    {
        public IUser user { get; set; }

        public Available(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            IRegistryQuerier registry = RegistryManager.Instance(ksp).registry;

            List<CkanModule> available = registry.Available(ksp.VersionCriteria());

            user.RaiseMessage("Mods available for KSP {0}", ksp.Version());
            user.RaiseMessage("");

            foreach (CkanModule module in available)
            {
                user.RaiseMessage(String.Format("* {0} ({1}) - {2}", module.identifier, module.version, module.name));
            }

            return Exit.OK;
        }
    }
}

