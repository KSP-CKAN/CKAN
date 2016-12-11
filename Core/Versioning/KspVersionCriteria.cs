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

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }

        public List<KspVersion> Versions {
            get {
                return versions;

            }
        }
    }
}
