using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using CKAN.Games;
using CKAN.Configuration;
using CKAN.Versioning;

namespace CKAN
{
    using RelationshipCache = ConcurrentDictionary<RelationshipDescriptor, ResolvedRelationship>;

    [Flags]
    public enum OptionalRelationships
    {
        None            = 0,
        Recommendations = 1,
        Suggestions     = 2,
        AllSuggestions  = 4,
    }

    public class ResolvedRelationshipsTree
    {
        public ResolvedRelationshipsTree(IReadOnlyCollection<CkanModule> modules,
                                         IRegistryQuerier                registry,
                                         IReadOnlyCollection<string>     dlls,
                                         IReadOnlyCollection<CkanModule> installed,
                                         StabilityToleranceConfig        stabilityTolerance,
                                         GameVersionCriteria             crit,
                                         OptionalRelationships           optRels)
        {
            this.modules = modules;
            this.registry = registry;
            this.installed = installed;
            this.stabilityTolerance = stabilityTolerance;
            this.crit = crit;
            resolved = ResolveManyCached(modules, registry, dlls, installed, stabilityTolerance, crit, optRels, relationshipCache).ToArray();
        }

        public static IEnumerable<ResolvedRelationship> ResolveModule(CkanModule                      module,
                                                                      IReadOnlyCollection<CkanModule> definitelyInstalling,
                                                                      IReadOnlyCollection<CkanModule> allInstalling,
                                                                      IRegistryQuerier                registry,
                                                                      IReadOnlyCollection<string>     dlls,
                                                                      IReadOnlyCollection<CkanModule> installed,
                                                                      StabilityToleranceConfig        stabilityTolerance,
                                                                      GameVersionCriteria             crit,
                                                                      OptionalRelationships           optRels,
                                                                      RelationshipCache               relationshipCache)
            => ResolveRelationships(module, module.depends, new SelectionReason.Depends(module),
                                    definitelyInstalling, allInstalling, registry, dlls, installed, stabilityTolerance, crit, optRels, relationshipCache)
                .Concat((optRels & OptionalRelationships.Recommendations) == 0
                    ? Enumerable.Empty<ResolvedRelationship>()
                    : ResolveRelationships(module, module.recommends, new SelectionReason.Recommended(module, 0),
                                           definitelyInstalling, allInstalling, registry, dlls, installed, stabilityTolerance, crit, optRels, relationshipCache))
                .Concat((optRels & OptionalRelationships.Suggestions) == 0
                    ? Enumerable.Empty<ResolvedRelationship>()
                    : ResolveRelationships(module, module.suggests, new SelectionReason.Suggested(module),
                                           definitelyInstalling, allInstalling, registry, dlls, installed, stabilityTolerance, crit, optRels, relationshipCache));

        public IEnumerable<UnsatisfiedRelation> Unsatisfied()
            => resolved.SelectMany(rr => rr.UnsatisfiedFrom())
                       .Select(EnrichRejection);

        // Sometimes we end up with a ResolvedByNew with no modules that satisfy it.
        // This augments the unsatisfied relation with potential candidates that were
        // filtered out so that we can give a better explanation of why they couldn't
        // be used.
        private UnsatisfiedRelation EnrichRejection(UnsatisfiedRelation u)
        {
            if (u.rejection != null
                || u.depends.LastOrDefault() is not ResolvedByNew last
                || last.resolved.Count > 0)
            {
                return u;
            }

            return UnsatisfiedCandidates(last.relationship, last, modules).FirstOrDefault() is { } found
                ? new UnsatisfiedRelation(u.depends, found.rejection)
                : u;
        }

        public IReadOnlyList<CkanModule> Candidates(RelationshipDescriptor          rel,
                                                    IReadOnlyCollection<CkanModule> installing,
                                                    IRegistryQuerier                registry,
                                                    IGame                           game)
        {
            var candidates = new List<CkanModule>();
            var unresolved = new List<UnsatisfiedRelation>();
            switch (relationshipCache.GetValueOrDefault(rel))
            {
                case ResolvedByInstalling resInstalling:
                    candidates.Add(resInstalling.installing);
                    break;

                case ResolvedByInstalled resInstalled:
                    candidates.Add(resInstalled.installed);
                    break;

                case ResolvedByNew resRel:
                    // We need to have this loop at this level to accumulate the list of candidates
                    foreach ((CkanModule module, ResolvedRelationship[] rrs) in resRel.resolved)
                    {
                        var versionClash = installing.FirstOrDefault(m => m.identifier == module.identifier
                                                                          && m.version  != module.version);
                        if (versionClash != null)
                        {
                            unresolved.Add(new UnsatisfiedRelation(new ResolvedRelationship[] { resRel },
                                                                   new RejectedByVersionMismatch(module, versionClash)));
                            continue;
                        }

                        var providesConflict = FindProvidesConflict(module, installing, installed);
                        if (providesConflict != null)
                        {
                            unresolved.Add(new UnsatisfiedRelation(new ResolvedRelationship[] { resRel },
                                                                   providesConflict));
                            continue;
                        }

                        if (module.BadRelationships(installing)
                                  .Select(r => new UnsatisfiedRelation(
                                                   relationshipCache.GetValueOrDefault(r.Descriptor) is ResolvedRelationship leaf
                                                       ? new ResolvedRelationship[] { resRel, leaf }
                                                       : new ResolvedRelationship[] { resRel },
                                                   new RejectedByRelationship(module, r)))
                                  .ToArray()
                            is { Length: > 0 } badRels)
                        {
                            unresolved.AddRange(badRels);
                            continue;
                        }
                        
                        if (rrs.SelectMany(subRR => subRR.BadRelationships(installing))
                                .Select(u =>
                                {
                                    ResolvedRelationship[] chain;
                                    if (u.rejection is RejectedByRelationship inner
                                        && inner.violation.Type == RelationshipType.Depends
                                        && relationshipCache.TryGetValue(inner.violation.Descriptor,
                                                                         out ResolvedRelationship? leaf))
                                    {
                                        chain = u.depends.Prepend(resRel).Append(leaf).ToArray();
                                    }
                                    else
                                    {
                                        chain = u.depends.Prepend(resRel).ToArray();
                                    }
                                    return new UnsatisfiedRelation(chain, u.rejection);
                                })
                                .ToArray()
                            is { Length: > 0 } badRRs)
                        {
                            unresolved.AddRange(badRRs);
                            continue;
                        }

                        candidates.Add(module);
                    }
                    break;
            }

            if (candidates.Count != 0)
            {
                return candidates;
            }

            // We weren't able to find anything. Redo the lookup without filters so
            // we can describe what couldn't be satisfied.
            if (unresolved.Count == 0
                && relationshipCache.GetValueOrDefault(rel) is ResolvedByNew cached)
            {
                unresolved.AddRange(UnsatisfiedCandidates(rel, cached, installing));
            }

            if (unresolved.Count == 0)
            {
                throw new InconsistentKraken(string.Format(
                    Properties.Resources.ResolvedRelationshipsTreeUnsatisfied,
                    rel,
                    string.Join(Environment.NewLine, installing.OrderBy(i => i.identifier))));
            }

            throw new DependenciesNotSatisfiedKraken(unresolved.Select(PrependAncestors).ToArray(),
                                                     registry, game, this);
        }

        // The chains we build inside Candidates() only see the ResolvedByNew that the
        // descriptor maps to; the wider ancestor context (e.g. Parent -> Intermediate ->
        // this) lives in other cache entries. Walk up by finding the cached ResolvedByNew
        // whose `resolved` keys contain the current head's source, and prepend it.
        private UnsatisfiedRelation PrependAncestors(UnsatisfiedRelation u)
        {
            if (u.depends.Length == 0)
            {
                return u;
            }
            var chain = u.depends.ToList();
            var current = chain[0];
            while (true)
            {
                var parent = relationshipCache.Values
                                              .OfType<ResolvedByNew>()
                                              .FirstOrDefault(r => r != current
                                                                   && !chain.Contains(r)
                                                                   && r.resolved.ContainsKey(current.source));
                if (parent == null)
                {
                    break;
                }
                chain.Insert(0, parent);
                current = parent;
            }
            return chain.Count == u.depends.Length
                ? u
                : new UnsatisfiedRelation(chain.ToArray(), u.rejection);
        }

        private IEnumerable<UnsatisfiedRelation> UnsatisfiedCandidates(
            RelationshipDescriptor          rel,
            ResolvedByNew                   resolved,
            IReadOnlyCollection<CkanModule> installing)
        {
            foreach (var module in rel.LatestAvailableWithProvides(registry, stabilityTolerance, crit, null, null))
            {
                var rejection = FindProvidesConflict(module, installing, installed)
                                ?? module.BadRelationships(installed)
                                            .Concat(module.BadRelationships(installing))
                                            .Select(r => (ProviderRejection)new RejectedByRelationship(module, r))
                                            .FirstOrDefault();
                if (rejection != null)
                {
                    yield return new UnsatisfiedRelation(
                        new ResolvedRelationship[] { resolved }, rejection);
                }
            }
        }

        private static ProviderRejection? FindProvidesConflict(
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

        [ExcludeFromCodeCoverage]
        public override string ToString()
            => string.Join(Environment.NewLine,
                           resolved.Select(rr => rr.ToString()));

        private static IEnumerable<ResolvedRelationship> ResolveManyCached(IReadOnlyCollection<CkanModule> modules,
                                                                           IRegistryQuerier                registry,
                                                                           IReadOnlyCollection<string>     dlls,
                                                                           IReadOnlyCollection<CkanModule> installed,
                                                                           StabilityToleranceConfig        stabilityTolerance,
                                                                           GameVersionCriteria             crit,
                                                                           OptionalRelationships           optRels,
                                                                           RelationshipCache               relationshipCache)
            => modules.SelectMany(m => ResolveModule(m, modules, modules, registry, dlls, installed, stabilityTolerance, crit, optRels,
                                                     relationshipCache));

        private static IEnumerable<ResolvedRelationship> ResolveRelationships(CkanModule                      module,
                                                                              List<RelationshipDescriptor>?   relationships,
                                                                              SelectionReason                 reason,
                                                                              IReadOnlyCollection<CkanModule> definitelyInstalling,
                                                                              IReadOnlyCollection<CkanModule> allInstalling,
                                                                              IRegistryQuerier                registry,
                                                                              IReadOnlyCollection<string>     dlls,
                                                                              IReadOnlyCollection<CkanModule> installed,
                                                                              StabilityToleranceConfig        stabilityTolerance,
                                                                              GameVersionCriteria             crit,
                                                                              OptionalRelationships           optRels,
                                                                              RelationshipCache               relationshipCache)
            => relationships?.Select(dep => Resolve(module, dep, reason,
                                                    definitelyInstalling, allInstalling, registry, dlls, installed,
                                                    stabilityTolerance, crit, optRels, relationshipCache))
                            ?? Enumerable.Empty<ResolvedRelationship>();

        private static ResolvedRelationship Resolve(CkanModule                      source,
                                                    RelationshipDescriptor          relationship,
                                                    SelectionReason                 reason,
                                                    IReadOnlyCollection<CkanModule> definitelyInstalling,
                                                    IReadOnlyCollection<CkanModule> allInstalling,
                                                    IRegistryQuerier                registry,
                                                    IReadOnlyCollection<string>     dlls,
                                                    IReadOnlyCollection<CkanModule> installed,
                                                    StabilityToleranceConfig        stabilityTolerance,
                                                    GameVersionCriteria             crit,
                                                    OptionalRelationships           optRels,
                                                    RelationshipCache               relationshipCache)
            => relationshipCache.GetOrAdd(relationship,
                                          rel => rel.MatchesAny(installed, dlls, registry.InstalledDlc,
                                                                out CkanModule? installedMatch)
                                                     ? installedMatch == null
                                                         ? new ResolvedByDLL(source, rel, reason)
                                                         : new ResolvedByInstalled(source, rel, reason, installedMatch)

                                               : rel.MatchesAny(allInstalling, null, null,
                                                                out CkanModule? installingMatch)
                                                 && installingMatch != null
                                                     ? new ResolvedByInstalling(source, rel, reason, installingMatch)

                                               : new ResolvedByNew(source, rel, reason,
                                                                   rel.LatestAvailableWithProvides(registry, stabilityTolerance, crit,
                                                                                                   installed, definitelyInstalling),
                                                                   definitelyInstalling, allInstalling.Append(source).ToArray(),
                                                                   registry, dlls, installed, stabilityTolerance,
                                                                   crit, optRels, relationshipCache))
                                .WithSource(source, reason);

        private readonly ResolvedRelationship[]          resolved;
        private readonly IReadOnlyCollection<CkanModule> modules;
        private readonly IRegistryQuerier                registry;
        private readonly IReadOnlyCollection<CkanModule> installed;
        private readonly StabilityToleranceConfig        stabilityTolerance;
        private readonly GameVersionCriteria             crit;
        private readonly RelationshipCache               relationshipCache = new RelationshipCache();
    }
}
