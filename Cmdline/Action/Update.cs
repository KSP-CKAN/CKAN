using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using CommandLine;

using CKAN.Games;

namespace CKAN.CmdLine
{
    // Does not always need an instance, so this is not an ICommand
    public class Update
    {
        /// <summary>
        /// Initialize the update command object
        /// </summary>
        /// <param name="mgr">GameInstanceManager containing our instances</param>
        /// <param name="user">IUser object for interaction</param>
        public Update(RepositoryDataManager repoData, IUser user, GameInstanceManager manager)
        {
            this.repoData = repoData;
            this.user     = user;
            this.manager  = manager;
        }

        /// <summary>
        /// Update the registry
        /// </summary>
        /// <param name="instance">Game instance to update</param>
        /// <param name="raw_options">Command line options object</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunCommand(object raw_options, IGame? game)
        {
            UpdateOptions options = (UpdateOptions) raw_options;

            try
            {
                if (options.repositoryURLs != null || options.game != null)
                {
                    if (game == null)
                    {
                        user.RaiseError(Properties.Resources.UpdateBadGame,
                                        options.game ?? "",
                                        string.Join(", ", KnownGames.AllGameShortNames()));
                        return Exit.BADOPT;
                    }
                    var repos = (options.repositoryURLs ?? Enumerable.Empty<string>())
                                        .Select((url, i) =>
                                            new Repository(string.Format(Properties.Resources.UpdateURLRepoName,
                                                                         i + 1),
                                                           getUri(url)))
                                        .DefaultIfEmpty(Repository.DefaultGameRepo(game))
                                        .ToArray();
                    if (options.list_changes)
                    {
                        var availablePrior = repoData.GetAllAvailableModules(repos)
                                                     .Select(am => am.Latest(ReleaseStatus.stable))
                                                     .OfType<CkanModule>()
                                                     .ToList();
                        UpdateRepositories(game, repos, options.NetUserAgent, options.force);
                        PrintChanges(availablePrior,
                                     repoData.GetAllAvailableModules(repos)
                                             .Select(am => am.Latest(ReleaseStatus.stable))
                                             .OfType<CkanModule>()
                                             .ToList());
                    }
                    else
                    {
                        UpdateRepositories(game, repos, options.NetUserAgent, options.force);
                    }
                }
                else
                {
                    var instance = MainClass.GetGameInstance(manager);
                    var stabilityTolerance = instance.StabilityToleranceConfig;
                    if (options.list_changes)
                    {
                        // Get a list of compatible modules prior to the update.
                        var registry = RegistryManager.Instance(instance, repoData).registry;
                        var crit     = instance.VersionCriteria();
                        var compatible_prior = registry.CompatibleModules(stabilityTolerance, crit).ToList();
                        UpdateRepositories(instance, options.NetUserAgent, options.force);
                        PrintChanges(compatible_prior,
                                     registry.CompatibleModules(stabilityTolerance, crit).ToList());
                    }
                    else
                    {
                        UpdateRepositories(instance, options.NetUserAgent, options.force);
                    }
                }
            }
            catch (MissingCertificateKraken kraken)
            {
                user.RaiseMessage("{0}", kraken.Message);
                return Exit.ERROR;
            }

            return Exit.OK;
        }

        /// <summary>
        /// Locates the changes between the prior and post state of the modules..
        /// </summary>
        /// <param name="modules_prior">List of the compatible modules prior to the update.</param>
        /// <param name="modules_post">List of the compatible modules after the update.</param>
        private void PrintChanges(List<CkanModule> modules_prior,
                                  List<CkanModule> modules_post)
        {
            var prior = new HashSet<CkanModule>(modules_prior, new NameComparer());
            var post  = new HashSet<CkanModule>(modules_post,  new NameComparer());

            var added   = new HashSet<CkanModule>(post.Except(prior, new NameComparer()));
            var removed = new HashSet<CkanModule>(prior.Except(post, new NameComparer()));

            // Default compare includes versions
            var unchanged = post.Intersect(prior);
            var updated   = post.Except(unchanged)
                                .Except(added)
                                .Except(removed)
                                .ToList();

            // Print the changes.
            user.RaiseMessage(Properties.Resources.UpdateChangesSummary,
                              added.Count, removed.Count, updated.Count);

            if (added.Count > 0)
            {
                PrintModules(Properties.Resources.UpdateAddedHeader, added);
            }

            if (removed.Count > 0)
            {
                PrintModules(Properties.Resources.UpdateRemovedHeader, removed);
            }

            if (updated.Count > 0)
            {
                PrintModules(Properties.Resources.UpdateUpdatedHeader, updated);
            }
        }

        /// <summary>
        /// Prints a message and a list of modules. Ends with a blank line.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="modules">The modules to list.</param>
        private void PrintModules(string message, IEnumerable<CkanModule> modules)
        {
            // Check input.
            if (message == null)
            {
                throw new BadCommandKraken("Message cannot be null.");
            }

            if (modules == null)
            {
                throw new BadCommandKraken("List of modules cannot be null.");
            }

            user.RaiseMessage("{0}", message);

            foreach (CkanModule module in modules.OrderBy(m => m.name))
            {
                user.RaiseMessage("{0} ({1})", module.name, module.identifier);
            }

            user.RaiseMessage("");
        }

        /// <summary>
        /// Updates repositories for a game instance
        /// </summary>
        /// <param name="instance">The game instance to work on.</param>
        /// <param name="repository">Repository to update. If null all repositories are used.</param>
        private void UpdateRepositories(CKAN.GameInstance instance, string? userAgent, bool force = false)
        {
            var registry = RegistryManager.Instance(instance, repoData).registry;
            var result = repoData.Update(registry.Repositories.Values.ToArray(),
                                         instance.Game, force,
                                         new NetAsyncDownloader(user, () => null, userAgent), user, userAgent);
            if (result == RepositoryDataManager.UpdateResult.Updated)
            {
                user.RaiseMessage(Properties.Resources.UpdateSummary,
                                  registry.CompatibleModules(instance.StabilityToleranceConfig,
                                                             instance.VersionCriteria())
                                          .Count());
            }
        }

        /// <summary>
        /// Updates repositories
        /// </summary>
        /// <param name="game">The game for which the URLs contain metadata</param>
        /// <param name="repos">Repositories to update</param>
        private void UpdateRepositories(IGame game, Repository[] repos, string? userAgent, bool force = false)
        {
            var result = repoData.Update(repos, game, force,
                                         new NetAsyncDownloader(user, () => null, userAgent),
                                         user, userAgent);
            if (result == RepositoryDataManager.UpdateResult.Updated)
            {
                user.RaiseMessage(Properties.Resources.UpdateSummary,
                                  repoData.GetAllAvailableModules(repos).Count());
            }
        }

        private static Uri getUri(string arg)
            => Uri.IsWellFormedUriString(arg, UriKind.Absolute)
                ? new Uri(arg)
                : File.Exists(arg)
                    ? new Uri(Path.GetFullPath(arg))
                    : throw new FileNotFoundKraken(arg);

        private readonly RepositoryDataManager repoData;
        private readonly IUser                 user;
        private readonly GameInstanceManager   manager;
    }

    internal class UpdateOptions : InstanceSpecificOptions
    {
        [Option('l', "list-changes", DefaultValue = false, HelpText = "List new and removed modules")]
        public bool list_changes { get; set; }

        // Can't specify DefaultValue here because we want to fall back to instance-based updates when omitted
        [Option('g', "game", HelpText = "Game for which to update repositories")]
        public string? game { set; get; }

        [OptionArray('u', "urls", HelpText = "URLs of repositories to update")]
        public string[]? repositoryURLs { get; set; }

        [Option('f', "force", DefaultValue = false, HelpText = "Download and parse metadata even if it hasn't changed")]
        public bool force { get; set; }
    }

}
