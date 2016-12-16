using System;
using System.Linq;

namespace CKAN.Versioning
{
    public sealed partial class KspVersionBound
    {
        public static readonly KspVersionBound Unbounded = new KspVersionBound();

        public KspVersion Value { get; private set; }
        public bool Inclusive { get; private set; }

        private readonly string _string;

        public KspVersionBound()
            : this(KspVersion.Any, true) { }

        public KspVersionBound(KspVersion value, bool inclusive)
        {
            if (ReferenceEquals(value, null))
                throw new ArgumentNullException("value");

            if (!value.IsAny && !value.IsFullyDefined)
                throw new ArgumentException("Version must be either fully undefined or fully defined.", "value");

            Value = value;
            Inclusive = inclusive;

            // Workaround an issue in old (<=3.2.x) versions of Mono that does not correctly handle null values
            // returned from ToString().
            var valueStr = value.ToString() ?? string.Empty;

            _string = inclusive ? string.Format("[{0}]", valueStr) : string.Format("({0})", valueStr);
        }

        public override string ToString()
        {
            return _string;
        }
    }

    public sealed partial class KspVersionBound : IEquatable<KspVersionBound>
    {
        public bool Equals(KspVersionBound other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Value, other.Value) && Inclusive == other.Inclusive;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is KspVersionBound && Equals((KspVersionBound) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0)*397) ^ Inclusive.GetHashCode();
            }
        }

        public static bool operator ==(KspVersionBound left, KspVersionBound right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KspVersionBound left, KspVersionBound right)
        {
            return !Equals(left, right);
        }
    }

    public sealed partial class KspVersionBound
    {
        /// <summary>
        /// Returns the lowest of a set of <see cref="KspVersionBound"/> objects. Analagous to
        /// <see cref="KspVersion.Min(KspVersion[])"/> but does not produce a stable sort because in the event of a
        /// tie inclusive bounds are treated as both lower and higher than equivalent exclusive bounds.
        /// </summary>
        /// <param name="versionBounds">The set of <see cref="KspVersionBound"/> objects to compare.</param>
        /// <returns>The lowest value in <see cref="versionBounds"/>.</returns>
        public static KspVersionBound Lowest(params KspVersionBound[] versionBounds)
        {
            if (versionBounds == null)
                throw new ArgumentNullException("versionBounds");

            if (!versionBounds.Any())
                throw new ArgumentException("Value cannot be empty.", "versionBounds");

            if (versionBounds.Any(i => i == null))
                throw new ArgumentException("Value cannot contain null.", "versionBounds");

            return versionBounds
                .OrderBy(i => i == Unbounded)
                .ThenBy(i => i.Value)
                .ThenBy(i => i.Inclusive)
                .First();
        }

        /// <summary>
        /// Returns the highest of a set of <see cref="KspVersionBound"/> objects. Analagous to
        /// <see cref="KspVersion.Max(KspVersion[])"/> but does not produce a stable sort because in the event of a
        /// tie inclusive bounds are treated as both lower and higher than equivalent exclusive bounds.
        /// </summary>
        /// <param name="versionBounds">The set of <see cref="KspVersionBound"/> objects to compare.</param>
        /// <returns>The highest value in <see cref="versionBounds"/>.</returns>
        public static KspVersionBound Highest(params KspVersionBound[] versionBounds)
        {
            if (versionBounds == null)
                throw new ArgumentNullException("versionBounds");

            if (!versionBounds.Any())
                throw new ArgumentException("Value cannot be empty.", "versionBounds");

            if (versionBounds.Any(i => i == null))
                throw new ArgumentException("Value cannot contain null.", "versionBounds");

            return versionBounds
                .OrderBy(i => i == Unbounded)
                .ThenByDescending(i => i.Value)
                .ThenBy(i => i.Inclusive)
                .First();
        }
    }
}
