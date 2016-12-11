using CKAN.Versioning;
using System;

namespace CKAN
{
    /// <summary>
    /// Test to see if a module is compatible with the user's installed game,
    /// using strict tests.
    /// </summary>
    public class StrictGameComparator : BaseGameComparator
    {
        public override bool SingleVersionsCompatible(KspVersion gameVersion, CkanModule module)
        {
            var gameVersionRange = gameVersion.ToVersionRange();

            var moduleRange = KspVersionRange.Any;

            if (module.ksp_version != null)
            {
                moduleRange = module.ksp_version.ToVersionRange();
            }
            else if (module.ksp_version_min != null || module.ksp_version_max != null)
            {
                if (module.ksp_version_min != null && module.ksp_version_max != null)
                {
                    if (module.ksp_version_min <= module.ksp_version_max)
                    {
                        var minRange = module.ksp_version_min.ToVersionRange();
                        var maxRange = module.ksp_version_max.ToVersionRange();

                        moduleRange = new KspVersionRange(minRange.Lower, maxRange.Upper);
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (module.ksp_version_min != null)
                {
                    var minRange = module.ksp_version_min.ToVersionRange();

                    moduleRange = new KspVersionRange(minRange.Lower, KspVersionBound.Unbounded);
                }
                else if (module.ksp_version_max != null)
                {
                    var maxRange = module.ksp_version_max.ToVersionRange();
                    //
                    //e.g module.ksp_version_max.ToVersionRange() changes 1.0 to [1.0.0.0, 1.1.0.0) so we are
                    //interested in Lower bound
                    //
                    moduleRange = new KspVersionRange(KspVersionBound.Unbounded, maxRange.Lower);
                }
            }
            else
            {
                return true;
            }
            
            if (!moduleRange.Upper.Value.IsAny && isBoundLower(moduleRange.Upper, gameVersionRange.Lower))
            {
                return false;
            }

            if (!moduleRange.Lower.Value.IsAny && isBoundLower(gameVersionRange.Upper, moduleRange.Lower))
            {
                return false;
            }

            return true;
        }

        private bool isBoundLower(KspVersionBound val1, KspVersionBound val2)
        {
            return val1.Value < val2.Value || (val1.Value == val2.Value && !val1.Inclusive);
                   
        }
    }
}
