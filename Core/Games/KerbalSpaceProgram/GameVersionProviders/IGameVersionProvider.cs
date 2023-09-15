using CKAN.Versioning;

namespace CKAN.Games.KerbalSpaceProgram.GameVersionProviders
{
    public interface IGameVersionProvider
    {
        bool TryGetVersion(string directory, out GameVersion result);
    }
}
