﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using CKAN.Versioning;
using CKAN.Win32Registry;

namespace CKAN.GameVersionProviders
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspBuildMap : IKspBuildMap
    {
        // TODO: Need a way for the client to configure this
        private static readonly Uri BuildMapUri =
            new Uri("https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/builds.json");

        private static readonly ILog Log = LogManager.GetLogger(typeof(KspBuildMap));

        private readonly object _buildMapLock = new object();
        private readonly IWin32Registry _registry;
        private JBuilds _jBuilds;

        public KspVersion this[string buildId]
        {
            get
            {
                EnsureBuildMap();

                string version;
                return _jBuilds.Builds.TryGetValue(buildId, out version) ? KspVersion.Parse(version) : null;
            }
        }

        public List<KspVersion> KnownVersions
        {
            get
            {
                EnsureBuildMap();
                List<KspVersion> knownVersions = new List<KspVersion>();
                foreach (var version in _jBuilds.Builds)
                {
                    knownVersions.Add(KspVersion.Parse(version.Value));
                }
                return knownVersions;
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
                lock (_buildMapLock)
                {
                    if (ReferenceEquals(_jBuilds, null))
                    {
                        Refresh(BuildMapSource.Cache);
                    }
                }
            }
        }

        /// <summary>
        /// Load a build map
        /// </summary>
        /// <param name="source">Remote to download builds.json from GitHub (default), Cache to use the data from the last remote refresh, Embedded to use the builds.json built into the exe</param>
        public void Refresh(BuildMapSource source = BuildMapSource.Remote)
        {
            switch (source)
            {
                case BuildMapSource.Cache:
                    if (TrySetRegistryBuildMap()) return;
                    if (TrySetRemoteBuildMap())   return;
                    break;

                case BuildMapSource.Remote:
                    if (TrySetRemoteBuildMap())   return;
                    if (TrySetRegistryBuildMap()) return;
                    break;
            }
            if (TrySetEmbeddedBuildMap()) return;

            Log.Warn("Could not refresh the build map from any source");
        }

        private bool TrySetBuildMap(string buildMapJson)
        {
            try
            {
                _jBuilds = JsonConvert.DeserializeObject<JBuilds>(buildMapJson);
                return true;
            }
            catch (Exception e)
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
                Log.WarnFormat("Could not retrieve latest build map from: {0}", BuildMapUri);
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
            catch (Exception e)
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
            catch (Exception e)
            {
                Log.WarnFormat("Could not retrieve build map from embedded resource");
                Log.Debug(e);
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
