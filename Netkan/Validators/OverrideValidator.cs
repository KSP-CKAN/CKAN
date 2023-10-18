using Newtonsoft.Json.Linq;
using CKAN.NetKAN.Model;
using System.Linq;

namespace CKAN.NetKAN.Validators
{
    internal sealed class OverrideValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var json      = metadata.Json();
            var overrides = json["x_netkan_override"];
            if (overrides != null)
            {
                if (!(overrides is JArray))
                {
                    throw new Kraken("Netkan overrides require an array");
                }
                foreach (JObject ovr in overrides.Cast<JObject>())
                {
                    if (!ovr.ContainsKey("version"))
                    {
                        throw new Kraken("Netkan overrides require a version");
                    }
                    if (!ovr.ContainsKey("delete") && !ovr.ContainsKey("override"))
                    {
                        throw new Kraken("Netkan overrides require a delete or override section");
                    }
                }
            }
        }
    }
}
