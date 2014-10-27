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

        struct Comparison {
            public int compare_to;
            public string remainder1;
            public string remainder2;
        }

        public Version (string version_string) {
            orig_string = version_string;

            // TODO: Can we get rid of $1 here? Does C# support (?:syntax)?
            Match match = Regex.Match (version_string, "^(([0-9]+):)?(.*)$");

            // If we have an epoch, then record it.
            if (match.Groups [2].Value.Length > 0) {
                epoch = Convert.ToInt32( match.Groups [2].Value );
            }

            version = match.Groups [3].Value;
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
       
        static Comparison StringComp(string v1, string v2) {
            Comparison comp;

            // Extract the string section from each part.

            Match v1_match  = Regex.Match (v1, "^([^0-9]*)(.*)");
            string v1_str   = v1_match.Groups [1].Value;
            comp.remainder1 = v1_match.Groups [2].Value;

            Match v2_match  = Regex.Match (v2, "^([^0-9]*)(.*)");
            string v2_str   = v2_match.Groups [1].Value;
            comp.remainder2 = v2_match.Groups [2].Value;

            // Do the comparison
            comp.compare_to = v1_str.CompareTo (v2_str);

            return comp;
        }

        /// <summary>
        /// Compare the leading numerical parts of two strings
        /// </summary>

        static Comparison NumComp(string v1, string v2) {
            Comparison comp;

            Match v1_match  = Regex.Match (v1, "^([0-9]*)(.*)");
            int v1_int      = Convert.ToInt32(v1_match.Groups [1].Value);
            comp.remainder1 = v1_match.Groups [2].Value;

            Match v2_match  = Regex.Match (v2, "^([0-9]*)(.*)");
            int v2_int      = Convert.ToInt32( v2_match.Groups [1].Value);
            comp.remainder2 = v2_match.Groups [2].Value;

            comp.compare_to = v1_int.CompareTo (v2_int);

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

