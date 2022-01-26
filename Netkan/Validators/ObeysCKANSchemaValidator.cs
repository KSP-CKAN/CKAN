using System.Linq;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class ObeysCKANSchemaValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var errors = CKANSchema.schema.Validate(metadata.Json());
            if (errors.Any())
            {
                string msg = errors
                    .Select(err => $"{err.Path}: {err.Kind}")
                    .Aggregate((a, b) => $"{a}\r\n{b}");
                throw new Kraken($"Schema validation failed: {msg}");
            }
        }
    }
}
