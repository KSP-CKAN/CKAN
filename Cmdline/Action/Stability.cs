using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using CommandLine;
using CommandLine.Text;

namespace CKAN.CmdLine
{
    public class Stability : ISubCommand
    {
        public Stability(GameInstanceManager   manager,
                         RepositoryDataManager repoData,
                         IUser                 user)
        {
            this.manager  = manager;
            this.repoData = repoData;
            this.user     = user;
        }

        public int RunSubCommand(CommonOptions?    opts,
                                 SubCommandOptions options)
        {
            int exitCode = Exit.OK;
            Parser.Default.ParseArgumentsStrict(options.options.ToArray(),
                                                new StabilitySubOptions(),
                                                (option, suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    exitCode = options.Handle(manager, user);
                    if (exitCode == Exit.OK)
                    {
                        switch (option)
                        {
                            case "list":
                                exitCode = List(MainClass.GetGameInstance(manager),
                                                user);
                                break;

                            case "set":
                                exitCode = Set((StabilitySetOptions)suboptions,
                                               MainClass.GetGameInstance(manager),
                                               user);
                                break;

                            default:
                                user.RaiseMessage(Properties.Resources.StabilityUnknownCommand,
                                                  option);
                                exitCode = Exit.BADOPT;
                                break;
                        }
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private static int List(CKAN.GameInstance instance, IUser user)
        {
            var stabilityTolerance = instance.StabilityToleranceConfig;
            user.RaiseMessage(Properties.Resources.StabilityOverallLabel,
                              stabilityTolerance.OverallStabilityTolerance);
            var rows = stabilityTolerance.OverriddenModIdentifiers
                                         .Order()
                                         .Select(ident => stabilityTolerance.ModStabilityTolerance(ident)
                                                          is ReleaseStatus relStat
                                                              ? Tuple.Create(ident, relStat.ToString())
                                                              : null)
                                         .OfType<Tuple<string, string>>()
                                         .ToArray();
            if (rows.Length > 0)
            {
                var modHeader       = Properties.Resources.StabilityListModHeader;
                var stabilityHeader = Properties.Resources.StabilityListStabilityHeader;
                var modWidth       = Enumerable.Repeat(modHeader, 1)
                                               .Concat(rows.Select(tuple => tuple.Item1))
                                               .Max(str => str.Length);
                var stabilityWidth = Enumerable.Repeat(stabilityHeader, 1)
                                               .Concat(rows.Select(tuple => tuple.Item2))
                                               .Max(str => str.Length);
                user.RaiseMessage("");
                user.RaiseMessage("{0}  {1}", modHeader.PadRight(modWidth),
                                              stabilityHeader.PadRight(stabilityWidth));
                user.RaiseMessage("{0}  {1}", new string('-', modWidth),
                                              new string('-', stabilityWidth));
                foreach (var (ident, relStat) in rows)
                {
                    user.RaiseMessage("{0}  {1}", ident.PadRight(modWidth),
                                                  relStat.PadRight(stabilityWidth));
                }
            }
            return Exit.OK;
        }

        private int Set(StabilitySetOptions opts, CKAN.GameInstance instance, IUser user)
        {
            var stabilityTolerance = instance.StabilityToleranceConfig;
            if (opts.Identifier == null)
            {
                if (opts.Stability is ReleaseStatus relStat)
                {
                    stabilityTolerance.OverallStabilityTolerance = relStat;
                }
                else
                {
                    user.RaiseError(Properties.Resources.ArgumentMissing);
                    PrintUsage(user, "set");
                    return Exit.BADOPT;
                }
            }
            else
            {
                using (var regMgr = RegistryManager.Instance(instance, repoData))
                {
                    var registry = regMgr.registry;
                    var idents   = new List<string> { opts.Identifier };
                    Search.AdjustModulesCase(instance, registry, idents);
                    stabilityTolerance.SetModStabilityTolerance(idents[0], opts.Stability);
                }
            }
            List(instance, user);
            return Exit.OK;
        }

        [ExcludeFromCodeCoverage]
        private static void PrintUsage(IUser user, string verb)
        {
            foreach (var h in StabilitySubOptions.GetHelp(verb))
            {
                user.RaiseError("{0}", h);
            }
        }

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private readonly IUser                 user;
    }

    [ExcludeFromCodeCoverage]
    public class StabilitySubOptions: VerbCommandOptions
    {
        [VerbOption("list", HelpText = "Print stability preferences")]
        public InstanceSpecificOptions? ListOptions { get; set; }

        [VerbOption("set", HelpText = "Change stability preferences")]
        public StabilitySetOptions? SetOptions { get; set; }

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

        [ExcludeFromCodeCoverage]
        public static IEnumerable<string> GetHelp(string verb)
        {
            // Add a usage prefix line
            yield return " ";
            if (string.IsNullOrEmpty(verb))
            {
                yield return $"ckan stability - {Properties.Resources.StabilityHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan stability <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "stability " + verb + " - " + GetDescription(typeof(StabilitySubOptions), verb);
                switch (verb)
                {
                    // Commands with one argument
                    case "set":
                        yield return $"{Properties.Resources.Usage}: ckan stability {verb} [{Properties.Resources.Options}] release_status";
                        break;

                    // Commands with only --flag type options
                    case "list":
                    default:
                        yield return $"{Properties.Resources.Usage}: ckan stability {verb} [{Properties.Resources.Options}]";
                        break;
                }
            }
        }
    }

    public class StabilitySetOptions : InstanceSpecificOptions
    {
        [Option("mod", HelpText = "Identifier of mod to override")]
        public string? Identifier { get; set; }

        [ValueOption(0)]
        public ReleaseStatus? Stability { get; set; }
    }
}
