using System.Collections.Generic;

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
            var mod = CkanModule.FromJson(metadata.AllJson.ToString());
            if (!mod.IsCompatible(new GameVersionCriteria(null, knownVersions)))
            {
                CkanModule.GetMinMaxVersions(new List<CkanModule>() { mod }, out _, out _,
                                             out GameVersion? minKsp, out GameVersion? maxKsp);
                throw new Kraken($"{metadata.Identifier} doesn't match any valid game version: {GameVersionRange.VersionSpan(game, minKsp ?? GameVersion.Any, maxKsp ?? GameVersion.Any)}");
            }
        }

        private readonly List<GameVersion> knownVersions;
        private readonly IGame             game;
    }
}
