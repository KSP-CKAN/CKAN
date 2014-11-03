using Newtonsoft.Json;
using ICSharpCode.SharpZipLib.Zip;
using CKAN;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using log4net;

namespace CKAN.NetKAN
{

    public class AVC : CkanInflator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (AVC));

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
        public static AVC FromZipFile(CkanModule module, string filename)
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
        public static AVC FromZipFile(CkanModule module, ZipFile zipfile)
        {
            log.DebugFormat("Finding AVC .version file for {0}", module);

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

            log.DebugFormat("Using AVC data from {0}", files[0].source.Name);

            // Hooray, found our entry. Extract and return it.
            using (var zipstream = zipfile.GetInputStream(files[0].source))
            using (StreamReader stream = new StreamReader(zipstream))
            {
                string json = stream.ReadToEnd();

                log.DebugFormat("Parsing {0}", json);
                return JsonConvert.DeserializeObject<AVC>(json);
            }
        }

        /// <summary>
        /// Inflates metadata with KSP Version information read from AVC.
        /// </summary>
        override public void InflateMetadata(JObject metadata, string unused_filename, object unused_context)
        {
            log.Debug("Inflating from contained AVC data...");

            // The CKAN spec states that only a KSP version can be supplied, *or* a min/max can
            // be provided. Since min/max are more descriptive, we check and use them first.
            if (ksp_version_min != null || ksp_version_max != null)
            {
                log.Debug("Inflating ksp min/max");
                metadata.Remove("ksp_version"); // In case it's there from KS
                Inflate(metadata, "ksp_version_min", ksp_version_min.ToString());
                Inflate(metadata, "ksp_version_max", ksp_version_max.ToString());
            }
            else if (ksp_version != null)
            {
                log.Debug("Inflating ksp_version");
                Inflate(metadata, "ksp_version", ksp_version.ToString());
            }

            // It's cool if we don't have version info at all, it's optional in the AVC spec.
        }
    }
}

