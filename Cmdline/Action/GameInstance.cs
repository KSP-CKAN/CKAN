using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using log4net;
using CKAN.Versioning;
using CKAN.Games;

namespace CKAN.CmdLine
{
    public class GameInstance : ISubCommand
    {
        public GameInstance() { }
        protected static readonly ILog log = LogManager.GetLogger(typeof(GameInstance));

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
                    ht.AddPreOptionsLine("ckan instance - Manage game instances");
                    ht.AddPreOptionsLine($"Usage: ckan instance <command> [options]");
                }
                else
                {
                    ht.AddPreOptionsLine("instance " + verb + " - " + GetDescription(verb));
                    switch (verb)
                    {
                        // First the commands with three string arguments
                        case "fake":
                            ht.AddPreOptionsLine($"Usage: ckan instance {verb} [options] name path version [--MakingHistory <version>] [--BreakingGround <version>]");
                            break;

                        case "clone":
                            ht.AddPreOptionsLine($"Usage: ckan instance {verb} [options] instanceNameOrPath newname newpath");
                            break;

                        // Second the commands with two string arguments
                        case "add":
                            ht.AddPreOptionsLine($"Usage: ckan instance {verb} [options] name url");
                            break;
                        case "rename":
                            ht.AddPreOptionsLine($"Usage: ckan instance {verb} [options] oldname newname");
                            break;

                        // Now the commands with one string argument
                        case "remove":
                        case "forget":
                        case "use":
                        case "default":
                            ht.AddPreOptionsLine($"Usage: ckan instance {verb} [options] name");
                            break;

                        // Now the commands with only --flag type options
                        case "list":
                        default:
                            ht.AddPreOptionsLine($"Usage: ckan instance {verb} [options]");
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
            [ValueOption(0)] public string old_name { get; set; }
            [ValueOption(1)] public string new_name { get; set; }
        }

        internal class ForgetOptions : CommonOptions
        {
            [ValueOption(0)] public string name { get; set; }
        }

        internal class DefaultOptions : CommonOptions
        {
            [ValueOption(0)] public string name { get; set; }
        }

        internal class FakeOptions : CommonOptions
        {
            [ValueOption(0)] public string name { get; set; }
            [ValueOption(1)] public string path { get; set; }
            [ValueOption(2)] public string version { get; set; }

            [Option("MakingHistory", DefaultValue = "none", HelpText = "The version of the Making History DLC to be faked.")]
            public string makingHistoryVersion { get; set; }
            [Option("BreakingGround", DefaultValue = "none", HelpText = "The version of the Breaking Ground DLC to be faked.")]
            public string breakingGroundVersion { get; set; }

            [Option("set-default", DefaultValue = false, HelpText = "Set the new instance as the default one.")]
            public bool setDefault { get; set; }
        }

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
                        return;

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
                            User.RaiseMessage("Unknown command: instance {0}", option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private GameInstanceManager Manager { get; set; }
        private IUser      User    { get; set; }

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
                    Version = i.Value.Version()?.ToString() ?? "<NONE>",
                    Default = i.Value.Name == Manager.AutoStartInstance ? "Yes" : "No",
                    Path = i.Value.GameDir()
                })
                .ToList();

            const string nameHeader = "Name";
            const string versionHeader = "Version";
            const string defaultHeader = "Default";
            const string pathHeader = "Path";

            var nameWidth = Enumerable.Repeat(nameHeader, 1).Concat(output.Select(i => i.Name)).Max(i => i.Length);
            var versionWidth = Enumerable.Repeat(versionHeader, 1).Concat(output.Select(i => i.Version)).Max(i => i.Length);
            var defaultWidth = Enumerable.Repeat(defaultHeader, 1).Concat(output.Select(i => i.Default)).Max(i => i.Length);
            var pathWidth = Enumerable.Repeat(pathHeader, 1).Concat(output.Select(i => i.Path)).Max(i => i.Length);

            const string columnFormat = "{0}  {1}  {2}  {3}";

            User.RaiseMessage(string.Format(columnFormat,
                nameHeader.PadRight(nameWidth),
                versionHeader.PadRight(versionWidth),
                defaultHeader.PadRight(defaultWidth),
                pathHeader.PadRight(pathWidth)
            ));

            User.RaiseMessage(string.Format(columnFormat,
                new string('-', nameWidth),
                new string('-', versionWidth),
                new string('-', defaultWidth),
                new string('-', pathWidth)
            ));

            foreach (var line in output)
            {
                User.RaiseMessage(string.Format(columnFormat,
                   line.Name.PadRight(nameWidth),
                   line.Version.PadRight(versionWidth),
                   line.Default.PadRight(defaultWidth),
                   line.Path.PadRight(pathWidth)
               ));
            }

            return Exit.OK;
        }

        private int AddInstall(AddOptions options)
        {
            if (options.name == null || options.path == null)
            {
                User.RaiseMessage("add <name> <path> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (Manager.HasInstance(options.name))
            {
                User.RaiseMessage("Install with name \"{0}\" already exists, aborting..", options.name);
                return Exit.BADOPT;
            }

            try
            {
                string path = options.path;
                Manager.AddInstance(path, options.name, User);
                User.RaiseMessage("Added \"{0}\" with root \"{1}\" to known installs", options.name, options.path);
                return Exit.OK;
            }
            catch (NotKSPDirKraken ex)
            {
                User.RaiseMessage("Sorry, {0} does not appear to be a game instance", ex.path);
                return Exit.BADOPT;
            }
        }

        private int CloneInstall(CloneOptions options)
        {
            if (options.nameOrPath == null || options.new_name == null || options.new_path == null)
            {
                User.RaiseMessage("instance clone <nameOrPathExistingInstance> <newName> <newPath> - argument(s) missing");
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
                else if (Manager.InstanceAt(instanceNameOrPath, newName) is CKAN.GameInstance instance && instance.Valid)
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
                User.RaiseError(String.Format("No instance with this name or at this path: {0}\n See below for a list of known instances:\n", instanceNameOrPath));
                ListInstalls();
                return Exit.ERROR;
            }
            catch (InstanceNameTakenKraken kraken)
            {
                User.RaiseError("This instance name is already taken: {0}", kraken.instName);
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
                User.RaiseMessage("Something went wrong. Please look if the new directory has been created.\n",
                    "Try to add the new instance manually with \"ckan instance add\".\n");
                return Exit.ERROR;
            }
        }

        private int RenameInstall(RenameOptions options)
        {
            if (options.old_name == null || options.new_name == null)
            {
                User.RaiseMessage("rename <old_name> <new_name> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (!Manager.HasInstance(options.old_name))
            {
                User.RaiseMessage("Couldn't find install with name \"{0}\", aborting..", options.old_name);
                return Exit.BADOPT;
            }

            Manager.RenameInstance(options.old_name, options.new_name);

            User.RaiseMessage("Successfully renamed \"{0}\" to \"{1}\"", options.old_name, options.new_name);
            return Exit.OK;
        }

        private int ForgetInstall(ForgetOptions options)
        {
            if (options.name == null)
            {
                User.RaiseMessage("forget <name> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (!Manager.HasInstance(options.name))
            {
                User.RaiseMessage("Couldn't find install with name \"{0}\", aborting..", options.name);
                return Exit.BADOPT;
            }

            Manager.RemoveInstance(options.name);

            User.RaiseMessage("Successfully removed \"{0}\"", options.name);
            return Exit.OK;
        }

        private int SetDefaultInstall(DefaultOptions options)
        {
            string name = options.name;

            if (name == null)
            {
                // No input argument from the user. Present a list of the possible instances.
                string message = "default <name> - argument missing, please select from the list below.";

                // Check if there is a default instance.
                string defaultInstance = Manager.Configuration.AutoStartInstance;
                int defaultInstancePresent = 0;

                if (!String.IsNullOrWhiteSpace(defaultInstance))
                {
                    defaultInstancePresent = 1;
                }

                object[] keys = new object[Manager.Instances.Count + defaultInstancePresent];

                // Populate the list of instances.
                for (int i = 0; i < Manager.Instances.Count; i++)
                {
                    var instance = Manager.Instances.ElementAt(i);

                    keys[i + defaultInstancePresent] = String.Format("\"{0}\" - {1}", instance.Key, instance.Value.GameDir());
                }

                // Mark the default instance for the user.
                if (!String.IsNullOrWhiteSpace(defaultInstance))
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
                User.RaiseMessage("Couldn't find install with name \"{0}\", aborting..", name);
                return Exit.BADOPT;
            }

            try
            {
                Manager.SetAutoStart(name);
            }
            catch (NotKSPDirKraken k)
            {
                User.RaiseMessage("Sorry, {0} does not appear to be a game instance", k.path);
                return Exit.BADOPT;
            }

            User.RaiseMessage("Successfully set \"{0}\" as the default game instance", name);
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
                User.RaiseMessage("--Error--");
                return Exit.ERROR;
            }

            int badArgument()
            {
                log.Debug("Instance faking failed: bad argument(s). See console output for details.");
                User.RaiseMessage("--Error: bad argument(s)--");
                return Exit.BADOPT;
            }


            if (options.name == null || options.path == null || options.version == null)
            {
                User.RaiseMessage("instance fake <name> <path> <version> " +
                	"[--MakingHistory <version>] [--BreakingGround <version>] - argument(s) missing");
                return badArgument();
            }

            log.Debug("Parsing arguments...");
            // Parse all options
            string installName = options.name;
            string path = options.path;
            GameVersion version;
            bool setDefault = options.setDefault;

            // options.<DLC>Version is "none" if the DLC should not be simulated.
            Dictionary<DLC.IDlcDetector, GameVersion> dlcs = new Dictionary<DLC.IDlcDetector, GameVersion>();
            if (options.makingHistoryVersion != null && options.makingHistoryVersion.ToLower() != "none")
            {
                if (GameVersion.TryParse(options.makingHistoryVersion, out GameVersion ver))
                {
                    dlcs.Add(new DLC.MakingHistoryDlcDetector(), ver);
                }
                else
                {
                    User.RaiseError("Please check the Making History DLC version argument - Format it like Maj.Min.Patch - e.g. 1.1.0");
                    return badArgument();
                }
            }
            if (options.breakingGroundVersion != null && options.breakingGroundVersion.ToLower() != "none")
            {
                if (GameVersion.TryParse(options.breakingGroundVersion, out GameVersion ver))
                {
                    dlcs.Add(new DLC.BreakingGroundDlcDetector(), ver);
                }
                else
                {
                    User.RaiseError("Please check the Breaking Ground DLC version argument - Format it like Maj.Min.Patch - e.g. 1.1.0");
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
                User.RaiseError("Please check the version argument - Format it like Maj.Min.Patch[.Build] - e.g. 1.6.0 or 1.2.2.1622");
                return badArgument();
            }

            // Get the full version including build number.
            try
            {
                version = version.RaiseVersionSelectionDialog(new KerbalSpaceProgram(), User);
            }
            catch (BadGameVersionKraken)
            {
                User.RaiseError("Couldn't find a valid game version for your input.\n" +
                	"Make sure to enter the at least the version major and minor values in the form Maj.Min - e.g. 1.5");
                return badArgument();
            }
            catch (CancelledActionKraken)
            {
                User.RaiseError("Selection cancelled! Please call 'ckan instance fake' again.");
                return error();
            }

            User.RaiseMessage(String.Format("Creating new fake game instance {0} at {1} with version {2}", installName, path, version.ToString()));
            log.Debug("Faking instance...");

            try
            {
                // Pass all arguments to CKAN.GameInstanceManager.FakeInstance() and create a new one.
                Manager.FakeInstance(new KerbalSpaceProgram(), installName, path, version, dlcs);
                if (setDefault)
                {
                    User.RaiseMessage("Setting new instance to default...");
                    Manager.SetAutoStart(installName);
                }
            }
            catch (InstanceNameTakenKraken kraken)
            {
                User.RaiseError("This instance name is already taken: {0}", kraken.instName);
                return badArgument();
            }
            catch (BadInstallLocationKraken kraken)
            {
                // The folder exists and is not empty.
                User.RaiseError(kraken.Message);
                return badArgument();
            }
            catch (WrongGameVersionKraken kraken)
            {
                // Thrown because the specified game instance is too old for one of the selected DLCs.
                User.RaiseError(kraken.Message);
                return badArgument();
            }
            catch (NotKSPDirKraken kraken)
            {
                // Something went wrong adding the new instance to the registry,
                // most likely because the newly created directory is somehow not valid.
                log.Error(kraken);
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
                User.RaiseMessage("--Done--");
                return Exit.OK;
            }
            else
            {
                User.RaiseError("Something went wrong. Try to add the instance yourself with \"ckan instance add\".",
                    "Also look if the new directory has been created.");
                return error();
            }
        }
        #endregion
    }
}
