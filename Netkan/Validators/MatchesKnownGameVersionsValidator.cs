using CKAN.GameVersionProviders;
using CKAN.Versioning;
﻿using CKAN.NetKAN.Model;
using Autofac;

namespace CKAN.NetKAN.Validators
{
    internal sealed class MatchesKnownGameVersionsValidator : IValidator
    {
        public MatchesKnownGameVersionsValidator()
        {
            buildMap = ServiceLocator.Container.Resolve<IKspBuildMap>();
            buildMap.Refresh(BuildMapSource.Embedded);
        }

        public void Validate(Metadata metadata)
        {
            var mod = CkanModule.FromJson(metadata.Json().ToString());
            if (!mod.IsCompatibleKSP(new KspVersionCriteria(null, buildMap.KnownVersions)))
            {
                throw new Kraken($"{metadata.Identifier} doesn't match any valid game version");
            }
        }

        private IKspBuildMap buildMap;
    }
}
