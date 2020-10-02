using System;
using System.Linq;
using System.Collections.Generic;

using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Methods to query a registry.
    /// </summary>
    public interface IRegistryQuerier
    {
        IEnumerable<InstalledModule> InstalledModules { get; }
        IEnumerable<string>          InstalledDlls    { get; }
        IDictionary<string, ModuleVersion> InstalledDlc { get; }

        int? DownloadCount(string identifier);

        /// <summary>
        /// Returns a simple array of the latest compatible module for each identifier for
        /// the specified version of KSP.
        /// </summary>
        IEnumerable<CkanModule> CompatibleModules(KspVersionCriteria ksp_version);

        /// <summary>
        /// Get full JSON metadata string for a mod's available versions
        /// </summary>
        /// <param name="identifier">Name of the mod to look up</param>
        /// <returns>
        /// JSON formatted string for all the available versions of the mod
        /// </returns>
        string GetAvailableMetadata(string identifier);

        /// <summary>
        /// Returns the latest available version of a module that satisfies the specified version.
        /// Returns null if there's simply no available version for this system.
        /// If no ksp_version is provided, the latest module for *any* KSP version is returned.
        /// <exception cref="ModuleNotFoundKraken">Throws if asked for a non-existent module.</exception>
        /// </summary>
        CkanModule LatestAvailable(string identifier, KspVersionCriteria ksp_version, RelationshipDescriptor relationship_descriptor = null);

        /// <summary>
        /// Returns the max game version that is compatible with the given mod.
        /// </summary>
        /// <param name="identifier">Name of mod to check</param>
        KspVersion LatestCompatibleKSP(string identifier);

        /// <summary>
        /// Returns all available versions of a module.
        /// <exception cref="ModuleNotFoundKraken">Throws if asked for a non-existent module.</exception>
        /// </summary>
        IEnumerable<CkanModule> AvailableByIdentifier(string identifier);

        /// <summary>
        /// Returns the latest available version of a module that satisfies the specified version and
        /// optionally a RelationshipDescriptor. Takes into account module 'provides', which may
        /// result in a list of alternatives being provided.
        /// Returns an empty list if nothing is available for our system, which includes if no such module exists.
        /// If no KSP version is provided, the latest module for *any* KSP version is given.
        /// </summary>
        List<CkanModule> LatestAvailableWithProvides(
            string                  identifier,
            KspVersionCriteria      ksp_version,
            RelationshipDescriptor  relationship_descriptor = null,
            IEnumerable<CkanModule> toInstall               = null
        );

        /// <summary>
        /// Checks the sanity of the registry, to ensure that all dependencies are met,
        /// and no mods conflict with each other.
        /// <exception cref="InconsistentKraken">Thrown if a inconsistency is found</exception>
        /// </summary>
        void CheckSanity();

        /// <summary>
        /// Finds and returns all modules that could not exist without the listed modules installed, including themselves.
        /// </summary>
        IEnumerable<string> FindReverseDependencies(
            IEnumerable<string> modulesToRemove, IEnumerable<CkanModule> modulesToInstall = null
        );

        /// <summary>
        /// Find auto-installed modules that have no depending modules
        /// or only auto-installed depending modules.
        /// installedModules is a parameter so we can experiment with
        /// changes that have not yet been made, such as removing other modules.
        /// </summary>
        /// <param name="installedModules">The modules currently installed</param>
        /// <returns>
        /// Sequence of removable auto-installed modules, if any
        /// </returns>
        IEnumerable<InstalledModule> FindRemovableAutoInstalled(IEnumerable<InstalledModule> installedModules);

        /// <summary>
        /// Gets the installed version of a mod. Does not check for provided or autodetected mods.
        /// </summary>
        /// <returns>The module or null if not found</returns>
        CkanModule GetInstalledVersion(string identifier);

        /// <summary>
        /// Attempts to find a module with the given identifier and version.
        /// </summary>
        /// <returns>The module if it exists, null otherwise.</returns>
        CkanModule GetModuleByVersion(string identifier, ModuleVersion version);

        /// <summary>
        /// Returns a simple array of all incompatible modules for
        /// the specified version of KSP.
        /// </summary>
        IEnumerable<CkanModule> IncompatibleModules(KspVersionCriteria ksp_version);

        /// <summary>
        /// Returns a dictionary of all modules installed, along with their
        /// versions.
        /// This includes DLLs, which will have a version type of `DllVersion`.
        /// This includes Provides if set, which will have a version of `ProvidesVersion`.
        /// </summary>
        Dictionary<string, ModuleVersion> Installed(bool withProvides = true, bool withDLLs = true);

        /// <summary>
        /// Returns the InstalledModule, or null if it is not installed.
        /// Does *not* look up virtual modules.
        /// </summary>
        InstalledModule InstalledModule(string identifier);

        /// <summary>
        /// Returns the installed version of a given mod.
        ///     If the mod was autodetected (but present), a version of type `DllVersion` is returned.
        ///     If the mod is provided by another mod (ie, virtual) a type of ProvidesVersion is returned.
        /// </summary>
        /// <param name="with_provides">If set to false will not check for provided versions.</param>
        /// <returns>The version of the mod or null if not found</returns>
        ModuleVersion InstalledVersion(string identifier, bool with_provides = true);
    }

    /// <summary>
    /// Helpers for <see cref="IRegistryQuerier"/>
    /// </summary>
    public static class IRegistryQuerierHelpers
    {
        /// <summary>
        /// Helper to call <see cref="IRegistryQuerier.GetModuleByVersion(string, ModuleVersion)"/>
        /// </summary>
        public static CkanModule GetModuleByVersion(this IRegistryQuerier querier, string ident, string version)
        {
            return querier.GetModuleByVersion(ident, new ModuleVersion(version));
        }

        /// <summary>
        ///     Check if a mod is installed (either via CKAN, DLL, or virtually)
        ///     If withProvides is set to false then we skip the check for if the
        ///     mod has been provided (rather than existing as a real mod).
        /// </summary>
        /// <returns><c>true</c>, if installed<c>false</c> otherwise.</returns>
        public static bool IsInstalled(this IRegistryQuerier querier, string identifier, bool with_provides = true)
        {
            return querier.InstalledVersion(identifier, with_provides) != null;
        }

        /// <summary>
        ///     Check if a mod is autodetected.
        /// </summary>
        /// <returns><c>true</c>, if autodetected<c>false</c> otherwise.</returns>
        public static bool IsAutodetected(this IRegistryQuerier querier, string identifier)
        {
            return querier.IsInstalled(identifier) && querier.InstalledVersion(identifier) is UnmanagedModuleVersion;
        }

        /// <summary>
        /// Is the mod installed and does it have a newer version compatible with version
        /// We can't update AD mods
        /// </summary>
        public static bool HasUpdate(this IRegistryQuerier querier, string identifier, KspVersionCriteria version)
        {
            CkanModule newest_version;
            try
            {
                newest_version = querier.LatestAvailable(identifier, version);
            }
            catch (Exception)
            {
                return false;
            }
            if (newest_version == null
                || !querier.IsInstalled(identifier, false)
                || querier.InstalledDlls.Contains(identifier)
                || !newest_version.version.IsGreaterThan(querier.InstalledVersion(identifier)))
            {
                return false;
            }
            // All quick checks pass. Now check the relationships.
            try
            {
                RelationshipResolver resolver = new RelationshipResolver(
                    new CkanModule[] { newest_version },
                    null,
                    new RelationshipResolverOptions()
                    {
                        with_recommends = false,
                        without_toomanyprovides_kraken = true,
                    },
                    querier,
                    version
                );
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Generate a string describing the range of game versions
        /// compatible with the given module.
        /// </summary>
        /// <param name="identifier">Mod name to findDependencyShallow</param>
        /// <returns>
        /// String describing range of compatible game versions.
        /// </returns>
        public static string CompatibleGameVersions(this IRegistryQuerier querier, string identifier)
        {
            List<CkanModule> releases = querier.AvailableByIdentifier(identifier).ToList();
            if (releases != null && releases.Count > 0) {
                ModuleVersion minMod = null, maxMod = null;
                KspVersion    minKsp = null, maxKsp = null;
                Registry.GetMinMaxVersions(releases, out minMod, out maxMod, out minKsp, out maxKsp);
                return KspVersionRange.VersionSpan(minKsp, maxKsp);
            }
            return "";
        }

        /// <summary>
        /// Generate a string describing the range of game versions
        /// compatible with the given module.
        /// </summary>
        /// <param name="identifier">Mod name to findDependencyShallow</param>
        /// <returns>
        /// String describing range of compatible game versions.
        /// </returns>
        public static string CompatibleGameVersions(this IRegistryQuerier querier, CkanModule module)
        {
            ModuleVersion minMod = null, maxMod = null;
            KspVersion    minKsp = null, maxKsp = null;
            Registry.GetMinMaxVersions(
                new CkanModule[] { module },
                out minMod, out maxMod,
                out minKsp, out maxKsp
            );
            return KspVersionRange.VersionSpan(minKsp, maxKsp);
        }

        /// <summary>
        /// Is the mod installed and does it have a replaced_by relationship with a compatible version
        /// Check latest information on installed version of mod "identifier" and if it has a "replaced_by"
        /// value, check if there is a compatible version of the linked mod
        /// Given a mod identifier, return a ModuleReplacement containing the relevant replacement
        /// if compatibility matches.
        /// </summary>
        public static ModuleReplacement GetReplacement(this IRegistryQuerier querier, string identifier, KspVersionCriteria version)
        {
            // We only care about the installed version
            CkanModule installedVersion;
            try
            {
                installedVersion = querier.GetInstalledVersion(identifier);
            }
            catch (ModuleNotFoundKraken)
            {
                return null;
            }
            return querier.GetReplacement(installedVersion, version);
        }

        public static ModuleReplacement GetReplacement(this IRegistryQuerier querier, CkanModule installedVersion, KspVersionCriteria version)
        {
            // Mod is not installed, so we don't care about replacements
            if (installedVersion == null)
                return null;
            // No replaced_by relationship
            if (installedVersion.replaced_by == null)
                return null;

            // Get the identifier from the replaced_by relationship, if it exists
            ModuleRelationshipDescriptor replacedBy = installedVersion.replaced_by;

            // Now we need to see if there is a compatible version of the replacement
            try
            {
                ModuleReplacement replacement = new ModuleReplacement();
                replacement.ToReplace = installedVersion;
                if (installedVersion.replaced_by.version != null)
                {
                    replacement.ReplaceWith = querier.GetModuleByVersion(installedVersion.replaced_by.name, installedVersion.replaced_by.version);
                    if (replacement.ReplaceWith != null)
                    {
                        if (replacement.ReplaceWith.IsCompatibleKSP(version))
                        {
                            return replacement;
                        }
                    }
                }
                else
                {
                    replacement.ReplaceWith = querier.LatestAvailable(installedVersion.replaced_by.name, version);
                    if (replacement.ReplaceWith != null)
                    {
                        if (installedVersion.replaced_by.min_version != null)
                        {
                            if (!replacement.ReplaceWith.version.IsLessThan(replacedBy.min_version))
                            {
                                return replacement;
                            }
                        }
                        else return replacement;
                    }
                }
                return null;
            }
            catch (ModuleNotFoundKraken)
            {
                return null;
            }
        }
    }
}
