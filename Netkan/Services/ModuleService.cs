using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using log4net;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.Extensions;
using CKAN.Avc;
using CKAN.SpaceWarp;
using CKAN.Games;
using CKAN.NetKAN.Sources.Github;

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

        public AvcVersion? GetInternalAvc(CkanModule module, string zipFilePath, string? internalFilePath = null)
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
        public JObject? GetInternalCkan(CkanModule module, string zipPath, GameInstance inst)
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
        private JObject? GetInternalCkan(CkanModule module, ZipFile zip, GameInstance inst)
            => (module.install != null
                    // Find embedded .ckan files that would be included in the install
                    ? GetFilesBySuffix(module, zip, ".ckan", inst)
                        .Select(instF => instF.source)
                    // Find embedded .ckan files anywhere in the ZIP
                    : zip.OfType<ZipEntry>()
                         .Where(ModuleInstaller.IsInternalCkan))
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
        /// Return a parsed JObject from a stream.
        /// Courtesy https://stackoverflow.com/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream/17788118#17788118
        /// </summary>
        private static JObject? DeserializeFromStream(Stream stream)
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
        public Tuple<ZipEntry, bool>? FindInternalAvc(CkanModule module,
                                                      ZipFile    zipfile,
                                                      string?    internalFilePath)
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
                var avcEntry = files
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
        public static AvcVersion? GetInternalAvc(ZipFile zipfile, ZipEntry? avcEntry)
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
                        exc.Message));
                }
            }
        }

        private const string SpaceWarpInfoFilename = "swinfo.json";
        private static readonly JsonSerializerSettings ignoreJsonErrors = new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Error = (sender, e) => e.ErrorContext.Handled = true
        };

        public SpaceWarpInfo? ParseSpaceWarpJson(string? json)
            => json == null ? null : JsonConvert.DeserializeObject<SpaceWarpInfo>(json, ignoreJsonErrors);

        public SpaceWarpInfo? GetInternalSpaceWarpInfo(CkanModule   module,
                                               ZipFile      zip,
                                               GameInstance inst,
                                               string?      internalFilePath = null)
            => GetInternalSpaceWarpInfos(module, zip, inst, internalFilePath).FirstOrDefault();

        private IEnumerable<SpaceWarpInfo> GetInternalSpaceWarpInfos(CkanModule   module,
                                                                     ZipFile      zip,
                                                                     GameInstance inst,
                                                                     string?      internalFilePath = null)
            => (string.IsNullOrWhiteSpace(internalFilePath)
                    ? GetFilesBySuffix(module, zip, SpaceWarpInfoFilename, inst)
                    : ModuleInstaller.FindInstallableFiles(module, zip, inst)
                                     .Where(instF => instF.source.Name == internalFilePath))
                .Select(instF => instF.source)
                .Select(entry => ParseSpaceWarpJson(new StreamReader(zip.GetInputStream(entry)).ReadToEnd()))
                .OfType<SpaceWarpInfo>();

        public SpaceWarpInfo? GetSpaceWarpInfo(CkanModule   module,
                                               ZipFile      zip,
                                               GameInstance inst,
                                               IGithubApi   githubApi,
                                               IHttpService httpSvc,
                                               string?      internalFilePath = null)
            => GetInternalSpaceWarpInfos(module, zip, inst, internalFilePath)
               .Select(swinfo => swinfo.version_check != null
                                 && Uri.IsWellFormedUriString(swinfo.version_check.OriginalString, UriKind.Absolute)
                                 && ParseSpaceWarpJson(githubApi?.DownloadText(swinfo.version_check)
                                                                ?? httpSvc.DownloadText(swinfo.version_check))
                                    is SpaceWarpInfo remoteSwinfo
                                 && remoteSwinfo.version == swinfo.version
                                     ? remoteSwinfo
                                     : swinfo)
               .FirstOrDefault();
    }
}
