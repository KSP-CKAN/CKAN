using CKAN.Versioning;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class ReplacedByValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            if (metadata.SpecVersion < v1p26 && json.ContainsKey("replaced_by"))
            {
                throw new Kraken("spec_version v1.26+ required for 'replaced_by'");
            }
        }

        private static readonly ModuleVersion v1p26 = new ModuleVersion("v1.26");
    }
}
