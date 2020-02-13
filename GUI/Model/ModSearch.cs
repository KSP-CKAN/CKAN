using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CKAN
{
    /// <summary>
    /// An object representing a search to be performed of the mod list.
    /// Create one with the constructor or Parse(), and use it with Matches().
    /// </summary>
    public sealed class ModSearch
    {
        /// <summary>
        /// Initialize a mod search object.
        /// Null or empty parameters are treated as matching everything.
        /// </summary>
        /// <param name="byName">String to search for in mod names, identifiers, and abbreviations</param>
        /// <param name="byAuthor">String to search for in author names</param>
        /// <param name="byDescription">String to search for in mod descriptions</param>
        /// <param name="localization">Language to search for in mod localizations</param>
        /// <param name="depends">Identifier prefix to find in mod depends relationships</param>
        /// <param name="recommends">Identifier prefix to find in mod recommends relationships</param>
        /// <param name="suggests">Identifier prefix to find in mod suggests relationships</param>
        /// <param name="conflicts">Identifier prefix to find in mod conflicts relationships</param>
        /// <param name="combined">Full formatted search string if known, will be auto generated otherwise</param>
        public ModSearch(
            string byName, string byAuthor, string byDescription, string localization,
            string depends, string recommends, string suggests, string conflicts,
            string combined = null)
        {
            Name         = CkanModule.nonAlphaNums.Replace(byName, "");
            Author       = CkanModule.nonAlphaNums.Replace(byAuthor, "");
            Description  = CkanModule.nonAlphaNums.Replace(byDescription, "");
            Localization = localization;

            DependsOn     = depends;
            Recommends    = recommends;
            Suggests      = suggests;
            ConflictsWith = conflicts;

            Combined = combined ?? getCombined();
        }

        /// <summary>
        /// String to search for in mod names, identifiers, and abbreviations
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// String to search for in author names
        /// </summary>
        public readonly string Author;

        /// <summary>
        /// String to search for in mod descriptions
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Language to search for in mod localizations
        /// </summary>
        public readonly string Localization;

        /// <summary>
        /// Identifier prefix to find in mod depends relationships
        /// </summary>
        public readonly string DependsOn;

        /// <summary>
        /// Identifier prefix to find in mod recommends relationships
        /// </summary>
        public readonly string Recommends;

        /// <summary>
        /// Identifier prefix to find in mod suggests relationships
        /// </summary>
        public readonly string Suggests;

        /// <summary>
        /// Identifier prefix to find in mod conflicts relationships
        /// </summary>
        public readonly string ConflictsWith;

        /// <summary>
        /// Full formatted search string
        /// </summary>
        public readonly string Combined;

        /// <summary>
        /// Generate a full formatted search string from the parameters.
        /// MUST be the inverse of Parse!
        /// </summary>
        /// <returns>
        ///
        /// </returns>
        private string getCombined()
        {
            var pieces = new List<string>();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                pieces.Add(Name);
            }
            if (!string.IsNullOrWhiteSpace(Author))
            {
                pieces.Add($"@{Author}");
            }
            if (!string.IsNullOrWhiteSpace(Description))
            {
                pieces.Add($"{Properties.Resources.ModSearchDescriptionPrefix}{Description}");
            }
            if (!string.IsNullOrWhiteSpace(Localization))
            {
                pieces.Add($"{Properties.Resources.ModSearchLanguagePrefix}{Localization}");
            }
            if (!string.IsNullOrWhiteSpace(DependsOn))
            {
                pieces.Add($"{Properties.Resources.ModSearchDependsPrefix}{DependsOn}");
            }
            if (!string.IsNullOrWhiteSpace(Recommends))
            {
                pieces.Add($"{Properties.Resources.ModSearchRecommendsPrefix}{Recommends}");
            }
            if (!string.IsNullOrWhiteSpace(Suggests))
            {
                pieces.Add($"{Properties.Resources.ModSearchSuggestsPrefix}{Suggests}");
            }
            if (!string.IsNullOrWhiteSpace(ConflictsWith))
            {
                pieces.Add($"{Properties.Resources.ModSearchConflictsPrefix}{ConflictsWith}");
            }
            return pieces.Count == 0
                ? null
                : string.Join(" ", pieces);
        }

        /// <summary>
        /// Parse a full formatted search string into its component parts.
        /// May throw a Kraken if the syntax is bad.
        /// MUST be the inverse of getCombined!
        /// </summary>
        /// <param name="combined">Full formatted search string</param>
        /// <returns>
        /// New search object, or null if no search terms defined
        /// </returns>
        public static ModSearch Parse(string combined)
        {
            if (string.IsNullOrWhiteSpace(combined))
            {
                return null;
            }
            string byName         = "";
            string byAuthor       = "";
            string byDescription  = "";
            string byLocalization = "";

            string depends    = "";
            string recommends = "";
            string suggests   = "";
            string conflicts  = "";

            var pieces = combined.Split(' ');
            foreach (string s in pieces)
            {
                if (TryPrefix(s, "@", out string auth))
                {
                    if (string.IsNullOrEmpty(byAuthor))
                    {
                        byAuthor = CkanModule.nonAlphaNums.Replace(auth, "");
                    }
                    else
                    {
                        throw new Kraken("Can't search multiple authors!");
                    }
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchDescriptionPrefix, out string desc))
                {
                    byDescription += CkanModule.nonAlphaNums.Replace(desc, "");
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchLanguagePrefix, out string lang))
                {
                    byLocalization += lang;
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchDependsPrefix, out string dep))
                {
                    depends += dep;
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchRecommendsPrefix, out string rec))
                {
                    recommends += rec;
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchSuggestsPrefix, out string sug))
                {
                    suggests += sug;
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchConflictsPrefix, out string conf))
                {
                    conflicts += conf;
                }
                else
                {
                    // No special format = search names and identifiers
                    byName += CkanModule.nonAlphaNums.Replace(s, "");
                }
            }
            return new ModSearch(
                byName, byAuthor, byDescription, byLocalization,
                depends, recommends, suggests, conflicts,
                combined
            );
        }

        private static bool TryPrefix(string container, string prefix, out string remainder)
        {
            if (container.StartsWith(prefix))
            {
                remainder = container.Substring(prefix.Length);
                return true;
            }
            else
            {
                remainder = "";
                return false;
            }
        }

        /// <summary>
        /// Check whether our filters match the given mod.
        /// </summary>
        /// <param name="mod">Mod to check</param>
        /// <returns>
        /// true if mod matches filters, false otherwise
        /// </returns>
        public bool Matches(GUIMod mod)
        {
            return MatchesName(mod)
                && MatchesAuthor(mod)
                && MatchesDescription(mod)
                && MatchesLocalization(mod)
                && MatchesDepends(mod)
                && MatchesRecommends(mod)
                && MatchesSuggests(mod)
                && MatchesConflicts(mod);
        }

        private bool MatchesName(GUIMod mod)
        {
            return string.IsNullOrWhiteSpace(Name)
                 || mod.Abbrevation.IndexOf(Name, StringComparison.InvariantCultureIgnoreCase) != -1
                || mod.SearchableName.IndexOf(Name, StringComparison.InvariantCultureIgnoreCase) != -1
                || mod.SearchableIdentifier.IndexOf(Name, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private bool MatchesAuthor(GUIMod mod)
        {
            return string.IsNullOrWhiteSpace(Author)
                 || mod.SearchableAuthors.Any(author =>
                     author.IndexOf(Author, StringComparison.InvariantCultureIgnoreCase) != -1);
        }

        private bool MatchesDescription(GUIMod mod)
        {
            return string.IsNullOrWhiteSpace(Description)
                || mod.SearchableAbstract.IndexOf(Description, StringComparison.InvariantCultureIgnoreCase) != -1
                || mod.SearchableDescription.IndexOf(Description, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        private bool MatchesLocalization(GUIMod mod)
        {
            var ckm = mod.ToModule();
            return string.IsNullOrWhiteSpace(Localization)
                || (
                    ckm.localizations != null
                    && ckm.localizations.Any(loc =>
                        loc.IndexOf(Localization, StringComparison.InvariantCultureIgnoreCase) != -1)
                );
        }

        private bool MatchesDepends(GUIMod mod)
        {
            return RelationshipMatch(mod.ToModule().depends, DependsOn);
        }
        private bool MatchesRecommends(GUIMod mod)
        {
            return RelationshipMatch(mod.ToModule().recommends, Recommends);
        }
        private bool MatchesSuggests(GUIMod mod)
        {
            return RelationshipMatch(mod.ToModule().suggests, Suggests);
        }
        private bool MatchesConflicts(GUIMod mod)
        {
            return RelationshipMatch(mod.ToModule().conflicts, ConflictsWith);
        }
        private bool RelationshipMatch(List<RelationshipDescriptor> rels, string toFind)
        {
            return string.IsNullOrWhiteSpace(toFind)
                || (rels != null && rels.Any(r => r.StartsWith(toFind)));
        }

    }
}
