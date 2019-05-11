using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
﻿using CKAN.NetKAN.Sources.Avc;

namespace CKAN.NetKAN.Services
{
    internal interface IModuleService
    {
        AvcVersion GetInternalAvc(CkanModule module, string filePath, string internalFilePath = null);
        JObject GetInternalCkan(string filePath);
        bool HasInstallableFiles(CkanModule module, string filePath);
        
        IEnumerable<InstallableFile> GetConfigFiles(CkanModule module, ZipFile zip);
        
    }
}
