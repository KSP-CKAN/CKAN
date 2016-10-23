using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CKAN.Versioning;
using log4net;
using Newtonsoft.Json;

namespace CKAN.GameVersionProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspBuildMap : IKspBuildMap
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KspBuildMap));

        private readonly string _buildUrl;
        private readonly object _buildMapLock = new object();
        private readonly IWin32Registry _registry;
        private BuildMap _buildMap;

        /// <summary>
        /// Create a KSPBuildMap object with a user-configured update URL
        /// </summary>
        /// <param name="buildUrl"></param>
        public KspBuildMap(string buildUrl)
        {
            _buildUrl = buildUrl;
        }

        public KspVersion this[string buildId]
        {
            get
            {
                EnsureBuildMap();

                string version;
                return _buildMap.Builds.TryGetValue(buildId, out version) ? KspVersion.Parse(version) : null;
            }
        }

        public KspBuildMap(IWin32Registry registry)
        {
            _registry = registry;
        }

        private void EnsureBuildMap()
        {
            if (!ReferenceEquals(_buildMap, null)) return;

            lock(_buildMapLock)
            {
                if (ReferenceEquals(_buildMap, null))
                {
                    Refresh(useCachedVersion: true);
                }
            }
        }

        public void Refresh()
        {
            Refresh(useCachedVersion: false);
        }

        private void Refresh(bool useCachedVersion)
        {
            if (useCachedVersion)
            {
                // Attempt to set the build map from the cached version in the registry
                if (TrySetRegistryBuildMap()) return;

                // Attempt to set the build map from the repository
                if (TrySetRemoteBuildMap()) return;
            }
            else
            {
                // Attempt to set the build map from the repository
                if (TrySetRemoteBuildMap()) return;

                // Attempt to set the build map from the cached version in the registry
                if (TrySetRegistryBuildMap()) return;
            }

            // If that fails attempt to set the build map from the embedded version
            if (TrySetEmbeddedBuildMap()) return;

            Log.Warn("Could not refresh the build map from any source");
        }

        private bool TrySetBuildMap(string buildMapJson)
        {
            try
            {
                _buildMap = JsonConvert.DeserializeObject<BuildMap>(buildMapJson);
                return true;
            }
            catch(Exception e)
            {
                Log.WarnFormat("Could not parse build map");
                Log.DebugFormat("{0}\n{1}", buildMapJson, e);
                return false;
            }
        }
        private bool TrySetRemoteBuildMap()
        {
            try
            {
                var json = Net.DownloadText(_buildUrl);

                if (TrySetBuildMap(json))
                {
                    _registry.SetKSPBuilds(json);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.WarnFormat("Could not retrieve latest build map from: {0}", _buildUrl);
                Log.Debug(e);
                return false;
            }
        }

        private bool TrySetRegistryBuildMap()
        {
            try
            {
                var json = _registry.GetKSPBuilds();
                return json != null && TrySetBuildMap(json);
            }
            catch(Exception e)
            {
                Log.WarnFormat("Could not retrieve build map from registry");
                Log.Debug(e);
                return false;
            }
        }

        private bool TrySetEmbeddedBuildMap()
        {
            try
            {
                var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CKAN.builds.json");

                if (resourceStream != null)
                {
                    using (var reader = new StreamReader(resourceStream))
                    {
                        TrySetBuildMap(reader.ReadToEnd());
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                Log.WarnFormat("Could not retrieve build map from embedded resource");
                Log.Debug(e);
                return false;
            }
        }

        private sealed class BuildMap
        {
            [JsonProperty("builds")]
            public Dictionary<string, string> Builds { get; set; }
        }
    }
}
