namespace CKAN.Games.KerbalSpaceProgram.DLC
{
    /// <summary>
    /// Represents an object that can detect the presence of the official Breaking Ground DLC in a KSP installation.
    /// </summary>
    public sealed class BreakingGroundDlcDetector : StandardDlcDetectorBase
    {
        public BreakingGroundDlcDetector()
            : base(new KerbalSpaceProgram(),
                   "BreakingGround",
                   "Serenity",
                   new Versioning.GameVersion(1, 7, 1))
        { }
    }
}
