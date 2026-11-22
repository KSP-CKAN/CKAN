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
        public SpaceWarpInfoValidator(ISpaceWarpInfoLoader loader,
                                      IModuleService       modSvc,
                                      ICacheService        cacheSvc)
        {
            this.loader   = loader;
            this.modSvc   = modSvc;
            this.cacheSvc = cacheSvc;
        }

        public void Validate(Metadata metadata)
        {
            var moduleJson = metadata.AllJson;
            var mod = CkanModule.FromJson(moduleJson.ToString());

            cacheSvc.ExecuteCached(metadata, typeof(SpaceWarpInfoValidator), file =>
            {
                if (file is null)
                {
                    return;
                }

                using var zip = new ZipFile(file);
                if (modSvc.GetInternalSpaceWarpInfos(mod, zip, null)
                         .Select(loader.Load)
                         .FirstOrDefault()
                   is not SpaceWarpInfo swinfo)
                {
                    return;
                }

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
            });
        }

        private readonly ISpaceWarpInfoLoader loader;
        private readonly IModuleService       modSvc;
        private readonly ICacheService        cacheSvc;

        private static readonly ILog log = LogManager.GetLogger(typeof(SpaceWarpInfoValidator));
    }
}
