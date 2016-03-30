using CKAN.Versioning;

namespace CKAN
{
    /// <summary>
    /// Used to compare if a mod is compatible with a game.
    /// </summary>
    public interface IGameComparator
    {
        /// <summary>
        /// Returns true if the given module is compatible with the supplied
        /// gameVersion, false otherwise.
        /// </summary>
        bool Compatible(KspVersion gameVersion, CkanModule module);
    }
}

