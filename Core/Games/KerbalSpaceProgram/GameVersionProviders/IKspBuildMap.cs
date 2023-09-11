using CKAN.Versioning;
using System.Collections.Generic;

namespace CKAN.Games.KerbalSpaceProgram.GameVersionProviders
{
    public interface IKspBuildMap
    {
        GameVersion this[string buildId] { get; }

        List<GameVersion> KnownVersions { get; }

        /// <summary>
        /// Download the build map from the server to the cache
        /// </summary>
        void Refresh();
    }
}
