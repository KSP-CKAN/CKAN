using System;
using System.Linq;

using ICSharpCode.SharpZipLib.Zip;
using log4net;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.Games;
using CKAN.NetKAN.Sources.Github;
using CKAN.SpaceWarp;

namespace CKAN.NetKAN.Validators
{
    internal sealed class SpaceWarpInfoValidator : IValidator
    {
        public SpaceWarpInfoValidator(IHttpService   httpSvc,
                                      IGithubApi     githubApi,
                                      IModuleService modSvc,
                                      IGame          game)
        {
            this.httpSvc   = httpSvc;
            this.githubApi = githubApi;
            this.modSvc    = modSvc;
            this.game      = game;
        }

        public void Validate(Metadata metadata)
        {
            var moduleJson = metadata.Json();
            CkanModule    mod  = CkanModule.FromJson(moduleJson.ToString());
            GameInstance  inst = new GameInstance(game, "/", "dummy", new NullUser());
            if (httpSvc.DownloadModule(metadata) is string file
                && new ZipFile(file) is ZipFile zip
                && modSvc.GetSpaceWarpInfo(mod, zip, inst, githubApi, httpSvc) is SpaceWarpInfo swinfo)
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

        private readonly IHttpService   httpSvc;
        private readonly IGithubApi     githubApi;
        private readonly IModuleService modSvc;
        private readonly IGame          game;

        private static readonly ILog log = LogManager.GetLogger(typeof(SpaceWarpInfoValidator));
    }
}
