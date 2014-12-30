using System;
using CommandLine;
using CKAN;

namespace CKAN.CmdLine
{
	public struct RepositoryList
	{
		public Repository[] repositories;
	}

	public class Repo : ISubCommand
	{
        public KSPManager Manager { get; set; }
        public IUser User { get; set; }
		public string option;
		public object suboptions;

        public Repo(KSPManager manager, IUser user)
        {
            Manager = manager;
            User = user;
        }

        internal class RepoSubOptions : CommonOptions
        {
            [VerbOption("available", HelpText="List (canonical) available repositories")]
            public CommonOptions AvailableOptions { get; set; }

            [VerbOption("list", HelpText="List repositories")]
            public CommonOptions ListOptions { get; set; }

            [VerbOption("add", HelpText="Add a repository")]
            public AddOptions AddOptions { get; set; }

            [VerbOption("forget", HelpText="Forget a repository")]
            public ForgetOptions ForgetOptions { get; set; }

            [VerbOption("default", HelpText="Set the default repository")]
            public DefaultOptions DefaultOptions { get; set; }
        }

        internal class AvailableOptions : CommonOptions
        {
        }

        internal class ListOptions : CommonOptions
        {
        }

        internal class AddOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }

            [ValueOption(1)]
            public string uri { get; set; }
        }

        internal class DefaultOptions : CommonOptions
        {
            [ValueOption(0)]
            public string uri { get; set; }
        }

        internal class ForgetOptions : CommonOptions
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
                    case "remove":
                        args[i] = "forget";
                        break;

                    default:
                        break;
                }
            }

            #endregion
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new RepoSubOptions (), Parse);

            // That line above will have set our 'option' and 'suboption' fields.

            switch (option)
            {
                case "available":
                    return AvailableRepositories();

                case "list":
                    return ListRepositories();

                case "add":
                    return AddRepository((AddOptions)suboptions);

                case "forget":
                    return ForgetRepository((ForgetOptions)suboptions);

                case "default":
                    return DefaultRepository((DefaultOptions)suboptions);

                default:
                    User.RaiseMessage("Unknown command: repo {0}", option);
                    return Exit.BADOPT;
            }
        }

        private int AvailableRepositories()
        {
            return Exit.OK;
        }

        private int ListRepositories()
        {
            return Exit.OK;
        }

        private int AddRepository(AddOptions options)
        {
            return Exit.OK;
        }

        private int ForgetRepository(ForgetOptions options)
        {
            return Exit.OK;
        }

        private int DefaultRepository(DefaultOptions options)
        {
            return Exit.OK;
        }
	}
}

