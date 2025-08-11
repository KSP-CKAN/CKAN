using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

using CKAN.Configuration;
using CKAN.Games;
using CKAN.Versioning;
using CKAN.Extensions;

namespace CKAN
{
    public abstract class RelationshipDescriptor : IEquatable<RelationshipDescriptor>
    {
        public bool MatchesAny(IReadOnlyCollection<CkanModule>              modules,
                               IReadOnlyCollection<string>?                 dlls,
                               IDictionary<string, UnmanagedModuleVersion>? dlc)
            => MatchesAny(modules, dlls, dlc, out CkanModule? _);

        public abstract bool MatchesAny(IReadOnlyCollection<CkanModule>              modules,
                                        IReadOnlyCollection<string>?                 dlls,
                                        IDictionary<string, UnmanagedModuleVersion>? dlc,
                                        out CkanModule?                              matched);

        public abstract bool WithinBounds(CkanModule otherModule);

        public abstract List<CkanModule> LatestAvailableWithProvides(IRegistryQuerier                 registry,
                                                                     StabilityToleranceConfig         stabilityTolerance,
                                                                     GameVersionCriteria?             crit,
                                                                     IReadOnlyCollection<CkanModule>? installed = null,
                                                                     IReadOnlyCollection<CkanModule>? toInstall = null);

        public abstract CkanModule? ExactMatch(IRegistryQuerier                 registry,
                                               StabilityToleranceConfig         stabilityTolerance,
                                               GameVersionCriteria?             crit,
                                               IReadOnlyCollection<CkanModule>? installed = null,
                                               IReadOnlyCollection<CkanModule>? toInstall = null);

        public override bool Equals(object? other)
            => Equals(other as RelationshipDescriptor);

        public abstract bool Equals(RelationshipDescriptor? other);

        public static bool operator ==(RelationshipDescriptor? left,
                                       RelationshipDescriptor? right)
            => Equals(left, right);

        public static bool operator !=(RelationshipDescriptor? left,
                                       RelationshipDescriptor? right)
            => !Equals(left, right);

        public abstract override int GetHashCode();

        public abstract bool ContainsAny(IEnumerable<string> identifiers);

        public abstract bool StartsWith(string prefix);

        public virtual string ToStringWithCompat(IRegistryQuerier registry,
                                                 IGame            game)
            => ToString() ?? "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? choice_help_text;

        /// <summary>
        /// If true, then don't show recommendations and suggestions of this module or its dependencies.
        /// Otherwise recommendations and suggestions of everything in changeset will be included.
        /// This is meant to allow the KSP-RO team to shorten the prompts that appear during their installation.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool suppress_recommendations;

        // virtual ToString() already present in 'object'
    }

    public class ModuleRelationshipDescriptor : RelationshipDescriptor
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion? max_version;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion? min_version;

        // The identifier to match
        public string name = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion? version;

        public override bool WithinBounds(CkanModule otherModule)
            // See if the real thing is there
            => otherModule.identifier == name ? WithinBounds(otherModule.version)
                                              // See if anyone else "provides" the target name
                                              // Note that versions can't be checked for "provides"
                                              : (otherModule.provides?.Contains(name) ?? false);

        /// <summary>
        /// Returns if the other version satisfies this RelationshipDescriptor.
        /// If the RelationshipDescriptor has version set it compares against that.
        /// Else it uses the {min,max}_version fields treating nulls as unbounded.
        /// Note: Uses inclusive inequalities.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if other_version is within the bounds</returns>
        public bool WithinBounds(ModuleVersion? other)
            // UnmanagedModuleVersions with unknown versions always satisfy the bound
            => other == null
               || (other is UnmanagedModuleVersion unmanagedModuleVersion
                   && unmanagedModuleVersion.IsUnknownVersion)
               || (version?.Equals(other)
                          ?? ((min_version == null || min_version <= other)
                              && (max_version == null || max_version >= other)));

        /// <summary>
        /// Check whether any of the modules in a given list match this descriptor.
        /// NOTE: Only proper modules and DLC can be checked for versions!
        ///       DLLs match all versions, as do "provides" clauses.
        /// </summary>
        /// <param name="modules">Sequence of modules to consider</param>
        /// <param name="dlls">Sequence of DLLs to consider</param>
        /// <param name="dlc">DLC to consider</param>
        /// <param name="matched">The first module that matched, if any</param>
        /// <returns>
        /// true if any of the modules match this descriptor, false otherwise.
        /// </returns>
        public override bool MatchesAny(IReadOnlyCollection<CkanModule>              modules,
                                        IReadOnlyCollection<string>?                 dlls,
                                        IDictionary<string, UnmanagedModuleVersion>? dlc,
                                        out CkanModule?                              matched)
        {
            // DLLs are considered to match any version
            if (dlls != null && dlls.Contains(name))
            {
                matched = null;
                return true;
            }

            // .AsParallel() makes this slower, too many threads
            matched = modules.FirstOrDefault(WithinBounds);
            if (matched != null)
            {
                return true;
            }

            return dlc != null && dlc.TryGetValue(name, out UnmanagedModuleVersion? dlcVer)
                               && WithinBounds(dlcVer);
        }

        public override List<CkanModule> LatestAvailableWithProvides(IRegistryQuerier                 registry,
                                                                     StabilityToleranceConfig         stabilityTolerance,
                                                                     GameVersionCriteria?             crit,
                                                                     IReadOnlyCollection<CkanModule>? installed = null,
                                                                     IReadOnlyCollection<CkanModule>? toInstall = null)
            => registry.LatestAvailableWithProvides(name, stabilityTolerance,
                                                    crit, this, installed, toInstall);

        public override CkanModule? ExactMatch(IRegistryQuerier                 registry,
                                               StabilityToleranceConfig         stabilityTolerance,
                                               GameVersionCriteria?             crit,
                                               IReadOnlyCollection<CkanModule>? installed = null,
                                               IReadOnlyCollection<CkanModule>? toInstall = null)
            => Utilities.DefaultIfThrows(() => registry.LatestAvailable(name, stabilityTolerance,
                                                                        crit, this, installed, toInstall));

        public override bool Equals(RelationshipDescriptor? other)
            => Equals(other as ModuleRelationshipDescriptor);

        protected bool Equals(ModuleRelationshipDescriptor? other)
            => other != null
                && name        == other.name
                && version     == other.version
                && min_version == other.min_version
                && max_version == other.max_version;

        public override int GetHashCode()
            => (name, version, min_version, max_version).GetHashCode();

        public override bool ContainsAny(IEnumerable<string> identifiers)
            => identifiers.Contains(name);

        public override bool StartsWith(string prefix)
            => name.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Generate a user readable description of the relationship
        /// </summary>
        /// <returns>
        /// Depending on the version properties, one of:
        /// name
        /// name version
        /// name min_version -- max_version
        /// name min_version or later
        /// name max_version or earlier
        /// </returns>
        public override string ToString()
            =>    version     != null                        ? $"{name} {version}"
                : min_version != null && max_version != null ? $"{name} {min_version}–{max_version}"
                : min_version != null
                    ? string.Format(Properties.Resources.RelationshipDescriptorMinVersionOnly, name, min_version)
                : max_version != null
                    ? string.Format(Properties.Resources.RelationshipDescriptorMaxVersionOnly, name, max_version)
                : name;

        public override string ToStringWithCompat(IRegistryQuerier registry,
                                                  IGame            game)
            => Utilities.DefaultIfThrows(() => registry.AllAvailableByProvides(name)
                                                       .SelectMany(avail => avail.AllAvailable())
                                                       .Where(WithinBounds)
                                                       .ToArray())
               is CkanModule[] { Length: > 0 } modules
                   ? string.Format("{0} ({1})", ToString(),
                                                DescribeCompatibility(modules, game))
                   : ToString();

        private static string DescribeCompatibility(CkanModule[] modules,
                                                    IGame        game)
        {
            CkanModule.GetMinMaxVersions(modules,
                                         out _, out _,
                                         out var minKsp, out var maxKsp);
            return GameVersionRange.VersionSpan(game,
                                                minKsp ?? GameVersion.Any,
                                                maxKsp ?? GameVersion.Any);
        }
    }

    public class AnyOfRelationshipDescriptor : RelationshipDescriptor
    {
        [JsonProperty("any_of", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonRelationshipConverter))]
        public List<RelationshipDescriptor>? any_of;

        public static readonly List<string> ForbiddenPropertyNames = new List<string>()
        {
            "name",
            "version",
            "min_version",
            "max_version",
        };

        public override bool WithinBounds(CkanModule otherModule)
            => any_of?.Any(r => r.WithinBounds(otherModule)) ?? false;

        public override bool MatchesAny(IReadOnlyCollection<CkanModule>              modules,
                                        IReadOnlyCollection<string>?                 dlls,
                                        IDictionary<string, UnmanagedModuleVersion>? dlc,
                                        out CkanModule?                              matched)
        {
            matched = any_of?.AsParallel()
                             .Select(rel => rel.MatchesAny(modules, dlls, dlc, out CkanModule? whatMached)
                                                ? whatMached
                                                : null)
                             .FirstOrDefault(m => m != null);
            return matched != null;
        }

        public override List<CkanModule> LatestAvailableWithProvides(IRegistryQuerier                 registry,
                                                                     StabilityToleranceConfig         stabilityTolerance,
                                                                     GameVersionCriteria?             crit,
                                                                     IReadOnlyCollection<CkanModule>? installed = null,
                                                                     IReadOnlyCollection<CkanModule>? toInstall = null)
            => (any_of?.SelectMany(r => r.LatestAvailableWithProvides(registry, stabilityTolerance, crit, installed, toInstall))
                       .Distinct()
                      ?? Enumerable.Empty<CkanModule>())
                      .ToList();

        // Exact match is not possible for any_of
        public override CkanModule? ExactMatch(IRegistryQuerier                 registry,
                                               StabilityToleranceConfig         stabilityTolerance,
                                               GameVersionCriteria?             crit,
                                               IReadOnlyCollection<CkanModule>? installed = null,
                                               IReadOnlyCollection<CkanModule>? toInstall = null)
            => null;

        public override bool Equals(RelationshipDescriptor? other)
            => Equals(other as AnyOfRelationshipDescriptor);

        protected bool Equals(AnyOfRelationshipDescriptor? other)
            => other != null
                && (any_of?.SequenceEqual(other.any_of ?? Enumerable.Empty<RelationshipDescriptor>())
                          ?? (other.any_of == null));

        public override int GetHashCode()
            => any_of?.ToSequenceHashCode() ?? 0;

        public override bool ContainsAny(IEnumerable<string> identifiers)
            => any_of?.Any(r => r.ContainsAny(identifiers)) ?? false;

        public override bool StartsWith(string prefix)
            => any_of?.Any(r => r.StartsWith(prefix)) ?? false;

        public override string ToString()
            => any_of?.Select(r => r.ToString())
                .Aggregate((a, b) =>
                    string.Format(Properties.Resources.RelationshipDescriptorAnyOfJoiner, a, b))
                     ?? "";

        public override string ToStringWithCompat(IRegistryQuerier registry, IGame game)
            => any_of?.Select(r => r.ToStringWithCompat(registry, game))
                .Aggregate((a, b) =>
                    string.Format(Properties.Resources.RelationshipDescriptorAnyOfJoiner, a, b))
                     ?? "";
    }
}
