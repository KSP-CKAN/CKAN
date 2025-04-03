using System.Linq;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class ObeysCKANSchemaValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var errors = CKANSchema.schema?.Validate(metadata.AllJson);
            if (errors?.Any() ?? false)
            {
                var msg = string.Join(", ", errors.Select(err => $"{err.Path}: {err.Kind}"));
                throw new Kraken($"Schema validation failed: {msg}");
            }
        }
    }
}
