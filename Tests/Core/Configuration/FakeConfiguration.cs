using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

using CKAN;
using CKAN.Configuration;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram.GameVersionProviders;

using Tests.Data;

namespace Tests.Core.Configuration
{
    public class FakeConfiguration : IConfiguration, IDisposable
    {
        public FakeConfiguration(GameInstance instance,
                                 string       autostart,
                                 string?      downloadCachePath = null)
            : this(new List<Tuple<string, string, string>>
                   {
                       new Tuple<string, string, string>(instance.Name,
                                                         instance.GameDir,
                                                         instance.Game.ShortName)
                   },
                   autostart,
                   downloadCachePath)
        {
        }

        /// <summary>
        /// Initialize the fake registry
        /// </summary>
        /// <param name="instances">List of name/path/game tuples for the instances</param>
        /// <param name="auto_start_instance">The auto start instance to use</param>
        public FakeConfiguration(List<Tuple<string, string, string>> instances,
                                 string?                             auto_start_instance,
                                 string?                             downloadCachePath)
        {
            Instances         = instances;
            AutoStartInstance = auto_start_instance;
            downloadCacheDirs.Add(downloadCachePath == null
                                      ? new TemporaryDirectory()
                                      : new TemporaryDirectory(downloadCachePath));
        }

        /// <summary>
        /// The instances in the fake registry
        /// </summary>
        public List<Tuple<string, string, string>> Instances { get; set; }
        /// <summary>
        /// Build map for the fake registry
        /// </summary>
        public JBuilds?                    BuildMap         { get; set; }

        /// <summary>
        /// Path to download cache folder for the fake registry
        /// </summary>
        public string?                     DownloadCacheDir
        {
            get => downloadCacheDirs.Last()?.Directory.FullName;
            set
            {
                // The GameInstanceManager sometimes re-assigns the current value
                if (value != downloadCacheDirs.Last()?.Directory.FullName)
                {
                    downloadCacheDirs.Add(value == null ? null
                                                        : new TemporaryDirectory(value));
                }
            }
        }
        private readonly List<TemporaryDirectory?> downloadCacheDirs = new List<TemporaryDirectory?>();

        /// <summary>
        /// Maximum number of bytes of downloads to retain on disk
        /// </summary>
        public long?                       CacheSizeLimit   { get; set; }
        /// <summary>
        /// Interval in minutes to refresh the modlist
        /// </summary>
        public int                         RefreshRate      { get; set; }

        /// <summary>
        /// Number of instances in the fake registry
        /// </summary>
        public int InstanceCount => Instances.Count;

        // In the Win32Registry it is not possible to get null in autostart.
        private string? _AutoStartInstance;

        /// <summary>
        /// The auto start instance for the fake registry
        /// </summary>
        public string? AutoStartInstance
        {
            get => _AutoStartInstance;
            #pragma warning disable IDE0027
            set
            {
                _AutoStartInstance = value;
            }
            #pragma warning restore IDE0027
        }

        /// <summary>
        /// Retrieve and instance from the fake registry
        /// </summary>
        /// <param name="i">Index of the instance to retrieve</param>
        /// <returns>
        /// Name/path pair for the requested instance
        /// </returns>
        public Tuple<string, string, string> GetInstance(int i) => Instances[i];

        /// <summary>
        /// Set the instance data for the fake registry
        /// </summary>
        /// <param name="instances">New list of instances to use</param>
        /// <param name="autoStartInstance">Which instance to use for auto start</param>
        /// <returns>
        /// Returns
        /// </returns>
        public void SetRegistryToInstances(SortedList<string, GameInstance> instances)
        {
            Instances = instances.Select(kvpair => Tuple.Create(kvpair.Key,
                                                                kvpair.Value.GameDir,
                                                                kvpair.Value.Game.ShortName))
                                 .ToList();
        }

        /// <summary>
        /// The instances in the fake registry
        /// </summary>
        public IEnumerable<Tuple<string, string, string>> GetInstances() => Instances;

        /// <summary>
        /// The build map of the fake registry
        /// </summary>
        public JBuilds? GetKSPBuilds() => BuildMap;

        /// <summary>
        /// Set the build map for the fake registry
        /// </summary>
        /// <param name="buildMap">New build map to use</param>
        public void SetKSPBuilds(JBuilds buildMap)
        {
            BuildMap = buildMap;
        }

        public IEnumerable<string> GetAuthTokenHosts()
            => authTokens.Keys;

        public void SetAuthToken(string host, string? token)
        {
            switch (token)
            {
                case string t:
                    authTokens.Add(host, t);
                    break;
                default:
                    authTokens.Remove(host);
                    break;
            }
        }

        public bool TryGetAuthToken(string host,
                                    [NotNullWhen(true)] out string? token)
            => authTokens.TryGetValue(host, out token);

        private readonly Dictionary<string, string> authTokens = new Dictionary<string, string>();

        private string? _Language;
        public string? Language
        {
            get => _Language;

            set
            {
                if (Utilities.AvailableLanguages.Contains(value))
                {
                    _Language = value;
                }
            }
        }

        private readonly IDictionary<string, string[]> globalInstallFilters = new Dictionary<string, string[]>();

        public string[] GetGlobalInstallFilters(IGame game)
            => globalInstallFilters.TryGetValue(game.ShortName, out string[]? value)
                   ? value
                   : Array.Empty<string>();

        public void SetGlobalInstallFilters(IGame game, string[] value)
        {
            globalInstallFilters[game.ShortName] = value;
        }

        public string?[] PreferredHosts { get; set; } = Array.Empty<string>();

        public bool? DevBuilds { get; set; }

        #pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
        #pragma warning restore CS0067

        public void Dispose()
        {
            foreach (var dir in downloadCacheDirs.OfType<TemporaryDirectory>())
            {
                dir.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
