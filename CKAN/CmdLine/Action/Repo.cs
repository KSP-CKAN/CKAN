using System;
using System.Collections.Generic;
using CommandLine;

namespace CKAN.CmdLine
{
    public class Repo : ISubCommand
    {
        public string option;
        public object suboptions;

        internal class RepoSubOptions : CommonOptions
        {
            [VerbOption("list", HelpText="List repositories")]
            public CommonOptions ListOptions { get; set; }

            [VerbOption("add", HelpText="Add a repository")]
            public AddOptions AddOptions { get; set; }

            [VerbOption("rename", HelpText="Rename a repository")]
            public RenameOptions RenameOptions { get; set; }

            [VerbOption("forget", HelpText="Forget a repository")]
            public ForgetOptions ForgetOptions { get; set; }

            [VerbOption("default", HelpText="Set the default repository")]
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

        public Repo()
        {
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

            if (args == null || args.Length == 0)
            {
                // There's got to be a better way of showing help...
                args = new string[1];
                args[0] = "help";
            }

            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new RepoSubOptions (), Parse, null);

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
                    User.WriteLine("Unknown command: ksp {0}", option);
                    return Exit.BADOPT;
            }
        }

        private static int ListInstalls()
        {
            User.WriteLine("Listing all known repositories:");
            return Exit.OK;
        }

        private static int AddInstall(AddOptions options)
        {
            User.WriteLine("Adding repository:");
            return Exit.OK;
        }

        private static int RenameInstall(RenameOptions options)
        {
            User.WriteLine("Renaming repository:");
            return Exit.OK;
        }

        private static int ForgetInstall(ForgetOptions options)
        {
            User.WriteLine("Forgetting repository:");
            return Exit.OK;
        }

        private static int SetDefaultInstall(DefaultOptions options)
        {
            User.WriteLine("Setting the default repository:");
            return Exit.OK;
        }
    }
}