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
        private GameInstanceManager manager;

        /// <summary>
        /// Initialize the remove command object
        /// </summary>
        /// <param name="mgr">GameInstanceManager containing our instances</param>
        /// <param name="user">IUser object for interaction</param>
        public Remove(GameInstanceManager mgr, IUser user)
        {
            manager   = mgr;
            this.user = user;
        }

        /// <summary>
        /// Uninstalls a module, if it exists.
        /// </summary>
        /// <param name="ksp">Game instance from which to remove</param>
        /// <param name="raw_options">Command line options object</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunCommand(CKAN.GameInstance ksp, object raw_options)
        {
            RemoveOptions options = (RemoveOptions) raw_options;
            RegistryManager regMgr = RegistryManager.Instance(ksp);

            // Use one (or more!) regex to select the modules to remove
            if (options.regex)
            {
                log.Debug("Attempting Regex");
                // Parse every "module" as a grumpy regex
                var justins = options.modules.Select(s => new Regex(s));

                // Modules that have been selected by one regex
                List<string> selectedModules = new List<string>();

                // Get the list of installed modules
                // Try every regex on every installed module:
                // if it matches, select for removal
                foreach (string mod in regMgr.registry.InstalledModules.Select(mod => mod.identifier))
                {
                    if (justins.Any(re => re.IsMatch(mod)))
                        selectedModules.Add(mod);
                }

                // Replace the regular expressions with the selected modules
                // and continue removal as usual
                options.modules = selectedModules;
            }

            if (options.rmall)
            {
                log.Debug("Removing all mods");
                // Add the list of installed modules to the list that should be uninstalled
                options.modules.AddRange(
                    regMgr.registry.InstalledModules
                        .Where(mod => !mod.Module.IsDLC)
                        .Select(mod => mod.identifier)
                );
            }

            if (options.modules != null && options.modules.Count > 0)
            {
                try
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    var installer = ModuleInstaller.GetInstance(ksp, manager.Cache, user);
                    Search.AdjustModulesCase(ksp, options.modules);
                    installer.UninstallList(options.modules, ref possibleConfigOnlyDirs, regMgr);
                    user.RaiseMessage("");
                }
                catch (ModNotInstalledKraken kraken)
                {
                    user.RaiseMessage("I can't do that, {0} isn't installed.", kraken.mod);
                    user.RaiseMessage("Try `ckan list` for a list of installed mods.");
                    return Exit.BADOPT;
                }
                catch (ModuleIsDLCKraken kraken)
                {
                    user.RaiseMessage($"CKAN can't remove expansion '{kraken.module.name}' for you.");
                    var res = kraken?.module?.resources;
                    var storePagesMsg = new Uri[] { res?.store, res?.steamstore }
                        .Where(u => u != null)
                        .Aggregate("", (a, b) => $"{a}\r\n- {b}");
                    if (!string.IsNullOrEmpty(storePagesMsg))
                    {
                        user.RaiseMessage($"To remove this expansion, follow the instructions for the store page from which you purchased it:\r\n{storePagesMsg}");
                    }
                    return Exit.BADOPT;
                }
                catch (CancelledActionKraken k)
                {
                    user.RaiseMessage("Remove aborted: {0}", k.Message);
                    return Exit.ERROR;
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
