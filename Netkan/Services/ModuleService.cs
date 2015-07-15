using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CKAN.NetKAN.Sources.Avc;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    .Where(entry => Regex.IsMatch(entry.Name, ".CKAN$", RegexOptions.IgnoreCase));

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
                .Where(source => source.Name.EndsWith(versionExt))
                .ToList();

            if (files.Count == 0)
            {
                // Oh dear, no version file at all? Let's see if we can find *any* to use.
                var versionFiles = zipfile.Cast<ZipEntry>().Where(file => file.Name.EndsWith(versionExt));
                files.AddRange(versionFiles);

                // Okay, there's *really* nothing there.
                if (files.Count == 0)
                {
                    return null;
                }
            }

            var remoteIndex = 0;

            if (!string.IsNullOrWhiteSpace(internalFilePath))
            {
                remoteIndex = -1;

                for (var i = 0; i < files.Count; i++)
                {
                    if (files[i].Name == internalFilePath)
                    {
                        remoteIndex = i;
                        break;
                    }
                }

                if (remoteIndex == -1)
                {
                    var remotes = files.Aggregate("", (current, file) => current + (file.Name + ", "));

                    throw new Kraken(string.Format("AVC: Invalid path to remote {0}, doesn't match any of: {1}",
                        internalFilePath,
                        remotes
                    ));
                }
            }
            else if (files.Count > 1)
            {
                throw new Kraken(
                    string.Format("Too may .version files located: {0}",
                              string.Join(", ", files.Select(x => x.Name))));
            }

            Log.DebugFormat("Using AVC data from {0}", files[remoteIndex].Name);

            // Hooray, found our entry. Extract and return it.
            using (var zipstream = zipfile.GetInputStream(files[remoteIndex]))
            using (var stream = new StreamReader(zipstream))
            {
                var json = stream.ReadToEnd();

                Log.DebugFormat("Parsing {0}", json);
                return JsonConvert.DeserializeObject<AvcVersion>(json);
            }
        }
    }
}
