using System;
using System.Collections.Generic;
using System.Linq;

using CKAN.Games;

namespace CKAN.Versioning
{
    public class GameVersionCriteria : IEquatable<GameVersionCriteria>
    {
        private List<GameVersion> _versions = new List<GameVersion>();

        public GameVersionCriteria(GameVersion v)
        {
            if (v != null)
            {
                this._versions.Add(v);
            }
        }

        public GameVersionCriteria(GameVersion v, List<GameVersion> compatibleVersions)
        {
            if (v != null)
            {
                this._versions.Add(v);
            }
            this._versions.AddRange(compatibleVersions);
            this._versions = this._versions.Distinct().ToList();
        }

        public IList<GameVersion> Versions => _versions.AsReadOnly();

        public GameVersionRange MinAndMax => Versions
            .Skip(1)
            .Select(v => v.ToVersionRange())
            .Aggregate(Versions.First().ToVersionRange(),
                       (range, v) => new GameVersionRange(
                            GameVersionBound.Lowest(range.Lower, v.Lower),
                            GameVersionBound.Highest(range.Upper, v.Upper)));

        public GameVersionCriteria Union(GameVersionCriteria other)
            => new GameVersionCriteria(null, _versions.Union(other.Versions).ToList());

        public override bool Equals(object obj)
            => Equals(obj as GameVersionCriteria);

        // From IEquatable<GameVersionCriteria>
        public bool Equals(GameVersionCriteria other)
            => other == null ? false
                             : !_versions.Except(other._versions).Any()
                                 && !other._versions.Except(_versions).Any();

        public override int GetHashCode()
            => _versions.Aggregate(19, (code, vers) => code * 31 + vers.GetHashCode());

        public override String ToString()
            => string.Format(Properties.Resources.GameVersionCriteriaToString,
                             string.Join(", ", _versions.Select(v => v.ToString())));

        public string ToSummaryString(IGame game)
            => MinAndMax.ToSummaryString(game);
    }
}
