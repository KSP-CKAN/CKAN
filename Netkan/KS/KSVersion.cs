using System;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN
{
    public class KSVersion
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (KSVersion));

        // These all get filled by JSON deserialisation.
        public KSPVersion KSP_version;
        public string changelog;

        [JsonConverter(typeof(JsonConvertFromRelativeKsUri))]
        public Uri download_path;

        public Version friendly_version;
        public int id;

        public string Download(string identifier, NetFileCache cache)
        {
            log.DebugFormat("Downloading {0}", download_path);

            string filename = ModuleInstaller.CachedOrDownload(identifier, friendly_version, download_path, cache);

            log.Debug("Downloaded.");

            return filename;
        }

        /// <summary>
        /// A simple helper class to prepend KerbalStuff URLs.
        /// </summary>
        internal class JsonConvertFromRelativeKsUri : JsonConverter
        {
            public override object ReadJson(
                JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer
            )
            {
                if(reader.Value!=null)
                    return KSAPI.ExpandPath(reader.Value.ToString());
                return null;

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