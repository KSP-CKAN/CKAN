using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using CommandLine;
using log4net;

namespace CKAN.CmdLine
{
    // TODO maybe rename to "remote" or something so this class can be re-used between "multiple repos" and "mirrors"?
    public struct Repository
    {
        public string name;
        public Uri uri;
        
        public override string ToString()
        {
            return String.Format("{0} ({1})", name, uri.DnsSafeHost);
        }
    }
    
    public struct RepositoryList
    {
        public Repository[] repositories;
    }

    public class Repo : ISubCommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Repo));

        public string option;
        public object suboptions;

        // TODO Change the URL base to api.ksp-ckan.org
        public static readonly Uri default_repo_master_list = new Uri("http://ksp.gurkensalat.com/repositories.json");

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
        }

        internal class AvailableOptions : CommonOptions
        {
        }

        internal class AddOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }

            [ValueOption(1)]
            public string uri { get; set; }
        }

        internal class ForgetOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }
        }

        public Repo()
        {
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

            if (args == null || args.Length == 0)
            {
                // There's got to be a better way of showing help...
                args = new string[1];
                args[0] = "help";
            }

            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new RepoSubOptions (), Parse, null);

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

                default:
                    User.WriteLine("Unknown command: ksp {0}", option);
                    return Exit.BADOPT;
            }
        }

        public static RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
        {
            WebClient client = new WebClient();

            if (master_uri == null)
            {
                master_uri = default_repo_master_list;
            }
            
            string json = client.DownloadString(master_uri);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }

        private static int AvailableRepositories()
        {
            User.WriteLine("Listing all (canonical) available CKAN repositories:");
            RepositoryList repositories = new RepositoryList();
            
            try
            {
                repositories = FetchMasterRepositoryList();
            }
            catch
            {
                User.Error("Couldn't fetch CKAN repositories master list from {0}", Repo.default_repo_master_list.ToString());
                return Exit.ERROR;
            }
            
            int maxNameLen = 0;
            foreach (Repository repository in repositories.repositories)
            {
                maxNameLen = Math.Max(maxNameLen, repository.name.Length);
            }

            foreach (Repository repository in repositories.repositories)
            {
                User.WriteLine("  {0}: {1}", repository.name.PadRight(maxNameLen), repository.uri);
            }
            
            return Exit.OK;
        }

        private static int ListRepositories()
        {
            User.WriteLine("Listing all known repositories:");
            RegistryManager manager = RegistryManager.Instance(KSPManager.CurrentInstance);
            Dictionary<string, Uri> repositories = manager.registry.Repositories;

            int maxNameLen = 0;
            foreach(KeyValuePair<string, Uri> repository in repositories)
            {
                maxNameLen = Math.Max(maxNameLen, repository.Key.Length);
            }

            foreach(KeyValuePair<string, Uri> repository in repositories)
            {
                User.WriteLine("  {0}: {1}", repository.Key.PadRight(maxNameLen), repository.Value);
            }

            return Exit.OK;
        }

        private static int AddRepository(AddOptions options)
        {
            RegistryManager manager = RegistryManager.Instance(KSPManager.CurrentInstance);

            if (options.name == null)
            {
                User.WriteLine("add <name> [ <uri> ] - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            if (options.uri == null)
            {
                RepositoryList repositoryList = new RepositoryList();

                try
                {
                    repositoryList = FetchMasterRepositoryList();
                }
                catch
                {
                    User.Error("Couldn't fetch CKAN repositories master list from {0}", Repo.default_repo_master_list.ToString());
                    return Exit.ERROR;
                }

                foreach (Repository repository in repositoryList.repositories)
                {
                    if (String.Equals(repository.name, options.name, StringComparison.OrdinalIgnoreCase))
                    {
                        options.name = repository.name;
                        options.uri = repository.uri.ToString();
                    }
                }

                // Nothing found in the master list?
                if (options.uri == null)
                {
                    User.WriteLine("Name {0} not found in master list, please provide name and uri.", options.name);
                    return Exit.BADOPT;
                }
            }

            log.DebugFormat("About to add repository '{0}' - '{1}'", options.name, options.uri);
            Dictionary<string, Uri> repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(options.name))
            {
                User.WriteLine("Repository with name \"{0}\" already exists, aborting..", options.name);
                return Exit.BADOPT;
            }

            repositories.Add(options.name, new System.Uri(options.uri));
            manager.Save();

            return Exit.OK;
        }

        private static int ForgetRepository(ForgetOptions options)
        {
            RegistryManager manager = RegistryManager.Instance(KSPManager.CurrentInstance);

            if (options.name == null)
            {
                User.WriteLine("forget <name> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            log.DebugFormat("About to forget repository '{0}'", options.name);
            Dictionary<string, Uri> repositories = manager.registry.Repositories;

            if (!(repositories.ContainsKey(options.name)))
            {
                User.WriteLine("Couldn't find repository with name \"{0}\", aborting..", options.name);
                return Exit.BADOPT;
            }

            repositories.Remove(options.name);
            manager.Save();

            return Exit.OK;
        }
    }
}