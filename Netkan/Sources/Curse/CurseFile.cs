using log4net;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace CKAN.NetKAN.Sources.Curse
{
    public class CurseFile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (CurseFile));

        internal CurseMod Mod;

        [JsonProperty] public string version; // KSP Version

        //[JsonProperty] public string changelog;

        //[JsonProperty] public Version friendly_version;

        [JsonProperty] public string name;
        [JsonProperty] public string type;
        [JsonProperty] public int id;

        private string _downloadUrl;
        private string _filename;
        private string _fileVersion;

        /// <summary>
        /// Returns the direct path to the file
        /// </summary>
        /// <returns>The download url</returns>
        public String GetDownloadUrl()
        {
            if (_downloadUrl == null)
            {
                _downloadUrl = CurseApi.ResolveRedirect(new Uri(Mod.GetPageUrl() + "/files/" + id + "/download")).ToString();
            }
            return _downloadUrl;
        }

        /// <summary>
        /// Returns the Curse Id version of the file
        /// </summary>
        /// <returns>The Curse Id version</returns>
        public string GetCurseIdVersion()
        {
            return "0curse" + id;
        }

        /// <summary>
        /// Returns the filename of the file
        /// </summary>
        /// <returns>The filename</returns>
        public string GetFilename()
        {
            if (_filename == null)
            {
                Match match = Regex.Match(GetDownloadUrl(), "[^/]*\\.zip");
                if (match.Groups.Count > 0) _filename = match.Groups[0].Value;
                else _filename = GetCurseIdVersion();
            }
            return _filename;
        }

        /// <summary>
        /// Returns the version of the file
        /// </summary>
        /// <returns>The version</returns>
        public string GetFileVersion()
        {
            if (_fileVersion == null)
            {
                // Matches the last group of numbers letters and dots before the .zip extension, staring at a number or a 'v' and a number
                Match match = Regex.Match(GetDownloadUrl(), "(v?[0-9][0-9a-z.]*[0-9a-z])[^0-9]*\\.zip");
                if (match.Groups.Count > 1) _fileVersion = match.Groups[1].Value;

                // The id is unique across all files, and is always incrementing.
                // This format also assures, that any "real" version precedes them.
                else _fileVersion = GetCurseIdVersion();
            }
            return _fileVersion;
        }

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