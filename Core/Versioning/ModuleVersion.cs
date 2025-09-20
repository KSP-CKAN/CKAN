using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN.Versioning
{
    /// <summary>
    /// Represents the version number of a module.
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
    public partial class ModuleVersion
    {
        private static readonly Regex Pattern =
            new Regex(@"^(?:(?<epoch>[0-9]+):)?(?<version>.*)$", RegexOptions.Compiled);

        private static readonly ConcurrentDictionary<Tuple<string, string>, int> ComparisonCache =
            new ConcurrentDictionary<Tuple<string, string>, int>();
    }

    public partial class ModuleVersion
    {
        private readonly int _epoch;
        private readonly string _version;
        private readonly string _string;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleVersion"/> class using the specified string.
        /// </summary>
        /// <param name="version">A <see cref="string"/> in the appropriate format.</param>
        public ModuleVersion(string version)
        {
            var match = Pattern.Match(version);

            if (!match.Success)
            {
                throw new FormatException("Input string was not in a correct format.");
            }

            // If we have an epoch, then record it.
            if (match.Groups["epoch"].Value.Length > 0)
            {
                _epoch = Convert.ToInt32( match.Groups["epoch"].Value );
            }

            _version = match.Groups["version"].Value;
            _string = version;
        }

        /// <returns>
        /// true if versions have the same epoch, false if different
        /// </returns>
        public bool EpochEquals(ModuleVersion other)
            => _epoch == other._epoch;

        /// <returns>
        /// New module version with same version as 'this' but with one greater epoch
        /// </returns>
        public ModuleVersion IncrementEpoch()
            => new ModuleVersion($"{_epoch + 1}:{_version}");

        /// <summary>
        /// Converts the value of the current <see cref="ModuleVersion"/> object to its equivalent
        /// <see cref="string"/> representation.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the current <see cref="ModuleVersion"/> object.
        /// </returns>
        /// /// <remarks>
        /// The return value should not be considered safe for use in file paths.
        /// </remarks>
        public override string ToString()
            => _string;

        public string ToString(bool hideEpoch, bool hideV)
            => hideEpoch
                ? hideV
                    ? StripEpoch(StripV(_string))
                    : StripEpoch(_string)
                : hideV
                    ? StripV(_string)
                    : _string;

        /// <summary>
        /// Remove prepending v V. Version_ etc
        /// </summary>
        private static string StripV(string version)
            => Regex.Match(version, @"^(?<num>\d\:)?[vV]+(ersion)?[_.]*(?<ver>\d.*)$") is Match match
               && match.Success
                   ? match.Groups["num"].Value + match.Groups["ver"].Value
                   : version;

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        public string StripEpoch()
            => StripEpoch(_string);

        /// <summary>
        /// Returns a version string shorn of any leading epoch as delimited by a single colon
        /// </summary>
        /// <param name="version">A version string that might contain an epoch</param>
        private static string StripEpoch(string version)
            // If our version number starts with a string of digits, followed by
            // a colon, and then has no more colons, we're probably safe to assume
            // the first string of digits is an epoch
            => epochMatch.IsMatch(version)
                ? epochReplace.Replace(version, @"$2")
                : version;

        /// <summary>
        /// As above, but includes the original in parentheses
        /// </summary>
        public string WithAndWithoutEpoch()
            => epochMatch.IsMatch(_string)
                ? $"{epochReplace.Replace(_string, @"$2")} ({_string})"
                : _string;

        private static readonly Regex epochMatch   = new Regex(@"^[0-9][0-9]*:[^:]+$", RegexOptions.Compiled);
        private static readonly Regex epochReplace = new Regex(@"^([^:]+):([^:]+)$",   RegexOptions.Compiled);
    }

    public partial class ModuleVersion : IEquatable<ModuleVersion>
    {
        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj)
                || (obj is ModuleVersion version && Equals(version));

        public bool Equals(ModuleVersion? other)
            => ReferenceEquals(this, other)
                || CompareTo(other) == 0;

        public override int GetHashCode()
            => (_epoch, _version).GetHashCode();

        /// <summary>
        /// Compares two <see cref="ModuleVersion"/> objects to determine if the first is equal to the second.
        /// </summary>
        /// <param name="left">The first of two <see cref="ModuleVersion"/> objects to compare.</param>
        /// <param name="right">The second of two <see cref="ModuleVersion"/> objects to compare.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When <paramref name="left"/> is equal to <paramref name="right"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When <paramref name="left"/> is not equal to <paramref name="right"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static bool operator ==(ModuleVersion? left, ModuleVersion? right)
            => Equals(left, right);

        /// <summary>
        /// Compares two <see cref="ModuleVersion"/> objects to determine if the first is not equal to the second.
        /// </summary>
        /// <param name="left">The first of two <see cref="ModuleVersion"/> objects to compare.</param>
        /// <param name="right">The second of two <see cref="ModuleVersion"/> objects to compare.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When <paramref name="left"/> is not equal to <paramref name="right"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When <paramref name="left"/> is equal to <paramref name="right"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public static bool operator !=(ModuleVersion? left, ModuleVersion? right)
            => !Equals(left, right);
    }

    public partial class ModuleVersion : IComparable<ModuleVersion>
    {
        /// <summary>
        /// Compares the current <see cref="ModuleVersion"/> object to a specified <see cref="ModuleVersion"/> object
        /// and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">
        /// A <see cref="ModuleVersion"/> object to compare to the current <see cref="ModuleVersion"/> object.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>A negative value</term>
        /// <description>
        /// When the current <see cref="ModuleVersion"/> object is less than the specified <see cref="ModuleVersion"/>
        /// object.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Zero</term>
        /// <description>
        /// When the current <see cref="ModuleVersion"/> object is equal to the specified <see cref="ModuleVersion"/>
        /// object.
        /// </description>
        /// </item>
        /// <item>
        /// <term>A positive value</term>
        /// <description>
        /// When the current <see cref="ModuleVersion"/> object is greater than the specified
        /// <see cref="ModuleVersion"/> object.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(ModuleVersion? other)
        {
            Comparison stringComp(string v1, string v2)
            {
                var comparison = new Comparison { FirstRemainder = "", SecondRemainder = "" };

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
                        comparison.FirstRemainder = v1[i..];
                        str1 = v1[..i];
                        break;
                    }
                }

                for (var i = 0; i < v2.Length; i++)
                {
                    if (char.IsNumber(v2[i]))
                    {
                        comparison.SecondRemainder = v2[i..];
                        str2 = v2[..i];
                        break;
                    }
                }

                // Then compare the two strings, and return our comparison state.
                // Override sorting of '.' to higher than other characters.
                if (//str1 is [char first1, ..] && str2 is [char first2, ..]
                    str1.Length > 0 && str1[0] is var first1
                 && str2.Length > 0 && str2[0] is var first2)
                {
                    if (first1 != '.' && first2 == '.')
                    {
                        comparison.CompareTo = -1;
                    }
                    else if (first1 == '.' && first2 != '.')
                    {
                        comparison.CompareTo = 1;
                    }
                    else if (first1 == '.' && first2 == '.')
                    {
                        if (str1.Length == 1 && str2.Length > 1)
                        {
                            comparison.CompareTo = 1;
                        }
                        else if (str1.Length > 1 && str2.Length == 1)
                        {
                            comparison.CompareTo = -1;
                        }
                    }
                    else
                    {
                        comparison.CompareTo = string.CompareOrdinal(str1, str2);
                    }
                }
                else
                {
                    comparison.CompareTo = string.CompareOrdinal(str1, str2);
                }
                return comparison;
            }

            Comparison numComp(string v1, string v2)
            {
                var comparison = new Comparison { FirstRemainder = "", SecondRemainder = "" };

                var minimumLength1 = 0;
                for (var i = 0; i < v1.Length; i++)
                {
                    if (!char.IsNumber(v1[i]))
                    {
                        comparison.FirstRemainder = v1[i..];
                        break;
                    }

                    minimumLength1++;
                }

                var minimumLength2 = 0;
                for (var i = 0; i < v2.Length; i++)
                {
                    if (!char.IsNumber(v2[i]))
                    {
                        comparison.SecondRemainder = v2[i..];
                        break;
                    }

                    minimumLength2++;
                }


                if (!int.TryParse(v1[..minimumLength1], out var integer1))
                {
                    integer1 = 0;
                }

                if (!int.TryParse(v2[..minimumLength2], out var integer2))
                {
                    integer2 = 0;
                }

                comparison.CompareTo = integer1.CompareTo(integer2);
                return comparison;
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (other._epoch == _epoch && other._version.Equals(_version))
            {
                return 0;
            }

            // Compare epochs first.
            if (_epoch != other._epoch)
            {
                return _epoch > other._epoch ? 1 : -1;
            }

            // Epochs are the same. Do the dance described in
            // https://github.com/KSP-CKAN/CKAN/blob/master/Spec.md#version-ordering
            var tuple = new Tuple<string, string>(_string, other._string);
            if (ComparisonCache.TryGetValue(tuple, out var ret))
            {
                return ret;
            }

            Comparison comp;
            comp.FirstRemainder = _version;
            comp.SecondRemainder = other._version;

            // Process our strings while there are characters remaining
            while (comp.FirstRemainder.Length > 0 && comp.SecondRemainder.Length > 0)
            {
                // Start by comparing the string parts.
                comp = stringComp(comp.FirstRemainder, comp.SecondRemainder);

                // If we've found a difference, return it.
                if (comp.CompareTo != 0)
                {
                    ComparisonCache.TryAdd(tuple, comp.CompareTo);
                    return comp.CompareTo;
                }

                // Otherwise, compare the number parts.
                // It's okay not to check if our strings are exhausted, because
                // if they are the exhausted parts will return zero.

                comp = numComp(comp.FirstRemainder, comp.SecondRemainder);

                // Again, return difference if found.
                if (comp.CompareTo != 0)
                {
                    ComparisonCache.TryAdd(tuple, comp.CompareTo);
                    return comp.CompareTo;
                }
            }

            // Oh, we've run out of one or both strings.

            if (comp.FirstRemainder.Length == 0)
            {
                if (comp.SecondRemainder.Length == 0)
                {
                    ComparisonCache.TryAdd(tuple, 0);
                    return 0;
                }

                // They *can't* be equal, because we would have detected that in our first test.
                // So, whichever version is empty first is the smallest. (1.2 < 1.2.3)
                ComparisonCache.TryAdd(tuple, -1);
                return -1;
            }
            ComparisonCache.TryAdd(tuple, 1);
            return 1;
        }

        /// <summary>
        /// Compares the current <see cref="ModuleVersion"/> object to a specified <see cref="ModuleVersion"/> object
        /// and returns if it is less than the other object.
        /// </summary>
        /// <param name="other">
        /// A <see cref="ModuleVersion"/> object to compare to the current <see cref="ModuleVersion"/> object, or
        /// <c>null</c>.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When the current <see cref="ModuleVersion"/> object is less than the specified <see cref="ModuleVersion"/>
        /// object.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When the current <see cref="ModuleVersion"/> object is not less than the specified
        /// <see cref="ModuleVersion"/> object.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public bool IsLessThan(ModuleVersion? other)
            => CompareTo(other) < 0;

        /// <summary>
        /// Compares the current <see cref="ModuleVersion"/> object to a specified <see cref="ModuleVersion"/> object
        /// and returns if it is greater than the other object.
        /// </summary>
        /// <param name="other">
        /// A <see cref="ModuleVersion"/> object to compare to the current <see cref="ModuleVersion"/> object, or
        /// <c>null</c>.
        /// </param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term><c>true</c></term>
        /// <description>
        /// When the current <see cref="ModuleVersion"/> object is greater than the specified
        /// <see cref="ModuleVersion"/> object.
        /// </description>
        /// </item>
        /// <item>
        /// <term><c>false</c></term>
        /// <description>
        /// When the current <see cref="ModuleVersion"/> object is not greater than the specified
        /// <see cref="ModuleVersion"/> object.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public bool IsGreaterThan(ModuleVersion other)
            => CompareTo(other) > 0;

        /// <summary>
        /// Compares two <see cref="ModuleVersion"/> objects to determine if the first is less than the second.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="ModuleVersion"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="ModuleVersion"/> objects to compare.</param>
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
        public static bool operator <(ModuleVersion ver1, ModuleVersion ver2)
            => ver1.CompareTo(ver2) < 0;

        /// <summary>
        /// Compares two <see cref="ModuleVersion"/> objects to determine if the first is less than or equal to the
        /// second.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="ModuleVersion"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="ModuleVersion"/> objects to compare.</param>
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
        public static bool operator <=(ModuleVersion ver1, ModuleVersion ver2)
            => ver1.CompareTo(ver2) <= 0;

        /// <summary>
        /// Compares two <see cref="ModuleVersion"/> objects to determine if the first is greater than the second.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="ModuleVersion"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="ModuleVersion"/> objects to compare.</param>
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
        public static bool operator >(ModuleVersion ver1, ModuleVersion ver2)
            => ver1.CompareTo(ver2) > 0;

        /// <summary>
        /// Compares two <see cref="ModuleVersion"/> objects to determine if the first is less than or equal to the
        /// second.
        /// </summary>
        /// <param name="ver1">The first of two <see cref="ModuleVersion"/> objects to compare.</param>
        /// <param name="ver2">The second of two <see cref="ModuleVersion"/> objects to compare.</param>
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
        public static bool operator >=(ModuleVersion ver1, ModuleVersion ver2)
            => ver1.CompareTo(ver2) >= 0;

        private struct Comparison
        {
            public int    CompareTo;
            public string FirstRemainder;
            public string SecondRemainder;
        }
    }
}
