using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using CKAN.Versioning;

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

        public virtual bool Unsatisfied()
            => false;

        public virtual bool Unsatisfied(ICollection<CkanModule> installing)
            => false;

        public override string ToString()
            => $"{source} {reason.GetType().Name} {relationship}";

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

        public ResolvedByNew(CkanModule              source,
                             RelationshipDescriptor  relationship,
                             SelectionReason         reason,
                             IEnumerable<CkanModule> providers,
                             ICollection<CkanModule> definitelyInstalling,
                             ICollection<CkanModule> allInstalling,
                             IRegistryQuerier        registry,
                             ICollection<string>     dlls,
                             ICollection<CkanModule> installed,
                             GameVersionCriteria     crit,
                             OptionalRelationships   optRels,
                             RelationshipCache       relationshipCache)
             : this(source, relationship, reason,
                    providers.ToDictionary(prov => prov,
                                           prov => ResolvedRelationshipsTree.ResolveModule(
                                                       prov, definitelyInstalling, allInstalling, registry, dlls, installed, crit,
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

        public override bool Unsatisfied()
            => reason is SelectionReason.Depends
               && resolved.Keys.Count(m => !m.IsDLC) == 0;

        public override bool Unsatisfied(ICollection<CkanModule> installing)
            => reason is SelectionReason.Depends
               && !resolved.Any(kvp => !kvp.Key.IsDLC
                                       && AvailableModule.DependsAndConflictsOK(kvp.Key, installing)
                                       && kvp.Value.All(rr => !rr.Unsatisfied(installing)));

        public override string ToString()
            => string.Join(Environment.NewLine, ToLines());

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
    }
}
