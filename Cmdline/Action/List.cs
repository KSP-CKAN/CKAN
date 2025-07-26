using System.IO;
using System.Collections.Generic;
using System.Linq;

using CommandLine;
using log4net;

using CKAN.Exporters;
using CKAN.Types;
using CKAN.Versioning;

namespace CKAN.CmdLine
{
    public class List : ICommand
    {
        public List(RepositoryDataManager repoData, IUser user, Stream outputStream)
        {
            this.repoData     = repoData;
            this.user         = user;
            this.outputStream = outputStream;
        }

        public int RunCommand(CKAN.GameInstance instance, object raw_options)
        {
            ListOptions options = (ListOptions) raw_options;

            var regMgr   = RegistryManager.Instance(instance, repoData);
            var registry = regMgr.registry;

            ExportFileType? exportFileType = null;

            if (!string.IsNullOrWhiteSpace(options.export))
            {
                exportFileType = GetExportFileType(options.export);

                if (exportFileType == null)
                {
                    user.RaiseError(Properties.Resources.ListUnknownFormat,
                                    options.export ?? "");
                }
            }

            if (!options.porcelain && exportFileType == null)
            {
                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.ListGameFound,
                                  instance.game.ShortName,
                                  Platform.FormatPath(instance.GameDir()));
                if (instance.Version() is GameVersion gv)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ListGameVersion,
                                      instance.game.ShortName, gv);
                }
                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.ListGameModulesHeader);
                user.RaiseMessage("");
            }

            if (exportFileType == null)
            {
                var installed = new SortedDictionary<string, ModuleVersion>(registry.Installed());
                var upgradeable = registry
                                  .CheckUpgradeable(instance, new HashSet<string>())
                                  [true]
                                  .Select(m => m.identifier)
                                  .ToHashSet();

                foreach (KeyValuePair<string, ModuleVersion> mod in installed)
                {
                    ModuleVersion current_version = mod.Value;
                    string modInfo = string.Format("{0} {1}", mod.Key, mod.Value);
                    string bullet = "*";

                    if (current_version is ProvidesModuleVersion)
                    {
                        // Skip virtuals for now.
                        continue;
                    }
                    else if (current_version is UnmanagedModuleVersion)
                    {
                        // Autodetected dll
                        bullet = "A";
                    }
                    else
                    {
                        try
                        {
                            // Check if upgrades are available, and show appropriately.
                            log.DebugFormat("Check if upgrades are available for {0}", mod.Key);
                            var latest = registry.LatestAvailable(mod.Key,
                                                                  instance.StabilityToleranceConfig,
                                                                  instance.VersionCriteria());
                            var current = registry.GetInstalledVersion(mod.Key);
                            var inst = registry.InstalledModule(mod.Key);

                            if (latest == null)
                            {
                                // Not compatible!
                                log.InfoFormat("Latest {0} is not compatible", mod.Key);
                                bullet = "X";
                                if (current == null)
                                {
                                    log.DebugFormat(" {0} installed version not found in registry", mod.Key);
                                }
                                // Check if mod is replaceable
                                else if (current.replaced_by != null)
                                {
                                    var replacement = registry.GetReplacement(mod.Key,
                                                                              instance.StabilityToleranceConfig,
                                                                              instance.VersionCriteria());
                                    if (replacement != null)
                                    {
                                        // Replaceable!
                                        bullet = ">";
                                        modInfo = string.Format("{0} > {1} {2}", modInfo, replacement.ReplaceWith.name, replacement.ReplaceWith.version);
                                    }
                                }
                            }
                            else if (!upgradeable.Contains(mod.Key))
                            {
                                // Up to date
                                log.InfoFormat("Latest {0} is {1}", mod.Key, latest.version);
                                bullet = (inst?.AutoInstalled ?? false) ? "+" : "-";
                                // Check if mod is replaceable
                                if (current?.replaced_by != null)
                                {
                                    var replacement = registry.GetReplacement(latest.identifier,
                                                                              instance.StabilityToleranceConfig,
                                                                              instance.VersionCriteria());
                                    if (replacement != null)
                                    {
                                        // Replaceable!
                                        bullet = ">";
                                        modInfo = string.Format("{0} > {1} {2}", modInfo, replacement.ReplaceWith.name, replacement.ReplaceWith.version);
                                    }
                                }
                            }
                            else
                            {
                                // Upgradable
                                bullet = "^";
                            }
                        }
                        catch (ModuleNotFoundKraken)
                        {
                            log.InfoFormat("{0} is installed, but no longer in the registry", mod.Key);
                            bullet = "?";
                        }
                    }

                    user.RaiseMessage("{0} {1}", bullet, modInfo);
                }
            }
            else
            {
                new Exporter(exportFileType.Value).Export(regMgr, registry, outputStream);
            }

            if (!(options.porcelain) && exportFileType == null)
            {
                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.ListLegend);
                // Broken mods are in a state that CKAN doesn't understand, and therefore can't handle automatically
            }

            return Exit.OK;
        }

        private static ExportFileType? GetExportFileType(string? export)
            => export?.ToLowerInvariant() switch
            {
                "ckan"     => ExportFileType.Ckan,
                "text"     => ExportFileType.PlainText,
                "markdown" => ExportFileType.Markdown,
                "bbcode"   => ExportFileType.BbCode,
                "csv"      => ExportFileType.Csv,
                "tsv"      => ExportFileType.Tsv,
                _          => null,
            };

        private readonly RepositoryDataManager repoData;
        private readonly IUser                 user;
        private readonly Stream                outputStream;
        private static readonly ILog log = LogManager.GetLogger(typeof(List));
    }

    internal class ListOptions : InstanceSpecificOptions
    {
        [Option("porcelain", HelpText = "Dump raw list of modules, good for shell scripting")]
        public bool porcelain { get; set; }

        [Option("export", HelpText = "Format of module list: ckan, text, markdown, bbcode, csv, tsv")]
        public string? export { get; set; }
    }

}
