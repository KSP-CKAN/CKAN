using System;
using System.Collections.Generic;

using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    /// <summary>
    /// [De]serializes a dictionary that might have some questionably
    /// valid data in it.
    /// If exceptions are thrown for any key/value pair, leave it out.
    /// Removes CkanModule objects from AvailableModule.module_version
    /// if License throws BadMetadataKraken.
    /// </summary>
    public class JsonLeakySortedDictionaryConverter<K, V> : JsonConverter where K: class where V: class
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var dict = new SortedDictionary<K, V>();
            foreach (var kvp in JObject.Load(reader))
            {
                try
                {
                    if (Activator.CreateInstance(typeof(K), kvp.Key) is K k
                        && kvp.Value?.ToObject<V>() is V v)
                    {
                        dict.Add(k, v);
                    }
                }
                catch (Exception exc)
                {
                    log.Warn($"Failed to deserialize {kvp.Key}: {kvp.Value}", exc);
                }
            }
            return dict;
        }

        /// <summary>
        /// Use default serializer for writing
        /// </summary>
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// We *only* want to be triggered for types that have explicitly
        /// set an attribute in their class saying they can be converted.
        /// By returning false here, we declare we're not interested in participating
        /// in any other conversions.
        /// </summary>
        /// <returns>
        /// false
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(JsonLeakySortedDictionaryConverter<K, V>));
    }
}
