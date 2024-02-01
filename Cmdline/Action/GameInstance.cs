using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CommandLine;
using CommandLine.Text;
using log4net;

using CKAN.Versioning;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram.DLC;

namespace CKAN.CmdLine
{
    internal class InstanceSubOptions : VerbCommandOptions
    {
        [VerbOption("list",    HelpText = "List game instances")]
        public CommonOptions  ListOptions    { get; set; }

        [VerbOption("add",     HelpText = "Add a game instance")]
        public AddOptions     AddOptions     { get; set; }

        [VerbOption("clone",   HelpText = "Clone an existing game instance")]
        public CloneOptions   CloneOptions   { get; set; }

        [VerbOption("rename",  HelpText = "Rename a game instance")]
        public RenameOptions  RenameOptions  { get; set; }

        [VerbOption("forget",  HelpText = "Forget a game instance")]
        public ForgetOptions  ForgetOptions  { get; set; }

        [VerbOption("default", HelpText = "Set the default game instance")]
        public DefaultOptions DefaultOptions { get; set; }

        [VerbOption("fake",    HelpText = "Fake a game instance")]
        public FakeOptions    FakeOptions    { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            HelpText ht = HelpText.AutoBuild(this, verb);
            // Add a usage prefix line
            ht.AddPreOptionsLine(" ");
            if (string.IsNullOrEmpty(verb))
            {
                ht.AddPreOptionsLine($"ckan instance - {Properties.Resources.InstanceHelpSummary}");
                ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan instance <{Properties.Resources.Command}> [{Properties.Resources.Options}]");
            }
            else
            {
                ht.AddPreOptionsLine("instance " + verb + " - " + GetDescription(verb));
                switch (verb)
                {
                    // First the commands with three string arguments
                    case "fake":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] name path version [--game KSP|KSP2] [--MakingHistory <version>] [--BreakingGround <version>]");
                        break;

                    case "clone":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] instanceNameOrPath newname newpath");
                        break;

                    // Second the commands with two string arguments
                    case "add":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] name url");
                        break;
                    case "rename":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] oldname newname");
                        break;

                    // Now the commands with one string argument
                    case "remove":
                    case "forget":
                    case "use":
                    case "default":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}] name");
                        break;

                    // Now the commands with only --flag type options
                    case "list":
                    default:
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan instance {verb} [{Properties.Resources.Options}]");
                        break;

                }
            }
            return ht;
        }
    }

    internal class AddOptions : CommonOptions
    {
        [ValueOption(0)] public string name { get; set; }
        [ValueOption(1)] public string path { get; set; }
    }

    internal class CloneOptions : CommonOptions
    {
        [ValueOption(0)] public string nameOrPath { get; set; }
        [ValueOption(1)] public string new_name { get; set; }
        [ValueOption(2)] public string new_path { get; set; }
    }

    internal class RenameOptions : CommonOptions
    {
        [GameInstances]
        [ValueOption(0)] public string old_name { get; set; }
        [ValueOption(1)] public string new_name { get; set; }
    }

    internal class ForgetOptions : CommonOptions
    {
        [GameInstances]
        [ValueOption(0)] public string name { get; set; }
    }

    internal class DefaultOptions : CommonOptions
    {
        [GameInstances]
        [ValueOption(0)] public string name { get; set; }
    }

    internal class FakeOptions : CommonOptions
    {
        [ValueOption(0)] public string name { get; set; }
        [ValueOption(1)] public string path { get; set; }
        [ValueOption(2)] public string version { get; set; }

        [Option("game", DefaultValue = "KSP", HelpText = "The game of the instance to be faked, either KSP or KSP2")]
        public string gameId { get; set; }

        [Option("MakingHistory", DefaultValue = "none", HelpText = "The version of the Making History DLC to be faked.")]
        public string makingHistoryVersion { get; set; }
        [Option("BreakingGround", DefaultValue = "none", HelpText = "The version of the Breaking Ground DLC to be faked.")]
        public string breakingGroundVersion { get; set; }

        [Option("set-default", DefaultValue = false, HelpText = "Set the new instance as the default one.")]
        public bool setDefault { get; set; }
    }

    public class GameInstance : ISubCommand
    {
        public GameInstance() { }
        protected static readonly ILog log = LogManager.GetLogger(typeof(GameInstance));

        // This is required by ISubCommand
        public int RunSubCommand(GameInstanceManager manager, CommonOptions opts, SubCommandOptions unparsed)
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
            Parser.Default.ParseArgumentsStrict(args, new InstanceSubOptions(), (string option, object suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    User     = new ConsoleUser(options.Headless);
                    Manager  = manager ?? new GameInstanceManager(User);
                    exitCode = options.Handle(Manager, User);
                    if (exitCode != Exit.OK)
                    {
                        return;
                    }

                    switch (option)
                    {
                        case "list":
                            exitCode = ListInstalls();
                            break;

                        case "add":
                            exitCode = AddInstall((AddOptions)suboptions);
                            break;

                        case "clone":
                            exitCode = CloneInstall((CloneOptions)suboptions);
                            break;

                        case "rename":
                            exitCode = RenameInstall((RenameOptions)suboptions);
                            break;

                        case "forget":
                            exitCode = ForgetInstall((ForgetOptions)suboptions);
                            break;

                        case "use":
                        case "default":
                            exitCode = SetDefaultInstall((DefaultOptions)suboptions);
                            break;

                        case "fake":
                            exitCode = FakeNewGameInstance((FakeOptions)suboptions);
                            break;

                        default:
                            User.RaiseMessage("{0}: instance {1}", Properties.Resources.UnknownCommand, option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private IUser               User    { get; set; }
        private GameInstanceManager Manager { get; set; }

        #region option functions

        private int ListInstalls()
        {
            var output = Manager.Instances
                .OrderByDescending(i => i.Value.Name == Manager.AutoStartInstance)
                .ThenByDescending(i => i.Value.Version() ?? GameVersion.Any)
                .ThenBy(i => i.Key)
                .Select(i => new
                {
                    Name = i.Key,
                    Version = i.Value.Version()?.ToString() ?? Properties.Resources.InstanceListNoVersion,
                    Default = i.Value.Name == Manager.AutoStartInstance
                        ? Properties.Resources.InstanceListYes
                        : Properties.Resources.InstanceListNo,
                    Path = i.Value.GameDir()
                })
                .ToList();

            string nameHeader    = Properties.Resources.InstanceListNameHeader;
            string versionHeader = Properties.Resources.InstanceListVersionHeader;
            string defaultHeader = Properties.Resources.InstanceListDefaultHeader;
            string pathHeader    = Properties.Resources.InstanceListPathHeader;

            var nameWidth    = Enumerable.Repeat(nameHeader, 1).Concat(output.Select(i => i.Name)).Max(i => i.Length);
            var versionWidth = Enumerable.Repeat(versionHeader, 1).Concat(output.Select(i => i.Version)).Max(i => i.Length);
            var defaultWidth = Enumerable.Repeat(defaultHeader, 1).Concat(output.Select(i => i.Default)).Max(i => i.Length);
            var pathWidth    = Enumerable.Repeat(pathHeader, 1).Concat(output.Select(i => i.Path)).Max(i => i.Length);

            const string columnFormat = "{0}  {1}  {2}  {3}";

            User.RaiseMessage(columnFormat,
                nameHeader.PadRight(nameWidth),
                versionHeader.PadRight(versionWidth),
                defaultHeader.PadRight(defaultWidth),
                pathHeader.PadRight(pathWidth)
            );

            User.RaiseMessage(columnFormat,
                new string('-', nameWidth),
                new string('-', versionWidth),
                new string('-', defaultWidth),
                new string('-', pathWidth)
            );

            foreach (var line in output)
            {
                User.RaiseMessage(columnFormat,
                    line.Name.PadRight(nameWidth),
                    line.Version.PadRight(versionWidth),
                    line.Default.PadRight(defaultWidth),
                    line.Path.PadRight(pathWidth)
                );
            }

            return Exit.OK;
        }

        private int AddInstall(AddOptions options)
        {
            if (options.name == null || options.path == null)
            {
                User.RaiseMessage("add <name> <{0}> - {1}", Properties.Resources.Path, Properties.Resources.ArgumentMissing);
                return Exit.BADOPT;
            }

            if (Manager.HasInstance(options.name))
            {
                User.RaiseMessage(Properties.Resources.InstanceAddDuplicate, options.name);
                return Exit.BADOPT;
            }

            try
            {
                string path = options.path;
                Manager.AddInstance(path, options.name, User);
                User.RaiseMessage(Properties.Resources.InstanceAdded, options.name, options.path);
                return Exit.OK;
            }
            catch (NotKSPDirKraken ex)
            {
                User.RaiseMessage(Properties.Resources.InstanceNotInstance, ex.path);
                return Exit.BADOPT;
            }
        }

        private int CloneInstall(CloneOptions options)
        {
            if (options.nameOrPath == null || options.new_name == null || options.new_path == null)
            {
                User.RaiseMessage("instance clone <nameOrPathExistingInstance> <newName> <newPath> - {0}",
                    Properties.Resources.ArgumentMissing);
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
                            Manager.CloneInstance(instance, newName, newPath);
                            break;
                        }
                    }
                }
                // Try to use instanceNameOrPath as a path and create a new game instance.
                // If it's valid, go on.
                else if (Manager.InstanceAt(instanceNameOrPath) is CKAN.GameInstance instance && instance.Valid)
                {
                    Manager.CloneInstance(instance, newName, newPath);
                }
                // There is no instance with this name or at this path.
                else
                {
                    throw new NoGameInstanceKraken();
                }
            }
            catch (NotKSPDirKraken kraken)
            {
                // Two possible reasons:
                // First: The instance to clone is not a valid game instance.
                // Only occurs if user manipulated directory and deleted files/folders
                // which CKAN searches for in validity test.

                // Second: Something went wrong adding the new instance to the registry,
                // most likely because the newly created directory is not valid.

                log.Error(kraken);
                return Exit.ERROR;
            }
            catch (PathErrorKraken kraken)
            {
                // The new path is not empty
                // The kraken contains a message to inform the user.
                log.Error(kraken.Message + kraken.path);
                return Exit.ERROR;
            }
            catch (IOException e)
            {
                // Something went wrong copying the files. Contains a message.
                log.Error(e);
                return Exit.ERROR;
            }
            catch (NoGameInstanceKraken)
            {
                User.RaiseError(Properties.Resources.InstanceCloneNotFound, instanceNameOrPath);
                ListInstalls();
                return Exit.ERROR;
            }
            catch (InstanceNameTakenKraken kraken)
            {
                User.RaiseError(Properties.Resources.InstanceDuplicate, kraken.instName);
                return Exit.BADOPT;
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
                User.RaiseMessage(Properties.Resources.InstanceCloneFailed);
                return Exit.ERROR;
            }
        }

        private int RenameInstall(RenameOptions options)
        {
            if (options.old_name == null || options.new_name == null)
            {
                User.RaiseMessage("rename <old_name> <new_name> - {0}", Properties.Resources.ArgumentMissing);
                return Exit.BADOPT;
            }

            if (!Manager.HasInstance(options.old_name))
            {
                User.RaiseMessage(Properties.Resources.InstanceNotFound, options.old_name);
                return Exit.BADOPT;
            }

            Manager.RenameInstance(options.old_name, options.new_name);

            User.RaiseMessage(Properties.Resources.InstanceRenamed, options.old_name, options.new_name);
            return Exit.OK;
        }

        private int ForgetInstall(ForgetOptions options)
        {
            if (options.name == null)
            {
                User.RaiseMessage("forget <name> - {0}", Properties.Resources.ArgumentMissing);
                return Exit.BADOPT;
            }

            if (!Manager.HasInstance(options.name))
            {
                User.RaiseMessage(Properties.Resources.InstanceNotFound, options.name);
                return Exit.BADOPT;
            }

            Manager.RemoveInstance(options.name);

            User.RaiseMessage(Properties.Resources.InstanceForgot, options.name);
            return Exit.OK;
        }

        private int SetDefaultInstall(DefaultOptions options)
        {
            string name = options.name;

            if (name == null)
            {
                // No input argument from the user. Present a list of the possible instances.
                string message = $"default <name> - {Properties.Resources.InstanceDefaultArgumentMissing}";

                // Check if there is a default instance.
                string defaultInstance = Manager.Configuration.AutoStartInstance;
                int defaultInstancePresent = 0;

                if (!string.IsNullOrWhiteSpace(defaultInstance))
                {
                    defaultInstancePresent = 1;
                }

                object[] keys = new object[Manager.Instances.Count + defaultInstancePresent];

                // Populate the list of instances.
                for (int i = 0; i < Manager.Instances.Count; i++)
                {
                    var instance = Manager.Instances.ElementAt(i);

                    keys[i + defaultInstancePresent] = string.Format("\"{0}\" - {1}", instance.Key, instance.Value.GameDir());
                }

                // Mark the default instance for the user.
                if (!string.IsNullOrWhiteSpace(defaultInstance))
                {
                    keys[0] = Manager.Instances.IndexOfKey(defaultInstance);
                }

                int result;

                try
                {
                    result = User.RaiseSelectionDialog(message, keys);
                }
                catch (Kraken)
                {
                    return Exit.BADOPT;
                }

                if (result < 0)
                {
                    return Exit.BADOPT;
                }

                name = Manager.Instances.ElementAt(result).Key;
            }

            if (!Manager.Instances.ContainsKey(name))
            {
                User.RaiseMessage(Properties.Resources.InstanceNotFound, name);
                return Exit.BADOPT;
            }

            try
            {
                Manager.SetAutoStart(name);
            }
            catch (NotKSPDirKraken k)
            {
                User.RaiseMessage(Properties.Resources.InstanceNotInstance, k.path);
                return Exit.BADOPT;
            }

            User.RaiseMessage(Properties.Resources.InstanceDefaultSet, name);
            return Exit.OK;
        }

        /// <summary>
        /// Creates a new fake game instance after the conditions CKAN tests for valid install directories.
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
                User.RaiseMessage(Properties.Resources.InstanceFakeBadArguments);
                return Exit.BADOPT;
            }


            if (options.name == null || options.path == null || options.version == null)
            {
                User.RaiseMessage("instance fake <name> <{0}> <version> " +
                    "[--game KSP|KSP2] " +
                	"[--MakingHistory <version>] [--BreakingGround <version>] - {1}",
                    Properties.Resources.Path, Properties.Resources.ArgumentMissing);
                return badArgument();
            }

            log.Debug("Parsing arguments...");
            // Parse all options
            string installName = options.name;
            string path = options.path;
            GameVersion version;
            bool setDefault = options.setDefault;
            IGame game = KnownGames.GameByShortName(options.gameId);
            if (game == null)
            {
                User.RaiseMessage(Properties.Resources.InstanceFakeBadGame, options.gameId);
                return badArgument();
            }

            // options.<DLC>Version is "none" if the DLC should not be simulated.
            Dictionary<DLC.IDlcDetector, GameVersion> dlcs = new Dictionary<DLC.IDlcDetector, GameVersion>();
            if (options.makingHistoryVersion != null && options.makingHistoryVersion.ToLower() != "none")
            {
                if (GameVersion.TryParse(options.makingHistoryVersion, out GameVersion ver))
                {
                    dlcs.Add(new MakingHistoryDlcDetector(), ver);
                }
                else
                {
                    User.RaiseError(Properties.Resources.InstanceFakeMakingHistory);
                    return badArgument();
                }
            }
            if (options.breakingGroundVersion != null && options.breakingGroundVersion.ToLower() != "none")
            {
                if (GameVersion.TryParse(options.breakingGroundVersion, out GameVersion ver))
                {
                    dlcs.Add(new BreakingGroundDlcDetector(), ver);
                }
                else
                {
                    User.RaiseError(Properties.Resources.InstanceFakeBreakingGround);
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
                User.RaiseError(Properties.Resources.InstanceFakeVersion);
                return badArgument();
            }

            // Get the full version including build number.
            try
            {
                version = version.RaiseVersionSelectionDialog(game, User);
            }
            catch (BadGameVersionKraken)
            {
                User.RaiseError(Properties.Resources.InstanceFakeBadGameVersion);
                return badArgument();
            }
            catch (CancelledActionKraken)
            {
                User.RaiseError(Properties.Resources.InstanceFakeCancelled);
                return error();
            }

            User.RaiseMessage(Properties.Resources.InstanceFakeCreating,
                installName, path, version.ToString());
            log.Debug("Faking instance...");

            try
            {
                // Pass all arguments to CKAN.GameInstanceManager.FakeInstance() and create a new one.
                Manager.FakeInstance(game, installName, path, version, dlcs);
                if (setDefault)
                {
                    User.RaiseMessage(Properties.Resources.InstanceFakeDefault);
                    Manager.SetAutoStart(installName);
                }
            }
            catch (InstanceNameTakenKraken kraken)
            {
                User.RaiseError(Properties.Resources.InstanceDuplicate, kraken.instName);
                return badArgument();
            }
            catch (BadInstallLocationKraken kraken)
            {
                // The folder exists and is not empty.
                User.RaiseError("{0}", kraken.Message);
                return badArgument();
            }
            catch (WrongGameVersionKraken kraken)
            {
                // Thrown because the specified game instance is too old for one of the selected DLCs.
                User.RaiseError("{0}", kraken.Message);
                return badArgument();
            }
            catch (NotKSPDirKraken kraken)
            {
                // Something went wrong adding the new instance to the registry,
                // most likely because the newly created directory is somehow not valid.
                log.Error(kraken);
                User.RaiseError("{0}", kraken.Message);
                return error();
            }
            catch (InvalidKSPInstanceKraken)
            {
                // Thrown by Manager.SetAutoStart() if Manager.HasInstance returns false.
                // Will be checked again down below with a proper error message
            }


            // Test if the instance was added to the registry.
            // No need to test if valid, because this is done in AddInstance().
            if (Manager.HasInstance(installName))
            {
                User.RaiseMessage(Properties.Resources.InstanceFakeDone);
                return Exit.OK;
            }
            else
            {
                User.RaiseError(Properties.Resources.InstanceFakeFailed);
                return error();
            }
        }
        #endregion
    }
}
