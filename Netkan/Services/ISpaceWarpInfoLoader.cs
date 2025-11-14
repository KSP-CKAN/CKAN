using CKAN.SpaceWarp;

namespace CKAN.NetKAN.Services
{
    public interface ISpaceWarpInfoLoader
    {
        SpaceWarpInfo? Load(string spaceWarpInfo);
    }
}
