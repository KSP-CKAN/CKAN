using System;
using System.Collections.Generic;
using CommandLine;

namespace CKAN.CmdLine
{
    public class KSP
    {
        public string option;
        public object suboptions;

        internal class KSPSubOptions
        {
            [VerbOption("list", HelpText="List KSP installs")]
            public CommonOptions ListOptions { get; set; }

            [VerbOption("add", HelpText="Add a KSP install")]
            public AddOptions AddOptions { get; set; }

            [VerbOption("rename", HelpText="Rename a KSP install")]
            public RenameOptions RenameOptions { get; set; }

            [VerbOption("forget", HelpText="Forget a KSP install")]
            public ForgetOptions ForgetOptions { get; set; }

            [VerbOption("default", HelpText="Set a default KSP install")]
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

        public KSP(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                // There's got to be a better way of showing help...
                args = new string[1];
                args[0] = "help";
            }

            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new KSPSubOptions (), Parse, null);
        }

        public void Parse(string option, object suboptions)
        {
            this.option = option;
            this.suboptions = suboptions;
        }

        public int RunSubCommand()
        {
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
                    User.WriteLine("Unknown command: ksp {0}", option);
                    return Exit.BADOPT;
            }
        }

        private static int ListInstalls()
        {
            User.WriteLine("Listing all known KSP installations:");
            User.WriteLine("");

            int count = 1;
            foreach (var instance in KSPManager.Instances)
            {
                User.WriteLine("{0}) \"{1}\" - {2}", count, instance.Key, instance.Value.GameDir());
                count++;
            }

            return Exit.OK;
        }

        private static int AddInstall(AddOptions options)
        {
            if (options.name == null || options.path == null)
            {
                User.WriteLine("add-install <name> <path> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (KSPManager.Instances.ContainsKey(options.name))
            {
                User.WriteLine("Install with name \"{0}\" already exists, aborting..", options.name);
                return Exit.BADOPT;
            }

            try
            {

                KSPManager.AddInstance(options.name, options.path);
                User.WriteLine("Added \"{0}\" with root \"{1}\" to known installs", options.name, options.path);
                return Exit.OK;
            }
            catch (NotKSPDirKraken ex)
            {
                User.WriteLine("Sorry, {0} does not appear to be a KSP directory", ex.path);
                return Exit.BADOPT;
            }
        }

        private static int RenameInstall(RenameOptions options)
        {
            if (options.old_name == null || options.new_name == null)
            {
                User.WriteLine("rename-install <old_name> <new_name> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (!KSPManager.Instances.ContainsKey(options.old_name))
            {
                User.WriteLine("Couldn't find install with name \"{0}\", aborting..", options.old_name);
                return Exit.BADOPT;
            }

            KSPManager.RenameInstance(options.old_name, options.new_name);

            User.WriteLine("Successfully renamed \"{0}\" to \"{1}\"", options.old_name, options.new_name);
            return Exit.OK;
        }

        private static int ForgetInstall(ForgetOptions options)
        {
            if (options.name == null)
            {
                User.WriteLine("remove-install <name> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (!KSPManager.Instances.ContainsKey(options.name))
            {
                User.WriteLine("Couldn't find install with name \"{0}\", aborting..", options.name);
                return Exit.BADOPT;
            }

            KSPManager.RemoveInstance(options.name);

            User.WriteLine("Successfully removed \"{0}\"", options.name);
            return Exit.OK;
        }

        private static int SetDefaultInstall(DefaultOptions options)
        {
            if (options.name == null)
            {
                User.WriteLine("set-default-install <name> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (!KSPManager.Instances.ContainsKey(options.name))
            {
                User.WriteLine("Couldn't find install with name \"{0}\", aborting..", options.name);
                return Exit.BADOPT;
            }

            KSPManager.SetAutoStart(options.name);

            User.WriteLine("Successfully set \"{0}\" as the default KSP installation", options.name);
            return Exit.OK;
        }
    }
}