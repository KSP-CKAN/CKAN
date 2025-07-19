using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;

namespace CKAN.CmdLine
{
    internal class RepairSubOptions : VerbCommandOptions
    {
        [VerbOption("registry", HelpText = "Try to repair the CKAN registry")]
        public InstanceSpecificOptions? Registry { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var ht = HelpText.AutoBuild(this, verb);
            foreach (var h in GetHelp(verb))
            {
                ht.AddPreOptionsLine(h);
            }
            return ht;
        }

        public static IEnumerable<string> GetHelp(string verb)
        {
            // Add a usage prefix line
            yield return " ";
            if (string.IsNullOrEmpty(verb))
            {
                yield return $"ckan repair - {Properties.Resources.RepairHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan repair <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "repair " + verb + " - " + GetDescription(typeof(RepairSubOptions), verb);
                switch (verb)
                {
                    // Commands with only --flag type options
                    case "registry":
                    default:
                        yield return $"{Properties.Resources.Usage}: ckan repair {verb} [{Properties.Resources.Options}]";
                        break;
                }
            }
        }
    }

    public class Repair : ISubCommand
    {
        public Repair(GameInstanceManager   manager,
                      RepositoryDataManager repoData,
                      IUser                 user)
        {
            this.manager  = manager;
            this.repoData = repoData;
            this.user     = user;
        }

        public int RunSubCommand(CommonOptions?    opts,
                                 SubCommandOptions unparsed)
        {
            int exitCode = Exit.OK;
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(unparsed.options.ToArray(), new RepairSubOptions(), (option, suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    exitCode = options.Handle(manager, user);
                    if (exitCode != Exit.OK)
                    {
                        return;
                    }

                    switch (option)
                    {
                        case "registry":
                            exitCode = Registry(RegistryManager.Instance
                                (MainClass.GetGameInstance(manager), repoData));
                            break;

                        default:
                            user.RaiseMessage(Properties.Resources.RepairUnknownCommand, option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private readonly GameInstanceManager   manager;
        private readonly IUser                 user;
        private readonly RepositoryDataManager repoData;

        /// <summary>
        /// Try to repair our registry.
        /// </summary>
        private int Registry(RegistryManager regMgr)
        {
            regMgr.registry.Repair();
            regMgr.Save();
            user.RaiseMessage(Properties.Resources.Repaired);
            return Exit.OK;
        }
    }
}
