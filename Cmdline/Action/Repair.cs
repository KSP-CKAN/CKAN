using CommandLine;
using CommandLine.Text;

namespace CKAN.CmdLine
{
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
                ht.AddPreOptionsLine($"ckan repair - {Properties.Resources.RepairHelpSummary}");
                ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan repair <{Properties.Resources.Command}> [{Properties.Resources.Options}]");
            }
            else
            {
                ht.AddPreOptionsLine("repair " + verb + " - " + GetDescription(verb));
                switch (verb)
                {
                    // Commands with only --flag type options
                    case "registry":
                    default:
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan repair {verb} [{Properties.Resources.Options}]");
                        break;
                }
            }
            return ht;
        }
    }

    public class Repair : ISubCommand
    {
        public Repair(RepositoryDataManager repoData)
        {
            this.repoData = repoData;
        }

        public int RunSubCommand(GameInstanceManager manager, CommonOptions opts, SubCommandOptions unparsed)
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
                        manager = new GameInstanceManager(User);
                    }
                    exitCode = options.Handle(manager, User);
                    if (exitCode != Exit.OK)
                        return;

                    switch (option)
                    {
                        case "registry":
                            exitCode = Registry(RegistryManager.Instance
                                (MainClass.GetGameInstance(manager), repoData));
                            break;

                        default:
                            User.RaiseMessage(Properties.Resources.RepairUnknownCommand, option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private IUser User { get; set; }
        private RepositoryDataManager repoData;

        /// <summary>
        /// Try to repair our registry.
        /// </summary>
        private int Registry(RegistryManager regMgr)
        {
            regMgr.registry.Repair();
            regMgr.Save();
            User.RaiseMessage(Properties.Resources.Repaired);
            return Exit.OK;
        }
    }
}
