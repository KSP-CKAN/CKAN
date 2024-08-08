using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using CommandLine;

namespace CKAN.CmdLine
{
    public class Search : ICommand
    {
        public Search(RepositoryDataManager repoData, IUser user)
        {
            this.repoData = repoData;
            this.user     = user;
        }

        public int RunCommand(CKAN.GameInstance ksp, object raw_options)
        {
            SearchOptions options = (SearchOptions)raw_options;

            // Check the input.
            if (string.IsNullOrWhiteSpace(options.search_term) && string.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                foreach (var h in Actions.GetHelp("search"))
                {
                    user.RaiseError(h);
                }
                return Exit.BADOPT;
            }

            List<CkanModule> matching_compatible = PerformSearch(ksp, options.search_term, options.author_term, false);
            List<CkanModule> matching_incompatible = new List<CkanModule>();
            if (options.all)
            {
                matching_incompatible = PerformSearch(ksp, options.search_term, options.author_term, true);
            }

            // Show how many matches we have.
            if (options.all && !string.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseMessage(Properties.Resources.SearchFoundByAuthorWithIncompat,
                    matching_compatible.Count().ToString(),
                    matching_incompatible.Count().ToString(),
                    options.search_term,
                    options.author_term);
            }
            else if (options.all && string.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseMessage(Properties.Resources.SearchFoundWithIncompat,
                    matching_compatible.Count().ToString(),
                    matching_incompatible.Count().ToString(),
                    options.search_term);
            }
            else if (!options.all && !string.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseMessage(Properties.Resources.SearchFoundByAuthor,
                    matching_compatible.Count().ToString(),
                    options.search_term,
                    options.author_term);
            }
            else if (!options.all && string.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseMessage(Properties.Resources.SearchFound,
                    matching_compatible.Count().ToString(),
                    options.search_term);
            }

            // Present the results.
            if (!matching_compatible.Any() && (!options.all || !matching_incompatible.Any()))
            {
                return Exit.OK;
            }

            if (options.detail)
            {
                user.RaiseMessage(Properties.Resources.SearchCompatibleModsHeader);
                foreach (CkanModule mod in matching_compatible)
                {
                    user.RaiseMessage(Properties.Resources.SearchCompatibleMod,
                        mod.identifier,
                        mod.version,
                        mod.name,
                        mod.author == null ? "N/A" : string.Join(", ", mod.author),
                        mod.@abstract);
                }

                if (matching_incompatible.Any())
                {
                    user.RaiseMessage(Properties.Resources.SearchIncompatibleModsHeader);
                    foreach (CkanModule mod in matching_incompatible)
                    {
                        CkanModule.GetMinMaxVersions(new List<CkanModule> { mod } , out _, out _, out var minKsp, out var maxKsp);
                        string GameVersion = Versioning.GameVersionRange.VersionSpan(ksp.game, minKsp, maxKsp).ToString();

                        user.RaiseMessage(Properties.Resources.SearchIncompatibleMod,
                            mod.identifier,
                            mod.version,
                            GameVersion,
                            mod.name,
                            mod.author == null ? "N/A" : string.Join(", ", mod.author),
                            mod.@abstract);
                    }
                }
            }
            else
            {
                List<CkanModule> matching = matching_compatible.Concat(matching_incompatible).ToList();
                matching.Sort((x, y) => string.Compare(x.identifier, y.identifier, StringComparison.Ordinal));

                foreach (CkanModule mod in matching)
                {
                    user.RaiseMessage(mod.identifier);
                }
            }

            return Exit.OK;
        }

        /// <summary>
        /// Searches for the term in the list of compatible or incompatible modules for the ksp instance.
        /// Looks in name, identifier and description fields, and if given, restricts to authors matching the author term.
        /// </summary>
        /// <returns>List of matching modules.</returns>
        /// <param name="ksp">The KSP instance to perform the search for.</param>
        /// <param name="term">The search term. Case insensitive.</param>
        /// <param name="author">Name of author to find</param>
        /// <param name="searchIncompatible">True to look for incompatible modules, false (default) to look for compatible</param>
        public List<CkanModule> PerformSearch(CKAN.GameInstance ksp, string term, string author = null, bool searchIncompatible = false)
        {
            // Remove spaces and special characters from the search term.
            term   = string.IsNullOrWhiteSpace(term)   ? string.Empty : CkanModule.nonAlphaNums.Replace(term, "");
            author = string.IsNullOrWhiteSpace(author) ? string.Empty : CkanModule.nonAlphaNums.Replace(author, "");

            var registry = RegistryManager.Instance(ksp, repoData).registry;

            return searchIncompatible
                ? registry
                    .IncompatibleModules(ksp.VersionCriteria())
                    // Look for a match in each string.
                    .Where(module => (module.SearchableName.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableIdentifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableAbstract.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableDescription.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                            && module.SearchableAuthors.Any((auth) => auth.IndexOf(author, StringComparison.OrdinalIgnoreCase) > -1))
                    .ToList()
                : registry
                    .CompatibleModules(ksp.VersionCriteria())
                    // Look for a match in each string.
                    .Where(module => (module.SearchableName.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableIdentifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableAbstract.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                            || module.SearchableDescription.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                            && module.SearchableAuthors.Any((auth) => auth.IndexOf(author, StringComparison.OrdinalIgnoreCase) > -1))
                    .ToList();
        }

        /// <summary>
        /// Find the proper capitalization of an identifier
        /// </summary>
        /// <param name="mods">List of valid mods from the registry</param>
        /// <param name="module">Identifier to be treated as case insensitive</param>
        /// <returns>
        /// The mod's properly capitalized identifier, or the original string if it doesn't exist
        /// </returns>
        private static string CaseInsensitiveExactMatch(List<CkanModule> mods, string module)
        {
            // Look for a matching mod with a case insensitive search
            CkanModule found = mods.FirstOrDefault(
                (CkanModule m) => string.Equals(m.identifier, module, StringComparison.OrdinalIgnoreCase)
            );
            // If we don't find anything, use the original string so the main code can raise errors
            return found?.identifier ?? module;
        }

        /// <summary>
        /// Convert case insensitive mod names from the user to case sensitive identifiers
        /// </summary>
        /// <param name="instance">Game instance forgetting the mods</param>
        /// <param name="modules">List of strings to convert, format 'identifier' or 'identifier=version'</param>
        public static void AdjustModulesCase(CKAN.GameInstance instance, Registry registry, List<string> modules)
        {
            // Get the list of all compatible and incompatible mods
            List<CkanModule> mods = registry.CompatibleModules(instance.VersionCriteria()).ToList();
            mods.AddRange(registry.IncompatibleModules(instance.VersionCriteria()));
            for (int i = 0; i < modules.Count; ++i)
            {
                Match match = CkanModule.idAndVersionMatcher.Match(modules[i]);
                if (match.Success)
                {
                    // Handle name=version format
                    string ident   = match.Groups["mod"].Value;
                    string version = match.Groups["version"].Value;
                    modules[i] = $"{CaseInsensitiveExactMatch(mods, ident)}={version}";
                }
                else
                {
                    modules[i] = CaseInsensitiveExactMatch(mods, modules[i]);
                }
            }
        }

        private readonly RepositoryDataManager repoData;
        private readonly IUser                 user;
    }

    internal class SearchOptions : InstanceSpecificOptions
    {
        [Option("detail", HelpText = "Show full name, latest compatible version and short description of each module")]
        public bool detail { get; set; }

        [Option("all", HelpText = "Show incompatible mods too")]
        public bool all { get; set; }

        [Option("author", HelpText = "Limit search results to mods by matching authors")]
        public string author_term { get; set; }

        [ValueOption(0)]
        public string search_term { get; set; }
    }

}
