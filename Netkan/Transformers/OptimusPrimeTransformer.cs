using CKAN.NetKAN.Model;
using log4net;
using Newtonsoft.Json.Linq;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class OptimusPrimeTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OptimusPrimeTransformer));

        public Metadata Transform(Metadata metadata)
        {
            var json = metadata.Json();

            JToken optimusPrime;
            if (json.TryGetValue("x_netkan_optimus_prime", out optimusPrime) && (bool)optimusPrime)
            {
                Log.Info("Autobots roll out!");
            }

            return metadata;
        }
    }
}
