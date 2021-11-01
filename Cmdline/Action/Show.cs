using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CKAN.Versioning;

namespace CKAN.CmdLine
{
    public class Show : ICommand
    {
        public Show(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.GameInstance ksp, object raw_options)
        {
            ShowOptions options = (ShowOptions) raw_options;
            if (options.modules == null || options.modules.Count < 1)
            {
                // empty argument
                user.RaiseMessage("show <module> - module name argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            int combined_exit_code = Exit.OK;
            // Check installed modules for an exact match.
            var registry = RegistryManager.Instance(ksp).registry;
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
                        ShowVersionTable(ksp, registry.AvailableByIdentifier(installedModuleToShow.identifier).ToList());
                    }
                    user.RaiseMessage("");
                    continue;
                }

                // Module was not installed, look for an exact match in the available modules,
                // either by "name" (the user-friendly display name) or by identifier
                CkanModule moduleToShow = registry
                                          .CompatibleModules(ksp.VersionCriteria())
                                          .SingleOrDefault(
                                                mod => mod.name       == modName
                                                    || mod.identifier == modName
                                          );
                if (moduleToShow == null)
                {
                    // No exact match found. Try to look for a close match for this KSP version.
                    user.RaiseMessage(
                        "{0} not installed or compatible with {1} {2}.",
                        modName,
                        ksp.game.ShortName,
                        string.Join(", ", ksp.VersionCriteria().Versions.Select(v => v.ToString()))
                    );
                    user.RaiseMessage("Looking for close matches in compatible mods...");

                    Search search = new Search(user);
                    var matches = search.PerformSearch(ksp, modName);

                    // Display the results of the search.
                    if (!matches.Any())
                    {
                        // No matches found.
                        user.RaiseMessage("No close matches found.");
                        combined_exit_code = CombineExitCodes(combined_exit_code, Exit.BADOPT);
                        user.RaiseMessage("");
                        continue;
                    }
                    else if (matches.Count() == 1)
                    {
                        // If there is only 1 match, display it.
                        user.RaiseMessage("Found 1 close match: {0}", matches[0].name);
                        user.RaiseMessage("");

                        moduleToShow = matches[0];
                    }
                    else
                    {
                        // Display the found close matches.
                        int selection = user.RaiseSelectionDialog(
                            "Close matches:",
                            matches.Select(m => m.name).ToArray()
                        );
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
                    ShowVersionTable(ksp, registry.AvailableByIdentifier(moduleToShow.identifier).ToList());
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
                ICollection<string> files = module.Files as ICollection<string>;
                if (files == null)
                {
                    throw new InvalidCastException();
                }

                user.RaiseMessage("");
                user.RaiseMessage("Showing {0} installed files:", files.Count);
                foreach (string file in files)
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
                user.RaiseMessage("Module info:");
                user.RaiseMessage("  Version:\t{0}", module.version);
                
                if (module.author != null)
                {
                    user.RaiseMessage("  Authors:\t{0}", string.Join(", ", module.author));
                }
                else
                {
                    // Did you know that authors are optional in the spec?
                    // You do now. #673.
                    user.RaiseMessage("  Authors:\tUNKNOWN");
                }
                
                if (module.release_status != null)
                {
                    user.RaiseMessage("  Status:\t{0}", module.release_status);
                }
                user.RaiseMessage("  License:\t{0}", string.Join(", ", module.license));
                if (module.Tags != null && module.Tags.Count > 0)
                {
                    // Need an extra space before the tab to line up with other fields
                    user.RaiseMessage("  Tags: \t{0}", string.Join(", ", module.Tags));
                }
                if (module.localizations != null && module.localizations.Length > 0)
                {
                    user.RaiseMessage("  Languages:\t{0}", string.Join(", ", module.localizations.OrderBy(l => l)));
                }
                #endregion
            }

            if (!opts.without_relationships)
            {
                #region Relationships
                if (module.depends != null && module.depends.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage("Depends:");
                    foreach (RelationshipDescriptor dep in module.depends)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(dep));
                    }
                }

                if (module.recommends != null && module.recommends.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage("Recommends:");
                    foreach (RelationshipDescriptor dep in module.recommends)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(dep));
                    }
                }

                if (module.suggests != null && module.suggests.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage("Suggests:");
                    foreach (RelationshipDescriptor dep in module.suggests)
                    {
                        user.RaiseMessage("  - {0}", RelationshipToPrintableString(dep));
                    }
                }

                if (module.provides != null && module.provides.Count > 0)
                {
                    user.RaiseMessage("");
                    user.RaiseMessage("Provides:");
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
                user.RaiseMessage("Resources:");
                if (module.resources.homepage != null)
                {
                    user.RaiseMessage("  Home page:\t{0}", Uri.EscapeUriString(module.resources.homepage.ToString()));
                }
                if (module.resources.manual != null)
                {
                    user.RaiseMessage("  Manual:\t{0}", Uri.EscapeUriString(module.resources.manual.ToString()));
                }
                if (module.resources.spacedock != null)
                {
                    user.RaiseMessage("  SpaceDock:\t{0}", Uri.EscapeUriString(module.resources.spacedock.ToString()));
                }
                if (module.resources.repository != null)
                {
                    user.RaiseMessage("  Repository:\t{0}", Uri.EscapeUriString(module.resources.repository.ToString()));
                }
                if (module.resources.bugtracker != null)
                {
                    user.RaiseMessage("  Bug tracker:\t{0}", Uri.EscapeUriString(module.resources.bugtracker.ToString()));
                }
                if (module.resources.curse != null)
                {
                    user.RaiseMessage("  Curse:\t{0}", Uri.EscapeUriString(module.resources.curse.ToString()));
                }
                if (module.resources.store != null)
                {
                    user.RaiseMessage("  Store:\t{0}", Uri.EscapeUriString(module.resources.store.ToString()));
                }
                if (module.resources.steamstore != null)
                {
                    user.RaiseMessage("  Steam store:\t{0}", Uri.EscapeUriString(module.resources.steamstore.ToString()));
                }
                if (module.resources.remoteAvc != null)
                {
                    user.RaiseMessage("  Version file:\t{0}", Uri.EscapeUriString(module.resources.remoteAvc.ToString()));
                }
            }

            if (!opts.without_files && !module.IsDLC)
            {
                // Compute the CKAN filename.
                string file_uri_hash = NetFileCache.CreateURLHash(module.download);
                string file_name = CkanModule.StandardName(module.identifier, module.version);
                
                user.RaiseMessage("");
                user.RaiseMessage("Filename: {0}", file_uri_hash + "-" + file_name);
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
                GameVersion minKsp = null, maxKsp = null;
                Registry.GetMinMaxVersions(new List<CkanModule>() { m }, out _, out _, out minKsp, out maxKsp);
                return GameVersionRange.VersionSpan(inst.game, minKsp, maxKsp);
            }).ToList();
            string[] headers = new string[] { "Version", "Game Versions" };
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
    }
}
