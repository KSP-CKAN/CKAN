using System;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;
using KSPMMCfgParser;

namespace CKAN.NetKAN.Services
{
    internal interface IConfigParser
    {
        Dictionary<InstallableFile, KSPConfigNode[]> GetConfigNodes(CkanModule module, ZipFile zip, GameInstance inst);
    }
}
