using System;
using System.Collections.Generic;
using System.Linq;
using CKAN.Versioning;
using log4net;

namespace CKAN
{
    /// <summary>
    /// Sanity checks on what mods we have installed, or may install.
    /// </summary>
    public static class SanityChecker
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(SanityChecker));

        /// <summary>
        ///     Checks the list of modules for consistency errors, returning a list of
        ///     errors found. The list will be empty if everything is fine.
        /// </summary>
        public static ICollection<string> ConsistencyErrors(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls,
            IDictionary<string, UnmanagedModuleVersion> dlc
        )
        {
            var errors = new HashSet<string>();

            // If we have no modules, then everything is fine. DLLs can't depend or conflict on things.
            if (modules == null)
            {
                return errors;
            }

            foreach (KeyValuePair<string,List<CkanModule>> entry in FindUnmetDependencies(modules, dlls, dlc))
            {
                foreach (CkanModule unhappy_mod in entry.Value)
                {
                    // This error can fire if a dependency
                    // IS listed in the index AND IS available for our version,
                    // but not installed.
                    // This happens when `dlls` is Registry.installed_dlls.Keys,
                    // as in Registry.GetSanityErrors.
                    errors.Add(string.Format(
                        "{0} has an unmet dependency: {1} is not installed",
                        unhappy_mod.identifier,
                        entry.Key
                    ));
                }
            }

            // Conflicts are more difficult. Mods are allowed to conflict with themselves.
            // So we walk all our mod conflicts, find what (if anything) provide those
            // conflicts, and return false if it's not the module we're examining.

            // TODO: This doesn't examine versions. We should!
            // TODO: It would be great to factor this into its own function, too.

            var provided = ModulesToProvided(modules, dlls, dlc);
            var providedByProvideeIdentifier = provided.ToLookup(i => i.ProvideeIdentifier);

            foreach (CkanModule mod in modules)
            {
                // If our mod doesn't conflict with anything, skip it.
                if (mod.conflicts == null)
                {
                    continue;
                }

                foreach (var conflict in mod.conflicts)
                {
                    // If nothing conflicts with us, skip.
                    if (!providedByProvideeIdentifier.Contains(conflict.name))
                    {
                        continue;
                    }

                    // If something does conflict with us, and it's not ourselves, that's a fail.
                    foreach (var p in providedByProvideeIdentifier[conflict.name])
                    {
                        if (p.ProviderIdentifier != mod.identifier)
                        {
                            errors.Add(string.Format("{0} conflicts with {1}.", mod.identifier, p.ProviderIdentifier));
                        }
                    }
                }
            }

            // Return whatever we've found, which could be empty.
            return errors;
        }

        /// <summary>
        /// Ensures all modules in the list provided can co-exist.
        /// Throws a InconsistentKraken containing a list of inconsistences if they do not.
        /// Does nothing if the modules can happily co-exist.
        /// </summary>
        public static void EnforceConsistency(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls = null,
            IDictionary<string, UnmanagedModuleVersion> dlc = null
        )
        {
            ICollection<string> errors = ConsistencyErrors(modules, dlls, dlc);

            if (errors.Count != 0)
            {
                throw new InconsistentKraken(errors);
            }
        }
    
        /// <summary>
        /// Returns true if the mods supplied can co-exist. This checks depends/pre-depends/conflicts only.
        /// </summary>
        public static bool IsConsistent(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls = null,
            IDictionary<string, UnmanagedModuleVersion> dlc = null
        )
        {
            return ConsistencyErrors(modules, dlls, dlc).Count == 0;
        }

        private static List<ProvidesInfo> ModulesToProvided(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls = null,
            IDictionary<string, UnmanagedModuleVersion> dlc = null
        )
        {
            var provided = new List<ProvidesInfo>();

            if (dlls == null)
                dlls = new List<string>();

            if (dlc == null)
                dlc = new Dictionary<string, UnmanagedModuleVersion>();

            foreach (var m in modules)
            {
                foreach (var p in m.ProvidesList)
                {
                    log.DebugFormat("{0} provides {1}", m, p);
                    provided.Add(new ProvidesInfo(m.identifier, m.version, p, null));
                }
            }

            // Add in our DLLs as things we know exist.
            foreach (var d in dlls)
            {
                provided.Add(new ProvidesInfo(d, null, d, null));
            }

            // Add in our DLC as things we know exist.
            foreach (var d in dlc)
            {
                provided.Add(new ProvidesInfo(d.Key, d.Value, d.Key, d.Value));
            }

            return provided;
        }

        /// <summary>
        /// Given a list of modules and optional dlls, returns a dictionary of dependencies which are still not met.
        /// The dictionary keys are the un-met depdendencies, the values are the modules requesting them.
        /// </summary>
        public static Dictionary<string,List<CkanModule>> FindUnmetDependencies(
            IEnumerable<CkanModule> modules,
            IEnumerable<string> dlls = null,
            IDictionary<string, UnmanagedModuleVersion> dlc = null
        )
        {
            return FindUnmetDependencies(modules, ModulesToProvided(modules, dlls, dlc));
        }

        /// <summary>
        /// Given a list of modules, and a set of providers, returns a dictionary of dependencies which have not been met.
        /// </summary>
        private static Dictionary<string,List<CkanModule>> FindUnmetDependencies(
            IEnumerable<CkanModule> modules,
            List<ProvidesInfo> provided
        )
        {
            var providedByProvideeIdentifier = provided.ToLookup(i => i.ProvideeIdentifier);

            var unmet = new Dictionary<string,List<CkanModule>>();

            foreach (var mod in modules)
            {
                // If this module has no dependencies then we're done.
                if (mod.depends is null)
                    continue;

                // If it does then iterate through the module's dependencies.
                foreach (var d in mod.depends)
                {
                    // If the dependency is provided and either has no version or its version is in bounds then it's
                    // okay.
                    var dependencyMet = providedByProvideeIdentifier.Contains(d.name) &&
                                        providedByProvideeIdentifier[d.name]
                                            .Any(i => i.ProvideeVersion is null || d.WithinBounds(i.ProvideeVersion));

                    if (dependencyMet)
                            continue;

                    // Ensure the list exists.
                    if (!unmet.ContainsKey(d.name))
                        unmet[d.name] = new List<CkanModule>();

                    // Add the dependency to the list of unmet dependencies.
                    unmet[d.name].Add(mod);
                }
            }

            return unmet;
        }

        private sealed class ProvidesInfo
        {
            public string ProviderIdentifier { get; }
            public ModuleVersion ProviderVersion { get; }
            public string ProvideeIdentifier { get; }
            public ModuleVersion ProvideeVersion { get; }

            public ProvidesInfo(
                string providerIdentifier,
                ModuleVersion providerVersion,
                string provideeIdentifier,
                ModuleVersion provideeVersion
            )
            {
                ProviderIdentifier = providerIdentifier;
                ProviderVersion = providerVersion;
                ProvideeIdentifier = provideeIdentifier;
                ProvideeVersion = provideeVersion;
            }
        }
    }
}
