using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autofac;
using log4net;
using Newtonsoft.Json;
using CKAN.GameVersionProviders;
using CKAN.Versioning;

namespace CKAN.Games
{
    public class Factorio : IGame
    {
        public string ShortName => "Factorio";

        public bool GameInFolder(DirectoryInfo where)
        {
            return Directory.Exists(Path.Combine(where.FullName, "data"))
                && Directory.Exists(Path.Combine(where.FullName, "data", "base"))
                && File.Exists(Path.Combine(where.FullName, "data", "base", "info.json"));
        }

        /// <summary>
        /// Finds the Steam KSP path. Returns null if the folder cannot be located.
        /// </summary>
        /// <returns>The KSP path.</returns>
        public string SteamPath()
        {
            // Attempt to get the Steam path.
            string steamPath = CKANPathUtils.SteamPath();

            if (steamPath == null)
            {
                return null;
            }

            // Default steam library
            string installPath = FactorioDirectory(steamPath);
            if (installPath != null)
            {
                return installPath;
            }

            // Attempt to find through config file
            string configPath = Path.Combine(steamPath, "config", "config.vdf");
            if (File.Exists(configPath))
            {
                log.InfoFormat("Found Steam config file at {0}", configPath);
                StreamReader reader = new StreamReader(configPath);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Found Steam library
                    if (line.Contains("BaseInstallFolder"))
                    {
                        // This assumes config file is valid, we just skip it if it looks funny.
                        string[] split_line = line.Split('"');

                        if (split_line.Length > 3)
                        {
                            log.DebugFormat("Found a Steam Libary Location at {0}", split_line[3]);

                            installPath = FactorioDirectory(split_line[3]);
                            if (installPath != null)
                            {
                                log.InfoFormat("Found a Factorio install at {0}", installPath);
                                return installPath;
                            }
                        }
                    }
                }
            }

            // Could not locate the folder.
            return null;
        }

        /// <summary>
        /// Get the default non-Steam path to KSP on macOS
        /// </summary>
        /// <returns>
        /// "/Applications/Kerbal Space Program" if it exists and we're on a Mac, else null
        /// </returns>
        public string MacPath()
        {
            if (Platform.IsMac)
            {
                string installPath = Path.Combine(
                    // This is "/Applications" in Mono on Mac
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "factorio.app/Contents"
                );
                return Directory.Exists(installPath) ? installPath : null;
            }
            return null;
        }

        public string PrimaryModDirectoryRelative => "mods";

        public string PrimaryModDirectory(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), PrimaryModDirectoryRelative));
        }

        public string[] StockFolders   => new string[] { };
        public string[] ReservedPaths  => new string[] { };
        public string[] CreateableDirs => new string[] { "mods", "scenario" };

        /// <summary>
        /// Checks the path against a list of reserved game directories
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsReservedDirectory(GameInstance inst, string path)
        {
            return path == inst.GameDir() || path == inst.CkanDir()
                || path == PrimaryModDirectory(inst)
                || path == Scenario(inst);
        }

        public bool AllowInstallationIn(string name, out string path)
        {
            return allowedFolders.TryGetValue(name, out path);
        }

        public void RebuildSubdirectories(GameInstance inst)
        {
            string[] FoldersToCheck = { "scenario", "mods" };
            foreach (string sRelativePath in FoldersToCheck)
            {
                string sAbsolutePath = inst.ToAbsoluteGameDir(sRelativePath);
                if (!Directory.Exists(sAbsolutePath))
                    Directory.CreateDirectory(sAbsolutePath);
            }
        }

        public string DefaultCommandLine =>
                  Platform.IsWindows ? "bin/x64/Factorio.exe"
                : Platform.IsUnix    ? "bin/x64/factorio"
                : Platform.IsMac     ? "MacOS/factorio"
                :                      "usr/bin/factorio";

        public string[] AdjustCommandLine(string[] args, GameVersion installedVersion)
        {
            return args;
        }

        public List<GameVersion> KnownVersions => new List<GameVersion>();

        public GameVersion DetectVersion(DirectoryInfo where)
        {
            if (GameVersion.TryParse(
                    JsonConvert.DeserializeObject<FactorioInfoJson>(
                        File.ReadAllText(Path.Combine(
                            where.FullName, "data", "base", "info.json"
                        ))
                    ).version.ToString(),
                    out GameVersion ver
                ))
            {
                return ver;
            }
            return null;
        }

        public string CompatibleVersionsFile => "compatible_factorio_versions.json";

        public string[] BuildIDFiles => new string[]
        {
            "config-path.cfg"
        };

        public Uri DefaultRepositoryURL => new Uri("http://cfan.trakos.pl/repo/repository_v2.tar.gz");

        public Uri RepositoryListURL => new Uri("http://cfan.trakos.pl/repositories.json");

        private string Scenario(GameInstance inst)
        {
            return CKANPathUtils.NormalizePath(
                Path.Combine(inst.GameDir(), "scenario"));
        }

        private readonly Dictionary<string, string> allowedFolders = new Dictionary<string, string>
        {
            { "Scenario", "scenario" }
        };

        /// <summary>
        /// Finds the Factorio path under a Steam Library. Returns null if the folder cannot be located.
        /// </summary>
        /// <param name="steamPath">Steam Library Path</param>
        /// <returns>The Factorio path.</returns>
        private static string FactorioDirectory(string steamPath)
        {
            // There are several possibilities for the path under Linux.
            // Try with the uppercase version.
            string installPath = Path.Combine(steamPath, "SteamApps", "common", "Factorio");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }

            // Try with the lowercase version.
            installPath = Path.Combine(steamPath, "steamapps", "common", "Factorio");

            if (Directory.Exists(installPath))
            {
                return installPath;
            }
            return null;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(Factorio));
    }

    // Thanks, CFAN!
    public class FactorioInfoJson
    {
        [JsonProperty(Required = Required.Always)]
        public string name;

        [JsonProperty(Required = Required.Always)]
        public ModuleVersion version;

        public GameVersion factorio_version;

        [JsonProperty(Required = Required.Always)]
        public string title;

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(JsonSingleOrArrayConverter<string>))]
        public List<string> author;

        public string contact;

        public string homepage;

        public string description;

        public string[] dependencies;
    }

    public class FactorioAggregatorData
    {
        [JsonProperty("x-source")]
        public string XSource;

        [JsonProperty("factorio-com-id")]
        public string factorioComId;

        [JsonProperty("factorio-com-source")]
        public string factorioComSource;

        [JsonProperty("requires-factorio-token")]
        public string requiresFactorioToken;
    }

    public class FactorioModule
    {
        [JsonProperty(Required = Required.Always)]
        public FactorioInfoJson modInfo;

        public string[] categories;
        public string[] tags;
        public string[] suggests;
        public string[] recommends;
        public string[] conflicts;

        [JsonProperty(Required = Required.Always)]
        public string[] downloadUrls;

        [JsonProperty(Required = Required.AllowNull)]
        public long downloadSize;

        [JsonProperty(Required = Required.Always)]
        public string type;

        [JsonProperty(Required = Required.AllowNull)]
        public DateTime? releasedAt;

        public FactorioAggregatorData aggregatorData;

        public CkanModule ToCkan()
        {
            if (downloadUrls.Length < 1)
            {
                // No modpacks, thanks
                return null;
            }
            var module = new CkanModule(
                new ModuleVersion("v1.29"),
                Identifier.Sanitize(modInfo.name),
                modInfo.title,
                modInfo.description,
                null,
                modInfo.author,
                new List<License>() { License.UnknownLicense },
                modInfo.version,
                downloadUrls.Length > 0 && !string.IsNullOrEmpty(downloadUrls[0])
                    ? new Uri(downloadUrls[0])
                    : null
            )
            {
                ksp_version   = modInfo.factorio_version,
                download_size = downloadSize,
                Tags          = new HashSet<string>(tags),
                install       = JsonConvert.DeserializeObject<ModuleInstallDescriptor[]>(
                                    // Installing Factorio mods is super simple, just
                                    // unzip into the mods folder
                                    $"[ {{ \"find_regexp\": \"^{modInfo.name}[^/]*\", \"install_to\": \"mods\" }} ]"
                                ),
            };
            string homepage = !string.IsNullOrEmpty(modInfo.homepage)      ? modInfo.homepage
                : !string.IsNullOrEmpty(aggregatorData?.factorioComSource) ? aggregatorData.factorioComSource
                : null;
            if (!string.IsNullOrEmpty(homepage))
            {
                try {
                    module.resources = new ResourcesDescriptor()
                    {
                        homepage = new Uri(homepage)
                    };
                }
                catch
                {
                    // Some of these are "None"
                }
            }
            foreach (string relationship in modInfo.dependencies)
            {
                Match match = dependsPattern.Match(relationship);
                if (match != null && match.Success)
                {
                    if (match.Groups["identifier"].Value == "base")
                    {
                        module.ksp_version = RelVerPropertyGame(match.Groups["version"].Value);
                        module.ksp_version_min = RelVerPropertyGame(match.Groups["min_version"].Value);
                        module.ksp_version_max = RelVerPropertyGame(match.Groups["max_version"].Value);
                    }
                    else
                    {
                        var rel = new ModuleRelationshipDescriptor()
                        {
                            name        = Identifier.Sanitize(match.Groups["identifier"].Value),
                            version     = RelVerPropertyModule(match.Groups["version"].Value),
                            min_version = RelVerPropertyModule(match.Groups["min_version"].Value),
                            max_version = RelVerPropertyModule(match.Groups["max_version"].Value),
                        };
                        switch (match.Groups["prefix"].Value)
                        {
                            case "! ":
                                if (module.conflicts == null)
                                {
                                    module.conflicts = new List<RelationshipDescriptor>();
                                }
                                module.conflicts.Add(rel);
                                break;
                            case "? ":
                                if (module.recommends == null)
                                {
                                    module.recommends = new List<RelationshipDescriptor>();
                                }
                                module.recommends.Add(rel);
                                break;
                            default:
                                if (module.depends == null)
                                {
                                    module.depends = new List<RelationshipDescriptor>();
                                }
                                module.depends.Add(rel);
                                break;
                        }
                    }
                }
            }
            return module;
        }

        // C#'s generics aren't quite able to handle these together
        private GameVersion RelVerPropertyGame(string val)
        {
            return string.IsNullOrEmpty(val) ? null : GameVersion.Parse(val);
        }
        private ModuleVersion RelVerPropertyModule(string val)
        {
            return string.IsNullOrEmpty(val) ? null : new ModuleVersion(val);
        }

        private static Regex dependsPattern = new Regex(
            "^(?<prefix>[!?] )?(?<identifier>[-_. A-Za-z0-9]+)(?: (?:== (?<version>[0-9.]+))| (?:>= (?<min_version>[0-9.]+))| (?:<= (?<max_version>[0-9.]+)))*$",
            RegexOptions.Compiled
        );
    }

}
