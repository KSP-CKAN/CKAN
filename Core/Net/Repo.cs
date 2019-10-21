using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autofac;
using ChinhDo.Transactions.FileManager;
using CKAN.GameVersionProviders;
using CKAN.Versioning;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Newtonsoft.Json;

namespace CKAN
{
    public enum RepoUpdateResult
    {
        Failed,
        Updated,
        NoChanges
    }

    /// <summary>
    ///     Class for downloading the CKAN meta-info itself.
    /// </summary>
    public static class Repo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Repo));

        /// <summary>
        /// Download and update the local CKAN meta-info.
        /// Optionally takes a URL to the zipfile repo to download.
        /// </summary>
        public static RepoUpdateResult UpdateAllRepositories(RegistryManager registry_manager, KSP ksp, NetModuleCache cache, IUser user)
        {
            SortedDictionary<string, Repository> sortedRepositories = registry_manager.registry.Repositories;
            if (sortedRepositories.Values.All(repo => !string.IsNullOrEmpty(repo.last_server_etag) && repo.last_server_etag == Net.CurrentETag(repo.uri)))
            {
                user?.RaiseMessage("No changes since last update");
                return RepoUpdateResult.NoChanges;
            }
            List<CkanModule> allAvail = new List<CkanModule>();
            foreach (KeyValuePair<string, Repository> repository in sortedRepositories)
            {
                log.InfoFormat("About to update {0}", repository.Value.name);
                SortedDictionary<string, int> downloadCounts;
                string newETag;
                List<CkanModule> avail = UpdateRegistry(repository.Value.uri, ksp, user, out downloadCounts, out newETag);
                registry_manager.registry.SetDownloadCounts(downloadCounts);
                if (avail == null)
                {
                    // Report failure if any repo fails, rather than losing half the list.
                    // UpdateRegistry will have alerted the user to specific errors already.
                    return RepoUpdateResult.Failed;
                }
                else
                {
                    log.InfoFormat("Updated {0}", repository.Value.name);
                    // Merge all the lists
                    allAvail.AddRange(avail);
                    repository.Value.last_server_etag = newETag;
                }
            }
            // Save allAvail to the registry if we found anything
            if (allAvail.Count > 0)
            {
                registry_manager.registry.SetAllAvailable(allAvail);
                // Save our changes.
                registry_manager.Save(enforce_consistency: false);

                ShowUserInconsistencies(registry_manager.registry, user);

                List<CkanModule> metadataChanges = GetChangedInstalledModules(registry_manager.registry);
                if (metadataChanges.Count > 0)
                {
                    HandleModuleChanges(metadataChanges, user, ksp, cache);
                }

                // Registry.Available is slow, just return success,
                // caller can check it if it's really needed
                return RepoUpdateResult.Updated;
            }
            else
            {
                // Return failure
                return RepoUpdateResult.Failed;
            }
        }

        /// <summary>
        /// Retrieve available modules from the URL given.
        /// </summary>
        private static List<CkanModule> UpdateRegistry(Uri repo, KSP ksp, IUser user, out SortedDictionary<string, int> downloadCounts, out string currentETag)
        {
            TxFileManager file_transaction = new TxFileManager();
            downloadCounts = null;

            // Use this opportunity to also update the build mappings... kind of hacky
            ServiceLocator.Container.Resolve<IKspBuildMap>().Refresh();

            log.InfoFormat("Downloading {0}", repo);

            string repo_file = String.Empty;
            try
            {
                repo_file = Net.Download(repo, out currentETag);
            }
            catch (System.Net.WebException ex)
            {
                user.RaiseMessage("Failed to download {0}: {1}", repo, ex.ToString());
                currentETag = null;
                return null;
            }

            // Check the filetype.
            FileType type = FileIdentifier.IdentifyFile(repo_file);

            List<CkanModule> newAvailable = null;
            switch (type)
            {
                case FileType.TarGz:
                    newAvailable = UpdateRegistryFromTarGz(repo_file, out downloadCounts);
                    break;
                case FileType.Zip:
                    newAvailable = UpdateRegistryFromZip(repo_file);
                    break;
            }

            // Remove our downloaded meta-data now we've processed it.
            // Seems weird to do this as part of a transaction, but Net.Download uses them, so let's be consistent.
            file_transaction.Delete(repo_file);

            return newAvailable;
        }

        /// <summary>
        /// Find installed modules that have different metadata in their equivalent available module
        /// </summary>
        /// <param name="registry">Registry to scan</param>
        /// <returns>
        /// List of CkanModules that are available and have changed metadata
        /// </returns>
        private static List<CkanModule> GetChangedInstalledModules(Registry registry)
        {
            List<CkanModule> metadataChanges = new List<CkanModule>();
            foreach (InstalledModule installedModule in registry.InstalledModules)
            {
                string identifier = installedModule.identifier;

                ModuleVersion installedVersion = registry.InstalledVersion(identifier);
                if (!(registry.available_modules.ContainsKey(identifier)))
                {
                    log.InfoFormat("UpdateRegistry, module {0}, version {1} not in registry", identifier, installedVersion);
                    continue;
                }

                if (!registry.available_modules[identifier].module_version.ContainsKey(installedVersion))
                {
                    continue;
                }

                // if the mod is installed and the metadata is different we have to reinstall it
                CkanModule metadata = registry.available_modules[identifier].module_version[installedVersion];

                CkanModule oldMetadata = registry.InstalledModule(identifier).Module;

                if (!MetadataEquals(metadata, oldMetadata))
                {
                    metadataChanges.Add(registry.available_modules[identifier].module_version[installedVersion]);
                }
            }
            return metadataChanges;
        }

        /// <summary>
        /// Resolve differences between installed and available metadata for given ModuleInstaller
        /// </summary>
        /// <param name="metadataChanges">List of modules that changed</param>
        /// <param name="user">Object for user interaction callbacks</param>
        /// <param name="ksp">Game instance</param>
        private static void HandleModuleChanges(List<CkanModule> metadataChanges, IUser user, KSP ksp, NetModuleCache cache)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < metadataChanges.Count; i++)
            {
                CkanModule module = metadataChanges[i];
                sb.AppendLine(string.Format("- {0} {1}", module.identifier, module.version));
            }

            if (user.RaiseYesNoDialog(string.Format(@"The following mods have had their metadata changed since last update:

{0}
You should reinstall them in order to preserve consistency with the repository.

Do you wish to reinstall now?", sb)))
            {
                ModuleInstaller installer = ModuleInstaller.GetInstance(ksp, cache, new NullUser());
                // New upstream metadata may break the consistency of already installed modules
                // e.g. if user installs modules A and B and then later up A is made to conflict with B
                // This is perfectly normal and shouldn't produce an error, therefore we skip enforcing
                // consistency. However, we will show the user any inconsistencies later on.

                // Use the identifiers so we use the overload that actually resolves relationships
                // Do each changed module one at a time so a failure of one doesn't cause all the others to fail
                foreach (string changedIdentifier in metadataChanges.Select(i => i.identifier))
                {
                    try
                    {
                        installer.Upgrade(
                            new[] { changedIdentifier },
                            new NetAsyncModulesDownloader(new NullUser(), cache),
                            enforceConsistency: false
                        );
                    }
                    // Thrown when a dependency couldn't be satisfied
                    catch (ModuleNotFoundKraken)
                    {
                        log.WarnFormat("Skipping installation of {0} due to relationship error.", changedIdentifier);
                        user.RaiseMessage("Skipping installation of {0} due to relationship error.", changedIdentifier);
                    }
                    // Thrown when a conflicts relationship is violated
                    catch (InconsistentKraken)
                    {
                        log.WarnFormat("Skipping installation of {0} due to relationship error.", changedIdentifier);
                        user.RaiseMessage("Skipping installation of {0} due to relationship error.", changedIdentifier);
                    }
                }
            }
        }

        private static bool MetadataEquals(CkanModule metadata, CkanModule oldMetadata)
        {
            if ((metadata.install == null) != (oldMetadata.install == null)
                    || (metadata.install != null
                        && metadata.install.Length != oldMetadata.install.Length))
            {
                return false;
            }
            else if (metadata.install != null)
            {
                for (int i = 0; i < metadata.install.Length; i++)
                {
                    if (!metadata.install[i].Equals(oldMetadata.install[i]))
                        return false;
                }
            }

            if (!RelationshipsAreEquivalent(metadata.conflicts,  oldMetadata.conflicts))
                return false;

            if (!RelationshipsAreEquivalent(metadata.depends,    oldMetadata.depends))
                return false;

            if (!RelationshipsAreEquivalent(metadata.recommends, oldMetadata.recommends))
                return false;

            if (metadata.provides != oldMetadata.provides)
            {
                if (metadata.provides == null || oldMetadata.provides == null)
                    return false;
                else if (!metadata.provides.OrderBy(i => i).SequenceEqual(oldMetadata.provides.OrderBy(i => i)))
                    return false;
            }
            return true;
        }

        private static bool RelationshipsAreEquivalent(List<RelationshipDescriptor> a, List<RelationshipDescriptor> b)
        {
            if (a == b)
                // If they're the same exact object they must be equivalent
                return true;

            if (a == null || b == null)
                // If they're not the same exact object and either is null then must not be equivalent
                return false;

            if (a.Count != b.Count)
                // If their counts different they must not be equivalent
                return false;

            // Sort the lists so we can compare each relationship
            var aSorted = a.OrderBy(i => i.ToString()).ToList();
            var bSorted = b.OrderBy(i => i.ToString()).ToList();

            for (var i = 0; i < a.Count; i++)
            {
                var aRel = aSorted[i];
                var bRel = bSorted[i];

                if (!aRel.Equals(bRel))
                {
                    return false;
                }
            }

            // If we couldn't find any differences they must be equivalent
            return true;
        }

        /// <summary>
        /// Set a registry's available modules to the list from just one repo
        /// </summary>
        /// <param name="registry_manager">Manager of the regisry of interest</param>
        /// <param name="ksp">Game instance</param>
        /// <param name="user">Object for user interaction callbacks</param>
        /// <param name="repo">Repository to check</param>
        /// <returns>
        /// Number of modules found in repo
        /// </returns>
        public static bool Update(RegistryManager registry_manager, KSP ksp, IUser user, string repo = null)
        {
            if (repo == null)
            {
                return Update(registry_manager, ksp, user, (Uri)null);
            }

            return Update(registry_manager, ksp, user, new Uri(repo));
        }

        // Same as above, just with a Uri instead of string for the repo
        public static bool Update(RegistryManager registry_manager, KSP ksp, IUser user, Uri repo = null)
        {
            // Use our default repo, unless we've been told otherwise.
            if (repo == null)
            {
                repo = CKAN.Repository.default_ckan_repo_uri;
            }

            SortedDictionary<string, int> downloadCounts;
            string newETag;
            List<CkanModule> newAvail = UpdateRegistry(repo, ksp, user, out downloadCounts, out newETag);

            registry_manager.registry.SetDownloadCounts(downloadCounts);

            if (newAvail != null && newAvail.Count > 0)
            {
                registry_manager.registry.SetAllAvailable(newAvail);
                // Save our changes!
                registry_manager.Save(enforce_consistency: false);
            }

            ShowUserInconsistencies(registry_manager.registry, user);

            // Registry.Available is slow, just return success,
            // caller can check it if it's really needed
            return true;
        }

        /// <summary>
        /// Returns available modules from the supplied tar.gz file.
        /// </summary>
        private static List<CkanModule> UpdateRegistryFromTarGz(string path, out SortedDictionary<string, int> downloadCounts)
        {
            log.DebugFormat("Starting registry update from tar.gz file: \"{0}\".", path);

            downloadCounts = null;
            List<CkanModule> modules = new List<CkanModule>();
            // Open the gzip'ed file.
            using (Stream inputStream = File.OpenRead(path))
            {
                // Create a gzip stream.
                using (GZipInputStream gzipStream = new GZipInputStream(inputStream))
                {
                    // Create a handle for the tar stream.
                    using (TarInputStream tarStream = new TarInputStream(gzipStream))
                    {
                        // Walk the archive, looking for .ckan files.
                        const string filter = @"\.ckan$";

                        while (true)
                        {
                            TarEntry entry = tarStream.GetNextEntry();

                            // Check for EOF.
                            if (entry == null)
                            {
                                break;
                            }

                            string filename = entry.Name;

                            if (filename.EndsWith("download_counts.json"))
                            {
                                downloadCounts = JsonConvert.DeserializeObject<SortedDictionary<string, int>>(
                                    tarStreamString(tarStream, entry)
                                );
                                continue;
                            }
                            else if (!Regex.IsMatch(filename, filter))
                            {
                                // Skip things we don't want.
                                log.DebugFormat("Skipping archive entry {0}", filename);
                                continue;
                            }

                            log.DebugFormat("Reading CKAN data from {0}", filename);

                            // Read each file into a buffer.
                            string metadata_json = tarStreamString(tarStream, entry);

                            CkanModule module = ProcessRegistryMetadataFromJSON(metadata_json, filename);
                            if (module != null)
                            {
                                modules.Add(module);
                            }
                        }
                    }
                }
            }
            return modules;
        }

        private static string tarStreamString(TarInputStream stream, TarEntry entry)
        {
            // Read each file into a buffer.
            int buffer_size;

            try
            {
                buffer_size = Convert.ToInt32(entry.Size);
            }
            catch (OverflowException)
            {
                log.ErrorFormat("Error processing {0}: Metadata size too large.", entry.Name);
                return null;
            }

            byte[] buffer = new byte[buffer_size];

            stream.Read(buffer, 0, buffer_size);

            // Convert the buffer data to a string.
            return Encoding.ASCII.GetString(buffer);
        }

        /// <summary>
        /// Returns available modules from the supplied zip file.
        /// </summary>
        private static List<CkanModule> UpdateRegistryFromZip(string path)
        {
            log.DebugFormat("Starting registry update from zip file: \"{0}\".", path);

            List<CkanModule> modules = new List<CkanModule>();
            using (var zipfile = new ZipFile(path))
            {
                // Walk the archive, looking for .ckan files.
                const string filter = @"\.ckan$";

                foreach (ZipEntry entry in zipfile)
                {
                    string filename = entry.Name;

                    // Skip things we don't want.
                    if (! Regex.IsMatch(filename, filter))
                    {
                        log.DebugFormat("Skipping archive entry {0}", filename);
                        continue;
                    }

                    log.DebugFormat("Reading CKAN data from {0}", filename);

                    // Read each file into a string.
                    string metadata_json;
                    using (var stream = new StreamReader(zipfile.GetInputStream(entry)))
                    {
                        metadata_json = stream.ReadToEnd();
                        stream.Close();
                    }

                    CkanModule module = ProcessRegistryMetadataFromJSON(metadata_json, filename);
                    if (module != null)
                    {
                        modules.Add(module);
                    }
                }

                zipfile.Close();
            }
            return modules;
        }

        private static CkanModule ProcessRegistryMetadataFromJSON(string metadata, string filename)
        {
            log.DebugFormat("Converting metadata from JSON.");

            try
            {
                CkanModule module = CkanModule.FromJson(metadata);
                // FromJson can return null for the empty string
                if (module != null)
                {
                    log.DebugFormat("Found {0} version {1}", module.identifier, module.version);
                }
                return module;
            }
            catch (Exception exception)
            {
                // Alas, we can get exceptions which *wrap* our exceptions,
                // because json.net seems to enjoy wrapping rather than propagating.
                // See KSP-CKAN/CKAN-meta#182 as to why we need to walk the whole
                // exception stack.

                bool handled = false;

                while (exception != null)
                {
                    if (exception is UnsupportedKraken || exception is BadMetadataKraken)
                    {
                        // Either of these can be caused by data meant for future
                        // clients, so they're not really warnings, they're just
                        // informational.

                        log.InfoFormat("Skipping {0} : {1}", filename, exception.Message);

                        // I'd *love a way to "return" from the catch block.
                        handled = true;
                        break;
                    }

                    // Look further down the stack.
                    exception = exception.InnerException;
                }

                // If we haven't handled our exception, then it really was exceptional.
                if (handled == false)
                {
                    if (exception == null)
                    {
                        // Had exception, walked exception tree, reached leaf, got stuck.
                        log.ErrorFormat("Error processing {0} (exception tree leaf)", filename);
                    }
                    else
                    {
                        // In case whatever's calling us is lazy in error reporting, we'll
                        // report that we've got an issue here.
                        log.ErrorFormat("Error processing {0} : {1}", filename, exception.Message);
                    }

                    throw;
                }
                return null;
            }
        }

        private static void ShowUserInconsistencies(Registry registry, IUser user)
        {
            // However, if there are any sanity errors let's show them to the user so at least they're aware
            var sanityErrors = registry.GetSanityErrors();
            if (sanityErrors.Any())
            {
                var sanityMessage = new StringBuilder();

                sanityMessage.AppendLine("The following inconsistencies were found:");
                foreach (var sanityError in sanityErrors)
                {
                    sanityMessage.Append("- ");
                    sanityMessage.AppendLine(sanityError);
                }

                user.RaiseMessage(sanityMessage.ToString());
            }
        }
    }
}
