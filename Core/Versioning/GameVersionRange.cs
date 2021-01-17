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
        public GameVersionBound Upper { get; private set;  }

        public GameVersionRange(GameVersionBound lower, GameVersionBound upper)
        {
            if (ReferenceEquals(lower, null))
                throw new ArgumentNullException("lower");

            if (ReferenceEquals(upper, null))
                throw new ArgumentNullException("upper");

            Lower = lower;
            Upper = upper;

            _string = DeriveString(this);
        }

        public GameVersionRange(GameVersion lower, GameVersion upper)
            : this(lower?.ToVersionRange().Lower, upper?.ToVersionRange().Upper) { }

        public override string ToString()
        {
            return _string;
        }

        public GameVersionRange IntersectWith(GameVersionRange other)
        {
            if (ReferenceEquals(other, null))
                throw new ArgumentNullException("other");

            var highestLow = GameVersionBound.Highest(Lower, other.Lower);
            var lowestHigh = GameVersionBound.Lowest(Upper, other.Upper);

            return IsEmpty(highestLow, lowestHigh) ? null : new GameVersionRange(highestLow, lowestHigh);
        }

        public bool IsSupersetOf(GameVersionRange other)
        {
            if (ReferenceEquals(other, null))
                throw new ArgumentNullException("other");

            var lowerIsOkay = Lower.Value.IsAny
                || (Lower.Value < other.Lower.Value)
                || (Lower.Value == other.Lower.Value && (Lower.Inclusive || !other.Lower.Inclusive));

            var upperIsOkay = Upper.Value.IsAny
                || (other.Upper.Value < Upper.Value)
                || (other.Upper.Value == Upper.Value && (Upper.Inclusive || !other.Upper.Inclusive));

            return lowerIsOkay && upperIsOkay;
        }

        private static bool IsEmpty(GameVersionBound lower, GameVersionBound upper)
        {
            return upper.Value < lower.Value ||
                (lower.Value == upper.Value && (!lower.Inclusive || !upper.Inclusive));
        }

        private static string DeriveString(GameVersionRange versionRange)
        {
            var sb = new StringBuilder();

            sb.Append(versionRange.Lower.Inclusive ? '[' : '(');

            if (versionRange.Lower.Value != null)
                sb.Append(versionRange.Lower.Value);

            sb.Append(',');

            if (versionRange.Upper.Value != null)
                sb.Append(versionRange.Upper.Value);

            sb.Append(versionRange.Upper.Inclusive ? ']' : ')');

            return sb.ToString();
        }

        private static string SameVersionString(GameVersion v)
        {
            return v == null ? "???"
                :  v.IsAny   ? "all versions"
                :              v.ToString();
        }

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
        {
            return minKsp == maxKsp ? $"{game.ShortName} {SameVersionString(minKsp)}"
                :  minKsp.IsAny     ? $"{game.ShortName} {maxKsp} and earlier"
                :  maxKsp.IsAny     ? $"{game.ShortName} {minKsp} and later"
                :                     $"{game.ShortName} {minKsp}–{maxKsp}";
        }

    }

    public sealed partial class GameVersionRange : IEquatable<GameVersionRange>
    {
        public bool Equals(GameVersionRange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Lower, other.Lower) && Equals(Upper, other.Upper);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GameVersionRange && Equals((GameVersionRange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Lower != null ? Lower.GetHashCode() : 0)*397) ^ (Upper != null ? Upper.GetHashCode() : 0);
            }
        }

        public static bool operator ==(GameVersionRange left, GameVersionRange right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GameVersionRange left, GameVersionRange right)
        {
            return !Equals(left, right);
        }
    }
}
