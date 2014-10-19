namespace CKAN.KerbalStuff {
    using System;
    using CKAN;
    using log4net;

    public class KSVersion {
        private static readonly ILog log = LogManager.GetLogger(typeof(KSVersion));

        // These all get filled by JSON deserialisation.
        public CKAN.Version friendly_version;
        public int id;
        public string download_path;
        public KSPVersion KSP_version;
        public string changelog;

        public string Download(string identifier) {
            Uri download_url = KSAPI.ExpandPath(download_path);

            log.DebugFormat ("Downloading {0}", download_url);

            var installer = new ModuleInstaller();

            string filename = installer.CachedOrDownload (identifier, friendly_version, download_url);

            log.Debug ("Downloaded.");

            return filename;

        }
    }
}

