using CKAN.GameVersionProviders;
using CKAN.Versioning;
﻿using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class MatchesKnownGameVersionsValidator : IValidator
    {
        public void Validate(Metadata metadata)
        {
            var mod = CkanModule.FromJson(metadata.Json().ToString());
            var knownVersions = new KspBuildMap(new Win32Registry()).KnownVersions;
            if (!mod.IsCompatibleKSP(new KspVersionCriteria(null, knownVersions)))
            {
                throw new Kraken($"{metadata.Identifier} doesn't match any valid game version");
            }
        }
    }
}
