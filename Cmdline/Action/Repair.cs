﻿using CommandLine;

namespace CKAN.CmdLine
{
    public class Repair : ISubCommand
    {
        public CKAN.KSP CurrentInstance { get; set; }
        public IUser User { get; set; }
        public string option;
        public object suboptions;

        internal class RepairSubOptions : CommonOptions
        {
            [VerbOption("registry", HelpText="Try to repair the CKAN registry")]
            public CommonOptions Registry { get; set; }
        }

        public Repair(CKAN.KSP current_instance,IUser user)
        {
            CurrentInstance = current_instance;
            User = user;
        }

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
            Parser.Default.ParseArgumentsStrict(args, new RepairSubOptions (), Parse);

            switch (option)
            {
                case "registry":
                    return Registry();
            }

            throw new BadCommandKraken("Unknown command: repair " + option);
        }

        public void Parse(string option, object suboptions)
        {
            this.option = option;
            this.suboptions = suboptions;
        }

        /// <summary>
        /// Try to repair our registry.
        /// </summary>
        private int Registry()
        {
            var manager = RegistryManager.Instance(CurrentInstance);
            manager.registry.Repair();
            manager.Save();
            User.RaiseMessage("Registry repairs attempted. Hope it helped.");
            return Exit.OK;
        }
    }
}

