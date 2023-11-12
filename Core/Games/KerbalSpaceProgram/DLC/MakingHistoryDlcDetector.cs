using System.Collections.Generic;

namespace CKAN.Games.KerbalSpaceProgram.DLC
{
    /// <summary>
    /// Represents an object that can detect the presence of the official Making History DLC in a KSP installation.
    /// </summary>
    public sealed class MakingHistoryDlcDetector : StandardDlcDetectorBase
    {
        public MakingHistoryDlcDetector()
            : base(new KerbalSpaceProgram(),
                   "MakingHistory",
                   new Versioning.GameVersion(1, 4, 1),
                   new Dictionary<string, string>()
                   {
                       { "1.0", "1.0.0" }
                   })
        { }
    }
}
