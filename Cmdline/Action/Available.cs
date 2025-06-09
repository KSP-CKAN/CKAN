using System.Collections.Generic;
using System.Linq;

using CommandLine;
using DotNet.Globbing;

namespace CKAN.CmdLine
{
    public class Available : ICommand
    {
        public Available(RepositoryDataManager repoData, IUser user)
        {
            this.repoData = repoData;
            this.user     = user;
        }

        public int RunCommand(CKAN.GameInstance instance, object raw_options)
        {
            AvailableOptions opts = (AvailableOptions)raw_options;

            IEnumerable<CkanModule> compatible = RegistryManager
                .Instance(instance, repoData)
                .registry
                .CompatibleModules(instance.StabilityToleranceConfig,
                                   instance.VersionCriteria())
                .Where(m => !m.IsDLC)
                .OrderBy(m => m.identifier);

            if (opts.globs is { Count: > 0 })
            {
                GlobOptions.Default.Evaluation.CaseInsensitive = true;
                var globs = opts.globs.Select(Glob.Parse).ToArray();
                compatible = compatible.Where(m => globs.Any(gl => gl.IsMatch(m.identifier)
                                                                   || gl.IsMatch(m.name)));
            }

            user.RaiseMessage(Properties.Resources.AvailableHeader,
                              instance.game.ShortName,
                              instance.Version()?.ToString() ?? "");
            user.RaiseMessage("");

            if (opts.detail)
            {
                foreach (CkanModule module in compatible)
                {
                    user.RaiseMessage("* {0} ({1}) - {2} - {3}",
                                      module.identifier, module.version,
                                      module.name, module.@abstract);
                }
            }
            else
            {
                foreach (CkanModule module in compatible)
                {
                    user.RaiseMessage("* {0} ({1}) - {2}",
                                      module.identifier, module.version,
                                      module.name);
                }
            }

            return Exit.OK;
        }

        private readonly IUser                 user;
        private readonly RepositoryDataManager repoData;
    }

    internal class AvailableOptions : InstanceSpecificOptions
    {
        [Option("detail", HelpText = "Show short description of each module")]
        public bool detail { get; set; }

        [ValueList(typeof(List<string>))]
        public List<string>? globs { get; set; }
    }

}
