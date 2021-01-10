using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    public abstract class JsonPropertyNamesChangedConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsClass;
        }

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

        // This is what you need to override in the child class
        protected abstract Dictionary<string, string> mapping
        {
            get;
        }
    }
}
