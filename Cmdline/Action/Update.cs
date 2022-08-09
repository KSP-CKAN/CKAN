using System.Collections.Generic;
using System.Linq;

namespace CKAN.CmdLine
{
    public class Update : ICommand
    {
        public IUser user { get; set; }
        private GameInstanceManager manager;

        /// <summary>
        /// Initialize the update command object
        /// </summary>
        /// <param name="mgr">GameInstanceManager containing our instances</param>
        /// <param name="user">IUser object for interaction</param>
        public Update(GameInstanceManager mgr, IUser user)
        {
            manager   = mgr;
            this.user = user;
        }

        /// <summary>
        /// Update the registry
        /// </summary>
        /// <param name="instance">Game instance to update</param>
        /// <param name="raw_options">Command line options object</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunCommand(CKAN.GameInstance instance, object raw_options)
        {
            UpdateOptions options = (UpdateOptions) raw_options;

            List<CkanModule> compatible_prior = null;

            if (options.list_changes)
            {
                // Get a list of compatible modules prior to the update.
                var registry = RegistryManager.Instance(instance).registry;
                compatible_prior = registry.CompatibleModules(instance.VersionCriteria()).ToList();
            }

            // If no repository is selected, select all.
            if (options.repo == null)
            {
                options.update_all = true;
            }

            try
            {
                if (options.update_all)
                {
                    UpdateRepository(instance);
                }
                else
                {
                    UpdateRepository(instance, options.repo);
                }
            }
            catch (ReinstallModuleKraken rmk)
            {
                Upgrade.UpgradeModules(manager, user, instance, false, rmk.Modules);
            }
            catch (MissingCertificateKraken kraken)
            {
                // Handling the kraken means we have prettier output.
                user.RaiseMessage(kraken.ToString());
                return Exit.ERROR;
            }

            if (options.list_changes)
            {
                var registry = RegistryManager.Instance(instance).registry;
                PrintChanges(compatible_prior, registry.CompatibleModules(instance.VersionCriteria()).ToList());
            }

            return Exit.OK;
        }

        /// <summary>
        /// Locates the changes between the prior and post state of the modules..
        /// </summary>
        /// <param name="modules_prior">List of the compatible modules prior to the update.</param>
        /// <param name="modules_post">List of the compatible modules after the update.</param>
        private void PrintChanges(List<CkanModule> modules_prior, List<CkanModule> modules_post)
        {
            var prior = new HashSet<CkanModule>(modules_prior, new NameComparer());
            var post = new HashSet<CkanModule>(modules_post, new NameComparer());


            var added = new HashSet<CkanModule>(post.Except(prior, new NameComparer()));
            var removed = new HashSet<CkanModule>(prior.Except(post, new NameComparer()));


            // Default compare includes versions
            var unchanged = post.Intersect(prior);
            var updated = post.Except(unchanged).Except(added).Except(removed).ToList();

            // Print the changes.
            user.RaiseMessage(Properties.Resources.UpdateChangesSummary, added.Count(), removed.Count(), updated.Count());

            if (added.Count > 0)
            {
                PrintModules(Properties.Resources.UpdateAddedHeader, added);
            }

            if (removed.Count > 0)
            {
                PrintModules(Properties.Resources.UpdateRemovedHeader, removed);
            }

            if (updated.Count > 0)
            {
                PrintModules(Properties.Resources.UpdateUpdatedHeader, updated);
            }
        }

        /// <summary>
        /// Prints a message and a list of modules. Ends with a blank line.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="modules">The modules to list.</param>
        private void PrintModules(string message, IEnumerable<CkanModule> modules)
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

            foreach (CkanModule module in modules)
            {
                user.RaiseMessage("{0} ({1})", module.name, module.identifier);
            }

            user.RaiseMessage("");
        }

        /// <summary>
        /// Updates the repository.
        /// </summary>
        /// <param name="instance">The KSP instance to work on.</param>
        /// <param name="repository">Repository to update. If null all repositories are used.</param>
        private void UpdateRepository(CKAN.GameInstance instance, string repository = null)
        {
            RegistryManager registry_manager = RegistryManager.Instance(instance);

            var updated = repository == null
                ? CKAN.Repo.UpdateAllRepositories(registry_manager, instance, manager.Cache, user) != CKAN.RepoUpdateResult.Failed
                : CKAN.Repo.Update(registry_manager, instance, user, repository);

            user.RaiseMessage(Properties.Resources.UpdateSummary, registry_manager.registry.CompatibleModules(instance.VersionCriteria()).Count());
        }
    }
}
