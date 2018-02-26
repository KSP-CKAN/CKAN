using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CKAN
{
    /// <summary>
    /// Represents the version number of a package.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The format of the version number is as follows. Optional components are shown in square brackets
    /// (<c>[</c> and <c>]</c>):
    /// </para>
    /// <code>
    /// [epoch:]version
    /// </code>
    /// <para>
    /// <c>epoch</c> must be an integer greater than or equal to 0. If not present it is assumed to be 0.
    /// <c>version</c> may be any arbitrary string.
    /// </para>
    /// </remarks>
    [Serializable]
    [JsonConverter(typeof(JsonSimpleStringConverter))]
    public partial class Version : IComparable<Version>
    {
        private readonly Dictionary<Tuple<Version, Version>, int> _cache =
            new Dictionary<Tuple<Version, Version>, int>();
        private readonly string _originalString;
        public const string AutodetectedDllString = "autodetected dll";

        public int EpochPart { get; }
        public string VersionPart { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class using the specified string.
        /// </summary>
        /// <param name="version">A <see cref="String"/> in the appropriate format.</param>
        public Version(string version)
        {
            _originalString = version;

            var match = Regex.Match(
                version,
                @"^(?:(?<epoch>[0-9]+):)?(?<version>.*)$"
            );

            // If we have an epoch, then record it.
            if (match.Groups["epoch"].Value.Length > 0)
            {
                EpochPart = Convert.ToInt32( match.Groups["epoch"].Value );
            }

            VersionPart = match.Groups["version"].Value;
        }

        /// <summary>
        /// Converts the value of the current <see cref="Version"/> object to its equivalent <see cref="String"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// The <see cref="String"/> representation of the current <see cref="Version"/> object.
        /// </returns>
        /// /// <remarks>
        /// The return value should not be considered safe for use in file paths.
        /// </remarks>
        public override string ToString()
        {
            return _originalString;
        }

        /// <summary>
        /// Converts the specified string to a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="version">A <see cref="String"/> in the appropriate format.</param>
        /// <returns>
        /// A new <see cref="Version"/> instance representing the specified <see cref="String"/>.
        /// </returns>
        public static explicit operator Version(string version)
        {
            return new Version(version);
        }

        /// <summary>
        /// Compares the current <see cref="Version"/> object to a specified <see cref="Version"/> object and returns
        /// an indication of their relative values.
        /// </summary>
        /// <param name="other">
        /// A <see cref="Version"/> object to compare to the current <see cref="Version"/> object, or <c>null</c>.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>A negative value</term>
        /// <description>
        /// When the current <see cref="Version"/> object is less than the specified <see cref="Version"/> object.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>
        /// When the current <see cref="Version"/> object is equal to the specified <see cref="Version"/> object.
        /// </description>
        /// </item>
        /// <item>
        /// <term>A positive value</term>
        /// <description>
        /// When the current <see cref="Version"/> object is greater than the specified <see cref="Version"/> object.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(Version other)
        {
            if (other.EpochPart == EpochPart && other.VersionPart.Equals(VersionPart))
                return 0;

            // Compare epochs first.
            if (EpochPart != other.EpochPart)
                return EpochPart > other.EpochPart ? 1 : -1;

            // Epochs are the same. Do the dance described in
            // https://github.com/KSP-CKAN/CKAN/blob/master/Spec.md#version-ordering
            var tuple = new Tuple<Version, Version>(this, other);
            if (_cache.TryGetValue(tuple, out var ret))
                return ret;

            Comparison comp;
            comp.FirstRemainder = VersionPart;
            comp.SecondRemainder = other.VersionPart;

            // Process our strings while there are characters remaining
            while (comp.FirstRemainder.Length > 0 && comp.SecondRemainder.Length > 0)
            {
                // Start by comparing the string parts.
                comp = StringComp(comp.FirstRemainder, comp.SecondRemainder);

                // If we've found a difference, return it.
                if (comp.CompareTo != 0)
                {
                    _cache.Add(tuple, comp.CompareTo);
                    return comp.CompareTo;
                }

                // Otherwise, compare the number parts.
                // It's okay not to check if our strings are exhausted, because
                // if they are the exhausted parts will return zero.

                comp = NumComp (comp.FirstRemainder, comp.SecondRemainder);

                // Again, return difference if found.
                if (comp.CompareTo != 0)
                {
                    _cache.Add(tuple, comp.CompareTo);
                    return comp.CompareTo;
                }
            }

            // Oh, we've run out of one or both strings.

            if (comp.FirstRemainder.Length == 0)
            {
                if (comp.SecondRemainder.Length == 0)
                {
                    _cache.Add(tuple, 0);
                    return 0;
                }

                // They *can't* be equal, because we would have detected that in our first test.
                // So, whichever version is empty first is the smallest. (1.2 < 1.2.3)
                _cache.Add(tuple, -1);
                return -1;
            }
            _cache.Add(tuple, 1);
            return 1;
        }

        /// <summary>
        /// Compares the current <see cref="Version"/> object to a specified <see cref="Version"/> object and returns
        /// if it is equal to the other object.
        /// </summary>
        /// <param name="other">
        /// A <see cref="Version"/> object to compare to the current <see cref="Version"/> object, or <c>null</c>.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When the current <see cref="Version"/> object is equal to the specified <see cref="Version"/> object.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When the current <see cref="Version"/> object is not equal to the specified <see cref="Version"/> object.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public bool IsEqualTo(Version other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Compares the current <see cref="Version"/> object to a specified <see cref="Version"/> object and returns
        /// if it is less than the other object.
        /// </summary>
        /// <param name="other">
        /// A <see cref="Version"/> object to compare to the current <see cref="Version"/> object, or <c>null</c>.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When the current <see cref="Version"/> object is less than the specified <see cref="Version"/> object.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When the current <see cref="Version"/> object is not less than the specified <see cref="Version"/> object.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public bool IsLessThan(Version other)
        {
            return CompareTo(other) < 0;
        }

        /// <summary>
        /// Compares the current <see cref="Version"/> object to a specified <see cref="Version"/> object and returns
        /// if it is greater than the other object.
        /// </summary>
        /// <param name="other">
        /// A <see cref="Version"/> object to compare to the current <see cref="Version"/> object, or <c>null</c>.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When the current <see cref="Version"/> object is greater than the specified <see cref="Version"/> object.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When the current <see cref="Version"/> object is not greater than the specified <see cref="Version"/>
        /// object.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public bool IsGreaterThan(Version other)
        {
            return CompareTo(other) > 0;
        }

        /// <summary>
        /// Returns the larger of two <see cref="Version"/> objects.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="Version"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="Version"/> objects to compare.</param>
        /// <returns>Parameter <paramref name="ver1"/> or <paramref name="ver2"/>, whichever is larger.</returns>
        /// <remarks>
        /// If parameters <paramref name="ver1"/> and <paramref name="ver2"/> are equal, it is undefined which
        /// particular instance will be returned.
        /// </remarks>
        public static Version Max(Version ver1, Version ver2)
        {
            if (ver1 == null)
                throw new ArgumentNullException(nameof(ver1));

            if (ver2 == null)
                throw new ArgumentNullException(nameof(ver2));

            return ver1.IsGreaterThan(ver2) ? ver1 : ver2;
        }

        /// <summary>
        /// Returns the smaller of two <see cref="Version"/> objects.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="Version"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="Version"/> objects to compare.</param>
        /// <returns>Parameter <paramref name="ver1"/> or <paramref name="ver2"/>, whichever is smaller.</returns>
        /// <remarks>
        /// If parameters <paramref name="ver1"/> and <paramref name="ver2"/> are equal, it is undefined which
        /// particular instance will be returned.
        /// </remarks>
        public static Version Min(Version ver1, Version ver2)
        {
            if (ver1 == null)
                throw new ArgumentNullException(nameof(ver1));

            if (ver2 == null)
                throw new ArgumentNullException(nameof(ver2));

            return ver1.IsLessThan(ver2) ? ver1 : ver2;
        }

        private static Comparison StringComp(string v1, string v2)
        {
            var comp = new Comparison {FirstRemainder = "", SecondRemainder = ""};

            // Our starting assumptions are that both versions are completely
            // strings, with no remainder. We'll then check if they're not.

            var str1 = v1;
            var str2 = v2;

            // Start by walking along our version string until we find a number,
            // thereby finding the starting string in both cases. If we fall off
            // the end, then our assumptions made above hold.

            for (var i = 0; i < v1.Length; i++)
            {
                if (char.IsNumber(v1[i]))
                {
                    comp.FirstRemainder = v1.Substring(i);
                    str1 = v1.Substring(0, i);
                    break;
                }
            }

            for (var i = 0; i < v2.Length; i++)
            {
                if (char.IsNumber(v2[i]))
                {
                    comp.SecondRemainder = v2.Substring(i);
                    str2 = v2.Substring(0, i);
                    break;
                }
            }

            // Then compare the two strings, and return our comparison state.
            // Override sorting of '.' to higher than other characters.
            if (str1.Length > 0 && str2.Length > 0)
            {
                if (str1[0] != '.' && str2[0] == '.')
                {
                    comp.CompareTo = -1;
                }
                else if (str1[0] == '.' && str2[0] != '.')
                {
                    comp.CompareTo = 1;
                }
                else if (str1[0] == '.' && str2[0] == '.')
                {
                    if (str1.Length == 1 && str2.Length > 1)
                    {
                        comp.CompareTo = 1;
                    }
                    else if (str1.Length > 1 && str2.Length == 1)
                    {
                        comp.CompareTo = -1;
                    }
                }
                else
                {
                    comp.CompareTo = string.CompareOrdinal(str1, str2);
                }
            }
            else
            {
                comp.CompareTo = string.CompareOrdinal(str1, str2);
            }
            return comp;
        }

        private static Comparison NumComp(string v1, string v2)
        {
            var comp = new Comparison {FirstRemainder = "", SecondRemainder = ""};

            var minimumLength1 = 0;
            for (var i = 0; i < v1.Length; i++)
            {
                if (!char.IsNumber(v1[i]))
                {
                    comp.FirstRemainder = v1.Substring(i);
                    break;
                }

                minimumLength1++;
            }
            
            var minimumLength2 = 0;
            for (var i = 0; i < v2.Length; i++)
            {
                if (!char.IsNumber(v2[i]))
                {
                    comp.SecondRemainder = v2.Substring(i);
                    break;
                }

                minimumLength2++;
            }


            if (!int.TryParse(v1.Substring(0, minimumLength1), out var integer1))
                integer1 = 0;

            if (!int.TryParse(v2.Substring(0, minimumLength2), out var integer2))
                integer2 = 0;

            comp.CompareTo = integer1.CompareTo(integer2);
            return comp;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is Version other && IsEqualTo(other);
        }

        public override int GetHashCode()
        {
            return VersionPart.GetHashCode();
        }

        /// <summary>
        /// Compares two <see cref="Version"/> objects to determine if the first is less than the second.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="Version"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="Version"/> objects to compare.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When <paramref name="ver1"/> is less than <paramref name="ver2"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When <paramref name="ver1"/> is not less than <paramref name="ver2"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static bool operator <(Version ver1, Version ver2)
        {
            return ver1.CompareTo(ver2) < 0;
        }

        /// <summary>
        /// Compares two <see cref="Version"/> objects to determine if the first is less than or equal to the second.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="Version"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="Version"/> objects to compare.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When <paramref name="ver1"/> is less than or equal to <paramref name="ver2"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When <paramref name="ver1"/> is not less than nor equal to <paramref name="ver2"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static bool operator <=(Version ver1, Version ver2)
        {
            return ver1.CompareTo(ver2) <= 0;
        }

        /// <summary>
        /// Compares two <see cref="Version"/> objects to determine if the first is greater than the second.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="Version"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="Version"/> objects to compare.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When <paramref name="ver1"/> is greater than <paramref name="ver2"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When <paramref name="ver1"/> is not greater than <paramref name="ver2"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static bool operator >(Version ver1, Version ver2)
        {
            return ver1.CompareTo(ver2) > 0;
        }

        /// <summary>
        /// Compares two <see cref="Version"/> objects to determine if the first is less than or equal to the second.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="Version"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="Version"/> objects to compare.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When <paramref name="ver1"/> is greater than or equal to <paramref name="ver2"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When <paramref name="ver1"/> is not greater than nor equal to <paramref name="ver2"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static bool operator >=(Version ver1, Version ver2)
        {
            return ver1.CompareTo(ver2) >= 0;
        }
    }

    public partial class Version
    {
        private struct Comparison
        {
            public int CompareTo;
            public string FirstRemainder;
            public string SecondRemainder;
        }
    }
}
