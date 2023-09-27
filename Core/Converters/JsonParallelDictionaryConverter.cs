using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CKAN.Extensions;

namespace CKAN
{
    /// <summary>
    /// A converter that loads a dictionary in parallel,
    /// use with large collections of complex objects for a speed boost
    /// </summary>
    public class JsonParallelDictionaryConverter<V> : JsonConverter
    {
        public override object ReadJson(JsonReader     reader,
                                        Type           objectType,
                                        object         existingValue,
                                        JsonSerializer serializer)
            => ParseWithProgress(JObject.Load(reader)
                                        .Properties()
                                        .ToArray(),
                                 serializer);

        private object ParseWithProgress(JProperty[]    properties,
                                         JsonSerializer serializer)
            => Partitioner.Create(properties, true)
                          .AsParallel()
                          .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                          .Select(prop => new KeyValuePair<string, V>(
                                              prop.Name,
                                              prop.Value.ToObject<V>()))
                          .WithProgress(properties.Length,
                                        serializer.Context.Context as IProgress<int>)
                          .ToDictionary();

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        // Only convert when we're an explicit attribute
        public override bool CanConvert(Type object_type) => false;
    }
}
