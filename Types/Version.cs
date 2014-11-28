namespace CKAN {
    using System.Text.RegularExpressions;
    using log4net;
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Version comparison utilities.
    /// </summary>

    [JsonConverter(typeof(JsonSimpleStringConverter))]
    public class Version : IComparable<Version> {
        private int epoch = 0;
        private string version = null;
        private string orig_string = null;
        // static readonly ILog log = LogManager.GetLogger(typeof(RegistryManager));

        public struct Comparison {
            public int compare_to;
            public string remainder1;
            public string remainder2;
        }

        /// <summary>
        /// Creates a new version object from the `ToString()` representation of anything!
        /// </summary>
        public Version (string version) {
            this.orig_string = version;

            Match match = Regex.Match (
                version,
                @"^(?:(?<epoch>[0-9]+):)?(?<version>.*)$"
            );

            // If we have an epoch, then record it.
            if (match.Groups["epoch"].Value.Length > 0) {
                this.epoch = Convert.ToInt32( match.Groups["epoch"].Value );
            }

            this.version = match.Groups["version"].Value;
        }

        override public string ToString() {
            return orig_string;
        }

        // When cast from a string.
        public static explicit operator Version(string v) {
            return new Version (v);
        }

        /// <summary>
        /// Returns -1 if this is less than that
        /// Returns +1 if this is greater than that
        /// Returns  0 if equal.
        /// </summary>

        public int CompareTo(Version that) {

            if (that.epoch == this.epoch && that.version == this.version) {
                return 0;
            }
 
            // Compare epochs first.
            if (this.epoch < that.epoch) {
                return -1;
            } else if (this.epoch > that.epoch) {
                return 1;
            }

            // Epochs are the same. Do the dance described in
            // https://github.com/KSP-CKAN/CKAN/blob/master/Spec.md#version-ordering

            Comparison comp;
            comp.remainder1 = this.version;
            comp.remainder2 = that.version;

            // Process our strings while there are characters remaining
            while (comp.remainder1.Length > 0 && comp.remainder2.Length > 0) {

                // Start by comparing the string parts.
                comp = StringComp (comp.remainder1, comp.remainder2);

                // If we've found a difference, return it.
                if (comp.compare_to != 0) {
                    return comp.compare_to;
                }

                // Otherwise, compare the number parts.
                // It's okay not to check if our strings are exhausted, because
                // if they are the exhausted parts will return zero.

                comp = NumComp (comp.remainder1, comp.remainder2);

                // Again, return difference if found.
                if (comp.compare_to != 0) {
                    return comp.compare_to;
                }
            }

            // Oh, we've run out of one or both strings.
            // They *can't* be equal, because we would have detected that in our first test.
            // So, whichever version is empty first is the smallest. (1.2 < 1.2.3)

            if (comp.remainder1.Length == 0) {
                return -1;
            }

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

        /// <summary>
        /// Compare the leading non-numerical parts of two strings
        /// </summary>
       
        public static Comparison StringComp(string v1, string v2)
        {
            var comp = new Comparison();
            comp.remainder1 = "";
            comp.remainder2 = "";

            var minimumLength1 = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                if (Char.IsNumber(v1[i]))
                {
                    comp.remainder1 = v1.Substring(i);
                    break;
                }

                minimumLength1++;
            }

            int minimumLength2 = 0;
            for (int i = 0; i < v2.Length; i++)
            {
                if (Char.IsNumber(v2[i]))
                {
                    comp.remainder2 = v2.Substring(i);
                    break;
                }

                minimumLength2++;
            }

            int minimumLength = Math.Min(minimumLength1, minimumLength2);
            if (minimumLength == 0)
            {
                if (minimumLength1 < minimumLength2)
                {
                    comp.compare_to = -1;
                    return comp;
                }
                else if (minimumLength1 > minimumLength2)
                {
                    comp.compare_to = 1;
                    return comp;
                }
            }

            for (int i = 0; i < minimumLength; i++)
            {
                if (v1[i] < v2[i])
                {
                    comp.compare_to = -1;
                    return comp;
                }
                else if (v1[i] > v2[i])
                {
                    comp.compare_to = 1;
                    return comp;
                }
            }

            comp.compare_to = 0;
            return comp;
        }

        /// <summary>
        /// Compare the leading numerical parts of two strings
        /// </summary>

        public static Comparison NumComp(string v1, string v2)
        {
            var comp = new Comparison();
            comp.remainder1 = "";
            comp.remainder2 = "";

            var minimumLength1 = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                if (!Char.IsNumber(v1[i]))
                {
                    comp.remainder1 = v1.Substring(i);
                    break;
                }

                minimumLength1++;
            }

            int minimumLength2 = 0;
            for (int i = 0; i < v2.Length; i++)
            {
                if (!Char.IsNumber(v2[i]))
                {
                    comp.remainder2 = v2.Substring(i);
                    break;
                }

                minimumLength2++;
            }

            int integer1 = int.Parse(v1.Substring(0, minimumLength1));
            int integer2 = int.Parse(v2.Substring(0, minimumLength2));

            comp.compare_to = integer1.CompareTo(integer2);
            return comp;
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
            return "autodetected dll";
        }
    }

    /// <summary>
    /// This class represents a virtual version that was provided by
    /// another module.
    /// </summary>
    public class ProvidesVersion : Version {
        internal string provided_by;

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

