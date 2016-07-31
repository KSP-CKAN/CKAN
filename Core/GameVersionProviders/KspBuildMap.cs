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
        // TODO: Need a way for the client to configure this
        private static readonly Uri BuildMapUri =
            new Uri("https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/builds.json");

        // TODO: Get this through dependency injection
        private readonly ILog _log = LogManager.GetLogger(typeof(KspBuildMap));

        private readonly object _buildMapLock = new object();
        private JBuilds _jBuilds;

        private readonly IWin32Registry _registry;
        
        public KspVersion this[string buildId]
        {
            get
            {
                EnsureBuildMap();

                string version;
                return _jBuilds.Builds.TryGetValue(buildId, out version) ? KspVersion.Parse(version) : null;
            }
        }

        public KspBuildMap(IWin32Registry registry)
        {
            _registry = registry;
        }

        private void EnsureBuildMap()
        {
            if (ReferenceEquals(_jBuilds, null))
            {
                lock(_buildMapLock)
                {
                    if (ReferenceEquals(_jBuilds, null))
                    {
                        Refresh(useCachedVersion: true);
                    }
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

            _log.Warn("Could not refresh the build map from any source");
        }

        private bool TrySetBuildMap(string buildMapJson)
        {
            try
            {
                _jBuilds = JsonConvert.DeserializeObject<JBuilds>(buildMapJson);
                return true;
            }
            catch(Exception e)
            {
                _log.WarnFormat("Could not parse build map");
                _log.DebugFormat("{0}\r\n{1}", buildMapJson, e);
                return false;
            }
        }

        private bool TrySetRemoteBuildMap()
        {
            try
            {
                var json = Net.DownloadText(BuildMapUri);

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
                _log.WarnFormat("Could not retrieve latest build map from: {0}", BuildMapUri);
                _log.Debug(e);
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
                _log.WarnFormat("Could not retrieve build map from registry");
                _log.Debug(e);
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
                _log.WarnFormat("Could not retrieve build map from embedded resource");
                _log.Debug(e);
                return false;
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class JBuilds
        {
            [JsonProperty("builds")]
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public Dictionary<string, string> Builds { get; set; }
        }
    }
}
