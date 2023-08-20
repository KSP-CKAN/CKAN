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
            var props = objectType.GetTypeInfo().DeclaredProperties.ToList();
            JObject jo = JObject.Load(reader);
            var changes = mapping;
            foreach (JProperty jp in jo.Properties())
            {
                string name;
                if (!changes.TryGetValue(jp.Name, out name))
                {
                    name = jp.Name;
                }
                PropertyInfo prop = props.FirstOrDefault(pi => pi.CanWrite && (
                    pi.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == name
                    || pi.Name == name));
                prop?.SetValue(instance, jp.Value.ToObject(prop.PropertyType, serializer));
            }
            return instance;
        }

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
