namespace CKAN.NetKAN.Sources.Kerbalstuff
{
    internal interface IKerbalstuffApi
    {
        /// <summary>
        /// Given a mod Id, returns a KSMod with its metadata from the network.
        /// </summary>
        KerbalstuffMod GetMod(int modId);
    }
}
