using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class Search : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Search));

        public IUser user { get; set; }

        public Search(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            SearchOptions options = (SearchOptions) raw_options;

            // Check the input.
            if (String.IsNullOrWhiteSpace(options.search_term))
            {
                user.RaiseError("No search term?");

                return Exit.BADOPT;
            }

            // Convert to lowercase for easier comparison.
            options.search_term = options.search_term.ToLower();

            // Get a list of available mods.
            List<CkanModule> available_mods = ksp.Registry.Available(ksp.Version());
            List<CkanModule> matching_mods = new List<CkanModule>();

            // Look for the search term in the list.
            // TODO: Could use some parallism here to speed up the search.
            foreach (CkanModule mod in available_mods)
            {
                // Extract the mod name, identifier. These are guaranteed to exist.
                string mod_name = mod.name.ToLower();
                string mod_identifier = mod.identifier.ToLower();

                // Extract the description. This is an optional field and may be null.
                string mod_description = String.Empty;

                if (!String.IsNullOrEmpty(mod.description))
                {
                    mod_description = mod.description.ToLower();
                }

                // Look for a match in each string.
                if (mod_name.Contains(options.search_term) || mod_identifier.Contains(options.search_term) || mod_description.Contains(options.search_term))
                {
                    matching_mods.Add(mod);
                }
            }

            // Show how many matches we have.
            user.RaiseMessage("Found " + matching_mods.Count.ToString() + " mods matching \"" + options.search_term + "\".");

            // Present the results.
            if (matching_mods.Count == 0)
            {
                return Exit.OK;
            }

            // Print each mod on a separate line.
            foreach (CkanModule mod in matching_mods)
            {
                user.RaiseMessage(mod.identifier);
            }

            return Exit.OK;
        }
    }
}

