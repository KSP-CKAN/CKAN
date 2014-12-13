using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CKAN {
    [JsonConverter(typeof(JsonSimpleStringConverter))]
    public class KSPVersion : IComparable<KSPVersion> {
        private string version;
        private Version cachedVersionObject;

        public KSPVersion (string v) {
            version = Normalise(v);
            version = AnyToNull(version);
            Validate (); // Throws on error.
        }

        // Casting function
        public static explicit operator KSPVersion(string v) {
            return new KSPVersion (v);
        }

        /// <summary>
        /// Normalises a version number. Currently this adds
        /// a leading zero if it's missing one.
        /// </summary>
        /// <param name="v">V.</param>
        private static string Normalise(string v)
        {
            if (v == null)
            {
                return null;
            }

            if (Regex.IsMatch(v, @"^\."))
            {
                return "0" + v;
            }

            return v;
        }

        // 0.25 -> 0.25.0
        public void ToLongMin() {
            if (IsShortVersion()) {
                version = version + ".0";
            }
        }

        // 0.25 -> 0.25.99
        public void ToLongMax() {
            if (IsShortVersion ()) {
                version = version + ".99"; // Ugh, magic number.
            }
        }

        // True for short version (eg: 0.25), false for long (eg: 0.25.2).
        public bool IsShortVersion() {
            if (version == null) {
                return false;
            }
            else if (Regex.IsMatch (version, @"^\d+\.\d+$")) {
                return true;
            }
            return false;
        }

        public bool IsLongVersion() {
            if (version == null) {
                return false;
            }
            else if (Regex.IsMatch (version, @"^\d+\.\d+\.\d+$")) {
                return true;
            }
            return false;
        }

        public bool IsAny() {
            return version == null;
        }

        public bool IsNotAny() {
            return ! IsAny ();
        }

        public string Version() {
            return version;
        }

        // Private for now, since we can't guarnatee public code will only call
        // us with long versions.
        private Version VersionObject()
        {
            if (cachedVersionObject == null)
            {
                cachedVersionObject = new Version(version);
            }

            return cachedVersionObject;
        }

        public int CompareTo(KSPVersion that) {

            // We need two long versions to be able to compare properly.
            if ((! this.IsLongVersion ()) && (! that.IsLongVersion ())) {
                throw new KSPVersionIncomparableException (this, that, "CompareTo");
            }

            // Hooray, we can hook the regular Version code here.

            Version v1 = this.VersionObject();
            Version v2 = that.VersionObject();

            return v1.CompareTo (v2);

        }

        // Returns true if this targets that version of KSP.
        // That must be a long (actual) version.
        // Eg: 0.25 targets 0.25.2

        public bool Targets(KSPVersion that) {

            // Cry if we're not looking at a long version to compare to.
            if (! that.IsLongVersion()) {
                throw new KSPVersionIncomparableException (this, that, "Targets");
            }

            // If we target any, then yes, it's a match.
            if (this.IsAny()) {
                return true;
            } else if (this.IsLongVersion()) {
                return this.CompareTo (that) == 0;
            }

            // We've got a short version, so split it into two separate versions,
            // and compare each.

            KSPVersion min = new KSPVersion (this.Version());
            min.ToLongMin ();

            KSPVersion max = new KSPVersion (this.Version());
            max.ToLongMax ();

            return (that >= min && that <= max);

        }

        // "any" -> null
        private static string AnyToNull(string v) {
            if (v != null && v == "any") {
                return null;
            }
            return v;
        }

        // Throws on error.
        private void Validate() {
            if (version == null || IsShortVersion() || IsLongVersion()) {
                return;
            }
            throw new BadKSPVersionException (version);
        }

        // Why don't I get operator overloads for free?
        // Is there a class I can delcare allegiance to that gives me this?
        // Where's my ComparableOperators role?

        public static bool operator <(KSPVersion v1, KSPVersion v2) {
            return v1.CompareTo(v2) < 0;
        }

        public static bool operator <=(KSPVersion v1, KSPVersion v2) {
            return v1.CompareTo (v2) <= 0;
        }

        public static bool operator >(KSPVersion v1, KSPVersion v2) {
            return v1.CompareTo (v2) > 0;
        }

        public static bool operator >=(KSPVersion v1, KSPVersion v2) {
            return v1.CompareTo (v2) >= 0;
        }

        public override string ToString () {
            return this.Version();
        }

    }

    public class BadKSPVersionException : Exception {
        private string version;

        public BadKSPVersionException(string v) {
            version = v;
        }

        public override string ToString ()
        {
            return string.Format ("[BadKSPVersionException] {0} is not a valid KSP version", version);
        }
    }

    public class KSPVersionIncomparableException : Exception {
        private string version1;
        private string version2;
        private string method;

        public KSPVersionIncomparableException(KSPVersion v1, KSPVersion v2, string m) {
            version1 = v1.Version();
            version2 = v2.Version();
            method    = m;
        }

        public override string ToString ()
        {
            return string.Format ("[KSPVersionIncomparableException] {0} and {1} cannot be compared by {2}", version1, version2, method);
        }
    }
}

