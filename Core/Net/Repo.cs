using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ChinhDo.Transactions;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using log4net;

namespace CKAN
{
    /// <summary>
    ///     Class for downloading the CKAN meta-info itself.
    /// </summary>
    public static class Repo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Repo));
        private static TxFileManager file_transaction = new TxFileManager();

        // Forward to keep existing code compiling, will be removed soon.
        public static readonly Uri default_ckan_repo = CKAN.Repository.default_ckan_repo_uri;

        internal static void ProcessRegistryMetadataFromJSON(string metadata, Registry registry, string filename)
        {
            log.DebugFormat("Converting metadata from JSON.");

            try
            {
                CkanModule module = CkanModule.FromJson(metadata);
                log.InfoFormat("Found {0} version {1}", module.identifier, module.version);
                registry.AddAvailable(module);
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
            }
        }

        /// <summary>
        ///     Download and update the local CKAN meta-info.
        ///     Optionally takes a URL to the zipfile repo to download.
        ///     Returns the number of unique modules updated.
        /// </summary>
        public static int UpdateAllRepositories(RegistryManager registry_manager, KSP ksp, IUser user)
        {
            // If we handle multiple repositories, we will call ClearRegistry() ourselves...
            registry_manager.registry.ClearAvailable();
            // TODO this should already give us a pre-sorted list
            SortedDictionary<string, Repository> sortedRepositories = registry_manager.registry.Repositories;
            foreach (KeyValuePair<string, Repository> repository in sortedRepositories)
            {
                log.InfoFormat("About to update {0}", repository.Value.name);
                UpdateRegistry(repository.Value.uri, registry_manager.registry, ksp, user, false);
                log.InfoFormat("Updated {0}", repository.Value.name);
            }

            // Save our changes.
            registry_manager.Save(enforce_consistency: false);

            ShowUserInconsistencies(registry_manager.registry, user);

            // Return how many we got!
            return registry_manager.registry.Available(ksp.Version()).Count;
        }

        public static int Update(RegistryManager registry_manager, KSP ksp, IUser user, Boolean clear = true, string repo = null)
        {
            if (repo == null)
            {
                return Update(registry_manager, ksp, user, clear, (Uri)null);
            }

            return Update(registry_manager, ksp, user, clear, new Uri(repo));
        }

        /// <summary>
        /// Updates the supplied registry from the URL given.
        /// This does not *save* the registry. For that, you probably want Repo.Update
        /// </summary>
        internal static void UpdateRegistry(Uri repo, Registry registry, KSP ksp, IUser user, Boolean clear = true)
        {
            log.InfoFormat("Downloading {0}", repo);

            string repo_file = String.Empty;
            try
            {
                repo_file = Net.Download(repo);
            }
            catch (System.Net.WebException)
            {
                user.RaiseMessage("Connection to {0} could not be established.", repo);
                return;
            }

            // Clear our list of known modules.
            if (clear)
            {
                registry.ClearAvailable();
            }

            // Check the filetype.
            FileType type = FileIdentifier.IdentifyFile(repo_file);

            switch (type)
            {
            case FileType.TarGz:
                UpdateRegistryFromTarGz (repo_file, registry);
                break;
            case FileType.Zip:
                UpdateRegistryFromZip (repo_file, registry);
                break;
            default:
                break;
            }

            List<CkanModule> metadataChanges = new List<CkanModule>();

            foreach (var installedModule in registry.InstalledModules)
            {
                var identifier = installedModule.identifier;

                var installedVersion = registry.InstalledVersion(identifier);
                if (!(registry.available_modules.ContainsKey(identifier)))
                {
                    log.InfoFormat("UpdateRegistry, module {0}, version {1} not in repository ({2})", identifier, installedVersion, repo);
                    continue;
                }

                if (!registry.available_modules[identifier].module_version.ContainsKey(installedVersion))
                {
                    continue;
                }

                // if the mod is installed and the metadata is different we have to reinstall it
                var metadata = registry.available_modules[identifier].module_version[installedVersion];

                var oldMetadata = registry.InstalledModule(identifier).Module;

                bool same = true;
                if ((metadata.install == null) != (oldMetadata.install == null) ||
                    (metadata.install != null && metadata.install.Length != oldMetadata.install.Length))
                {
                    same = false;
                }
                else
                {
                    if(metadata.install != null)
                    for (int i = 0; i < metadata.install.Length; i++)
                    {
                        if (metadata.install[i].file != oldMetadata.install[i].file)
                        {
                            same = false;
                            break;
                        }

                        if (metadata.install[i].install_to != oldMetadata.install[i].install_to)
                        {
                            same = false;
                            break;
                        }

                        if (metadata.install[i].@as != oldMetadata.install[i].@as)
                        {
                            same = false;
                            break;
                        }

                        if ((metadata.install[i].filter == null) != (oldMetadata.install[i].filter == null))
                        {
                            same = false;
                            break;
                        }


                        if(metadata.install[i].filter != null)
                        if (!metadata.install[i].filter.SequenceEqual(oldMetadata.install[i].filter))
                        {
                            same = false;
                            break;
                        }

                        if ((metadata.install[i].filter_regexp == null) != (oldMetadata.install[i].filter_regexp == null))
                        {
                            same = false;
                            break;
                        }

                        if(metadata.install[i].filter_regexp != null)
                        if (!metadata.install[i].filter_regexp.SequenceEqual(oldMetadata.install[i].filter_regexp))
                        {
                            same = false;
                            break;
                        }
                    }
                }

                if (!RelationshipsAreEquivalent(metadata.conflicts, oldMetadata.conflicts))
                    same = false;

                if (!RelationshipsAreEquivalent(metadata.depends, oldMetadata.depends))
                    same = false;

                if (!RelationshipsAreEquivalent(metadata.recommends, oldMetadata.recommends))
                    same = false;

                if (metadata.provides != oldMetadata.provides)
                {
                    if (metadata.provides == null || oldMetadata.provides == null)
                        same = false;
                    else if (!metadata.provides.OrderBy(i => i).SequenceEqual(oldMetadata.provides.OrderBy(i => i)))
                        same = false;
                }

                if (!same)
                {
                    metadataChanges.Add(registry.available_modules[identifier].module_version[installedVersion]);
                }
            }

            if (metadataChanges.Any())
            {
                var sb = new StringBuilder();

                for (var i = 0; i < metadataChanges.Count; i++)
                {
                    var module = metadataChanges[i];

                    sb.AppendLine(string.Format("- {0} {1}", module.identifier, module.version));
                }

                if(user.RaiseYesNoDialog(string.Format(@"The following mods have had their metadata changed since last update:

{0}
You should reinstall them in order to preserve consistency with the repository.

Do you wish to reinstall now?", sb)))
                {
                    ModuleInstaller installer = ModuleInstaller.GetInstance(ksp, new NullUser());
                    // New upstream metadata may break the consistency of already installed modules
                    // e.g. if user installs modules A and B and then later up A is made to conflict with B
                    // This is perfectly normal and shouldn't produce an error, therefore we skip enforcing
                    // consistency. However, we will show the user any inconsistencies later on.

                    // Use the identifiers so we use the overload that actually resolves relationships
                    // Do each changed module one at a time so a failure of one doesn't cause all the others to fail
                    foreach (var changedIdentifier in metadataChanges.Select(i => i.identifier))
                    {
                        try
                        {
                            installer.Upgrade(
                                new[] { changedIdentifier },
                                new NetAsyncDownloader(new NullUser()),
                                enforceConsistency: false
                            );
                        }
                        // Thrown when a dependency couldn't be satisfied
                        catch(ModuleNotFoundKraken)
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

            // Remove our downloaded meta-data now we've processed it.
            // Seems weird to do this as part of a transaction, but Net.Download uses them, so let's be consistent.
            file_transaction.Delete(repo_file);
        }

        private static bool RelationshipsAreEquivalent(List<RelationshipDescriptor> a, List<RelationshipDescriptor> b)
        {
            if (a == b)
                return true; // If they're the same exact object they must be equivalent

            if (a == null || b == null)
                return false; // If they're not the same exact object and either is nul then must not be equivalent

            if (a.Count != b.Count)
                return false; // If their counts different they must not be equivalent

            // Sort the lists so we can compare each relationship
            var aSorted = a.OrderBy(i => i.name).ToList();
            var bSorted = b.OrderBy(i => i.name).ToList();

            for(var i = 0; i < a.Count; i++)
            {
                var aRel = aSorted[i];
                var bRel = bSorted[i];

                if (aRel.name != bRel.name)
                {
                    return false; // If corresponding relationships are the same they must not be equivalent
                }

                // Calculate min/max for each based on explicit min/max or the bare version
                var aMinVersion = aRel.min_version ?? aRel.version;
                var aMaxVersion = aRel.max_version ?? aRel.version;
                var bMinVersion = bRel.min_version ?? bRel.version;
                var bMaxVersion = bRel.max_version ?? bRel.version;

                if (!ReferenceEquals(aMinVersion, bMinVersion))
                {
                    // If they're not the same object they may not be equivalent

                    if (aMinVersion != null && bMinVersion != null)
                    {
                        if (aMinVersion.CompareTo(bMinVersion) != 0)
                        {
                            // If they're not equal then the ymust not be equivalent
                            return false;
                        }
                    }
                    else
                    {
                        // If one or the other is null they must not be equivalent
                        return false;
                    }
                }

                if (!ReferenceEquals(aMaxVersion, bMaxVersion))
                {
                    // If they're not the same object they may not be equivalent

                    if (aMaxVersion != null && bMaxVersion != null)
                    {
                        if (aMaxVersion.CompareTo(bMaxVersion) != 0)
                        {
                            // If they're not equal then the ymust not be equivalent
                            return false;
                        }
                    }
                    else
                    {
                        // If one or the other is null they must not be equivalent
                        return false;
                    }
                }
            }

            // If we couldn't find any differences they must be equivalent
            return true;
        }

        private static int Update(RegistryManager registry_manager, KSP ksp, IUser user, Boolean clear = true, Uri repo = null)
        {
            // Use our default repo, unless we've been told otherwise.
            if (repo == null)
            {
                repo = default_ckan_repo;
            }

            UpdateRegistry(repo, registry_manager.registry, ksp, user, clear);

            // Save our changes!
            registry_manager.Save(enforce_consistency: false);

            ShowUserInconsistencies(registry_manager.registry, user);

            // Return how many we got!
            return registry_manager.registry.Available(ksp.Version()).Count;
        }

        /// <summary>
        /// Updates the supplied registry from the supplied zip file.
        /// This will *clear* the registry of available modules first.
        /// This does not *save* the registry. For that, you probably want Repo.Update
        /// </summary>
        internal static void UpdateRegistryFromTarGz(string path, Registry registry)
        {
            log.DebugFormat("Starting registry update from tar.gz file: \"{0}\".", path);

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

                            // Skip things we don't want.
                            if (!Regex.IsMatch(filename, filter))
                            {
                                log.DebugFormat("Skipping archive entry {0}", filename);
                                continue;
                            }

                            log.DebugFormat("Reading CKAN data from {0}", filename);

                            // Read each file into a buffer.
                            int buffer_size;

                            try
                            {
                                buffer_size = Convert.ToInt32(entry.Size);
                            }
                            catch (OverflowException)
                            {
                                log.ErrorFormat("Error processing {0}: Metadata size too large.", entry.Name);
                                continue;
                            }

                            byte[] buffer = new byte[buffer_size];

                            tarStream.Read(buffer, 0, buffer_size);

                            // Convert the buffer data to a string.
                            string metadata_json = Encoding.ASCII.GetString(buffer);

                            ProcessRegistryMetadataFromJSON(metadata_json, registry, filename);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the supplied registry from the supplied zip file.
        /// This will *clear* the registry of available modules first.
        /// This does not *save* the registry. For that, you probably want Repo.Update
        /// </summary>
        internal static void UpdateRegistryFromZip(string path, Registry registry)
        {
            log.DebugFormat("Starting registry update from zip file: \"{0}\".", path);

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

                    ProcessRegistryMetadataFromJSON(metadata_json, registry, filename);
                }

                zipfile.Close();
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