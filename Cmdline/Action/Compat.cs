using System.Linq;
using CKAN.Versioning;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing KSP version compatibility.
    /// </summary>
    public class Compat : ISubCommand
    {
        private GameInstanceManager _manager;
        private IUser _user;

        /// <summary>
        /// Run the 'compat' command.
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
                case "AddCompat":
                    exitCode = AddCompatibility(args);
                    break;
                case "ForgetCompat":
                    exitCode = ForgetCompatibility(args);
                    break;
                case "ListCompat":
                    exitCode = ListCompatibility();
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
                case "add":
                case "forget":
                    return $"{prefix} {args[0]} {args[1]} [options] <version>";
                case "list":
                    return $"{prefix} {args[0]} {args[1]} [options]";
                default:
                    return $"{prefix} {args[0]} <command> [options]";
            }
        }

        private int AddCompatibility(object args)
        {
            var opts = (CompatOptions.AddCompat)args;
            if (opts.Version == null)
            {
                _user.RaiseMessage("add <version> - argument missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            var inst = MainClass.GetGameInstance(_manager);
            if (GameVersion.TryParse(opts.Version, out GameVersion gameVersion))
            {
                var newCompatibleVersion = inst.GetCompatibleVersions();
                newCompatibleVersion.Add(gameVersion);
                inst.SetCompatibleVersions(newCompatibleVersion);
                _user.RaiseMessage("Successfully added \"{0}\".", gameVersion);
            }
            else
            {
                _user.RaiseError("Invalid KSP version.");
                return Exit.Error;
            }

            return Exit.Ok;
        }

        private int ForgetCompatibility(object args)
        {
            var opts = (CompatOptions.ForgetCompat)args;
            if (opts.Version == null)
            {
                _user.RaiseMessage("forget <version> - argument missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            var inst = MainClass.GetGameInstance(_manager);
            if (GameVersion.TryParse(opts.Version, out GameVersion gameVersion))
            {
                if (gameVersion != inst.Version())
                {
                    var newCompatibleVersion = inst.GetCompatibleVersions();
                    newCompatibleVersion.RemoveAll(i => i == gameVersion);
                    inst.SetCompatibleVersions(newCompatibleVersion);
                    _user.RaiseMessage("Successfully removed \"{0}\".", gameVersion);
                }
                else
                {
                    _user.RaiseError("Cannot forget actual KSP version.");
                    return Exit.Error;
                }
            }
            else
            {
                _user.RaiseError("Invalid KSP version.");
                return Exit.Error;
            }

            return Exit.Ok;
        }

        private int ListCompatibility()
        {
            const string versionHeader = "Version";
            const string actualHeader = "Actual";

            var inst = MainClass.GetGameInstance(_manager);

            var output = inst
                .GetCompatibleVersions()
                .Select(i => new
                {
                    Version = i,
                    Actual = false
                })
                .ToList();

            output.Add(new
            {
                Version = inst.Version(),
                Actual = true
            });

            output = output
                .OrderByDescending(i => i.Actual)
                .ThenByDescending(i => i.Version)
                .ToList();

            var versionWidth = Enumerable
                .Repeat(versionHeader, 1)
                .Concat(output.Select(i => i.Version.ToString()))
                .Max(i => i.Length);

            var actualWidth = Enumerable
                .Repeat(actualHeader, 1)
                .Concat(output.Select(i => i.Actual.ToString()))
                .Max(i => i.Length);

            _user.RaiseMessage("{0}  {1}",
                versionHeader.PadRight(versionWidth),
                actualHeader.PadRight(actualWidth)
            );

            _user.RaiseMessage("{0}  {1}",
                new string('-', versionWidth),
                new string('-', actualWidth)
            );

            foreach (var line in output)
            {
                _user.RaiseMessage("{0}  {1}",
                    line.Version.ToString().PadRight(versionWidth),
                    line.Actual.ToString().PadRight(actualWidth)
                );
            }

            return Exit.Ok;
        }
    }

    [Verb("compat", HelpText = "Manage KSP version compatibility")]
    [ChildVerbs(typeof(AddCompat), typeof(ForgetCompat), typeof(ListCompat))]
    internal class CompatOptions
    {
        [VerbExclude]
        [Verb("add", HelpText = "Add version to KSP compatibility list")]
        internal class AddCompat : InstanceSpecificOptions
        {
            [Value(0, MetaName = "KSP version", HelpText = "The KSP version to add as compatible")]
            public string Version { get; set; }
        }

        [VerbExclude]
        [Verb("forget", HelpText = "Forget version on KSP compatibility list")]
        internal class ForgetCompat : InstanceSpecificOptions
        {
            [Value(0, MetaName = "KSP version", HelpText = "The KSP version to remove as compatible")]
            public string Version { get; set; }
        }

        [VerbExclude]
        [Verb("list", HelpText = "List compatible KSP versions")]
        internal class ListCompat : InstanceSpecificOptions { }
    }
}
