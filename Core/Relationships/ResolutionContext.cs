using System.Collections.Generic;

using CKAN.Configuration;
using CKAN.Versioning;

namespace CKAN
{

    public sealed class ResolutionContext
    {
        public readonly IRegistryQuerier                Registry;
        public readonly IReadOnlyCollection<CkanModule> Installed;
        public readonly IReadOnlyCollection<CkanModule> Installing;
        public readonly StabilityToleranceConfig        StabilityTolerance;
        public readonly GameVersionCriteria             Crit;

        public ResolutionContext(IRegistryQuerier                registry,
                                 IReadOnlyCollection<CkanModule> installed,
                                 IReadOnlyCollection<CkanModule> installing,
                                 StabilityToleranceConfig        stabilityTolerance,
                                 GameVersionCriteria             crit)
        {
            Registry           = registry;
            Installed          = installed;
            Installing         = installing;
            StabilityTolerance = stabilityTolerance;
            Crit               = crit;
        }
    }
}
