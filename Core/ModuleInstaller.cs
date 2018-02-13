using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using ChinhDo.Transactions;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using CKAN.Types;

namespace CKAN
{
    public delegate void ModuleInstallerReportModInstalled(CkanModule module);

    public struct InstallableFile
    {
        public ZipEntry source;
        public string destination;
        public bool makedir;
    }

    public class ModuleInstaller
    {
        public IUser User { get; set; }

        // To allow the ModuleInstaller to work on multiple KSP instances, keep a list of each ModuleInstaller and return the correct one upon request.
        private static SortedList<string, ModuleInstaller> instances = new SortedList<string, ModuleInstaller>();

        private static readonly ILog log = LogManager.GetLogger(typeof(ModuleInstaller));
        private static readonly TxFileManager file_transaction = new TxFileManager();

        private RegistryManager registry_manager;
        private KSP ksp;

        public ModuleInstallerReportModInstalled onReportModInstalled = null;

        // Our own cache is that of the KSP instance we're using.
        public NetModuleCache Cache
        {
            get
            {
                return ksp.Cache;
            }
        }

        // Constructor
        private ModuleInstaller(KSP ksp, IUser user)
        {
            User = user;
            this.ksp = ksp;
            registry_manager = RegistryManager.Instance(ksp);
            log.DebugFormat("Creating ModuleInstaller for {0}", ksp.GameDir());
        }

        /// <summary>
        /// Gets the ModuleInstaller instance associated with the passed KSP instance. Creates a new ModuleInstaller instance if none exists.
        /// </summary>
        /// <returns>The ModuleInstaller instance.</returns>
        /// <param name="ksp_instance">Current KSP instance.</param>
        /// <param name="user">IUser implementation.</param>
        public static ModuleInstaller GetInstance(KSP ksp_instance, IUser user)
        {
            ModuleInstaller instance;

            // Check in the list of instances if we have already created a ModuleInstaller instance for this KSP instance.
            if (!instances.TryGetValue(ksp_instance.GameDir().ToLower(), out instance))
            {
                // Create a new instance and insert it in the static list.
                instance = new ModuleInstaller(ksp_instance, user);

                instances.Add(ksp_instance.GameDir().ToLower(), instance);
            }
            else if (user != null)
            {
                // Caller passed in a valid IUser. Let's use it.
                instance.User = user;
            }

            return instance;
        }

        /// <summary>
        /// Downloads the given mod to the cache. Returns the filename it was saved to.
        /// </summary>
        public string Download(CkanModule module, string filename)
        {
            User.RaiseProgress(String.Format("Downloading \"{0}\"", module.download), 0);
            return Download(module, filename, Cache);
        }

        /// <summary>
        /// Downloads the given mod to the cache. Returns the filename it was saved to.
        /// </summary>
        public static string Download(CkanModule module, string filename, NetModuleCache cache)
        {
            log.Info("Downloading " + filename);

            string tmp_file = Net.Download(module.download);

            return cache.Store(module, tmp_file, filename, true);
        }

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        ///
        /// If no filename is provided, the module's standard name will be used.
        /// Chcecks the CKAN cache first.
        /// </summary>
        public string CachedOrDownload(CkanModule module, string filename = null)
        {
            return CachedOrDownload(module, Cache, filename);
        }

        /// <summary>
        /// Returns the path to a cached copy of a module if it exists, or downloads
        /// and returns the downloaded copy otherwise.
        ///
        /// If no filename is provided, the module's standard name will be used.
        /// Chcecks provided cache first.
        /// </summary>
        public static string CachedOrDownload(CkanModule module, NetModuleCache cache, string filename = null)
        {
            if (filename == null)
            {
                filename = CkanModule.StandardName(module.identifier, module.version);
            }

            string full_path = cache.GetCachedZip(module);
            if (full_path == null)
            {
                return Download(module, filename, cache);
            }

            log.DebugFormat("Using {0} (cached)", filename);
            return full_path;
        }

        public void InstallList(List<string> modules, RelationshipResolverOptions options, IDownloader downloader = null)
        {
            var resolver = new RelationshipResolver(modules, options, registry_manager.registry, ksp.VersionCriteria());
            InstallList(resolver.ModList().ToList(), options, downloader);
        }

        /// <summary>
        ///     Installs all modules given a list of identifiers as a transaction. Resolves dependencies.
        ///     This *will* save the registry at the end of operation.
        ///
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// Propagates a CancelledActionKraken if the user cancelled the install.
        /// </summary>
        public void InstallList(ICollection<CkanModule> modules, RelationshipResolverOptions options, IDownloader downloader = null)
        {
            // TODO: Break this up into smaller pieces! It's huge!
            var resolver = new RelationshipResolver(modules, options, registry_manager.registry, ksp.VersionCriteria());
            var modsToInstall = resolver.ModList().ToList();
            List<CkanModule> downloads = new List<CkanModule>();

            // TODO: All this user-stuff should be happening in another method!
            // We should just be installing mods as a transaction.

            User.RaiseMessage("About to install...\r\n");

            foreach (CkanModule module in modsToInstall)
            {
                if (!ksp.Cache.IsCachedZip(module))
                {
                    User.RaiseMessage(" * {0} {1} ({2}, {3})",
                        module.name,
                        module.version,
                        module.download.Host,
                        CkanModule.FmtSize(module.download_size)
                    );
                    downloads.Add(module);
                }
                else
                {
                    User.RaiseMessage(" * {0} {1} (cached)", module.name, module.version);
                }
            }

            bool ok = User.RaiseYesNoDialog("\r\nContinue?");

            if (!ok)
            {
                throw new CancelledActionKraken("User declined install list");
            }

            User.RaiseMessage(String.Empty); // Just to look tidy.

            if (downloads.Count > 0)
            {
                if (downloader == null)
                {
                    downloader = new NetAsyncModulesDownloader(User);
                }

                downloader.DownloadModules(ksp.Cache, downloads);
            }

            // We're about to install all our mods; so begin our transaction.
            using (TransactionScope transaction = CkanTransaction.CreateTransactionScope())
            {
                for (int i = 0; i < modsToInstall.Count; i++)
                {
                    int percent_complete = (i * 100) / modsToInstall.Count;

                    User.RaiseProgress(String.Format("Installing mod \"{0}\"", modsToInstall[i]),
                                         percent_complete);

                    Install(modsToInstall[i]);
                }

                User.RaiseProgress("Updating registry", 70);

                registry_manager.Save(!options.without_enforce_consistency);

                User.RaiseProgress("Committing filesystem changes", 80);

                transaction.Complete();

            }

            // We can scan GameData as a separate transaction. Installing the mods
            // leaves everything consistent, and this is just gravy. (And ScanGameData
            // acts as a Tx, anyway, so we don't need to provide our own.)

            User.RaiseProgress("Rescanning GameData", 90);

            if (!options.without_enforce_consistency)
            {
                ksp.ScanGameData();
            }

            User.RaiseProgress("Done!\r\n", 100);
        }

        public void InstallList(ModuleResolution modules, RelationshipResolverOptions options)
        {
            // We're about to install all our mods; so begin our transaction.
            using (TransactionScope transaction = CkanTransaction.CreateTransactionScope())
            {
                var enumeratedMods = modules.Select((m, i) => new { Idx = i, Module = m });
                foreach (var item in enumeratedMods)
                {
                    var percentComplete = (item.Idx * 100) / modules.Count;
                    User.RaiseProgress(string.Format("Installing mod \"{0}\"", item.Module), percentComplete);
                    Install(item.Module);
                }

                User.RaiseProgress("Updating registry", 70);

                registry_manager.Save(!options.without_enforce_consistency);

                User.RaiseProgress("Committing filesystem changes", 80);

                transaction.Complete();
            }
        }

        /// <summary>
        /// Returns the module contents if and only if we have it
        /// available in our cache. Returns null, otherwise.
        ///
        /// Intended for previews.
        /// </summary>
        // TODO: Return files relative to GameRoot
        public IEnumerable<string> GetModuleContentsList(CkanModule module)
        {
            string filename = ksp.Cache.GetCachedZip(module);

            if (filename == null)
            {
                return null;
            }

            return FindInstallableFiles(module, filename, ksp)
                .Select(x => x.destination);
        }

        /// <summary>
        ///     Install our mod from the filename supplied.
        ///     If no file is supplied, we will check the cache or throw FileNotFoundKraken.
        ///     Does *not* resolve dependencies; this actually does the heavy listing.
        ///     Does *not* save the registry.
        ///     Do *not* call this directly, use InstallList() instead.
        ///
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// Throws a FileNotFoundKraken if we can't find the downloaded module.
        ///
        /// </summary>
        //
        // TODO: The name of this and InstallModule() need to be made more distinctive.

        private void Install(CkanModule module, string filename = null)
        {
            CheckMetapackageInstallationKraken(module);

            Version version = registry_manager.registry.InstalledVersion(module.identifier);

            // TODO: This really should be handled by higher-up code.
            if (version != null)
            {
                User.RaiseMessage("    {0} {1} already installed, skipped", module.identifier, version);
                return;
            }

            // Find our in the cache if we don't already have it.
            filename = filename ?? Cache.GetCachedZip(module);

            // If we *still* don't have a file, then kraken bitterly.
            if (filename == null)
            {
                throw new FileNotFoundKraken(
                    null,
                    String.Format("Trying to install {0}, but it's not downloaded or download is corrupted", module)
                );
            }

            // We'll need our registry to record which files we've installed.
            Registry registry = registry_manager.registry;

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                // Install all the things!
                IEnumerable<string> files = InstallModule(module, filename);

                // Register our module and its files.
                registry.RegisterModule(module, files, ksp);

                // Finish our transaction, but *don't* save the registry; we may be in an
                // intermediate, inconsistent state.
                // This is fine from a transaction standpoint, as we may not have an enclosing
                // transaction, and if we do, they can always roll us back.
                transaction.Complete();
            }

            // Fire our callback that we've installed a module, if we have one.
            if (onReportModInstalled != null)
            {
                onReportModInstalled(module);
            }

        }

        /// <summary>
        /// Check if the given module is a metapackage:
        /// if it is, throws a BadCommandKraken.
        /// </summary>
        private static void CheckMetapackageInstallationKraken(CkanModule module)
        {
            if (module.IsMetapackage)
            {
                throw new BadCommandKraken("Metapackages can not be installed!");
            }
        }

        /// <summary>
        /// Installs the module from the zipfile provided.
        /// Returns a list of files installed.
        /// Propagates a BadMetadataKraken if our install metadata is bad.
        /// Propagates a FileExistsKraken if we were going to overwrite a file.
        /// </summary>
        private IEnumerable<string> InstallModule(CkanModule module, string zip_filename)
        {
            CheckMetapackageInstallationKraken(module);

            using (ZipFile zipfile = new ZipFile(zip_filename))
            {
                IEnumerable<InstallableFile> files = FindInstallableFiles(module, zipfile, ksp);

                try
                {
                    foreach (InstallableFile file in files)
                    {
                        log.DebugFormat("Copying {0}", file.source.Name);
                        CopyZipEntry(zipfile, file.source, file.destination, file.makedir);
                    }
                    log.InfoFormat("Installed {0}", module);
                }
                catch (FileExistsKraken kraken)
                {
                    // Decorate the kraken with our module and re-throw
                    kraken.filename = ksp.ToRelativeGameDir(kraken.filename);
                    kraken.installingModule = module;
                    kraken.owningModule = registry_manager.registry.FileOwner(kraken.filename);
                    throw;
                }

                return files.Select(x => x.destination);
            }
        }

        /// <summary>
        /// Checks the path against a list of reserved game directories
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsReservedDirectory(string path)
        {
            return path == ksp.Tutorial() || path == ksp.ShipsVab()
                    || path == ksp.ShipsSph() || path == ksp.Ships()
                    || path == ksp.Scenarios() || path == ksp.GameData()
                    || path == ksp.GameDir() || path == ksp.CkanDir()
                    || path == ksp.ShipsThumbs() || path == ksp.ShipsThumbsVAB()
                    || path == ksp.ShipsThumbsSPH();
        }

        /// <summary>
        /// Given a stanza and an open zipfile, returns all files that would be installed
        /// for this stanza.
        ///
        /// If a KSP instance is provided, it will be used to generate output paths, otherwise these will be null.
        ///
        /// Throws a BadInstallLocationKraken if the install stanza targets an
        /// unknown install location (eg: not GameData, Ships, etc)
        ///
        /// Throws a BadMetadataKraken if the stanza resulted in no files being returned.
        /// </summary>
        /// <exception cref="BadInstallLocationKraken">Thrown when the installation path is not valid according to the spec.</exception>
        internal static List<InstallableFile> FindInstallableFiles(ModuleInstallDescriptor stanza, ZipFile zipfile, KSP ksp)
        {
            string installDir;
            bool makeDirs;
            var files = new List<InstallableFile>();

            // Normalize the path before doing everything else
            // TODO: This really should happen in the ModuleInstallDescriptor itself.
            stanza.install_to = KSPPathUtils.NormalizePath(stanza.install_to);

            // Convert our stanza to a standard `file` type. This is a no-op if it's
            // already the basic type.

            stanza = stanza.ConvertFindToFile(zipfile);

            if (stanza.install_to == "GameData" || stanza.install_to.StartsWith("GameData/"))
            {
                // The installation path can be either "GameData" or a sub-directory of "GameData"
                // but it cannot contain updirs
                if (stanza.install_to.Contains("/../") || stanza.install_to.EndsWith("/.."))
                    throw new BadInstallLocationKraken("Invalid installation path: " + stanza.install_to);

                string subDir = stanza.install_to.Substring("GameData".Length);    // remove "GameData"
                subDir = subDir.StartsWith("/") ? subDir.Substring(1) : subDir;    // remove a "/" at the beginning, if present

                // Add the extracted subdirectory to the path of KSP's GameData
                installDir = ksp == null ? null : (KSPPathUtils.NormalizePath(ksp.GameData() + "/" + subDir));
                makeDirs = true;
            }
            else if (stanza.install_to.StartsWith("Ships"))
            {
                // Don't allow directory creation in ships directory
                makeDirs = false;

                switch (stanza.install_to)
                {
                    case "Ships":
                        installDir = ksp == null ? null : ksp.Ships();
                        break;
                    case "Ships/VAB":
                        installDir = ksp == null ? null : ksp.ShipsVab();
                        break;
                    case "Ships/SPH":
                        installDir = ksp == null ? null : ksp.ShipsSph();
                        break;
                    case "Ships/@thumbs":
                        installDir = ksp == null ? null : ksp.ShipsThumbs();
                        break;
                    case "Ships/@thumbs/VAB":
                        installDir = ksp == null ? null : ksp.ShipsThumbsVAB();
                        break;
                    case "Ships/@thumbs/SPH":
                        installDir = ksp == null ? null : ksp.ShipsThumbsSPH();
                        break;
                    default:
                        throw new BadInstallLocationKraken("Unknown install_to " + stanza.install_to);
                }
            }
            else
            {
                switch (stanza.install_to)
                {
                    case "Tutorial":
                        installDir = ksp == null ? null : ksp.Tutorial();
                        makeDirs = true;
                        break;

                    case "Scenarios":
                        installDir = ksp == null ? null : ksp.Scenarios();
                        makeDirs = true;
                        break;

                    case "GameRoot":
                        installDir = ksp == null ? null : ksp.GameDir();
                        makeDirs = false;
                        break;

                    default:
                        throw new BadInstallLocationKraken("Unknown install_to " + stanza.install_to);
                }
            }

            // O(N^2) solution, as we're walking the zipfile for each stanza.
            // Surely there's a better way, although this is fast enough we may not care.

            foreach (ZipEntry entry in zipfile)
            {
                // Skips things not prescribed by our install stanza.
                if (!stanza.IsWanted(entry.Name))
                {
                    continue;
                }

                // Prepare our file info.
                InstallableFile file_info = new InstallableFile
                {
                    source = entry,
                    makedir = makeDirs,
                    destination = null
                };

                // If we have a place to install it, fill that in...
                if (installDir != null)
                {
                    // Get the full name of the file.
                    string outputName = entry.Name;

                    // Update our file info with the install location
                    file_info.destination = TransformOutputName(stanza.file, outputName, installDir, stanza.@as);
                }

                files.Add(file_info);
            }

            // If we have no files, then something is wrong! (KSP-CKAN/CKAN#93)
            if (files.Count == 0)
            {
                // We have null as the first argument here, because we don't know which module we're installing
                throw new BadMetadataKraken(null, String.Format("No files found in {0} to install!", stanza.file));
            }

            return files;
        }

        /// <summary>
        /// Transforms the name of the output. This will strip the leading directories from the stanza file from
        /// output name and then combine it with the installDir.
        /// EX: "kOS-1.1/GameData/kOS", "kOS-1.1/GameData/kOS/Plugins/kOS.dll", "GameData" will be transformed
        /// to "GameData/kOS/Plugins/kOS.dll"
        /// </summary>
        /// <returns>The output name.</returns>
        /// <param name="file">The file directive of the stanza.</param>
        /// <param name="outputName">The name of the file to transform.</param>
        /// <param name="installDir">The installation dir where the file should end up with.</param>
        internal static string TransformOutputName(string file, string outputName, string installDir, string @as)
        {
            string leadingPathToRemove = KSPPathUtils.GetLeadingPathElements(file);

            // Special-casing, if stanza.file is just "GameData" or "Ships", strip it.
            // TODO: Do we need to do anything special for tutorials or GameRoot?
            if (
                leadingPathToRemove == string.Empty &&
                (file == "GameData" || file == "Ships")
            )
            {
                leadingPathToRemove = file;

                // It's unclear what the behavior should be in this special case if `as` is specified, therefore
                // disallow it.
                if (!string.IsNullOrWhiteSpace(@as))
                {
                    throw new BadMetadataKraken(null, "Cannot specify `as` if `file` is GameData or Ships.");
                }
            }

            // If there's a leading path to remove, then we have some extra work that
            // needs doing...
            if (leadingPathToRemove != string.Empty)
            {
                string leadingRegEx = "^" + Regex.Escape(leadingPathToRemove) + "/";
                if (!Regex.IsMatch(outputName, leadingRegEx))
                {
                    throw new BadMetadataKraken(null,
                        String.Format("Output file name ({0}) not matching leading path of stanza.file ({1})",
                            outputName, leadingRegEx
                        )
                    );
                }
                // Strip off leading path name
                outputName = Regex.Replace(outputName, leadingRegEx, "");
            }

            // If an `as` is specified, replace the first component in the file path with the value of `as`
            // This works for both when `find` specifies a directory and when it specifies a file.
            if (!string.IsNullOrWhiteSpace(@as))
            {
                if (!@as.Contains("/") && !@as.Contains("\\"))
                {
                    var components = outputName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    components[0] = @as;

                    outputName = string.Join("/", components);
                }
                else
                {
                    throw new BadMetadataKraken(null, "`as` may not include path seperators.");
                }
            }

            // Return our snipped, normalised, and ready to go output filename!
            return KSPPathUtils.NormalizePath(
                Path.Combine(installDir, outputName)
            );
        }

        /// <summary>
        /// Given a module and an open zipfile, return all the files that would be installed
        /// for this module.
        ///
        /// If a KSP instance is provided, it will be used to generate output paths, otherwise these will be null.
        ///
        /// Throws a BadMetadataKraken if the stanza resulted in no files being returned.
        /// </summary>
        public static List<InstallableFile> FindInstallableFiles(CkanModule module, ZipFile zipfile, KSP ksp)
        {
            var files = new List<InstallableFile>();

            try
            {
                // Use the provided stanzas, or use the default install stanza if they're absent.
                if (module.install != null && module.install.Length != 0)
                {
                    foreach (ModuleInstallDescriptor stanza in module.install)
                    {
                        files.AddRange(FindInstallableFiles(stanza, zipfile, ksp));
                    }
                }
                else
                {
                    ModuleInstallDescriptor default_stanza = ModuleInstallDescriptor.DefaultInstallStanza(module.identifier, zipfile);
                    files.AddRange(FindInstallableFiles(default_stanza, zipfile, ksp));
                }
            }
            catch (BadMetadataKraken kraken)
            {
                // Decorate our kraken with the current module, as the lower-level
                // methods won't know it.
                kraken.module = module;
                throw;
            }

            return files;
        }

        /// <summary>
        /// Given a module and a path to a zipfile, returns all the files that would be installed
        /// from that zip for this module.
        ///
        /// This *will* throw an exception if the file does not exist.
        ///
        /// Throws a BadMetadataKraken if the stanza resulted in no files being returned.
        ///
        /// If a KSP instance is provided, it will be used to generate output paths, otherwise these will be null.
        /// </summary>
        // TODO: Document which exception!
        public static List<InstallableFile> FindInstallableFiles(CkanModule module, string zip_filename, KSP ksp)
        {
            // `using` makes sure our zipfile gets closed when we exit this block.
            using (ZipFile zipfile = new ZipFile(zip_filename))
            {
                log.DebugFormat("Searching {0} using {1} as module", zip_filename, module);
                return FindInstallableFiles(module, zipfile, ksp);
            }
        }

        /// <summary>
        /// Copy the entry from the opened zipfile to the path specified.
        /// </summary>
        internal static void CopyZipEntry(ZipFile zipfile, ZipEntry entry, string fullPath, bool makeDirs)
        {
            if (entry.IsDirectory)
            {
                // Skip if we're not making directories for this install.
                if (!makeDirs)
                {
                    log.DebugFormat("Skipping {0}, we don't make directories for this path", fullPath);
                    return;
                }

                log.DebugFormat("Making directory {0}", fullPath);
                file_transaction.CreateDirectory(fullPath);
            }
            else
            {
                log.DebugFormat("Writing file {0}", fullPath);

                // Sometimes there are zipfiles that don't contain entries for the
                // directories their files are in. No, I understand either, but
                // the result is we have to make sure our directories exist, just in case.
                if (makeDirs)
                {
                    string directory = Path.GetDirectoryName(fullPath);
                    file_transaction.CreateDirectory(directory);
                }

                // We don't allow for the overwriting of files. See #208.
                if (File.Exists(fullPath))
                {
                    throw new FileExistsKraken(fullPath, string.Format("Trying to write {0} but it already exists.", fullPath));
                }

                // Snapshot whatever was there before. If there's nothing, this will just
                // remove our file on rollback. We still need this even thought we won't
                // overwite files, as it ensures deletiion on rollback.
                file_transaction.Snapshot(fullPath);

                try
                {
                    // It's a file! Prepare the streams
                    using (Stream zipStream = zipfile.GetInputStream(entry))
                    using (FileStream writer = File.Create(fullPath))
                    {
                        // 4k is the block size on practically every disk and OS.
                        byte[] buffer = new byte[4096];
                        StreamUtils.Copy(zipStream, writer, buffer);
                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    throw new DirectoryNotFoundKraken("", ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Uninstalls all the mods provided, including things which depend upon them.
        /// This *DOES* save the registry.
        /// Preferred over Uninstall.
        /// </summary>
        public void UninstallList(IEnumerable<string> mods)
        {
            // Pre-check, have they even asked for things which are installed?

            foreach (string mod in mods.Where(mod => registry_manager.registry.InstalledModule(mod) == null))
            {
                throw new ModNotInstalledKraken(mod);
            }

            // Find all the things which need uninstalling.
            IEnumerable<string> goners = registry_manager.registry.FindReverseDependencies(mods);

            // If there us nothing to uninstall, skip out.
            if (!goners.Any())
            {
                return;
            }

            User.RaiseMessage("About to remove:\r\n");

            foreach (string mod in goners)
            {
                InstalledModule module = registry_manager.registry.InstalledModule(mod);
                User.RaiseMessage(" * {0} {1}", module.Module.name, module.Module.version);
            }

            bool ok = User.RaiseYesNoDialog("\r\nContinue?");

            if (!ok)
            {
                User.RaiseMessage("Mod removal aborted at user request.");
                return;
            }

            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                foreach (string mod in goners)
                {
                    User.RaiseMessage("Removing {0}...", mod);
                    Uninstall(mod);
                }

                registry_manager.Save();

                transaction.Complete();
            }

            User.RaiseMessage("Done!\r\n");
        }

        public void UninstallList(string mod)
        {
            var list = new List<string> { mod };
            UninstallList(list);
        }

        /// <summary>
        /// Uninstall the module provided. For internal use only.
        /// Use UninstallList for user queries, it also does dependency handling.
        /// This does *NOT* save the registry.
        /// </summary>

        private void Uninstall(string modName)
        {
            using (var transaction = CkanTransaction.CreateTransactionScope())
            {
                InstalledModule mod = registry_manager.registry.InstalledModule(modName);

                if (mod == null)
                {
                    log.ErrorFormat("Trying to uninstall {0} but it's not installed", modName);
                    throw new ModNotInstalledKraken(modName);
                }

                // Walk our registry to find all files for this mod.
                IEnumerable<string> files = mod.Files;

                var directoriesToDelete = new HashSet<string>();

                foreach (string file in files)
                {
                    string path = ksp.ToAbsoluteGameDir(file);

                    try
                    {
                        FileAttributes attr = File.GetAttributes(path);

                        // [This is] bitwise math. Basically, attr is some binary value with one bit meaning
                        // "this is a directory". The bitwise and & operator will return a binary value where
                        // only the bits that are on (1) in both the operands are turned on. In this case
                        // doing a bitwise and operation against attr and the FileAttributes.Directory value
                        // will return the value of FileAttributes.Directory if the Directory file attribute
                        // bit is turned on. See en.wikipedia.org/wiki/Bitwise_operation for a better
                        // explanation. – Kyle Trauberman Aug 30 '12 at 21:28
                        // (https://stackoverflow.com/questions/1395205/better-way-to-check-if-path-is-a-file-or-a-directory)
                        // This is the fastest way to do this test.
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            if (!directoriesToDelete.Contains(path))
                            {
                                directoriesToDelete.Add(path);
                            }
                        }
                        else
                        {
                            // Add this files' directory to the list for deletion if it isn't already there.
                            // Helps clean up directories when modules are uninstalled out of dependency order
                            // Since we check for directory contents when deleting, this should purge empty
                            // dirs, making less ModuleManager headaches for people.
                            var directoryName = Path.GetDirectoryName(path);
                            if (!(directoriesToDelete.Contains(directoryName)))
                            {
                                directoriesToDelete.Add(directoryName);
                            }

                            log.DebugFormat("Removing {0}", file);
                            file_transaction.Delete(path);


                        }
                    }
                    catch (Exception ex)
                    {
                        // XXX: This is terrible, we're catching all exceptions.
                        // We don't consider this problem serious enough to abort and revert,
                        // so treat it as a "--verbose" level log message.
                        log.InfoFormat("Failure in locating file {0} : {1}", path, ex.Message);
                    }
                }

                // Remove from registry.
                registry_manager.registry.DeregisterModule(ksp, modName);

                // Our collection of directories may leave empty parent directories.
                directoriesToDelete = AddParentDirectories(directoriesToDelete);

                // Sort our directories from longest to shortest, to make sure we remove child directories
                // before parents. GH #78.
                foreach (string directory in directoriesToDelete.OrderBy(dir => dir.Length).Reverse())
                {
                    if (!Directory.EnumerateFileSystemEntries(directory).Any())
                    {
                        // It is bad if any of this directories gets removed
                        // So we protect them
                        if (IsReservedDirectory(directory))
                        {
                            continue;
                        }

                        // We *don't* use our file_transaction to delete files here, because
                        // it fails if the system's temp directory is on a different device
                        // to KSP. However we *can* safely delete it now we know it's empty,
                        // because the TxFileMgr *will* put it back if there's a file inside that
                        // needs it.
                        //
                        // This works around GH #251.
                        // The filesystem boundry bug is described in https://transactionalfilemgr.codeplex.com/workitem/20

                        log.DebugFormat("Removing {0}", directory);
                        Directory.Delete(directory);
                    }
                    else
                    {
                        log.InfoFormat("Not removing directory {0}, it's not empty", directory);
                    }
                }
                log.InfoFormat("Removed {0}", modName);
                transaction.Complete();
            }
        }

        /// <summary>
        /// Takes a collection of directories and adds all parent directories within the GameData structure.
        /// </summary>
        /// <param name="directories">The collection of directory path strings to examine</param>
        public HashSet<string> AddParentDirectories(HashSet<string> directories)
        {
            if (directories == null || directories.Count == 0)
            {
                return new HashSet<string>();
            }

            var gameDir = KSPPathUtils.NormalizePath(ksp.GameDir());
            return directories
                .Where(dir => !string.IsNullOrWhiteSpace(dir))
                // normalize all paths before deduplicate
                .Select(KSPPathUtils.NormalizePath)
                // remove any duplicate paths
                .Distinct()
                .SelectMany(dir =>
                {
                    var results = new HashSet<string>();
                    // adding in the DirectorySeparatorChar fixes attempts on Windows
                    // to parse "X:" which resolves to Environment.CurrentDirectory
                    var dirInfo = new DirectoryInfo(dir + Path.DirectorySeparatorChar);

                    // if this is a parentless directory (Windows)
                    // or if the Root equals the current directory (Mono)
                    if (dirInfo.Parent == null || dirInfo.Root == dirInfo)
                    {
                        return results;
                    }

                    if (!dir.StartsWith(gameDir, StringComparison.CurrentCultureIgnoreCase))
                    {
                        dir = KSPPathUtils.ToAbsolute(dir, gameDir);
                    }

                    // remove the system paths, leaving the path under the instance directory
                    var relativeHead = KSPPathUtils.ToRelative(dir, gameDir);
                    var pathArray = relativeHead.Split('/');
                    var builtPath = string.Empty;
                    foreach (var path in pathArray)
                    {
                        builtPath += path + '/';
                        results.Add(KSPPathUtils.ToAbsolute(builtPath, gameDir));
                    }

                    return results;
                })
                .Where(dir => !IsReservedDirectory(dir))
                .ToHashSet();
        }

        #region AddRemove

        /// <summary>
        /// Adds and removes the listed modules as a single transaction.
        /// No relationships will be processed.
        /// This *will* save the registry.
        /// </summary>
        /// <param name="add">Add.</param>
        /// <param name="remove">Remove.</param>
        public void AddRemove(IEnumerable<CkanModule> add = null, IEnumerable<string> remove = null, bool enforceConsistency = true)
        {
            // TODO: We should do a consistency check up-front, rather than relying
            // upon our registry catching inconsistencies at the end.

            using (var tx = CkanTransaction.CreateTransactionScope())
            {

                foreach (string identifier in remove)
                {
                    Uninstall(identifier);
                }

                foreach (CkanModule module in add)
                {
                    Install(module);
                }

                registry_manager.Save(enforceConsistency);

                tx.Complete();
            }
        }

        /// <summary>
        /// Upgrades the mods listed to the latest versions for the user's KSP.
        /// Will *re-install* with warning even if an upgrade is not available.
        /// Throws ModuleNotFoundKraken if module is not installed, or not available.
        /// </summary>
        public void Upgrade(IEnumerable<string> identifiers, IDownloader netAsyncDownloader, bool enforceConsistency = true)
        {
            var options = new RelationshipResolverOptions();

            // We do not wish to pull in any suggested or recommended mods.
            options.with_recommends = false;
            options.with_suggests = false;

            var resolver = new RelationshipResolver(identifiers.ToList(), options, registry_manager.registry, ksp.VersionCriteria());
            Upgrade(resolver.ModList(), netAsyncDownloader, enforceConsistency);
        }

        /// <summary>
        /// Upgrades or installs the mods listed to the specified versions for the user's KSP.
        /// Will *re-install* or *downgrade* (with a warning) as well as upgrade.
        /// Throws ModuleNotFoundKraken if a module is not installed.
        /// </summary>
        public void Upgrade(IEnumerable<CkanModule> modules, IDownloader netAsyncDownloader, bool enforceConsistency = true)
        {
            // Start by making sure we've downloaded everything.
            DownloadModules(modules, netAsyncDownloader);

            // Our upgrade involves removing everything that's currently installed, then
            // adding everything that needs installing (which may involve new mods to
            // satisfy dependencies). We always know the list passed in is what we need to
            // install, but we need to calculate what needs to be removed.
            var to_remove = new List<string>();

            // Let's discover what we need to do with each module!
            foreach (CkanModule module in modules)
            {
                string ident = module.identifier;
                InstalledModule installed_mod = registry_manager.registry.InstalledModule(ident);

                if (installed_mod == null)
                {
                    //Maybe ModuleNotInstalled ?
                    if (registry_manager.registry.IsAutodetected(ident))
                    {
                        throw new ModuleNotFoundKraken(ident, module.version.ToString(), String.Format("Can't upgrade {0} as it was not installed by CKAN. \r\n Please remove manually before trying to install it.", ident));
                    }

                    User.RaiseMessage("Installing previously uninstalled mod {0}", ident);
                }
                else
                {
                    // Module already installed. We'll need to remove it first.
                    to_remove.Add(module.identifier);

                    CkanModule installed = installed_mod.Module;
                    if (installed.version.IsEqualTo(module.version))
                    {
                        log.InfoFormat("{0} is already at the latest version, reinstalling", installed.identifier);
                    }
                    else if (installed.version.IsGreaterThan(module.version))
                    {
                        log.WarnFormat("Downgrading {0} from {1} to {2}", ident, installed.version, module.version);
                    }
                    else
                    {
                        log.InfoFormat("Upgrading {0} to {1}", ident, module.version);
                    }
                }
            }

            AddRemove(
                modules,
                to_remove,
                enforceConsistency
            );
        }

        #endregion

        /// <summary>
        /// Makes sure all the specified mods are downloaded.
        /// </summary>
        private void DownloadModules(IEnumerable<CkanModule> mods, IDownloader downloader)
        {
            List<CkanModule> downloads = mods.Where(module => !ksp.Cache.IsCachedZip(module)).ToList();

            if (downloads.Count > 0)
            {
                downloader.DownloadModules(ksp.Cache, downloads);
            }
        }

        /// <summary>
        /// Import a list of files into the download cache, with progress bar and
        /// interactive prompts for installation and deletion.
        /// </summary>
        /// <param name="files">Set of files to import</param>
        /// <param name="user">Object for user interaction</param>
        /// <param name="installMod">Function to call to mark a mod for installation</param>
        /// <param name="allowDelete">True to ask user whether to delete imported files, false to leave the files as is</param>
        public void ImportFiles(HashSet<FileInfo> files, IUser user, Action<string> installMod, bool allowDelete = true)
        {
            Registry         registry    = registry_manager.registry;
            HashSet<string>  installable = new HashSet<string>();
            List<FileInfo>   deletable   = new List<FileInfo>();
            // Get the mapping of known hashes to modules
            Dictionary<string, List<CkanModule>> index = registry.GetSha1Index();
            int i = 0;
            foreach (FileInfo f in files)
            {
                int percent = i * 100 / files.Count;
                user.RaiseProgress($"Importing {f.Name}... ({percent}%)", percent);
                // Calc SHA-1 sum
                string sha1 = NetModuleCache.GetFileHashSha1(f.FullName);
                // Find SHA-1 sum in registry (potentially multiple)
                if (index.ContainsKey(sha1))
                {
                    deletable.Add(f);
                    List<CkanModule> matches = index[sha1];
                    foreach (CkanModule mod in matches)
                    {
                        if (mod.IsCompatibleKSP(ksp.VersionCriteria()))
                        {
                            installable.Add(mod.identifier);
                        }
                        if (Cache.IsMaybeCachedZip(mod))
                        {
                            user.RaiseMessage("Already cached: {0}", f.Name);
                        }
                        else
                        {
                            user.RaiseMessage($"Importing {mod.identifier} {StripEpoch(mod.version)}...");
                            Cache.Store(mod, f.FullName);
                        }
                    }
                }
                else
                {
                    user.RaiseMessage("Not found in index: {0}", f.Name);
                }
                ++i;
            }
            if (installable.Count > 0 && user.RaiseYesNoDialog($"Install {installable.Count} compatible imported mods in game instance {ksp.Name} ({ksp.GameDir()})?"))
            {
                // Install the imported mods
                foreach (string identifier in installable)
                {
                    installMod(identifier);
                }
            }
            if (allowDelete && deletable.Count > 0 && user.RaiseYesNoDialog($"Import complete. Delete {deletable.Count} old files?"))
            {
                // Delete old files
                foreach (FileInfo f in deletable)
                {
                    f.Delete();
                }
            }
        }

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        /// <param name="version">A version that might contain an epoch</param>
        public static string StripEpoch(Version version)
        {
            return StripEpoch(version.ToString());
        }

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        public static string StripEpoch(string version)
        {
            // If our version number starts with a string of digits, followed by
            // a colon, and then has no more colons, we're probably safe to assume
            // the first string of digits is an epoch
            return epochMatch.IsMatch(version)
                ? epochReplace.Replace(version, @"$2")
                : version;
        }

        /// <summary>
        /// As above, but includes the original in parentheses
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        public static string WithAndWithoutEpoch(string version)
        {
            return epochMatch.IsMatch(version)
                ? $"{epochReplace.Replace(version, @"$2")} ({version})"
                : version;
        }

        private static readonly Regex epochMatch   = new Regex(@"^[0-9][0-9]*:[^:]+$", RegexOptions.Compiled);
        private static readonly Regex epochReplace = new Regex(@"^([^:]+):([^:]+)$",   RegexOptions.Compiled);
    }
}
