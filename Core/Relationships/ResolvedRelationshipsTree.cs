using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using CKAN.Configuration;
using CKAN.Versioning;
using CKAN.Extensions;

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
            => resolved.SelectMany(UnsatisfiedFrom);

        private static IEnumerable<ResolvedRelationship[]> UnsatisfiedFrom(ResolvedRelationship rr)
        {
            // Our goal here is to return an array of ResolvedRelationships for each full
            // trace from rr to a relationship we can't satisfy.
            // First we need to make sure we even care about this one, i.e. that it's required.
            if (rr.reason is SelectionReason.Depends)
            {
                // Now if this relationship itself can't be resolved directly, return it.
                if (rr.Unsatisfied())
                {
                    return Enumerable.Repeat(new ResolvedRelationship[] { rr }, 1);
                }
                // Now we know it's a dependency that has at least one option for satisfying it,
                // but those options may or may not be fully satisfied when considering _their_ dependencies.

                // If any of these options works, then we want to return nothing.
                // Otherwise we want to return all of the descriptions of why everything failed,
                // with rr prepended to the start of each array.
                if (rr is ResolvedByNew rbn)
                {
                    var unsats = rbn.resolved.Values
                                             .Select(modsRels => modsRels.SelectMany(UnsatisfiedFrom)
                                                                         .ToArray())
                                             .Memoize();

                    return unsats.Any(u => u.Length == 0)
                        // One of the dependencies is fully satisfied
                        ? Enumerable.Empty<ResolvedRelationship[]>()
                        : unsats.SelectMany(uns => uns.Select(u => u.Prepend(rr).ToArray()));
                }
            }
            return Enumerable.Empty<ResolvedRelationship[]>();
        }

        public IEnumerable<CkanModule> Candidates(RelationshipDescriptor          rel,
                                                  IReadOnlyCollection<CkanModule> installing)
            => relationshipCache.TryGetValue(rel, out ResolvedRelationship? rr)
               && rr is ResolvedByNew resRel
                   ? resRel.resolved
                           .Where(kvp => kvp.Key.DependsAndConflictsOK(installing)
                                         && kvp.Value.All(subRR => !subRR.Unsatisfied(installing)))
                           .Select(kvp => kvp.Key)
                   : Enumerable.Empty<CkanModule>();

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
