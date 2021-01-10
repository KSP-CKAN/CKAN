using System.Collections.Generic;
using Autofac;
using CKAN.GameVersionProviders;
using CKAN.Versioning;
using CKAN.Games;
using CKAN.NetKAN.Model;

namespace CKAN.NetKAN.Validators
{
    internal sealed class MatchesKnownGameVersionsValidator : IValidator
    {
        public MatchesKnownGameVersionsValidator()
        {
            knownVersions = new KerbalSpaceProgram().KnownVersions;
        }

        public void Validate(Metadata metadata)
        {
            var mod = CkanModule.FromJson(metadata.Json().ToString());
            if (!mod.IsCompatibleKSP(new GameVersionCriteria(null, knownVersions)))
            {
                GameVersion minKsp = null, maxKsp = null;
                Registry.GetMinMaxVersions(new List<CkanModule>() {mod}, out _, out _, out minKsp, out maxKsp);
                var game = new KerbalSpaceProgram();
                throw new Kraken($"{metadata.Identifier} doesn't match any valid game version: {GameVersionRange.VersionSpan(game, minKsp, maxKsp)}");
            }
        }

        private List<GameVersion> knownVersions;
    }
}
