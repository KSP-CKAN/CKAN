using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version number of a Kerbal Space Program (KSP) installation.
    /// </summary>
    [JsonConverter(typeof(KspVersionJsonConverter))]
    public sealed partial class KspVersion
    {
        private static readonly Regex Pattern = new Regex(
            @"^(?<major>\d+)(?:\.(?<minor>\d+)(?:\.(?<patch>\d+)(?:\.(?<build>\d+))?)?)?$",
            RegexOptions.Compiled
        );

        private const int Undefined = -1;

        public static readonly KspVersion Any = new KspVersion();

        private readonly int _major;
        private readonly int _minor;
        private readonly int _patch;
        private readonly int _build;

        private readonly string _string;

        /// <summary>
        /// Gets the value of the major component of the version number for the current <see cref="KspVersion"/>
        /// object.
        /// </summary>
        public int Major {  get { return _major; } }

        /// <summary>
        /// Gets the value of the minor component of the version number for the current <see cref="KspVersion"/>
        /// object.
        /// </summary>
        public int Minor { get { return _minor; } }

        /// <summary>
        /// Gets the value of the patch component of the version number for the current <see cref="KspVersion"/>
        /// object.
        /// </summary>
        public int Patch { get { return _patch; } }

        /// <summary>
        /// Gets the value of the build component of the version number for the current <see cref="KspVersion"/>
        /// object.
        /// </summary>
        public int Build { get { return _build; } }

        /// <summary>
        /// Gets whether or not the major component of the version number for the current <see cref="KspVersion"/>
        /// object is defined.
        /// </summary>
        public bool IsMajorDefined { get { return _major != Undefined; } }

        /// <summary>
        /// Gets whether or not the minor component of the version number for the current <see cref="KspVersion"/>
        /// object is defined.
        /// </summary>
        public bool IsMinorDefined { get { return _minor != Undefined; } }

        /// <summary>
        /// Gets whether or not the patch component of the version number for the current <see cref="KspVersion"/>
        /// object is defined.
        /// </summary>
        public bool IsPatchDefined { get { return _patch != Undefined; } }

        /// <summary>
        /// Gets whether or not the build component of the version number for the current <see cref="KspVersion"/>
        /// object is defined.
        /// </summary>
        public bool IsBuildDefined {  get { return _build != Undefined; } }

        /// <summary>
        /// Indicates whether or not all components of the current <see cref="KspVersion"/> are defined.
        /// </summary>
        public bool IsFullyDefined
        {
            get { return IsMajorDefined && IsMinorDefined && IsPatchDefined && IsBuildDefined; }
        }

        /// <summary>
        /// Indicates wheter or not all the components of the current <see cref="KspVersion"/> are undefined.
        /// </summary>
        public bool IsAny
        {
            get { return !IsMajorDefined && !IsMinorDefined && !IsPatchDefined && !IsBuildDefined; }
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="KspVersion"/> class with all components unspecified.
        /// </summary>
        public KspVersion()
        {
            _major = Undefined;
            _minor = Undefined;
            _patch = Undefined;
            _build = Undefined;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="KspVersion"/> class using the specified major value.
        /// </summary>
        /// <param name="major">The major version number.</param>
        public KspVersion(int major)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException("major");

            _major = major;
            _minor = Undefined;
            _patch = Undefined;
            _build = Undefined;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="KspVersion"/> class using the specified major and minor
        /// values.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public KspVersion(int major, int minor)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException("major");

            if (minor < 0)
                throw new ArgumentOutOfRangeException("minor");

            _major = major;
            _minor = minor;
            _patch = Undefined;
            _build = Undefined;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="KspVersion"/> class using the specified major, minor, and
        /// patch values.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        public KspVersion(int major, int minor, int patch)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException("major");

            if (minor < 0)
                throw new ArgumentOutOfRangeException("minor");

            if (patch < 0)
                throw new ArgumentOutOfRangeException("patch");

            _major = major;
            _minor = minor;
            _patch = patch;
            _build = Undefined;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="KspVersion"/> class using the specified major, minor, patch,
        /// and build values.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="build">The build verison number.</param>
        public KspVersion(int major, int minor, int patch, int build)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException("major");

            if (minor < 0)
                throw new ArgumentOutOfRangeException("minor");

            if (patch < 0)
                throw new ArgumentOutOfRangeException("patch");

            if (build < 0)
                throw new ArgumentOutOfRangeException("build");

            _major = major;
            _minor = minor;
            _patch = patch;
            _build = build;

            _string = DeriveString(_major, _minor, _patch, _build);
        }

        /// <summary>
        /// Converts the value of the current <see cref="KspVersion"/> to its equivalent <see cref="String"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// <para>
        /// The <see cref="String"/> representation of the values of the major, minor, patch, and build components of
        /// the current <see cref="KspVersion"/> object as depicted in the following format. Each component is
        /// separated by a period character ('.'). Square brackets ('[' and ']') indicate a component that will not
        /// appear in the return value if the component is not defined:
        /// </para>
        /// <para>
        /// [<i>major</i>[.<i>minor</i>[.<i>patch</i>[.<i>build</i>]]]]
        /// </para>
        /// <para>
        /// For example, if you create a <see cref="KspVersion"/> object using the constructor <c>KspVersion(1,1)</c>,
        /// the returned string is "1.1". If you create a <see cref="KspVersion"/> using the constructor (1,3,4,2),
        /// the returned string is "1.3.4.2".
        /// </para>
        /// <para>
        /// If the current <see cref="KspVersion"/> is totally undefined the return value will <c>null</c>.
        /// </para>
        /// </returns>
        public override string ToString()
        {
            return _string;
        }

        /// <summary>
        /// Converts the value of the current <see cref="KspVersion"/> to its equivalent
        /// <see cref="KspVersionRange"/>.
        /// </summary>
        /// <returns>
        /// <para>
        /// A <see cref="KspVersionRange"/> which specifies a set of versions equivalent to the current
        /// <see cref="KspVersion"/>.
        /// </para>
        /// <para>
        /// For example, the version "1.0.0.0" would be equivalent to the range ["1.0.0.0", "1.0.0.0"], while the
        /// version "1.0" would be equivalent to the range ["1.0.0.0", "1.1.0.0"). Where '[' and ']' represent 
        /// inclusive bounds and '(' and ')' represent exclusive bounds.
        /// </para>
        /// </returns>
        public KspVersionRange ToVersionRange()
        {
            KspVersionBound lower;
            KspVersionBound upper;
        
            if (IsBuildDefined)
            {
                lower = new KspVersionBound(this, inclusive: true);
                upper = new KspVersionBound(this, inclusive: true);
            }
            else if (IsPatchDefined)
            {
                lower = new KspVersionBound(new KspVersion(Major, Minor, Patch, 0), inclusive: true);
                upper = new KspVersionBound(new KspVersion(Major, Minor, Patch + 1, 0), inclusive: false);
            }
            else if (IsMinorDefined)
            {
                lower = new KspVersionBound(new KspVersion(Major, Minor, 0, 0), inclusive: true);
                upper = new KspVersionBound(new KspVersion(Major, Minor + 1, 0, 0), inclusive: false);
            }
            else if (IsMajorDefined)
            {
                lower = new KspVersionBound(new KspVersion(Major, 0, 0, 0), inclusive: true);
                upper = new KspVersionBound(new KspVersion(Major + 1, 0, 0, 0), inclusive: false);
            }
            else
            {
                lower = KspVersionBound.Unbounded;
                upper = KspVersionBound.Unbounded;
            }

            return new KspVersionRange(lower, upper);
        }

        /// <summary>
        /// Converts the string representation of a version number to an equivalent <see cref="KspVersion"/> object.
        /// </summary>
        /// <param name="input">A string that contains a version number to convert.</param>
        /// <returns>
        /// A <see cref="KspVersion"/> object that is equivalent to the version number specified in the
        /// <see cref="input"/> parameter.
        /// </returns>
        public static KspVersion Parse(string input)
        {
            if (ReferenceEquals(input, null))
                throw new ArgumentNullException("input");

            KspVersion result;
            if (TryParse(input, out result))
                return result;
            else
                throw new FormatException();
        }

        /// <summary>
        /// Tries to convert the string representation of a version number to an equivalent <see cref="KspVersion"/>
        /// object and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">
        /// A string that contains a version number to convert.
        /// </param>
        /// <param name="result">
        /// When this method returns <c>true</c>, contains the <see cref="KspVersion"/> equivalent of the number that
        /// is contained in <see cref="input"/>. When this method returns <c>false</c>, the value is unspecified.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="input"/> parameter was converted successfully; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryParse(string input, out KspVersion result)
        {
            result = null;

            if (ReferenceEquals(input, null))
                return false;

            var major = Undefined;
            var minor = Undefined;
            var patch = Undefined;
            var build = Undefined;

            var match = Pattern.Match(input.Trim());

            if (match.Success)
            {
                var majorGroup = match.Groups["major"];
                var minorGroup = match.Groups["minor"];
                var patchGroup = match.Groups["patch"];
                var buildGroup = match.Groups["build"];

                if (majorGroup.Success)
                    if (!int.TryParse(majorGroup.Value, out major))
                        return false;

                if (minorGroup.Success)
                    if (!int.TryParse(minorGroup.Value, out minor))
                        return false;

                if (patchGroup.Success)
                    if (!int.TryParse(patchGroup.Value, out patch))
                        return false;

                if (buildGroup.Success)
                    if (!int.TryParse(buildGroup.Value, out build))
                        return false;

                if (minor == Undefined)
                    result = new KspVersion(major);
                else if (patch == Undefined)
                    result = new KspVersion(major, minor);
                else if (build == Undefined)
                    result = new KspVersion(major, minor, patch);
                else
                    result = new KspVersion(major, minor, patch, build);

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public sealed partial class KspVersion : IEquatable<KspVersion>
    {
        /// <summary>
        /// Returns a value indicating whether the current <see cref="KspVersion"/> object and specified
        /// <see cref="KspVersion"/> object represent the same value.
        /// </summary>
        /// <param name="obj">
        /// A <see cref="KspVersion"/> object to compare to the current <see cref="KspVersion"/> object, or
        /// <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if every component of the current <see cref="KspVersion"/> matches the corresponding component
        /// of the <see cref="obj"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(KspVersion obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(obj, this)) return true;
            return _major == obj._major && _minor == obj._minor && _patch == obj._patch && _build == obj._build;
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="KspVersion"/> object is equal to a specified
        /// object.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with the current <see cref="KspVersion"/> object, or <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current <see cref="KspVersion"/> object and <see cref="obj"/> are both
        /// <see cref="KspVersion"/> objects and every component of the current <see cref="KspVersion"/> object
        /// matches the corresponding component of <see cref="obj"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(obj, this)) return true;
            return obj is KspVersion && Equals((KspVersion) obj);
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="KspVersion"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _major.GetHashCode();
                hashCode = (hashCode*397) ^ _minor.GetHashCode();
                hashCode = (hashCode*397) ^ _patch.GetHashCode();
                hashCode = (hashCode*397) ^ _build.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether two specified <see cref="KspVersion"/> objects are equal.
        /// </summary>
        /// <param name="v1">The first <see cref="KspVersion"/> object.</param>
        /// <param name="v2">The second <see cref="KspVersion"/> object.</param>
        /// <returns><c>true</c> if <see cref="v1"/> equals <see cref="v2"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(KspVersion v1, KspVersion v2)
        {
            return Equals(v1, v2);
        }

        /// <summary>
        /// Determines whether two specified <see cref="KspVersion"/> objects are not equal.
        /// </summary>
        /// <param name="v1">The first <see cref="KspVersion"/> object.</param>
        /// <param name="v2">The second <see cref="KspVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if <see cref="v1"/> does not equal <see cref="v2"/>; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(KspVersion v1, KspVersion v2)
        {
            return !Equals(v1, v2);
        }
    }

    public sealed partial class KspVersion : IComparable, IComparable<KspVersion>
    {
        /// <summary>
        /// Compares the current <see cref="KspVersion"/> object to a specified object and returns an indication of
        /// their relative values.
        /// </summary>
        /// <param name="obj">An object to compare, or <c>null</c>.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of the two objects, as shown in the following table.
        /// <list type="table">
        /// <listheader>
        /// <term>Return value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term>
        /// <description>
        /// The current <see cref="KspVersion"/> object is a version before <see cref="obj"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>
        /// The current <see cref="KspVersion"/> object is the same version as <see cref="obj"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>
        /// <para>
        /// The current <see cref="KspVersion"/> object is a version subsequent to <see cref="obj"/>.
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null))
                throw new ArgumentNullException("obj");

            var objKspVersion = obj as KspVersion;

            if (objKspVersion != null)
                return CompareTo(objKspVersion);
            else
                throw new ArgumentException("Object must be of type KspVersion.");
        }

        /// <summary>
        /// Compares the current <see cref="KspVersion"/> object to a specified object and returns an indication of
        /// their relative values.
        /// </summary>
        /// <param name="other">An object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of the two objects, as shown in the following table.
        /// <list type="table">
        /// <listheader>
        /// <term>Return value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>Less than zero</term>
        /// <description>
        /// The current <see cref="KspVersion"/> object is a version before <see cref="other"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>
        /// The current <see cref="KspVersion"/> object is the same version as <see cref="other"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Greater than zero</term>
        /// <description>
        /// <para>
        /// The current <see cref="KspVersion"/> object is a version subsequent to <see cref="other"/>.
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(KspVersion other)
        {
            if (ReferenceEquals(other, null))
                throw new ArgumentNullException("other");

            if (Equals(this, other))
                return 0;

            var majorCompare = _major.CompareTo(other._major);

            if (majorCompare == 0)
            {
                var minorCompare = _minor.CompareTo(other._minor);

                if (minorCompare == 0)
                {
                    var patchCompare = _patch.CompareTo(other._patch);

                    return patchCompare == 0 ? _build.CompareTo(other._build) : patchCompare;
                }
                else
                {
                    return minorCompare;
                }
            }
            else
            {
                return majorCompare;
            }
        }

        /// <summary>
        /// Determines whether the first specified <see cref="KspVersion"/> object is less than the second specified
        /// <see cref="KspVersion"/> object.
        /// </summary>
        /// <param name="left">The first <see cref="KspVersion"/> object.</param>
        /// <param name="right">The second <see cref="KspVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if <see cref="left"/> is less than <see cref="right"/>; otherwise, <c>flase</c>.
        /// </returns>
        public static bool operator <(KspVersion left, KspVersion right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException("left");

            if (ReferenceEquals(right, null))
                throw new ArgumentNullException("right");

            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Determines whether the first specified <see cref="KspVersion"/> object is greater than the second
        /// specified <see cref="Version"/> object.
        /// </summary>
        /// <param name="left">The first <see cref="KspVersion"/> object.</param>
        /// <param name="right">The second <see cref="KspVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if <see cref="left"/> is greater than <see cref="right"/>; otherwise, <c>flase</c>.
        /// </returns>
        public static bool operator >(KspVersion left, KspVersion right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException("left");

            if (ReferenceEquals(right, null))
                throw new ArgumentNullException("right");

            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Determines whether the first specified <see cref="KspVersion"/> object is less than or equal to the second
        /// specified <see cref="KspVersion"/> object.
        /// </summary>
        /// <param name="left">The first <see cref="KspVersion"/> object.</param>
        /// <param name="right">The second <see cref="KspVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if <see cref="left"/> is less than or equal to <see cref="right"/>; otherwise, <c>flase</c>.
        /// </returns>
        public static bool operator <=(KspVersion left, KspVersion right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException("left");

            if (ReferenceEquals(right, null))
                throw new ArgumentNullException("right");

            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Determines whether the first specified <see cref="KspVersion"/> object is greater than or equal to the
        /// second specified <see cref="KspVersion"/> object.
        /// </summary>
        /// <param name="left">The first <see cref="KspVersion"/> object.</param>
        /// <param name="right">The second <see cref="KspVersion"/> object.</param>
        /// <returns>
        /// <c>true</c> if <see cref="left"/> is greater than or equal to <see cref="right"/>; otherwise, <c>flase</c>.
        /// </returns>
        public static bool operator >=(KspVersion left, KspVersion right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException("left");

            if (ReferenceEquals(right, null))
                throw new ArgumentNullException("right");

            return left.CompareTo(right) >= 0;
        }
    }

    public sealed partial class KspVersion
    {
        private static string DeriveString(int major, int minor, int patch, int build)
        {
            var sb = new StringBuilder();

            if (major != Undefined)
            {
                sb.Append(major);
            }

            if (minor != Undefined)
            {
                sb.Append(".");
                sb.Append(minor);
            }

            if (patch != Undefined)
            {
                sb.Append(".");
                sb.Append(patch);
            }

            if (build != Undefined)
            {
                sb.Append(".");
                sb.Append(build);
            }

            var s = sb.ToString();

            return s.Equals(string.Empty) ? null : s;
        }
    }

    public sealed class KspVersionJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value == null ? null : reader.Value.ToString();

            switch (value)
            {
                case null:
                case "any":
                    return null;
                default:
                    KspVersion result;

                    // For a little while, AVC files which didn't specify a full three-part
                    // version number could result in versions like `1.1.`, which cause our
                    // code to fail. Here we strip any trailing dot from the version number,
                    // which makes them valid again before parsing. CKAN#1780

                    value = Regex.Replace(value, @"\.$", "");

                    if (KspVersion.TryParse(value, out result))
                        return result;
                    else
                        throw new JsonException(string.Format("Could not parse KSP version: {0}", value));
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(KspVersion);
        }
    }
}
