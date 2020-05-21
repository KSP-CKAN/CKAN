using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CKAN.NetKAN.Sources.Jenkins
{
    public class JenkinsBuild
    {
        [JsonProperty("url")]
        public string Url;

        [JsonProperty("result")]
        public string Result;

        [JsonProperty("artifacts")]
        public JenkinsArtifact[] Artifacts;

        /// <summary>
        /// Milliseconds since 1970-01-01
        /// </summary>
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixDateTimeMillisecondsConverter))]
        public DateTime? Timestamp;
    }
    
    /// <summary>
    /// UnixDateTimeConverter / 1000
    /// https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Converters/UnixDateTimeConverter.cs
    /// </summary>
    public class UnixDateTimeMillisecondsConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return UnixEpoch.AddMilliseconds((long)reader.Value);
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime dateTime)
            {
                writer.WriteValue((long)(
                    dateTime.ToUniversalTime() - UnixEpoch
                ).TotalMilliseconds);
            }
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
