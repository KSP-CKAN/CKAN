namespace CKAN.NetKAN.Sources.Spacedock
{
    internal interface ISpacedockApi
    {
        /// <summary>
        /// Given a mod Id, returns a SDMod with its metadata from the network.
        /// </summary>
        SpacedockMod GetMod(int modId);
    }
}