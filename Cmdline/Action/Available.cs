using System.Linq;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for listing the available mods.
    /// </summary>
    public class Available : ICommand
    {
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Available"/> class.
        /// </summary>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Available(IUser user)
        {
            _user = user;
        }

        /// <summary>
        /// Run the 'available' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (AvailableOptions)args;
            IRegistryQuerier registry = RegistryManager.Instance(inst).registry;

            var compatible = registry
                .CompatibleModules(inst.VersionCriteria())
                .Where(m => !m.IsDLC);

            _user.RaiseMessage("Mods compatible with {0} {1}\r\n", inst.game.ShortName, inst.Version());

            if (opts.Detail)
            {
                foreach (var module in compatible)
                {
                    _user.RaiseMessage("* {0} ({1}) - {2} - {3}", module.identifier, module.version, module.name, module.@abstract);
                }
            }
            else
            {
                foreach (var module in compatible)
                {
                    _user.RaiseMessage("* {0} ({1}) - {2}", module.identifier, module.version, module.name);
                }
            }

            return Exit.Ok;
        }
    }

    [Verb("available", HelpText = "List available mods")]
    internal class AvailableOptions : InstanceSpecificOptions
    {
        [Option("detail", HelpText = "Shows a short description of each mod")]
        public bool Detail { get; set; }
    }
}
