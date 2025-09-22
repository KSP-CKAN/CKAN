using System;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;

using CKAN.IO;
using CKAN.Avc;
using CKAN.SpaceWarp;
using CKAN.NetKAN.Sources.Github;

namespace CKAN.NetKAN.Services
{
    internal interface IModuleService
    {
        Tuple<ZipEntry, bool>? FindInternalAvc(CkanModule module, ZipFile zipfile, string internalFilePath);
        AvcVersion? GetInternalAvc(CkanModule module, string filePath, string? internalFilePath = null);
        JObject? GetInternalCkan(CkanModule module, string zipPath);
        bool HasInstallableFiles(CkanModule module, string filePath);

        IEnumerable<InstallableFile> GetConfigFiles(CkanModule module, ZipFile zip);
        IEnumerable<InstallableFile> GetPlugins(CkanModule module, ZipFile zip);
        IEnumerable<InstallableFile> GetCrafts(CkanModule module, ZipFile zip);
        IEnumerable<InstallableFile> GetSourceCode(CkanModule module, ZipFile zip);

        SpaceWarpInfo? ParseSpaceWarpJson(string? json);
        SpaceWarpInfo? GetInternalSpaceWarpInfo(CkanModule module, ZipFile zip, string? internalFilePath = null);
        SpaceWarpInfo? GetSpaceWarpInfo(CkanModule module, ZipFile zip, IGithubApi githubApi, IHttpService httpSvc, string? internalFilePath = null);

        IEnumerable<ZipEntry> FileSources(CkanModule module, ZipFile zip);
        IEnumerable<string> FileDestinations(CkanModule module, string filePath);
    }
}
