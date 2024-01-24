using System;
using System.Collections.Generic;

namespace CKAN.Configuration
{
    public interface IConfiguration
    {
        string AutoStartInstance { get; set; }

        /// <summary>
        /// Get and set the path to the download cache
        /// </summary>
        string DownloadCacheDir { get; set; }

        /// <summary>
        /// Get and set the maximum number of bytes allowed in the cache.
        /// Unlimited if null.
        /// </summary>
        long? CacheSizeLimit { get; set; }

        /// <summary>
        /// Get and set the interval in minutes to refresh the modlist.
        /// Never refresh if 0.
        /// </summary>
        int RefreshRate { get; set; }

        string Language { get; set; }

        /// <summary>
        /// Get the hosts that have auth tokens stored in the registry
        /// </summary>
        /// <returns>
        /// Strings that are values of the auth token registry key
        /// </returns>
        IEnumerable<string> GetAuthTokenHosts();

        /// <summary>
        /// Look for an auth token in the registry.
        /// </summary>
        /// <param name="host">Host for which to find a token</param>
        /// <param name="token">Value of the token returned in parameter</param>
        /// <returns>
        /// True if found, false otherwise
        /// </returns>
        bool TryGetAuthToken(string host, out string token);

        /// <summary>
        /// Set an auth token in the registry
        /// </summary>
        /// <param name="host">Host for which to set the token</param>
        /// <param name="token">Token to set, or null to delete</param>
        void SetAuthToken(string host, string token);

        void SetRegistryToInstances(SortedList<string, GameInstance> instances);
        IEnumerable<Tuple<string, string, string>> GetInstances();

        /// <summary>
        /// Paths that should be excluded from all installations
        /// </summary>
        string[] GlobalInstallFilters { get; set; }

        /// <summary>
        /// List of hosts in order of priority when there are multiple URLs to choose from.
        /// The first null value represents where all other hosts should go.
        /// </summary>
        string[] PreferredHosts { get; set; }

        /// <summary>
        /// true if user wants to use nightly builds from S3, false to use releases from GitHub
        /// </summary>
        bool? DevBuilds { get; set; }
    }
}
