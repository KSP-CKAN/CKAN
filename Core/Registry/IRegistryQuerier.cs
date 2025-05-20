using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autofac;
using log4net;

using CKAN.Configuration;
using CKAN.Versioning;
using CKAN.Games;

namespace CKAN
{
    /// <summary>
    /// Methods to query a registry.
    /// </summary>
    public interface IRegistryQuerier
    {
        ReadOnlyDictionary<string, Repository>      Repositories     { get; }
        IReadOnlyCollection<InstalledModule>        InstalledModules { get; }
        IReadOnlyCollection<string>                 InstalledDlls    { get; }
        IDictionary<string, UnmanagedModuleVersion> InstalledDlc     { get; }

        /// <summary>
        /// Returns a simple array of the latest compatible module for each identifier for
        /// the specified game version.
        /// </summary>
        IEnumerable<CkanModule> CompatibleModules(StabilityToleranceConfig stabilityTolerance,
                                                  GameVersionCriteria?     ksp_version);

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
        CkanModule? LatestAvailable(string                           identifier,
                                    StabilityToleranceConfig         stabilityTolerance,
                                    GameVersionCriteria?             ksp_version,
                                    RelationshipDescriptor?          relationship_descriptor = null,
                                    IReadOnlyCollection<CkanModule>? installed               = null,
                                    IReadOnlyCollection<CkanModule>? toInstall               = null);

        /// <summary>
        /// Returns the max game version that is compatible with the given mod.
        /// </summary>
        /// <param name="realVersions">List of game versions to check against</param>
        /// <param name="identifier">Name of mod to check</param>
        GameVersion? LatestCompatibleGameVersion(List<GameVersion> realVersions, string identifier);

        /// <summary>
        /// Returns all available versions of a module.
        /// <exception cref="ModuleNotFoundKraken">Throws if asked for a non-existent module.</exception>
        /// </summary>
        IEnumerable<CkanModule> AvailableByIdentifier(string identifier);

        IEnumerable<AvailableModule> AllAvailableByProvides(string identifier);

        /// <summary>
        /// Returns the latest available version of a module that satisfies the specified version and
        /// optionally a RelationshipDescriptor. Takes into account module 'provides', which may
        /// result in a list of alternatives being provided.
        /// Returns an empty list if nothing is available for our system, which includes if no such module exists.
        /// If no KSP version is provided, the latest module for *any* KSP version is given.
        /// </summary>
        List<CkanModule> LatestAvailableWithProvides(string                           identifier,
                                                     StabilityToleranceConfig         stabilityTolerance,
                                                     GameVersionCriteria?             ksp_version,
                                                     RelationshipDescriptor?          relationship_descriptor = null,
                                                     IReadOnlyCollection<CkanModule>? installed               = null,
                                                     IReadOnlyCollection<CkanModule>? toInstall               = null);

        /// <summary>
        /// Checks the sanity of the registry, to ensure that all dependencies are met,
        /// and no mods conflict with each other.
        /// <exception cref="InconsistentKraken">Thrown if a inconsistency is found</exception>
        /// </summary>
        void CheckSanity();

        /// <summary>
        /// Finds and returns all modules that could not exist without the listed modules installed, including themselves.
        /// </summary>
        IEnumerable<string> FindReverseDependencies(IReadOnlyCollection<string>         modulesToRemove,
                                                    IReadOnlyCollection<CkanModule>?    modulesToInstall = null,
                                                    Func<RelationshipDescriptor, bool>? satisfiedFilter  = null);

        /// <summary>
        /// Gets the installed version of a mod. Does not check for provided or autodetected mods.
        /// </summary>
        /// <returns>The module or null if not found</returns>
        CkanModule? GetInstalledVersion(string identifier);

        /// <summary>
        /// Attempts to find a module with the given identifier and version.
        /// </summary>
        /// <returns>The module if it exists, null otherwise.</returns>
        CkanModule? GetModuleByVersion(string identifier, ModuleVersion version);

        /// <summary>
        /// Returns a simple array of all incompatible modules for
        /// the specified version of KSP.
        /// </summary>
        IEnumerable<CkanModule> IncompatibleModules(StabilityToleranceConfig stabilityTolerance,
                                                    GameVersionCriteria      ksp_version);

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
        InstalledModule? InstalledModule(string identifier);

        /// <summary>
        /// Returns the installed version of a given mod.
        ///     If the mod was autodetected (but present), a version of type `DllVersion` is returned.
        ///     If the mod is provided by another mod (ie, virtual) a type of ProvidesVersion is returned.
        /// </summary>
        /// <param name="identifier">Identifier of mod</param>
        /// <param name="with_provides">If set to false will not check for provided versions.</param>
        /// <returns>The version of the mod or null if not found</returns>
        ModuleVersion? InstalledVersion(string identifier, bool with_provides = true);

        /// <summary>
        /// Check whether any versions of this mod are installable (including dependencies) on the given game versions
        /// </summary>
        /// <param name="identifier">Identifier of mod</param>
        /// <param name="stabilityTolerance">Stability tolerance for the game instance</param>
        /// <param name="crit">Game versions</param>
        /// <returns>true if any version is recursively compatible, false otherwise</returns>
        bool IdentifierCompatible(string identifier, StabilityToleranceConfig stabilityTolerance, GameVersionCriteria crit);
    }

    /// <summary>
    /// Helpers for <see cref="IRegistryQuerier"/>
    /// </summary>
    public static class IRegistryQuerierHelpers
    {
        /// <summary>
        /// Helper to call <see cref="IRegistryQuerier.GetModuleByVersion(string, ModuleVersion)"/>
        /// </summary>
        public static CkanModule? GetModuleByVersion(this IRegistryQuerier querier, string ident, string version)
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
        /// Is the mod installed and does it have a newer version compatible with the game instance's version criteria
        /// </summary>
        /// <param name="querier">A registry</param>
        /// <param name="identifier">Identifier of mod to check</param>
        /// <param name="stabilityTolerance">Stability tolerance for the game instance</param>
        /// <param name="instance">The registry's game instance</param>
        /// <param name="filters">Install filters that will cause missing files to be skipped</param>
        /// <param name="checkMissingFiles">If true, check if any of the files or directories are missing</param>
        /// <param name="latestMod">The latest mod if an update is available</param>
        /// <param name="installed">The installed modules to check against</param>
        public static bool HasUpdate(this IRegistryQuerier            querier,
                                     string                           identifier,
                                     StabilityToleranceConfig         stabilityTolerance,
                                     GameInstance?                    instance,
                                     HashSet<string>                  filters,
                                     bool                             checkMissingFiles,
                                     out CkanModule?                  latestMod,
                                     IReadOnlyCollection<CkanModule>? installed = null)
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
                latestMod = querier.LatestAvailable(identifier, stabilityTolerance,
                                                    instance?.VersionCriteria(), null, installed);
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
                              && (!checkMissingFiles
                                  || instance == null
                                  || (querier.InstalledModule(identifier)
                                             ?.AllFilesExist(instance, filters)
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

        /// <summary>
        /// Partition the installed mods based on whether they are upgradeable
        /// </summary>
        /// <param name="querier">A registry</param>
        /// <param name="instance">The registry's game instance</param>
        /// <param name="heldIdents">Identifiers of mods that the user has designated as not to be upgraded</param>
        /// <param name="ignoreMissingIdents">Identifiers of mods that the user has designated as allowed to have missing files</param>
        /// <returns>
        /// Dictionary with true key containing list of upgradeable installed modules
        /// and false key containing list of non-upgradeable installed modules
        /// </returns>
        public static Dictionary<bool, List<CkanModule>> CheckUpgradeable(this IRegistryQuerier querier,
                                                                          GameInstance          instance,
                                                                          HashSet<string>       heldIdents,
                                                                          HashSet<string>?      ignoreMissingIdents = null)
        {
            var filters = ServiceLocator.Container.Resolve<IConfiguration>()
                                                  .GetGlobalInstallFilters(instance.game)
                                                  .Concat(instance.InstallFilters)
                                                  .ToHashSet();
            // Get the absolute latest versions ignoring restrictions,
            // to break out of mutual version-depending deadlocks
            var unlimited = querier.Installed(false)
                                   .Keys
                                   .Select(ident => !heldIdents.Contains(ident)
                                                    && querier.HasUpdate(ident, instance.StabilityToleranceConfig, instance, filters,
                                                                         !ignoreMissingIdents?.Contains(ident) ?? true,
                                                                         out CkanModule? latest)
                                                    && latest is not null
                                                    && !latest.IsDLC
                                                        ? latest
                                                        : querier.GetInstalledVersion(ident))
                                   .OfType<CkanModule>()
                                   .ToList();
            return querier.CheckUpgradeable(instance, heldIdents, unlimited, filters, ignoreMissingIdents);
        }

        /// <summary>
        /// Partition the installed mods based on whether they are upgradeable
        /// </summary>
        /// <param name="querier">A registry</param>
        /// <param name="instance">The registry's game instance</param>
        /// <param name="heldIdents">Identifiers of mods that the user has designated as not to be upgraded</param>
        /// <param name="initial">Installed modules to start out considering as possibly upgradeable, to be checked against each other</param>
        /// <param name="filters">Install filters that will cause missing files to be skipped</param>
        /// <param name="ignoreMissingIdents">Identifiers of mods that the user has designated as allowed to have missing files</param>
        /// <returns>
        /// Dictionary with true key containing list of upgradeable installed modules
        /// and false key containing list of non-upgradeable installed modules
        /// </returns>
        public static Dictionary<bool, List<CkanModule>> CheckUpgradeable(this IRegistryQuerier querier,
                                                                          GameInstance          instance,
                                                                          HashSet<string>       heldIdents,
                                                                          List<CkanModule>      initial,
                                                                          HashSet<string>?      filters             = null,
                                                                          HashSet<string>?      ignoreMissingIdents = null)
        {
            filters ??= ServiceLocator.Container.Resolve<IConfiguration>()
                                                .GetGlobalInstallFilters(instance.game)
                                                .Concat(instance.InstallFilters)
                                                .ToHashSet();
            // Use those as the installed modules
            var upgradeable    = new List<CkanModule>();
            var notUpgradeable = new List<CkanModule>();
            foreach (var ident in initial.Select(module => module.identifier))
            {
                if (!heldIdents.Contains(ident)
                    && querier.HasUpdate(ident, instance.StabilityToleranceConfig, instance, filters,
                                         !ignoreMissingIdents?.Contains(ident) ?? true,
                                         out CkanModule? latest, initial)
                    && latest is not null
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

        /// <summary>
        /// Check if any important metadata of a module has changed since it was installed
        /// </summary>
        /// <param name="querier">A registry</param>
        /// <param name="identifier">Identifier of mod to check</param>
        /// <returns>True if any property has changed that can affect how the mod is installed</returns>
        public static bool MetadataChanged(this IRegistryQuerier querier, string identifier)
        {
            try
            {
                var installed = querier.InstalledModule(identifier)?.Module;
                return installed != null
                    && (!querier.GetModuleByVersion(identifier, installed.version)
                                ?.MetadataEquals(installed)
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
        /// <param name="querier">A registry</param>
        /// <param name="game">Game to represent</param>
        /// <param name="identifier">Mod name to findDependencyShallow</param>
        /// <returns>
        /// String describing range of compatible game versions.
        /// </returns>
        public static string CompatibleGameVersions(this IRegistryQuerier querier,
                                                    IGame                 game,
                                                    string                identifier)
        {
            List<CkanModule>? releases = null;
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
                                             out GameVersion? minKsp, out GameVersion? maxKsp);
                return GameVersionRange.VersionSpan(game,
                                                    minKsp ?? GameVersion.Any,
                                                    maxKsp ?? GameVersion.Any);
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
            CkanModule.GetMinMaxVersions(new CkanModule[] { module },
                                         out _, out _,
                                         out GameVersion? minKsp, out GameVersion? maxKsp);
            return GameVersionRange.VersionSpan(game,
                                                minKsp ?? GameVersion.Any,
                                                maxKsp ?? GameVersion.Any);
        }

        /// <summary>
        /// Is the mod installed and does it have a replaced_by relationship with a compatible version
        /// Check latest information on installed version of mod "identifier" and if it has a "replaced_by"
        /// value, check if there is a compatible version of the linked mod
        /// Given a mod identifier, return a ModuleReplacement containing the relevant replacement
        /// if compatibility matches.
        /// </summary>
        public static ModuleReplacement? GetReplacement(this IRegistryQuerier    querier,
                                                        string                   identifier,
                                                        StabilityToleranceConfig stabilityTolerance,
                                                        GameVersionCriteria      version)
        // We only care about the installed version
            => querier.GetInstalledVersion(identifier) is CkanModule mod
                ? Utilities.DefaultIfThrows(() => querier.GetReplacement(mod, stabilityTolerance, version))
                : null;

        public static ModuleReplacement? GetReplacement(this IRegistryQuerier    querier,
                                                        CkanModule               installedVersion,
                                                        StabilityToleranceConfig stabilityTolerance,
                                                        GameVersionCriteria      version)
        {
            // No replaced_by relationship
            if (installedVersion.replaced_by == null)
            {
                return null;
            }

            // Get the identifier from the replaced_by relationship, if it exists
            var replacedBy = installedVersion.replaced_by;

            // Now we need to see if there is a compatible version of the replacement
            try
            {
                if (installedVersion.replaced_by.version != null)
                {
                    if (querier.GetModuleByVersion(installedVersion.replaced_by.name, installedVersion.replaced_by.version)
                        is CkanModule replacement && replacement.IsCompatible(version))
                    {
                        return new ModuleReplacement(installedVersion, replacement);
                    }
                }
                else if (querier.LatestAvailable(installedVersion.replaced_by.name, stabilityTolerance, version, replacedBy)
                         is CkanModule replacement && replacement.IsCompatible(version))
                {
                    return new ModuleReplacement(installedVersion, replacement);
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
        /// <param name="querier">A registry</param>
        /// <param name="installed">The modules currently installed</param>
        /// <param name="installing">The modules to be installed</param>
        /// <param name="game">The registry's game instance</param>
        /// <param name="stabilityTolerance">Stability tolerance for the game instance</param>
        /// <param name="crit">Version criteria for resolving relationships</param>
        /// <returns>
        /// Sequence of removable auto-installed modules, if any
        /// </returns>
        public static IEnumerable<InstalledModule> FindRemovableAutoInstalled(
            this IRegistryQuerier                querier,
            IReadOnlyCollection<InstalledModule> installed,
            IReadOnlyCollection<CkanModule>      installing,
            IGame                                game,
            StabilityToleranceConfig             stabilityTolerance,
            GameVersionCriteria                  crit)
        {
            log.DebugFormat("Finding removable autoInstalled for: {0}",
                            string.Join(", ", installed.Select(im => im.identifier)));

            var opts = RelationshipResolverOptions.DependsOnlyOpts(stabilityTolerance);
            opts.without_toomanyprovides_kraken = true;
            opts.without_enforce_consistency    = true;
            opts.proceed_with_inconsistencies   = true;

            // Calculate the full changeset for the mods we're installing
            // (the already installed ones already have their dependencies in the registry)
            var resolver = new RelationshipResolver(
                               installing,
                               querier.InstalledModules.Except(installed.Where(im => !im.AutoInstalled))
                                                       .Select(other => other.Module),
                               opts, querier, game, crit);
            // Keep the mods that are not auto-installed
            var keeping = installed.Where(im => !im.AutoInstalled)
                                   .Select(im => im.Module)
                                   // Don't remove anything we still need for newly installed mods
                                   .Concat(resolver.ModList())
                                   .Distinct()
                                   .ToHashSet();
            // Treat any auto-installed mods that aren't marked as needed as initially removable
            var removable = installed.Where(im => im.AutoInstalled
                                                  && !keeping.Contains(im.Module))
                                     .ToList();
            // Get the unsatisfied relationships of mods we're installing or not removing
            var depends = keeping.SelectMany(m => m.depends
                                                  ?? Enumerable.Empty<RelationshipDescriptor>())
                                 .Distinct()
                                 .Where(dep => !dep.MatchesAny(keeping, null, null))
                                 .ToArray();
            if (depends.Length > 0)
            {
                while (true)
                {
                    // Find previously-removable modules that satisfy previously unsatisfied relationships
                    var stillNeeded = removable.Select(im => depends.Where(dep => dep.MatchesAny(new CkanModule[] { im.Module },
                                                                                                 null, null))
                                                                    .ToArray()
                                                             is { Length: > 0 } deps
                                                                 ? (KeyValuePair<InstalledModule, RelationshipDescriptor[]>?)
                                                                   new KeyValuePair<InstalledModule, RelationshipDescriptor[]>(im, deps)
                                                                 : null)
                                               .OfType<KeyValuePair<InstalledModule, RelationshipDescriptor[]>>()
                                               .ToDictionary();
                    if (stillNeeded.Count > 0)
                    {
                        // Move this pass of mods we still need from removable into keeping
                        removable.RemoveAll(stillNeeded.ContainsKey);
                        keeping.UnionWith(stillNeeded.Keys.Select(im => im.Module));

                        // Remove relationships that are satisfied by this pass of mods we still need
                        depends = depends.Except(stillNeeded.Values.SelectMany(deps => deps))
                                         // Add relationships from this pass of mods...
                                         .Concat(stillNeeded.Keys.SelectMany(im => im.Module.depends
                                                                                   ?? Enumerable.Empty<RelationshipDescriptor>())
                                                            // ... except for ones that are already satisfied
                                                            .Where(dep => !dep.MatchesAny(keeping, null, null)))
                                         .Distinct()
                                         .ToArray();
                        // If no relationships still need to be satisfied, we are done
                        if (depends.Length < 1)
                        {
                            return removable;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return removable;
        }

        /// <summary>
        /// Find auto-installed modules that have no depending modules
        /// or only auto-installed depending modules.
        /// </summary>
        /// <param name="querier">A registry</param>
        /// <param name="instance">The game instance</param>
        /// <returns>
        /// Sequence of removable auto-installed modules, if any
        /// </returns>
        public static IEnumerable<InstalledModule> FindRemovableAutoInstalled(
            this IRegistryQuerier querier,
            GameInstance          instance)
            => querier.FindRemovableAutoInstalled(querier.InstalledModules,
                                                  Array.Empty<CkanModule>(),
                                                  instance.game,
                                                  instance.StabilityToleranceConfig,
                                                  instance.VersionCriteria());

        private static readonly ILog log = LogManager.GetLogger(typeof(IRegistryQuerierHelpers));
    }
}
