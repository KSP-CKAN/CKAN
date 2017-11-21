using CKAN.Versioning;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace CKAN.NetKAN.Sources.Curse
{
    public class CurseFile
    {
        [JsonConverter(typeof(JsonConvertKSPVersion))]
        [JsonProperty] public KspVersion version;
        [JsonProperty] public string name = "";
        [JsonProperty] public string type;
        [JsonProperty] public int id;
        [JsonProperty] public DateTime uploaded_at;
        [JsonProperty] public string url;

        private string _downloadUrl;
        private string _filename;
        private string _fileVersion;
        public  string ModPageUrl = "";

        /// <summary>
        /// Returns the direct path to the file
        /// </summary>
        /// <returns>
        /// The download URL
        /// </returns>
        public string GetDownloadUrl()
        {
            if (string.IsNullOrWhiteSpace(_downloadUrl))
            {
                _downloadUrl = CurseApi.ResolveRedirect(new Uri(ModPageUrl + "/files/" + id + "/download")).ToString();
            }
            return _downloadUrl;
        }

        /// <summary>
        /// Sets the download URL of the file
        /// </summary>
        public void SetDownloadUrl(string u)
        {
            _downloadUrl = u;
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
                Match match = Regex.Match(GetDownloadUrl(), "(v?[0-9][0-9a-z.]*[0-9a-z])[^0-9]*\\.zip");
                if (match.Groups.Count > 1)
                    _fileVersion = match.Groups[1].Value;
                else
                    _fileVersion = GetCurseIdVersion();
            }
            return _fileVersion;
        }

        /// <summary>
        /// Sets the version of the file
        /// </summary>
        public void SetFileVersion(string version)
        {
            _fileVersion = version;
        }

        /// <summary>
        /// Curse has versions that don't play nicely with CKAN, for example "1.1-prerelease".
        /// This transformer strips out the dash and anything after it.
        /// </summary>
        internal class JsonConvertKSPVersion : JsonConverter
        {
            public override object ReadJson(
                JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer
            )
            {
                if (reader.Value == null)
                    return null;

                string raw_version = reader.Value.ToString();

                return KspVersion.Parse(Regex.Replace(raw_version, @"-.*$", ""));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                throw new NotImplementedException();
            }
        }

    }
}
