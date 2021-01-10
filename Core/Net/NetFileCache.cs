using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Security.Permissions;
using System.Security.Cryptography;

using log4net;
using ChinhDo.Transactions.FileManager;
using ICSharpCode.SharpZipLib.Zip;

using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{

    /// <summary>
    /// A local cache dedicated to storing and retrieving files based upon their
    /// URL.
    /// </summary>

    // We require fancy permissions to use the FileSystemWatcher
    [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public class NetFileCache : IDisposable
    {
        private FileSystemWatcher watcher;
        // hash => full file path
        private Dictionary<string, string> cachedFiles;
        private string cachePath;
        private GameInstanceManager manager;
        private static readonly Regex cacheFileRegex = new Regex("^[0-9A-F]{8}-", RegexOptions.Compiled);
        private static readonly ILog log = LogManager.GetLogger(typeof (NetFileCache));

        /// <summary>
        /// Initialize a cache given a GameInstanceManager
        /// </summary>
        /// <param name="mgr">GameInstanceManager object containing the Instances that might have old caches</param>
        public NetFileCache(GameInstanceManager mgr, string path)
            : this(path)
        {
            manager = mgr;
        }

        /// <summary>
        /// Initialize a cache given a path
        /// </summary>
        /// <param name="path">Location of folder to use for caching</param>
        public NetFileCache(string path)
        {
            cachePath = path;

            // Basic validation, our cache has to exist.
            if (!Directory.Exists(cachePath))
            {
                throw new DirectoryNotFoundKraken(cachePath, $"Cannot find cache directory: {cachePath}");
            }

            // Establish a watch on our cache. This means we can cache the directory contents,
            // and discard that cache if we spot changes.
            watcher = new FileSystemWatcher(cachePath, "*.zip")
            {
                NotifyFilter = NotifyFilters.LastWrite
                             | NotifyFilters.LastAccess
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
            };

            // If we spot any changes, we fire our event handler.
            // NOTE: FileSystemWatcher.Changed fires when you READ info about a file,
            //       do NOT listen for it!
            watcher.Created += new FileSystemEventHandler(OnCacheChanged);
            watcher.Deleted += new FileSystemEventHandler(OnCacheChanged);
            watcher.Renamed += new RenamedEventHandler(OnCacheChanged);

            // Enable events!
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CKAN.NetFileCache"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CKAN.NetFileCache"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="CKAN.NetFileCache"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="CKAN.NetFileCache"/> so the garbage
        /// collector can reclaim the memory that the <see cref="CKAN.NetFileCache"/> was occupying.</remarks>
        public void Dispose()
        {
            // All we really need to do is clear our FileSystemWatcher.
            // We disable its event raising capabilities first for good measure.
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        /// <summary>
        /// Called from our FileSystemWatcher. Use OnCacheChanged()
        /// without arguments to signal manually.
        /// </summary>
        private void OnCacheChanged(object source, FileSystemEventArgs e)
        {
            log.Debug("File system watcher event fired");
            OnCacheChanged();
        }

        /// <summary>
        /// When our cache dirctory changes, we just clear the list of
        /// files we know about.
        /// </summary>
        public void OnCacheChanged()
        {
            log.Debug("Purging cache index");
            cachedFiles = null;
        }

        // returns true if a url is already in the cache
        public bool IsCached(Uri url)
        {
            return GetCachedFilename(url) != null;
        }

        // returns true if a url is already in the cache
        // returns the filename in the outFilename parameter
        public bool IsCached(Uri url, out string outFilename)
        {
            outFilename = GetCachedFilename(url);

            return outFilename != null;
        }

        /// <summary>
        /// Returns true if our given URL is cached, *and* it passes zip
        /// validation tests. Prefer this over IsCached when working with
        /// zip files.
        /// </summary>
        public bool IsCachedZip(Uri url)
        {
            return GetCachedZip(url) != null;
        }

        /// <summary>
        /// Returns true if a file matching the given URL is cached, but makes no
        /// attempts to check if it's even valid. This is very fast.
        ///
        /// Use IsCachedZip() for a slower but more reliable method.
        /// </summary>
        public bool IsMaybeCachedZip(Uri url, DateTime? remoteTimestamp = null)
        {
            return GetCachedFilename(url, remoteTimestamp) != null;
        }

        /// <summary>>
        /// Returns the filename of an already cached url or null otherwise
        /// </summary>
        /// <param name="url">The URL to check for in the cache</param>
        /// <param name="remoteTimestamp">Timestamp of the remote file, if known; cached files older than this will be considered invalid</param>
        public string GetCachedFilename(Uri url, DateTime? remoteTimestamp = null)
        {
            log.DebugFormat("Checking cache for {0}", url);

            if (url == null)
            {
                return null;
            }

            string hash = CreateURLHash(url);

            // Use our existing list of files, or retrieve and
            // store the list of files in our cache. Note that
            // we copy cachedFiles into our own variable as it
            // *may* get cleared by OnCacheChanged while we're
            // using it.

            Dictionary<string, string> files = cachedFiles;

            if (files == null)
            {
                log.Debug("Rebuilding cache index");
                cachedFiles = files = allFiles()
                    .GroupBy(fi => fi.Name.Substring(0, 8))
                    .ToDictionary(
                        grp => grp.Key,
                        grp => grp.First().FullName
                    );
            }

            // Now that we have a list of files one way or another,
            // check them to see if we can find the one we're looking
            // for.

            string found = scanDirectory(files, hash, remoteTimestamp);
            if (!string.IsNullOrEmpty(found))
            {
                return found;
            }

            return null;
        }

        private string scanDirectory(Dictionary<string, string> files, string findHash, DateTime? remoteTimestamp = null)
        {
            string file;
            if (files.TryGetValue(findHash, out file))
            {
                log.DebugFormat("Found file {0}", file);
                // Check local vs remote timestamps; if local is older, then it's invalid.
                // null means we don't know the remote timestamp (so file is OK)
                if (remoteTimestamp == null
                    || remoteTimestamp < File.GetLastWriteTime(file).ToUniversalTime())
                {
                    // File not too old, use it
                    log.Debug("Found good file, using it");
                    return file;
                }
                else
                {
                    // Local file too old, delete it
                    log.Debug("Found stale file, deleting it");
                    File.Delete(file);
                    File.Delete($"{file}.sha1");
                    File.Delete($"{file}.sha256");
                    sha1Cache.Remove(file);
                    sha256Cache.Remove(file);
                }
            }
            else
            {
                log.DebugFormat("{0} not in cache", findHash);
            }
            return null;
        }

        /// <summary>
        /// Returns the filename for a cached URL, if and only if it
        /// passes zipfile validation tests. Prefer this to GetCachedFilename
        /// when working with zip files. Returns null if not available, or
        /// validation failed.
        ///
        /// Low level CRC (cyclic redundancy check) checks will be done.
        /// This can take time on order of seconds for larger zip files.
        /// </summary>
        public string GetCachedZip(Uri url)
        {
            string filename = GetCachedFilename(url);
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }
            else
            {
                string invalidReason;
                if (ZipValid(filename, out invalidReason))
                {
                    return filename;
                }
                else
                {
                    // Purge invalid cache entries
                    File.Delete(filename);
                    return null;
                }
            }
        }

        /// <summary>
        /// Count the files and bytes in the cache
        /// </summary>
        /// <param name="numFiles">Output parameter set to number of files in cache</param>
        /// <param name="numBytes">Output parameter set to number of bytes in cache</param>
        public void GetSizeInfo(out int numFiles, out long numBytes)
        {
            numFiles = 0;
            numBytes = 0;
            GetSizeInfo(cachePath, ref numFiles, ref numBytes);
            foreach (var legacyDir in legacyDirs())
            {
                GetSizeInfo(legacyDir, ref numFiles, ref numBytes);
            }
        }

        private void GetSizeInfo(string path, ref int numFiles, ref long numBytes)
        {
            DirectoryInfo cacheDir = new DirectoryInfo(path);
            foreach (var file in cacheDir.EnumerateFiles())
            {
                ++numFiles;
                numBytes += file.Length;
            }
        }

        private HashSet<string> legacyDirs()
        {
            return manager?.Instances.Values
                .Where(ksp => ksp.Valid)
                .Select(ksp => ksp.DownloadCacheDir())
                .Where(dir => Directory.Exists(dir))
                .ToHashSet()
                ?? new HashSet<string>();
        }

        public void EnforceSizeLimit(long bytes, Registry registry)
        {
            int numFiles;
            long curBytes;
            GetSizeInfo(out numFiles, out curBytes);
            if (curBytes > bytes)
            {
                // This object will let us determine whether a module is compatible with any of our instances
                GameVersionCriteria aggregateCriteria = manager?.Instances.Values
                    .Where(ksp => ksp.Valid)
                    .Select(ksp => ksp.VersionCriteria())
                    .Aggregate(
                        manager?.CurrentInstance?.VersionCriteria()
                            ?? new GameVersionCriteria(null),
                        (combinedCrit, nextCrit) => combinedCrit.Union(nextCrit)
                    );

                // This object lets us find the modules associated with a cached file
                Dictionary<string, List<CkanModule>> hashMap = registry.GetDownloadHashIndex();

                // Prune the module lists to only those that are compatible
                foreach (var kvp in hashMap)
                {
                    kvp.Value.RemoveAll(mod => !mod.IsCompatibleKSP(aggregateCriteria));
                }

                // Now get all the files in all the caches...
                List<FileInfo> files = allFiles();
                // ... and sort them by compatibilty and timestamp...
                files.Sort((a, b) => compareFiles(
                    hashMap, aggregateCriteria, a, b
                ));

                // ... and delete them till we're under the limit
                foreach (FileInfo fi in files)
                {
                    curBytes -= fi.Length;
                    fi.Delete();
                    File.Delete($"{fi.Name}.sha1");
                    File.Delete($"{fi.Name}.sha256");
                    if (curBytes <= bytes)
                    {
                        // Limit met, all done!
                        break;
                    }
                }
                OnCacheChanged();
                sha1Cache.Clear();
                sha256Cache.Clear();
            }
        }

        private int compareFiles(Dictionary<string, List<CkanModule>> hashMap, GameVersionCriteria crit, FileInfo a, FileInfo b)
        {
            // Compatible modules for file A
            List<CkanModule> modulesA;
            hashMap.TryGetValue(a.Name.Substring(0, 8), out modulesA);
            bool compatA = modulesA?.Any() ?? false;

            // Compatible modules for file B
            List<CkanModule> modulesB;
            hashMap.TryGetValue(b.Name.Substring(0, 8), out modulesB);
            bool compatB = modulesB?.Any() ?? false;

            if (modulesA == null && modulesB != null)
            {
                // A isn't indexed but B is, delete A first
                return -1;
            }
            else if (modulesA != null && modulesB == null)
            {
                // A is indexed but B isn't, delete B first
                return 1;
            }
            else if (!compatA && compatB)
            {
                // A isn't compatible but B is, delete A first
                return -1;
            }
            else if (compatA && !compatB)
            {
                // A is compatible but B isn't, delete B first
                return 1;
            }
            else
            {
                // Both are either compatible or incompatible
                // Go by file age, oldest first
                return (int)(a.CreationTime - b.CreationTime).TotalSeconds;
            }
        }

        private List<FileInfo> allFiles()
        {
            DirectoryInfo mainDir = new DirectoryInfo(cachePath);
            var files = mainDir.EnumerateFiles();
            foreach (string legacyDir in legacyDirs())
            {
                DirectoryInfo legDir = new DirectoryInfo(legacyDir);
                files = files.Union(legDir.EnumerateFiles());
            }
            return files.Where(fi =>
                    // Require 8 digit hex prefix followed by dash; any else was not put there by CKAN
                    cacheFileRegex.IsMatch(fi.Name)
                    // Treat the hash files as companions of the main files, not their own entries
                    && !fi.Name.EndsWith(".sha1") && !fi.Name.EndsWith(".sha256")
                ).ToList();
        }

        /// <summary>
        /// Check whether a ZIP file is valid
        /// </summary>
        /// <param name="filename">Path to zip file to check</param>
        /// <param name="invalidReason">Description of problem with the file</param>
        /// <returns>
        /// True if valid, false otherwise. See invalidReason param for explanation.
        /// </returns>
        public static bool ZipValid(string filename, out string invalidReason)
        {
            try
            {
                if (filename != null)
                {
                    using (ZipFile zip = new ZipFile(filename))
                    {
                        string zipErr = null;
                        // Perform CRC and other checks
                        if (zip.TestArchive(true, TestStrategy.FindFirstError,
                            (TestStatus st, string msg) =>
                            {
                                // This delegate is called as TestArchive proceeds through its
                                // steps, both routine and abnormal.
                                // The second parameter is non-null if an error occurred.
                                if (st != null && !st.EntryValid && !string.IsNullOrEmpty(msg))
                                {
                                    // Capture the error string so we can return it
                                    zipErr = $"Error in step {st.Operation} for {st.Entry?.Name}: {msg}";
                                }
                            }))
                        {
                            invalidReason = "";
                            return true;
                        }
                        else
                        {
                            invalidReason = zipErr ?? "ZipFile.TestArchive(true) returned false";
                            return false;
                        }
                    }
                }
                else
                {
                    invalidReason = "Null file name";
                    return false;
                }
            }
            catch (ZipException ze)
            {
                // Save the errors someplace useful
                invalidReason = ze.Message;
                return false;
            }
            catch (ArgumentException ex)
            {
                invalidReason = ex.Message;
                return false;
            }
            catch (NotSupportedException nse) when (Platform.IsMono)
            {
                // SharpZipLib throws this if your locale isn't installed on Mono
                invalidReason = $"{nse.Message}\r\n\r\nInstall the `mono-complete` package or equivalent for your operating system.";
                return false;
            }
        }

        /// <summary>
        /// Stores the results of a given URL in the cache.
        /// Description is adjusted to be filesystem-safe and then appended to the file hash when saving.
        /// If not present, the filename will be used.
        /// If `move` is true, then the file will be moved; otherwise, it will be copied.
        ///
        /// Returns a path to the newly cached file.
        ///
        /// This method is filesystem transaction aware.
        /// </summary>
        public string Store(Uri url, string path, string description = null, bool move = false)
        {
            log.DebugFormat("Storing {0}", url);

            TxFileManager tx_file = new TxFileManager();

            // Make sure we clear our cache entry first.
            Remove(url);

            string hash = CreateURLHash(url);

            description = description ?? Path.GetFileName(path);

            Debug.Assert(
                Regex.IsMatch(description, "^[A-Za-z0-9_.-]*$"),
                "description isn't as filesystem safe as we thought... (#1266)"
            );

            string fullName = String.Format("{0}-{1}", hash, Path.GetFileName(description));
            string targetPath = Path.Combine(cachePath, fullName);

            // Purge hashes associated with the new file
            PurgeHashes(tx_file, targetPath);

            log.InfoFormat("Storing {0} in {1}", path, targetPath);

            if (move)
            {
                tx_file.Move(path, targetPath);
            }
            else
            {
                tx_file.Copy(path, targetPath, true);
            }

            // We've changed our cache, so signal that immediately.
            if (!cachedFiles?.ContainsKey(hash) ?? false)
            {
                cachedFiles?.Add(hash, targetPath);
            }

            return targetPath;
        }

        /// <summary>
        /// Removes the given URL from the cache.
        /// Returns true if any work was done, false otherwise.
        /// This method is filesystem transaction aware.
        /// </summary>
        public bool Remove(Uri url)
        {
            TxFileManager tx_file = new TxFileManager();

            string file = GetCachedFilename(url);

            if (file != null)
            {
                tx_file.Delete(file);
                // We've changed our cache, so signal that immediately.
                cachedFiles?.Remove(CreateURLHash(url));
                PurgeHashes(tx_file, file);
                return true;
            }

            return false;
        }

        private void PurgeHashes(TxFileManager tx_file, string file)
        {
            tx_file.Delete($"{file}.sha1");
            tx_file.Delete($"{file}.sha256");

            sha1Cache.Remove(file);
            sha256Cache.Remove(file);
        }

        /// <summary>
        /// Clear all files in cache, including main directory and legacy directories
        /// </summary>
        public void RemoveAll()
        {
            foreach (string file in Directory.EnumerateFiles(cachePath))
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }
            foreach (string dir in legacyDirs())
            {
                foreach (string file in Directory.EnumerateFiles(dir))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
            OnCacheChanged();
            sha1Cache.Clear();
            sha256Cache.Clear();
        }

        /// <summary>
        /// Move files from another folder into this cache
        /// May throw an IOException if disk is full!
        /// </summary>
        /// <param name="fromDir">Path from which to move files</param>
        public void MoveFrom(string fromDir)
        {
            if (cachePath != fromDir && Directory.Exists(fromDir))
            {
                bool hasAny = false;
                foreach (string fromFile in Directory.EnumerateFiles(fromDir))
                {
                    string toFile = Path.Combine(cachePath, Path.GetFileName(fromFile));
                    if (File.Exists(toFile))
                    {
                        if (File.GetCreationTime(fromFile) == File.GetCreationTime(toFile))
                        {
                            // Same filename with same timestamp, almost certainly the same
                            // actual file on disk via different paths thanks to symlinks.
                            // Skip this whole folder!
                            break;
                        }
                        else
                        {
                            // Don't need multiple copies of the same file
                            File.Delete(fromFile);
                        }
                    }
                    else
                    {
                        File.Move(fromFile, toFile);
                        hasAny = true;
                    }
                }
                if (hasAny)
                {
                    OnCacheChanged();
                    sha1Cache.Clear();
                    sha256Cache.Clear();
                }
            }
        }

        /// <summary>
        /// Generate the hash used for caching
        /// </summary>
        /// <param name="url">URL to hash</param>
        /// <returns>
        /// Returns the 8-byte hash for a given url
        /// </returns>
        public static string CreateURLHash(Uri url)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(url.ToString()));

                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 8);
            }
        }

        /// <summary>
        /// Calculate the SHA1 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <returns>
        /// SHA1 hash, in all-caps hexadecimal format
        /// </returns>
        public string GetFileHashSha1(string filePath)
        {
            string hash = null;
            string hashFile = $"{filePath}.sha1";
            if (sha1Cache.TryGetValue(filePath, out hash))
            {
                return hash;
            }
            else if (File.Exists(hashFile))
            {
                hash = File.ReadAllText(hashFile);
                sha1Cache.Add(filePath, hash);
                return hash;
            }
            else
            {
                using (FileStream     fs   = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (BufferedStream bs   = new BufferedStream(fs))
                using (SHA1           sha1 = new SHA1CryptoServiceProvider())
                {
                    hash = BitConverter.ToString(sha1.ComputeHash(bs)).Replace("-", "");
                    sha1Cache.Add(filePath, hash);
                    if (Path.GetDirectoryName(hashFile) == Path.GetFullPath(cachePath))
                    {
                        File.WriteAllText(hashFile, hash);
                    }
                    return hash;
                }
            }
        }

        /// <summary>
        /// Calculate the SHA256 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <returns>
        /// SHA256 hash, in all-caps hexadecimal format
        /// </returns>
        public string GetFileHashSha256(string filePath)
        {
            string hash = null;
            string hashFile = $"{filePath}.sha256";
            if (sha256Cache.TryGetValue(filePath, out hash))
            {
                return hash;
            }
            else if (File.Exists(hashFile))
            {
                hash = File.ReadAllText(hashFile);
                sha256Cache.Add(filePath, hash);
                return hash;
            }
            else
            {
                using (FileStream     fs     = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (BufferedStream bs     = new BufferedStream(fs))
                using (SHA256Managed  sha256 = new SHA256Managed())
                {
                    hash = BitConverter.ToString(sha256.ComputeHash(bs)).Replace("-", "");
                    sha256Cache.Add(filePath, hash);
                    if (Path.GetDirectoryName(hashFile) == Path.GetFullPath(cachePath))
                    {
                        File.WriteAllText(hashFile, hash);
                    }
                    return hash;
                }
            }
        }

        private Dictionary<string, string> sha1Cache   = new Dictionary<string, string>();
        private Dictionary<string, string> sha256Cache = new Dictionary<string, string>();
    }
}
