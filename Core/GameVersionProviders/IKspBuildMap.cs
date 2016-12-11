using CKAN.Versioning;
using System.Collections.Generic;

namespace CKAN.GameVersionProviders
{
    public interface IKspBuildMap
    {
        KspVersion this[string buildId] { get; }

        List<KspVersion> getKnownVersions();

        void Refresh();
    }
}
