using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using CommandLine;

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
            AvailableOptions opts     = (AvailableOptions)raw_options;
            IRegistryQuerier registry = RegistryManager.Instance(instance, repoData).registry;

            IEnumerable<CkanModule> compatible = registry
                .CompatibleModules(instance.VersionCriteria())
                .Where(m => !m.IsDLC)
                .OrderBy(m => m.identifier);

            if (opts.globs != null)
            {
                var regexes = opts.globs
                                  .Select(str => new Regex("^"
                                                           + str.Replace("?", ".")
                                                                .Replace("*", ".*")
                                                           + "$",
                                                           RegexOptions.Compiled
                                                           | RegexOptions.IgnoreCase))
                                  .ToArray();
                compatible = compatible.Where(m => regexes.Any(re => re.IsMatch(m.identifier)
                                                                     || re.IsMatch(m.name)));
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
