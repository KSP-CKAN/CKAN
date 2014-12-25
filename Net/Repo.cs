using System;
using System.IO;
using System.Text.RegularExpressions;
using ChinhDo.Transactions;
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

        // Right now we only use the default repo, but in the future we'll
        // want to let users add their own.
        public static readonly Uri default_ckan_repo = new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/master.zip");

        /// <summary>
        ///     Download and update the local CKAN meta-info.
        ///     Optionally takes a URL to the zipfile repo to download.
        ///     Returns the number of unique modules updated.
        /// </summary>
        public static int Update(RegistryManager registry_manager, KSPVersion ksp_version, Uri repo = null)
        {
            // Use our default repo, unless we've been told otherwise.
            if (repo == null)
            {
                repo = default_ckan_repo;
            }

            UpdateRegistry(repo, registry_manager.registry);

            // Save our changes!
            registry_manager.Save();

            // Return how many we got!
            return registry_manager.registry.Available(ksp_version).Count;
        }

        public static int Update(RegistryManager registry_manager, KSPVersion ksp_version, string repo = null)
        {
            if (repo == null)
            {
                return Update(registry_manager, ksp_version, (Uri) null);
            }

            return Update(registry_manager, ksp_version, new Uri(repo));
        }

        /// <summary>
        /// Updates the supplied registry from the URL given.
        /// This will *clear* the registry of available modules first.
        /// This does not *save* the registry. For that, you probably want Repo.Update
        /// </summary>
        internal static void UpdateRegistry(Uri repo, Registry registry)
        {
            log.InfoFormat("Downloading {0}", repo);

            string repo_file = Net.Download(repo);

            using (var zipfile = new ZipFile(repo_file))
            {
                // Clear our list of known modules.
                registry.ClearAvailable();

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

                    log.Debug("Converting from JSON...");

                    try 
                    {
                        CkanModule module = CkanModule.FromJson(metadata_json);
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
                            // In case whatever's calling us is lazy in error reporting, we'll
                            // report that we've got an issue here.
                            log.ErrorFormat("Error processing {0} : {1}", filename, exception.Message);
                            throw;
                        }
                    }
                }

                zipfile.Close();
            }

            // Remove our downloaded meta-data now we've processed it.
            // Seems weird to do this as part of a transaction, but Net.Download uses them, so let's be consistent.
            file_transaction.Delete(repo_file);
        }
    }
}