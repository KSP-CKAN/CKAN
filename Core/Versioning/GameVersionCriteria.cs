using System;
using System.Collections.Generic;
using System.Linq;

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

        public IList<GameVersion> Versions
        {
            get
            {
                return _versions.AsReadOnly();
            }
        }

        public GameVersionCriteria Union(GameVersionCriteria other)
        {
            return new GameVersionCriteria(
                null,
                _versions.Union(other.Versions).ToList()
            );
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GameVersionCriteria);
        }

        // From IEquatable<GameVersionCriteria>
        public bool Equals(GameVersionCriteria other)
        {
            return other == null
                ? false
                : !_versions.Except(other._versions).Any()
                    && !other._versions.Except(_versions).Any();
        }

        public override int GetHashCode()
        {
            return _versions.Aggregate(19, (code, vers) => code * 31 + vers.GetHashCode());
        }

        public override String ToString()
        {
            List<String> versionList = new List<String>();
            foreach (GameVersion version in _versions)
            {
                versionList.Add(version.ToString());
            }
            return "[Versions: " + String.Join( ", ", versionList) + "]";
        }
    }
}
