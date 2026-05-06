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

    public abstract class ProviderRejection
    {
        public readonly CkanModule provider;
        protected ProviderRejection(CkanModule provider)
        {
            this.provider = provider;
        }
    }

    public sealed class RejectedByRelationship : ProviderRejection
    {
        public readonly Relationship violation;
        public RejectedByRelationship(CkanModule provider, Relationship violation)
            : base(provider)
        {
            this.violation = violation;
        }

        public override bool Equals(object? other)
            => other is RejectedByRelationship r
               && provider.Equals(r.provider)
               && violation.Equals(r.violation);

        public override int GetHashCode()
            => (provider, violation).GetHashCode();
    }

    public sealed class RejectedByProvidesConflict : ProviderRejection
    {
        public readonly string     providedIdentifier;
        public readonly CkanModule blockingMod;
        public readonly bool       blockerIsInstalled;
        public RejectedByProvidesConflict(CkanModule provider,
                                          string     providedIdentifier,
                                          CkanModule blockingMod,
                                          bool       blockerIsInstalled)
            : base(provider)
        {
            this.providedIdentifier = providedIdentifier;
            this.blockingMod        = blockingMod;
            this.blockerIsInstalled = blockerIsInstalled;
        }

        public override bool Equals(object? other)
            => other is RejectedByProvidesConflict r
               && provider.Equals(r.provider)
               && providedIdentifier == r.providedIdentifier
               && blockingMod.Equals(r.blockingMod)
               && blockerIsInstalled == r.blockerIsInstalled;

        public override int GetHashCode()
            => (provider, providedIdentifier, blockingMod, blockerIsInstalled).GetHashCode();
    }

    public sealed class ResolutionContext
    {
        public readonly IRegistryQuerier                Registry;
        public readonly IReadOnlyCollection<CkanModule> Installed;
        public readonly IReadOnlyCollection<CkanModule> Installing;
        public readonly StabilityToleranceConfig        StabilityTolerance;
        public readonly GameVersionCriteria             Crit;

        public ResolutionContext(IRegistryQuerier                registry,
                                 IReadOnlyCollection<CkanModule> installed,
                                 IReadOnlyCollection<CkanModule> installing,
                                 StabilityToleranceConfig        stabilityTolerance,
                                 GameVersionCriteria             crit)
        {
            Registry           = registry;
            Installed          = installed;
            Installing         = installing;
            StabilityTolerance = stabilityTolerance;
            Crit               = crit;
        }
    }

    public sealed class RejectedByVersionMismatch : ProviderRejection
    {
        public readonly CkanModule blockingMod;
        public RejectedByVersionMismatch(CkanModule provider, CkanModule blockingMod)
            : base(provider)
        {
            this.blockingMod = blockingMod;
        }

        public override bool Equals(object? other)
            => other is RejectedByVersionMismatch r
               && provider.Equals(r.provider)
               && blockingMod.Equals(r.blockingMod);

        public override int GetHashCode()
            => (provider, blockingMod).GetHashCode();
    }

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


        public virtual IEnumerable<UnsatisfiedRelation> UnsatisfiedFrom()
            => reason is SelectionReason.Depends && Unsatisfied()
                   ? Enumerable.Repeat(new UnsatisfiedRelation(new ResolvedRelationship[] { this }, null), 1)
                   : Enumerable.Empty<UnsatisfiedRelation>();

        public virtual IReadOnlyCollection<UnsatisfiedRelation> BadRelationships(IReadOnlyCollection<CkanModule> installing)
            => Array.Empty<UnsatisfiedRelation>();
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
            => source == newSrc ? this
                                : new ResolvedByInstalled(newSrc, relationship, newRsn, installed);
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
            => source == newSrc ? this
                                : new ResolvedByInstalling(newSrc, relationship, newRsn, installing);
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
            => source == newSrc ? this
                                : new ResolvedByDLL(newSrc, relationship, newRsn);
    }

    public class ResolvedByNew : ResolvedRelationship
    {
        public ResolvedByNew(CkanModule                                              source,
                             RelationshipDescriptor                                  relationship,
                             SelectionReason                                         reason,
                             IReadOnlyDictionary<CkanModule, ResolvedRelationship[]> resolved,
                             ResolutionContext?                                      context = null)
              : base(source, relationship, reason)
        {
            this.resolved = resolved;
            this.context  = context;
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
                             IReadOnlyCollection<CkanModule> providers,
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
                                                       // If we are choosing between multiple options,
                                                       // cache the branches separately
                                                       providers.Count == 1
                                                           ? relationshipCache
                                                           : new RelationshipCache(relationshipCache))
                                                       .ToArray()),
                    new ResolutionContext(registry, installed,
                                          allInstalling.Append(source).ToArray(),
                                          stabilityTolerance, crit))
        {
        }

        /// <summary>
        /// The modules that can satisfy this relationship and their own relationships.
        /// If this is empty, then the relationship cannot be satisfied.
        /// </summary>
        public readonly IReadOnlyDictionary<CkanModule, ResolvedRelationship[]> resolved;

        /// <summary>
        /// The world this relationship was resolved against, if known.
        /// </summary>
        public readonly ResolutionContext? context;

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
            => source == newSrc ? this
                                : new ResolvedByNew(newSrc, relationship, newRsn, resolved, context);


        public override IEnumerable<UnsatisfiedRelation> UnsatisfiedFrom()
        {
            // Our goal here is to return an array of ResolvedRelationships for each full
            // trace from rr to a relationship we can't satisfy.
            // First we need to make sure we even care about this one, i.e. that it's required.
            if (reason is SelectionReason.Depends)
            {
                // Now if this relationship itself can't be resolved directly, return it.
                if (Unsatisfied())
                {
                    return Enumerable.Repeat(new UnsatisfiedRelation(new ResolvedRelationship[] { this }, null), 1);
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
                    ? Enumerable.Empty<UnsatisfiedRelation>()
                    : unsats.SelectMany(uns => uns.Select(u => new UnsatisfiedRelation(
                                                                   u.depends.Prepend(this).ToArray(),
                                                                   u.rejection)));
            }
            return Enumerable.Empty<UnsatisfiedRelation>();
        }

        public override IReadOnlyCollection<UnsatisfiedRelation> BadRelationships(IReadOnlyCollection<CkanModule> installing)
        {
            if (reason is SelectionReason.Depends)
            {
                var unsatisfied = new List<UnsatisfiedRelation>();
                foreach ((CkanModule module, ResolvedRelationship[] resRels) in resolved)
                {
                    if (module.BadRelationships(installing)
                              .Select(r => new UnsatisfiedRelation(new ResolvedRelationship[] { this },
                                                                   new RejectedByRelationship(module, r)))
                              .ToArray()
                        is { Length: > 0 } badRels)
                    {
                        unsatisfied.AddRange(badRels);
                    }
                    else if (resRels.SelectMany(rr => rr.BadRelationships(installing))
                                    .Select(u => new UnsatisfiedRelation(u.depends.Prepend(this).ToArray(),
                                                                         u.rejection))
                                    .ToArray()
                             is { Length: > 0 } badRRs)
                    {
                        unsatisfied.AddRange(badRRs);
                    }
                    else
                    {
                        // This relationship is satisfied
                        return Array.Empty<UnsatisfiedRelation>();
                    }
                }
                return unsatisfied;
            }
            return Array.Empty<UnsatisfiedRelation>();
        }

        /// <summary>
        /// Re-resolves this relationship without filters and returns a list of
        /// <see cref="UnsatisfiedRelation"/> that describe why each candidate
        /// would be rejected.
        /// </summary>
        public IEnumerable<UnsatisfiedRelation> UnsatisfiedCandidates()
        {
            if (context == null)
            {
                yield break;
            }

            foreach (var module in relationship.LatestAvailableWithProvides(
                         context.Registry, context.StabilityTolerance, context.Crit, null, null))
            {
                var rejection = FindProvidesConflict(module, context.Installing, context.Installed)
                                ?? module.BadRelationships(context.Installed)
                                          .Concat(module.BadRelationships(context.Installing))
                                          .Select(r => (ProviderRejection)new RejectedByRelationship(module, r))
                                          .FirstOrDefault();
                if (rejection != null)
                {
                    yield return new UnsatisfiedRelation(
                        new ResolvedRelationship[] { this }, rejection);
                }
            }
        }

        public static ProviderRejection? FindProvidesConflict(
            CkanModule                      candidate,
            IReadOnlyCollection<CkanModule> installing,
            IReadOnlyCollection<CkanModule> installed)
        {
            if (candidate.provides == null)
            {
                return null;
            }

            foreach (var providedId in candidate.provides)
            {
                var installedConflict = installed.FirstOrDefault(m => m.identifier != candidate.identifier
                                                                   && (m.identifier == providedId
                                                                       || (m.provides?.Contains(providedId) ?? false)));
                if (installedConflict != null)
                {
                    return new RejectedByProvidesConflict(candidate, providedId, installedConflict, blockerIsInstalled: true);
                }

                var installingConflict = installing.FirstOrDefault(m => m.identifier != candidate.identifier
                                                                     && (m.identifier == providedId
                                                                         || (m.provides?.Contains(providedId) ?? false)));
                if (installingConflict != null)
                {
                    return new RejectedByProvidesConflict(candidate, providedId, installingConflict, blockerIsInstalled: false);
                }
            }

            return null;
        }
    }

    public sealed class UnsatisfiedRelation
    {
        /// <summary>
        /// The dependency chain to reach this relationship.
        /// </summary>
        public readonly ResolvedRelationship[] depends;

        /// <summary>
        /// The reason that this relationship could not be satisfied, if any.
        /// </summary>
        public readonly ProviderRejection? rejection;

        public UnsatisfiedRelation(ResolvedRelationship[] depends,
                                   ProviderRejection? rejection)
        {
            this.depends = depends;
            this.rejection = rejection;
        }
    }
}
