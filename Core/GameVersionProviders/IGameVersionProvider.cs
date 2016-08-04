using CKAN.Versioning;

namespace CKAN.GameVersionProviders
{
    public interface IGameVersionProvider
    {
        bool TryGetVersion(string directory, out KspVersion result);
    }
}