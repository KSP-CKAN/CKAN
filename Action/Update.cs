using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace CKAN.CmdLine
{
    public class Update : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Update));

        public IUser user { get; set; }

        public Update(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
        {
            UpdateOptions options = (UpdateOptions) raw_options;

            List<CkanModule> available_prior = null;

            user.RaiseMessage("Downloading updates...");

            if (options.list_changes)
            {
                // Get a list of available modules prior to the update.
                available_prior = ksp.Registry.Available(ksp.Version());
            }

            try
            {
                if (options.update_all)
                {
                    UpdateRepository(ksp);
                }
                else
                {
                    UpdateRepository(ksp, options.repo);
                }
            }
            catch (MissingCertificateKraken kraken)
            {
                // Handling the kraken means we have prettier output.
                user.RaiseMessage(kraken.ToString());
                return Exit.ERROR;
            }

            if (options.list_changes)
            {
                PrintChanges(available_prior, ksp.Registry.Available(ksp.Version()));
            }

            return Exit.OK;
        }

        /// <summary>
        /// Locates the changes between the prior and post state of the modules..
        /// </summary>
        /// <param name="modules_prior">List of the available modules prior to the update.</param>
        /// <param name="modules_post">List of the available modules after the update.</param>
        private void PrintChanges(List<CkanModule> modules_prior, List<CkanModule> modules_post)
        {
            // Check for new modules.
            List<CkanModule> added = modules_post.Where(a => !modules_prior.Select(b => b.name).Contains(a.name)).ToList();

            // Check for removed modules.
            List<CkanModule> removed = modules_prior.Where(a => !modules_post.Select(b => b.name).Contains(a.name)).ToList();

            // Check for updated modules.
            // TODO: There is most likely a better way of doing this in a single statement using LINQ.
            List<CkanModule> updated = new List<CkanModule>();

            // First, get the identifiers and version of all the mods.
            Dictionary<string, Version> identifiers_prior = (from a in modules_prior select new {a.identifier, a.version}).ToDictionary(x => x.identifier, x => x.version);
            Dictionary<string, Version> identifiers_post = (from b in modules_post select new {b.identifier, b.version}).ToDictionary(x => x.identifier, x => x.version);

            // Compare the two lists.
            foreach (var a in identifiers_prior)
            {
                // Check that the mod is still there after the update.
                if (identifiers_post.ContainsKey(a.Key))
                {
                    // Check that the version has increased.
                    if (identifiers_post[a.Key].IsGreaterThan(a.Value))
                    {
                        // Extract the mod information from the updated modules list.
                        updated.Add(modules_post.Where(b => b.identifier == a.Key).First());
                    }
                }
            }

            // Print the changes.
            user.RaiseMessage("Found {0} new modules, {1} removed modules and {2} updated modules.", added.Count, removed.Count, updated.Count);

            if (added.Count > 0)
            {
                PrintModules("New modules [Name (CKAN identifier)]:", added);
            }

            if (removed.Count > 0)
            {
                PrintModules("Removed modules [Name (CKAN identifier)]:", removed);
            }

            if (updated.Count > 0)
            {
                PrintModules("Updated modules [Name (CKAN identifier)]:", updated);
            }
        }

        /// <summary>
        /// Prints a message and a list of modules. Ends with a blank line.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="modules">The modules to list.</param>
        private void PrintModules(string message, List<CkanModule> modules)
        {
            // Check input.
            if (message == null)
            {
                throw new BadCommandKraken("Message cannot be null.");
            }

            if (modules == null)
            {
                throw new BadCommandKraken("List of modules cannot be null.");
            }

            user.RaiseMessage(message);

            foreach(CkanModule module in modules)
            {
                user.RaiseMessage("{0} ({1})", module.name, module.identifier);
            }

            user.RaiseMessage("");
        }

        /// <summary>
        /// Updates the repository.
        /// </summary>
        /// <param name="ksp">The KSP instance to work on.</param>
        /// <param name="repository">Repository to update. If null all repositories are used.</param>
        private void UpdateRepository(CKAN.KSP ksp, string repository = null)
        {
            RegistryManager registry_manager = RegistryManager.Instance(ksp);

            // Update the repository/repositories.
            int updated = CKAN.Repo.Update(registry_manager, ksp, user, true, repository);

            user.RaiseMessage("Updated information on {0} available modules", updated);
        }
    }
}
