using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using CommandLine;

namespace CKAN.CmdLine
{
    // TODO maybe rename to "remote" or something so this class can be re-used between "multiple repos" and "mirrors"?
    public struct Repository
    {
        public string name;
        public Uri url;
        
        public override string ToString()
        {
            return String.Format("{0} ({1})", name, url.DnsSafeHost);
        }
    }
    
    public struct RepositoryList
    {
        public Repository[] repositories;
    }

    public class Repo : ISubCommand
    {
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

            [VerbOption("rename", HelpText="Rename a repository")]
            public RenameOptions RenameOptions { get; set; }

            [VerbOption("forget", HelpText="Forget a repository")]
            public ForgetOptions ForgetOptions { get; set; }

            [VerbOption("default", HelpText="Set the default repository")]
            public DefaultOptions DefaultOptions { get; set; }
        }

        internal class AvailableOptions : CommonOptions
        {
        }

        internal class AddOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }

            [ValueOption(1)]
            public string path { get; set; }
        }

        internal class RenameOptions : CommonOptions
        {
            [ValueOption(0)]
            public string old_name { get; set; }

            [ValueOption(1)]
            public string new_name { get; set; }
        }

        internal class ForgetOptions : CommonOptions
        {
            [ValueOption(0)]
            public string name { get; set; }
        }

        internal class DefaultOptions : CommonOptions
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
                    return ListInstalls();

                case "add":
                    return AddInstall((AddOptions)suboptions);

                case "rename":
                    return RenameInstall((RenameOptions)suboptions);

                case "forget":
                    return ForgetInstall((ForgetOptions)suboptions);

                case "default":
                    return SetDefaultInstall((DefaultOptions)suboptions);

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
                User.WriteLine("  {0}: {1}", repository.name.PadRight(maxNameLen), repository.url);
            }
            
            return Exit.OK;
        }

        private static int ListInstalls()
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

        private static int AddInstall(AddOptions options)
        {
            User.WriteLine("Adding repository:");
            return Exit.OK;
        }

        private static int RenameInstall(RenameOptions options)
        {
            User.WriteLine("Renaming repository:");
            return Exit.OK;
        }

        private static int ForgetInstall(ForgetOptions options)
        {
            User.WriteLine("Forgetting repository:");
            return Exit.OK;
        }

        private static int SetDefaultInstall(DefaultOptions options)
        {
            User.WriteLine("Setting the default repository:");
            return Exit.OK;
        }
    }
}