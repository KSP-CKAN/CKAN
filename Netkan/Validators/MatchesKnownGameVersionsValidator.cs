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
        }

        public void Validate(Metadata metadata)
        {
            var mod = CkanModule.FromJson(metadata.Json().ToString());
            // Get latest builds from server
            buildMap.Refresh();
            if (!mod.IsCompatibleKSP(new KspVersionCriteria(null, buildMap.KnownVersions)))
            {
                throw new Kraken($"{metadata.Identifier} doesn't match any valid game version");
            }
        }

        private KspBuildMap buildMap;
    }
}
