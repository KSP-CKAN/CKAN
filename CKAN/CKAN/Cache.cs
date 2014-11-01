using System.IO;

namespace CKAN
{
    /// <summary>
    /// Everything do with file caching operations.
    /// </summary>
    public class Cache
    {
        internal string cache_path;

        /// <summary>
        /// Creates a new cache object, using the path provided for cached files.
        /// If the directory does not exist, a DirectoryNotFoundKraken is thrown.
        public Cache(string cache_path)
        {
            if (! Directory.Exists(cache_path))
            {
                throw new DirectoryNotFoundKraken(cache_path);
            }
            this.cache_path = cache_path;
        }

        /// <summary>
        /// Returns true if the given module is present in our cache.
        /// </summary>
        //
        // TODO: Update this if we start caching by URL (GH #111)
        public bool IsCached(CkanModule module)
        {
            return IsCached(module.StandardName());
        }

        /// <summary>
        /// Returns true if the given file is in our cache.
        /// </summary>
        public bool IsCached(string filename)
        {
            // It's cached if we can find it on a cache lookup.
            return CachedFile(filename) != null;
        }

        /// <summary>
        /// Returns the path to the cached copy of the file, or null if it's not cached.
        /// </summary>
        public string CachedFile(string file)
        {
            string full_path = CachePath(file);
            if (File.Exists(full_path))
            {
                return full_path;
            }
            return null;
        }

        /// <summary>
        /// Returns the path to the cached copy of the module, or null if it's not cached.
        /// </summary>
        public string CachedFile(CkanModule module)
        {
            return CachedFile(module.StandardName());
        }

        /// <summary>
        /// Returns where the given file is cached, or would be cached if it we had it.
        /// </summary>
        public string CachePath(string file)
        {
            return Path.Combine(cache_path, file);
        }

        public string CachePath(CkanModule module)
        {
            return CachePath(module.StandardName());
        }

        /// <summary>
        /// Returns the path used by this cache.
        /// </summary>
        public string CachePath()
        {
            return cache_path;
        }

    }
}

