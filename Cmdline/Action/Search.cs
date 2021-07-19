using System;
using System.Collections.Generic;
using System.Linq;
using CKAN.Games;
using CKAN.Versioning;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for searching mods.
    /// </summary>
    public class Search : ICommand
    {
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Search"/> class.
        /// </summary>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Search(IUser user)
        {
            _user = user;
        }

        /// <summary>
        /// Run the 'search' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (SearchOptions)args;
            if (string.IsNullOrWhiteSpace(opts.SearchTerm) && string.IsNullOrWhiteSpace(opts.Author))
            {
                _user.RaiseMessage("search <search term> - argument missing, perhaps you forgot it?");
                _user.RaiseMessage("If you just want to search by author, use:   ckan search --author <author>");
                return Exit.BadOpt;
            }

            var matchingCompatible = PerformSearch(inst, opts.SearchTerm, opts.Author);
            var matchingIncompatible = new List<CkanModule>();
            if (opts.All)
            {
                matchingIncompatible = PerformSearch(inst, opts.SearchTerm, opts.Author, true);
            }

            string message;

            // Return if no mods were found
            if (!matchingCompatible.Any() && !matchingIncompatible.Any())
            {
                message = "Couldn't find any mod";

                if (!string.IsNullOrWhiteSpace(opts.SearchTerm))
                {
                    message += string.Format(" matching \"{0}\"", opts.SearchTerm);
                }

                if (!string.IsNullOrWhiteSpace(opts.Author))
                {
                    message += string.Format(" by \"{0}\"", opts.Author);
                }

                _user.RaiseMessage("{0}.", message);
                return Exit.Ok;
            }

            // Show how many matches we have
            message = string.Format("Found {0} compatible", matchingCompatible.Count.ToString());

            if (opts.All)
            {
                message += string.Format(" and {0} incompatible", matchingIncompatible.Count.ToString());
            }

            message += " mods";

            if (!string.IsNullOrWhiteSpace(opts.SearchTerm))
            {
                message += string.Format(" matching \"{0}\"", opts.SearchTerm);
            }

            if (!string.IsNullOrWhiteSpace(opts.Author))
            {
                message += string.Format(" by \"{0}\"", opts.Author);
            }

            _user.RaiseMessage("{0}.", message);

            // Show detailed information of matches
            if (opts.Detail)
            {
                if (matchingCompatible.Any())
                {
                    _user.RaiseMessage("Matching compatible mods:");
                    foreach (var mod in matchingCompatible)
                    {
                        _user.RaiseMessage("* {0} ({1}) - {2} by {3} - {4}",
                            mod.identifier,
                            mod.version,
                            mod.name,
                            mod.author == null ? "N/A" : string.Join(", ", mod.author),
                            mod.@abstract);
                    }
                }

                if (matchingIncompatible.Any())
                {
                    _user.RaiseMessage("Matching incompatible mods:");
                    foreach (var mod in matchingIncompatible)
                    {
                        Registry.GetMinMaxVersions(new List<CkanModule> { mod }, out _, out _, out GameVersion minVer, out GameVersion maxVer);
                        var gameVersion = GameVersionRange.VersionSpan(inst.game, minVer, maxVer);

                        _user.RaiseMessage("* {0} ({1} - {2}) - {3} by {4} - {5}",
                            mod.identifier,
                            mod.version,
                            gameVersion,
                            mod.name,
                            mod.author == null ? "N/A" : string.Join(", ", mod.author),
                            mod.@abstract);
                    }
                }
            }
            else
            {
                var matching = matchingCompatible.Concat(matchingIncompatible).ToList();
                matching.Sort((x, y) => string.Compare(x.identifier, y.identifier, StringComparison.Ordinal));

                foreach (var mod in matching)
                {
                    _user.RaiseMessage(mod.identifier);
                }
            }

            return Exit.Ok;
        }

        /// <summary>
        /// Searches for the <paramref name="term"/> in the list of compatible or incompatible modules for the game instance.
        /// Looks in name, identifier and description fields, and if given, restricts to authors matching the <paramref name="author"/> term.
        /// </summary>
        /// <param name="inst">The game instance which to handle with mods.</param>
        /// <param name="term">The string to search for in mods. This is case insensitive.</param>
        /// <param name="author">The author to search for. Default is <see langword="null"/>.</param>
        /// <param name="searchIncompatible">Boolean to also search for incompatible mods or not. Default is <see langword="false"/>.</param>
        /// <returns>A <see cref="System.Collections.Generic.List{T}"/> of matching modules as a <see cref="CKAN.CkanModule"/>.</returns>
        public List<CkanModule> PerformSearch(CKAN.GameInstance inst, string term, string author = null, bool searchIncompatible = false)
        {
            // Remove spaces and special characters from the search term
            term = string.IsNullOrWhiteSpace(term) ? string.Empty : CkanModule.nonAlphaNums.Replace(term, "");
            author = string.IsNullOrWhiteSpace(author) ? string.Empty : CkanModule.nonAlphaNums.Replace(author, "");

            var registry = RegistryManager.Instance(inst).registry;

            if (!searchIncompatible)
            {
                return registry
                    .CompatibleModules(inst.VersionCriteria())
                    .Where(module =>
                    {
                        // Look for a match in each string
                        return (module.SearchableName.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                                || module.SearchableIdentifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                                || module.SearchableAbstract.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                                || module.SearchableDescription.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                               && module.SearchableAuthors.Any(auth => auth.IndexOf(author, StringComparison.OrdinalIgnoreCase) > -1);
                    }).ToList();
            }

            return registry
                .IncompatibleModules(inst.VersionCriteria())
                .Where(module =>
                {
                    // Look for a match in each string
                    return (module.SearchableName.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableIdentifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableAbstract.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableDescription.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                           && module.SearchableAuthors.Any(auth => auth.IndexOf(author, StringComparison.OrdinalIgnoreCase) > -1);
                }).ToList();
        }

        private static string CaseInsensitiveExactMatch(List<CkanModule> mods, string module)
        {
            // Look for a matching mod with a case insensitive search
            var found = mods.FirstOrDefault(m => string.Equals(m.identifier, module, StringComparison.OrdinalIgnoreCase));

            // If we don't find anything, use the original string so the main code can raise errors
            return found?.identifier ?? module;
        }

        /// <summary>
        /// Convert case insensitive mod names from the user to case sensitive identifiers.
        /// </summary>
        /// <param name="inst">The game instance which to handle with mods.</param>
        /// <param name="modules">The <see cref="System.Collections.Generic.List{T}"/> of strings to convert.</param>
        public static void AdjustModulesCase(CKAN.GameInstance inst, List<string> modules)
        {
            IRegistryQuerier registry = RegistryManager.Instance(inst).registry;

            // Get the list of all compatible and incompatible mods
            var mods = registry.CompatibleModules(inst.VersionCriteria()).ToList();
            mods.AddRange(registry.IncompatibleModules(inst.VersionCriteria()));

            for (var i = 0; i < modules.Count; ++i)
            {
                var match = CkanModule.idAndVersionMatcher.Match(modules[i]);
                if (match.Success)
                {
                    // Handle 'name=version' format
                    var ident = match.Groups["mod"].Value;
                    var version = match.Groups["version"].Value;
                    modules[i] = string.Format("{0}={1}", CaseInsensitiveExactMatch(mods, ident), version);
                }
                else
                {
                    modules[i] = CaseInsensitiveExactMatch(mods, modules[i]);
                }
            }
        }
    }

    [Verb("search", HelpText = "Search for mods")]
    internal class SearchOptions : InstanceSpecificOptions
    {
        [Option("detail", HelpText = "Show full name, latest compatible version and short description of each module")]
        public bool Detail { get; set; }

        [Option("all", HelpText = "Show incompatible mods too")]
        public bool All { get; set; }

        [Option("author", HelpText = "Limit search results to mods by matching author")]
        public string Author { get; set; }

        [Value(0, MetaName = "Search term", HelpText = "The term to search for")]
        public string SearchTerm { get; set; }
    }
}
