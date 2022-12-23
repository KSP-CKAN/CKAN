using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{
    public abstract class RelationshipDescriptor : IEquatable<RelationshipDescriptor>
    {
        public bool MatchesAny(
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, ModuleVersion> dlc
        )
        {
            return MatchesAny(modules, dlls, dlc, out CkanModule _);
        }

        public abstract bool MatchesAny(
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, ModuleVersion> dlc,
            out CkanModule matched
        );

        public abstract bool WithinBounds(CkanModule otherModule);

        public abstract List<CkanModule> LatestAvailableWithProvides(
            IRegistryQuerier registry, GameVersionCriteria crit, IEnumerable<CkanModule> installed = null,
            IEnumerable<CkanModule> toInstall = null);

        public abstract CkanModule ExactMatch(
            IRegistryQuerier registry, GameVersionCriteria crit, IEnumerable<CkanModule> installed = null,
            IEnumerable<CkanModule> toInstall = null);

        public abstract bool Equals(RelationshipDescriptor other);

        public abstract bool ContainsAny(IEnumerable<string> identifiers);

        public abstract bool StartsWith(string prefix);

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string choice_help_text;

        // virtual ToString() already present in 'object'
    }

    public class ModuleRelationshipDescriptor : RelationshipDescriptor
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion max_version;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion min_version;
        //Why is the identifier called name?
        public /* required */ string name;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ModuleVersion version;

        public override bool WithinBounds(CkanModule otherModule)
        {
            return otherModule.ProvidesList.Contains(name)
                && WithinBounds(otherModule.version);
        }

        /// <summary>
        /// Returns if the other version satisfies this RelationshipDescriptor.
        /// If the RelationshipDescriptor has version set it compares against that.
        /// Else it uses the {min,max}_version fields treating nulls as unbounded.
        /// Note: Uses inclusive inequalities.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if other_version is within the bounds</returns>
        public bool WithinBounds(ModuleVersion other)
        {
            // UnmanagedModuleVersions with unknown versions always satisfy the bound
            if (other is UnmanagedModuleVersion unmanagedModuleVersion && unmanagedModuleVersion.IsUnknownVersion)
                return true;

            if (version == null)
            {
                if (max_version == null && min_version == null)
                    return true;

                var minSat = min_version == null || min_version <= other;
                var maxSat = max_version == null || max_version >= other;

                if (minSat && maxSat)
                    return true;
            }
            else
            {
                if (version.Equals(other))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check whether any of the modules in a given list match this descriptor.
        /// NOTE: Only proper modules and DLC can be checked for versions!
        ///       DLLs match all versions, as do "provides" clauses.
        /// </summary>
        /// <param name="modules">Sequence of modules to consider</param>
        /// <param name="dlls">Sequence of DLLs to consider</param>
        /// <param name="dlc">DLC to consider</param>
        /// <returns>
        /// true if any of the modules match this descriptor, false otherwise.
        /// </returns>
        public override bool MatchesAny(
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, ModuleVersion> dlc,
            out CkanModule matched
        )
        {
            modules = modules?.AsCollection();

            // DLLs are considered to match any version
            if (dlls != null && dlls.Contains(name))
            {
                matched = null;
                return true;
            }

            if (modules != null)
            {
                // See if anyone else "provides" the target name
                // Note that versions can't be checked for "provides" clauses
                var matches = modules
                    .Where(m =>
                        m.identifier != name
                        && m.provides != null
                        && m.provides.Contains(name))
                    .ToList();
                if (matches.Any())
                {
                    matched = matches.FirstOrDefault();
                    return true;
                }

                // See if the real thing is there
                foreach (var m in modules.Where(m => m.identifier == name))
                {
                    if (WithinBounds(m))
                    {
                        matched = m;
                        return true;
                    }
                }
            }

            if (dlc != null)
            {
                foreach (var d in dlc.Where(i => i.Key == name))
                {
                    if (WithinBounds(d.Value))
                    {
                        matched = null;
                        return true;
                    }
                }
            }

            matched = null;
            return false;
        }

        public override List<CkanModule> LatestAvailableWithProvides(
            IRegistryQuerier registry, GameVersionCriteria crit, IEnumerable<CkanModule> installed = null,
            IEnumerable<CkanModule> toInstall = null)
            => registry.LatestAvailableWithProvides(name, crit, this, installed, toInstall);

        public override CkanModule ExactMatch(
            IRegistryQuerier registry, GameVersionCriteria crit, IEnumerable<CkanModule> installed = null,
            IEnumerable<CkanModule> toInstall = null)
            => registry.LatestAvailableWithProvides(name, crit, this, installed, toInstall)
                .FirstOrDefault(mod => mod.identifier == name);

        public override bool Equals(RelationshipDescriptor other)
        {
            ModuleRelationshipDescriptor modRel = other as ModuleRelationshipDescriptor;
            return modRel != null
                && name == modRel.name
                && version == modRel.version
                && min_version == modRel.min_version
                && max_version == modRel.max_version;
        }

        public override bool ContainsAny(IEnumerable<string> identifiers) => identifiers.Contains(name);

        public override bool StartsWith(string prefix)
            => name.IndexOf(prefix, StringComparison.CurrentCultureIgnoreCase) == 0;

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
            => version != null ? $"{name} {version}"
                : min_version != null && max_version != null ? $"{name} {min_version}–{max_version}"
                : min_version != null
                    ? string.Format(Properties.Resources.RelationshipDescriptorMinVersionOnly, name, min_version)
                : max_version != null
                    ? string.Format(Properties.Resources.RelationshipDescriptorMaxVersionOnly, name, max_version)
                : name;

    }

    public class AnyOfRelationshipDescriptor : RelationshipDescriptor
    {
        [JsonProperty("any_of", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonRelationshipConverter))]
        public List<RelationshipDescriptor> any_of;

        public static readonly List<string> ForbiddenPropertyNames = new List<string>()
        {
            "name",
            "version",
            "min_version",
            "max_version"
        };

        public override bool WithinBounds(CkanModule otherModule)
        {
            return any_of?.Any(r => r.WithinBounds(otherModule))
                ?? false;
        }

        public override bool MatchesAny(
            IEnumerable<CkanModule> modules,
            HashSet<string> dlls,
            IDictionary<string, ModuleVersion> dlc,
            out CkanModule matched
        )
        {
            if (any_of != null)
            {
                foreach (RelationshipDescriptor rel in any_of)
                {
                    if (rel.MatchesAny(modules, dlls, dlc, out CkanModule whatMatched))
                    {
                        matched = whatMatched;
                        return true;
                    }
                }
            }
            matched = null;
            return false;
        }

        public override List<CkanModule> LatestAvailableWithProvides(
            IRegistryQuerier registry, GameVersionCriteria crit, IEnumerable<CkanModule> installed = null,
            IEnumerable<CkanModule> toInstall = null)
            => any_of?.SelectMany(r => r.LatestAvailableWithProvides(registry, crit, installed, toInstall)).Distinct().ToList();

        // Exact match is not possible for any_of
        public override CkanModule ExactMatch(
            IRegistryQuerier registry, GameVersionCriteria crit, IEnumerable<CkanModule> installed = null,
            IEnumerable<CkanModule> toInstall = null)
            => null;

        public override bool Equals(RelationshipDescriptor other)
        {
            AnyOfRelationshipDescriptor anyRel = other as AnyOfRelationshipDescriptor;
            return anyRel != null
                && (any_of?.SequenceEqual(anyRel.any_of) ?? anyRel.any_of == null);
        }

        public override bool ContainsAny(IEnumerable<string> identifiers)
            => any_of?.Any(r => r.ContainsAny(identifiers)) ?? false;

        public override bool StartsWith(string prefix)
            => any_of?.Any(r => r.StartsWith(prefix)) ?? false;

        public override string ToString()
            => any_of?.Select(r => r.ToString())
                .Aggregate((a, b) =>
                    string.Format(Properties.Resources.RelationshipDescriptorAnyOfJoiner, a, b));
    }
}
