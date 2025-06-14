using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using CKAN.IO;
using CKAN.Extensions;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram;

namespace CKAN.Configuration
{
    public class JsonConfiguration : IConfiguration
    {
        #region JSON structures

        [JsonObject(MemberSerialization   = MemberSerialization.OptOut,
                    ItemNullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ConfigConverter))]
        private class Config
        {
            public string?                       AutoStartInstance    { get; set; }
            public string?                       DownloadCacheDir     { get; set; }
            public long?                         CacheSizeLimit       { get; set; }
            public int?                          RefreshRate          { get; set; }
            public string?                       Language             { get; set; }
            public IList<GameInstanceEntry>?     GameInstances        { get; set; } = new List<GameInstanceEntry>();
            public IDictionary<string, string>?  AuthTokens           { get; set; } = new Dictionary<string, string>();
            [JsonProperty("GlobalInstallFiltersByGame")]
            [JsonConverter(typeof(JsonToGamesDictionaryConverter))]
            public Dictionary<string, string[]>? GlobalInstallFilters { get; set; } = new Dictionary<string, string[]>();
            public string?[]?                    PreferredHosts       { get; set; } = Array.Empty<string>();
            public bool?                         DevBuilds            { get; set; }
        }

        /// <summary>
        /// Protect old clients from trying to load a file they can't parse
        /// </summary>
        private class ConfigConverter : JsonPropertyNamesChangedConverter
        {
            protected override Dictionary<string, string> mapping
                => new Dictionary<string, string>
                {
                    { "KspInstances",         "GameInstances" },
                    { "GlobalInstallFilters", "GlobalInstallFiltersByGame" },
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
            if (token is {Length: > 0})
            {
                config.AuthTokens ??= new Dictionary<string, string>();
                config.AuthTokens[host] = token;
            }
            else if (config.AuthTokens != null
                     && config.AuthTokens.ContainsKey(host))
            {
                if (config.AuthTokens.Count > 1)
                {
                    config.AuthTokens.Remove(host);
                }
                else
                {
                    config.AuthTokens = null;
                }
            }
            else
            {
                // No changes needed, skip saving
                return;
            }
            SaveConfig();
        }

        public string[] GetGlobalInstallFilters(IGame game)
            => config.GlobalInstallFilters?.GetOrDefault(game.ShortName)
                                          ?? Array.Empty<string>();

        public void SetGlobalInstallFilters(IGame game, string[] value)
        {
            if (value.Length > 0)
            {
                // Set the list for this game
                config.GlobalInstallFilters ??= new Dictionary<string, string[]>();
                config.GlobalInstallFilters[game.ShortName] = value;
            }
            else if (config.GlobalInstallFilters != null
                     && config.GlobalInstallFilters.ContainsKey(game.ShortName))
            {
                if (config.GlobalInstallFilters.Count > 1)
                {
                    // Purge this game's entry
                    config.GlobalInstallFilters.Remove(game.ShortName);
                }
                else
                {
                    // Discard empty dictionary
                    config.GlobalInstallFilters = null;
                }
            }
            else
            {
                // No changes needed, skip saving and notifications
                return;
            }
            SaveConfig();
            // Refresh the Contents tab
            OnPropertyChanged();
        }

        public string?[] PreferredHosts
        {
            get => config.PreferredHosts ?? Array.Empty<string>();

            set
            {
                config.PreferredHosts = value is {Length: > 0} ? value : null;
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
            JsonConvert.SerializeObject(config, Formatting.Indented)
                       .WriteThroughTo(configFile);
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
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                // This runs if the configuration does not exist
                // Ensure the directory exists
                new FileInfo(configFile).Directory?.Create();

                // Create a new configuration
                config = new Config();

                SaveConfig();
            }
            if (
                #if NET6_0_OR_GREATER
                Platform.IsWindows &&
                #endif
                Win32RegistryConfiguration.DoesRegistryConfigurationExist())
            {
                // Clean up very old Windows registry keys
                Win32RegistryConfiguration.DeleteAllKeys();
            }
        }
    }
}
