using System.Collections.Generic;

namespace CKAN.DLC
{
    /// <summary>
    /// Represents an object that can detect the presence of the official Making History DLC in a KSP installation.
    /// </summary>
    public sealed class MakingHistoryDlcDetector : StandardDlcDetectorBase
    {
        public MakingHistoryDlcDetector()
            : base("MakingHistory", new Versioning.KspVersion(1, 4, 1), new Dictionary<string, string>()
                {
                    { "1.0", "1.0.0" }
                }
            ) { }
    }
}
