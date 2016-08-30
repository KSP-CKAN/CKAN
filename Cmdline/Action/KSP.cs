using System;
using System.Linq;
using System.Text;
using CommandLine;

namespace CKAN.CmdLine
{
    public class KSP : ISubCommand
    {
        public KSPManager Manager { get; set; }
        public IUser User { get; set; }
        public string option;
        public object suboptions;

        public KSP(KSPManager manager, IUser user)
        {
            Manager = manager;
            User = user;
        }

        internal class KSPSubOptions : CommonOptions
        {
            [VerbOption("list", HelpText="List KSP installs")]
            public CommonOptions ListOptions { get; set; }

            [VerbOption("add", HelpText="Add a KSP install")]
            public AddOptions AddOptions { get; set; }

            [VerbOption("rename", HelpText="Rename a KSP install")]
            public RenameOptions RenameOptions { get; set; }

            [VerbOption("forget", HelpText="Forget a KSP install")]
            public ForgetOptions ForgetOptions { get; set; }

            [VerbOption("default", HelpText="Set the default KSP install")]
            public DefaultOptions DefaultOptions { get; set; }
        }

        internal class AddOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }

            [ValueOption(1)]
            public string path { get; set; }
        }

        internal class RenameOptions : CommonOptions
        {
            [ValueOption(0)]
            public string old_name { get; set; }

            [ValueOption(1)]
            public string new_name { get; set; }
        }

        internal class ForgetOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }
        }

        internal class DefaultOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }
        }

        internal void Parse(string option, object suboptions)
        {
            this.option = option;
            this.suboptions = suboptions;
        }

        // This is required by ISubCommand
        public int RunSubCommand(SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();

            if (args.Length == 0)
            {
                // There's got to be a better way of showing help...
                args = new string[1];
                args[0] = "help";
            }

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

            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new KSPSubOptions (), Parse);

            // That line above will have set our 'option' and 'suboption' fields.

            switch (option)
            {
                case "list":
                    return ListInstalls();

                case "add":
                    return AddInstall((AddOptions)suboptions);

                case "rename":
                    return RenameInstall((RenameOptions)suboptions);

                case "forget":
                    return ForgetInstall((ForgetOptions)suboptions);

                case "default":
                    return SetDefaultInstall((DefaultOptions)suboptions);

                default:
                    User.RaiseMessage("Unknown command: ksp {0}", option);
                    return Exit.InvalidOption;
            }
        }

        private int ListInstalls()
        {
            string preferredGameDir = null;

            var preferredInstance = Manager.GetPreferredInstance();

            if (preferredInstance != null)
            {
                preferredGameDir = preferredInstance.GameDir();
            }

            var output = Manager.Instances
                .OrderByDescending(i => i.Value.GameDir() == preferredGameDir)
                .ThenByDescending(i => i.Value.Version())
                .ThenBy(i => i.Key)
                .Select(i => new
                {
                    Name = i.Key,
                    Version = i.Value.Version().ToString(),
                    Default = i.Value.GameDir() == preferredGameDir ? "Yes" : "No",
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

            return Exit.Ok;
        }

        private int AddInstall(AddOptions options)
        {
            if (options.name == null || options.path == null)
            {
                User.RaiseMessage("add <name> <path> - argument missing, perhaps you forgot it?");
                return Exit.InvalidOption;
            }

            if (Manager.HasInstance(options.name))
            {
                User.RaiseMessage("Install with name \"{0}\" already exists, aborting..", options.name);
                return Exit.InvalidOption;
            }

            try
            {
                string path = options.path;
                Manager.AddInstance(options.name, new CKAN.KSP(path, User));
                User.RaiseMessage("Added \"{0}\" with root \"{1}\" to known installs", options.name, options.path);
                return Exit.Ok;
            }
            catch (NotKSPDirKraken ex)
            {
                User.RaiseMessage("Sorry, {0} does not appear to be a KSP directory", ex.path);
                return Exit.InvalidOption;
            }
        }

        private int RenameInstall(RenameOptions options)
        {
            if (options.old_name == null || options.new_name == null)
            {
                User.RaiseMessage("rename <old_name> <new_name> - argument missing, perhaps you forgot it?");
                return Exit.InvalidOption;
            }

            if (!Manager.HasInstance(options.old_name))
            {
                User.RaiseMessage("Couldn't find install with name \"{0}\", aborting..", options.old_name);
                return Exit.InvalidOption;
            }

            Manager.RenameInstance(options.old_name, options.new_name);

            User.RaiseMessage("Successfully renamed \"{0}\" to \"{1}\"", options.old_name, options.new_name);
            return Exit.Ok;
        }

        private int ForgetInstall(ForgetOptions options)
        {
            if (options.name == null)
            {
                User.RaiseMessage("forget <name> - argument missing, perhaps you forgot it?");
                return Exit.InvalidOption;
            }

            if (!Manager.HasInstance(options.name))
            {
                User.RaiseMessage("Couldn't find install with name \"{0}\", aborting..", options.name);
                return Exit.InvalidOption;
            }

            Manager.RemoveInstance(options.name);

            User.RaiseMessage("Successfully removed \"{0}\"", options.name);
            return Exit.Ok;
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
                    return Exit.InvalidOption;
                }

                if (result < 0)
                {
                    return Exit.InvalidOption;
                }

                name = Manager.Instances.ElementAt(result).Key;
            }

            if (!Manager.Instances.ContainsKey(name))
            {
                User.RaiseMessage("Couldn't find install with name \"{0}\", aborting..", name);
                return Exit.InvalidOption;
            }

            Manager.SetAutoStart(name);

            User.RaiseMessage("Successfully set \"{0}\" as the default KSP installation", name);
            return Exit.Ok;
        }
    }
}