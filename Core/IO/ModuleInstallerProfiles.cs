using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CKAN.IO
{
    public sealed class ProfileChangeSet
    {
        internal ProfileChangeSet(RelationshipResolver resolver,
                                  CkanModule[] install, InstalledModule[] remove)
        {
            Resolver = resolver;
            Install = Array.AsReadOnly(install);
            Remove = Array.AsReadOnly(remove);
        }

        public IReadOnlyCollection<CkanModule> Install { get; }
        public IReadOnlyCollection<InstalledModule> Remove { get; }
        internal RelationshipResolver Resolver { get; }
    }

    public partial class ModuleInstaller
    {
        /// <summary>
        /// Resolve a complete target, treating obsolete installed mods as removals.
        /// The registry is not changed. Profiles are desired roots, not version locks.
        /// </summary>
        public ProfileChangeSet PlanProfile(string name, Registry registry)
        {
            var crit = instance.VersionCriteria();
            var installed = registry.InstalledModules.Where(im => !im.Module.IsDLC)
                                    .Select(im => im.Module).ToArray();
            var roots = registry.ProfileRoots(name).Select(id =>
                registry.GetInstalledVersion(id) is CkanModule current && current.IsCompatible(crit)
                    ? current
                    : registry.LatestAvailable(id, instance.StabilityToleranceConfig, crit)
                        ?? throw new ModuleNotFoundKraken(id)).ToArray();

            // Retain installed dependency versions where the target's relationships
            // allow them, including dependencies of dependencies. The resolver still
            // validates the entire selection with the old configuration removed.
            var preferred = roots.ToHashSet();
            var candidates = installed.Where(m => m.IsCompatible(crit)).ToArray();
            bool AddInstalledDependencies(IEnumerable<CkanModule> modules)
            {
                bool added = false;
                var queue = new Queue<CkanModule>(modules);
                var visited = new HashSet<CkanModule>();
                while (queue.Count > 0)
                {
                    var module = queue.Dequeue();
                    if (!visited.Add(module))
                    {
                        continue;
                    }
                    foreach (var rel in module.depends ?? Enumerable.Empty<RelationshipDescriptor>())
                    {
                        if (!rel.MatchesAny(preferred, registry.InstalledDlls, registry.InstalledDlc)
                            && rel.MatchesAny(candidates, null, null, out var dependency)
                            && dependency != null && preferred.Add(dependency))
                        {
                            queue.Enqueue(dependency);
                            added = true;
                        }
                    }
                }
                return added;
            }
            AddInstalledDependencies(roots);
            var options = RelationshipResolverOptions.DependsOnlyOpts(instance.StabilityToleranceConfig);
            RelationshipResolver resolver;
            try
            {
                resolver = new RelationshipResolver(preferred, installed, options,
                                                    registry, instance.Game, crit);
            }
            catch (Kraken) when (preferred.Count > roots.Length)
            {
                // An installed dependency may conflict with a newly required mod.
                // Let the normal resolver choose replacements instead.
                resolver = new RelationshipResolver(roots, installed, options,
                                                    registry, instance.Game, crit);
            }
            // Newly selected dependencies can themselves require a still-installed
            // dependency. Retry with those versions pinned as choices for the
            // existing resolver, keeping an unchanged shared dependency in place.
            while (AddInstalledDependencies(resolver.ModList()))
            {
                try
                {
                    resolver = new RelationshipResolver(preferred, installed, options,
                                                        registry, instance.Game, crit);
                }
                catch (Kraken)
                {
                    resolver = new RelationshipResolver(roots, installed, options,
                                                        registry, instance.Game, crit);
                    break;
                }
            }
            var target = resolver.ModList().ToHashSet();
            var dlc = target.FirstOrDefault(m => m.IsDLC);
            if (dlc != null)
            {
                throw new ModuleIsDLCKraken(dlc);
            }
            return new ProfileChangeSet(resolver,
                target.Except(installed).ToArray(),
                registry.InstalledModules.Where(im => !im.Module.IsDLC && !target.Contains(im.Module)).ToArray());
        }

        /// <summary>
        /// Apply a profile with CKAN's file and registry transaction. This provides
        /// exception rollback, not recovery after process termination or power loss.
        /// </summary>
        public void SwitchProfile(string name, RegistryManager manager, IDownloader downloader)
        {
            var registry = manager.registry;
            if (registry.ActiveProfile == name)
            {
                return;
            }
            var plan = PlanProfile(name, registry);
            CheckAddRemoveFreeSpace(plan.Install, plan.Remove);
            downloader.DownloadModules(plan.Install.Where(m => !m.IsMetapackage && !cache.IsCached(m)));
            CheckProfileFiles(plan, registry);
            if (cancelToken.IsCancellationRequested)
            {
                throw new CancelledActionKraken();
            }

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                registry.CaptureActiveProfile();
                registry.SwitchingProfile = true;
                preserveUnmanagedFiles = true;
                try
                {
                    HashSet<string>? unusedDirectories = null;
                    var roots = registry.ProfileRoots(name).ToHashSet();
                    AddRemove(ref unusedDirectories, manager, plan.Resolver,
                              plan.Install, plan.Install.Where(m => !roots.Contains(m.identifier)).ToHashSet(),
                              plan.Remove, new HashSet<CkanModule>(), downloader, true,
                              saveResult: false);
                    if (cancelToken.IsCancellationRequested)
                    {
                        throw new CancelledActionKraken();
                    }
                    registry.ActivateProfile(name);
                    manager.Save();
                    transaction.Complete();
                }
                finally
                {
                    registry.SwitchingProfile = false;
                    preserveUnmanagedFiles = false;
                }
            }
        }

        // Download and inspect every archive before removing any installed file.
        // Installation checks again, so a late collision also aborts the transaction.
        private void CheckProfileFiles(ProfileChangeSet plan, Registry registry)
        {
            var removing = plan.Remove.Select(im => im.identifier).ToHashSet();
            var destinations = new Dictionary<string, bool>(Platform.PathComparer);
            var filters = config.GetGlobalInstallFilters(instance.Game).Concat(instance.InstallFilters).ToArray();
            foreach (var mod in plan.Install.Where(m => !m.IsMetapackage))
            {
                var archive = cache.GetCachedFilename(mod) ?? throw new ModuleNotFoundKraken(mod.identifier);
                foreach (var file in FindInstallableFiles(mod, archive, instance.Game)
                                     .Where(f => !IsInternalCkan(f.source)
                                                 && !filters.Any(filt => f.destination.Contains(filt))))
                {
                    var path = instance.ToAbsoluteGameDir(file.destination);
                    bool isDirectory = file.source.IsDirectory;
                    if (destinations.TryGetValue(path, out bool otherDirectory)
                        && (!isDirectory || !otherDirectory))
                    {
                        throw new FileExistsKraken(path);
                    }
                    destinations[path] = isDirectory;
                    if (File.Exists(path)
                        && (isDirectory || registry.FileOwner(file.destination) is not InstalledModule owner
                                        || !removing.Contains(owner.identifier)))
                    {
                        throw new FileExistsKraken(path);
                    }
                    if (!isDirectory && Directory.Exists(path))
                    {
                        throw new FileExistsKraken(path);
                    }
                    for (var parent = Path.GetDirectoryName(path); parent != null;
                         parent = Path.GetDirectoryName(parent))
                    {
                        if (File.Exists(parent))
                        {
                            throw new FileExistsKraken(parent);
                        }
                    }
                }
            }
            foreach (var path in destinations.Keys)
            {
                for (var parent = Path.GetDirectoryName(path); parent != null;
                     parent = Path.GetDirectoryName(parent))
                {
                    if (destinations.TryGetValue(parent, out bool directory) && !directory)
                    {
                        throw new FileExistsKraken(parent);
                    }
                }
            }
        }
    }
}
