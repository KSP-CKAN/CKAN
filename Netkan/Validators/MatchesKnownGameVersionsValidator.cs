using CKAN.GameVersionProviders;
using CKAN.Versioning;
﻿using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class MatchesKnownGameVersionsValidator : IValidator
    {
        public MatchesKnownGameVersionsValidator()
        {
            buildMap = new KspBuildMap(new Win32Registry());
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

        private KspBuildMap buildMap;
    }
}
