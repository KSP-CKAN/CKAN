using System;
using System.Collections.Generic;
using System.Linq;

using CKAN.Versioning;

namespace CKAN
{
    public enum HistoryChangeType
    {
        Added,
        Removed,
        Upgraded,
        Downgraded,
        Unchanged,
    }

    /// <summary>
    /// A difference in one mod between two installation history entries
    /// </summary>
    public class HistoryChange
    {
        public HistoryChange(string identifier, ModuleVersion? before, ModuleVersion? after)
        {
            Identifier = identifier;
            Before     = before;
            After      = after;
        }

        public readonly string         Identifier;
        public readonly ModuleVersion? Before;
        public readonly ModuleVersion? After;

        public HistoryChangeType Type
        {
            get
            {
                if (Before == null)
                {
                    return HistoryChangeType.Added;
                }
                if (After == null)
                {
                    return HistoryChangeType.Removed;
                }
                var comparison = Before.CompareTo(After);
                return comparison < 0 ? HistoryChangeType.Upgraded
                     : comparison > 0 ? HistoryChangeType.Downgraded
                                      : HistoryChangeType.Unchanged;
            }
        }
    }

    public static class InstallHistoryDiff
    {
        /// <summary>
        /// Find the mods that differ between two installation history entries
        /// </summary>
        /// <param name="older">The entry to compare from, null if there was nothing installed before</param>
        /// <param name="newer">The entry to compare to</param>
        /// <returns>
        /// Changes sorted by identifier, mods with the same version are omitted
        /// </returns>
        public static IReadOnlyList<HistoryChange> Between(CkanModule? older, CkanModule newer)
        {
            var before = Versions(older);
            var after  = Versions(newer);
            return before.Keys
                         .Union(after.Keys)
                         .OrderBy(ident => ident, StringComparer.OrdinalIgnoreCase)
                         .Select(ident => new HistoryChange(ident,
                                                            before.TryGetValue(ident, out ModuleVersion? b) ? b : null,
                                                            after.TryGetValue(ident, out ModuleVersion? a) ? a : null))
                         .Where(change => change.Type != HistoryChangeType.Unchanged)
                         .ToArray();
        }

        private static Dictionary<string, ModuleVersion> Versions(CkanModule? modpack)
            => (modpack?.depends ?? new List<RelationshipDescriptor>())
                   .OfType<ModuleRelationshipDescriptor>()
                   .Where(rel => rel.version != null)
                   .GroupBy(rel => rel.name)
                   .ToDictionary(grp => grp.Key,
                                 grp => grp.First().version!);
    }
}
