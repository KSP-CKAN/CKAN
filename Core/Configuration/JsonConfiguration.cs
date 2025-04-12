using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using CKAN.Games.KerbalSpaceProgram;

namespace CKAN.Configuration
{
    public class JsonConfiguration : IConfiguration
    {
        #region JSON structures

        [JsonConverter(typeof(ConfigConverter))]
        private class Config
        {
            public string?                      AutoStartInstance    { get; set; }
            public string?                      DownloadCacheDir     { get; set; }
            public long?                        CacheSizeLimit       { get; set; }
            public int?                         RefreshRate          { get; set; }
            public string?                      Language             { get; set; }
            public IList<GameInstanceEntry>?    GameInstances        { get; set; } = new List<GameInstanceEntry>();
            public IDictionary<string, string>? AuthTokens           { get; set; } = new Dictionary<string, string>();
            public string[]?                    GlobalInstallFilters { get; set; } = Array.Empty<string>();
            public string?[]?                   PreferredHosts       { get; set; } = Array.Empty<string>();
            public bool?                        DevBuilds            { get; set; }
        }

        public class ConfigConverter : JsonPropertyNamesChangedConverter
        {
            protected override Dictionary<string, string> mapping
                => new Dictionary<string, string>
                {
                    { "KspInstances", "GameInstances" }
                };
        }

        private class GameInstanceEntry
        {
            [JsonConstructor]
            public GameInstanceEntry(string name, string path, string game)
            {
                Name = name;
                Path = path;
                Game = game;
            }

            public string Name { get; set; }
            public string Path { get; set; }
            public string Game { get; set; }
        }

        #endregion

        /// <summary>
        /// Loads configuration from the given file, or the default path if null.
        /// </summary>
        public JsonConfiguration(string? newConfig = null)
        {
            configFile = newConfig ?? defaultConfigFile;
            LoadConfig();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // The standard location of the config file. Where this actually points is platform dependent,
        // but it's the same place as the downloads folder. The location can be overwritten with the
        // CKAN_CONFIG_FILE environment variable.
        public static readonly string defaultConfigFile =
            Environment.GetEnvironmentVariable("CKAN_CONFIG_FILE")
            ?? Path.Combine(CKANPathUtils.AppDataPath, "config.json");

        public static readonly string DefaultDownloadCacheDir =
            Path.Combine(CKANPathUtils.AppDataPath, "downloads");

        // The actual config file state and its location on the disk (we allow
        // the location to be changed for unit tests). This version is considered
        // authoritative, and we save it to the disk every time it gets changed.
        //
        // If you have multiple instances of CKAN running at the same time, each will
        // believe that their copy of the config file in memory is authoritative, so
        // changes made by one copy will not be respected by the other.
        private readonly string configFile = defaultConfigFile;
        private Config config;

        public string? DownloadCacheDir
        {
            get => config.DownloadCacheDir ?? DefaultDownloadCacheDir;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    config.DownloadCacheDir = null;
                }
                else
                {
                    if (!Path.IsPathRooted(value))
                    {
                        value = Path.GetFullPath(value);
                    }
                    config.DownloadCacheDir = value;
                }
                SaveConfig();
            }
        }

        public long? CacheSizeLimit
        {
            get => config.CacheSizeLimit;

            set
            {
                config.CacheSizeLimit = value < 0 ? null : value;
                SaveConfig();
            }
        }

        public int RefreshRate
        {
            get => config.RefreshRate ?? 0;

            set
            {
                config.RefreshRate = value <= 0 ? null : value;
                SaveConfig();
            }
        }

        public string? Language
        {
            get => config.Language;

            set
            {
                if (Utilities.AvailableLanguages.Contains(value))
                {
                    config.Language = value;
                    SaveConfig();
                }
            }
        }

        public string? AutoStartInstance
        {
            get => config.AutoStartInstance ?? "";

            set
            {
                config.AutoStartInstance = value ?? "";
                SaveConfig();
            }
        }

        public IEnumerable<Tuple<string, string, string>> GetInstances()
            => config.GameInstances?.Select(instance =>
                    new Tuple<string, string, string>(
                        instance.Name,
                        instance.Path,
                        instance.Game))
                ?? Enumerable.Empty<Tuple<string, string, string>>();

        public void SetRegistryToInstances(SortedList<string, GameInstance> instances)
        {
            config.GameInstances = instances.Select(inst => new GameInstanceEntry(inst.Key,
                                                                                  inst.Value.GameDir(),
                                                                                  inst.Value.game.ShortName))
                                            .ToList();
            SaveConfig();
        }

        public IEnumerable<string> GetAuthTokenHosts()
            => config.AuthTokens?.Keys
                                ?? Enumerable.Empty<string>();

        public bool TryGetAuthToken(string host,
                                    [NotNullWhen(returnValue: true)] out string? token)
        {
            if (config.AuthTokens == null)
            {
                token = null;
                return false;
            }
            return config.AuthTokens.TryGetValue(host, out token);
        }

        public void SetAuthToken(string host, string? token)
        {
            if (token == null || string.IsNullOrEmpty(token))
            {
                config.AuthTokens?.Remove(host);
            }
            else
            {
                if (config.AuthTokens is not null)
                {
                    config.AuthTokens[host] = token;
                }
            }
            SaveConfig();
        }

        public string[] GlobalInstallFilters
        {
            get => config.GlobalInstallFilters
                   ?? Array.Empty<string>();

            set
            {
                config.GlobalInstallFilters = value;
                SaveConfig();
                // Refresh the Contents tab
                OnPropertyChanged();
            }
        }

        public string?[] PreferredHosts
        {
            get => config.PreferredHosts
                   ?? Array.Empty<string>();

            set
            {
                config.PreferredHosts = value;
                SaveConfig();
            }
        }

        public bool? DevBuilds
        {
            get => config.DevBuilds;

            set
            {
                config.DevBuilds = value;
                SaveConfig();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // <summary>
        // Save the JSON configuration file.
        // </summary>
        private void SaveConfig()
        {
            File.WriteAllText(configFile,
                              JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        /// <summary>
        /// Load the JSON configuration file.
        ///
        /// If the configuration file does not exist, this will create it and then
        /// try to populate it with values in the registry left from the old system.
        /// </summary>
        [MemberNotNull(nameof(config))]
        private void LoadConfig()
        {
            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile))
                         ?? new Config();

                if (config.GameInstances == null)
                {
                    config.GameInstances = new List<GameInstanceEntry>();
                }
                else
                {
                    var gameName = new KerbalSpaceProgram().ShortName;
                    foreach (var e in config.GameInstances)
                    {
                        e.Game ??= gameName;
                    }
                }
                config.AuthTokens ??= new Dictionary<string, string>();
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                // This runs if the configuration does not exist
                // Ensure the directory exists
                new FileInfo(configFile).Directory?.Create();

                // Try to migrate from the real registry
                if (
                    #if NET6_0_OR_GREATER
                    Platform.IsWindows &&
                    #endif
                    Win32RegistryConfiguration.DoesRegistryConfigurationExist())
                {
                    // Try to migrate from the Windows registry
                    config = FromWindowsRegistry(new Win32RegistryConfiguration());

                    // TODO: At some point, we can uncomment this to clean up after ourselves.
                    // Win32RegistryConfiguration.DeleteAllKeys();
                }
                else
                {
                    // Create a new configuration
                    config = new Config();
                }
                SaveConfig();
            }
        }

        /// <summary>
        /// Extract the configuration from the Windows registry
        /// </summary>
        #if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
        #endif
        private static Config FromWindowsRegistry(Win32RegistryConfiguration regCfg)
            => new Config()
            {
                GameInstances     = regCfg.GetInstances()
                                          .Select(inst => new GameInstanceEntry(inst.Item1,
                                                                                inst.Item2,
                                                                                inst.Item3))
                                          .ToList(),
                AutoStartInstance = regCfg.AutoStartInstance,
                DownloadCacheDir  = regCfg.DownloadCacheDir,
                CacheSizeLimit    = regCfg.CacheSizeLimit,
                RefreshRate       = regCfg.RefreshRate,
                AuthTokens        = regCfg.GetAuthTokenHosts()
                                          .Select(host => regCfg.TryGetAuthToken(host, out string? token)
                                                              ? new KeyValuePair<string, string>(host, token)
                                                              : (KeyValuePair<string, string>?)null)
                                          .OfType<KeyValuePair<string, string>>()
                                          .ToDictionary(),
            };
    }
}
