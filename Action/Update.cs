using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class Update : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Update));

        public IUser user { get; set; }

        public Update(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            UpdateOptions options = (UpdateOptions) raw_options;

            RegistryManager registry_manager = RegistryManager.Instance(ksp);

            Registry registry = registry_manager.registry;

            user.RaiseMessage("Downloading updates...");

            try
            {
                if (options.update_all)
                {
                    int updated = CKAN.Repo.UpdateAllRepositories(registry_manager, ksp, user);
                    user.RaiseMessage("Updated information on {0} available modules", updated);
                }
                else
                {
                    int updated = CKAN.Repo.Update(registry_manager, ksp, user, true, options.repo);
                    user.RaiseMessage("Updated information on {0} available modules", updated);
                }
            }
            catch (MissingCertificateKraken kraken)
            {
                // Handling the kraken means we have prettier output.
                user.RaiseMessage(kraken.ToString());
                return Exit.ERROR;
            }

            return Exit.OK;
        }
    }
}

