using System;
using System.Collections.Generic;

namespace CKAN.Win32Registry
{
    public interface IWin32Registry
    {
        void SetRegistryToInstances (SortedList<string, KSP> instances);
        IEnumerable<Tuple<string, string>> GetInstances ();
        string GetKSPBuilds ();
        void SetKSPBuilds (string buildMap);

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
    }
}
