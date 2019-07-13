using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        JBuilds GetKSPBuilds();
        void SetKSPBuilds(JBuilds buildMap);

        void SetRegistryToInstances(SortedList<string, KSP> instances);
        IEnumerable<Tuple<string, string>> GetInstances();
    }

    // <summary>
    // THIS IS NOT THE BUILD MAP! If you are trying to access the build map,
    // you want to use IKspBuildMap.
    //
    // This class represents the internal JSON structure of the build map,
    // and should only be used by implementations of IKspBuildMap and
    // IConfiguration.
    // </summary>
    public sealed class JBuilds
    {
        [JsonProperty("builds")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Dictionary<string, string> Builds { get; set; }
    }
}
