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
            List<string> tagNames, List<ModuleLabel> labels,
            bool? compatible, bool? installed, bool? cached, bool? newlyCompatible,
            bool? upgradeable, bool? replaceable,
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

            if (tagNames?.Any() ?? false)
            {
                TagNames.AddRange(tagNames);
            }
            if (labels?.Any() ?? false)
            {
                Labels.AddRange(labels);
            }

            Compatible      = compatible;
            Installed       = installed;
            Cached          = cached;
            NewlyCompatible = newlyCompatible;
            Upgradeable     = upgradeable;
            Replaceable     = replaceable;

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

        public readonly List<string>      TagNames = new List<string>();
        public readonly List<ModuleLabel> Labels   = new List<ModuleLabel>();

        public readonly bool? Compatible;
        public readonly bool? Installed;
        public readonly bool? Cached;
        public readonly bool? NewlyCompatible;
        public readonly bool? Upgradeable;
        public readonly bool? Replaceable;

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
            foreach (var tagName in TagNames)
            {
                pieces.Add($"{Properties.Resources.ModSearchTagPrefix}{tagName}");
            }
            foreach (var label in Labels)
            {
                pieces.Add($"{Properties.Resources.ModSearchLabelPrefix}{label.Name}");
            }
            if (Compatible.HasValue)
            {
                pieces.Add(triStateString(Compatible.Value, Properties.Resources.ModSearchCompatibleSuffix));
            }
            if (Installed.HasValue)
            {
                pieces.Add(triStateString(Installed.Value, Properties.Resources.ModSearchInstalledSuffix));
            }
            if (Cached.HasValue)
            {
                pieces.Add(triStateString(Cached.Value, Properties.Resources.ModSearchCachedSuffix));
            }
            if (NewlyCompatible.HasValue)
            {
                pieces.Add(triStateString(NewlyCompatible.Value, Properties.Resources.ModSearchNewlyCompatibleSuffix));
            }
            if (Upgradeable.HasValue)
            {
                pieces.Add(triStateString(Upgradeable.Value, Properties.Resources.ModSearchUpgradeableSuffix));
            }
            if (Replaceable.HasValue)
            {
                pieces.Add(triStateString(Replaceable.Value, Properties.Resources.ModSearchReplaceableSuffix));
            }
            return pieces.Count == 0
                ? null
                : string.Join(" ", pieces);
        }

        private static string triStateString(bool val, string suffix)
        {
            string prefix = val
                ? Properties.Resources.ModSearchYesPrefix
                : Properties.Resources.ModSearchNoPrefix;
            return prefix + suffix;
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
        public static ModSearch Parse(string combined, List<ModuleLabel> knownLabels)
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

            List<string>      tagNames = new List<string>();
            List<ModuleLabel> labels   = new List<ModuleLabel>();

            bool? compatible      = null;
            bool? installed       = null;
            bool? cached          = null;
            bool? newlyCompatible = null;
            bool? upgradeable     = null;
            bool? replaceable     = null;

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
                else if (TryPrefix(s, Properties.Resources.ModSearchTagPrefix, out string tagName))
                {
                    tagNames.Add(tagName);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchLabelPrefix, out string labelName))
                {
                    labels.AddRange(knownLabels.Where(lb => lb.Name == labelName));
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchYesPrefix, out string yesSuffix))
                {
                    if (yesSuffix == Properties.Resources.ModSearchCompatibleSuffix)
                    {
                        compatible = true;
                    }
                    else if (yesSuffix == Properties.Resources.ModSearchInstalledSuffix)
                    {
                        installed = true;
                    }
                    else if (yesSuffix == Properties.Resources.ModSearchCachedSuffix)
                    {
                        cached = true;
                    }
                    else if (yesSuffix == Properties.Resources.ModSearchNewlyCompatibleSuffix)
                    {
                        newlyCompatible = true;
                    }
                    else if (yesSuffix == Properties.Resources.ModSearchUpgradeableSuffix)
                    {
                        upgradeable = true;
                    }
                    else if (yesSuffix == Properties.Resources.ModSearchReplaceableSuffix)
                    {
                        replaceable = true;
                    }
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchNoPrefix, out string noSuffix))
                {
                    if (noSuffix == Properties.Resources.ModSearchCompatibleSuffix)
                    {
                        compatible = false;
                    }
                    else if (noSuffix == Properties.Resources.ModSearchInstalledSuffix)
                    {
                        installed = false;
                    }
                    else if (noSuffix == Properties.Resources.ModSearchCachedSuffix)
                    {
                        cached = false;
                    }
                    else if (noSuffix == Properties.Resources.ModSearchNewlyCompatibleSuffix)
                    {
                        newlyCompatible = false;
                    }
                    else if (noSuffix == Properties.Resources.ModSearchUpgradeableSuffix)
                    {
                        upgradeable = false;
                    }
                    else if (noSuffix == Properties.Resources.ModSearchReplaceableSuffix)
                    {
                        replaceable = false;
                    }
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
                tagNames, labels,
                compatible, installed, cached, newlyCompatible,
                upgradeable, replaceable,
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
                && MatchesConflicts(mod)
                && MatchesTags(mod)
                && MatchesLabels(mod)
                && MatchesCompatible(mod)
                && MatchesInstalled(mod)
                && MatchesCached(mod)
                && MatchesNewlyCompatible(mod)
                && MatchesUpgradeable(mod)
                && MatchesReplaceable(mod);
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

        private bool MatchesTags(GUIMod mod)
        {
            var tagsInMod = mod.ToModule().Tags;
            return TagNames.Count < 1
                || ((tagsInMod?.Any() ?? false)
                    && TagNames.All(tn => tagsInMod.Contains(tn)));
        }

        private bool MatchesLabels(GUIMod mod)
        {
            // Every label in Labels must contain this mod
            return Labels.Count < 1 || Labels.All(lb => lb.ModuleIdentifiers.Contains(mod.Identifier));
        }

        private bool MatchesCompatible(GUIMod mod)
        {
            return !Compatible.HasValue || Compatible.Value == !mod.IsIncompatible;
        }

        private bool MatchesInstalled(GUIMod mod)
        {
            return !Installed.HasValue || Installed.Value == mod.IsInstalled;
        }

        private bool MatchesCached(GUIMod mod)
        {
            return !Cached.HasValue || Cached.Value == mod.IsCached;
        }

        private bool MatchesNewlyCompatible(GUIMod mod)
        {
            return !NewlyCompatible.HasValue || NewlyCompatible.Value == mod.IsNew;
        }

        private bool MatchesUpgradeable(GUIMod mod)
        {
            return !Upgradeable.HasValue || Upgradeable.Value == mod.HasUpdate;
        }

        private bool MatchesReplaceable(GUIMod mod)
        {
            return !Replaceable.HasValue || Replaceable.Value == mod.HasReplacement;
        }
    }
}
