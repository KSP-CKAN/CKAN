using System;

namespace CKAN
{
    /// <summary>
    /// Test to see if a module is compatible with the user's installed game,
    /// using strict tests.
    /// </summary>
    public class GameComparatorStrict : IGameComparator
    {

        public bool Compatible(KSPVersion gameVersion, CkanModule module)
        {
            KSPVersion ksp_version = module.ksp_version;
            KSPVersion ksp_version_min = module.ksp_version_min;
            KSPVersion ksp_version_max = module.ksp_version_max;

            // Check the min and max versions.

            if (ksp_version_min.IsNotAny() && gameVersion < ksp_version_min)
            {
                return false;
            }

            if (ksp_version_max.IsNotAny() && gameVersion > ksp_version_max)
            {
                return false;
            }

            // We didn't hit the min/max guards. They may not have existed.

            // Note that since ksp_version is "any" if not specified, this
            // will work fine if there's no target, or if there were min/max
            // fields and we passed them successfully.

            return ksp_version.Targets(gameVersion);
        }
    }
}

