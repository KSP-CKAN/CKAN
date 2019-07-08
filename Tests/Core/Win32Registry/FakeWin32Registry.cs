using System;
using System.Collections.Generic;
using System.Linq;
using CKAN;
using CKAN.Win32Registry;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Win32Registry
{
    public class FakeWin32Registry : IWin32Registry
    {
        public FakeWin32Registry(CKAN.KSP instance, string autostart)
            : this(
                new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("test", instance.GameDir())
                },
                autostart
            )
        {
        }

        /// <summary>
        /// Initialize the fake registry
        /// </summary>
        /// <param name="instances">List of name/path pairs for the instances</param>
        /// <param name="auto_start_instance">The auto start instance to use</param>
        public FakeWin32Registry(List<Tuple<string, string>> instances, string auto_start_instance)
        {
            Instances         = instances;
            AutoStartInstance = auto_start_instance;
            DownloadCacheDir  = TestData.NewTempDir();
        }

        /// <summary>
        /// The instances in the fake registry
        /// </summary>
        public List<Tuple<string, string>> Instances        { get; set; }
        /// <summary>
        /// Build map for the fake registry
        /// </summary>
        public string                      BuildMap         { get; set; }
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
        public int InstanceCount
        {
            get { return Instances.Count; }
        }

        // In the Win32Registry it is not possible to get null in autostart.
        private string _AutoStartInstance;
        /// <summary>
        /// The auto start instance for the fake registry
        /// </summary>
        public string AutoStartInstance
        {
            get { return _AutoStartInstance ?? string.Empty; }
            set
            {
                _AutoStartInstance = value;
            }
        }

        /// <summary>
        /// Retrieve and instance from the fake registry
        /// </summary>
        /// <param name="i">Index of the instance to retrieve</param>
        /// <returns>
        /// Name/path pair for the requested instance
        /// </returns>
        public Tuple<string, string> GetInstance(int i)
        {
            return Instances[i];
        }

        /// <summary>
        /// Set the instance data for the fake registry
        /// </summary>
        /// <param name="instances">New list of instances to use</param>
        /// <param name="autoStartInstance">Which instance to use for auto start</param>
        /// <returns>
        /// Returns
        /// </returns>
        public void SetRegistryToInstances(SortedList<string, CKAN.KSP> instances)
        {
            Instances =
                instances.Select(kvpair => new Tuple<string, string>(kvpair.Key, kvpair.Value.GameDir())).ToList();
        }

        /// <summary>
        /// The instances in the fake registry
        /// </summary>
        public IEnumerable<Tuple<string, string>> GetInstances()
        {
            return Instances;
        }

        /// <summary>
        /// The build map of the fake registry
        /// </summary>
        public string GetKSPBuilds()
        {
            return BuildMap;
        }

        /// <summary>
        /// Set the build map for the fake registry
        /// </summary>
        /// <param name="buildMap">New build map to use</param>
        public void SetKSPBuilds(string buildMap)
        {
            BuildMap = buildMap;
        }

        public IEnumerable<string> GetAuthTokenHosts ()
        {
            throw new NotImplementedException();
        }

        public void SetAuthToken (string host, string token)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAuthToken (string host, out string token)
        {
            throw new NotImplementedException();
        }
    }
}
