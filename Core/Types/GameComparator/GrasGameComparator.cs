using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Test to see if the user's game is "Generally Recognised As Safe" (GRAS) with a given mod,
    /// with extra understanding of which KSP versions are "safe" (ie: 1.0.5 mostly works with 1.0.4 mods).
    /// If the mod has `ksp_version_strict` set then this is identical to strict checking.
    /// </summary>
    public class GrasGameComparator : BaseGameComparator
    {
        static readonly StrictGameComparator strict = new StrictGameComparator ();
        static readonly KspVersion v103 = KspVersion.Parse ("1.0.3");

        public override bool Compatible (KspVersionCriteria gameVersionCriteria, CkanModule module)
        {
            // If it's strictly compatible, then it's compatible.
            if (strict.Compatible (gameVersionCriteria, module))
                return true;

            // If we're in strict mode, and it's not strictly compatible, then it's
            // not compatible.
            if (module.ksp_version_strict)
                return false;

            return base.Compatible (gameVersionCriteria, module);
        }

        public override bool SingleVersionsCompatible (KspVersion gameVersion, CkanModule module){
            
            // Otherwise, check if it's "generally recognise as safe".

            // If we're running KSP 1.0.4, then allow the mod to run if we would have
            // considered it compatible under 1.0.3 (as 1.0.4 was "just a hotfix").
            if (gameVersion.Equals (KspVersion.Parse ("1.0.4")))
                return strict.SingleVersionsCompatible (v103, module);

            return false;
        }
    }
}
