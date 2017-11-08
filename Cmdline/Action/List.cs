using System;
using System.Collections.Generic;
using CKAN.Exporters;
using CKAN.Types;
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

        public int RunCommand(CKAN.KSP ksp, object raw_options)
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
                var installed = new SortedDictionary<string, Version>(registry.Installed());

                foreach (KeyValuePair<string, Version> mod in installed)
                {
                    Version current_version = mod.Value;
                    string modInfo = string.Format("{0} {1}", mod.Key, mod.Value);
                    string bullet = "*";

                    if (current_version is ProvidesVersion)
                    {
                        // Skip virtuals for now.
                        continue;
                    }
                    else if (current_version is DllVersion)
                    {
                        // Autodetected dll
                        bullet = "A";
                    }
                    else
                    {
                        try
                        {
                            // Check if upgrades are available, and show appropriately.
                            CkanModule latest = registry.LatestAvailable(mod.Key, ksp.VersionCriteria());
                            CkanModule current = registry.GetInstalledVersion(mod.Key);
                            
                            log.InfoFormat("Latest {0} is {1}", mod.Key, latest);

                            if (latest == null)
                            {
                                // Not compatible!
                                bullet = "X";
                                //Check if mod is replaceable
                                string newModInfo = ReplacmentModInfo (current.replaced_by, registry, ksp.VersionCriteria());
                                if ( newModInfo != null)
                                {
                                    bullet = ">";
                                    modInfo = string.Format("{0} > {1}", modInfo, newModInfo);
                                }
                            }
                            else if (latest.version.IsEqualTo(current_version))
                            {
                                // Up to date
                                bullet = "-";
                                //Check if mod is replaceable
                                string newModInfo = ReplacmentModInfo (current.replaced_by, registry, ksp.VersionCriteria());
                                if ( newModInfo != null)
                                {
                                    bullet = ">";
                                    modInfo = string.Format("{0} > {1}", modInfo, newModInfo);
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
                user.RaiseMessage("\r\nLegend: -: Up to date. X: Incompatible. ^: Upgradable. >: Replaceable\r\n        A: Autodetected. ?: Unknown. *: Broken. ");
                // Broken mods are in a state that CKAN doesn't understand, and therefore can't handle automatically
            }

            return Exit.OK;
        }

        private string ReplacmentModInfo (RelationshipDescriptor replacedBy, IRegistryQuerier registry, Versioning.KspVersionCriteria versionCriteria)
        {
            if ( replacedBy != null)
            {
                try
                {
                    if (replacedBy.min_version != null)
                    {
                        CkanModule replacement = registry.LatestAvailable(replacedBy.name, versionCriteria);
                        if (replacement != null)
                        {
                            if (!replacement.version.IsLessThan(replacedBy.min_version))
                            {
                                // Replaceable
                                return string.Format("{0} {1}", replacement.name, replacement.version);
                            }
                        }
                    }
                    else if (replacedBy.version != null)
                    {
                        CkanModule replacement = registry.GetModuleByVersion(replacedBy.name, replacedBy.version);
                        if (replacement != null)
                        {
                            if (replacement.IsCompatibleKSP(versionCriteria))
                            {
                                // Replaceable
                                return string.Format("{0} {1}", replacement.name, replacement.version);
                            }
                        }
                    }
                    else
                    {
                        CkanModule replacement = registry.LatestAvailable(replacedBy.name, versionCriteria);
                        if (replacement != null)
                        {
                            // Replaceable
                            return string.Format("{0} {1}", replacement.name, replacement.version);
                        }
                    }
                }
                catch (ModuleNotFoundKraken)
                {
                    log.InfoFormat("Specified replacement mod {0} is not in the registry", replacedBy.name);
                    return null;
                }                        
            } 
            return null;
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
