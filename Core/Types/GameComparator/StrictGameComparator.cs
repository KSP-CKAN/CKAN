using CKAN.Versioning;

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

                    moduleRange = new KspVersionRange(KspVersionBound.Unbounded, maxRange.Upper);
                }
            }
            else
            {
                return true;
            }

            return moduleRange.IsSupersetOf(gameVersionRange);
        }
    }
}
