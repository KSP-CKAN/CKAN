using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using CKAN.Versioning;
using CKAN.Configuration;

namespace CKAN.GameVersionProviders
{
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

    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class KspBuildMap : IKspBuildMap
    {
        // TODO: Need a way for the client to configure this
        private static readonly Uri BuildMapUri =
            new Uri("https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/builds.json");

        private static readonly ILog Log = LogManager.GetLogger(typeof(KspBuildMap));

        private readonly object _buildMapLock = new object();
        private readonly IConfiguration _configuration;
        private JBuilds _jBuilds;

        public GameVersion this[string buildId]
        {
            get
            {
                EnsureBuildMap();

                string version;
                return _jBuilds.Builds.TryGetValue(buildId, out version) ? GameVersion.Parse(version) : null;
            }
        }

        public List<GameVersion> KnownVersions
        {
            get
            {
                EnsureBuildMap();
                List<GameVersion> knownVersions = new List<GameVersion>();
                foreach (var version in _jBuilds.Builds)
                {
                    knownVersions.Add(GameVersion.Parse(version.Value));
                }
                return knownVersions;
            }
        }

        public KspBuildMap(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private void EnsureBuildMap()
        {
            if (ReferenceEquals(_jBuilds, null))
            {
                lock (_buildMapLock)
                {
                    if (ReferenceEquals(_jBuilds, null))
                    {
                        Refresh();
                    }
                }
            }
        }

        /// <summary>
        /// Load a build map
        /// </summary>
        public void Refresh()
        {
            if (TrySetRemoteBuildMap())   return;
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
    }
}
