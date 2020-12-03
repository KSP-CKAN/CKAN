using System.Linq;
using System.Collections.Generic;

namespace CKAN.CmdLine
{
    public class Available : ICommand
    {
        public IUser user { get; set; }

        public Available(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.GameInstance ksp, object raw_options)
        {
            AvailableOptions opts       = (AvailableOptions)raw_options;
            IRegistryQuerier registry   = RegistryManager.Instance(ksp).registry;
            
            var compatible = registry
                .CompatibleModules(ksp.VersionCriteria())
                .Where(m => !m.IsDLC);

            user.RaiseMessage("Modules compatible with KSP {0}", ksp.Version());
            user.RaiseMessage("");

            if (opts.detail)
            {
                foreach (CkanModule module in compatible)
                {
                    user.RaiseMessage("* {0} ({1}) - {2} - {3}", module.identifier, module.version, module.name, module.@abstract);
                }
            }
            else
            {
                foreach (CkanModule module in compatible)
                {
                    user.RaiseMessage("* {0} ({1}) - {2}", module.identifier, module.version, module.name);
                }
            }

            return Exit.OK;
        }
    }
}
