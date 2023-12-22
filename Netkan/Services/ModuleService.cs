using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Extensions;
using CKAN.Versioning;
using CKAN.NetKAN.Sources.Avc;
using CKAN.NetKAN.Sources.SpaceWarp;
using CKAN.Games;

namespace CKAN.NetKAN.Services
{
    internal sealed class ModuleService : IModuleService
    {
        public ModuleService(IGame game)
        {
            this.game = game;
        }

        private readonly IGame game;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ModuleService));

        public AvcVersion GetInternalAvc(CkanModule module, string zipFilePath, string internalFilePath = null)
        {
            using (var zipfile = new ZipFile(zipFilePath))
            {
                return GetInternalAvc(zipfile, FindInternalAvc(module, zipfile, internalFilePath)?.Item1);
            }
        }

        /// <summary>
        /// Find and parse a .ckan file in a ZIP.
        /// If the module has an `install` property, only files that would
        /// be installed are considered. Otherwise the whole ZIP is searched.
        /// </summary>
        /// <param name="module">The CkanModule associated with the ZIP, so we can tell which files would be installed</param>
        /// <param name="zipPath">Where the ZIP file is</param>
        /// <param name="inst">Game instance for generating InstallableFiles</param>
        /// <returns>Parsed contents of the file, or null if none found</returns>
        public JObject GetInternalCkan(CkanModule module, string zipPath, GameInstance inst)
            => GetInternalCkan(module, new ZipFile(zipPath), inst);

        /// <summary>
        /// Find and parse a .ckan file in the ZIP.
        /// If the module has an `install` property, only files that would
        /// be installed are considered. Otherwise the whole ZIP is searched.
        /// </summary>
        /// <param name="module">The CkanModule associated with the ZIP, so we can tell which files would be installed</param>
        /// <param name="zip">The ZipFile to search</param>
        /// <param name="inst">Game instance for generating InstallableFiles</param>
        /// <returns>Parsed contents of the file, or null if none found</returns>
        private JObject GetInternalCkan(CkanModule module, ZipFile zip, GameInstance inst)
            => (module.install != null
                    // Find embedded .ckan files that would be included in the install
                    ? GetFilesBySuffix(module, zip, ".ckan", inst)
                        .Select(instF => instF.source)
                    // Find embedded .ckan files anywhere in the ZIP
                    : zip.Cast<ZipEntry>()
                        .Where(entry => entry.Name.EndsWith(".ckan", StringComparison.InvariantCultureIgnoreCase)))
                .Select(entry => DeserializeFromStream(
                                    zip.GetInputStream(entry)))
                .FirstOrDefault();

        public bool HasInstallableFiles(CkanModule module, string filePath)
        {
            try
            {
                ModuleInstaller.FindInstallableFiles(module, filePath,
                    new GameInstance(game, "/", "dummy", new NullUser()));
            }
            catch (BadMetadataKraken)
            {
                // TODO: DBB: Let's not use exceptions for flow control
                return false;
            }

            return true;
        }

        public IEnumerable<InstallableFile> GetConfigFiles(CkanModule module, ZipFile zip, GameInstance inst)
            => GetFilesBySuffix(module, zip, ".cfg", inst);

        public IEnumerable<InstallableFile> GetPlugins(CkanModule module, ZipFile zip, GameInstance inst)
            => GetFilesBySuffix(module, zip, ".dll", inst);

        public IEnumerable<InstallableFile> GetCrafts(CkanModule module, ZipFile zip, GameInstance inst)
            => GetFilesBySuffix(module, zip, ".craft", inst);

        private IEnumerable<InstallableFile> GetFilesBySuffix(CkanModule module, ZipFile zip, string suffix, GameInstance inst)
            => ModuleInstaller.FindInstallableFiles(module, zip, inst)
                              .Where(instF => instF.destination.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase));

        public IEnumerable<ZipEntry> FileSources(CkanModule module, ZipFile zip, GameInstance inst)
            => ModuleInstaller.FindInstallableFiles(module, zip, inst)
                              .Select(instF => instF.source)
                              .Where(ze => !ze.IsDirectory);

        public IEnumerable<string> FileDestinations(CkanModule module, string filePath)
        {
            var inst = new GameInstance(game, "/", "dummy", null);
            return ModuleInstaller
                .FindInstallableFiles(module, filePath, inst)
                .Where(f => !f.source.IsDirectory)
                .Select(f => inst.ToRelativeGameDir(f.destination));
        }

        /// <summary>
        /// Update the game versions of a module.
        /// Final range will be the union of the previous and new ranges.
        /// Note that this means we always increase, never decrease, compatibility.
        /// </summary>
        /// <param name="json">The module being inflated</param>
        /// <param name="ver">The single game version</param>
        /// <param name="minVer">The minimum game version</param>
        /// <param name="maxVer">The maximum game version</param>
        public static void ApplyVersions(JObject json, GameVersion ver, GameVersion minVer, GameVersion maxVer)
        {
            // Get the minimum and maximum game versions that already exist in the metadata.
            // Use specific game version if min/max don't exist.
            var existingMinStr = (string)json["ksp_version_min"] ?? (string)json["ksp_version"];
            var existingMaxStr = (string)json["ksp_version_max"] ?? (string)json["ksp_version"];

            var existingMin = existingMinStr == null ? null : GameVersion.Parse(existingMinStr);
            var existingMax = existingMaxStr == null ? null : GameVersion.Parse(existingMaxStr);

            GameVersion avcMin, avcMax;
            if (minVer == null && maxVer == null)
            {
                // Use specific game version if min/max don't exist
                avcMin = avcMax = ver;
            }
            else
            {
                avcMin = minVer;
                avcMax = maxVer;
            }

            // Now calculate the minimum and maximum KSP versions between both the existing metadata and the
            // AVC file.
            var gameVerMins  = new List<GameVersion>();
            var gameVerMaxes = new List<GameVersion>();

            if (!GameVersion.IsNullOrAny(existingMin))
            {
                gameVerMins.Add(existingMin);
            }

            if (!GameVersion.IsNullOrAny(avcMin))
            {
                gameVerMins.Add(avcMin);
            }

            if (!GameVersion.IsNullOrAny(existingMax))
            {
                gameVerMaxes.Add(existingMax);
            }

            if (!GameVersion.IsNullOrAny(avcMax))
            {
                gameVerMaxes.Add(avcMax);
            }

            var gameVerMin = gameVerMins.DefaultIfEmpty(null).Min();
            var gameVerMax = gameVerMaxes.DefaultIfEmpty(null).Max();

            if (gameVerMin != null || gameVerMax != null)
            {
                // If we have either a minimum or maximum game version, remove all existing game version
                // information from the metadata.
                json.Remove("ksp_version");
                json.Remove("ksp_version_min");
                json.Remove("ksp_version_max");

                if (gameVerMin != null && gameVerMax != null)
                {
                    // If we have both a minimum and maximum game version...
                    if (gameVerMin.Equals(gameVerMax))
                    {
                        // ...and they are equal, then just set ksp_version
                        Log.DebugFormat("Min and max game versions are same, setting ksp_version");
                        json["ksp_version"] = gameVerMin.ToString();
                    }
                    else
                    {
                        // ...otherwise set both ksp_version_min and ksp_version_max
                        Log.DebugFormat("Min and max game versions are different, setting both");
                        json["ksp_version_min"] = gameVerMin.ToString();
                        json["ksp_version_max"] = gameVerMax.ToString();
                    }
                }
                else
                {
                    // If we have only one or the other then set which ever is applicable
                    if (gameVerMin != null)
                    {
                        Log.DebugFormat("Only min game version is set");
                        json["ksp_version_min"] = gameVerMin.ToString();
                    }
                    if (gameVerMax != null)
                    {
                        Log.DebugFormat("Only max game version is set");
                        json["ksp_version_max"] = gameVerMax.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Return a parsed JObject from a stream.
        /// Courtesy https://stackoverflow.com/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream/17788118#17788118
        /// </summary>
        private static JObject DeserializeFromStream(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                // Only one document per internal .ckan
                return YamlExtensions.Parse(sr).FirstOrDefault()?.ToJObject();
            }
        }

        /// <summary>
        /// Locate a version file in an archive.
        /// This requires a module object as we *first* search files we might install,
        /// falling back to a search of all files in the archive.
        /// Returns null if no version is found.
        /// Throws a Kraken if too many versions are found.
        /// </summary>
        /// <param name="module">The metadata associated with this module, used to find installable files</param>
        /// <param name="zipfile">The archive containing the module's files</param>
        /// <param name="internalFilePath">Filter for selecting a version file, either exact match or regular expression</param>
        /// <returns>
        /// Tuple consisting of the chosen file's entry in the archive plus a boolean
        /// indicating whether it's a file would be extracted to disk at installation
        /// </returns>
        public Tuple<ZipEntry, bool> FindInternalAvc(CkanModule module, ZipFile zipfile, string internalFilePath)
        {
            Log.DebugFormat("Finding AVC .version file for {0}", module);

            const string versionExt = ".version";

            // Get all our version files
            var ksp = new GameInstance(game, "/", "dummy", new NullUser());
            var files = ModuleInstaller.FindInstallableFiles(module, zipfile, ksp)
                .Select(x => x.source)
                .Where(source => source.Name.EndsWith(versionExt,
                    StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            // By default, we look for ones we can install
            var installable = true;

            if (files.Count == 0)
            {
                // Oh dear, no version file at all? Let's see if we can find *any* to use.
                files.AddRange(zipfile.Cast<ZipEntry>()
                    .Where(file => file.Name.EndsWith(versionExt,
                        StringComparison.InvariantCultureIgnoreCase)));

                if (files.Count == 0)
                {
                    // Okay, there's *really* nothing there.
                    return null;
                }
                // Tell calling code that it may not be a "real" version file
                installable = false;
            }

            if (!string.IsNullOrWhiteSpace(internalFilePath))
            {
                Regex internalRE = new Regex(internalFilePath, RegexOptions.Compiled);
                ZipEntry avcEntry = files
                    .FirstOrDefault(f => f.Name == internalFilePath || internalRE.IsMatch(f.Name));
                if (avcEntry == null)
                {
                    throw new Kraken(
                        string.Format("AVC: Invalid path to remote {0}, doesn't match any of: {1}",
                        internalFilePath,
                        string.Join(", ", files.Select(f => f.Name))));
                }
                return new Tuple<ZipEntry, bool>(avcEntry, installable);
            }
            else if (files.Count > 1)
            {
                throw new Kraken(
                    string.Format("Too many .version files located: {0}",
                              string.Join(", ", files.Select(x => x.Name).OrderBy(f => f))));
            }
            else
            {
                return new Tuple<ZipEntry, bool>(files.First(), installable);
            }
        }

        /// <summary>
        /// Returns an AVC object for the given file in the archive, if any.
        /// </summary>
        public static AvcVersion GetInternalAvc(ZipFile zipfile, ZipEntry avcEntry)
        {
            if (avcEntry == null)
            {
                return null;
            }
            Log.DebugFormat("Using AVC data from {0}", avcEntry.Name);

            // Hooray, found our entry. Extract and return it.
            using (var zipstream = zipfile.GetInputStream(avcEntry))
            using (var stream = new StreamReader(zipstream))
            {
                var json = stream.ReadToEnd();

                Log.DebugFormat("Parsing {0}", json);
                try
                {
                    return JsonConvert.DeserializeObject<AvcVersion>(json);
                }
                catch (JsonException exc)
                {
                    throw new Kraken(string.Format(
                        "Error parsing version file {0}: {1}",
                        avcEntry.Name,
                        exc.Message
                    ));
                }
            }
        }

        private const string SpaceWarpInfoFilename = "swinfo.json";
        private static readonly JsonSerializerSettings ignoreJsonErrors = new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Error = (sender, e) => e.ErrorContext.Handled = true
        };

        public SpaceWarpInfo ParseSpaceWarpJson(string json)
            => JsonConvert.DeserializeObject<SpaceWarpInfo>(json, ignoreJsonErrors);

        public SpaceWarpInfo GetSpaceWarpInfo(CkanModule module, ZipFile zip, GameInstance inst, string internalFilePath = null)
            => (string.IsNullOrWhiteSpace(internalFilePath)
                    ? GetFilesBySuffix(module, zip, SpaceWarpInfoFilename, inst)
                    : ModuleInstaller.FindInstallableFiles(module, zip, inst)
                        .Where(instF => instF.source.Name == internalFilePath))
                .Select(instF => instF.source)
                .Select(entry => ParseSpaceWarpJson(
                    new StreamReader(zip.GetInputStream(entry)).ReadToEnd()))
                .FirstOrDefault();
    }
}
