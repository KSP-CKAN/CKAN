using System;
using System.Collections.Generic;
using CKAN.Exporters;
using CKAN.Types;
using CKAN.Versioning;
using log4net;

namespace CKAN.CmdLine
{
    public class List : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(List));

        public IUser user { get; set; }

        public List(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.GameInstance ksp, object raw_options)
        {
            ListOptions options = (ListOptions) raw_options;

            IRegistryQuerier registry = RegistryManager.Instance(ksp).registry;

            ExportFileType? exportFileType = null;

            if (!string.IsNullOrWhiteSpace(options.export))
            {
                exportFileType = GetExportFileType(options.export);

                if (exportFileType == null)
                {
                    user.RaiseError("Unknown export format: {0}", options.export);
                }
            }

            if (!(options.porcelain) && exportFileType == null)
            {
                user.RaiseMessage("\r\nKSP found at {0}\r\n", ksp.GameDir());
                user.RaiseMessage("KSP Version: {0}\r\n", ksp.Version());

                user.RaiseMessage("Installed Modules:\r\n");
            }

            if (exportFileType == null)
            {
                var installed = new SortedDictionary<string, ModuleVersion>(registry.Installed());

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
                            CkanModule latest = registry.LatestAvailable(mod.Key, ksp.VersionCriteria());
                            CkanModule current = registry.GetInstalledVersion(mod.Key);
                            InstalledModule inst = registry.InstalledModule(mod.Key);

                            if (latest == null)
                            {
                                // Not compatible!
                                log.InfoFormat("Latest {0} is not compatible", mod.Key);
                                bullet = "X";
                                if ( current == null ) log.DebugFormat( " {0} installed version not found in registry", mod.Key);
                                    
                                // Check if mod is replaceable
                                if (current.replaced_by != null)
                                {
                                    ModuleReplacement replacement = registry.GetReplacement(mod.Key, ksp.VersionCriteria());
                                    if (replacement != null)
                                    {
                                        // Replaceable!
                                        bullet = ">";
                                        modInfo = string.Format("{0} > {1} {2}", modInfo, replacement.ReplaceWith.name, replacement.ReplaceWith.version);
                                    }
                                }
                            }
                            else if (latest.version.IsEqualTo(current_version))
                            {
                                // Up to date
                                log.InfoFormat("Latest {0} is {1}", mod.Key, latest.version);
                                bullet = (inst?.AutoInstalled ?? false) ? "+" : "-";
                                // Check if mod is replaceable
                                if (current.replaced_by != null)
                                {
                                    ModuleReplacement replacement = registry.GetReplacement(latest.identifier, ksp.VersionCriteria());
                                    if (replacement != null)
                                    {
                                        // Replaceable!
                                        bullet = ">";
                                        modInfo = string.Format("{0} > {1} {2}", modInfo, replacement.ReplaceWith.name, replacement.ReplaceWith.version);
                                    }
                                }
                            }
                            else if (latest.version.IsGreaterThan(mod.Value))
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
                var stream = Console.OpenStandardOutput();
                new Exporter(exportFileType.Value).Export(registry, stream);
                stream.Flush();
            }

            if (!(options.porcelain) && exportFileType == null)
            {
                user.RaiseMessage("\r\nLegend: -: Up to date. +:Auto-installed. X: Incompatible. ^: Upgradable. >: Replaceable\r\n        A: Autodetected. ?: Unknown. *: Broken. ");
                // Broken mods are in a state that CKAN doesn't understand, and therefore can't handle automatically
            }

            return Exit.OK;
        }

        private static ExportFileType? GetExportFileType(string export)
        {
            export = export.ToLowerInvariant();

            switch (export)
            {
                case "text":     return ExportFileType.PlainText;
                case "markdown": return ExportFileType.Markdown;
                case "bbcode":   return ExportFileType.BbCode;
                case "csv":      return ExportFileType.Csv;
                case "tsv":      return ExportFileType.Tsv;
                default:         return null;
            }
        }
    }
}
