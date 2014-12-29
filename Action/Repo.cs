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

            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new RepoSubOptions (), Parse);

            // That line above will have set our 'option' and 'suboption' fields.

            switch (option)
            {

                default:
                    User.RaiseMessage("Unknown command: repo {0}", option);
                    return Exit.BADOPT;
            }
		}
	}
}

