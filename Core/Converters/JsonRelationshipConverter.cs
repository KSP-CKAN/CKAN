using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CKAN
{
    public class JsonRelationshipConverter : JsonConverter
    {
        // Only convert when we're an explicit attribute
        [ExcludeFromCodeCoverage]
        public override bool CanConvert(Type object_type) => false;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
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
                        foreach (string forbiddenPropertyName in AnyOfRelationshipDescriptor.ForbiddenPropertyNames)
                        {
                            if (child.Property(forbiddenPropertyName) != null)
                            {
                                throw new Kraken(string.Format(
                                    Properties.Resources.JsonRelationshipConverterAnyOfCombined, forbiddenPropertyName));
                            }
                        }
                        if (child.ToObject<AnyOfRelationshipDescriptor>()
                            is AnyOfRelationshipDescriptor rel)
                        {
                            rels.Add(rel);
                        }
                    }
                    else if (child["name"] != null)
                    {
                        if (child.ToObject<ModuleRelationshipDescriptor>()
                            is ModuleRelationshipDescriptor rel)
                        {
                            rels.Add(rel);
                        }
                    }

                }
                return rels;
            }
            return null;
        }

        [ExcludeFromCodeCoverage]
        public override bool CanWrite => false;

        [ExcludeFromCodeCoverage]
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
