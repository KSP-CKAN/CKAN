using Newtonsoft.Json;
using ICSharpCode.SharpZipLib.Zip;
using CKAN;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CKAN.NetKAN
{

    public class AVC
    {

        // Right now we only support KSP versioning info.

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version_min;

        [JsonConverter(typeof (JsonAvcToKspVersion))]
        public KSPVersion ksp_version_max;

        /// <summary>
        /// Locates a version file in the zipfile specified, and returns an AVC object.
        /// This requires a module object as we *only* search files we might install.
        /// </summary>
        public AVC FromZipfile(CkanModule module, string filename)
        {
            using (ZipFile zipfile = new ZipFile(filename))
            {
                return FromZipFile(module, zipfile);
            }
        }

        /// <summary>
        /// Locates a version file in the zipfile specified, and returns an AVC object.
        /// This requires a module object as we *only* search files we might install.
        /// 
        /// Returns null if no version is found.
        /// Throws a Kraken if too many versions are found.
        /// </summary>
        public AVC FromZipFile(CkanModule module, ZipFile zipfile)
        {
            // Get all our version files.
            List<InstallableFile> files = ModuleInstaller.FindInstallableFiles(module, zipfile, null)
                .Where(x => x.source.Name.EndsWith(".version")).ToList();

            if (files.Count == 0)
            {
                return null;
            }

            if (files.Count > 1)
            {
                throw new Kraken(
                    string.Format("Too may .version files located: {0}",
                              string.Join(", ",files.Select(x => x.source.Name))));
            }

            // Hooray, found our entry. Extract and return it.
            using (var zipstream = zipfile.GetInputStream(files[0].source))
            using (StreamReader stream = new StreamReader(zipstream))
            {
                return JsonConvert.DeserializeObject<AVC>(stream.ReadToEnd());
            }
        }
    }
}

