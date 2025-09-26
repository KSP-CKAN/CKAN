using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CKAN
{
    public class NameComparer : IEqualityComparer<CkanModule>
    {
        [ExcludeFromCodeCoverage]
        public bool Equals(CkanModule? x, CkanModule? y)
            => x?.identifier.Equals(y?.identifier)
                ?? (y == null);

        public int GetHashCode(CkanModule obj)
            => obj.identifier.GetHashCode();
    }
}
