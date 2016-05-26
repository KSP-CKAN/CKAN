﻿using System;

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
        static readonly KSPVersion v103 = new KSPVersion("1.0.3");
        static readonly KSPVersion v110 = new KSPVersion("1.1.0");
        static readonly KSPVersion v111 = new KSPVersion("1.1.1");

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

            // If we're running KSP 1.0.4, then allow the mod to run if we would have
            // considered it compatible under 1.0.3 (as 1.0.4 was "just a hotfix").
            if (gameVersion.Equals("1.0.4"))
                return strict.Compatible(v103, module);
				
			// 1.1.1 and 1.1.2 are hotfixes to 1.1.0
            if (gameVersion.Equals("1.1.2") || gameVersion.Equals("1.1.1"))
                return strict.Compatible(v110, module) || strict.Compatible(v111, module);

            return false;
        }
    }
}

