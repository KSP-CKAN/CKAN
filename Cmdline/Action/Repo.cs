using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{

    public class Repo : ISubCommand
    {
        public Repo() { }

        internal class RepoSubOptions : VerbCommandOptions
        {
            [VerbOption("available", HelpText = "List (canonical) available repositories")]
            public AvailableOptions AvailableOptions { get; set; }

            [VerbOption("list",      HelpText = "List repositories")]
            public ListOptions ListOptions { get; set; }

            [VerbOption("add",       HelpText = "Add a repository")]
            public AddOptions AddOptions { get; set; }

            [VerbOption("forget",    HelpText = "Forget a repository")]
            public ForgetOptions ForgetOptions { get; set; }

            [VerbOption("default",   HelpText = "Set the default repository")]
            public DefaultOptions DefaultOptions { get; set; }

            [HelpVerbOption]
            public string GetUsage(string verb)
            {
                HelpText ht = HelpText.AutoBuild(this, verb);
                // Add a usage prefix line
                ht.AddPreOptionsLine(" ");
                if (string.IsNullOrEmpty(verb))
                {
                    ht.AddPreOptionsLine("ckan repo - Manage CKAN repositories");
                    ht.AddPreOptionsLine($"Usage: ckan repo <command> [options]");
                }
                else
                {
                    ht.AddPreOptionsLine("repo " + verb + " - " + GetDescription(verb));
                    switch (verb)
                    {
                        // First the commands with two arguments
                        case "add":
                            ht.AddPreOptionsLine($"Usage: ckan repo {verb} [options] name url");
                            break;

                        // Then the commands with one argument
                        case "remove":
                        case "forget":
                        case "default":
                            ht.AddPreOptionsLine($"Usage: ckan repo {verb} [options] name");
                            break;

                        // Now the commands with only --flag type options
                        case "available":
                        case "list":
                        default:
                            ht.AddPreOptionsLine($"Usage: ckan repo {verb} [options]");
                            break;
                    }
                }
                return ht;
            }
        }

        internal class AvailableOptions : CommonOptions { }
        internal class ListOptions      : InstanceSpecificOptions { }

        internal class AddOptions : InstanceSpecificOptions
        {
            [ValueOption(0)] public string name { get; set; }
            [ValueOption(1)] public string uri { get; set; }
        }

        internal class DefaultOptions : InstanceSpecificOptions
        {
            [ValueOption(0)] public string uri { get; set; }
        }

        internal class ForgetOptions : InstanceSpecificOptions
        {
            [ValueOption(0)] public string name { get; set; }
        }

        // This is required by ISubCommand
        public int RunSubCommand(KSPManager manager, CommonOptions opts, SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();

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

            int exitCode = Exit.OK;

            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new RepoSubOptions(), (string option, object suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    User     = new ConsoleUser(options.Headless);
                    Manager  = manager ?? new KSPManager(User);
                    exitCode = options.Handle(Manager, User);
                    if (exitCode != Exit.OK)
                        return;

                    switch (option)
                    {
                        case "available":
                            exitCode = AvailableRepositories();
                            break;

                        case "list":
                            exitCode = ListRepositories();
                            break;

                        case "add":
                            exitCode = AddRepository((AddOptions)suboptions);
                            break;

                        case "remove":
                        case "forget":
                            exitCode = ForgetRepository((ForgetOptions)suboptions);
                            break;

                        case "default":
                            exitCode = DefaultRepository((DefaultOptions)suboptions);
                            break;

                        default:
                            User.RaiseMessage("Unknown command: repo {0}", option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private static RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
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
            var manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager));
            User.RaiseMessage("Listing all known repositories:");
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
            RegistryManager manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager));

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

            RegistryManager manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager));
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
            RegistryManager manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager));

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

        private KSPManager Manager { get; set; }
        private IUser      User    { get; set; }

        private static readonly ILog log = LogManager.GetLogger(typeof (Repo));
    }

    public struct RepositoryList
    {
        public Repository[] repositories;
    }

}
