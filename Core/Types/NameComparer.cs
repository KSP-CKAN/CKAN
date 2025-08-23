using System.Collections.Generic;

namespace CKAN
{
    public class NameComparer : IEqualityComparer<CkanModule>
    {
        public bool Equals(CkanModule? x, CkanModule? y)
            => x?.identifier.Equals(y?.identifier)
                ?? (y == null);

        public int GetHashCode(CkanModule obj)
            => obj.identifier.GetHashCode();
    }
}
