using System;

using Newtonsoft.Json;

namespace CKAN
{
    public class ResourcesDescriptor
    {
        [JsonProperty("homepage", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri homepage;

        [JsonProperty("spacedock", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri spacedock;

        [JsonProperty("curse", Order = 3, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri curse;

        [JsonProperty("repository", Order = 4, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri repository;

        [JsonProperty("bugtracker", Order = 5, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri bugtracker;

        [JsonProperty("discussions", Order = 6, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri discussions;

        [JsonProperty("ci", Order = 7, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri ci;

        [JsonProperty("license", Order = 8, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri license;

        [JsonProperty("manual", Order = 9, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonIgnoreBadUrlConverter))]
        public Uri manual;

        [JsonProperty("metanetkan", Order = 10, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri metanetkan;

        [JsonProperty("remote-avc", Order = 11, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri remoteAvc;

        [JsonProperty("remote-swinfo", Order = 12, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri remoteSWInfo;

        [JsonProperty("store", Order = 13, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri store;

        [JsonProperty("steamstore", Order = 14, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonOldResourceUrlConverter))]
        public Uri steamstore;
    }
}
