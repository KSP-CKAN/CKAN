namespace CKAN.NetKAN.Sources.SourceForge
{
    internal interface ISourceForgeApi
    {
        SourceForgeMod GetMod(SourceForgeRef sfRef);
    }
}
