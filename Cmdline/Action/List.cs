using System;
using System.Collections.Generic;
using CKAN.Exporters;
using CKAN.Types;
using CKAN.Versioning;
using CommandLine;
using log4net;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for listing the installed mods.
    /// </summary>
    public class List : ICommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(List));

        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.List"/> class.
        /// </summary>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public List(IUser user)
        {
            _user = user;
        }

        /// <summary>
        /// Run the 'list' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (ListOptions)args;
            IRegistryQuerier registry = RegistryManager.Instance(inst).registry;
            ExportFileType? exportFileType = null;

            if (!string.IsNullOrWhiteSpace(opts.Export))
            {
                exportFileType = GetExportFileType(opts.Export);
                if (exportFileType == null)
                {
                    _user.RaiseError("Unknown export format: {0}", opts.Export);
                }
            }

            if (!opts.Porcelain && exportFileType == null)
            {
                _user.RaiseMessage("\r\nFound {0} at \"{1}\"", inst.game.ShortName, inst.GameDir());
                _user.RaiseMessage("\r\n{0} version: \"{1}\"", inst.game.ShortName, inst.Version());
                _user.RaiseMessage("\r\nInstalled Modules:\r\n");
            }

            if (exportFileType == null)
            {
                var mods = new SortedDictionary<string, ModuleVersion>(registry.Installed());
                foreach (var mod in mods)
                {
                    var currentVersion = mod.Value;
                    var modInfo = string.Format("{0} {1}", mod.Key, mod.Value);
                    var bullet = "*";

                    if (currentVersion is ProvidesModuleVersion)
                    {
                        // Skip virtuals for now
                        continue;
                    }

                    if (currentVersion is UnmanagedModuleVersion)
                    {
                        // Autodetected dll
                        bullet = "A";
                    }
                    else
                    {
                        try
                        {
                            // Check if upgrades are available, and show appropriately
                            Log.DebugFormat("Checking if upgrades are available for \"{0}\"...", mod.Key);
                            var latest = registry.LatestAvailable(mod.Key, inst.VersionCriteria());
                            var current = registry.GetInstalledVersion(mod.Key);
                            var installed = registry.InstalledModule(mod.Key);

                            if (latest == null)
                            {
                                // Not compatible!
                                Log.InfoFormat("Latest \"{0}\" is not compatible.", mod.Key);
                                bullet = "X";
                                if (current == null)
                                {
                                    Log.DebugFormat("No installed version of \"{0}\" found in the registry.", mod.Key);
                                }

                                // Check if mod is replaceable
                                if (current.replaced_by != null)
                                {
                                    var replacement = registry.GetReplacement(mod.Key, inst.VersionCriteria());
                                    if (replacement != null)
                                    {
                                        // Replaceable!
                                        bullet = ">";
                                        modInfo = string.Format("{0} > {1} {2}", modInfo, replacement.ReplaceWith.name, replacement.ReplaceWith.version);
                                    }
                                }
                            }
                            else if (latest.version.IsEqualTo(currentVersion))
                            {
                                // Up to date
                                Log.InfoFormat("Latest \"{0}\" is {1}", mod.Key, latest.version);
                                bullet = installed?.AutoInstalled ?? false ? "+" : "-";

                                // Check if mod is replaceable
                                if (current.replaced_by != null)
                                {
                                    var replacement = registry.GetReplacement(latest.identifier, inst.VersionCriteria());
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
                            Log.InfoFormat("\"{0}\" is installed, but no longer in the registry.", mod.Key);
                            bullet = "?";
                        }
                    }

                    _user.RaiseMessage("{0} {1}", bullet, modInfo);
                }
            }
            else
            {
                var stream = Console.OpenStandardOutput();
                new Exporter(exportFileType.Value).Export(registry, stream);
                stream.Flush();
            }

            if (!opts.Porcelain && exportFileType == null)
            {
                // Broken mods are in a state that CKAN doesn't understand, and therefore can't handle automatically
                _user.RaiseMessage("\r\nLegend: -: Up to date. +: Auto-installed. X: Incompatible. ^: Upgradable. >: Replaceable\r\n        A: Autodetected. ?: Unknown. *: Broken. ");
            }

            return Exit.Ok;
        }

        private static ExportFileType? GetExportFileType(string export)
        {
            export = export.ToLowerInvariant();

            switch (export)
            {
                case "text":
                    return ExportFileType.PlainText;
                case "markdown":
                    return ExportFileType.Markdown;
                case "bbcode":
                    return ExportFileType.BbCode;
                case "csv":
                    return ExportFileType.Csv;
                case "tsv":
                    return ExportFileType.Tsv;
                default:
                    return null;
            }
        }
    }

    [Verb("list", HelpText = "List installed mods")]
    internal class ListOptions : InstanceSpecificOptions
    {
        [Option("porcelain", HelpText = "Dump a raw list of mods, good for shell scripting")]
        public bool Porcelain { get; set; }

        [Option("export", HelpText = "Export a list of mods in specified format to stdout")]
        public string Export { get; set; }
    }
}
