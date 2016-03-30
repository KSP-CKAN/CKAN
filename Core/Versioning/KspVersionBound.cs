using System;

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

            _string = inclusive ? string.Format("[{0}]", value) : string.Format("({0})", value);
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
}
