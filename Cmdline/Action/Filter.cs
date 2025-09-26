using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

using CKAN.Games;

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
        public Filter(GameInstanceManager mgr,
                      IUser               user)
        {
            manager   = mgr;
            this.user = user;
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
            Parser.Default.ParseArgumentsStrict(args, new FilterSubOptions(), (option, suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    exitCode = options.Handle(manager, user);
                    if (exitCode == Exit.OK)
                    {
                        try
                        {
                            exitCode = suboptions switch
                            {
                                FilterListOptions   opts => ListFilters(opts),
                                FilterAddOptions    opts => AddFilters(opts, option),
                                FilterRemoveOptions opts => RemoveFilters(opts, option),
                                _                        => UnknownCommand(option),
                            };
                        }
                        catch (Kraken k)
                        {
                            user.RaiseError("{0}", k.Message);
                            exitCode = Exit.BADOPT;
                        }
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private int ListFilters(FilterListOptions opts)
        {
            int exitCode = opts.Handle(manager, user);
            if (exitCode != Exit.OK)
            {
                return exitCode;
            }

            var instance = MainClass.GetGameInstance(manager);
            var game = GetGame(opts.gameId, instance);
            user.RaiseMessage(Properties.Resources.FilterListGlobalHeader,
                               game.ShortName);
            foreach (string filter in manager.Configuration
                                             .GetGlobalInstallFilters(game))
            {
                user.RaiseMessage("\t- {0}", filter);
            }
            user.RaiseMessage("");

            user.RaiseMessage(Properties.Resources.FilterListInstanceHeader,
                              instance.Name);
            foreach (string filter in instance.InstallFilters)
            {
                user.RaiseMessage("\t- {0}", filter);
            }
            return Exit.OK;
        }

        private int AddFilters(FilterAddOptions opts, string verb)
        {
            if (opts.filters?.Count < 1)
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
            if (opts.global)
            {
                var game = GetGame(opts.gameId, instance);
                var duplicates = manager.Configuration
                                        .GetGlobalInstallFilters(game)
                                        .Intersect(opts.filters ?? Enumerable.Empty<string>())
                                        .ToArray();
                if (duplicates.Length > 0)
                {
                    user.RaiseError(Properties.Resources.FilterAddGlobalDuplicateError,
                                    string.Join(", ", duplicates));
                    return Exit.BADOPT;
                }
                else
                {
                    manager.Configuration.SetGlobalInstallFilters(
                        game,
                        manager.Configuration
                               .GetGlobalInstallFilters(game)
                               .Concat(opts.filters ?? Enumerable.Empty<string>())
                               .Distinct()
                               .ToArray());
                }
            }
            else
            {
                var duplicates = instance.InstallFilters
                                         .Intersect(opts.filters ?? Enumerable.Empty<string>())
                                         .ToArray();
                    if (duplicates.Length > 0)
                    {
                        user.RaiseError(Properties.Resources.FilterAddInstanceDuplicateError,
                                         string.Join(", ", duplicates));
                        return Exit.BADOPT;
                    }
                    else
                    {
                        instance.InstallFilters = instance.InstallFilters
                            .Concat(opts.filters ?? Enumerable.Empty<string>())
                            .Distinct()
                            .ToArray();
                    }
            }
            return Exit.OK;
        }

        private int RemoveFilters(FilterRemoveOptions opts, string verb)
        {
            if (opts.filters?.Count < 1)
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
            if (opts.global)
            {
                var game = GetGame(opts.gameId, instance);
                var notFound = (opts.filters ?? Enumerable.Empty<string>())
                    .Except(manager.Configuration.GetGlobalInstallFilters(game))
                    .ToArray();
                if (notFound.Length > 0)
                {
                    user.RaiseError(Properties.Resources.FilterRemoveGlobalNotFoundError,
                                    string.Join(", ", notFound));
                    return Exit.BADOPT;
                }
                else
                {
                    manager.Configuration.SetGlobalInstallFilters(
                        game,
                        manager.Configuration.GetGlobalInstallFilters(game)
                           .Except(opts.filters ?? Enumerable.Empty<string>())
                           .ToArray());
                }
            }
            else
            {
                var notFound = (opts.filters ?? Enumerable.Empty<string>())
                    .Except(instance.InstallFilters)
                    .ToArray();
                if (notFound.Length > 0)
                {
                    user.RaiseError(Properties.Resources.FilterRemoveInstanceNotFoundError,
                                    string.Join(", ", notFound));
                    return Exit.BADOPT;
                }
                else
                {
                    instance.InstallFilters = instance.InstallFilters
                                                      .Except(opts.filters ?? Enumerable.Empty<string>())
                                                      .ToArray();
                }
            }
            return Exit.OK;
        }

        [ExcludeFromCodeCoverage]
        private int UnknownCommand(string option)
        {
            user.RaiseMessage("{0}: filter {1}",
                              Properties.Resources.UnknownCommand, option);
            return Exit.BADOPT;
        }

        private static IGame GetGame(string? gameId, CKAN.GameInstance instance)
        {
            if (gameId != null)
            {
                if (KnownGames.GameByShortName(gameId.ToUpper()) is IGame game)
                {
                    return game;
                }
                throw new Kraken(string.Format(Properties.Resources.FilterGameNotFoundError,
                                               gameId,
                                               string.Join(" ", KnownGames.AllGameShortNames())));
            }
            return instance.Game;
        }

        [ExcludeFromCodeCoverage]
        private void PrintUsage(string verb)
        {
            foreach (var h in FilterSubOptions.GetHelp(verb))
            {
                user.RaiseError("{0}", h);
            }
        }

        private readonly GameInstanceManager manager;
        private readonly IUser               user;
    }

    [ExcludeFromCodeCoverage]
    internal class FilterSubOptions : VerbCommandOptions
    {
        [VerbOption("list", HelpText = "List install filters")]
        public FilterListOptions? FilterListOptions { get; set; }

        [VerbOption("add", HelpText = "Add install filters")]
        public FilterAddOptions? FilterAddOptions { get; set; }

        [VerbOption("remove", HelpText = "Remove install filters")]
        public FilterRemoveOptions? FilterRemoveOptions { get; set; }

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
        [Option("game", DefaultValue = null,
                HelpText = "The game for which to list global filters, either KSP or KSP2")]
        public string? gameId { get; set; }
    }

    internal class FilterAddOptions : InstanceSpecificOptions
    {
        [Option("global", DefaultValue = false, HelpText = "Add global filters")]
        public bool global { get; set; }

        [Option("game", DefaultValue = null,
                HelpText = "The game for which to set global filters, either KSP or KSP2")]
        public string? gameId { get; set; }

        [ValueList(typeof(List<string>))]
        public List<string>? filters { get; set; }
    }

    internal class FilterRemoveOptions : InstanceSpecificOptions
    {
        [Option("global", DefaultValue = false, HelpText = "Remove global filters")]
        public bool global { get; set; }

        [Option("game", DefaultValue = null,
                HelpText = "The game for which to remove global filters, either KSP or KSP2")]
        public string? gameId { get; set; }

        [ValueList(typeof(List<string>))]
        public List<string>? filters { get; set; }
    }

}
