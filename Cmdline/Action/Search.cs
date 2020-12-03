using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CKAN.Games;

namespace CKAN.CmdLine
{
    public class Search : ICommand
    {
        public IUser user { get; set; }

        public Search(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.GameInstance ksp, object raw_options)
        {
            SearchOptions options = (SearchOptions)raw_options;

            // Check the input.
            if (String.IsNullOrWhiteSpace(options.search_term) && String.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseError("No search term?");

                return Exit.BADOPT;
            }

            List<CkanModule> matching_compatible = PerformSearch(ksp, options.search_term, options.author_term, false);
            List<CkanModule> matching_incompatible = new List<CkanModule>();
            if (options.all)
            {
                matching_incompatible = PerformSearch(ksp, options.search_term, options.author_term, true);
            }

            // Show how many matches we have.
            if (options.all && !String.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseMessage("Found {0} compatible and {1} incompatible mods matching \"{2}\" by \"{3}\".",
                    matching_compatible.Count().ToString(),
                    matching_incompatible.Count().ToString(),
                    options.search_term,
                    options.author_term);
            }
            else if (options.all && String.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseMessage("Found {0} compatible and {1} incompatible mods matching \"{2}\".",
                    matching_compatible.Count().ToString(),
                    matching_incompatible.Count().ToString(),
                    options.search_term);
            }
            else if (!options.all && !String.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseMessage("Found {0} compatible mods matching \"{1}\" by \"{2}\".",
                    matching_compatible.Count().ToString(),
                    options.search_term,
                    options.author_term);
            }
            else if (!options.all && String.IsNullOrWhiteSpace(options.author_term))
            {
                user.RaiseMessage("Found {0} compatible mods matching \"{1}\".",
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
                user.RaiseMessage("Matching compatible mods:");
                foreach (CkanModule mod in matching_compatible)
                {
                    user.RaiseMessage("* {0} ({1}) - {2} by {3} - {4}",
                        mod.identifier,
                        mod.version,
                        mod.name,
                        mod.author == null ? "N/A" : String.Join(", ", mod.author),
                        mod.@abstract);
                }

                if (matching_incompatible.Any())
                {
                    user.RaiseMessage("Matching incompatible mods:");
                    foreach (CkanModule mod in matching_incompatible)
                    {
                        Registry.GetMinMaxVersions(new List<CkanModule> { mod } , out _, out _, out var minKsp, out var maxKsp);
                        string GameVersion = Versioning.GameVersionRange.VersionSpan(ksp.game, minKsp, maxKsp).ToString();

                        user.RaiseMessage("* {0} ({1} - {2}) - {3} by {4} - {5}",
                            mod.identifier,
                            mod.version,
                            GameVersion,
                            mod.name,
                            mod.author == null ? "N/A" : String.Join(", ", mod.author),
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
        public List<CkanModule> PerformSearch(CKAN.GameInstance ksp, string term, string author = null, bool searchIncompatible = false)
        {
            // Remove spaces and special characters from the search term.
            term   = String.IsNullOrWhiteSpace(term)   ? string.Empty : CkanModule.nonAlphaNums.Replace(term, "");
            author = String.IsNullOrWhiteSpace(author) ? string.Empty : CkanModule.nonAlphaNums.Replace(author, "");

            var registry = RegistryManager.Instance(ksp).registry;

            if (!searchIncompatible)
            {
                return registry
                    .CompatibleModules(ksp.VersionCriteria())
                    .Where((module) =>
                {
                    // Look for a match in each string.
                    return (module.SearchableName.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                        || module.SearchableIdentifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                        || module.SearchableAbstract.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                        || module.SearchableDescription.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                        && module.SearchableAuthors.Any((auth) => auth.IndexOf(author, StringComparison.OrdinalIgnoreCase) > -1);
                }).ToList();
            }
            else
            {
                return registry
                    .IncompatibleModules(ksp.VersionCriteria())
                    .Where((module) =>
                {
                    // Look for a match in each string.
                    return (module.SearchableName.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                        || module.SearchableIdentifier.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                        || module.SearchableAbstract.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1
                        || module.SearchableDescription.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1)
                        && module.SearchableAuthors.Any((auth) => auth.IndexOf(author, StringComparison.OrdinalIgnoreCase) > -1);
                }).ToList();
            }
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
        /// <param name="ksp">Game instance forgetting the mods</param>
        /// <param name="modules">List of strings to convert, format 'identifier' or 'identifier=version'</param>
        public static void AdjustModulesCase(CKAN.GameInstance ksp, List<string> modules)
        {
            IRegistryQuerier registry = RegistryManager.Instance(ksp).registry;
            // Get the list of all compatible and incompatible mods
            List<CkanModule> mods = registry.CompatibleModules(ksp.VersionCriteria()).ToList();
            mods.AddRange(registry.IncompatibleModules(ksp.VersionCriteria()));
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

    }
}
