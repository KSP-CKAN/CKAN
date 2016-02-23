namespace CKAN.NetKAN.Sources.Curse
{
    internal interface ICurseApi
    {
        /// <summary>
        /// Given a mod Id, returns a CurseMod with its metadata from the network.
        /// </summary>
        CurseMod GetMod(int modId);
    }
}
