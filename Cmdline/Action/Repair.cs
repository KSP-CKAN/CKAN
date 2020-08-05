using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing repair tasks.
    /// </summary>
    public class Repair : ISubCommand
    {
        private GameInstanceManager _manager;
        private IUser _user;

        /// <summary>
        /// Run the 'repair' command.
        /// </summary>
        /// <inheritdoc cref="ISubCommand.RunCommand"/>
        public int RunCommand(GameInstanceManager manager, object args)
        {
            var s = args.ToString();
            var opts = s.Replace(s.Substring(0, s.LastIndexOf('.') + 1), "").Split('+');

            CommonOptions options = new CommonOptions();
            _user = new ConsoleUser(options.Headless);
            _manager = manager ?? new GameInstanceManager(_user);
            var exitCode = options.Handle(_manager, _user);

            if (exitCode != Exit.Ok)
                return exitCode;

            switch (opts[1])
            {
                case "RepairRegistry":
                    exitCode = Registry(MainClass.GetGameInstance(_manager));
                    break;
                default:
                    exitCode = Exit.BadOpt;
                    break;
            }

            return exitCode;
        }

        /// <inheritdoc cref="ISubCommand.GetUsage"/>
        public string GetUsage(string prefix, string[] args)
        {
            if (args.Length == 1)
                return $"{prefix} {args[0]} <command> [options]";

            switch (args[1])
            {
                case "registry":
                    return $"{prefix} {args[0]} {args[1]} [options]";
                default:
                    return $"{prefix} {args[0]} <command> [options]";
            }
        }

        private int Registry(CKAN.GameInstance inst)
        {
            var manager = RegistryManager.Instance(inst);
            manager.registry.Repair();
            manager.Save();
            _user.RaiseMessage("Attempted various registry repairs. Hope it helped.");
            return Exit.Ok;
        }
    }

    [Verb("repair", HelpText = "Attempt various automatic repairs")]
    [ChildVerbs(typeof(RepairRegistry))]
    internal class RepairOptions
    {
        [VerbExclude]
        [Verb("registry", HelpText = "Try to repair the CKAN registry")]
        internal class RepairRegistry : InstanceSpecificOptions { }
    }
}
