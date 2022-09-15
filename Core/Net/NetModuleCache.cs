using System;
using System.IO;
using System.Security.Cryptography;

namespace CKAN
{
    /// <summary>
    /// A cache object that protects the validity of the files it contains.
    /// A CkanModule must be provided for each file added, and the following
    /// properties are checked before adding:
    ///   - CkanModule.download_size
    ///   - CkanModule.download_hash.sha1
    ///   - CkanModule.download_hash.sha256
    /// </summary>
    public class NetModuleCache : IDisposable
    {

        /// <summary>
        /// Initialize the cache
        /// </summary>
        /// <param name="mgr">GameInstanceManager containing instances that might have legacy caches</param>
        public NetModuleCache(GameInstanceManager mgr, string path)
        {
            cache = new NetFileCache(mgr, path);
        }

        /// <summary>
        /// Initialize the cache
        /// </summary>
        /// <param name="path">Path to directory to use as the cache</param>
        public NetModuleCache(string path)
        {
            cache = new NetFileCache(path);
        }

        // Simple passthrough wrappers
        public void Dispose()
        {
            cache.Dispose();
        }
        public void RemoveAll()
        {
            cache.RemoveAll();
        }
        public void MoveFrom(string fromDir)
        {
            cache.MoveFrom(fromDir);
        }
        public bool IsCached(CkanModule m)
        {
            return cache.IsCached(m.download);
        }
        public bool IsCached(CkanModule m, out string outFilename)
        {
            return cache.IsCached(m.download, out outFilename);
        }
        public bool IsCachedZip(CkanModule m)
        {
            return cache.IsCachedZip(m.download);
        }
        public bool IsMaybeCachedZip(CkanModule m)
        {
            return cache.IsMaybeCachedZip(m.download, m.release_date);
        }
        public string GetCachedFilename(CkanModule m)
        {
            return cache.GetCachedFilename(m.download, m.release_date);
        }
        public string GetCachedZip(CkanModule m)
        {
            return cache.GetCachedZip(m.download);
        }
        public void GetSizeInfo(out int numFiles, out long numBytes, out long bytesFree)
        {
            cache.GetSizeInfo(out numFiles, out numBytes, out bytesFree);
        }
        public void EnforceSizeLimit(long bytes, Registry registry)
        {
            cache.EnforceSizeLimit(bytes, registry);
        }
        public void CheckFreeSpace(long bytesToStore)
        {
            cache.CheckFreeSpace(bytesToStore);
        }

        public string GetInProgressFileName(CkanModule m)
            => cache.GetInProgressFileName(m.download, m.StandardName());

        private static string DescribeUncachedAvailability(CkanModule m, FileInfo fi)
            => fi.Exists
                ? string.Format(Properties.Resources.NetModuleCacheModuleResuming,
                    m.name, m.version, m.download.Host ?? "",
                    CkanModule.FmtSize(m.download_size - fi.Length))
                : string.Format(Properties.Resources.NetModuleCacheModuleHostSize,
                    m.name, m.version, m.download.Host ?? "", CkanModule.FmtSize(m.download_size));

        public string DescribeAvailability(CkanModule m)
            => m.IsMetapackage
                ? string.Format(Properties.Resources.NetModuleCacheMetapackage, m.name, m.version)
                : IsMaybeCachedZip(m)
                    ? string.Format(Properties.Resources.NetModuleCacheModuleCached, m.name, m.version)
                    : DescribeUncachedAvailability(m, new FileInfo(GetInProgressFileName(m)));

        /// <summary>
        /// Calculate the SHA1 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <returns>
        /// SHA1 hash, in all-caps hexadecimal format
        /// </returns>
        public string GetFileHashSha1(string filePath, IProgress<long> progress)
        {
            return cache.GetFileHashSha1(filePath, progress);
        }

        /// <summary>
        /// Calculate the SHA256 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <returns>
        /// SHA256 hash, in all-caps hexadecimal format
        /// </returns>
        public string GetFileHashSha256(string filePath, IProgress<long> progress)
        {
            return cache.GetFileHashSha256(filePath, progress);
        }

        /// <summary>
        /// Try to add a file to the module cache.
        /// Throws exceptions if the file doesn't match the metadata.
        /// </summary>
        /// <param name="module">The module object corresponding to the download</param>
        /// <param name="path">Path to the file to add</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <param name="description">Description of the file</param>
        /// <param name="move">True to move the file, false to copy</param>
        /// <returns>
        /// Name of the new file in the cache
        /// </returns>
        public string Store(CkanModule module, string path, IProgress<long> progress, string description = null, bool move = false)
        {
            // ZipValid takes a lot longer than the hash steps, so scale them 60:20:20
            const int zipValidPercent   = 60;
            const int hashSha1Percent   = 20;
            const int hashSha256Percent = 20;

            progress.Report(0);
            // Check file exists
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists)
                throw new FileNotFoundKraken(path);

            // Check file size
            if (module.download_size > 0 && fi.Length != module.download_size)
                throw new InvalidModuleFileKraken(module, path, string.Format(
                    Properties.Resources.NetModuleCacheBadLength,
                    module, path, fi.Length, module.download_size));

            // Check valid CRC
            string invalidReason;
            if (!NetFileCache.ZipValid(path, out invalidReason, new Progress<long>(percent =>
                progress?.Report(percent * zipValidPercent / 100))))
            {
                throw new InvalidModuleFileKraken(module, path, string.Format(
                    Properties.Resources.NetModuleCacheNotValidZIP,
                    module, path, invalidReason));
            }

            // Some older metadata doesn't have hashes
            if (module.download_hash != null)
            {
                // Check SHA1 match
                string sha1 = GetFileHashSha1(path, new Progress<long>(percent =>
                    progress?.Report(zipValidPercent + percent * hashSha1Percent / 100)));
                if (sha1 != module.download_hash.sha1)
                {
                    throw new InvalidModuleFileKraken(module, path, string.Format(
                        Properties.Resources.NetModuleCacheMismatchSHA1,
                        module, path, sha1, module.download_hash.sha1));
                }

                // Check SHA256 match
                string sha256 = GetFileHashSha256(path, new Progress<long>(percent =>
                    progress?.Report(zipValidPercent + hashSha1Percent + percent * hashSha256Percent / 100)));
                if (sha256 != module.download_hash.sha256)
                {
                    throw new InvalidModuleFileKraken(module, path, string.Format(
                        Properties.Resources.NetModuleCacheMismatchSHA256,
                        module, path, sha256, module.download_hash.sha256));
                }
            }

            // If no exceptions, then everything is fine
            var success = cache.Store(module.download, path, description ?? module.StandardName(), move);
            // Make sure completion is signalled so progress bars go away
            progress?.Report(100);
            return success;
        }

        /// <summary>
        /// Remove a module's download file from the cache
        /// </summary>
        /// <param name="module">Module to purge</param>
        /// <returns>
        /// True if purged, false otherwise
        /// </returns>
        public bool Purge(CkanModule module)
        {
            return cache.Remove(module.download);
        }

        private NetFileCache cache;
    }
}
