using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    /// <summary>
    /// Base class for a class-level converter that transfers values from
    /// the old name for a property to its new name.
    /// Inherit, then override the mapping property to specify the renamings.
    /// </summary>
    public abstract class JsonPropertyNamesChangedConverter : JsonConverter
    {
        /// <summary>
        /// We don't want to make any changes during serialization
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// We don't want to make any changes during serialization
        /// </summary>
        /// <param name="writer">The object writing JSON to disk</param>
        /// <param name="value">A value to be written for this class</param>
        /// <param name="serializer">Generates output objects from tokens</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// We only want to convert classes, not properties
        /// </summary>
        /// <param name="objectType">Type where this class been used as a JsonConverter</param>
        /// <returns>true if it's a class, false otherwise</returns>
        public override bool CanConvert(Type objectType) => objectType.GetTypeInfo().IsClass;

        /// <summary>
        /// Parse JSON to an object, renaming properties according to the mapping property
        /// </summary>
        /// <param name="reader">Object that provides tokens to be translated</param>
        /// <param name="objectType">The output type to be populated</param>
        /// <param name="existingValue">Not used</param>
        /// <param name="serializer">Generates output objects from tokens</param>
        /// <returns>Class object populated according to the renaming scheme</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object instance = Activator.CreateInstance(objectType);
            JObject jo = JObject.Load(reader);
            var changes = mapping;
            foreach (JProperty jp in jo.Properties())
            {
                if (!changes.TryGetValue(jp.Name, out string name))
                {
                    name = jp.Name;
                }
                PropertyInfo prop = objectType.GetTypeInfo().DeclaredProperties.FirstOrDefault(pi =>
                    pi.CanWrite
                    && (pi.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? pi.Name) == name);
                if (prop != null)
                {
                    prop.SetValue(instance,
                                  GetValue(prop.GetCustomAttribute<JsonConverterAttribute>(),
                                           jp.Value, prop.PropertyType, serializer));
                }
                else
                {
                    // No property, maybe there's a field
                    FieldInfo field = objectType.GetTypeInfo().DeclaredFields.FirstOrDefault(fi =>
                        (fi.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? fi.Name) == name);
                    field?.SetValue(instance,
                                    GetValue(field.GetCustomAttribute<JsonConverterAttribute>(),
                                             jp.Value, field.FieldType, serializer));
                }
            }
            return instance;
        }

        private static object GetValue(JsonConverterAttribute attrib,
                                       JToken value, Type outputType, JsonSerializer serializer)
            => attrib != null ? ApplyConverter((JsonConverter)Activator.CreateInstance(attrib.ConverterType,
                                                                                       attrib.ConverterParameters),
                                               value, outputType, serializer)
                              : value.ToObject(outputType, serializer);

        private static object ApplyConverter(JsonConverter converter,
                                             JToken value, Type outputType, JsonSerializer serializer)
            => converter.CanRead ? converter.ReadJson(new JTokenReader(value),
                                                      outputType, null, serializer)
                                 : value.ToObject(outputType, serializer);

        /// <summary>
        /// This is what you need to override in your child class
        /// </summary>
        /// <value>Mapping from old names to new names</value>
        protected abstract Dictionary<string, string> mapping
        {
            get;
        }
    }
}
