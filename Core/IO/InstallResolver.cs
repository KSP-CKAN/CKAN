using System;
using System.Collections.Generic;
using System.Linq;

using CKAN.Configuration;
using CKAN.Versioning;

namespace CKAN.IO
{
    public enum InstallOutcome
    {
        Ready,
        AlreadyInstalled,
        PinMissed, // The pinned version isn't in the registry.
        Incompatible,
        Unknown,
    }

    public readonly struct ResolvedMod
    {
        public ResolvedMod(string query, CkanModule? module, InstallOutcome outcome)
        {
            Query = query;
            Module = module;
            Outcome = outcome;
        }

        public readonly string Query;
        public readonly CkanModule? Module;
        public readonly InstallOutcome Outcome;
    }

    public static class InstallResolver
    {
        /// <summary>
        /// Resolve the mods from an install URL in a single pass over the registry.
        /// </summary>
        public static List<ResolvedMod> Resolve(
            IEnumerable<(string Mod, string? Version)> mods,
            IRegistryQuerier registry,
            StabilityToleranceConfig stability,
            GameVersionCriteria versions)
        {
            // Matching is case insensitive so a hand written URL needn't have perfect capitalisation.
            // The spec requires identifiers to be unique regardless of case.            
            var find = new Dictionary<string, (string Query, string? Version)>(StringComparer.OrdinalIgnoreCase);
            foreach (var (mod, version) in mods)
            {
                find[mod] = (mod, version); // Tolerate duplicates by re-assignment. The equivelant LINQ is ugly.
            }

            var found = new Dictionary<string, ResolvedMod>(StringComparer.OrdinalIgnoreCase);

            // Remove target mods from find and add to found as they are found with compatible modules.
            TakeMatches(registry.CompatibleModules(stability, versions), true, find, found, registry, versions);

            // Any remaining mods may be incompatible, so check those too.
            if (find.Count > 0)
            {
                TakeMatches(registry.IncompatibleModules(stability, versions), false, find, found, registry, versions);
            }

            // Anything left now is definitely not in the registry for this game.
            foreach (var (key, want) in find)
            {
                found[key] = new ResolvedMod(want.Query, null, InstallOutcome.Unknown);
            }

            // One result per mod, in the order the URL listed them.
            return mods.Select(m => m.Mod)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .Select(key => found[key])
                       .ToList();
        }

        private static void TakeMatches(
            IEnumerable<CkanModule> modules,
            bool compatible,
            Dictionary<string, (string Query, string? Version)> find,
            Dictionary<string, ResolvedMod> found,
            IRegistryQuerier registry,
            GameVersionCriteria versions)
        {
            foreach (var module in modules)
            {
                if (find.Count == 0) { break; }
                if (find.TryGetValue(module.identifier, out var want))
                {
                    find.Remove(module.identifier);
                    found[module.identifier] = Classify(want.Query, module, compatible, want.Version, registry, versions);
                }
            }
        }

        private static ResolvedMod Classify(
            string query, CkanModule available, bool compatible, string? version, IRegistryQuerier registry,
            GameVersionCriteria versions)
        {
            CkanModule? pinned = version == null ? null : registry.GetModuleByVersionTolerant(available.identifier, version);
            CkanModule module = pinned ?? available;

            // A mod can come from `registry.CompatibleModules` but the pinned version may be incompatible.
            bool isCompatible = pinned == null ? compatible : module.IsCompatible(versions);

            var outcome = !isCompatible ? InstallOutcome.Incompatible
                : version != null && pinned == null ? InstallOutcome.PinMissed
                : module.Equals(registry.GetInstalledVersion(module.identifier)) ? InstallOutcome.AlreadyInstalled
                : InstallOutcome.Ready;

            return new ResolvedMod(query, module, outcome);
        }
    }
}
