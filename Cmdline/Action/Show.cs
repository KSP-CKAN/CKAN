using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommandLine;

using CKAN.Versioning;

namespace CKAN.CmdLine
{
    public class Show : ICommand
    {
        public Show(RepositoryDataManager repoData, IUser user)
        {
            this.repoData = repoData;
            this.user     = user;
        }

        public int RunCommand(CKAN.GameInstance instance, object raw_options)
        {
            ShowOptions options = (ShowOptions) raw_options;
            if (options.modules == null || options.modules.Count < 1)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                foreach (var h in Actions.GetHelp("show"))
                {
                    user.RaiseError("{0}", h);
                }
                return Exit.BADOPT;
            }

            int combined_exit_code = Exit.OK;
            // Check installed modules for an exact match.
            var registry = RegistryManager.Instance(instance, repoData).registry;
            foreach (string modName in options.modules)
            {
                (InstalledModule?, CkanModule?) toShow = (null, null);
                if (registry.InstalledModule(modName) is InstalledModule instModule)
                {
                    // Show the installed module
                    toShow = (instModule, null);
                }
                else if (registry.CompatibleModules(instance.StabilityToleranceConfig,
                                                    instance.VersionCriteria())
                                 .SingleOrDefault(mod => mod.name       == modName
                                                      || mod.identifier == modName)
                         is CkanModule module)
                {
                    // Fall back to exact match in the available modules, if any,
                    // either by "name" (the user-friendly display name) or by identifier
                    toShow = (null, module);
                }
                else
                {
                    // No exact match found. Try to look for a close match for this KSP version.
                    user.RaiseMessage(Properties.Resources.ShowNotInstalledOrCompatible,
                        modName,
                        instance.Game.ShortName,
                        string.Join(", ", instance.VersionCriteria().Versions.Select(v => v.ToString())));
                    user.RaiseMessage(Properties.Resources.ShowLookingForClose);
                    user.RaiseMessage("");

                    var instMods = registry.InstalledModules
                                           .Where(im => Search.TermMatchesModule(im.Module, modName, ""))
                                           .OrderBy(im => im.Module.name)
                                           .ThenBy(im => im.identifier)
                                           .ToArray();
                    var search     = new Search(repoData, user);
                    var uninstMods = search.PerformSearch(instance, modName)
                                           .Where(m => !instMods.Any(im => im.identifier == m.identifier))
                                           .ToArray();
                    // Collect all matches in (InstalledModule?, CkanModule?) tuples so we can present one prompt for all
                    var both = instMods.Select(im => ((InstalledModule?)im, (CkanModule?)null))
                                       .Concat(uninstMods.Select(m => ((InstalledModule?)null, (CkanModule?)m)))
                                       .ToArray();

                    // Display the results of the search.
                    if (both.Length == 0)
                    {
                        // No matches found.
                        user.RaiseMessage(Properties.Resources.ShowNoClose);
                        combined_exit_code = CombineExitCodes(combined_exit_code, Exit.BADOPT);
                    }
                    else
                    {
                        toShow = Choose(both, tuple => tuple.Item1?.Module.name ?? tuple.Item2?.name ?? "");
                    }
                }
                switch (toShow)
                {
                    case (InstalledModule im, _):
                        combined_exit_code = CombineExitCodes(combined_exit_code,
                                                              ShowMod(im, options));
                        if (options.with_versions)
                        {
                            ShowVersionTable(instance, registry.AvailableByIdentifier(im.identifier)
                                                               .ToArray());
                        }
                        break;

                    case (_, CkanModule m):
                        combined_exit_code = CombineExitCodes(combined_exit_code,
                                                              ShowMod(m, options));
                        if (options.with_versions)
                        {
                            ShowVersionTable(instance, registry.AvailableByIdentifier(m.identifier)
                                                               .ToArray());
                        }
                        break;
                }
                user.RaiseMessage("");
            }
            return combined_exit_code;
        }

        private T? Choose<T>(IReadOnlyList<T> choices,
                             Func<T, string>  getLabel)
            where T : notnull
            => choices switch
               {
                   { Count: 1 } => choices.Single(),
                   _            => user.RaiseSelectionDialog(Properties.Resources.ShowClosePrompt,
                                                             choices.Select(getLabel).ToArray())
                                   is >= 0 and int choice
                                       ? choices[choice]
                                       : default,
               };

        /// <summary>
        /// Shows information about the mod.
        /// </summary>
        /// <returns>Success status.</returns>
        /// <param name="module">The module to show.</param>
        private int ShowMod(InstalledModule module, ShowOptions opts)
        {
            // Display the basic info.
            int return_value = ShowMod(module.Module, opts);

            if (!opts.without_files && !module.Module.IsDLC)
            {
                // Display InstalledModule specific information.
                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.ShowFilesHeader, module.Files.Count);
                foreach (string file in module.Files)
                {
                    user.RaiseMessage("  - {0}", file);
                }
            }

            return return_value;
        }

        /// <summary>
        /// Shows information about the mod.
        /// </summary>
        /// <returns>Success status.</returns>
        /// <param name="module">The module to show.</param>
        private int ShowMod(CkanModule module, ShowOptions opts)
        {
            if (!opts.without_description)
            {
                #region Abstract and description
                if (!string.IsNullOrEmpty(module.@abstract))
                {
                    user.RaiseMessage("{0}: {1}", module.name, module.@abstract);
                }
                else
                {
                    user.RaiseMessage("{0}", module.name);
                }

                if (module.description != null && !string.IsNullOrEmpty(module.description))
                {
                    user.RaiseMessage("");
                    user.RaiseMessage("{0}", module.description);
                }
                #endregion
            }

            if (!opts.without_module_info)
            {
                #region General info (author, version...)
                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.ShowModuleInfoHeader);
                user.RaiseMessage(Properties.Resources.ShowVersion, module.version);

                if (module.author != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowAuthor, string.Join(", ", module.author));
                }
                else
                {
                    // Did you know that authors used to be optional in the spec?
                    // You do now. #673.
                    user.RaiseMessage(Properties.Resources.ShowAuthorUnknown);
                }

                if (module.release_status != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowStatus, module.release_status);
                }
                user.RaiseMessage(Properties.Resources.ShowLicence, string.Join(", ", module.license));
                if (module.Tags != null && module.Tags.Count > 0)
                {
                    // Need an extra space before the tab to line up with other fields
                    user.RaiseMessage(Properties.Resources.ShowTags, string.Join(", ", module.Tags));
                }
                if (module.localizations != null && module.localizations.Length > 0)
                {
                    user.RaiseMessage(Properties.Resources.ShowLanguages, string.Join(", ", module.localizations.Order()));
                }
                #endregion
            }

            if (!opts.without_relationships)
            {
                #region Relationships
                if (module.depends != null && module.depends.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowDependsHeader);
                    foreach (var dep in module.depends)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(dep));
                    }
                }

                if (module.recommends != null && module.recommends.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowRecommendsHeader);
                    foreach (var rec in module.recommends)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(rec));
                    }
                }

                if (module.suggests != null && module.suggests.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowSuggestsHeader);
                    foreach (var sug in module.suggests)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(sug));
                    }
                }

                if (module.supports != null && module.supports.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowSupportsHeader);
                    foreach (var sup in module.supports)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(sup));
                    }
                }

                if (module.provides != null && module.provides.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowProvidesHeader);
                    foreach (var prov in module.provides)
                    {
                        user.RaiseMessage("  - {0}", prov);
                    }
                }
                #endregion
            }

            if (!opts.without_resources && module.resources != null)
            {
                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.ShowResourcesHeader);
                RaiseResource(Properties.Resources.ShowHomePage,
                              module.resources.homepage);
                RaiseResource(Properties.Resources.ShowManual,
                              module.resources.manual);
                RaiseResource(Properties.Resources.ShowSpaceDock,
                              module.resources.spacedock);
                RaiseResource(Properties.Resources.ShowRepository,
                              module.resources.repository);
                RaiseResource(Properties.Resources.ShowBugTracker,
                              module.resources.bugtracker);
                RaiseResource(Properties.Resources.ShowDiscussions,
                              module.resources.discussions);
                RaiseResource(Properties.Resources.ShowCurse,
                              module.resources.curse);
                RaiseResource(Properties.Resources.ShowStore,
                              module.resources.store);
                RaiseResource(Properties.Resources.ShowSteamStore,
                              module.resources.steamstore);
                RaiseResource(Properties.Resources.ShowGogStore,
                              module.resources.gogstore);
                RaiseResource(Properties.Resources.ShowEpicStore,
                              module.resources.epicstore);
                RaiseResource(Properties.Resources.ShowVersionFile,
                              module.resources.remoteAvc);
                RaiseResource(Properties.Resources.ShowSpaceWarpInfo,
                              module.resources.remoteSWInfo);
            }

            if (!opts.without_files && !module.IsDLC
                //&& module.download is [Uri url, ..]
                && module.download != null
                && module.download.Count > 0
                && module.download[0] is Uri url)
            {
                // Compute the CKAN filename.
                string file_uri_hash = NetFileCache.CreateURLHash(url);
                string file_name = CkanModule.StandardName(module.identifier, module.version);

                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.ShowFileName, file_uri_hash + "-" + file_name);
            }

            return Exit.OK;
        }

        private void RaiseResource(string fmt, Uri? url)
        {
            if (url is Uri u && Net.NormalizeUri(u.ToString()) is string s)
            {
                user.RaiseMessage(fmt, s);
            }
        }

        private static int CombineExitCodes(int a, int b)
        {
            // Failures should dominate, keep whichever one isn't OK
            return a == Exit.OK ? b : a;
        }

        private void ShowVersionTable(CKAN.GameInstance inst, IReadOnlyCollection<CkanModule> modules)
        {
            var versions     = modules.Select(m => m.version.ToString()).ToList();
            var gameVersions = modules.Select(m =>
            {
                CkanModule.GetMinMaxVersions(new List<CkanModule>() { m }, out _, out _,
                                             out GameVersion? minKsp, out GameVersion? maxKsp);
                return GameVersionRange.VersionSpan(inst.Game,
                                                    minKsp ?? GameVersion.Any,
                                                    maxKsp ?? GameVersion.Any);
            }).ToList();
            int versionLength     = Math.Max(Properties.Resources.ShowVersionHeader.Length,
                                             versions.Max(v => v.Length));
            int gameVersionLength = Math.Max(Properties.Resources.ShowGameVersionsHeader.Length,
                                             gameVersions.Max(v => v.Length));
            user.RaiseMessage("");
            user.RaiseMessage("{0}  {1}",
                              Properties.Resources.ShowVersionHeader.PadRight(versionLength),
                              Properties.Resources.ShowGameVersionsHeader.PadRight(gameVersionLength));
            user.RaiseMessage("{0}  {1}",
                              new string('-', versionLength),
                              new string('-', gameVersionLength));
            for (int row = 0; row < versions.Count; ++row)
            {
                user.RaiseMessage("{0}  {1}",
                                  versions[row].PadRight(versionLength),
                                  gameVersions[row].PadRight(gameVersionLength));
            }
        }

        /// <summary>
        /// Formats a RelationshipDescriptor into a user-readable string:
        /// Name, version: x, min: x, max: x
        /// </summary>
        private static string RelationshipToPrintableString(RelationshipDescriptor dep)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(dep.ToString());
            return sb.ToString();
        }

        private IUser user { get; set; }
        private readonly RepositoryDataManager repoData;
    }

    internal class ShowOptions : InstanceSpecificOptions
    {
        [Option("without-description", HelpText = "Don't show the name, abstract, or description")]
        public bool without_description { get; set; }

        [Option("without-module-info", HelpText = "Don't show the version, authors, status, license, tags, languages")]
        public bool without_module_info { get; set; }

        [Option("without-relationships", HelpText = "Don't show dependencies or conflicts")]
        public bool without_relationships { get; set; }

        [Option("without-resources", HelpText = "Don't show home page, etc.")]
        public bool without_resources { get; set; }

        [Option("without-files", HelpText = "Don't show contained files")]
        public bool without_files { get; set; }

        [Option("with-versions", HelpText = "Print table of all versions of the mod and their compatible game versions")]
        public bool with_versions { get; set; }

        [ValueList(typeof(List<string>))]
        [AvailableIdentifiers]
        public List<string>? modules { get; set; }
    }

}
