using log4net;

namespace CKAN.NetKAN.Sources.Curse
{
    public class CurseFile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (CurseFile));

        // These all get filled by JSON deserialisation.

        public string version; //KSPVersion
        //public string changelog;

        //public Version friendly_version;

        public string name;
        public string type;
        public int id;

        //public string Download(string identifier, NetFileCache cache)
        //{
        //    log.DebugFormat("Downloading {0}", download_path);
        //
        //    string filename = ModuleInstaller.CachedOrDownload(identifier, friendly_version, download_path, cache);
        //
        //    log.Debug("Downloaded.");
        //
        //    return filename;
        //}
    }
}