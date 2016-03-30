using CKAN.Versioning;

namespace CKAN.GameVersionProviders
{
    public interface IKspBuildMap
    {
        KspVersion this[string buildId] { get; }

        void Refresh();
    }
}
