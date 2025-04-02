using System.Collections.Generic;

using log4net;

using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Transformers
{
    internal sealed class OptimusPrimeTransformer : ITransformer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OptimusPrimeTransformer));

        public string Name => "optimus_prime";

        public IEnumerable<Metadata> Transform(Metadata metadata, TransformOptions opts)
        {
            if (metadata.OptimusPrime)
            {
                Log.Info("Autobots roll out!");
            }

            yield return metadata;
        }
    }
}
