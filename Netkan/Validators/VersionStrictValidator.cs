using CKAN.Versioning;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class VersionStrictValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            if (metadata.SpecVersion < v1p16 && json.ContainsKey("ksp_version_strict"))
            {
                throw new Kraken("spec_version v1.16+ required for 'ksp_version_strict'");
            }
        }

        private static readonly ModuleVersion v1p16 = new ModuleVersion("v1.16");
    }
}
