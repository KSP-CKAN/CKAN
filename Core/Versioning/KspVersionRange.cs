using System;
using System.Text;

namespace CKAN.Versioning
{
    public sealed partial class KspVersionRange
    {
        private readonly string _string;

        public static readonly KspVersionRange Any =
            new KspVersionRange(KspVersionBound.Unbounded, KspVersionBound.Unbounded);

        public KspVersionBound Lower { get; private set; }
        public KspVersionBound Upper { get; private set;  }

        public KspVersionRange(KspVersionBound lower, KspVersionBound upper)
        {
            if (ReferenceEquals(lower, null))
                throw new ArgumentNullException("lower");

            if (ReferenceEquals(upper, null))
                throw new ArgumentNullException("upper");

            Lower = lower;
            Upper = upper;

            _string = DeriveString(this);
        }

        public override string ToString()
        {
            return _string;
        }

        public KspVersionRange IntersectWith(KspVersionRange other)
        {
            if (ReferenceEquals(other, null))
                throw new ArgumentNullException("other");

            var highestLow = KspVersionBound.Highest(Lower, other.Lower);
            var lowestHigh = KspVersionBound.Lowest(Upper, other.Upper);

            return IsEmpty(highestLow, lowestHigh) ? null : new KspVersionRange(highestLow, lowestHigh);
        }

        public bool IsSupersetOf(KspVersionRange other)
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

        private static bool IsEmpty(KspVersionBound lower, KspVersionBound upper)
        {
            return upper.Value < lower.Value ||
                (lower.Value == upper.Value && (!lower.Inclusive || !upper.Inclusive));
        }

        private static string DeriveString(KspVersionRange versionRange)
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
    }

    public sealed partial class KspVersionRange : IEquatable<KspVersionRange>
    {
        public bool Equals(KspVersionRange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Lower, other.Lower) && Equals(Upper, other.Upper);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is KspVersionRange && Equals((KspVersionRange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Lower != null ? Lower.GetHashCode() : 0)*397) ^ (Upper != null ? Upper.GetHashCode() : 0);
            }
        }

        public static bool operator ==(KspVersionRange left, KspVersionRange right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KspVersionRange left, KspVersionRange right)
        {
            return !Equals(left, right);
        }
    }
}
