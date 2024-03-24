using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using log4net;

using CKAN.Versioning;
using CKAN.Games;
#if NETSTANDARD2_0
using CKAN.Extensions;
#endif

namespace CKAN
{
    /// <summary>
    /// Methods to query a registry.
    /// </summary>
    public interface IRegistryQuerier
    {
        ReadOnlyDictionary<string, Repository> Repositories     { get; }
        IEnumerable<InstalledModule>           InstalledModules { get; }
        IEnumerable<string>                    InstalledDlls    { get; }
        IDictionary<string, ModuleVersion>     InstalledDlc     { get; }

        /// <summary>
        /// Returns a simple array of the latest compatible module for each identifier for
        /// the specified version of KSP.
        /// </summary>
        IEnumerable<CkanModule> CompatibleModules(GameVersionCriteria ksp_version);

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
        CkanModule LatestAvailable(string                  identifier,
                                   GameVersionCriteria     ksp_version,
                                   RelationshipDescriptor  relationship_descriptor = null,
                                   ICollection<CkanModule> installed = null,
                                   ICollection<CkanModule> toInstall = null);

        /// <summary>
        /// Returns the max game version that is compatible with the given mod.
        /// </summary>
        /// <param name="identifier">Name of mod to check</param>
        GameVersion LatestCompatibleGameVersion(List<GameVersion> realVersions, string identifier);

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
        List<CkanModule> LatestAvailableWithProvides(string                  identifier,
                                                     GameVersionCriteria     ksp_version,
                                                     RelationshipDescriptor  relationship_descriptor = null,
                                                     ICollection<CkanModule> installed = null,
                                                     ICollection<CkanModule> toInstall = null);

        /// <summary>
        /// Checks the sanity of the registry, to ensure that all dependencies are met,
        /// and no mods conflict with each other.
        /// <exception cref="InconsistentKraken">Thrown if a inconsistency is found</exception>
        /// </summary>
        void CheckSanity();

        /// <summary>
        /// Finds and returns all modules that could not exist without the listed modules installed, including themselves.
        /// </summary>
        IEnumerable<string> FindReverseDependencies(List<string> modulesToRemove,
                                                    List<CkanModule> modulesToInstall = null,
                                                    Func<RelationshipDescriptor, bool> satisfiedFilter = null);

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
        IEnumerable<CkanModule> IncompatibleModules(GameVersionCriteria ksp_version);

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

        /// <summary>
        /// Check whether any versions of this mod are installable (including dependencies) on the given game versions
        /// </summary>
        /// <param name="identifier">Identifier of mod</param>
        /// <param name="crit">Game versions</param>
        /// <returns>true if any version is recursively compatible, false otherwise</returns>
        bool IdentifierCompatible(string identifier, GameVersionCriteria crit);
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
            => querier.GetModuleByVersion(ident, new ModuleVersion(version));

        /// <summary>
        ///     Check if a mod is installed (either via CKAN, DLL, or virtually)
        ///     If withProvides is set to false then we skip the check for if the
        ///     mod has been provided (rather than existing as a real mod).
        /// </summary>
        /// <returns><c>true</c>, if installed<c>false</c> otherwise.</returns>
        public static bool IsInstalled(this IRegistryQuerier querier, string identifier, bool with_provides = true)
            => querier.InstalledVersion(identifier, with_provides) != null;

        /// <summary>
        ///     Check if a mod is autodetected.
        /// </summary>
        /// <returns><c>true</c>, if autodetected<c>false</c> otherwise.</returns>
        public static bool IsAutodetected(this IRegistryQuerier querier, string identifier)
            => querier.IsInstalled(identifier)
                && querier.InstalledVersion(identifier) is UnmanagedModuleVersion;

        /// <summary>
        /// Is the mod installed and does it have a newer version compatible with versionCrit
        /// </summary>
        public static bool HasUpdate(this IRegistryQuerier   querier,
                                     string                  identifier,
                                     GameInstance            instance,
                                     out CkanModule          latestMod,
                                     ICollection<CkanModule> installed = null)
        {
            // Check if it's installed (including manually!)
            var instVer = querier.InstalledVersion(identifier);
            if (instVer == null)
            {
                latestMod = null;
                return false;
            }
            // Check if it's available
            try
            {
                latestMod = querier.LatestAvailable(identifier, instance.VersionCriteria(), null, installed);
            }
            catch
            {
                latestMod = null;
            }
            if (latestMod == null)
            {
                return false;
            }
            // Check if the installed module is up to date
            var comp = latestMod.version.CompareTo(instVer);
            if (comp == -1
                || (comp == 0 && !querier.MetadataChanged(identifier)
                              // Check if any of the files or directories are missing
                              && (instance == null
                                  || (querier.InstalledModule(identifier)
                                             ?.Files
                                              .Select(instance.ToAbsoluteGameDir)
                                              .All(p => Directory.Exists(p) || File.Exists(p))
                                             // Manually installed, consider up to date
                                             ?? true))))
            {
                latestMod = null;
                return false;
            }
            // Checking with a RelationshipResolver here would commit us to
            // testing against the currently installed modules in the registry,
            // which could block us from upgrading away from a problem.
            // Trust our LatestAvailable call above.
            return true;
        }

        public static Dictionary<bool, List<CkanModule>> CheckUpgradeable(this IRegistryQuerier querier,
                                                                          GameInstance          instance,
                                                                          HashSet<string>       heldIdents)
        {
            // Get the absolute latest versions ignoring restrictions,
            // to break out of mutual version-depending deadlocks
            var unlimited = querier.Installed(false)
                                   .Keys
                                   .Select(ident => !heldIdents.Contains(ident)
                                                    && querier.HasUpdate(ident, instance,
                                                                         out CkanModule latest)
                                                    && !latest.IsDLC
                                                        ? latest
                                                        : querier.GetInstalledVersion(ident))
                                   .Where(m => m != null)
                                   .ToList();
            return querier.CheckUpgradeable(instance, heldIdents, unlimited);
        }

        public static Dictionary<bool, List<CkanModule>> CheckUpgradeable(this IRegistryQuerier querier,
                                                                          GameInstance          instance,
                                                                          HashSet<string>       heldIdents,
                                                                          List<CkanModule>      initial)
        {
            // Use those as the installed modules
            var upgradeable    = new List<CkanModule>();
            var notUpgradeable = new List<CkanModule>();
            foreach (var ident in initial.Select(module => module.identifier))
            {
                if (!heldIdents.Contains(ident)
                    && querier.HasUpdate(ident, instance,
                                         out CkanModule latest, initial)
                    && !latest.IsDLC)
                {
                    upgradeable.Add(latest);
                }
                else
                {
                    var current = querier.InstalledModule(ident);
                    if (current != null && !current.Module.IsDLC)
                    {
                        notUpgradeable.Add(current.Module);
                    }
                }
            }
            return new Dictionary<bool, List<CkanModule>>
            {
                { true,  upgradeable    },
                { false, notUpgradeable },
            };
        }

        private static bool MetadataChanged(this IRegistryQuerier querier, string identifier)
        {
            try
            {
                var installed = querier.InstalledModule(identifier)?.Module;
                return installed != null
                    && (!querier.GetModuleByVersion(identifier, installed.version)?.MetadataEquals(installed)
                        ?? false);
            }
            catch
            {
                // Treat exceptions as not-changed
                return false;
            }
        }

        /// <summary>
        /// Generate a string describing the range of game versions
        /// compatible with the given module.
        /// </summary>
        /// <param name="identifier">Mod name to findDependencyShallow</param>
        /// <returns>
        /// String describing range of compatible game versions.
        /// </returns>
        public static string CompatibleGameVersions(this IRegistryQuerier querier,
                                                    IGame                 game,
                                                    string                identifier)
        {
            List<CkanModule> releases = null;
            try
            {
                releases = querier.AvailableByIdentifier(identifier)
                                  .ToList();
            }
            catch
            {
                var instMod = querier.InstalledModule(identifier);
                if (instMod != null)
                {
                    releases = Enumerable.Repeat(instMod.Module, 1)
                                         .ToList();
                }
            }
            if (releases != null && releases.Count > 0)
            {
                CkanModule.GetMinMaxVersions(releases, out _, out _,
                                             out GameVersion minKsp, out GameVersion maxKsp);
                return GameVersionRange.VersionSpan(game, minKsp, maxKsp);
            }
            return "";
        }

        /// <summary>
        /// Generate a string describing the range of game versions
        /// compatible with the given module.
        /// </summary>
        /// <param name="game">Game to represent</param>
        /// <param name="module">Mod to findDependencyShallow</param>
        /// <returns>
        /// String describing range of compatible game versions.
        /// </returns>
        public static string CompatibleGameVersions(this CkanModule module, IGame game)
        {
            CkanModule.GetMinMaxVersions(
                new CkanModule[] { module },
                out _, out _,
                out GameVersion minKsp, out GameVersion maxKsp
            );
            return GameVersionRange.VersionSpan(game, minKsp, maxKsp);
        }

        /// <summary>
        /// Is the mod installed and does it have a replaced_by relationship with a compatible version
        /// Check latest information on installed version of mod "identifier" and if it has a "replaced_by"
        /// value, check if there is a compatible version of the linked mod
        /// Given a mod identifier, return a ModuleReplacement containing the relevant replacement
        /// if compatibility matches.
        /// </summary>
        public static ModuleReplacement GetReplacement(this IRegistryQuerier querier, string identifier, GameVersionCriteria version)
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

        public static ModuleReplacement GetReplacement(this IRegistryQuerier querier, CkanModule installedVersion, GameVersionCriteria version)
        {
            // Mod is not installed, so we don't care about replacements
            if (installedVersion == null)
            {
                return null;
            }
            // No replaced_by relationship
            if (installedVersion.replaced_by == null)
            {
                return null;
            }

            // Get the identifier from the replaced_by relationship, if it exists
            ModuleRelationshipDescriptor replacedBy = installedVersion.replaced_by;

            // Now we need to see if there is a compatible version of the replacement
            try
            {
                ModuleReplacement replacement = new ModuleReplacement
                {
                    ToReplace = installedVersion
                };
                if (installedVersion.replaced_by.version != null)
                {
                    replacement.ReplaceWith = querier.GetModuleByVersion(installedVersion.replaced_by.name, installedVersion.replaced_by.version);
                    if (replacement.ReplaceWith != null)
                    {
                        if (replacement.ReplaceWith.IsCompatible(version))
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
                        else
                        {
                            return replacement;
                        }
                    }
                }
                return null;
            }
            catch (ModuleNotFoundKraken)
            {
                return null;
            }
        }

        /// <summary>
        /// Find auto-installed modules that have no depending modules
        /// or only auto-installed depending modules.
        /// </summary>
        /// <param name="installedModules">The modules currently installed</param>
        /// <param name="dlls">The DLLs that are manually installed</param>
        /// <param name="dlc">The DLCs that are installed</param>
        /// <param name="crit">Version criteria for resolving relationships</param>
        /// <returns>
        /// Sequence of removable auto-installed modules, if any
        /// </returns>
        private static IEnumerable<InstalledModule> FindRemovableAutoInstalled(
            this IRegistryQuerier              querier,
            List<InstalledModule>              installedModules,
            HashSet<string>                    dlls,
            IDictionary<string, ModuleVersion> dlc,
            GameVersionCriteria                crit)
        {
            log.DebugFormat("Finding removable autoInstalled for: {0}",
                            string.Join(", ", installedModules.Select(im => im.identifier)));

            var autoInstMods = installedModules.Where(im => im.AutoInstalled).ToList();
            var autoInstIds  = autoInstMods.Select(im => im.Module.identifier).ToHashSet();

            // Need to get the full changeset for this to work as intended
            RelationshipResolverOptions opts = RelationshipResolverOptions.DependsOnlyOpts();
            opts.without_toomanyprovides_kraken = true;
            opts.without_enforce_consistency    = true;
            opts.proceed_with_inconsistencies   = true;
            var resolver = new RelationshipResolver(
                // DLC silently crashes the resolver
                installedModules.Where(im => !im.Module.IsDLC)
                                .Select(im => im.Module),
                null,
                opts, querier, crit);

            var mods = resolver.ModList().ToHashSet();
            return autoInstMods.Where(
                im => autoInstIds.IsSupersetOf(
                    Registry.FindReverseDependencies(new List<string> { im.identifier },
                                                     new List<CkanModule>(),
                                                     mods, dlls, dlc)));
        }

        /// <summary>
        /// Find auto-installed modules that have no depending modules
        /// or only auto-installed depending modules.
        /// installedModules is a parameter so we can experiment with
        /// changes that have not yet been made, such as removing other modules.
        /// </summary>
        /// <param name="installedModules">The modules currently installed</param>
        /// <param name="crit">Version criteria for resolving relationships</param>
        /// <returns>
        /// Sequence of removable auto-installed modules, if any
        /// </returns>
        public static IEnumerable<InstalledModule> FindRemovableAutoInstalled(
            this IRegistryQuerier querier,
            List<InstalledModule> installedModules,
            GameVersionCriteria   crit)
            => querier?.FindRemovableAutoInstalled(installedModules,
                                                   querier.InstalledDlls.ToHashSet(),
                                                   querier.InstalledDlc,
                                                   crit)
                      ?? Enumerable.Empty<InstalledModule>();

        private static readonly ILog log = LogManager.GetLogger(typeof(IRegistryQuerierHelpers));
    }
}
