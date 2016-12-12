using System;
using System.Collections.Generic;

namespace CKAN.Versioning
{
    public class KspVersionCriteria
    {
        private List<KspVersion> versions = new List<KspVersion> ();

        public KspVersionCriteria (KspVersion v)
        {
            this.versions.Add (v);
        }

        public KspVersionCriteria(KspVersion v, List<KspVersion> compatibleVersions)
        {
            this.versions.Add(v);
            this.versions.AddRange(compatibleVersions);
        }

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }

        public List<KspVersion> Versions {
            get {
                return versions;

            }
        }

        public override String ToString()
        {
            return "[Versions: " + versions.ToString() + "]";
        }
    }
}
