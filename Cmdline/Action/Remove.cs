using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using log4net;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing the removal of mods.
    /// </summary>
    public class Remove : ICommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Remove));

        private readonly GameInstanceManager _manager;
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Remove"/> class.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Remove(GameInstanceManager manager, IUser user)
        {
            _manager = manager;
            _user = user;
        }

        /// <summary>
        /// Run the 'remove' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (RemoveOptions)args;
            if (!opts.Mods.Any())
            {
                _user.RaiseMessage("remove <mod> [<mod2> ...] - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            var regMgr = RegistryManager.Instance(inst);

            // Use one (or more!) regex to select the modules to remove
            if (opts.Regex)
            {
                Log.Debug("Attempting Regex...");

                // Parse every "module" as a grumpy regex
                var justins = opts.Mods.Select(s => new Regex(s));

                // Modules that have been selected by one regex
                var selectedModules = new List<string>();

                // Get the list of installed modules
                // Try every regex on every installed module:
                // if it matches, select for removal
                foreach (var mod in regMgr.registry.InstalledModules.Select(mod => mod.identifier))
                {
                    if (justins.Any(re => re.IsMatch(mod)))
                    {
                        selectedModules.Add(mod);
                    }
                }

                // Replace the regular expressions with the selected modules
                // and continue removal as usual
                opts.Mods = selectedModules;
            }

            if (opts.All)
            {
                Log.Debug("Removing all mods...");

                // Add the list of installed modules to the list that should be uninstalled
                opts.Mods.ToList().AddRange(
                    regMgr.registry.InstalledModules
                        .Where(mod => !mod.Module.IsDLC)
                        .Select(mod => mod.identifier)
                );
            }

            try
            {
                HashSet<string> possibleConfigOnlyDirs = null;
                var installer = new ModuleInstaller(inst, _manager.Cache, _user);
                Search.AdjustModulesCase(inst, opts.Mods.ToList());
                installer.UninstallList(opts.Mods, ref possibleConfigOnlyDirs, regMgr);
                _user.RaiseMessage("");
            }
            catch (ModNotInstalledKraken kraken)
            {
                _user.RaiseMessage("Can't remove \"{0}\" because it isn't installed.\r\nTry 'ckan list' for a list of installed mods.", kraken.mod);
                return Exit.Error;
            }
            catch (ModuleIsDLCKraken kraken)
            {
                _user.RaiseMessage("Can't remove the expansion \"{0}\".", kraken.module.name);
                var res = kraken?.module?.resources;
                var storePagesMsg = new[] { res?.store, res?.steamstore }
                    .Where(u => u != null)
                    .Aggregate("", (a, b) => $"{a}\r\n- {b}");

                if (!string.IsNullOrEmpty(storePagesMsg))
                {
                    _user.RaiseMessage("To remove this expansion, follow the instructions for the store page from which you purchased it:\r\n   {0}", storePagesMsg);
                }

                return Exit.Error;
            }
            catch (CancelledActionKraken kraken)
            {
                _user.RaiseMessage("Remove aborted: {0}", kraken.Message);
                return Exit.Error;
            }

            _user.RaiseMessage("Successfully removed requested mods.");
            return Exit.Ok;
        }
    }

    [Verb("remove", HelpText = "Remove an installed mod")]
    internal class RemoveOptions : InstanceSpecificOptions
    {
        [Option("re", HelpText = "Parse arguments as regular expressions")]
        public bool Regex { get; set; }

        [Option("all", HelpText = "Remove all installed mods")]
        public bool All { get; set; }

        [Value(0, MetaName = "Mod name(s)", HelpText = "The mod name(s) to remove")]
        public IEnumerable<string> Mods { get; set; }
    }
}
