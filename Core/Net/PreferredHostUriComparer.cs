using System;
using System.Linq;
using System.Collections.Generic;

namespace CKAN
{
    public class PreferredHostUriComparer : IComparer<Uri>
    {
        public PreferredHostUriComparer(IEnumerable<string> hosts)
        {
            this.hosts      = (hosts ?? Enumerable.Empty<string>()).ToList();
            // null represents the position in the list for all other hosts
            defaultPriority = this.hosts.IndexOf(null);
        }

        public int Compare(Uri a, Uri b)
            => GetPriority(a).CompareTo(GetPriority(b));

        private int GetPriority(Uri u)
        {
            var index = hosts.IndexOf(u.Host);
            return index == -1 ? defaultPriority
                               : index;
        }

        private readonly List<string> hosts;
        private readonly int          defaultPriority;
    }
}
