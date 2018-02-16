using System;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using CKAN.Versioning;

namespace CKAN.CmdLine
{
    public class KSP : ISubCommand
    {
        public KSP() { }

        internal class KSPSubOptions : VerbCommandOptions
        {
            [VerbOption("list",    HelpText = "List KSP installs")]
            public CommonOptions ListOptions     { get; set; }

            [VerbOption("add",     HelpText = "Add a KSP install")]
            public AddOptions    AddOptions      { get; set; }

            [VerbOption("rename",  HelpText = "Rename a KSP install")]
            public RenameOptions RenameOptions   { get; set; }

            [VerbOption("forget",  HelpText = "Forget a KSP install")]
            public ForgetOptions ForgetOptions   { get; set; }

            [VerbOption("default", HelpText = "Set the default KSP install")]
            public DefaultOptions DefaultOptions { get; set; }

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
                        // First the commands with two string arguments
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

                // Mark the default intance for the user.
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
    }
}
