using System.Linq;

using CKAN.Versioning;

namespace CKAN
{
    public abstract class BaseGameComparator : IGameComparator
    {
        public BaseGameComparator() { }

        public virtual bool Compatible(GameVersionCriteria gameVersionCriteria,
                                       CkanModule          module)
            => gameVersionCriteria.Versions.Count == 0
                || gameVersionCriteria.Versions
                                      .Any(gv => SingleVersionsCompatible(gv, module));

        public abstract bool SingleVersionsCompatible(GameVersion gameVersionCriteria,
                                                      CkanModule  module);
    }
}
