namespace CKAN.NetKAN.Sources.Curse
{
    internal interface ICurseApi
    {
        /// <summary>
        /// Given a mod name or id, returns a CurseMod with its metadata from the network
        /// </summary>
        CurseMod GetMod(string nameOrId);
    }
}
