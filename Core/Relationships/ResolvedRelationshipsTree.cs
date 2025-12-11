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

        public IEnumerable<ResolvedRelationship[]> Unsatisfied()
            => resolved.SelectMany(rr => rr.UnsatisfiedFrom());

        public IReadOnlyList<CkanModule> Candidates(RelationshipDescriptor          rel,
                                                    IReadOnlyCollection<CkanModule> installing,
                                                    IRegistryQuerier                registry,
                                                    IGame                           game)
        {
            var candidates = new List<CkanModule>();
            var unresolved = new List<ResolvedRelationship[]>();
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
                        if (module.BadRelationships(installing)
                                  .Select(r => relationshipCache.GetValueOrDefault(r.Descriptor))
                                  .OfType<ResolvedRelationship>()
                                  .Select(badRR => new ResolvedRelationship[] { resRel, badRR })
                                  .ToArray()
                            is { Length: > 0 } badRels)
                        {
                            unresolved.AddRange(badRels);
                        }
                        else if (rrs.SelectMany(subRR => subRR.BadRelationships(installing))
                                    .Select(tuple => tuple.Item2.Type == RelationshipType.Depends
                                                     && relationshipCache.TryGetValue(tuple.Item2.Descriptor,
                                                                                      out ResolvedRelationship? leaf)
                                                         ? tuple.Item1.Prepend(resRel).Append(leaf).ToArray()
                                                         : tuple.Item1.Prepend(resRel).ToArray())
                                    .ToArray()
                                 is { Length: > 0 } badRRs)
                        {
                            unresolved.AddRange(badRRs);
                        }
                        else
                        {
                            candidates.Add(module);
                        }
                    }
                    break;
            }
            if (candidates.Count == 0)
            {
                if (unresolved.Count > 0)
                {
                    throw new DependenciesNotSatisfiedKraken(unresolved, registry, game, this);
                }
                else
                {
                    throw new InconsistentKraken(string.Format(Properties.Resources.ResolvedRelationshipsTreeUnsatisfied,
                                                               rel,
                                                               string.Join(Environment.NewLine,
                                                                           installing.OrderBy(i => i.identifier))));
                }
            }
            return candidates;
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
            => relationshipCache.TryGetValue(relationship,
                                             out ResolvedRelationship? cachedRel)
                ? cachedRel.WithSource(source, reason)
                : relationship.MatchesAny(installed, dlls, registry.InstalledDlc,
                                          out CkanModule? installedMatch)
                        ? relationshipCache.GetOrAdd(
                            relationship,
                            installedMatch == null ? new ResolvedByDLL(source, relationship, reason)
                                                   : new ResolvedByInstalled(source, relationship, reason,
                                                                             installedMatch))
                        : relationship.MatchesAny(allInstalling, null, null,
                                                  out CkanModule? installingMatch)
                          && installingMatch != null
                            // Installing mods are branch-specific, so don't cache them
                            ? new ResolvedByInstalling(source, relationship, reason, installingMatch)
                            : relationshipCache.GetOrAdd(
                                relationship,
                                new ResolvedByNew(source, relationship, reason,
                                                  relationship.LatestAvailableWithProvides(registry, stabilityTolerance, crit,
                                                                                           installed, definitelyInstalling),
                                                  definitelyInstalling,
                                                  allInstalling.Append(source).ToArray(),
                                                  registry, dlls, installed, stabilityTolerance, crit, optRels,
                                                  relationshipCache));

        private readonly ResolvedRelationship[] resolved;
        private readonly RelationshipCache      relationshipCache = new RelationshipCache();
    }
}
