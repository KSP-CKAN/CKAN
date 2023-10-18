using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class DownloadVersionValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.Json();
            if (json.ContainsKey("download")
                && !json.ContainsKey("version")
                && !json.ContainsKey("$vref"))
            {
                throw new Kraken($"{metadata.Identifier} expects a version when a download url is provided");
            }
        }
    }
}
