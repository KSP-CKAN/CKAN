using CommandLine;
using CommandLine.Text;

namespace CKAN.CmdLine
{
    public class Repair : ISubCommand
    {
        public Repair() { }

        internal class RepairSubOptions : VerbCommandOptions
        {
            [VerbOption("registry", HelpText = "Try to repair the CKAN registry")]
            public InstanceSpecificOptions Registry { get; set; }

            [HelpVerbOption]
            public string GetUsage(string verb)
            {
                HelpText ht = HelpText.AutoBuild(this, verb);
                // Add a usage prefix line
                ht.AddPreOptionsLine(" ");
                if (string.IsNullOrEmpty(verb))
                {
                    ht.AddPreOptionsLine("ckan repair - Attempt various automatic repairs");
                    ht.AddPreOptionsLine($"Usage: ckan repair <command> [options]");
                }
                else
                {
                    ht.AddPreOptionsLine("repair " + verb + " - " + GetDescription(verb));
                    switch (verb)
                    {
                        // Commands with only --flag type options
                        case "registry":
                        default:
                            ht.AddPreOptionsLine($"Usage: ckan repair {verb} [options]");
                            break;
                    }
                }
                return ht;
            }
        }

        public int RunSubCommand(KSPManager manager, CommonOptions opts, SubCommandOptions unparsed)
        {
            int exitCode = Exit.OK;
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(unparsed.options.ToArray(), new RepairSubOptions(), (string option, object suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    User = new ConsoleUser(options.Headless);
                    if (manager == null)
                    {
                        manager = new KSPManager(User);
                    }
                    exitCode = options.Handle(manager, User);
                    if (exitCode != Exit.OK)
                        return;

                    switch (option)
                    {
                        case "registry":
                            exitCode = Registry(MainClass.GetGameInstance(manager));
                            break;

                        default:
                            User.RaiseMessage("Unknown command: repair {0}", option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private IUser User { get; set; }

        /// <summary>
        /// Try to repair our registry.
        /// </summary>
        private int Registry(CKAN.KSP ksp)
        {
            RegistryManager manager = RegistryManager.Instance(ksp);
            manager.registry.Repair();
            manager.Save();
            User.RaiseMessage("Registry repairs attempted. Hope it helped.");
            return Exit.OK;
        }
    }
}
