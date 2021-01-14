using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.Versioning;
using CKAN.Extensions;
using CKAN.NetKAN.Sources.Avc;
using CKAN.Games;

namespace CKAN.NetKAN.Services
{
    internal sealed class ModuleService : IModuleService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ModuleService));

        public AvcVersion GetInternalAvc(CkanModule module, string filePath, string internalFilePath = null)
        {
            using (var zipfile = new ZipFile(filePath))
            {
                return GetInternalAvc(module, zipfile, internalFilePath);
            }
        }

        public JObject GetInternalCkan(string filePath)
        {
            using (var zipfile = new ZipFile(filePath))
            {
                // Skip everything but embedded .ckan files.
                var entries = zipfile
                    .Cast<ZipEntry>()
                    .Where(entry => entry.Name.EndsWith(".ckan",
                        StringComparison.InvariantCultureIgnoreCase));

                foreach (var entry in entries)
                {
                    Log.DebugFormat("Reading {0}", entry.Name);

                    using (var zipStream = zipfile.GetInputStream(entry))
                    {
                        return DeserializeFromStream(zipStream);
                    }
                }
            }

            return null;
        }

        public bool HasInstallableFiles(CkanModule module, string filePath)
        {
            try
            {
                ModuleInstaller.FindInstallableFiles(module, filePath,
                    new GameInstance(new KerbalSpaceProgram(), "/", "dummy", new NullUser()));
            }
            catch (BadMetadataKraken)
            {
                // TODO: DBB: Let's not use exceptions for flow control
                return false;
            }

            return true;
        }

        public IEnumerable<InstallableFile> GetConfigFiles(CkanModule module, ZipFile zip, GameInstance inst)
        {
            return GetFilesBySuffix(module, zip, ".cfg", inst);
        }

        public IEnumerable<InstallableFile> GetPlugins(CkanModule module, ZipFile zip, GameInstance inst)
        {
            return GetFilesBySuffix(module, zip, ".dll", inst);
        }

        public IEnumerable<InstallableFile> GetCrafts(CkanModule module, ZipFile zip, GameInstance ksp)
        {
            return GetFilesBySuffix(module, zip, ".craft", ksp);
        }

        private IEnumerable<InstallableFile> GetFilesBySuffix(CkanModule module, ZipFile zip, string suffix, GameInstance ksp)
        {
            return ModuleInstaller
                .FindInstallableFiles(module, zip, ksp)
                .Where(instF => instF.destination.EndsWith(suffix,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<string> FileDestinations(CkanModule module, string filePath)
        {
            var ksp = new GameInstance(new KerbalSpaceProgram(), "/", "dummy", null, false);
            return ModuleInstaller
                .FindInstallableFiles(module, filePath, ksp)
                .Where(f => !f.source.IsDirectory)
                .Select(f => ksp.ToRelativeGameDir(f.destination));
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
                gameVerMins.Add(existingMin);

            if (!GameVersion.IsNullOrAny(avcMin))
                gameVerMins.Add(avcMin);

            if (!GameVersion.IsNullOrAny(existingMax))
                gameVerMaxes.Add(existingMax);

            if (!GameVersion.IsNullOrAny(avcMax))
                gameVerMaxes.Add(avcMax);

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
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return (JObject)JToken.ReadFrom(jsonTextReader);
                }
            }
        }

        /// <summary>
        /// Locates a version file in the zipfile specified, and returns an AVC object.
        /// This requires a module object as we *first* search files we might install,
        /// falling back to a search of all files in the archive.
        ///
        /// Returns null if no version is found.
        /// Throws a Kraken if too many versions are found.
        /// </summary>
        private static AvcVersion GetInternalAvc(CkanModule module, ZipFile zipfile, string internalFilePath)
        {
            Log.DebugFormat("Finding AVC .version file for {0}", module);

            const string versionExt = ".version";

            // Get all our version files.
            var ksp = new GameInstance(new KerbalSpaceProgram(), "/", "dummy", new NullUser());
            var files = ModuleInstaller.FindInstallableFiles(module, zipfile, ksp)
                .Select(x => x.source)
                .Where(source => source.Name.EndsWith(versionExt,
                    StringComparison.InvariantCultureIgnoreCase))
                .ToList();

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
            }

            ZipEntry avcEntry = null;

            if (!string.IsNullOrWhiteSpace(internalFilePath))
            {
                Regex internalRE = new Regex(internalFilePath, RegexOptions.Compiled);
                avcEntry = files
                    .Where(f => f.Name == internalFilePath || internalRE.IsMatch(f.Name))
                    .FirstOrDefault();
                if (avcEntry == null)
                {
                    throw new Kraken(
                        string.Format("AVC: Invalid path to remote {0}, doesn't match any of: {1}",
                        internalFilePath,
                        string.Join(", ", files.Select(f => f.Name))
                    ));
                }
            }
            else if (files.Count > 1)
            {
                throw new Kraken(
                    string.Format("Too many .version files located: {0}",
                              string.Join(", ", files.Select(x => x.Name))));
            }
            else
            {
                avcEntry = files.First();
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
    }
}
