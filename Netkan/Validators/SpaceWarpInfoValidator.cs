using System;
using System.Linq;

using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.SpaceWarp;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;

namespace CKAN.NetKAN.Validators
{
    internal sealed class SpaceWarpInfoValidator : IValidator
    {
        public SpaceWarpInfoValidator(IHttpService         httpSvc,
                                      ISpaceWarpInfoLoader loader,
                                      IModuleService       modSvc)
        {
            this.httpSvc = httpSvc;
            this.loader  = loader;
            this.modSvc  = modSvc;
        }

        public void Validate(Metadata metadata)
        {
            var moduleJson = metadata.AllJson;
            var mod = CkanModule.FromJson(moduleJson.ToString());
            if (httpSvc.DownloadModule(metadata) is string file
                && new ZipFile(file) is ZipFile zip
                && modSvc.GetInternalSpaceWarpInfos(mod, zip, null)
                         .Select(loader.Load)
                         .FirstOrDefault()
                   is SpaceWarpInfo swinfo)
            {
                var moduleDeps = (mod.depends?.OfType<ModuleRelationshipDescriptor>()
                                              .Select(r => r.name)
                                  ?? Enumerable.Empty<string>())
                                  .ToHashSet();
                var missingDeps = (swinfo.dependencies
                                         ?.Select(dep => dep.id)
                                          .OfType<string>()
                                          .Where(depId => !moduleDeps.Contains(
                                              // Remove up to last period
                                              Identifier.Sanitize(
                                                  depId[(depId.LastIndexOf('.') + 1)..], ""),
                                              // Case insensitive
                                              StringComparer.InvariantCultureIgnoreCase))
                                         ?? Enumerable.Empty<string>())
                                          .ToList();
                if (missingDeps.Count != 0)
                {
                    log.WarnFormat("Dependencies from swinfo.json missing from module: {0}",
                                   string.Join(", ", missingDeps));
                }
            }
        }

        private readonly IHttpService         httpSvc;
        private readonly ISpaceWarpInfoLoader loader;
        private readonly IModuleService       modSvc;

        private static readonly ILog log = LogManager.GetLogger(typeof(SpaceWarpInfoValidator));
    }
}
