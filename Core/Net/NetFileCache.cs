using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Security.Cryptography;
#if NETFRAMEWORK
using System.Security.Permissions;
#endif

using log4net;
using ChinhDo.Transactions.FileManager;

using CKAN.IO;
using CKAN.Extensions;
using CKAN.Versioning;

namespace CKAN
{

    /// <summary>
    /// A local cache dedicated to storing and retrieving files based upon their
    /// URL.
    /// </summary>

    // We require fancy permissions to use the FileSystemWatcher
    // (No longer supported by .NET Core/Standard/5/6/7/etc.)
    #if NETFRAMEWORK
    [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    #endif
    public class NetFileCache : IDisposable
    {
        private readonly FileSystemWatcher watcher;
        // hash => full file path
        private Dictionary<string, string>? cachedFiles;
        private readonly DirectoryInfo cachePath;
        // Files go here while they're downloading
        private readonly DirectoryInfo inProgressPath;
        private readonly GameInstanceManager? manager;
        private static readonly Regex cacheFileRegex = new Regex("^[0-9A-F]{8}-", RegexOptions.Compiled);
        private static readonly ILog log = LogManager.GetLogger(typeof (NetFileCache));

        /// <summary>
        /// Initialize a cache given a GameInstanceManager
        /// </summary>
        /// <param name="mgr">GameInstanceManager object containing the Instances that might have old caches</param>
        /// <param name="path">Location of folder to use for caching</param>
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
            cachePath = new DirectoryInfo(path);
            // Basic validation, our cache has to exist.
            if (!cachePath.Exists)
            {
                throw new DirectoryNotFoundKraken(
                    path,
                    string.Format(Properties.Resources.NetFileCacheCannotFind,
                                  path));
            }
            inProgressPath = new DirectoryInfo(Path.Combine(path, "downloading"));

            // Establish a watch on our cache. This means we can cache the directory contents,
            // and discard that cache if we spot changes.
            watcher = new FileSystemWatcher(cachePath.FullName, "*.zip")
            {
                NotifyFilter = NotifyFilters.LastWrite
                             | NotifyFilters.LastAccess
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
            };

            // If we spot any changes, we fire our event handler.
            // NOTE: FileSystemWatcher.Changed fires when you READ info about a file,
            //       do NOT listen for it!
            watcher.Created += OnCacheChanged;
            watcher.Deleted += OnCacheChanged;
            watcher.Renamed += OnCacheChanged;

            // Enable events!
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="NetFileCache"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="NetFileCache"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="NetFileCache"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the <see cref="NetFileCache"/> so the garbage
        /// collector can reclaim the memory that the <see cref="NetFileCache"/> was occupying.</remarks>
        public void Dispose()
        {
            // All we really need to do is clear our FileSystemWatcher.
            // We disable its event raising capabilities first for good measure.
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            GC.SuppressFinalize(this);
        }

        private FileInfo GetInProgressFileName(string hash, string description)
        {
            inProgressPath.Create();
            return inProgressPath.EnumerateFiles()
                                 .FirstOrDefault(path => path.Name.StartsWith(hash))
                                 // If not found, return the name to create
                                 ?? new FileInfo(Path.Combine(inProgressPath.FullName,
                                                              $"{hash}-{description}"));
        }

        public FileInfo GetInProgressFileName(Uri url, string description)
            => GetInProgressFileName(CreateURLHash(url),
                                     description);

        public FileInfo? GetInProgressFileName(List<Uri> urls, string description)
        {
            var filenames = urls.Select(url => GetInProgressFileName(CreateURLHash(url), description))
                                .Memoize();
            return filenames.FirstOrDefault(fi => fi.Exists)
                ?? filenames.FirstOrDefault();
        }

        /// <summary>
        /// Called from our FileSystemWatcher. Use OnCacheChanged()
        /// without arguments to signal manually.
        /// </summary>
        private void OnCacheChanged(object source, FileSystemEventArgs e)
        {
            log.DebugFormat("File system watcher event {0} fired for {1}",
                            e.ChangeType.ToString(),
                            e.FullPath);
            OnCacheChanged();
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                log.DebugFormat("Purging hashes reactively: {0}", e.FullPath);
                PurgeHashes(null, e.FullPath);
            }
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
        public bool IsCached(Uri url) => GetCachedFilename(url) != null;

        // returns true if a url is already in the cache
        // returns the filename in the outFilename parameter
        public bool IsCached(Uri url, out string? outFilename)
        {
            outFilename = GetCachedFilename(url);
            return outFilename != null;
        }

        /// <summary>
        /// Returns true if a file matching the given URL is cached, but makes no
        /// attempts to check if it's even valid. This is very fast.
        ///
        /// Use IsCachedZip() for a slower but more reliable method.
        /// </summary>
        public bool IsMaybeCachedZip(Uri url, DateTime? remoteTimestamp = null)
            => GetCachedFilename(url, remoteTimestamp) != null;

        /// <summary>>
        /// Returns the filename of an already cached url or null otherwise
        /// </summary>
        /// <param name="url">The URL to check for in the cache</param>
        /// <param name="remoteTimestamp">Timestamp of the remote file, if known; cached files older than this will be considered invalid</param>
        public string? GetCachedFilename(Uri url, DateTime? remoteTimestamp = null)
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

            var files = cachedFiles;

            if (files == null)
            {
                log.Debug("Rebuilding cache index");
                cachedFiles = files = allFiles()
                    .GroupBy(fi => fi.Name[..8])
                    .ToDictionary(grp => grp.Key,
                                  grp => grp.First().FullName);
            }

            // Now that we have a list of files one way or another,
            // check them to see if we can find the one we're looking
            // for.

            var found = scanDirectory(files, hash, remoteTimestamp);
            return string.IsNullOrEmpty(found) ? null : found;
        }

        private string? scanDirectory(Dictionary<string, string> files,
                                      string                     findHash,
                                      DateTime?                  remoteTimestamp = null)
        {
            if (files.TryGetValue(findHash, out string? file)
                && File.Exists(file))
            {
                log.DebugFormat("Found file {0}", file);
                // Check local vs remote timestamps; if local is older, then it's invalid.
                // null means we don't know the remote timestamp (so file is OK)
                if (remoteTimestamp == null
                    || remoteTimestamp < File.GetLastWriteTimeUtc(file))
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
                    PurgeHashes(null, file);
                }
            }
            else
            {
                log.DebugFormat("{0} not in cache", findHash);
            }
            return null;
        }

        /// <summary>
        /// Count the files and bytes in the cache
        /// </summary>
        /// <param name="numFiles">Output parameter set to number of files in cache</param>
        /// <param name="numBytes">Output parameter set to number of bytes in cache</param>
        /// <param name="bytesFree">Output parameter set to number of bytes free</param>
        public void GetSizeInfo(out int numFiles, out long numBytes, out long? bytesFree)
        {
            bytesFree = cachePath.GetDrive()?.AvailableFreeSpace;
            (numFiles, numBytes) = Enumerable.Repeat(cachePath, 1)
                                             .Concat(legacyDirs())
                                             .Select(GetDirSizeInfo)
                                             .Aggregate((numFiles: 0,
                                                         numBytes: 0L),
                                                        (total, next) => (numFiles: total.numFiles + next.numFiles,
                                                                          numBytes: total.numBytes + next.numBytes));
        }

        private static (int numFiles, long numBytes) GetDirSizeInfo(DirectoryInfo cacheDir)
            => cacheDir.EnumerateFiles("*", SearchOption.AllDirectories)
                       .Aggregate((numFiles: 0,
                                   numBytes: 0L),
                                  (tuple, fi) => (numFiles: tuple.numFiles + 1,
                                                  numBytes: tuple.numBytes + fi.Length));

        public void CheckFreeSpace(long bytesToStore)
        {
            CKANPathUtils.CheckFreeSpace(cachePath,
                                         bytesToStore,
                                         Properties.Resources.NotEnoughSpaceToCache);
        }

        private IEnumerable<DirectoryInfo> legacyDirs()
            => manager?.Instances.Values
                       .Where(ksp => ksp.Valid)
                       .Select(ksp => new DirectoryInfo(ksp.DownloadCacheDir()))
                       .Where(dir => dir.Exists)
                      ?? Enumerable.Empty<DirectoryInfo>();

        public void EnforceSizeLimit(long bytes, Registry registry)
        {
            GetSizeInfo(out int numFiles, out long curBytes, out _);
            if (curBytes > bytes)
            {
                // This object will let us determine whether a module is compatible with any of our instances
                var aggregateCriteria = manager?.Instances.Values
                    .Where(ksp => ksp.Valid)
                    .Select(ksp => ksp.VersionCriteria())
                    .Aggregate(
                        manager?.CurrentInstance?.VersionCriteria()
                            ?? new GameVersionCriteria(null),
                        (combinedCrit, nextCrit) => combinedCrit.Union(nextCrit))
                    ?? new GameVersionCriteria(null);

                // This object lets us find the modules associated with a cached file
                var hashMap = registry.GetDownloadUrlHashIndex();

                // Prune the module lists to only those that are compatible
                foreach (var kvp in hashMap)
                {
                    kvp.Value.RemoveAll(mod => !mod.IsCompatible(aggregateCriteria));
                }

                // Now get all the files in all the caches, including in progress...
                List<FileInfo> files = allFiles(true);
                // ... and sort them by compatibility and timestamp...
                files.Sort((a, b) => compareFiles(hashMap, a, b));

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

        private static int compareFiles(IReadOnlyDictionary<string, List<CkanModule>> hashMap, FileInfo a, FileInfo b)
        {
            // Compatible modules for file A
            hashMap.TryGetValue(a.Name[..8], out List<CkanModule>? modulesA);
            bool compatA = modulesA?.Any() ?? false;

            // Compatible modules for file B
            hashMap.TryGetValue(b.Name[..8], out List<CkanModule>? modulesB);
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

        private List<FileInfo> allFiles(bool includeInProgress = false)
        {
            var files = cachePath.EnumerateFiles("*",
                                                 includeInProgress ? SearchOption.AllDirectories
                                                                   : SearchOption.TopDirectoryOnly);
            foreach (var legacyDir in legacyDirs())
            {
                files = files.Union(legacyDir.EnumerateFiles());
            }
            return files.Where(fi =>
                    // Require 8 digit hex prefix followed by dash; any else was not put there by CKAN
                    cacheFileRegex.IsMatch(fi.Name)
                    // Treat the hash files as companions of the main files, not their own entries
                    && !fi.Name.EndsWith(".sha1") && !fi.Name.EndsWith(".sha256")
                ).ToList();
        }

        public IEnumerable<(string hash, long size)> CachedHashesAndSizes()
            => allFiles(false).Select(fi => (hash: fi.Name[..8],
                                             size: fi.Length));

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
        public string Store(Uri     url,
                            string  path,
                            string? description = null,
                            bool    move        = false)
        {
            log.DebugFormat("Storing {0}", url);

            TxFileManager tx_file = new TxFileManager();

            // Clear our cache entry first
            Remove(url);

            string hash = CreateURLHash(url);

            description ??= Path.GetFileName(path);

            Debug.Assert(
                Regex.IsMatch(description, "^[A-Za-z0-9_.-]*$"),
                $"description {description} isn't as filesystem safe as we thought... (#1266)");

            string fullName = string.Format("{0}-{1}", hash, Path.GetFileName(description));
            string targetPath = Path.Combine(cachePath.FullName, fullName);

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
            if (GetCachedFilename(url) is string file
                && File.Exists(file))
            {
                TxFileManager tx_file = new TxFileManager();
                tx_file.Delete(file);
                // We've changed our cache, so signal that immediately.
                cachedFiles?.Remove(CreateURLHash(url));
                PurgeHashes(tx_file, file);
                return true;
            }
            return false;
        }

        public bool Remove(IEnumerable<Uri> urls)
            => urls.Select(Remove)
                   // Force all elements to be evaluated
                   .ToArray()
                   .Any(found => found);

        private void PurgeHashes(TxFileManager? tx_file, string file)
        {
            try
            {
                sha1Cache.TryRemove(file, out _);
                sha256Cache.TryRemove(file, out _);

                tx_file ??= new TxFileManager();
                tx_file.Delete($"{file}.sha1");
                tx_file.Delete($"{file}.sha256");
            }
            catch
            {
            }
        }

        /// <summary>
        /// Clear all files in cache, including main directory and legacy directories
        /// </summary>
        public void RemoveAll()
        {
            var dirs = Enumerable.Repeat(cachePath, 1)
                .Concat(Enumerable.Repeat(inProgressPath, 1))
                .Concat(legacyDirs());
            foreach (var dir in dirs)
            {
                foreach (var file in dir.EnumerateFiles())
                {
                    try
                    {
                        file.Delete();
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
        /// <param name="percentProgress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        public void MoveFrom(DirectoryInfo  fromDir,
                             IProgress<int> percentProgress)
        {
            if (fromDir.Exists && !cachePath.PathEquals(fromDir))
            {
                var files = fromDir.GetFiles("*", SearchOption.AllDirectories);
                var bytesProgress = new ProgressScalePercentsByFileSizes(
                                        percentProgress,
                                        files.Select(f => f.Length));
                bool hasAny = false;
                foreach (var fromFile in files)
                {
                    bytesProgress.Report(0);

                    var toFile = Path.Combine(cachePath.FullName,
                                              CKANPathUtils.ToRelative(fromFile.FullName,
                                                                       fromDir.FullName));
                    if (File.Exists(toFile))
                    {
                        if (fromFile.CreationTimeUtc == File.GetCreationTimeUtc(toFile))
                        {
                            // Same filename with same timestamp, almost certainly the same
                            // actual file on disk via different paths thanks to symlinks.
                            // Skip this whole folder!
                            break;
                        }
                        else
                        {
                            // Don't need multiple copies of the same file
                            fromFile.Delete();
                        }
                    }
                    else
                    {
                        try
                        {
                            if (Path.GetDirectoryName(toFile) is string parent)
                            {
                                Directory.CreateDirectory(parent);
                            }
                            fromFile.MoveTo(toFile);
                            hasAny = true;
                        }
                        catch (Exception exc)
                        {
                            // On Windows, FileInfo.MoveTo sometimes throws exceptions after it succeeds (!!).
                            // Just log it and ignore.
                            log.ErrorFormat("Couldn't move {0} to {1}: {2}",
                                            fromFile.FullName,
                                            toFile,
                                            exc.Message);
                        }
                    }
                    bytesProgress.Report(100);
                    bytesProgress.NextFile();
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
        public static string CreateURLHash(Uri? url)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(url?.ToString() ?? ""));

                return BitConverter.ToString(hash).Replace("-", "")[..8];
            }
        }

        /// <summary>
        /// Calculate the SHA1 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <param name="cancelToken">Cancellation token to cancel the operation</param>
        /// <returns>
        /// SHA1 hash, in all-caps hexadecimal format
        /// </returns>
        public string GetFileHashSha1(string             filePath,
                                      IProgress<int>?    progress,
                                      CancellationToken? cancelToken = default)
            => GetFileHash(filePath, "sha1", sha1Cache, SHA1.Create, progress, cancelToken);

        /// <summary>
        /// Calculate the SHA256 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <param name="cancelToken">Cancellation token to cancel the operation</param>
        /// <returns>
        /// SHA256 hash, in all-caps hexadecimal format
        /// </returns>
        public string GetFileHashSha256(string             filePath,
                                        IProgress<int>?    progress,
                                        CancellationToken? cancelToken = default)
            => GetFileHash(filePath, "sha256", sha256Cache, SHA256.Create, progress, cancelToken);

        /// <summary>
        /// Calculate the hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <param name="hashSuffix">Suffix to use for the hash file</param>
        /// <param name="cache">Cache to use for storing the hash</param>
        /// <param name="getHashAlgo">Function to get the hash algorithm</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <param name="cancelToken">Cancellation token to cancel the operation</param>
        /// <returns>
        /// Hash, in all-caps hexadecimal format
        /// </returns>
        private string GetFileHash(string                               filePath,
                                   string                               hashSuffix,
                                   ConcurrentDictionary<string, string> cache,
                                   Func<HashAlgorithm>                  getHashAlgo,
                                   IProgress<int>?                      progress,
                                   CancellationToken?                   cancelToken)
            => cache.GetOrAdd(filePath, p =>
               {
                   var hashFile = $"{p}.{hashSuffix}";
                   if (File.Exists(hashFile))
                   {
                       return File.ReadAllText(hashFile);
                   }
                   else
                   {
                       using (var fs     = new FileStream(p, FileMode.Open, FileAccess.Read))
                       using (var bs     = new BufferedStream(fs))
                       using (var hasher = getHashAlgo())
                       {
                           var hash = BitConverter.ToString(hasher.ComputeHash(bs, progress, cancelToken))
                                                  .Replace("-", "");
                           if (Path.GetDirectoryName(hashFile) == cachePath.FullName)
                           {
                               hash.WriteThroughTo(hashFile);
                           }
                           return hash;
                       }
                   }
               });

        private readonly ConcurrentDictionary<string, string> sha1Cache   = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> sha256Cache = new ConcurrentDictionary<string, string>();
    }
}
