using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

namespace CKAN.CmdLine
{
    /// <summary>
    /// Subcommand for setting flags on modules,
    /// currently the auto-installed flag
    /// </summary>
    public class Mark : ISubCommand
    {
        /// <summary>
        /// Initialize the subcommand
        /// </summary>
        public Mark(GameInstanceManager   mgr,
                    RepositoryDataManager repoData,
                    IUser                 user)
        {
            manager       = mgr;
            this.repoData = repoData;
            this.user     = user;
        }

        /// <summary>
        /// Run the subcommand
        /// </summary>
        /// <param name="mgr">Manager to provide game instances</param>
        /// <param name="opts">Command line parameters paritally handled by parser</param>
        /// <param name="unparsed">Command line parameters not yet handled by parser</param>
        /// <returns>
        /// Exit code
        /// </returns>
        public int RunSubCommand(CommonOptions?    opts,
                                 SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();
            int exitCode = Exit.OK;
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new MarkSubOptions(), (option, suboptions) =>
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
                        case "auto":
                            exitCode = MarkAuto((MarkAutoOptions)suboptions,
                                                true,
                                                option,
                                                Properties.Resources.MarkAutoInstalled,
                                                manager);
                            break;

                        case "user":
                            exitCode = MarkAuto((MarkAutoOptions)suboptions,
                                                false,
                                                option,
                                                Properties.Resources.MarkUserSelected,
                                                manager);
                            break;

                        default:
                            user.RaiseMessage(Properties.Resources.MarkUnknownCommand, option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private int MarkAuto(MarkAutoOptions     opts,
                             bool                value,
                             string              verb,
                             string              descrip,
                             GameInstanceManager manager)
        {
            if (opts.modules == null || opts.modules.Count < 1)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage(verb);
                return Exit.BADOPT;
            }

            int exitCode = opts.Handle(manager, user);
            if (exitCode != Exit.OK)
            {
                return exitCode;
            }

            var instance = MainClass.GetGameInstance(manager);
            var regMgr = RegistryManager.Instance(instance, repoData);
            bool needSave = false;
            if (opts.modules != null)
            {
                Search.AdjustModulesCase(instance, regMgr.registry, opts.modules);
                foreach (string id in opts.modules)
                {
                    var im = regMgr.registry.InstalledModule(id);
                    if (im == null)
                    {
                        user.RaiseError(Properties.Resources.MarkNotInstalled, id);
                    }
                    else if (im.AutoInstalled == value)
                    {
                        user.RaiseError(Properties.Resources.MarkAlready, id, descrip);
                    }
                    else
                    {
                        user.RaiseMessage(Properties.Resources.Marking, id, descrip);
                        try
                        {
                            im.AutoInstalled = value;
                            needSave = true;
                        }
                        catch (ModuleIsDLCKraken kraken)
                        {
                            user.RaiseMessage(Properties.Resources.MarkDLC, kraken.module.name);
                            return Exit.BADOPT;
                        }
                    }
                }
                if (needSave)
                {
                    regMgr.Save(false);
                    user.RaiseMessage(Properties.Resources.MarkChanged);
                }
                return Exit.OK;
            }
            return Exit.ERROR;
        }

        [ExcludeFromCodeCoverage]
        private void PrintUsage(string verb)
        {
            foreach (var h in MarkSubOptions.GetHelp(verb))
            {
                user.RaiseError("{0}", h);
            }
        }

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private readonly IUser                 user;
    }

    [ExcludeFromCodeCoverage]
    internal class MarkSubOptions : VerbCommandOptions
    {
        [VerbOption("auto", HelpText = "Mark modules as auto installed")]
        public MarkAutoOptions? MarkAutoOptions { get; set; }

        [VerbOption("user", HelpText = "Mark modules as user selected (opposite of auto installed)")]
        public MarkAutoOptions? MarkUserOptions { get; set; }

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
                yield return $"ckan mark - {Properties.Resources.MarkHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan mark <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "mark " + verb + " - " + GetDescription(typeof(MarkSubOptions), verb);
                switch (verb)
                {
                    case "auto":
                        yield return $"{Properties.Resources.Usage}: ckan mark {verb} [{Properties.Resources.Options}] Mod [Mod2 ...]";
                        break;

                    case "user":
                        yield return $"{Properties.Resources.Usage}: ckan mark {verb} [{Properties.Resources.Options}] Mod [Mod2 ...]";
                        break;
                }
            }
        }
    }

    internal class MarkAutoOptions : InstanceSpecificOptions
    {
        [ValueList(typeof(List<string>))]
        [InstalledIdentifiers]
        public List<string>? modules { get; set; }
    }
}
