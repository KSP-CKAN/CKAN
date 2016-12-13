using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN.Versioning
{
    public class KspVersionCriteria
    {
        private List<KspVersion> _versions = new List<KspVersion> ();

        public KspVersionCriteria (KspVersion v)
        {
            if (v != null)
            {
                this._versions.Add(v);
            }
        }

        public KspVersionCriteria(KspVersion v, List<KspVersion> compatibleVersions)
        {
            if(v != null)
            { 
                this._versions.Add(v);
            }
            this._versions.AddRange(compatibleVersions);
            this._versions = this._versions.Distinct().ToList();
        }

        public IList<KspVersion> Versions {
            get
            {
                return _versions.AsReadOnly();
            }
        }

        public override String ToString()
        {
            return "[Versions: " + _versions.ToString() + "]";
        }
    }
}
