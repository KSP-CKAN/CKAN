using System.Collections.Generic;
using Autofac;

using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.Games;

namespace CKAN.NetKAN.Validators
{
    internal sealed class MatchesKnownGameVersionsValidator : IValidator
    {
        public MatchesKnownGameVersionsValidator(IGame game)
        {
            this.game     = game;
            knownVersions = game.KnownVersions;
        }

        public void Validate(Metadata metadata)
        {
            var mod = CkanModule.FromJson(metadata.Json().ToString());
            if (!mod.IsCompatible(new GameVersionCriteria(null, knownVersions)))
            {
                GameVersion minKsp = null, maxKsp = null;
                Registry.GetMinMaxVersions(new List<CkanModule>() {mod}, out _, out _, out minKsp, out maxKsp);
                throw new Kraken($"{metadata.Identifier} doesn't match any valid game version: {GameVersionRange.VersionSpan(game, minKsp, maxKsp)}");
            }
        }

        private List<GameVersion> knownVersions;
        private IGame             game;
    }
}
