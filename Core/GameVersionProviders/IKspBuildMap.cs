using CKAN.Versioning;
using System.Collections.Generic;

namespace CKAN.GameVersionProviders
{
    public interface IKspBuildMap
    {
        GameVersion this[string buildId] { get; }

        List<GameVersion> KnownVersions { get; }

        void Refresh();
    }
}
