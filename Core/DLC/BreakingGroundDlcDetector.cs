using System.Collections.Generic;

namespace CKAN.DLC
{
    /// <summary>
    /// Represents an object that can detect the presence of the official Making History DLC in a KSP installation.
    /// </summary>
    public sealed class BreakingGroundDlcDetector : StandardDlcDetectorBase
    {
        public BreakingGroundDlcDetector()
            : base("BreakingGround", "Serenity", new Versioning.KspVersion(1, 7, 1)) { }
    }
}
