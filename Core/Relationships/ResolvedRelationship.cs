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

        [ExcludeFromCodeCoverage]
        public virtual bool Contains(CkanModule mod)
            => false;

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
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

        [ExcludeFromCodeCoverage]
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
                                          new StabilityToleranceConfig(stabilityTolerance),
                                          crit))
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

        [ExcludeFromCodeCoverage]
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
                    if (RejectedByRelationship.WrapMany(module, module.BadRelationships(installing))
                            .Select(rej => new UnsatisfiedRelation(new ResolvedRelationship[] { this }, rej))
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
                var rejection = FindConflict(module, context.Installing, context.Installed)
                                ?? (RejectedProvider?)RejectedByRelationship.WrapMany(
                                       module,
                                       module.BadRelationships(context.Installed)
                                             .Concat(module.BadRelationships(context.Installing))
                                             .Where(r => r.Type == RelationshipType.Depends))
                                   .FirstOrDefault();
                if (rejection != null)
                {
                    yield return new UnsatisfiedRelation(
                        new ResolvedRelationship[] { this }, rejection);
                }
            }
        }

        // Find a mod that explicitly conflicts with the candidate (in either
        // direction). Returns a rejection naming both sides; if they happen to
        // share a virtual provides id, that's recorded for nicer messaging but
        // is not required for the rejection to fire.
        public static RejectedProvider? FindConflict(
            CkanModule                      candidate,
            IReadOnlyCollection<CkanModule> installing,
            IReadOnlyCollection<CkanModule> installed)
        {
            foreach (var blocker in installed)
            {
                if (blocker.identifier != candidate.identifier
                    && HasExplicitConflict(candidate, blocker))
                {
                    return new RejectedByConflict(candidate, SharedProvidesId(candidate, blocker),
                                                  blocker, blockerIsInstalled: true);
                }
            }

            foreach (var blocker in installing)
            {
                if (blocker.identifier != candidate.identifier
                    && HasExplicitConflict(candidate, blocker))
                {
                    return new RejectedByConflict(candidate, SharedProvidesId(candidate, blocker),
                                                  blocker, blockerIsInstalled: false);
                }
            }

            return null;
        }

        private static bool HasExplicitConflict(CkanModule candidate, CkanModule blocker)
            => candidate.BadRelationships(new[] { blocker })
                        .Any(r => r.Type == RelationshipType.Conflicts);

        // Best-effort: find a shared virtual id between the two mods so the
        // message can name it. Each side's identifier is treated as an implicit
        // provide so identifier-shadowing cases still produce a meaningful name.
        // Returns null when there is no shared id; the conflict still stands.
        private static string? SharedProvidesId(CkanModule candidate, CkanModule blocker)
        {
            var blockerIds = new HashSet<string>(blocker.provides ?? Enumerable.Empty<string>())
            {
                blocker.identifier,
            };

            return (candidate.provides ?? Enumerable.Empty<string>())
                       .Append(candidate.identifier)
                       .FirstOrDefault(blockerIds.Contains);
        }
    }

}
