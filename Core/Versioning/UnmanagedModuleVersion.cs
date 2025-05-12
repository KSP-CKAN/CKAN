using System;

using Newtonsoft.Json;

namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version of a module that is not managed by CKAN.
    /// </summary>
    [JsonConverter(typeof(JsonUnmanagedModuleVersionConverter))]
    public sealed class UnmanagedModuleVersion : ModuleVersion
    {
        private readonly string _string;

        public bool IsUnknownVersion { get; }

        // HACK: Hardcoding a value of "0" for autodetected DLLs preserves previous behavior.
        public UnmanagedModuleVersion(string? version) : base(version ?? "0")
        {
            OriginalString = version;
            IsUnknownVersion = OriginalString == null;
            _string = OriginalString == null
                          ? Properties.Resources.UnmanagedModuleVersionUnknown
                          : string.Format(Properties.Resources.UnmanagedModuleVersionKnown,
                                          OriginalString);
        }

        public readonly string? OriginalString;

        public override string ToString()
            => _string;
    }

    public class JsonUnmanagedModuleVersionConverter : JsonConverter
    {
        public override object? ReadJson(JsonReader     reader,
                                         Type           objectType,
                                         object?        existingValue,
                                         JsonSerializer serializer)
            => Activator.CreateInstance(objectType, reader.Value?.ToString());

        public override void WriteJson(JsonWriter     writer,
                                       object?        value,
                                       JsonSerializer serializer)
        {
            writer.WriteValue(value is UnmanagedModuleVersion unm
                                  ? unm.OriginalString
                                  : value?.ToString());
        }

        // Explicit conversions only, please.
        public override bool CanConvert(Type objectType) => false;
    }
}
