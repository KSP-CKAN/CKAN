using log4net;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class KrefValidator : IValidator
    {
        public KrefValidator() { }

        public void Validate(Metadata metadata)
        {
            Log.Debug("Validating that metadata contains valid or null $kref");

            switch (metadata.Kref?.Source)
            {
                case null:
                case "curse":
                case "github":
                case "gitlab":
                case "http":
                case "ksp-avc":
                case "jenkins":
                case "netkan":
                case "spacedock":
                    // We know this $kref, looks good
                    break;

                default:
                    // Anything not in the above list won't trigger a Transformer
                    throw new Kraken($"Invalid $kref: {metadata.Kref}");
            }

        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(KrefValidator));
    }
}
