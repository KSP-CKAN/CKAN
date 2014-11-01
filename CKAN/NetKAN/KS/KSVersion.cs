using System;
using System.Runtime.Serialization;
using log4net;

namespace CKAN.NetKAN
{
    public class KSVersion
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (KSVersion));

        // These all get filled by JSON deserialisation.
        public KSPVersion KSP_version;
        public string changelog;
        public string download_path;
        public Version friendly_version;
        public int id;

        [OnDeserialized]
        private void DeSerialisationFixes(StreamingContext like_i_could_care)
        {
            // Turn our download path into a fully qualified URL.
            download_path = KSAPI.ExpandPath(download_path).ToString();

            log.DebugFormat("Download path is {0}", download_path);
        }

        public string Download(string identifier, Cache cache)
        {
            log.DebugFormat("Downloading {0}", download_path);

            string filename = ModuleInstaller.CachedOrDownload(identifier, friendly_version, new Uri(download_path), cache);

            log.Debug("Downloaded.");

            return filename;
        }
    }
}