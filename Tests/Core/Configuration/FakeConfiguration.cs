using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CKAN.Configuration;
using CKAN.Games.KerbalSpaceProgram.GameVersionProviders;

using Tests.Data;

namespace Tests.Core.Configuration
{
    public class FakeConfiguration : IConfiguration, IDisposable
    {
        public FakeConfiguration(CKAN.GameInstance instance, string autostart)
            : this(new List<Tuple<string, string, string>>
                   {
                       new Tuple<string, string, string>("test", instance.GameDir(), "KSP")
                   },
                   autostart)
        {
        }

        /// <summary>
        /// Initialize the fake registry
        /// </summary>
        /// <param name="instances">List of name/path pairs for the instances</param>
        /// <param name="auto_start_instance">The auto start instance to use</param>
        public FakeConfiguration(List<Tuple<string, string, string>> instances, string auto_start_instance)
        {
            Instances         = instances;
            AutoStartInstance = auto_start_instance;
            DownloadCacheDir  = TestData.NewTempDir();
        }

        /// <summary>
        /// The instances in the fake registry
        /// </summary>
        public List<Tuple<string, string, string>> Instances { get; set; }
        /// <summary>
        /// Build map for the fake registry
        /// </summary>
        public JBuilds                     BuildMap         { get; set; }
        /// <summary>
        /// Path to download cache folder for the fake registry
        /// </summary>
        public string                      DownloadCacheDir { get; set; }
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
        private string _AutoStartInstance;

        /// <summary>
        /// The auto start instance for the fake registry
        /// </summary>
        public string AutoStartInstance
        {
            get => _AutoStartInstance ?? string.Empty;
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
        public void SetRegistryToInstances(SortedList<string, CKAN.GameInstance> instances)
        {
            Instances =
                instances.Select(kvpair => new Tuple<string, string, string>(kvpair.Key, kvpair.Value.GameDir(), "KSP")).ToList();
        }

        /// <summary>
        /// The instances in the fake registry
        /// </summary>
        public IEnumerable<Tuple<string, string, string>> GetInstances() => Instances;

        /// <summary>
        /// The build map of the fake registry
        /// </summary>
        public JBuilds GetKSPBuilds() => BuildMap;

        /// <summary>
        /// Set the build map for the fake registry
        /// </summary>
        /// <param name="buildMap">New build map to use</param>
        public void SetKSPBuilds(JBuilds buildMap)
        {
            BuildMap = buildMap;
        }

        public IEnumerable<string> GetAuthTokenHosts()
        {
            throw new NotImplementedException();
        }

        public void SetAuthToken(string host, string token)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAuthToken(string host, out string token)
        {
            throw new NotImplementedException();
        }

        private string _Language;
        public string Language
        {
            get => _Language;

            set
            {
                if (CKAN.Utilities.AvailableLanguages.Contains(value))
                {
                    _Language = value;
                }
            }
        }

        public string[] GlobalInstallFilters { get; set; } = Array.Empty<string>();

        public string[] PreferredHosts { get; set; } = Array.Empty<string>();

        public bool? DevBuilds { get; set; }

        public void Dispose()
        {
            Directory.Delete(DownloadCacheDir, true);
        }
    }
}
