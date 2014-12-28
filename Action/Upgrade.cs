using System.Collections.Generic;
using System.Linq;
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
                options.modules.Add(MainClass.LoadCkanFromFile(ksp, options.ckan_file).identifier);
            }

            if (options.modules.Count == 0)
            {
                // What? No files specified?
                User.RaiseMessage("Usage: ckan upgrade Mod [Mod2, ...]");
                return Exit.BADOPT;
            }

            User.RaiseMessage("\nUpgrading modules...\n");
            // TODO: These instances all need to go.
            try
            {
                ModuleInstaller.GetInstance(ksp, User).Upgrade(options.modules, new NetAsyncDownloader(User));
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

