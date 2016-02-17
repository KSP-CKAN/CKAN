using System;
using System.Text.RegularExpressions;
using log4net;
using Newtonsoft.Json;

namespace CKAN.NetKAN.Sources.Spacedock
{
    public class SDVersion
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SDVersion));

        // These all get filled by JSON deserialisation.

        [JsonConverter(typeof(JsonConvertKSPVersion))]
        public KSPVersion KSP_version;
        public string changelog;

        [JsonConverter(typeof(JsonConvertFromRelativeSdUri))]
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
        /// Spacedock always trims trailing zeros from a three-part version
        /// (eg: 1.0.0 -> 1.0). This means we could potentially think some mods
        /// will work with more versions than they actually will. This converter
        /// puts the .0 back on when appropriate. GH #1156.
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

                return new KSPVersion( ExpandVersionIfNeeded(raw_version) );
            }

            /// <summary>
            /// Actually expand the KSP version. It's way easier to test this than the override. :)
            /// </summary>
            public static string ExpandVersionIfNeeded(string version)
            {
                if (Regex.IsMatch(version,@"^\d+\.\d+$"))
                {
                    // Two part string, add our .0
                    return version + ".0";
                }

                return version;
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

        /// <summary>
        /// A simple helper class to prepend Spacedock URLs.
        /// </summary>
        internal class JsonConvertFromRelativeSdUri : JsonConverter
        {
            public override object ReadJson(
                JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer
            )
            {
                if(reader.Value!=null)
                    return SpacedockApi.ExpandPath(reader.Value.ToString());
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