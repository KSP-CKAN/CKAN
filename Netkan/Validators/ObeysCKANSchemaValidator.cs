using System.IO;
using System.Linq;
using System.Reflection;
using NJsonSchema;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class ObeysCKANSchemaValidator : IValidator
    {
        static ObeysCKANSchemaValidator()
        {
            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedSchema);
            using (var reader = new StreamReader(resourceStream))
            {
                schema = JsonSchema.FromJsonAsync(reader.ReadToEnd()).Result;
            }
        }

        public void Validate(Metadata metadata)
        {
            var errors = schema.Validate(metadata.Json());
            if (errors.Any())
            {
                string msg = errors
                    .Select(err => $"{err.Path}: {err.Kind}")
                    .Aggregate((a, b) => $"{a}\r\n{b}");
                throw new Kraken($"Schema validation failed: {msg}");
            }
        }

        private static readonly JsonSchema schema;
        private const string embeddedSchema = "CKAN.NetKAN.CKAN.schema";
    }
}
