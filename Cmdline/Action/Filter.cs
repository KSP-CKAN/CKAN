using System.Linq;
using System.Collections.Generic;

using Autofac;
using CommandLine;
using CommandLine.Text;

using CKAN.Configuration;

namespace CKAN.CmdLine
{
    /// <summary>
    /// Subcommand for managing installation filters
    /// </summary>
    public class Filter : ISubCommand
    {
        /// <summary>
        /// Initialize the subcommand
        /// </summary>
        public Filter() { }

        /// <summary>
        /// Run the subcommand
        /// </summary>
        /// <param name="mgr">Manager to provide game instances</param>
        /// <param name="opts">Command line parameters paritally handled by parser</param>
        /// <param name="unparsed">Command line parameters not yet handled by parser</param>
        /// <returns>
        /// Exit code
        /// </returns>
        public int RunSubCommand(GameInstanceManager mgr, CommonOptions opts, SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();
            int exitCode = Exit.OK;
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new FilterSubOptions(), (string option, object suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    user     = new ConsoleUser(options.Headless);
                    manager  = mgr ?? new GameInstanceManager(user);
                    exitCode = options.Handle(manager, user);
                    if (exitCode != Exit.OK)
                    {
                        return;
                    }

                    switch (option)
                    {
                        case "list":
                            exitCode = ListFilters((FilterListOptions)suboptions);
                            break;

                        case "add":
                            exitCode = AddFilters((FilterAddOptions)suboptions, option);
                            break;

                        case "remove":
                            exitCode = RemoveFilters((FilterRemoveOptions)suboptions, option);
                            break;

                        default:
                            user.RaiseMessage("{0}: filter {1}", Properties.Resources.UnknownCommand, option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private int ListFilters(FilterListOptions opts)
        {
            int exitCode = opts.Handle(manager, user);
            if (exitCode != Exit.OK)
            {
                return exitCode;
            }

            var cfg = ServiceLocator.Container.Resolve<IConfiguration>();
            user.RaiseMessage(Properties.Resources.FilterListGlobalHeader);
            foreach (string filter in cfg.GlobalInstallFilters)
            {
                user.RaiseMessage("\t- {0}", filter);
            }
            user.RaiseMessage("");

            var instance = MainClass.GetGameInstance(manager);
            user.RaiseMessage(Properties.Resources.FilterListInstanceHeader);
            foreach (string filter in instance.InstallFilters)
            {
                user.RaiseMessage("\t- {0}", filter);
            }
            return Exit.OK;
        }

        private int AddFilters(FilterAddOptions opts, string verb)
        {
            if (opts.filters.Count < 1)
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

            if (opts.global)
            {
                var cfg = ServiceLocator.Container.Resolve<IConfiguration>();
                var duplicates = cfg.GlobalInstallFilters
                    .Intersect(opts.filters)
                    .ToArray();
                if (duplicates.Length > 0)
                {
                    user.RaiseError(
                        Properties.Resources.FilterAddGlobalDuplicateError,
                        string.Join(", ", duplicates)
                    );
                    return Exit.BADOPT;
                }
                else
                {
                    cfg.GlobalInstallFilters = cfg.GlobalInstallFilters
                        .Concat(opts.filters)
                        .Distinct()
                        .ToArray();
                }
            }
            else
            {
                var instance = MainClass.GetGameInstance(manager);
                var duplicates = instance.InstallFilters
                    .Intersect(opts.filters)
                    .ToArray();
                    if (duplicates.Length > 0)
                    {
                        user.RaiseError(
                            Properties.Resources.FilterAddInstanceDuplicateError,
                            string.Join(", ", duplicates)
                        );
                        return Exit.BADOPT;
                    }
                    else
                    {
                        instance.InstallFilters = instance.InstallFilters
                            .Concat(opts.filters)
                            .Distinct()
                            .ToArray();
                    }
            }
            return Exit.OK;
        }

        private int RemoveFilters(FilterRemoveOptions opts, string verb)
        {
            if (opts.filters.Count < 1)
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

            if (opts.global)
            {
                var cfg = ServiceLocator.Container.Resolve<IConfiguration>();
                var notFound = opts.filters
                    .Except(cfg.GlobalInstallFilters)
                    .ToArray();
                if (notFound.Length > 0)
                {
                    user.RaiseError(
                        Properties.Resources.FilterRemoveGlobalNotFoundError,
                        string.Join(", ", notFound)
                    );
                    return Exit.BADOPT;
                }
                else
                {
                    cfg.GlobalInstallFilters = cfg.GlobalInstallFilters
                        .Except(opts.filters)
                        .ToArray();
                }
            }
            else
            {
                var instance = MainClass.GetGameInstance(manager);
                var notFound = opts.filters
                    .Except(instance.InstallFilters)
                    .ToArray();
                if (notFound.Length > 0)
                {
                    user.RaiseError(
                        Properties.Resources.FilterRemoveInstanceNotFoundError,
                        string.Join(", ", notFound)
                    );
                    return Exit.BADOPT;
                }
                else
                {
                    instance.InstallFilters = instance.InstallFilters
                        .Except(opts.filters)
                        .ToArray();
                }
            }
            return Exit.OK;
        }

        private void PrintUsage(string verb)
        {
            foreach (var h in FilterSubOptions.GetHelp(verb))
            {
                user.RaiseError(h);
            }
        }

        private GameInstanceManager manager;
        private IUser               user;
    }

    internal class FilterSubOptions : VerbCommandOptions
    {
        [VerbOption("list", HelpText = "List install filters")]
        public FilterListOptions FilterListOptions { get; set; }

        [VerbOption("add", HelpText = "Add install filters")]
        public FilterAddOptions FilterAddOptions { get; set; }

        [VerbOption("remove", HelpText = "Remove install filters")]
        public FilterRemoveOptions FilterRemoveOptions { get; set; }

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
                yield return $"ckan filter - {Properties.Resources.FilterHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan filter <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "filter " + verb + " - " + GetDescription(typeof(FilterSubOptions), verb);
                switch (verb)
                {
                    case "list":
                        yield return $"{Properties.Resources.Usage}: ckan filter {verb}";
                        break;

                    case "add":
                        yield return $"{Properties.Resources.Usage}: ckan filter {verb} [{Properties.Resources.Options}] filter1 [filter2 ...]";
                        break;

                    case "remove":
                        yield return $"{Properties.Resources.Usage}: ckan filter {verb} [{Properties.Resources.Options}] filter1 [filter2 ...]";
                        break;
                }
            }
        }
    }

    internal class FilterListOptions : InstanceSpecificOptions
    {
    }

    internal class FilterAddOptions : InstanceSpecificOptions
    {
        [Option("global", DefaultValue = false, HelpText = "Add global filters")]
        public bool global { get; set; }

        [ValueList(typeof(List<string>))]
        public List<string> filters { get; set; }
    }

    internal class FilterRemoveOptions : InstanceSpecificOptions
    {
        [Option("global", DefaultValue = false, HelpText = "Remove global filters")]
        public bool global { get; set; }

        [ValueList(typeof(List<string>))]
        public List<string> filters { get; set; }
    }

}
