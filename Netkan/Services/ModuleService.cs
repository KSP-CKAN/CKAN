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
using CKAN.NetKAN.Sources.Avc;

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
                ModuleInstaller.FindInstallableFiles(module, filePath, null);
            }
            catch (BadMetadataKraken)
            {
                // TODO: DBB: Let's not use exceptions for flow control
                return false;
            }

            return true;
        }

        public IEnumerable<InstallableFile> GetConfigFiles(CkanModule module, ZipFile zip, KSP ksp)
        {
            return GetFilesBySuffix(module, zip, ".cfg", ksp);
        }

        public IEnumerable<InstallableFile> GetPlugins(CkanModule module, ZipFile zip, KSP ksp)
        {
            return GetFilesBySuffix(module, zip, ".dll", ksp);
        }

        public IEnumerable<InstallableFile> GetCrafts(CkanModule module, ZipFile zip, KSP ksp)
        {
            return GetFilesBySuffix(module, zip, ".craft", ksp);
        }

        private IEnumerable<InstallableFile> GetFilesBySuffix(CkanModule module, ZipFile zip, string suffix, KSP ksp)
        {
            return ModuleInstaller
                .FindInstallableFiles(module, zip, ksp)
                .Where(instF => instF.destination.EndsWith(suffix,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<string> FileDestinations(CkanModule module, string filePath)
        {
            var ksp = new KSP("/", "dummy", null, false);
            return ModuleInstaller
                .FindInstallableFiles(module, filePath, ksp)
                .Where(f => !f.source.IsDirectory)
                .Select(f => ksp.ToRelativeGameDir(f.destination));
        }

        /// <summary>
        /// Return a parsed JObject from a stream.
        /// </summary>

        // Courtesy https://stackoverflow.com/questions/8157636/can-json-net-serialize-deserialize-to-from-a-stream/17788118#17788118
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
            var files = ModuleInstaller.FindInstallableFiles(module, zipfile, null)
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
