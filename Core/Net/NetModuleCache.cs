using System;
using System.Linq;
using System.IO;
using System.Threading;

using ICSharpCode.SharpZipLib.Zip;

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
        static NetModuleCache()
        {
            // SharpZibLib 1.1.0 changed this to default to false, but we depend on it for international mods.
            // https://github.com/icsharpcode/SharpZipLib/issues/591
            ZipStrings.UseUnicode = true;
        }

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

        public event Action<CkanModule> ModStored;
        public event Action<CkanModule> ModPurged;

        // Simple passthrough wrappers
        public void Dispose()
        {
            cache.Dispose();
        }
        public void RemoveAll()
        {
            cache.RemoveAll();
            ModPurged?.Invoke(null);
        }
        public void MoveFrom(string fromDir)
        {
            cache.MoveFrom(fromDir);
        }
        public bool IsCached(CkanModule m)
            => m.download?.Any(dlUri => cache.IsCached(dlUri))
                ?? false;
        public bool IsCached(CkanModule m, out string outFilename)
        {
            if (m.download != null)
            {
                foreach (var dlUri in m.download)
                {
                    if (cache.IsCached(dlUri, out outFilename))
                    {
                        return true;
                    }
                }
            }
            outFilename = null;
            return false;
        }
        public bool IsMaybeCachedZip(CkanModule m)
            => m.download?.Any(dlUri => cache.IsMaybeCachedZip(dlUri, m.release_date))
                ?? false;
        public string GetCachedFilename(CkanModule m)
            => m.download?.Select(dlUri => cache.GetCachedFilename(dlUri, m.release_date))
                          .FirstOrDefault(filename => filename != null);
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
            => m.download == null
                ? null
                : cache.GetInProgressFileName(m.download, m.StandardName());

        private static string DescribeUncachedAvailability(CkanModule m, FileInfo fi)
            => fi.Exists
                ? string.Format(Properties.Resources.NetModuleCacheModuleResuming,
                    m.name, m.version,
                    string.Join(", ", ModuleInstaller.PrioritizedHosts(m.download)),
                    CkanModule.FmtSize(m.download_size - fi.Length))
                : string.Format(Properties.Resources.NetModuleCacheModuleHostSize,
                    m.name, m.version,
                    string.Join(", ", ModuleInstaller.PrioritizedHosts(m.download)),
                    CkanModule.FmtSize(m.download_size));

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
        public string GetFileHashSha1(string filePath, IProgress<int> progress, CancellationToken cancelToken = default)
            => cache.GetFileHashSha1(filePath, progress, cancelToken);

        /// <summary>
        /// Calculate the SHA256 hash of a file
        /// </summary>
        /// <param name="filePath">Path to file to examine</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <returns>
        /// SHA256 hash, in all-caps hexadecimal format
        /// </returns>
        public string GetFileHashSha256(string filePath, IProgress<int> progress, CancellationToken cancelToken = default)
            => cache.GetFileHashSha256(filePath, progress, cancelToken);

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
        public string Store(CkanModule        module,
                            string            path,
                            IProgress<int>    progress,
                            string            description = null,
                            bool              move        = false,
                            CancellationToken cancelToken = default,
                            bool              validate    = true)
        {
            if (validate)
            {
                // ZipValid takes a lot longer than the hash check, so scale them 70:30 if hashes are present
                int zipValidPercent = module.download_hash == null ? 100 : 70;

                progress?.Report(0);
                // Check file exists
                FileInfo fi = new FileInfo(path);
                if (!fi.Exists)
                {
                    throw new FileNotFoundKraken(path);
                }

                // Check file size
                if (module.download_size > 0 && fi.Length != module.download_size)
                {
                    throw new InvalidModuleFileKraken(module, path, string.Format(
                        Properties.Resources.NetModuleCacheBadLength,
                        module, path, fi.Length, module.download_size));
                }

                cancelToken.ThrowIfCancellationRequested();

                // Check valid CRC
                if (!ZipValid(path, out string invalidReason, new Progress<int>(percent =>
                    progress?.Report(percent * zipValidPercent / 100))))
                {
                    throw new InvalidModuleFileKraken(module, path, string.Format(
                        Properties.Resources.NetModuleCacheNotValidZIP,
                        module, path, invalidReason));
                }

                cancelToken.ThrowIfCancellationRequested();

                // Some older metadata doesn't have hashes
                if (module.download_hash != null)
                {
                    int hashPercent = 100 - zipValidPercent;
                    // Only check one hash, sha256 if it's set, sha1 otherwise
                    if (!string.IsNullOrEmpty(module.download_hash.sha256))
                    {
                        // Check SHA256 match
                        string sha256 = GetFileHashSha256(path, new Progress<int>(percent =>
                            progress?.Report(zipValidPercent + (percent * hashPercent / 100))), cancelToken);
                        if (sha256 != module.download_hash.sha256)
                        {
                            throw new InvalidModuleFileKraken(module, path, string.Format(
                                Properties.Resources.NetModuleCacheMismatchSHA256,
                                module, path, sha256, module.download_hash.sha256));
                        }
                    }
                    else if (!string.IsNullOrEmpty(module.download_hash.sha1))
                    {
                        // Check SHA1 match
                        string sha1 = GetFileHashSha1(path, new Progress<int>(percent =>
                            progress?.Report(zipValidPercent + (percent * hashPercent / 100))), cancelToken);
                        if (sha1 != module.download_hash.sha1)
                        {
                            throw new InvalidModuleFileKraken(module, path, string.Format(
                                Properties.Resources.NetModuleCacheMismatchSHA1,
                                module, path, sha1, module.download_hash.sha1));
                        }
                    }
                }

                cancelToken.ThrowIfCancellationRequested();
            }
            // If no exceptions, then everything is fine
            var success = cache.Store(module.download[0], path, description ?? module.StandardName(), move);
            // Make sure completion is signalled so progress bars go away
            progress?.Report(100);
            ModStored?.Invoke(module);
            return success;
        }

        /// <summary>
        /// Check whether a ZIP file is valid
        /// </summary>
        /// <param name="filename">Path to zip file to check</param>
        /// <param name="invalidReason">Description of problem with the file</param>
        /// <param name="progress">Callback to notify as we traverse the input, called with percentages from 0 to 100</param>
        /// <returns>
        /// True if valid, false otherwise. See invalidReason param for explanation.
        /// </returns>
        public static bool ZipValid(string         filename,
                                    out string     invalidReason,
                                    IProgress<int> progress)
        {
            try
            {
                if (filename != null)
                {
                    using (ZipFile zip = new ZipFile(filename))
                    {
                        string zipErr = null;
                        // Limit progress updates to 100 per ZIP file
                        long highestPercent = -1;
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
                                    zipErr = string.Format(
                                        Properties.Resources.NetFileCacheZipError,
                                        st.Operation, st.Entry?.Name, msg);
                                }
                                else if (st.Entry != null && progress != null)
                                {
                                    // Report progress
                                    var percent = (int)(100 * st.Entry.ZipFileIndex / zip.Count);
                                    if (percent > highestPercent)
                                    {
                                        progress.Report(percent);
                                        highestPercent = percent;
                                    }
                                }
                            }))
                        {
                            invalidReason = "";
                            return true;
                        }
                        else
                        {
                            invalidReason = zipErr ?? Properties.Resources.NetFileCacheZipTestArchiveFalse;
                            return false;
                        }
                    }
                }
                else
                {
                    invalidReason = Properties.Resources.NetFileCacheNullFileName;
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
                invalidReason = string.Format(Properties.Resources.NetFileCacheMonoNotSupported, nse.Message);
                return false;
            }
        }

        /// <summary>
        /// Remove a module's download files from the cache
        /// </summary>
        /// <param name="module">Module to purge</param>
        /// <returns>
        /// True if all purged, false otherwise
        /// </returns>
        public bool Purge(CkanModule module)
        {
            if (module.download != null)
            {
                foreach (var dlUri in module.download)
                {
                    if (!cache.Remove(dlUri))
                    {
                        return false;
                    }
                }
            }
            ModPurged?.Invoke(module);
            return true;
        }

        private readonly NetFileCache cache;
    }
}
