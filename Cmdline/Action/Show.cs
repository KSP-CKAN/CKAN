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
                    user.RaiseError(h);
                }
                return Exit.BADOPT;
            }

            int combined_exit_code = Exit.OK;
            // Check installed modules for an exact match.
            var registry = RegistryManager.Instance(instance, repoData).registry;
            foreach (string modName in options.modules)
            {
                var installedModuleToShow = registry.InstalledModule(modName);
                if (installedModuleToShow != null)
                {
                    // Show the installed module.
                    combined_exit_code = CombineExitCodes(
                        combined_exit_code,
                        ShowMod(installedModuleToShow, options)
                    );
                    if (options.with_versions)
                    {
                        ShowVersionTable(instance, registry.AvailableByIdentifier(installedModuleToShow.identifier).ToList());
                    }
                    user.RaiseMessage("");
                    continue;
                }

                // Module was not installed, look for an exact match in the available modules,
                // either by "name" (the user-friendly display name) or by identifier
                CkanModule moduleToShow = registry
                                          .CompatibleModules(instance.VersionCriteria())
                                          .SingleOrDefault(
                                                mod => mod.name       == modName
                                                    || mod.identifier == modName
                                          );
                if (moduleToShow == null)
                {
                    // No exact match found. Try to look for a close match for this KSP version.
                    user.RaiseMessage(Properties.Resources.ShowNotInstalledOrCompatible,
                        modName,
                        instance.game.ShortName,
                        string.Join(", ", instance.VersionCriteria().Versions.Select(v => v.ToString())));
                    user.RaiseMessage(Properties.Resources.ShowLookingForClose);

                    Search search = new Search(repoData, user);
                    var matches = search.PerformSearch(instance, modName);

                    // Display the results of the search.
                    if (!matches.Any())
                    {
                        // No matches found.
                        user.RaiseMessage(Properties.Resources.ShowNoClose);
                        combined_exit_code = CombineExitCodes(combined_exit_code, Exit.BADOPT);
                        user.RaiseMessage("");
                        continue;
                    }
                    else if (matches.Count() == 1)
                    {
                        // If there is only 1 match, display it.
                        user.RaiseMessage(Properties.Resources.ShowFoundOne, matches[0].name);
                        user.RaiseMessage("");

                        moduleToShow = matches[0];
                    }
                    else
                    {
                        // Display the found close matches.
                        int selection = user.RaiseSelectionDialog(
                            Properties.Resources.ShowClosePrompt,
                            matches.Select(m => m.name).ToArray());
                        user.RaiseMessage("");
                        if (selection < 0)
                        {
                            combined_exit_code = CombineExitCodes(combined_exit_code, Exit.BADOPT);
                            continue;
                        }

                        // Mark the selection as the one to show.
                        moduleToShow = matches[selection];
                    }
                }

                combined_exit_code = CombineExitCodes(
                    combined_exit_code,
                    ShowMod(moduleToShow, options)
                );
                if (options.with_versions)
                {
                    ShowVersionTable(instance, registry.AvailableByIdentifier(moduleToShow.identifier).ToList());
                }
                user.RaiseMessage("");
            }
            return combined_exit_code;
        }

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
                if (module.Files is ICollection<string> files)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowFilesHeader, files.Count);
                    foreach (string file in files)
                    {
                        user.RaiseMessage("  - {0}", file);
                    }
                }
                else
                {
                    throw new InvalidCastException();
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

                if (!string.IsNullOrEmpty(module.description))
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
                    // Did you know that authors are optional in the spec?
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
                    user.RaiseMessage(Properties.Resources.ShowLanguages, string.Join(", ", module.localizations.OrderBy(l => l)));
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
                    foreach (RelationshipDescriptor dep in module.depends)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(dep));
                    }
                }

                if (module.recommends != null && module.recommends.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowRecommendsHeader);
                    foreach (RelationshipDescriptor dep in module.recommends)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(dep));
                    }
                }

                if (module.suggests != null && module.suggests.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowSuggestsHeader);
                    foreach (RelationshipDescriptor dep in module.suggests)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(dep));
                    }
                }

                if (module.provides != null && module.provides.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.ShowProvidesHeader);
                    foreach (string prov in module.provides)
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
                if (module.resources.homepage != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowHomePage,
                                      Net.NormalizeUri(module.resources.homepage.ToString()));
                }
                if (module.resources.manual != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowManual,
                                      Net.NormalizeUri(module.resources.manual.ToString()));
                }
                if (module.resources.spacedock != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowSpaceDock,
                                      Net.NormalizeUri(module.resources.spacedock.ToString()));
                }
                if (module.resources.repository != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowRepository,
                                      Net.NormalizeUri(module.resources.repository.ToString()));
                }
                if (module.resources.bugtracker != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowBugTracker,
                                      Net.NormalizeUri(module.resources.bugtracker.ToString()));
                }
                if (module.resources.discussions != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowDiscussions,
                                      Net.NormalizeUri(module.resources.discussions.ToString()));
                }
                if (module.resources.curse != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowCurse,
                                      Net.NormalizeUri(module.resources.curse.ToString()));
                }
                if (module.resources.store != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowStore,
                                      Net.NormalizeUri(module.resources.store.ToString()));
                }
                if (module.resources.steamstore != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowSteamStore,
                                      Net.NormalizeUri(module.resources.steamstore.ToString()));
                }
                if (module.resources.remoteAvc != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowVersionFile,
                                      Net.NormalizeUri(module.resources.remoteAvc.ToString()));
                }
                if (module.resources.remoteSWInfo != null)
                {
                    user.RaiseMessage(Properties.Resources.ShowSpaceWarpInfo,
                                      Net.NormalizeUri(module.resources.remoteSWInfo.ToString()));
                }
            }

            if (!opts.without_files && !module.IsDLC)
            {
                // Compute the CKAN filename.
                string file_uri_hash = NetFileCache.CreateURLHash(module.download[0]);
                string file_name = CkanModule.StandardName(module.identifier, module.version);

                user.RaiseMessage("");
                user.RaiseMessage(Properties.Resources.ShowFileName, file_uri_hash + "-" + file_name);
            }

            return Exit.OK;
        }

        private static int CombineExitCodes(int a, int b)
        {
            // Failures should dominate, keep whichever one isn't OK
            return a == Exit.OK ? b : a;
        }

        private void ShowVersionTable(CKAN.GameInstance inst, List<CkanModule> modules)
        {
            var versions     = modules.Select(m => m.version.ToString()).ToList();
            var gameVersions = modules.Select(m =>
            {
                CkanModule.GetMinMaxVersions(new List<CkanModule>() { m }, out _, out _, out GameVersion minKsp, out GameVersion maxKsp);
                return GameVersionRange.VersionSpan(inst.game, minKsp, maxKsp);
            }).ToList();
            string[] headers = new string[] {
                Properties.Resources.ShowVersionHeader,
                Properties.Resources.ShowGameVersionsHeader
            };
            int versionLength     = Math.Max(headers[0].Length, versions.Max(v => v.Length));
            int gameVersionLength = Math.Max(headers[1].Length, gameVersions.Max(v => v.Length));
            user.RaiseMessage("");
            user.RaiseMessage(
                "{0}  {1}",
                headers[0].PadRight(versionLength),
                headers[1].PadRight(gameVersionLength)
            );
            user.RaiseMessage(
                "{0}  {1}",
                new string('-', versionLength),
                new string('-', gameVersionLength)
            );
            for (int row = 0; row < versions.Count; ++row)
            {
                user.RaiseMessage(
                    "{0}  {1}",
                    versions[row].PadRight(versionLength),
                    gameVersions[row].PadRight(gameVersionLength)
                );
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
        public List<string> modules { get; set; }
    }

}
