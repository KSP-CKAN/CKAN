using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class KrefDownloadMutexValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json = metadata.AllJson;
            var kind = json.Value<string>("kind");
            if (json.ContainsKey("download") && json.ContainsKey("$kref"))
            {
                throw new Kraken($"{metadata.Identifier} has a $kref and a download field, this is likely incorrect");
            }
            if (kind != "dlc" && !json.ContainsKey("download") && !json.ContainsKey("$kref"))
            {
                throw new Kraken($"{metadata.Identifier} has no $kref field, this is required when no download url is specified");
            }
        }
    }
}
