using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN {
    /// <summary>
    /// Version comparison utilities.
    /// </summary>

    [Serializable]
    [JsonConverter(typeof(JsonSimpleStringConverter))]
    public class Version : IComparable<Version> {
        private readonly int epoch;
        private readonly string version;
        private readonly string orig_string;
        // static readonly ILog log = LogManager.GetLogger(typeof(RegistryManager));
        public const string AutodetectedDllString = "autodetected dll";

        public int EpochPart
        {
            get { return epoch; }
        }

        public string VersionPart
        {
            get { return version; }
        }

        public struct Comparison {
            public int compare_to;
            public string remainder1;
            public string remainder2;
        }

        /// <summary>
        /// Creates a new version object from the `ToString()` representation of anything!
        /// </summary>
        public Version (string version) {
            orig_string = version;

            Match match = Regex.Match (
                version,
                @"^(?:(?<epoch>[0-9]+):)?(?<version>.*)$"
            );

            // If we have an epoch, then record it.
            if (match.Groups["epoch"].Value.Length > 0) {
                epoch = Convert.ToInt32( match.Groups["epoch"].Value );
            }

            this.version = match.Groups["version"].Value;
        }

        /// <summary>
        /// Returns the original string used to generate this version. Note this may *NOT* be
        /// safe for use in filenames, as it may contain colons (for the epoch) and other
        /// funny characters.
        /// </summary>
        override public string ToString() {
            return orig_string;
        }

        // When cast from a string.
        public static explicit operator Version(string v) {
            return new Version (v);
        }


        private Dictionary<Tuple<Version,Version>, int> cache = new Dictionary<Tuple<Version, Version>, int>();
        /// <summary>
        /// Returns -1 if this is less than that
        /// Returns +1 if this is greater than that
        /// Returns  0 if equal.
        /// </summary>
        public int CompareTo(Version that) {

            if (that.epoch == epoch && that.version.Equals(version)) {
                return 0;
            }

            // Compare epochs first.
            if (epoch != that.epoch) {
                return epoch > that.epoch ?1:-1;
            }

            // Epochs are the same. Do the dance described in
            // https://github.com/KSP-CKAN/CKAN/blob/master/Spec.md#version-ordering
            int ret;
            var tuple = new Tuple<Version, Version>(this, that);
            if (cache.TryGetValue(tuple, out ret))
            {
                return ret;
            }

            Comparison comp;
            comp.remainder1 = version;
            comp.remainder2 = that.version;

            // Process our strings while there are characters remaining
            while (comp.remainder1.Length > 0 && comp.remainder2.Length > 0) {

                // Start by comparing the string parts.
                comp = StringComp (comp.remainder1, comp.remainder2);

                // If we've found a difference, return it.
                if (comp.compare_to != 0) {
                    cache.Add(tuple, comp.compare_to);
                    return comp.compare_to;
                }

                // Otherwise, compare the number parts.
                // It's okay not to check if our strings are exhausted, because
                // if they are the exhausted parts will return zero.

                comp = NumComp (comp.remainder1, comp.remainder2);

                // Again, return difference if found.
                if (comp.compare_to != 0) {
                    cache.Add(tuple, comp.compare_to);
                    return comp.compare_to;
                }
            }

            // Oh, we've run out of one or both strings.


            if (comp.remainder1.Length == 0) {
                if (comp.remainder2.Length == 0)
                {
                    cache.Add(tuple, 0);
                    return 0;
                }

                // They *can't* be equal, because we would have detected that in our first test.
                // So, whichever version is empty first is the smallest. (1.2 < 1.2.3)
                cache.Add(tuple, -1);
                return -1;
            }
            cache.Add(tuple, 1);
            return 1;

        }

        public bool IsEqualTo(Version that) {
            return CompareTo (that) == 0;
        }

        public bool IsLessThan(Version that) {
            return CompareTo (that) < 0;
        }

        public bool IsGreaterThan(Version that) {
            return CompareTo (that) > 0;
        }

        public static Version Max(Version a, Version b)
        {
            if (a == null)
                throw new ArgumentNullException("a");

            if (b == null)
                throw new ArgumentNullException("b");

            return a.IsGreaterThan(b) ? a : b;
        }

        public static Version Min(Version a, Version b)
        {
            if (a == null)
                throw new ArgumentNullException("a");

            if (b == null)
                throw new ArgumentNullException("b");

            return a.IsLessThan(b) ? a : b;
        }

        /// <summary>
        /// Compare the leading non-numerical parts of two strings
        /// </summary>

        internal static Comparison StringComp(string v1, string v2)
        {
            var comp = new Comparison {remainder1 = "", remainder2 = ""};

            // Our starting assumptions are that both versions are completely
            // strings, with no remainder. We'll then check if they're not.

            string str1 = v1;
            string str2 = v2;

            // Start by walking along our version string until we find a number,
            // thereby finding the starting string in both cases. If we fall off
            // the end, then our assumptions made above hold.

            for (int i = 0; i < v1.Length; i++)
            {
                if (Char.IsNumber(v1[i]))
                {
                    comp.remainder1 = v1.Substring(i);
                    str1 = v1.Substring(0, i);
                    break;
                }
            }

            for (int i = 0; i < v2.Length; i++)
            {
                if (Char.IsNumber(v2[i]))
                {
                    comp.remainder2 = v2.Substring(i);
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
                    comp.compare_to = -1;
                }
                else if (str1[0] == '.' && str2[0] != '.')
                {
                    comp.compare_to = 1;
                }
                else if (str1.Length == 1 && str2.Length > 1)
                {
                    comp.compare_to = 1;
                }
                else if (str1.Length > 1 && str2.Length == 1)
                {
                    comp.compare_to = -1;
                }
                else
                {
                    comp.compare_to = String.CompareOrdinal(str1, str2);
                }
            }
            else
            {
                comp.compare_to = String.CompareOrdinal(str1, str2);
            }
            return comp;
        }

        /// <summary>
        /// Compare the leading numerical parts of two strings
        /// </summary>

        internal static Comparison NumComp(string v1, string v2)
        {
            var comp = new Comparison {remainder1 = "", remainder2 = ""};

            int minimum_length1 = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                if (!char.IsNumber(v1[i]))
                {
                    comp.remainder1 = v1.Substring(i);
                    break;
                }

                minimum_length1++;
            }

            int minimum_length2 = 0;
            for (int i = 0; i < v2.Length; i++)
            {
                if (!char.IsNumber(v2[i]))
                {
                    comp.remainder2 = v2.Substring(i);
                    break;
                }

                minimum_length2++;
            }

            int integer1;
            int integer2;

            if (!int.TryParse(v1.Substring(0, minimum_length1), out integer1))
            {
                integer1 = 0;
            }

            if (!int.TryParse(v2.Substring(0, minimum_length2), out integer2))
            {
                integer2 = 0;
            }

            comp.compare_to = integer1.CompareTo(integer2);
            return comp;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Version;
            return other != null ? IsEqualTo(other) : base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return version.GetHashCode();
        }
        int IComparable<Version>.CompareTo(Version other)
        {
            return CompareTo(other);
        }

        public static bool operator <(Version v1, Version v2)
        {
            return v1.CompareTo(v2) < 0;
        }

        public static bool operator <=(Version v1, Version v2)
        {
            return v1.CompareTo(v2) <= 0;
        }

        public static bool operator >(Version v1, Version v2)
        {
            return v1.CompareTo(v2) > 0;
        }

        public static bool operator >=(Version v1, Version v2)
        {
            return v1.CompareTo(v2) >= 0;
        }

    }

    /// <summary>
    /// This class represents a DllVersion. They don't have real
    /// version numbers or anything
    /// </summary>
    public class DllVersion : Version {
        public DllVersion() :base("0")
        {
        }

        override public string ToString()
        {
            return AutodetectedDllString;
        }
    }

    /// <summary>
    /// This class represents a virtual version that was provided by
    /// another module.
    /// </summary>
    public class ProvidesVersion : Version {
        internal readonly string provided_by;

        public ProvidesVersion(string provided_by) :base("0")
        {
            this.provided_by = provided_by;
        }

        override public string ToString()
        {
            return string.Format("provided by {0}", provided_by);
        }
    }
}

