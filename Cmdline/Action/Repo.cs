using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using CommandLine;
using CKAN;
using log4net;

namespace CKAN.CmdLine
{
    public struct RepositoryList
    {
        public Repository[] repositories;
    }

    public class Repo : ISubCommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Repo));

        public KSPManager Manager { get; set; }
        public IUser User { get; set; }
        public string option;
        public object suboptions;


        public Repo(KSPManager manager, IUser user)
        {
            Manager = manager;
            User = user;
        }

        internal class RepoSubOptions : CommonOptions
        {
            [VerbOption("available", HelpText="List (canonical) available repositories")]
            public CommonOptions AvailableOptions { get; set; }

            [VerbOption("list", HelpText="List repositories")]
            public CommonOptions ListOptions { get; set; }

            [VerbOption("add", HelpText="Add a repository")]
            public AddOptions AddOptions { get; set; }

            [VerbOption("forget", HelpText="Forget a repository")]
            public ForgetOptions ForgetOptions { get; set; }

            [VerbOption("default", HelpText="Set the default repository")]
            public DefaultOptions DefaultOptions { get; set; }
        }

        internal class AvailableOptions : CommonOptions
        {
        }

        internal class ListOptions : CommonOptions
        {
        }

        internal class AddOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }

            [ValueOption(1)]
            public string uri { get; set; }
        }

        internal class DefaultOptions : CommonOptions
        {
            [ValueOption(0)]
            public string uri { get; set; }
        }

        internal class ForgetOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }
        }

        internal void Parse(string option, object suboptions)
        {
            this.option = option;
            this.suboptions = suboptions;
        }

        // This is required by ISubCommand
        public int RunSubCommand(SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();

            if (args.Length == 0)
            {
                // There's got to be a better way of showing help...
                args = new string[1];
                args[0] = "help";
            }

            #region Aliases

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "remove":
                        args[i] = "forget";
                        break;
                }
            }

            #endregion
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new RepoSubOptions (), Parse);

            // That line above will have set our 'option' and 'suboption' fields.

            switch (option)
            {
                case "available":
                    return AvailableRepositories();

                case "list":
                    return ListRepositories();

                case "add":
                    return AddRepository((AddOptions)suboptions);

                case "forget":
                    return ForgetRepository((ForgetOptions)suboptions);

                case "default":
                    return DefaultRepository((DefaultOptions)suboptions);

                default:
                    User.RaiseMessage("Unknown command: repo {0}", option);
                    return Exit.BADOPT;
            }
        }

        public static RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
        {
            WebClient client = new WebClient();

            if (master_uri == null)
            {
                master_uri = Repository.default_repo_master_list;
            }

            string json = client.DownloadString(master_uri);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }

        private int AvailableRepositories()
        {
            User.RaiseMessage("Listing all (canonical) available CKAN repositories:");
            RepositoryList repositories;

            try
            {
                repositories = FetchMasterRepositoryList();
            }
            catch
            {
                User.RaiseError("Couldn't fetch CKAN repositories master list from {0}", Repository.default_repo_master_list.ToString());
                return Exit.ERROR;
            }

            int maxNameLen = 0;
            foreach (Repository repository in repositories.repositories)
            {
                maxNameLen = Math.Max(maxNameLen, repository.name.Length);
            }

            foreach (Repository repository in repositories.repositories)
            {
                User.RaiseMessage("  {0}: {1}", repository.name.PadRight(maxNameLen), repository.uri);
            }

            return Exit.OK;
        }

        private int ListRepositories()
        {
            User.RaiseMessage("Listing all known repositories:");
            var manager = RegistryManager.Instance(Manager.CurrentInstance);
            SortedDictionary<string, Repository> repositories = manager.registry.Repositories;

            int maxNameLen = 0;
            foreach(Repository repository in repositories.Values)
            {
                maxNameLen = Math.Max(maxNameLen, repository.name.Length);
            }

            foreach(Repository repository in repositories.Values)
            {
                User.RaiseMessage("  {0}: {1}: {2}", repository.name.PadRight(maxNameLen), repository.priority, repository.uri);
            }

            return Exit.OK;
        }

        private int AddRepository(AddOptions options)
        {
            RegistryManager manager = RegistryManager.Instance(Manager.CurrentInstance);

            if (options.name == null)
            {
                User.RaiseMessage("add <name> [ <uri> ] - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (options.uri == null)
            {
                RepositoryList repositoryList;

                try
                {
                    repositoryList = FetchMasterRepositoryList();
                }
                catch
                {
                    User.RaiseError("Couldn't fetch CKAN repositories master list from {0}", Repository.default_repo_master_list.ToString());
                    return Exit.ERROR;
                }

                foreach (Repository candidate in repositoryList.repositories)
                {
                    if (String.Equals(candidate.name, options.name, StringComparison.OrdinalIgnoreCase))
                    {
                        options.name = candidate.name;
                        options.uri = candidate.uri.ToString();
                    }
                }

                // Nothing found in the master list?
                if (options.uri == null)
                {
                    User.RaiseMessage("Name {0} not found in master list, please provide name and uri.", options.name);
                    return Exit.BADOPT;
                }
            }

            log.DebugFormat("About to add repository '{0}' - '{1}'", options.name, options.uri);
            SortedDictionary<string, Repository> repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(options.name))
            {
                User.RaiseMessage("Repository with name \"{0}\" already exists, aborting..", options.name);
                return Exit.BADOPT;
            }

            repositories.Add(options.name, new Repository(options.name, options.uri));

            User.RaiseMessage("Added repository '{0}' - '{1}'", options.name, options.uri);
            manager.Save();

            return Exit.OK;
        }

        private int ForgetRepository(ForgetOptions options)
        {
            if (options.name == null)
            {
                User.RaiseError("forget <name> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            RegistryManager manager = RegistryManager.Instance(Manager.CurrentInstance);
            var registry = manager.registry;
            log.DebugFormat("About to forget repository '{0}'", options.name);

            var repos = registry.Repositories;

            string name = options.name;
            if (!repos.ContainsKey(options.name))
            {
                name = repos.Keys.FirstOrDefault(repo => repo.Equals(options.name, StringComparison.OrdinalIgnoreCase));
                if (name == null)
                {
                    User.RaiseMessage("Couldn't find repository with name \"{0}\", aborting..", options.name);
                    return Exit.BADOPT;
                }
                User.RaiseMessage("Removing insensitive match \"{0}\"", name);
            }

            registry.Repositories.Remove(name);
            User.RaiseMessage("Successfully removed \"{0}\"", options.name);
            manager.Save();

            return Exit.OK;
        }

        private int DefaultRepository(DefaultOptions options)
        {
            RegistryManager manager = RegistryManager.Instance(Manager.CurrentInstance);

            if (options.uri == null)
            {
                User.RaiseMessage("default <uri> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            log.DebugFormat("About to add repository '{0}' - '{1}'", Repository.default_ckan_repo_name, options.uri);
            SortedDictionary<string, Repository> repositories = manager.registry.Repositories;

            if (repositories.ContainsKey (Repository.default_ckan_repo_name))
            {
                repositories.Remove (Repository.default_ckan_repo_name);
            }

            repositories.Add(Repository.default_ckan_repo_name, new Repository(Repository.default_ckan_repo_name, Repository.default_ckan_repo_uri));

            User.RaiseMessage("Set {0} repository to '{1}'", Repository.default_ckan_repo_name, options.uri);
            manager.Save();

            return Exit.OK;
        }
    }
}

