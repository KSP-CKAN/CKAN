using System;
using System.Linq;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    public class JsonRelationshipConverter : JsonConverter
    {
        public override bool CanConvert(Type object_type)
        {
            // Only convert when we're an explicit attribute
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                List<RelationshipDescriptor> rels = new List<RelationshipDescriptor>();
                foreach (JObject child in token.Children<JObject>())
                {
                    if (child["any_of"] != null)
                    {
                        // Catch confused/invalid metadata
                        if (child.Properties().Count() > 1)
                        {
                            throw new Kraken("`any_of` should not be combined with other properties");
                        }
                        rels.Add(child.ToObject<AnyOfRelationshipDescriptor>());
                    }
                    else if (child["name"] != null)
                    {
                        rels.Add(child.ToObject<ModuleRelationshipDescriptor>());
                    }

                }
                return rels;
            }
            return null;
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
