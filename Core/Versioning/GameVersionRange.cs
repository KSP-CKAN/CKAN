using System;
using System.Text;
using CKAN.Games;

namespace CKAN.Versioning
{
    public sealed partial class GameVersionRange
    {
        private readonly string _string;

        public static readonly GameVersionRange Any =
            new GameVersionRange(GameVersionBound.Unbounded, GameVersionBound.Unbounded);

        public GameVersionBound Lower { get; private set; }
        public GameVersionBound Upper { get; private set; }

        public GameVersionRange(GameVersionBound lower, GameVersionBound upper)
        {
            Lower = lower ?? GameVersionBound.Unbounded;
            Upper = upper ?? GameVersionBound.Unbounded;

            _string = DeriveString(this);
        }

        public GameVersionRange(GameVersion lower, GameVersion upper)
            : this(lower?.ToVersionRange().Lower, upper?.ToVersionRange().Upper) { }

        public override string ToString() => _string;

        public GameVersionRange IntersectWith(GameVersionRange other)
        {
            if (other is null)
            {
                throw new ArgumentNullException("other");
            }

            var highestLow = GameVersionBound.Highest(Lower, other.Lower);
            var lowestHigh = GameVersionBound.Lowest(Upper, other.Upper);

            return IsEmpty(highestLow, lowestHigh) ? null : new GameVersionRange(highestLow, lowestHigh);
        }

        // Same logic as above but without "new"
        private bool Intersects(GameVersionRange other)
            => !IsEmpty(GameVersionBound.Highest(Lower, other.Lower),
                        GameVersionBound.Lowest(Upper, other.Upper));

        public bool IsSupersetOf(GameVersionRange other)
        {
            if (other is null)
            {
                throw new ArgumentNullException("other");
            }

            var lowerIsOkay = Lower.Value.IsAny
                || (Lower.Value < other.Lower.Value)
                || (Lower.Value == other.Lower.Value && (Lower.Inclusive || !other.Lower.Inclusive));

            var upperIsOkay = Upper.Value.IsAny
                || (other.Upper.Value < Upper.Value)
                || (other.Upper.Value == Upper.Value && (Upper.Inclusive || !other.Upper.Inclusive));

            return lowerIsOkay && upperIsOkay;
        }

        /// <summary>
        /// Check whether a given game version is within this range
        /// </summary>
        /// <param name="ver">The game version to check</param>
        /// <returns>True if within bounds, false otherwise</returns>
        public bool Contains(GameVersion ver)
            => Intersects(ver.ToVersionRange());

        private static bool IsEmpty(GameVersionBound lower, GameVersionBound upper)
            => upper.Value < lower.Value ||
                (lower.Value == upper.Value && (!lower.Inclusive || !upper.Inclusive));

        private static string DeriveString(GameVersionRange versionRange)
        {
            var sb = new StringBuilder();

            sb.Append(versionRange.Lower.Inclusive ? '[' : '(');

            if (versionRange.Lower.Value != null)
            {
                sb.Append(versionRange.Lower.Value);
            }

            sb.Append(',');

            if (versionRange.Upper.Value != null)
            {
                sb.Append(versionRange.Upper.Value);
            }

            sb.Append(versionRange.Upper.Inclusive ? ']' : ')');

            return sb.ToString();
        }

        private static string SameVersionString(GameVersion v)
            => v == null ? "???"
             : v.IsAny   ? Properties.Resources.CkanModuleAllVersions
             :             v.ToString();

        /// <summary>
        /// Generate a string describing a range of KSP versions.
        /// May be bounded or unbounded on either side.
        /// </summary>
        /// <param name="minKsp">Lowest version in the range</param>
        /// <param name="maxKsp">Highest version in the range</param>
        /// <returns>
        /// Human readable string describing the versions.
        /// </returns>
        public static string VersionSpan(IGame game, GameVersion minKsp, GameVersion maxKsp)
            => minKsp == maxKsp
                ? $"{game.ShortName} {SameVersionString(minKsp)}"
                : minKsp.IsAny
                    ? string.Format(Properties.Resources.GameVersionRangeMinOnly, game.ShortName, maxKsp)
                    : maxKsp.IsAny
                        ? string.Format(Properties.Resources.GameVersionRangeMaxOnly, game.ShortName, minKsp)
                        : $"{game.ShortName} {minKsp}–{maxKsp}";

        public string ToSummaryString(IGame game)
            => VersionSpan(game,
                           Lower.AsInclusiveLower().WithoutBuild,
                           Upper.AsInclusiveUpper().WithoutBuild);
    }

    public sealed partial class GameVersionRange : IEquatable<GameVersionRange>
    {
        public bool Equals(GameVersionRange other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Lower, other.Lower) && Equals(Upper, other.Upper);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is GameVersionRange range && Equals(range);
        }

        public override int GetHashCode()
            => (Lower, Upper).GetHashCode();

        public static bool operator ==(GameVersionRange left, GameVersionRange right) => Equals(left, right);
        public static bool operator !=(GameVersionRange left, GameVersionRange right) => !Equals(left, right);
    }
}
