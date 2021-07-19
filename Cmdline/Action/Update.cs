using System.Collections.Generic;
using System.Linq;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for updating the list of mods.
    /// </summary>
    public class Update : ICommand
    {
        private readonly GameInstanceManager _manager;
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Update"/> class.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Update(GameInstanceManager manager, IUser user)
        {
            _manager = manager;
            _user = user;
        }

        /// <summary>
        /// Run the 'update' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (UpdateOptions)args;

            List<CkanModule> compatiblePrior = null;

            if (opts.ListChanges)
            {
                // Get a list of compatible modules prior to the update
                var registry = RegistryManager.Instance(inst).registry;
                compatiblePrior = registry.CompatibleModules(inst.VersionCriteria()).ToList();
            }

            // If no repository is selected, select all
            if (opts.Repo == null)
            {
                opts.All = true;
            }

            try
            {
                if (opts.All)
                {
                    UpdateRepository(inst);
                }
                else
                {
                    UpdateRepository(inst, opts.Repo);
                }
            }
            catch (ReinstallModuleKraken rmk)
            {
                Upgrade.UpgradeModules(_manager, _user, inst, false, rmk.Modules);
            }
            catch (MissingCertificateKraken kraken)
            {
                // Handling the kraken means we have prettier output
                _user.RaiseMessage(kraken.ToString());
                return Exit.Error;
            }

            if (opts.ListChanges)
            {
                var registry = RegistryManager.Instance(inst).registry;
                PrintChanges(compatiblePrior, registry.CompatibleModules(inst.VersionCriteria()).ToList());
            }

            return Exit.Ok;
        }

        private void PrintChanges(List<CkanModule> modulesPrior, List<CkanModule> modulesPost)
        {
            var prior = new HashSet<CkanModule>(modulesPrior, new NameComparer());
            var post = new HashSet<CkanModule>(modulesPost, new NameComparer());

            var added = new HashSet<CkanModule>(post.Except(prior, new NameComparer()));
            var removed = new HashSet<CkanModule>(prior.Except(post, new NameComparer()));

            // Default compare includes versions
            var unchanged = post.Intersect(prior);
            var updated = post.Except(unchanged).Except(added).Except(removed).ToList();

            // Print the changes
            _user.RaiseMessage("Found {0} new mods, {1} removed mods and {2} updated mods.", added.Count, removed.Count, updated.Count);

            if (added.Count > 0)
            {
                PrintModules("New mods [Name (CKAN identifier)]:", added);
            }

            if (removed.Count > 0)
            {
                PrintModules("Removed mods [Name (CKAN identifier)]:", removed);
            }

            if (updated.Count > 0)
            {
                PrintModules("Updated mods [Name (CKAN identifier)]:", updated);
            }
        }

        private void PrintModules(string message, IEnumerable<CkanModule> modules)
        {
            // Check input
            if (message == null)
            {
                throw new BadCommandKraken("Message cannot be null.");
            }

            if (modules == null)
            {
                throw new BadCommandKraken("List of modules cannot be null.");
            }

            _user.RaiseMessage(message);

            foreach (var module in modules)
            {
                _user.RaiseMessage("{0} ({1})", module.name, module.identifier);
            }

            _user.RaiseMessage("");
        }

        private void UpdateRepository(CKAN.GameInstance inst, string repository = null)
        {
            var registryManager = RegistryManager.Instance(inst);

            _ = repository == null
                ? CKAN.Repo.UpdateAllRepositories(registryManager, inst, _manager.Cache, _user) != RepoUpdateResult.Failed
                : CKAN.Repo.Update(registryManager, inst, _user, repository);

            _user.RaiseMessage("Updated information on {0} compatible mods.", registryManager.registry.CompatibleModules(inst.VersionCriteria()).Count());
        }
    }

    [Verb("update", HelpText = "Update list of available mods")]
    internal class UpdateOptions : InstanceSpecificOptions
    {
        [Option('r', "repo", HelpText = "CKAN repository to use (experimental!)")]
        public string Repo { get; set; }

        [Option("all", HelpText = "Upgrade all available updated modules")]
        public bool All { get; set; }

        [Option("list-changes", HelpText = "List new and removed modules")]
        public bool ListChanges { get; set; }
    }
}
