using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class Remove : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Remove));

        public IUser user { get; set; }

        public Remove(IUser user)
        {
            this.user = user;
        }

        // Uninstalls a module, if it exists.
        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            RemoveOptions options = (RemoveOptions) raw_options;

            if (options.modules != null && options.modules.Count > 0)
            {
                try
                {
                    var installer = ModuleInstaller.GetInstance(ksp, user);
                    installer.UninstallList(options.modules);
                }
                catch (ModNotInstalledKraken kraken)
                {
                    user.RaiseMessage("I can't do that, {0} isn't installed.", kraken.mod);
                    user.RaiseMessage("Try `ckan list` for a list of installed mods.");
                    return Exit.BADOPT;
                }
            }
            else
            {
                user.RaiseMessage("No mod selected, nothing to do");
                return Exit.BADOPT;
            }

            return Exit.OK;
        }
    }
}

