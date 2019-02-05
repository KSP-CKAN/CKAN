using System;
using System.IO;
using System.Linq;
using CKAN.Versioning;
using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{
    public class KSP : ISubCommand
    {
        public KSP() { }
        protected static readonly ILog log = LogManager.GetLogger(typeof(KSP));

        internal class KSPSubOptions : VerbCommandOptions
        {
            [VerbOption("list",    HelpText = "List KSP installs")]
            public CommonOptions  ListOptions    { get; set; }

            [VerbOption("add",     HelpText = "Add a KSP install")]
            public AddOptions     AddOptions     { get; set; }

            [VerbOption("clone",   HelpText = "Clone an existing KSP install")]
            public CloneOptions   CloneOptions   { get; set; }

            [VerbOption("rename",  HelpText = "Rename a KSP install")]
            public RenameOptions  RenameOptions  { get; set; }

            [VerbOption("forget",  HelpText = "Forget a KSP install")]
            public ForgetOptions  ForgetOptions  { get; set; }

            [VerbOption("default", HelpText = "Set the default KSP install")]
            public DefaultOptions DefaultOptions { get; set; }

            [VerbOption("fake",    HelpText = "Fake a KSP install")]
            public FakeOptions    FakeOptions    { get; set; }

            [HelpVerbOption]
            public string GetUsage(string verb)
            {
                HelpText ht = HelpText.AutoBuild(this, verb);
                // Add a usage prefix line
                ht.AddPreOptionsLine(" ");
                if (string.IsNullOrEmpty(verb))
                {
                    ht.AddPreOptionsLine("ckan ksp - Manage KSP installs");
                    ht.AddPreOptionsLine($"Usage: ckan ksp <command> [options]");
                }
                else
                {
                    ht.AddPreOptionsLine("ksp " + verb + " - " + GetDescription(verb));
                    switch (verb)
                    {
                        // First the commands with three string arguments
                        case "fake":
                            ht.AddPreOptionsLine($"Usage: ckan ksp {verb} [options] name path version dlcVersion");
                            ht.AddPreOptionsLine($"Choose dlcVersion \"none\" if you want no simulated dlc.");
                            break;

                        case "clone":
                            ht.AddPreOptionsLine($"Usage: ckan ksp {verb} [options] instanceNameOrPath newname newpath");
                            break;

                        // Second the commands with two string arguments
                        case "add":
                            ht.AddPreOptionsLine($"Usage: ckan ksp {verb} [options] name url");
                            break;
                        case "rename":
                            ht.AddPreOptionsLine($"Usage: ckan ksp {verb} [options] oldname newname");
                            break;

                        // Now the commands with one string argument
                        case "remove":
                        case "forget":
                        case "use":
                        case "default":
                            ht.AddPreOptionsLine($"Usage: ckan ksp {verb} [options] name");
                            break;

                        // Now the commands with only --flag type options
                        case "list":
                        default:
                            ht.AddPreOptionsLine($"Usage: ckan ksp {verb} [options]");
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
            [ValueOption(3)] public string dlcVersion { get; set; }
            [Option("set-default", DefaultValue = false, HelpText = "Set the new instance to the default one.")] public bool setToDefault { get; set; }
        }

        // This is required by ISubCommand
        public int RunSubCommand(KSPManager manager, CommonOptions opts, SubCommandOptions unparsed)
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
            Parser.Default.ParseArgumentsStrict(args, new KSPSubOptions(), (string option, object suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    User     = new ConsoleUser(options.Headless);
                    Manager  = manager ?? new KSPManager(User);
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
                            exitCode = FakeNewKSPInstall((FakeOptions)suboptions);
                            break;

                        default:
                            User.RaiseMessage("Unknown command: ksp {0}", option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private KSPManager Manager { get; set; }
        private IUser      User    { get; set; }

        #region option functions

        private int ListInstalls()
        {
            var output = Manager.Instances
                .OrderByDescending(i => i.Value.Name == Manager.AutoStartInstance)
                .ThenByDescending(i => i.Value.Version() ?? KspVersion.Any)
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
                Manager.AddInstance(new CKAN.KSP(path, options.name, User));
                User.RaiseMessage("Added \"{0}\" with root \"{1}\" to known installs", options.name, options.path);
                return Exit.OK;
            }
            catch (NotKSPDirKraken ex)
            {
                User.RaiseMessage("Sorry, {0} does not appear to be a KSP directory", ex.path);
                return Exit.BADOPT;
            }
        }

        private int CloneInstall(CloneOptions options)
        {
            if (options.nameOrPath == null || options.new_name == null || options.new_path == null)
            {
                User.RaiseMessage("ksp clone <nameOrPathExistingInstance> <newName> <newPath> - argument(s) missing");
                return Exit.BADOPT;
            }

            // Parse all options
            string instanceNameOrPath = options.nameOrPath;
            string new_name = options.new_name;
            string new_path = options.new_path;


            log.Info("Cloning the KSP instance: " + options.nameOrPath);

            try
            {
                // Try instanceNameOrPath as name and search the registry for it.
                if (Manager.HasInstance(instanceNameOrPath))
                {
                    CKAN.KSP[] listOfInstances = Manager.Instances.Values.ToArray();
                    foreach (CKAN.KSP instance in listOfInstances)
                    {
                        if (instance.Name == instanceNameOrPath)
                        {
                            // Found it, now clone it.
                            Manager.CloneInstance(instance, new_name, new_path);
                            break;
                        }
                    }
                }
                // Try to use instanceNameOrPath as a path and create a new KSP object.
                // If it's valid, go on.
                else if (new CKAN.KSP(instanceNameOrPath, new_name, User) is CKAN.KSP instance && instance.Valid)
                {
                    Manager.CloneInstance(instance, new_name, new_path);

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
                // First: The instance to clone is not a valid KSP instance.
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


            // Test if the instance was added to the registry.
            // No need to test if valid, because this is done in AddInstance(),
            // so if something went wrong, HasInstance is false.
            if (Manager.HasInstance(new_name))
            {
                return Exit.OK;
            }
            else
            {
                User.RaiseMessage("Something went wrong. Please look if the new directory has been created.\n",
                    "Try to add the new instance manually with \"ckan ksp add\".\n");
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
                string defaultInstance = Manager.Win32Registry.AutoStartInstance;
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
                User.RaiseMessage("Sorry, {0} does not appear to be a KSP directory", k.path);
                return Exit.BADOPT;
            }

            User.RaiseMessage("Successfully set \"{0}\" as the default KSP installation", name);
            return Exit.OK;
        }

        /// <summary>
        /// Creates a new fake KSP install after the conditions CKAN tests for valid install directories.
        /// Used for developing and testing purposes.
        /// </summary>
        private int FakeNewKSPInstall (FakeOptions options)
        {
            if (options.name == null || options.path == null || options.version == null)
            {
                User.RaiseMessage("ksp fake <name> <path> <version> [dlcVersion] - argument(s) missing");
                return Exit.BADOPT;
            }

            log.Debug("Parsing arguments...");
            // Parse all options
            string installName = options.name;
            string path = options.path;
            KspVersion version;
            bool setToDefault = options.setToDefault;
            // dlcVersion is null if a user wants no simulated DLC
            string dlcVersion;
            if (options.dlcVersion == null || options.dlcVersion.ToLower() == "none")
            {
                dlcVersion = null;
            }
            else
            {
                dlcVersion = options.dlcVersion;
            }

            // Parse the choosen KSP version
            try
            {
                version = KspVersion.Parse(options.version);
            }
            catch (FormatException)
            {
                // Thrown if there is anything besides numbers and points in the version string or a different syntactic error.
                User.RaiseError("Please check the version argument - Format it like Maj.Min.Patch[.Build] - e.g. 1.6.0 or 1.2.2.1622");
                return Exit.BADOPT;
            }

            // Get the full version including build number.
            try
            {
                version = version.RaiseVersionSelectionDialog(User);
            }
            catch (IncorrectKSPVersionKraken)
            {
                User.RaiseError("Couldn't find a valid KSP version for your input.\n" +
                	"Make sure to enter the at least the version major and minor values in the form Maj.Min - e.g. 1.5");
                return Exit.BADOPT;
            }
            catch (CancelledActionKraken)
            {
                User.RaiseError("Selection cancelled! Please call 'ckan ksp fake' again.");
                return Exit.ERROR;
            }


            User.RaiseMessage(String.Format("Creating new fake KSP install {0} at {1} with version {2}", installName, path, version.ToString()));

            try
            {
                // Pass all arguments to CKAN.KSPManager.FakeInstance() and create a new one.
                Manager.FakeInstance(installName, path, version, dlcVersion);
                if (setToDefault)
                    User.RaiseMessage("Setting new instance to default...");
                    Manager.SetAutoStart(installName);
            }
            catch (BadInstallLocationKraken kraken)
            {
                // The folder exists and is not empty.
                log.Error(kraken);
                return Exit.ERROR;
            }
            catch (NotKSPDirKraken kraken)
            {
                // Something went wrong adding the new instance to the registry,
                // most likely because the newly created directory is not valid.
                log.Error(kraken);
                return Exit.ERROR;
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
                User.RaiseError("Something went wrong. Try to add the instance yourself with \"ckan ksp add\".",
                    "Also look if the new directory has been created.");
                return Exit.ERROR;
            }
        }
        #endregion
    }
}
