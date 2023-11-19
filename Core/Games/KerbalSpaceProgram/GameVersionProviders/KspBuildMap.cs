using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using log4net;
using Newtonsoft.Json;

using CKAN.Versioning;

namespace CKAN.Games.KerbalSpaceProgram.GameVersionProviders
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
        private static readonly string cachedBuildMapPath =
            Path.Combine(CKANPathUtils.AppDataPath, "builds-ksp.json");

        private static readonly ILog Log = LogManager.GetLogger(typeof(KspBuildMap));

        private readonly object _buildMapLock = new object();
        private JBuilds _jBuilds;

        public GameVersion this[string buildId]
        {
            get
            {
                EnsureBuildMap();

                return _jBuilds.Builds.TryGetValue(buildId, out string version) ? GameVersion.Parse(version) : null;
            }
        }

        public List<GameVersion> KnownVersions
        {
            get
            {
                EnsureBuildMap();
                return _jBuilds.Builds
                    .Select(b => GameVersion.Parse(b.Value))
                    .ToList();
            }
        }

        public KspBuildMap()
        {
        }

        private void EnsureBuildMap()
        {
            if (_jBuilds is null)
            {
                lock (_buildMapLock)
                {
                    if (_jBuilds is null)
                    {
                        // Check for a cached copy of the remote build map
                        if (TrySetCachedBuildMap())
                        {
                            return;
                        }
                        // If that doesn't exist, use the copy from when we were compiled
                        if (TrySetEmbeddedBuildMap())
                        {
                            return;
                        }

                        Log.Warn("Could not load build map from cached or embedded copy");
                    }
                }
            }
        }

        /// <summary>
        /// Download the build map from the server to the cache
        /// </summary>
        public void Refresh()
        {
            Log.Debug("Refreshing build map from server");
            if (TrySetRemoteBuildMap())
            {
                return;
            }

            Log.Warn("Could not refresh the build map from remote server");
        }

        private bool TrySetBuildMap(string buildMapJson)
        {
            try
            {
                _jBuilds = JsonConvert.DeserializeObject<JBuilds>(buildMapJson);
                return _jBuilds != null;
            }
            catch (Exception e)
            {
                Log.WarnFormat("Could not parse build map");
                Log.DebugFormat("{0}\n{1}", buildMapJson, e);
                return false;
            }
        }

        private bool TrySetCachedBuildMap()
        {
            try
            {
                Log.Debug("Getting cached build map");
                return TrySetBuildMap(File.ReadAllText(cachedBuildMapPath));
            }
            catch
            {
                return false;
            }
        }

        private bool TrySetRemoteBuildMap()
        {
            try
            {
                Log.Debug("Getting remote build map");
                var json = Net.DownloadText(BuildMapUri);
                if (TrySetBuildMap(json))
                {
                    // Save to disk if parse succeeds
                    new FileInfo(cachedBuildMapPath).Directory.Create();
                    File.WriteAllText(cachedBuildMapPath, json);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.WarnFormat("Could not retrieve latest build map from: {0}", BuildMapUri);
                Log.Debug(e);
            }
            return false;
        }

        private bool TrySetEmbeddedBuildMap()
        {
            try
            {
                Log.Debug("Getting embedded build map");
                var resourceStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("CKAN.builds-ksp.json");

                if (resourceStream != null)
                {
                    using (var reader = new StreamReader(resourceStream))
                    {
                        return TrySetBuildMap(reader.ReadToEnd());
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
