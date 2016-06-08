using System.Collections.Generic;
using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Methods to query a registry.
    /// </summary>
    public interface IRegistryQuerier
    {
        IEnumerable<InstalledModule> InstalledModules { get;}
        IEnumerable<string> InstalledDlls { get; }

        /// <summary>
        /// Returns a simple array of all latest available modules for
        /// the specified version of KSP.
        /// </summary>
        // TODO: This name is misleading. It's more a LatestAvailable's'
        List<CkanModule> Available(KspVersion ksp_version);

        /// <summary>
        ///     Returns the latest available version of a module that
        ///     satisifes the specified version.
        ///     Returns null if there's simply no compatible version for this system.
        ///     If no ksp_version is provided, the latest module for *any* KSP is returned.
        /// <exception cref="ModuleNotFoundKraken">Throws if asked for a non-existent module.</exception>
        /// </summary>
        CkanModule LatestAvailable(string identifier, KspVersion ksp_version, RelationshipDescriptor relationship_descriptor = null);

        /// <summary>
        ///     Returns the latest available version of a module that satisifes the specified version and
        ///     optionally a RelationshipDescriptor. Takes into account module 'provides', which may
        ///     result in a list of alternatives being provided.
        ///     Returns an empty list if nothing is available for our system, which includes if no such module exists.
        ///     If no KSP version is provided, the latest module for *any* KSP version is given.
        /// </summary>
        List<CkanModule> LatestAvailableWithProvides(string identifier, KspVersion ksp_version, RelationshipDescriptor relationship_descriptor = null);

        /// <summary>
        ///     Checks the sanity of the registry, to ensure that all dependencies are met,
        ///     and no mods conflict with each other.
        /// <exception cref="InconsistentKraken">Thrown if a inconsistency is found</exception>
        /// </summary>
        void CheckSanity();

        /// <summary>
        /// Finds and returns all modules that could not exist without the listed modules installed, including themselves.
        /// </summary>
        HashSet<string> FindReverseDependencies(IEnumerable<string> modules);


        /// <summary>
        /// Gets the installed version of a mod. Does not check for provided or autodetected mods.
        /// </summary>
        /// <returns>The module or null if not found</returns>
        CkanModule GetInstalledVersion(string identifer);

        /// <summary>
        /// Attempts to find a module with the given identifier and version.
        /// </summary>
        /// <returns>The module if it exists, null otherwise.</returns>
        CkanModule GetModuleByVersion(string identifier, Version version);

        /// <summary>
        ///     Returns a simple array of all incompatible modules for
        ///     the specified version of KSP.
        /// </summary>
        List<CkanModule> Incompatible(KspVersion ksp_version);

        /// <summary>
        /// Returns a dictionary of all modules installed, along with their
        /// versions.
        /// This includes DLLs, which will have a version type of `DllVersion`.
        /// This includes Provides if set, which will have a version of `ProvidesVersion`.
        /// </summary>
        Dictionary<string, Version> Installed(bool include_provides = true);

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
        Version InstalledVersion(string identifier, bool with_provides = true);
    }

    /// <summary>
    /// Helpers for <see cref="IRegistryQuerier"/>
    /// </summary>
    public static class IRegistryQuerierHelpers
{
        /// <summary>
        /// Helper to call <see cref="IRegistryQuerier.GetModuleByVersion(string, Version)"/>
        /// </summary>
        public static CkanModule GetModuleByVersion(this IRegistryQuerier querier, string ident, string version)
        {
            return querier.GetModuleByVersion(ident, new Version(version));
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
            return querier.IsInstalled(identifier) && querier.InstalledVersion(identifier) is DllVersion;
        }

        /// <summary>
        /// Is the mod installed and does it have a newer version compatible with version
        /// We can't update AD mods
        /// </summary>
        public static bool HasUpdate(this IRegistryQuerier querier, string identifier, KspVersion version)
        {
            CkanModule newest_version;
            try
            {
                newest_version = querier.LatestAvailable(identifier, version);
            }
            catch (ModuleNotFoundKraken)
            {
                return false;
            }
            if (newest_version == null) return false;
            return !new List<string>(querier.InstalledDlls).Contains(identifier) && querier.IsInstalled(identifier, false) 
                && newest_version.version.IsGreaterThan(querier.InstalledVersion(identifier));
        }
    }
}