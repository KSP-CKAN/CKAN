using System;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Sources.Avc;
using CKAN.NetKAN.Sources.SpaceWarp;

namespace CKAN.NetKAN.Services
{
    internal interface IModuleService
    {
        Tuple<ZipEntry, bool> FindInternalAvc(CkanModule module, ZipFile zipfile, string internalFilePath);
        AvcVersion GetInternalAvc(CkanModule module, string filePath, string internalFilePath = null);
        JObject GetInternalCkan(CkanModule module, string zipPath, GameInstance inst);
        bool HasInstallableFiles(CkanModule module, string filePath);

        IEnumerable<InstallableFile> GetConfigFiles(CkanModule module, ZipFile zip, GameInstance inst);
        IEnumerable<InstallableFile> GetPlugins(CkanModule module, ZipFile zip, GameInstance inst);
        IEnumerable<InstallableFile> GetCrafts(CkanModule module, ZipFile zip, GameInstance inst);

        SpaceWarpInfo ParseSpaceWarpJson(string json);
        SpaceWarpInfo GetSpaceWarpInfo(CkanModule module, ZipFile zip, GameInstance inst, string internalFilePath = null);

        IEnumerable<ZipEntry> FileSources(CkanModule module, ZipFile zip, GameInstance inst);
        IEnumerable<string> FileDestinations(CkanModule module, string filePath);
    }
}
