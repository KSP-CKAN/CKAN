using System;
using CommandLine;

namespace CKAN.CmdLine
{
    public class Repair : ISubCommand
    {
        public string option;
        public object suboptions;

        internal class RepairSubOptions : CommonOptions
        {
            [VerbOption("registry", HelpText="Try to repair the CKAN registry")]
            public CommonOptions Registry { get; set; }
        }

        public Repair() {}

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
            Parser.Default.ParseArgumentsStrict(args, new RepairSubOptions (), Parse, null);

            switch (option)
            {
                case "installs":
                    return Installs();
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
        /// Repair the list of installs in the registry by removing invalid keys.
        /// </summary>
        private int Installs()
        {
            KSPManager.RemoveMissing();
            Console.WriteLine("Removed invalid instance registry keys.");
            return Exit.OK;
        }

        /// <summary>
        /// Try to repair our registry.
        /// </summary>
        private int Registry()
        {
            KSPManager.CurrentInstance.Registry.Repair();
            KSPManager.CurrentInstance.RegistryManager.Save();
            Console.WriteLine("Registry repairs attempted. Hope it helped.");
            return Exit.OK;
        }
    }
}

