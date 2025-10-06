using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using CKAN.Configuration;
using CKAN.Versioning;
using CKAN.Extensions;

namespace CKAN
{
    using RelationshipCache = ConcurrentDictionary<RelationshipDescriptor, ResolvedRelationship>;

    public abstract class ResolvedRelationship : IEquatable<ResolvedRelationship>
    {
        public ResolvedRelationship(CkanModule             source,
                                    RelationshipDescriptor relationship,
                                    SelectionReason        reason)
        {
            this.source       = source;
            this.relationship = relationship;
            this.reason       = reason;
        }

        public readonly CkanModule             source;
        public readonly RelationshipDescriptor relationship;
        public readonly SelectionReason        reason;

        public virtual bool Contains(CkanModule mod)
            => false;

        protected virtual bool Unsatisfied()
            => false;

        [ExcludeFromCodeCoverage]
        public override string ToString()
            => $"{source} {reason.GetType().Name} {relationship}";

        [ExcludeFromCodeCoverage]
        public virtual IEnumerable<string> ToLines()
            => Enumerable.Repeat(ToString(), 1);

        public abstract ResolvedRelationship WithSource(CkanModule      newSrc,
                                                        SelectionReason newRsn);

        public override bool Equals(object? other)
            => Equals(other as ResolvedRelationship);

        public bool Equals(ResolvedRelationship? other)
            => source.Equals(other?.source)
               && relationship.Equals(other?.relationship)
               && reason.Equals(other?.reason);

        public override int GetHashCode()
            => (source, relationship, reason).GetHashCode();

        public virtual IEnumerable<ResolvedRelationship[]> UnsatisfiedFrom()
            => reason is SelectionReason.Depends && Unsatisfied()
                   ? Enumerable.Repeat(new ResolvedRelationship[] { this }, 1)
                   : Enumerable.Empty<ResolvedRelationship[]>();

        public virtual IReadOnlyCollection<(ResolvedRelationship[], Relationship)> BadRelationships(IReadOnlyCollection<CkanModule> installing)
            => Array.Empty<(ResolvedRelationship[], Relationship)>();
    }

    public class ResolvedByInstalled : ResolvedRelationship
    {
        public ResolvedByInstalled(CkanModule             source,
                                   RelationshipDescriptor relationship,
                                   SelectionReason        reason,
                                   CkanModule             installed)
             : base(source, relationship, reason)
        {
            this.installed = installed;
        }

        public readonly CkanModule installed;

        public override bool Contains(CkanModule mod)
            => installed == mod;

        [ExcludeFromCodeCoverage]
        public override string ToString()
            => $"{source} {relationship}: Installed {installed}";

        public override ResolvedRelationship WithSource(CkanModule newSrc, SelectionReason newRsn)
            => new ResolvedByInstalled(newSrc, relationship, newRsn, installed);
    }

    public class ResolvedByInstalling : ResolvedRelationship
    {
        public ResolvedByInstalling(CkanModule             source,
                                    RelationshipDescriptor relationship,
                                    SelectionReason        reason,
                                    CkanModule             installing)
             : base(source, relationship, reason)
        {
            this.installing = installing;
        }

        public readonly CkanModule installing;

        public override bool Contains(CkanModule mod)
            => installing == mod;

        [ExcludeFromCodeCoverage]
        public override string ToString()
            => $"{base.ToString()}: Installing {installing}";

        public override ResolvedRelationship WithSource(CkanModule newSrc, SelectionReason newRsn)
            => new ResolvedByInstalling(newSrc, relationship, newRsn, installing);
    }

    public class ResolvedByDLL : ResolvedRelationship
    {
        public ResolvedByDLL(CkanModule             source,
                             RelationshipDescriptor relationship,
                             SelectionReason        reason)
             : base(source, relationship, reason)
        {
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
            => $"{base.ToString()}: DLL";

        public override ResolvedRelationship WithSource(CkanModule newSrc, SelectionReason newRsn)
            => new ResolvedByDLL(newSrc, relationship, newRsn);
    }

    public class ResolvedByNew : ResolvedRelationship
    {
        public ResolvedByNew(CkanModule                                              source,
                             RelationshipDescriptor                                  relationship,
                             SelectionReason                                         reason,
                             IReadOnlyDictionary<CkanModule, ResolvedRelationship[]> resolved)
              : base(source, relationship, reason)
        {
            this.resolved = resolved;
        }

        public ResolvedByNew(CkanModule             source,
                             RelationshipDescriptor relationship,
                             SelectionReason        reason)
             : this(source, relationship, reason,
                    new Dictionary<CkanModule, ResolvedRelationship[]>())
        {
        }

        public ResolvedByNew(CkanModule                      source,
                             RelationshipDescriptor          relationship,
                             SelectionReason                 reason,
                             IEnumerable<CkanModule>         providers,
                             IReadOnlyCollection<CkanModule> definitelyInstalling,
                             IReadOnlyCollection<CkanModule> allInstalling,
                             IRegistryQuerier                registry,
                             IReadOnlyCollection<string>     dlls,
                             IReadOnlyCollection<CkanModule> installed,
                             StabilityToleranceConfig        stabilityTolerance,
                             GameVersionCriteria             crit,
                             OptionalRelationships           optRels,
                             RelationshipCache               relationshipCache)
             : this(source, relationship, reason,
                    providers.ToDictionary(prov => prov,
                                           prov => ResolvedRelationshipsTree.ResolveModule(
                                                       prov, definitelyInstalling, allInstalling, registry, dlls, installed, stabilityTolerance, crit,
                                                       relationship.suppress_recommendations
                                                           ? optRels & ~OptionalRelationships.Recommendations
                                                                     & ~OptionalRelationships.Suggestions
                                                           : (optRels & OptionalRelationships.AllSuggestions) == 0
                                                               ? optRels & ~OptionalRelationships.Suggestions
                                                               : optRels,
                                                       relationshipCache)
                                                       .ToArray()))
        {
        }

        /// <summary>
        /// The modules that can satisfy this relationship and their own relationships.
        /// If this is empty, then the relationship cannot be satisfied.
        /// </summary>
        public readonly IReadOnlyDictionary<CkanModule, ResolvedRelationship[]> resolved;

        public override bool Contains(CkanModule mod)
            => resolved.Any(rr => rr.Key == mod || rr.Value.Any(rrr => rrr.Contains(mod)));

        protected override bool Unsatisfied()
            => reason is SelectionReason.Depends
               && !resolved.Keys.Any(m => !m.IsDLC);

        [ExcludeFromCodeCoverage]
        public override string ToString()
            => string.Join(Environment.NewLine, ToLines());

        [ExcludeFromCodeCoverage]
        public override IEnumerable<string> ToLines()
            => Enumerable.Repeat(resolved.Count > 0 ? $"{base.ToString()}:"
                                                    : $"UNRESOLVED {base.ToString()}", 1)
                         .Concat(resolved.SelectMany(kvp => Enumerable.Repeat(kvp.Value.Length > 0
                                                                                  ? $"Module {kvp.Key}:"
                                                                                  : $"Module {kvp.Key}", 1)
                                                                      .Concat(kvp.Value
                                                                                 .SelectMany(rr => rr.ToLines())
                                                                                 .Select(line => $"\t{line}")))
                                         .Select(line => $"\t{line}"));

        public override ResolvedRelationship WithSource(CkanModule newSrc, SelectionReason newRsn)
            => new ResolvedByNew(newSrc, relationship, newRsn, resolved);

        public override IEnumerable<ResolvedRelationship[]> UnsatisfiedFrom()
        {
            // Our goal here is to return an array of ResolvedRelationships for each full
            // trace from rr to a relationship we can't satisfy.
            // First we need to make sure we even care about this one, i.e. that it's required.
            if (reason is SelectionReason.Depends)
            {
                // Now if this relationship itself can't be resolved directly, return it.
                if (Unsatisfied())
                {
                    return Enumerable.Repeat(new ResolvedRelationship[] { this }, 1);
                }
                // Now we know it's a dependency that has at least one option for satisfying it,
                // but those options may or may not be fully satisfied when considering _their_ dependencies.

                // If any of these options works, then we want to return nothing.
                // Otherwise we want to return all of the descriptions of why everything failed,
                // with rr prepended to the start of each array.
                var unsats = resolved.Values.Select(modsRels => modsRels.SelectMany(rr => rr.UnsatisfiedFrom())
                                                                        .ToArray())
                                            .Memoize();

                return unsats.Any(u => u.Length == 0)
                    // One of the dependencies is fully satisfied
                    ? Enumerable.Empty<ResolvedRelationship[]>()
                    : unsats.SelectMany(uns => uns.Select(u => u.Prepend(this).ToArray()));
            }
            return Enumerable.Empty<ResolvedRelationship[]>();
        }

        public override IReadOnlyCollection<(ResolvedRelationship[], Relationship)> BadRelationships(IReadOnlyCollection<CkanModule> installing)
        {
            if (reason is SelectionReason.Depends)
            {
                var unsatisfied = new List<(ResolvedRelationship[], Relationship)>();
                foreach ((CkanModule module, ResolvedRelationship[] resRels) in resolved)
                {
                    if (module.BadRelationships(installing)
                              .Select(r => (new ResolvedRelationship[] { this }, r))
                              .ToArray()
                        is { Length: > 0 } badRels)
                    {
                        unsatisfied.AddRange(badRels);
                    }
                    else if (resRels.SelectMany(rr => rr.BadRelationships(installing))
                                    .Select(tuple => (tuple.Item1.Prepend(this).ToArray(),
                                                      tuple.Item2))
                                    .ToArray()
                             is { Length: > 0 } badRRs)
                    {
                        unsatisfied.AddRange(badRRs);
                    }
                    else
                    {
                        // This relationship is satisfied
                        return Array.Empty<(ResolvedRelationship[], Relationship)>();
                    }
                }
                return unsatisfied;
            }
            return Array.Empty<(ResolvedRelationship[], Relationship)>();
        }
    }
}
