using System;

namespace CKAN
{
    public class Repository
    {
        public string name;
        public Uri uri;

        public Repository()
        {
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})", name, uri.DnsSafeHost);
        }
    }
}

