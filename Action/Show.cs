using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class Show : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Show));

        public IUser user { get; set; }

        public Show(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            ShowOptions options = (ShowOptions) raw_options;

            if (options.Modname == null)
            {
                // empty argument
                user.RaiseMessage("show <module> - module name argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            // Find the module: either by "name" (the user-friendly display name) or by identifier
            CkanModule moduleToShow = ksp.Registry                  
                                      .Available(ksp.Version())
                                      .SingleOrDefault(
                                            mod => mod.name == options.Modname
                                                || mod.identifier == options.Modname
                                      );
                
            if (moduleToShow == null)
            {
                // No exact match found. Try to look for a close match.
                user.RaiseMessage("{0} not found.", options.Modname);
                user.RaiseMessage("Looking for close matches.");

                Search search = new Search(user);
                List<CkanModule> matches = search.PerformSearch(ksp, options.Modname);

                // Display the results of the search.
                if (matches.Count == 0)
                {
                    // No matches found.
                    user.RaiseMessage("No close matches found.");
                    return Exit.BADOPT;
                }
                else if (matches.Count == 1)
                {
                    // If there is only 1 match, display it.
                    user.RaiseMessage("Found 1 close match: {0}", matches[0].name);
                    user.RaiseMessage("");

                    moduleToShow = matches[0];
                }
                else
                {
                    // Display the found close matches.
                    string[] strings_matches = new string[matches.Count];

                    for (int i = 0; i < matches.Count; i++)
                    {
                        strings_matches[i] = matches[i].name;
                    }

                    int selection = user.RaiseSelectionDialog("Close matches", strings_matches);

                    if (selection < 0)
                    {
                        return Exit.BADOPT;
                    }

                    // Mark the selection as the one to show.
                    moduleToShow = matches[selection];
                }
            }

            // If the selected module is installed, we have additional data to show. First, check if the module is installed.
            InstalledModule installedModuleToShow = ksp.Registry.InstalledModule(moduleToShow.identifier);

            // If the module is installed (not null), show it. Else, show the generic information.
            if (installedModuleToShow != null)
            {
                return ShowMod(installedModuleToShow);
            }

            return ShowMod(moduleToShow);
        }

        /// <summary>
        /// Shows information about the mod.
        /// </summary>
        /// <returns>Success status.</returns>
        /// <param name="module">The module to show.</param>
        public int ShowMod(InstalledModule module)
        {
            // Display the basic info.
            int return_value = ShowMod(module.Module);

            // Display InstalledModule specific information.
            ICollection<string> files = module.Files as ICollection<string>;
            if (files == null) throw new InvalidCastException();

            user.RaiseMessage("\nShowing {0} installed files:", files.Count);
            foreach (string file in files)
            {
                user.RaiseMessage("- {0}", file);
            }

            return return_value;
        }

        /// <summary>
        /// Shows information about the mod.
        /// </summary>
        /// <returns>Success status.</returns>
        /// <param name="module">The module to show.</param>
        public int ShowMod(Module module)
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
                user.RaiseMessage("\n{0}\n", module.description);
            }
            #endregion

            #region General info (author, version...)
            user.RaiseMessage("\nModule info:");
            user.RaiseMessage("- version:\t{0}", module.version);

            if (module.author != null)
            {
                user.RaiseMessage("- authors:\t{0}", string.Join(", ", module.author));
            }
            else
            {
                // Did you know that authors are optional in the spec?
                // You do now. #673.
                user.RaiseMessage("- authors:\tUNKNOWN");
            }

            user.RaiseMessage("- status:\t{0}", module.release_status);
            user.RaiseMessage("- license:\t{0}", module.license); 
            #endregion

            #region Relationships
            if (module.depends != null && module.depends.Count > 0)
            {
                user.RaiseMessage("\nDepends:");
                foreach (RelationshipDescriptor dep in module.depends)
                    user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
            }

            if (module.recommends != null && module.recommends.Count > 0)
            {
                user.RaiseMessage("\nRecommends:");
                foreach (RelationshipDescriptor dep in module.recommends)
                    user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
            }

            if (module.suggests != null && module.suggests.Count > 0)
            {
                user.RaiseMessage("\nSuggests:");
                foreach (RelationshipDescriptor dep in module.suggests)
                    user.RaiseMessage("- {0}", RelationshipToPrintableString(dep));
            }

            if (module.ProvidesList != null && module.ProvidesList.Count > 0)
            {
                user.RaiseMessage("\nProvides:");
                foreach (string prov in module.ProvidesList)
                    user.RaiseMessage("- {0}", prov);
            } 
            #endregion

            user.RaiseMessage("\nResources:");
            if (module.resources != null)
            {
                if (module.resources.bugtracker != null)
                    user.RaiseMessage("- bugtracker: {0}", Uri.EscapeUriString(module.resources.bugtracker.ToString()));
                if (module.resources.homepage != null)
                    user.RaiseMessage("- homepage: {0}", Uri.EscapeUriString(module.resources.homepage.ToString()));
                if (module.resources.kerbalstuff != null)
                    user.RaiseMessage("- kerbalstuff: {0}", Uri.EscapeUriString(module.resources.kerbalstuff.ToString()));
                if (module.resources.repository != null)
                    user.RaiseMessage("- repository: {0}", Uri.EscapeUriString(module.resources.repository.ToString()));
            }

            return Exit.OK;
        }

        /// <summary>
        /// Formats a RelationshipDescriptor into a user-readable string:
        /// Name, version: x, min: x, max: x
        /// </summary>
        private static string RelationshipToPrintableString(RelationshipDescriptor dep)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(dep.name);
            if (!string.IsNullOrEmpty(dep.version)) sb.Append(", version: " + dep.version);
            if (!string.IsNullOrEmpty(dep.min_version)) sb.Append(", min: " + dep.version);
            if (!string.IsNullOrEmpty(dep.max_version)) sb.Append(", max: " + dep.version);
            return sb.ToString();
        }
    }
}

