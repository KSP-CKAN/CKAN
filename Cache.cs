using System.IO;

namespace CKAN
{
    /// <summary>
    /// Everything do with file caching operations.
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Returns true if the given module is present in our cache.
        /// </summary>
        //
        // TODO: Update this if we start caching by URL (GH #111)
        public static bool IsCached(CkanModule module)
        {
            return IsCached(module.StandardName());
        }

        public static bool IsCached(string filename)
        {
            // It's cached if we can find it on a cache lookup.
            return CachedFile(filename) != null;
        }

        /// <summary>
        /// Returns the path to the cached copy of the file or module, or null if it's not cached.
        /// </summary>
        public static string CachedFile(string file)
        {
            string full_path = CachePath(file);
            if (File.Exists(full_path))
            {
                return full_path;
            }
            return null;
        }

        public static string CachedFile(CkanModule module)
        {
            return CachedFile(module.StandardName());
        }

        /// <summary>
        /// Returns where the given file is cached, or would be cached if it we had it.
        /// </summary>
        public static string CachePath(string file)
        {
            return Path.Combine(KSPManager.CurrentInstance.DownloadCacheDir(), file);
        }

    }
}

