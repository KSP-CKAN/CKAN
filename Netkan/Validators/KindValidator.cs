using CKAN.NetKAN.Model;
using CKAN.Versioning;

namespace CKAN.NetKAN.Validators
{
    internal sealed class KindValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            var kind = json["kind"];
            if (metadata.SpecVersion < v1p6 && (string)kind == "metapackage")
            {
                throw new Kraken($"spec_version 1.6+ required for kind 'metpackage'");
            }
            if (metadata.SpecVersion < v1p28 && (string)kind == "dlc")
            {
                throw new Kraken($"spec_version 1.28+ required for kind 'dlc'");
            }
        }

        private static readonly ModuleVersion v1p6  = new ModuleVersion("v1.6");
        private static readonly ModuleVersion v1p28 = new ModuleVersion("v1.28");
    }
}
