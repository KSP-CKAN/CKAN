using System;
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
        /// Returns where the given file is cached, or would be cached if it we had it.
        /// </summary>
        public string CachePath(Uri url)
        {
            string full_path;
            if (!KSPManager.CurrentInstance.Cache.IsCached(url, out full_path))
            {
                return null;
            }

            return full_path;
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

