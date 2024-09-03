using CKAN.Versioning;
using System.Diagnostics.CodeAnalysis;

namespace CKAN.Games.KerbalSpaceProgram.GameVersionProviders
{
    public interface IGameVersionProvider
    {
        bool TryGetVersion(string directory,
                           [NotNullWhen(returnValue: true)] out GameVersion? result);
    }
}
