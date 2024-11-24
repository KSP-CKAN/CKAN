using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CKAN
{
    public class JsonReleaseStatusConverter : StringEnumConverter
    {
        public override object? ReadJson(JsonReader     reader,
                                         Type           objectType,
                                         object?        existingValue,
                                         JsonSerializer serializer)
            => reader.Value?.ToString() switch
            {
                "alpha" => ReleaseStatus.development,
                "beta"  => ReleaseStatus.testing,
                null    => ReleaseStatus.stable,
                ""      => throw new JsonException("Empty release_status string"),
                _       => base.ReadJson(reader, objectType,
                                         existingValue, serializer),
            };

        public override bool CanWrite => true;
        public override bool CanConvert(Type object_type) => false;
    }
}
