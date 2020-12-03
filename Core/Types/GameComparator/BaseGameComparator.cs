using CKAN.Versioning;

namespace CKAN
{
    public abstract class BaseGameComparator : IGameComparator
    {
        public BaseGameComparator() { }

        public virtual bool Compatible(GameVersionCriteria gameVersionCriteria, CkanModule module)
        {
            if (gameVersionCriteria.Versions.Count == 0)
            {
                return true;
            }
            foreach (GameVersion gameVersion in gameVersionCriteria.Versions)
            {
                if (SingleVersionsCompatible (gameVersion, module))
                {
                    return true;
                }
            }
            return false;
        }

        public abstract bool SingleVersionsCompatible(GameVersion gameVersionCriteria, CkanModule module);
    }
}
