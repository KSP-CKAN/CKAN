using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;
using log4net;

using CKAN.Versioning;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram.DLC;

namespace CKAN.CmdLine
{
    [ExcludeFromCodeCoverage]
    internal class InstanceSubOptions : VerbCommandOptions
    {
        [VerbOption("list",    HelpText = "List game instances")]
        public ListInstancesOptions? ListOptions { get; set; }

        [VerbOption("add",     HelpText = "Add a game instance")]
        public AddOptions?     AddOptions     { get; set; }

        [VerbOption("clone",   HelpText = "Clone an existing game instance")]
        public CloneOptions?   CloneOptions   { get; set; }

        [VerbOption("rename",  HelpText = "Rename a game instance")]
        public RenameOptions?  RenameOptions  { get; set; }

        [VerbOption("forget",  HelpText = "Forget a game instance")]
        public ForgetOptions?  ForgetOptions  { get; set; }

        [VerbOption("default", HelpText = "Set the default game instance")]
        public DefaultOptions? DefaultOptions { get; set; }

        [VerbOption("fake",    HelpText = "Fake a game instance")]
        public FakeOptions?    FakeOptions    { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var ht = HelpText.AutoBuild(this, verb);
            foreach (var h in GetHelp(verb))
            {
                ht.AddPreOptionsLine(h);
            }
            return ht;
        }

        [ExcludeFromCodeCoverage]
        public static IEnumerable<string> GetHelp(string verb)
        {
            // Add a usage prefix line
            yield return " ";
            if (string.IsNullOrEmpty(verb))
            {
                yield return $"ckan instance - {Properties.Resources.InstanceHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan instance <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "instance " + verb + " - " + GetDescription(typeof(InstanceSubOptions), verb);
                switch (verb)
                {
                    // First the commands with three string arguments
                    case "fake":
                        yield return $"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] name path version [--game KSP|KSP2] [--MakingHistory <version>] [--BreakingGround <version>]";
                        break;

                    case "clone":
                        yield return $"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] instanceNameOrPath newname newpath";
                        break;

                    // Second the commands with two string arguments
                    case "add":
                        yield return $"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] name url";
                        break;
                    case "rename":
                        yield return $"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] oldname newname";
                        break;

                    // Now the commands with one string argument
                    case "remove":
                    case "forget":
                    case "use":
                    case "default":
                        yield return $"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] name";
                        break;

                    // Now the commands with only --flag type options
                    case "list":
                    default:
                        yield return $"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}]";
                        break;

                }
            }
        }
    }

    internal class ListInstancesOptions : CommonOptions { }

    internal class AddOptions : CommonOptions
    {
        [ValueOption(0)] public string? name { get; set; }
        [ValueOption(1)] public string? path { get; set; }
    }

    internal class CloneOptions : CommonOptions
    {
        [Option("share-stock", DefaultValue = false, HelpText = "Use junction points (Windows) or symbolic links (Unix) for stock dirs instead of copying")]
        public bool shareStock { get; set; }

        [ValueOption(0)] public string? nameOrPath { get; set; }
        [ValueOption(1)] public string? new_name { get; set; }
        [ValueOption(2)] public string? new_path { get; set; }
    }

    internal class RenameOptions : CommonOptions
    {
        [GameInstances]
        [ValueOption(0)] public string? old_name { get; set; }
        [ValueOption(1)] public string? new_name { get; set; }
    }

    internal class ForgetOptions : CommonOptions
    {
        [GameInstances]
        [ValueOption(0)] public string? name { get; set; }
    }

    internal class DefaultOptions : CommonOptions
    {
        [GameInstances]
        [ValueOption(0)] public string? name { get; set; }
    }

    internal class FakeOptions : CommonOptions
    {
        [ValueOption(0)] public string? name { get; set; }
        [ValueOption(1)] public string? path { get; set; }
        [ValueOption(2)] public string? version { get; set; }

        [Option("game", DefaultValue = "KSP", HelpText = "The game of the instance to be faked, either KSP or KSP2")]
        public string? gameId { get; set; }

        [Option("MakingHistory", DefaultValue = "none", HelpText = "The version of the Making History DLC to be faked.")]
        public string? makingHistoryVersion { get; set; }
        [Option("BreakingGround", DefaultValue = "none", HelpText = "The version of the Breaking Ground DLC to be faked.")]
        public string? breakingGroundVersion { get; set; }

        [Option("set-default", DefaultValue = false, HelpText = "Set the new instance as the default one.")]
        public bool setDefault { get; set; }
    }

    public class GameInstance : ISubCommand
    {
        public GameInstance(GameInstanceManager manager,
                            IUser               user)
        {
            Manager = manager;
            this.user = user;
        }

        // This is required by ISubCommand
        public int RunSubCommand(CommonOptions?    opts,
                                 SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();

            #region Aliases

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "use":
                        args[i] = "default";
                        break;

                    default:
                        break;
                }
            }

            #endregion

            int exitCode = Exit.OK;
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new InstanceSubOptions(), (option, suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    exitCode = options.Handle(Manager, user);
                    if (exitCode == Exit.OK)
                    {
                        exitCode = suboptions switch
                        {
                            ListInstancesOptions => ListInstances(),
                            AddOptions     opts  => AddInstance(opts),
                            CloneOptions   opts  => CloneInstance(opts),
                            RenameOptions  opts  => RenameInstance(opts),
                            ForgetOptions  opts  => ForgetInstance(opts),
                            DefaultOptions opts  => SetDefaultInstance(opts),
                            FakeOptions    opts  => FakeNewGameInstance(opts),
                            // The parser makes this case impossible,
                            // but the compiler doesn't know that
                            _                    => UnknownCommand(option),
                        };
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private readonly IUser               user;
        private readonly GameInstanceManager Manager;

        #region option functions

        private int ListInstances()
        {
            var output = Manager.Instances.Values
                .OrderByDescending(i => i.Name == Manager.Configuration.AutoStartInstance)
                .ThenByDescending(i => i.Game.FirstReleaseDate)
                .ThenByDescending(i => i.Version() ?? GameVersion.Any)
                .ThenBy(i => i.Name)
                .Select(i => new Tuple<string, string, string, string, string>(
                                i.Name,
                                i.Game.ShortName,
                                i.Version()?.ToString() ?? Properties.Resources.InstanceListNoVersion,
                                i.Name == Manager.Configuration.AutoStartInstance
                                    ? Properties.Resources.InstanceListYes
                                    : Properties.Resources.InstanceListNo,
                                Platform.FormatPath(i.GameDir)))
                .ToList();

            if (output != null && user != null)
            {
                var nameHeader    = Properties.Resources.InstanceListNameHeader;
                var gameHeader    = Properties.Resources.InstanceListGameHeader;
                var versionHeader = Properties.Resources.InstanceListVersionHeader;
                var defaultHeader = Properties.Resources.InstanceListDefaultHeader;
                var pathHeader    = Properties.Resources.InstanceListPathHeader;

                var nameWidth    = output.Select(i => i.Item1).Append(nameHeader).Max(i => i.Length);
                var gameWidth    = output.Select(i => i.Item2).Append(gameHeader).Max(i => i.Length);
                var versionWidth = output.Select(i => i.Item3).Append(versionHeader).Max(i => i.Length);
                var defaultWidth = output.Select(i => i.Item4).Append(defaultHeader).Max(i => i.Length);
                var pathWidth    = output.Select(i => i.Item5).Append(pathHeader).Max(i => i.Length);

                var columnFormat = string.Join("  ", $"{{0,-{nameWidth}}}",
                                                     $"{{1,-{gameWidth}}}",
                                                     $"{{2,-{versionWidth}}}",
                                                     $"{{3,-{defaultWidth}}}",
                                                     $"{{4,-{pathWidth}}}");

                user.RaiseMessage(columnFormat,
                                  nameHeader, gameHeader, versionHeader, defaultHeader, pathHeader);

                user.RaiseMessage(columnFormat, new string('-', nameWidth),
                                                new string('-', gameWidth),
                                                new string('-', versionWidth),
                                                new string('-', defaultWidth),
                                                new string('-', pathWidth));

                foreach (var line in output)
                {
                    user.RaiseMessage(columnFormat, line.Item1, line.Item2, line.Item3, line.Item4, line.Item5);
                }

                return Exit.OK;
            }
            return Exit.ERROR;
        }

        private int AddInstance(AddOptions options)
        {
            if (options.name == null || options.path == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("add");
                return Exit.BADOPT;
            }

            if (Manager.HasInstance(options.name))
            {
                user.RaiseError(Properties.Resources.InstanceAddDuplicate, options.name);
                return Exit.BADOPT;
            }

            try
            {
                if (user != null)
                {
                    string path = options.path;
                    Manager.AddInstance(path, options.name, user);
                    user.RaiseMessage(Properties.Resources.InstanceAdded, options.name, options.path);
                }
                return Exit.OK;
            }
            catch (NotGameDirKraken ex)
            {
                user.RaiseError(Properties.Resources.InstanceNotInstance, ex.path);
                return Exit.BADOPT;
            }
        }

        private int CloneInstance(CloneOptions options)
        {
            if (options.nameOrPath == null || options.new_name == null || options.new_path == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("clone");
                return Exit.BADOPT;
            }

            // Parse all options
            string instanceNameOrPath = options.nameOrPath;
            string newName = options.new_name;
            string newPath = options.new_path;

            log.Info("Cloning the game instance: " + options.nameOrPath);

            try
            {
                // Try instanceNameOrPath as name and search the registry for it.
                if (Manager.HasInstance(instanceNameOrPath))
                {
                    CKAN.GameInstance[] listOfInstances = Manager.Instances.Values.ToArray();
                    foreach (CKAN.GameInstance instance in listOfInstances)
                    {
                        if (instance.Name == instanceNameOrPath)
                        {
                            // Found it, now clone it.
                            Manager.CloneInstance(instance, newName, newPath, options.shareStock);
                            break;
                        }
                    }
                }
                // Try to use instanceNameOrPath as a path and create a new game instance.
                // If it's valid, go on.
                else if (Manager.InstanceAt(instanceNameOrPath) is CKAN.GameInstance instance && instance.Valid)
                {
                    Manager.CloneInstance(instance, newName, newPath, options.shareStock);
                }
                // There is no instance with this name or at this path.
                else
                {
                    user.RaiseError(Properties.Resources.InstanceCloneNotFound, instanceNameOrPath);
                    ListInstances();
                    return Exit.ERROR;
                }
            }
            catch (Kraken k)
            {
                user.RaiseError("{0}", k.Message);
                return Exit.ERROR;
            }
            catch (Exception e)
            {
                // Something went wrong copying the files. Contains a message.
                log.Error(e);
                return Exit.ERROR;
            }

            // Test if the instance was added to the registry.
            // No need to test if valid, because this is done in AddInstance(),
            // so if something went wrong, HasInstance is false.
            if (Manager.HasInstance(newName))
            {
                return Exit.OK;
            }
            else
            {
                user.RaiseError(Properties.Resources.InstanceCloneFailed);
                return Exit.ERROR;
            }
        }

        private int RenameInstance(RenameOptions options)
        {
            if (options.old_name == null || options.new_name == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("rename");
                return Exit.BADOPT;
            }

            if (!Manager.HasInstance(options.old_name))
            {
                user.RaiseError(Properties.Resources.InstanceNotFound, options.old_name);
                return Exit.BADOPT;
            }

            if (Manager.HasInstance(options.new_name))
            {
                user.RaiseError(Properties.Resources.InstanceAddDuplicate, options.old_name);
                return Exit.BADOPT;
            }

            Manager.RenameInstance(options.old_name, options.new_name);

            user.RaiseMessage(Properties.Resources.InstanceRenamed, options.old_name, options.new_name);
            return Exit.OK;
        }

        private int ForgetInstance(ForgetOptions options)
        {
            if (options.name == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("forget");
                return Exit.BADOPT;
            }

            if (!Manager.HasInstance(options.name))
            {
                user.RaiseError(Properties.Resources.InstanceNotFound, options.name);
                return Exit.BADOPT;
            }

            Manager.RemoveInstance(options.name);

            user.RaiseMessage(Properties.Resources.InstanceForgot, options.name);
            return Exit.OK;
        }

        private int SetDefaultInstance(DefaultOptions options)
        {
            var name = options.name;
            // Oh right, this is that one that didn't work AT ALL at one point
            if (name == null)
            {
                // No input argument from the user. Present a list of the possible instances.

                try
                {
                    // If there is a default instance, its index is passed first
                    var defaultInstance = Manager.Configuration.AutoStartInstance;
                    int result = user.RaiseSelectionDialog(
                                     $"default <name> - {Properties.Resources.InstanceDefaultArgumentMissing}",
                                     (defaultInstance == null || string.IsNullOrWhiteSpace(defaultInstance)
                                         ? Enumerable.Empty<object>()
                                         : Enumerable.Repeat((object)Manager.Instances.IndexOfKey(defaultInstance), 1))
                                         .Concat(Manager.Instances.Select(kvp => string.Format("\"{0}\" - {1}",
                                                                                               kvp.Key, kvp.Value.GameDir)))
                                         .ToArray());
                    if (result < 0)
                    {
                        return Exit.BADOPT;
                    }
                    name = Manager.Instances.ElementAt(result).Key;
                }
                catch (Kraken)
                {
                    return Exit.BADOPT;
                }
            }

            if (name != null)
            {
                if (!Manager.Instances.ContainsKey(name))
                {
                    user.RaiseError(Properties.Resources.InstanceNotFound, name);
                    return Exit.BADOPT;
                }

                try
                {
                    Manager.SetAutoStart(name);
                }
                catch (NotGameDirKraken k)
                {
                    user.RaiseError(Properties.Resources.InstanceNotInstance, k.path);
                    return Exit.BADOPT;
                }

                user.RaiseMessage(Properties.Resources.InstanceDefaultSet, name);
                return Exit.OK;
            }
            return Exit.BADOPT;
        }

        /// <summary>
        /// Creates a new fake game instance after the conditions CKAN tests for valid game directories.
        /// Used for developing and testing purposes.
        /// </summary>
        private int FakeNewGameInstance(FakeOptions options)
        {
            int error()
            {
                log.Debug("Instance faking failed, see console output for details.");
                return Exit.ERROR;
            }

            int badArgument()
            {
                log.Debug("Instance faking failed: bad argument(s). See console output for details.");
                user.RaiseError(Properties.Resources.InstanceFakeBadArguments);
                return Exit.BADOPT;
            }


            if (options.name == null || options.path == null || options.version == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("fake");
                return Exit.BADOPT;
            }

            log.Debug("Parsing arguments...");
            // Parse all options
            string instanceName = options.name;
            string path = options.path;
            GameVersion? version;
            bool setDefault = options.setDefault;
            var game = KnownGames.GameByShortName(options.gameId ?? "");
            if (game == null)
            {
                user.RaiseError(Properties.Resources.InstanceFakeBadGame, options.gameId ?? "");
                return badArgument();
            }

            // options.<DLC>Version is "none" if the DLC should not be simulated.
            var dlcs = new Dictionary<DLC.IDlcDetector, GameVersion>();
            if (options.makingHistoryVersion != null && options.makingHistoryVersion.ToLower() != "none")
            {
                if (GameVersion.TryParse(options.makingHistoryVersion, out GameVersion? ver))
                {
                    dlcs.Add(new MakingHistoryDlcDetector(), ver);
                }
                else
                {
                    user.RaiseError(Properties.Resources.InstanceFakeMakingHistory);
                    return badArgument();
                }
            }
            if (options.breakingGroundVersion != null && options.breakingGroundVersion.ToLower() != "none")
            {
                if (GameVersion.TryParse(options.breakingGroundVersion, out GameVersion? ver))
                {
                    dlcs.Add(new BreakingGroundDlcDetector(), ver);
                }
                else
                {
                    user.RaiseError(Properties.Resources.InstanceFakeBreakingGround);
                    return badArgument();
                }
            }

            // Parse the choosen game version
            try
            {
                version = GameVersion.Parse(options.version);
            }
            catch (FormatException)
            {
                // Thrown if there is anything besides numbers and points in the version string or a different syntactic error.
                user.RaiseError(Properties.Resources.InstanceFakeVersion);
                return badArgument();
            }

            // Get the full version including build number.
            try
            {
                version = version.RaiseVersionSelectionDialog(game, user);
            }
            catch (BadGameVersionKraken)
            {
                user.RaiseError(Properties.Resources.InstanceFakeBadGameVersion);
                return badArgument();
            }
            catch (CancelledActionKraken)
            {
                user.RaiseError(Properties.Resources.InstanceFakeCancelled);
                return error();
            }

            if (version == null)
            {
                return error();
            }

            user.RaiseMessage(Properties.Resources.InstanceFakeCreating,
                              instanceName, path, version.ToString() ?? "");
            log.Debug("Faking instance...");

            try
            {
                // Pass all arguments to CKAN.GameInstanceManager.FakeInstance() and create a new one.
                Manager.FakeInstance(game, instanceName, path, version, dlcs);
                if (setDefault)
                {
                    user.RaiseMessage(Properties.Resources.InstanceFakeDefault);
                    Manager.SetAutoStart(instanceName);
                }
            }
            catch (InstanceNameTakenKraken kraken)
            {
                user.RaiseError(Properties.Resources.InstanceDuplicate, kraken.instName);
                return badArgument();
            }
            catch (BadInstallLocationKraken kraken)
            {
                // The folder exists and is not empty.
                user.RaiseError("{0}", kraken.Message);
                return badArgument();
            }
            catch (WrongGameVersionKraken kraken)
            {
                // Thrown because the specified game instance is too old for one of the selected DLCs.
                user.RaiseError("{0}", kraken.Message);
                return badArgument();
            }
            catch (NotGameDirKraken kraken)
            {
                user.RaiseError(Properties.Resources.InstanceNotInstance, kraken.path);
                return error();
            }
            catch (InvalidGameInstanceKraken)
            {
                // Thrown by Manager.SetAutoStart() if Manager.HasInstance returns false.
                // Will be checked again down below with a proper error message
            }

            // Test if the instance was added to the registry.
            // No need to test if valid, because this is done in AddInstance().
            if (Manager.HasInstance(instanceName))
            {
                user.RaiseMessage(Properties.Resources.InstanceFakeDone);
                return Exit.OK;
            }
            else
            {
                user.RaiseError(Properties.Resources.InstanceFakeFailed);
                return error();
            }
        }

        [ExcludeFromCodeCoverage]
        private int UnknownCommand(string option)
        {
            user.RaiseError("{0}: instance {1}",
                            Properties.Resources.UnknownCommand, option);
            return Exit.BADOPT;
        }

        #endregion

        [ExcludeFromCodeCoverage]
        private void PrintUsage(string verb)
        {
            foreach (var h in InstanceSubOptions.GetHelp(verb))
            {
                user.RaiseError("{0}", h);
            }
        }

        protected static readonly ILog log = LogManager.GetLogger(typeof(GameInstance));
    }
}
