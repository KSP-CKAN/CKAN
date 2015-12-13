using System;

namespace CKAN
{
    /// <summary>
    /// Test to see if the user's game is "Generally Recognised As Safe" (GRAS) with a given mod,
    /// with extra understanding of which KSP versions are "safe" (ie: 1.0.5 mostly works with 1.0.4 mods).
    /// If the mod has `ksp_version_strict` set then this is identical to strict checking.
    /// </summary>
    public class GrasGameComparator : IGameComparator
    {
        static readonly StrictGameComparator strict = new StrictGameComparator();
        static readonly KSPVersion v104 = new KSPVersion("1.0.4");

        public bool Compatible(KSPVersion gameVersion, CkanModule module)
        {
            // If it's strictly compatible, then it's compatible.
            if (strict.Compatible(gameVersion, module))
                return true;

            // If we're in strict mode, and it's not strictly compatible, then it's
            // not compatible.
            if (module.ksp_version_strict)
                return false;

            // Otherwise, check if it's "generally recognise as safe".

            // If we're running KSP 1.0.5, then allow the mod to run if we would have
            // considered it compatible under 1.0.4
            if (gameVersion.Equals("1.0.5"))
                return strict.Compatible(v104, module);

            return false;
        }
    }
}

