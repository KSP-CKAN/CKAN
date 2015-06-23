using System;
using System.Linq;
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
                    return Exit.BADOPT;
            }
        }

        private int ListInstalls()
        {
            User.RaiseMessage("Listing all known KSP installations:");
            User.RaiseMessage(String.Empty);

            int count = 1;
            foreach (var instance in Manager.Instances)
            {
                User.RaiseMessage("{0}) \"{1}\" - {2}", count, instance.Key, instance.Value.GameDir());
                count++;
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
                Manager.AddInstance(options.name, new CKAN.KSP(path, User));
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

            Manager.SetAutoStart(name);

            User.RaiseMessage("Successfully set \"{0}\" as the default KSP installation", name);
            return Exit.OK;
        }
    }
}