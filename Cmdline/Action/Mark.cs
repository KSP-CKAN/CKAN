using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for setting flags on mods, currently the auto-installed flag.
    /// </summary>
    public class Mark : ISubCommand
    {
        private GameInstanceManager _manager;
        private IUser _user;

        /// <summary>
        /// Run the 'mark' command.
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
                case "MarkAuto":
                    exitCode = MarkAuto(args);
                    break;
                case "MarkUser":
                    exitCode = MarkUser(args);
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
                case "auto":
                case "user":
                    return $"{prefix} {args[0]} {args[1]} [options] <mod> [<mod2> ...]";
                default:
                    return $"{prefix} {args[0]} <command> [options]";
            }
        }

        private int MarkAuto(object args)
        {
            var opts = (MarkOptions.MarkAuto)args;
            if (!opts.Mods.Any())
            {
                _user.RaiseMessage("auto <mod> [<mod2> ...] - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            var exitCode = opts.Handle(_manager, _user);
            if (exitCode != Exit.Ok)
                return exitCode;

            var inst = MainClass.GetGameInstance(_manager);
            var regMgr = RegistryManager.Instance(inst);
            var needSave = false;
            Search.AdjustModulesCase(inst, opts.Mods.ToList());

            foreach (var id in opts.Mods)
            {
                var mod = regMgr.registry.InstalledModule(id);
                if (mod == null)
                {
                    _user.RaiseError("\"{0}\" is not installed.", id);
                }
                else if (mod.AutoInstalled)
                {
                    _user.RaiseError("\"{0}\" is already marked as auto-installed.", id);
                }
                else
                {
                    _user.RaiseMessage("Marking \"{0}\" as auto-installed...", id);
                    try
                    {
                        mod.AutoInstalled = true;
                        needSave = true;
                    }
                    catch (ModuleIsDLCKraken kraken)
                    {
                        _user.RaiseMessage("Can't mark expansion \"{0}\" as auto-installed.", kraken.module.name);
                        return Exit.BadOpt;
                    }
                }
            }

            if (needSave)
            {
                regMgr.Save(false);
                _user.RaiseMessage("Successfully applied changes.");
            }

            return Exit.Ok;
        }

        private int MarkUser(object args)
        {
            var opts = (MarkOptions.MarkUser)args;
            if (!opts.Mods.Any())
            {
                _user.RaiseMessage("user <mod> [<mod2> ...] - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            var exitCode = opts.Handle(_manager, _user);
            if (exitCode != Exit.Ok)
                return exitCode;

            var inst = MainClass.GetGameInstance(_manager);
            var regMgr = RegistryManager.Instance(inst);
            var needSave = false;
            Search.AdjustModulesCase(inst, opts.Mods.ToList());

            foreach (var id in opts.Mods)
            {
                var mod = regMgr.registry.InstalledModule(id);
                if (mod == null)
                {
                    _user.RaiseError("\"{0}\" is not installed.", id);
                }
                else if (!mod.AutoInstalled)
                {
                    _user.RaiseError("\"{0}\" is already marked as user-selected.", id);
                }
                else
                {
                    _user.RaiseMessage("Marking \"{0}\" as user-selected...", id);
                    try
                    {
                        mod.AutoInstalled = false;
                        needSave = true;
                    }
                    catch (ModuleIsDLCKraken kraken)
                    {
                        _user.RaiseMessage("Can't mark expansion \"{0}\" as auto-installed.", kraken.module.name);
                        return Exit.BadOpt;
                    }
                }
            }

            if (needSave)
            {
                regMgr.Save(false);
                _user.RaiseMessage("Successfully applied changes.");
            }

            return Exit.Ok;
        }
    }

    [Verb("mark", HelpText = "Edit flags on mods")]
    [ChildVerbs(typeof(MarkAuto), typeof(MarkUser))]
    internal class MarkOptions
    {
        [VerbExclude]
        [Verb("auto", HelpText = "Mark mods as auto installed")]
        internal class MarkAuto : InstanceSpecificOptions
        {
            [Value(0, MetaName = "Mod name(s)", HelpText = "The mod name(s) to mark as auto-installed")]
            public IEnumerable<string> Mods { get; set; }
        }

        [VerbExclude]
        [Verb("user", HelpText = "Mark mods as user selected (opposite of auto installed)")]
        internal class MarkUser : InstanceSpecificOptions
        {
            [Value(0, MetaName = "Mod name(s)", HelpText = "The mod name(s) to mark as user-installed")]
            public IEnumerable<string> Mods { get; set; }
        }
    }
}
