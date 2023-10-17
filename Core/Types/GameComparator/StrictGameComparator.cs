using log4net;

using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Test to see if a module is compatible with the user's installed game,
    /// using strict tests.
    /// </summary>
    public class StrictGameComparator : BaseGameComparator
    {
        public override bool SingleVersionsCompatible(GameVersion gameVersion, CkanModule module)
        {
            var gameVersionRange = gameVersion.ToVersionRange();

            var moduleRange = GameVersionRange.Any;

            if (module.ksp_version != null)
            {
                moduleRange = module.ksp_version.ToVersionRange();
            }
            else if (module.ksp_version_min != null || module.ksp_version_max != null)
            {
                if (module.ksp_version_min != null && module.ksp_version_max != null)
                {
                    moduleRange = new GameVersionRange(module.ksp_version_min, module.ksp_version_max);
                    if (moduleRange.Lower.Value > moduleRange.Upper.Value)
                    {
                        log.WarnFormat("{0} is not less or equal to {1}",
                            module.ksp_version_min, module.ksp_version_max);
                        return false;
                    }
                }
                else if (module.ksp_version_min != null)
                {
                    moduleRange = new GameVersionRange(module.ksp_version_min, GameVersion.Any);
                }
                else if (module.ksp_version_max != null)
                {
                    moduleRange = new GameVersionRange(GameVersion.Any, module.ksp_version_max);
                }
            }
            else
            {
                return true;
            }

            return gameVersionRange.IntersectWith(moduleRange) != null;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(StrictGameComparator));
    }
}
