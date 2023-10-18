using Newtonsoft.Json.Linq;

using CKAN.Versioning;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class DownloadArrayValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            if (json.ContainsKey("download"))
            {
                if (metadata.SpecVersion < v1p34 && json["download"] is JArray)
                {
                    throw new Kraken(ErrorMessage);
                }
            }
        }

        private static readonly ModuleVersion v1p34 = new ModuleVersion("v1.34");
        internal const string ErrorMessage = "spec_version v1.34+ required for download as array";
    }
}
