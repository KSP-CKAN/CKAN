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
        public void Clear()
        {
            cache.OnCacheChanged();
        }
        public string GetCachePath()
        {
            return cache.GetCachePath();
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
            return cache.IsMaybeCachedZip(m.download);
        }
        public string GetCachedFilename(CkanModule m)
        {
            return cache.GetCachedFilename(m.download);
        }
        public string GetCachedZip(CkanModule m)
        {
            return cache.GetCachedZip(m.download);
        }

        /// <summary>
        /// Calculate the SHA1 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <returns>
        /// SHA1 hash, in all-caps hexadecimal format
        /// </returns>
        public static string GetFileHashSha1(string filePath)
        {
            using (FileStream     fs   = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BufferedStream bs   = new BufferedStream(fs))
            using (SHA1Cng        sha1 = new SHA1Cng())
            {
                return BitConverter.ToString(sha1.ComputeHash(bs)).Replace("-", "");
            }
        }

        /// <summary>
        /// Calculate the SHA256 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <returns>
        /// SHA256 hash, in all-caps hexadecimal format
        /// </returns>
        public static string GetFileHashSha256(string filePath)
        {
            using (FileStream     fs     = new FileStream(@filePath, FileMode.Open, FileAccess.Read))
            using (BufferedStream bs     = new BufferedStream(fs))
            using (SHA256Managed  sha256 = new SHA256Managed())
            {
                return BitConverter.ToString(sha256.ComputeHash(bs)).Replace("-", "");
            }
        }

        /// <summary>
        /// Try to add a file to the module cache.
        /// Throws exceptions if the file doesn't match the metadata.
        /// </summary>
        /// <param name="module">The module object corresponding to the download</param>
        /// <param name="path">Path to the file to add</param>
        /// <param name="description">Description of the file</param>
        /// <param name="move">True to move the file, false to copy</param>
        /// <returns>
        /// Name of the new file in the cache
        /// </returns>
        public string Store(CkanModule module, string path, string description = null, bool move = false)
        {
            // Check file exists
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists)
                throw new FileNotFoundKraken(path);

            // Check file size
            if (fi.Length != module.download_size)
                throw new InvalidModuleFileKraken(module, path,
                    $"{path} has length {fi.Length}, should be {module.download_size}");

            // Check valid CRC
            string invalidReason;
            if (!NetFileCache.ZipValid(path, out invalidReason))
                throw new InvalidModuleFileKraken(module, path,
                    $"{path} is not a valid ZIP file: {invalidReason}");

            // Some older metadata doesn't have hashes
            if (module.download_hash != null)
            {
                // Check SHA1 match
                string sha1 = GetFileHashSha1(path);
                if (sha1 != module.download_hash.sha1)
                    throw new InvalidModuleFileKraken(module, path,
                        $"{path} has SHA1 {sha1}, should be {module.download_hash.sha1}");

                // Check SHA256 match
                string sha256 = GetFileHashSha256(path);
                if (sha256 != module.download_hash.sha256)
                    throw new InvalidModuleFileKraken(module, path,
                        $"{path} has SHA256 {sha256}, should be {module.download_hash.sha256}");
            }

            // If no exceptions, then everything is fine
            return cache.Store(module.download, path, description ?? module.StandardName(), move);
        }

        private NetFileCache cache;
    }
}
