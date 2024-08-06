using System;
using System.Linq;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace CKAN.GUI
{
    /// <summary>
    /// An object representing a search to be performed of the mod list.
    /// Create one with the constructor or Parse(), and use it with Matches().
    /// </summary>
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    public sealed class ModSearch : IEquatable<ModSearch>
    {
        /// <summary>
        /// Initialize a mod search object.
        /// Null or empty parameters are treated as matching everything.
        /// </summary>
        /// <param name="byName">String to search for in mod names, identifiers, and abbreviations</param>
        /// <param name="byAuthors">String to search for in author names</param>
        /// <param name="byDescription">String to search for in mod descriptions</param>
        /// <param name="localizations">Language to search for in mod localizations</param>
        /// <param name="depends">Identifier prefix to find in mod depends relationships</param>
        /// <param name="recommends">Identifier prefix to find in mod recommends relationships</param>
        /// <param name="suggests">Identifier prefix to find in mod suggests relationships</param>
        /// <param name="conflicts">Identifier prefix to find in mod conflicts relationships</param>
        /// <param name="combined">Full formatted search string if known, will be auto generated otherwise</param>
        public ModSearch(
            string byName, List<string> byAuthors, string byDescription,
            List<string> licenses, List<string> localizations,
            List<string> depends, List<string> recommends, List<string> suggests, List<string> conflicts,
            List<string> supports,
            List<string> tagNames, List<ModuleLabel> labels,
            bool? compatible, bool? installed, bool? cached, bool? newlyCompatible,
            bool? upgradeable, bool? replaceable,
            string combined = null)
        {
            Name = (ShouldNegateTerm(byName, out string subName) ? "-" : "")
                + CkanModule.nonAlphaNums.Replace(subName, "");
            initStringList(Authors, byAuthors);
            Description = (ShouldNegateTerm(byDescription, out string subDesc) ? "-" : "")
                + CkanModule.nonAlphaNums.Replace(subDesc, "");
            initStringList(Localizations, localizations);
            initStringList(Licenses, licenses);

            initStringList(DependsOn,     depends);
            initStringList(Recommends,    recommends);
            initStringList(Suggests,      suggests);
            initStringList(ConflictsWith, conflicts);
            initStringList(Supports,      supports);

            initStringList(TagNames, tagNames);
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

        private void initStringList(List<string> dest, List<string> source)
        {
            if (source?.Any() ?? false)
            {
                dest.AddRange(source);
            }
        }

        public ModSearch(GUIModFilter filter, ModuleTag tag = null, ModuleLabel label = null)
        {
            switch (filter)
            {
                case GUIModFilter.Compatible:               Compatible      = true;  break;
                case GUIModFilter.Incompatible:             Compatible      = false; break;
                case GUIModFilter.Installed:                Installed       = true;  break;
                case GUIModFilter.NotInstalled:             Installed       = false; break;
                case GUIModFilter.InstalledUpdateAvailable: Upgradeable     = true;  break;
                case GUIModFilter.Replaceable:              Replaceable     = true;  break;
                case GUIModFilter.Cached:                   Cached          = true;  break;
                case GUIModFilter.Uncached:                 Cached          = false; break;
                case GUIModFilter.NewInRepository:          NewlyCompatible = true;  break;
                case GUIModFilter.Tag:                      TagNames.Add(tag?.Name); break;
                case GUIModFilter.CustomLabel:
                    if (label != null)
                    {
                        Labels.Add(label);
                    }
                    break;
                default:
                case GUIModFilter.All:                                               break;
            }
            Combined = getCombined();
        }

        /// <summary>
        /// String to search for in mod names, identifiers, and abbreviations
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// String to search for in author names
        /// </summary>
        public readonly List<string> Authors = new List<string>();

        /// <summary>
        /// String to search for in mod descriptions
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// License substring to search for in mod licenses
        /// </summary>
        public readonly List<string> Licenses = new List<string>();

        /// <summary>
        /// Language to search for in mod localizations
        /// </summary>
        public readonly List<string> Localizations = new List<string>();

        /// <summary>
        /// Identifier prefix to find in mod depends relationships
        /// </summary>
        public readonly List<string> DependsOn = new List<string>();

        /// <summary>
        /// Identifier prefix to find in mod recommends relationships
        /// </summary>
        public readonly List<string> Recommends = new List<string>();

        /// <summary>
        /// Identifier prefix to find in mod suggests relationships
        /// </summary>
        public readonly List<string> Suggests = new List<string>();

        /// <summary>
        /// Identifier prefix to find in mod conflicts relationships
        /// </summary>
        public readonly List<string> ConflictsWith = new List<string>();

        /// <summary>
        /// Identifier prefix to find in mod conflicts relationships
        /// </summary>
        public readonly List<string> Supports = new List<string>();

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
        /// Create a new search from a list of authors
        /// </summary>
        /// <param name="authors">The authors for the search</param>
        /// <returns>A search for the authors</returns>
        public static ModSearch FromAuthors(IEnumerable<string> authors)
            => new ModSearch(
                // Can't search for spaces, so massage them like SearchableAuthors
                "", authors.Select(a => CkanModule.nonAlphaNums.Replace(a, "")).ToList(), "",
                null, null,
                null, null, null, null, null,
                null, null,
                null, null, null, null,
                null, null);

        /// <summary>
        /// Create a new search by merging this and another
        /// </summary>
        /// <param name="other">The other search for merging</param>
        /// <returns>A search containing all the search terms</returns>
        public ModSearch MergedWith(ModSearch other)
            => new ModSearch(
                Name + other.Name,
                Authors.Concat(other.Authors).Distinct().ToList(),
                Description + other.Description,
                Licenses.Concat(other.Licenses).Distinct().ToList(),
                Localizations.Concat(other.Localizations).Distinct().ToList(),
                DependsOn.Concat(other.DependsOn).Distinct().ToList(),
                Recommends.Concat(other.Recommends).Distinct().ToList(),
                Suggests.Concat(other.Suggests).Distinct().ToList(),
                ConflictsWith.Concat(other.ConflictsWith).Distinct().ToList(),
                Supports.Concat(other.Supports).Distinct().ToList(),
                TagNames.Concat(other.TagNames).Distinct().ToList(),
                Labels.Concat(other.Labels).Distinct().ToList(),
                Compatible      ?? other.Compatible,
                Installed       ?? other.Installed,
                Cached          ?? other.Cached,
                NewlyCompatible ?? other.NewlyCompatible,
                Upgradeable     ?? other.Upgradeable,
                Replaceable     ?? other.Replaceable);

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
            foreach (var author in Authors.Where(auth => !string.IsNullOrEmpty(auth)))
            {
                pieces.Add(AddTermPrefix("@", author));
            }
            if (!string.IsNullOrWhiteSpace(Description))
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchDescriptionPrefix, Description));
            }
            foreach (var license in Licenses.Where(lic => !string.IsNullOrEmpty(lic)))
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchLicensePrefix, license));
            }
            foreach (var localization in Localizations.Where(lang => !string.IsNullOrEmpty(lang)))
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchLanguagePrefix, localization));
            }
            foreach (var dep in DependsOn.Where(d => !string.IsNullOrEmpty(d)))
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchDependsPrefix, dep));
            }
            foreach (var rec in Recommends.Where(r => !string.IsNullOrEmpty(r)))
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchRecommendsPrefix, rec));
            }
            foreach (var sug in Suggests.Where(s => !string.IsNullOrEmpty(s)))
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchSuggestsPrefix, sug));
            }
            foreach (var conf in ConflictsWith.Where(c => !string.IsNullOrEmpty(c)))
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchConflictsPrefix, conf));
            }
            foreach (var sup in Supports.Where(s => !string.IsNullOrEmpty(s)))
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchSupportsPrefix, sup));
            }
            foreach (var tagName in TagNames)
            {
                pieces.Add(AddTermPrefix(Properties.Resources.ModSearchTagPrefix, tagName ?? ""));
            }
            foreach (var label in Labels)
            {
                pieces.Add($"{Properties.Resources.ModSearchLabelPrefix}{label.Name.Replace(" ", "")}");
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
            string byName          = "";
            var    byAuthors       = new List<string>();
            string byDescription   = "";
            var    byLicenses      = new List<string>();
            var    byLocalizations = new List<string>();

            var depends    = new List<string>();
            var recommends = new List<string>();
            var suggests   = new List<string>();
            var conflicts  = new List<string>();
            var supports   = new List<string>();

            List<string>      tagNames = new List<string>();
            List<ModuleLabel> labels   = new List<ModuleLabel>();

            bool? compatible      = null;
            bool? installed       = null;
            bool? cached          = null;
            bool? newlyCompatible = null;
            bool? upgradeable     = null;
            bool? replaceable     = null;

            var pieces = combined.Split();
            foreach (string s in pieces)
            {
                if (TryPrefix(s, "@", out string auth))
                {
                    byAuthors.Add(auth);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchDescriptionPrefix, out string desc))
                {
                    byDescription += (ShouldNegateTerm(desc, out string subDesc) ? "-" : "") + CkanModule.nonAlphaNums.Replace(subDesc, "");
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchLicensePrefix, out string lic))
                {
                    byLicenses.Add(lic);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchLanguagePrefix, out string lang))
                {
                    byLocalizations.Add(lang);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchDependsPrefix, out string dep))
                {
                    depends.Add(dep);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchRecommendsPrefix, out string rec))
                {
                    recommends.Add(rec);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchSuggestsPrefix, out string sug))
                {
                    suggests.Add(sug);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchConflictsPrefix, out string conf))
                {
                    conflicts.Add(conf);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchSupportsPrefix, out string sup))
                {
                    supports.Add(sup);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchTagPrefix, out string tagName))
                {
                    tagNames.Add(tagName);
                }
                else if (TryPrefix(s, Properties.Resources.ModSearchLabelPrefix, out string labelName))
                {
                    labels.AddRange(
                        // Label searches exclude spaces, but label names can include them
                        knownLabels.Where(lb => lb.Name.Replace(" ", "") == labelName)
                            // If label doesn't exist, maybe it will be created later or the user is still typing.
                            // Make an unofficial label object to accurately reflect the search.
                            .DefaultIfEmpty(new ModuleLabel() { Name = labelName })
                    );
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
                    byName += (ShouldNegateTerm(s, out string subS) ? "-" : "") + CkanModule.nonAlphaNums.Replace(subS, "");
                }
            }
            return new ModSearch(
                byName, byAuthors, byDescription,
                byLicenses, byLocalizations,
                depends, recommends, suggests, conflicts, supports,
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
            else if (container.StartsWith($"-{prefix}"))
            {
                remainder = $"-{container.Substring(prefix.Length + 1)}";
                return true;
            }
            else
            {
                remainder = "";
                return false;
            }
        }

        private static string AddTermPrefix(string prefix, string term)
            => term.StartsWith("-")
                ? $"-{prefix}{term.Substring(1)}"
                : $"{prefix}{term}";

        private static bool ShouldNegateTerm(string term, out string subTerm)
        {
            if (term.StartsWith("-"))
            {
                subTerm = term.Substring(1);
                return true;
            }
            else
            {
                subTerm = term;
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
            => MatchesName(mod)
                && MatchesAuthors(mod)
                && MatchesDescription(mod)
                && MatchesLicenses(mod)
                && MatchesLocalizations(mod)
                && MatchesDepends(mod)
                && MatchesRecommends(mod)
                && MatchesSuggests(mod)
                && MatchesConflicts(mod)
                && MatchesSupports(mod)
                && MatchesTags(mod)
                && MatchesLabels(mod)
                && MatchesCompatible(mod)
                && MatchesInstalled(mod)
                && MatchesCached(mod)
                && MatchesNewlyCompatible(mod)
                && MatchesUpgradeable(mod)
                && MatchesReplaceable(mod);

        private bool MatchesName(GUIMod mod)
            => string.IsNullOrWhiteSpace(Name)
                || ShouldNegateTerm(Name, out string subName) ^ (
                    mod.Abbrevation.IndexOf(subName, StringComparison.InvariantCultureIgnoreCase) != -1
                    || mod.SearchableName.IndexOf(subName, StringComparison.InvariantCultureIgnoreCase) != -1
                    || mod.SearchableIdentifier.IndexOf(subName, StringComparison.InvariantCultureIgnoreCase) != -1);

        private bool MatchesAuthors(GUIMod mod)
            => Authors.Count < 1
                || Authors.All(searchAuth =>
                    ShouldNegateTerm(searchAuth, out string subAuth)
                    ^ mod.SearchableAuthors.Any(modAuth =>
                            modAuth.StartsWith(subAuth, StringComparison.InvariantCultureIgnoreCase)));

        private bool MatchesDescription(GUIMod mod)
            => string.IsNullOrWhiteSpace(Description)
                || ShouldNegateTerm(Description, out string subDesc) ^ (
                    mod.SearchableAbstract.IndexOf(subDesc, StringComparison.InvariantCultureIgnoreCase) != -1
                    || mod.SearchableDescription.IndexOf(subDesc, StringComparison.InvariantCultureIgnoreCase) != -1);

        private bool MatchesLicenses(GUIMod mod)
        {
            var ckm = mod.ToModule();
            return Licenses.Count < 1
                || (
                    ckm.license != null
                    && Licenses.All(searchLic =>
                        ShouldNegateTerm(searchLic, out string subLic)
                        ^ ckm.license.Any(modLic =>
                            modLic.ToString().StartsWith(subLic, StringComparison.InvariantCultureIgnoreCase)))
                );
        }

        private bool MatchesLocalizations(GUIMod mod)
        {
            var ckm = mod.ToModule();
            return Localizations.Count < 1
                || (
                    ckm.localizations != null
                    && Localizations.All(searchLoc =>
                        ShouldNegateTerm(searchLoc, out string subLoc)
                        ^ ckm.localizations.Any(modLoc =>
                            modLoc.StartsWith(subLoc, StringComparison.InvariantCultureIgnoreCase)))
                );
        }

        private bool MatchesDepends(GUIMod mod)
            => RelationshipMatch(mod.ToModule().depends, DependsOn);
        private bool MatchesRecommends(GUIMod mod)
            => RelationshipMatch(mod.ToModule().recommends, Recommends);
        private bool MatchesSuggests(GUIMod mod)
            => RelationshipMatch(mod.ToModule().suggests, Suggests);
        private bool MatchesConflicts(GUIMod mod)
            => RelationshipMatch(mod.ToModule().conflicts, ConflictsWith);
        private bool MatchesSupports(GUIMod mod)
            => RelationshipMatch(mod.ToModule().supports, Supports);
        private bool RelationshipMatch(List<RelationshipDescriptor> rels, List<string> toFind)
            => toFind.Count < 1
                || (rels != null && toFind.All(searchRel =>
                    ShouldNegateTerm(searchRel, out string subRel)
                    ^ rels.Any(r => r.StartsWith(subRel))));

        private bool MatchesTags(GUIMod mod)
        {
            var tagsInMod = mod.ToModule().Tags;
            return TagNames.Count < 1
                || TagNames.All(tn =>
                    ShouldNegateTerm(tn, out string subTag) ^ (
                        string.IsNullOrEmpty(subTag)
                            ? tagsInMod == null
                            : tagsInMod?.Contains(subTag) ?? false));
        }

        private bool MatchesLabels(GUIMod mod)
            // Every label in Labels must contain this mod
            => Labels.Count < 1 || Labels.All(lb =>
                lb.ContainsModule(Main.Instance.CurrentInstance.game, mod.Identifier));

        private bool MatchesCompatible(GUIMod mod)
            => !Compatible.HasValue || Compatible.Value == !mod.IsIncompatible;

        private bool MatchesInstalled(GUIMod mod)
            => !Installed.HasValue || Installed.Value == mod.IsInstalled;

        private bool MatchesCached(GUIMod mod)
            => !Cached.HasValue || Cached.Value == mod.IsCached;

        private bool MatchesNewlyCompatible(GUIMod mod)
            => !NewlyCompatible.HasValue || NewlyCompatible.Value == mod.IsNew;

        private bool MatchesUpgradeable(GUIMod mod)
            => !Upgradeable.HasValue || Upgradeable.Value == mod.HasUpdate;

        private bool MatchesReplaceable(GUIMod mod)
            => !Replaceable.HasValue || Replaceable.Value == (mod.IsInstalled && mod.HasReplacement);

        public bool Equals(ModSearch other)
            => other != null
                && Name            == other.Name
                && Description     == other.Description
                && Compatible      == other.Compatible
                && Installed       == other.Installed
                && Cached          == other.Cached
                && NewlyCompatible == other.NewlyCompatible
                && Upgradeable     == other.Upgradeable
                && Replaceable     == other.Replaceable
                && Authors.SequenceEqual(other.Authors)
                && Licenses.SequenceEqual(other.Licenses)
                && Localizations.SequenceEqual(other.Localizations)
                && DependsOn.SequenceEqual(other.DependsOn)
                && Recommends.SequenceEqual(other.Recommends)
                && Suggests.SequenceEqual(other.Suggests)
                && ConflictsWith.SequenceEqual(other.ConflictsWith)
                && Supports.SequenceEqual(other.Supports)
                && TagNames.SequenceEqual(other.TagNames)
                && Labels.SequenceEqual(other.Labels);

        public override bool Equals(object obj)
            => Equals(obj as ModSearch);

        public override int GetHashCode()
            => Combined.GetHashCode();

    }
}
