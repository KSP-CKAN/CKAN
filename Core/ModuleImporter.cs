using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;
using YamlDotNet.RepresentationModel;

using CKAN.Extensions;
using CKAN.Versioning;
using CKAN.Avc;
using CKAN.SpaceWarp;

namespace CKAN
{
    public static class ModuleImporter
    {
        /// <summary>
        /// Import a list of files into the download cache, with progress bar and
        /// interactive prompts for installation and deletion.
        /// </summary>
        /// <param name="files">Set of files to import</param>
        /// <param name="user">Object for user interaction</param>
        /// <param name="installMod">Function to call to mark a mod for installation</param>
        /// <param name="allowDelete">True to ask user whether to delete imported files, false to leave the files as is</param>
        public static bool ImportFiles(HashSet<FileInfo>  files,
                                       IUser              user,
                                       Action<CkanModule> installMod,
                                       Registry           registry,
                                       GameInstance       instance,
                                       NetModuleCache     Cache,
                                       bool               allowDelete = true)
        {
            if (!TryGetFileModules(files, registry, Cache,
                                   out Dictionary<FileInfo, List<CkanModule>> matched,
                                   out List<FileInfo> notFound,
                                   new ProgressImmediate<int>(p =>
                                       user.RaiseProgress(Properties.Resources.ModuleInstallerImportScanningFiles,
                                                          p))))
            {
                // We're not going to do anything, so let the user know they failed
                user.RaiseError(Properties.Resources.ModuleInstallerImportNotFound,
                                string.Join(", ", notFound.Select(fi => fi.Name)));
                return false;
            }

            if (notFound.Count > 0)
            {
                // At least one was found, so just warn about the rest
                user.RaiseMessage(" ");
                user.RaiseMessage(Properties.Resources.ModuleInstallerImportNotFound,
                                  string.Join(", ", notFound.Select(fi => fi.Name)));
            }
            var installable = matched.Values.SelectMany(modules => modules)
                                            .Where(m => registry.IdentifierCompatible(m.identifier,
                                                                                      instance.StabilityToleranceConfig,
                                                                                      instance.VersionCriteria()))
                                            .ToHashSet();

            var deletable = matched.Keys.ToList();
            var delete    = allowDelete
                            && deletable.Count > 0
                            && user.RaiseYesNoDialog(string.Format(Properties.Resources.ModuleInstallerImportDeletePrompt,
                                                                   deletable.Count));

            // Store once per "primary" URL since each has its own URL hash
            var cachedGroups = matched.SelectMany(kvp => kvp.Value.DistinctBy(m => m.download?.First())
                                                                  .Select(m => (File:   kvp.Key,
                                                                                Module: m)))
                                      .GroupBy(tuple => Cache.IsMaybeCachedZip(tuple.Module))
                                      .ToDictionary(grp => grp.Key,
                                                    grp => grp.ToArray());
            if (cachedGroups.TryGetValue(true, out (FileInfo File, CkanModule Module)[]? alreadyStored))
            {
                // Notify about files that are already cached
                user.RaiseMessage(" ");
                user.RaiseMessage(Properties.Resources.ModuleInstallerImportAlreadyCached,
                                  string.Join(", ", alreadyStored.Select(tuple => $"{tuple.Module} ({tuple.File.Name})")));
            }
            if (cachedGroups.TryGetValue(false, out (FileInfo File, CkanModule Module)[]? toStore))
            {
                // Store any new files
                user.RaiseMessage(" ");
                var description = "";
                long installedBytes = 0;
                var rateCounter = new ByteRateCounter()
                {
                    Size      = toStore.Sum(tuple => tuple.File.Length),
                    BytesLeft = toStore.Sum(tuple => tuple.File.Length),
                };
                rateCounter.Start();
                var progress = new ProgressImmediate<long>(bytes =>
                {
                    rateCounter.BytesLeft = rateCounter.Size - (installedBytes + bytes);
                    user.RaiseProgress(rateCounter);
                });
                foreach ((FileInfo fi, CkanModule module) in toStore)
                {

                    // Update the progress string
                    description = $"{module} ({fi.Name})";
                    Cache.Store(module, fi.FullName, progress,
                                // Move if user said we could delete and we don't need to make any more copies
                                move: delete && toStore.Last(tuple => tuple.File == fi).Module == module,
                                // Skip revalidation because we had to check the hashes to get here!
                                validate: false);
                    installedBytes += fi.Length;
                }
                rateCounter.Stop();
            }

            // Here we have installable containing mods that can be installed, and the importable files have been stored in cache.
            if (installable.Count > 0
                && user.RaiseYesNoDialog(string.Format(Properties.Resources.ModuleInstallerImportInstallPrompt,
                                                       installable.Count,
                                                       instance.Name,
                                                       instance.GameDir())))
            {
                // Install the imported mods
                foreach (var mod in installable)
                {
                    installMod(mod);
                }
            }
            if (delete)
            {
                // Delete old files that weren't already moved into cache
                foreach (var f in deletable.Where(f => f.Exists))
                {
                    f.Delete();
                }
            }

            return true;
        }

        private static bool TryGetFileModules(HashSet<FileInfo>                          files,
                                              Registry                                   registry,
                                              NetModuleCache                             Cache,
                                              out Dictionary<FileInfo, List<CkanModule>> matched,
                                              out List<FileInfo>                         notFound,
                                              IProgress<int>                             percentProgress)
        {
            matched      = new Dictionary<FileInfo, List<CkanModule>>();
            notFound     = new List<FileInfo>();
            var index    = registry.GetDownloadHashesIndex();
            var progress = new ProgressScalePercentsByFileSizes(percentProgress,
                                                                files.Select(fi => fi.Length));
            foreach (var fi in files.Distinct())
            {
                if (index.TryGetValue(Cache.GetFileHashSha256(fi.FullName, progress),
                                      out List<CkanModule>? modules)
                    // The progress bar will jump back and "redo" the same span
                    // for non-matched files, but that's... OK?
                    || index.TryGetValue(Cache.GetFileHashSha1(fi.FullName, progress),
                                         out modules))
                {
                    matched.Add(fi, modules);
                }
                else if (GetInternalModules(new ZipFile(fi.FullName)).ToList() is var ckans
                         && ckans.Count > 0)
                {
                    matched.Add(fi, ckans);
                }
                else
                {
                    notFound.Add(fi);
                }
                progress.NextFile();
            }
            return matched.Count > 0;
        }

        private static readonly Regex avcRegex    = new Regex(@"^#/ckan/ksp-avc(/(?<filter>.*))?$",
                                                              RegexOptions.Compiled);

        private static readonly Regex swInfoRegex = new Regex(@"^#/ckan/space-warp(/(?<filter>.*))?$",
                                                              RegexOptions.Compiled);

        private static IEnumerable<CkanModule> GetInternalModules(ZipFile zip)
        {
            var internalCkans = InternalCkanFiles(zip).ToArray();
            // CkanModule.download is required for cache management, set a default if missing
            foreach (var ckan in internalCkans)
            {
                ckan["download"] ??= $"https://ckan/imported-from-zip/{ckan["identifier"]}/{zip.Name}";
            }
            // Set the version and compatibility if we can
            foreach (var grp in internalCkans.GroupBy(ckan => (string?)ckan["$vref"] ?? ""))
            {
                if (avcRegex.TryMatch(grp.Key, out Match? avcMatch))
                {
                    var filter = avcMatch.Groups["filter"].Success
                                     ? avcMatch.Groups["filter"].ToString()
                                     : null;
                    ApplyAvcs(InternalVersionFiles(zip, filter).ToArray(), grp);
                }
                else if (swInfoRegex.TryMatch(grp.Key, out Match? swInfoMatch))
                {
                    var filter = swInfoMatch.Groups["filter"].Success
                                     ? swInfoMatch.Groups["filter"].ToString()
                                     : null;
                    ApplySpaceWarpInfos(InternalSpaceWarpInfos(zip, filter).ToArray(), grp);
                }
            }
            return internalCkans.Select(json => json.ToObject<CkanModule>())
                                .OfType<CkanModule>();
        }

        private static void ApplyAvcs(AvcVersion[]         avcs,
                                      IEnumerable<JObject> ckans)
        {
            if (avcs.Length > 1)
            {
                throw new Kraken(string.Format("Too many .version files found!"));
            }
            foreach (var avc in avcs)
            {
                foreach (var ckan in ckans)
                {
                    ckan.Remove("$vref");
                    if (avc.version is ModuleVersion ver)
                    {
                        ckan["version"] ??= ver.ToString();
                    }
                    GameVersion.SetJsonCompatibility(ckan, avc.ksp_version,
                                                           avc.ksp_version_min,
                                                           avc.ksp_version_max);
                }
            }
        }

        private static void ApplySpaceWarpInfos(SpaceWarpInfo[]      swInfos,
                                                IEnumerable<JObject> ckans)
        {
            if (swInfos.Length > 1)
            {
                throw new Kraken(string.Format("Too many swinfo.json files found!"));
            }
            foreach (var swInfo in swInfos)
            {
                bool hasMin = GameVersion.TryParse(swInfo.ksp2_version?.min, out GameVersion? minVer);
                bool hasMax = GameVersion.TryParse(swInfo.ksp2_version?.max, out GameVersion? maxVer);
                foreach (var ckan in ckans)
                {
                    ckan.Remove("$vref");
                    ckan["version"] ??= swInfo.version;
                    if (hasMin || hasMax)
                    {
                        GameVersion.SetJsonCompatibility(ckan, null,
                                                               minVer?.WithoutBuild,
                                                               maxVer?.WithoutBuild);
                    }
                }
            }
        }

        private static IEnumerable<JObject> InternalCkanFiles(ZipFile zip)
            => GetInternalYamlFiles(zip, ".ckan", null);

        private static IEnumerable<AvcVersion> InternalVersionFiles(ZipFile zip, string? filter)
            => GetInternalJsonFiles(zip, ".version", filter)
                   .Select(json => json.ToObject<AvcVersion>())
                   .OfType<AvcVersion>();

        private static IEnumerable<SpaceWarpInfo> InternalSpaceWarpInfos(ZipFile zip, string? filter)
            => GetInternalJsonFiles(zip, "swinfo.json", filter)
                   .Select(json => json.ToObject<SpaceWarpInfo>())
                   .OfType<SpaceWarpInfo>();

        private static IEnumerable<JObject> GetInternalYamlFiles(ZipFile zip, string nameSuffix, string? filter)
            => FilterEntries(zip, nameSuffix, filter)
                   .SelectMany(entry =>
                               {
                                   var stream = new YamlStream();
                                   stream.Load(new StreamReader(zip.GetInputStream(entry)));
                                   return stream.Documents.Select(doc => doc?.RootNode)
                                                          .OfType<YamlMappingNode>()
                                                          .Select(yaml => yaml.ToJObject());
                               });

        private static IEnumerable<JObject> GetInternalJsonFiles(ZipFile zip, string nameSuffix, string? filter)
            => FilterEntries(zip, nameSuffix, filter)
                   .Select(entry => JObject.Parse(new StreamReader(zip.GetInputStream(entry)).ReadToEnd()));

        private static IEnumerable<ZipEntry> FilterEntries(ZipFile zip, string nameSuffix, string? filter)
            => zip.OfType<ZipEntry>()
                  .Where(entry => entry.Name.EndsWith(nameSuffix, StringComparison.InvariantCultureIgnoreCase)
                                  && (filter == null
                                      || entry.Name == filter
                                      || Regex.IsMatch(entry.Name, filter)));

    }
}
