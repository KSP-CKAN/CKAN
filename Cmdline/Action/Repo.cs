using System;
using System.Linq;
using CommandLine;
using log4net;
using Newtonsoft.Json;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing CKAN repositories.
    /// </summary>
    public class Repo : ISubCommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Repo));

        private GameInstanceManager _manager;
        private IUser _user;

        /// <summary>
        /// Run the 'repo' command.
        /// </summary>
        /// <inheritdoc cref="ISubCommand.RunCommand"/>
        public int RunCommand(GameInstanceManager manager, object args)
        {
            var s = args.ToString();
            var opts = s.Replace(s.Substring(0, s.LastIndexOf('.') + 1), "").Split('+');

            CommonOptions options = new CommonOptions();
            _user = new ConsoleUser(options.Headless);
            _manager = manager ?? new GameInstanceManager(_user);
            var exitCode = options.Handle(_manager, _user);

            if (exitCode != Exit.Ok)
                return exitCode;

            switch (opts[1])
            {
                case "AddRepo":
                    exitCode = AddRepository(args);
                    break;
                case "AvailableRepo":
                    exitCode = AvailableRepositories();
                    break;
                case "DefaultRepo":
                    exitCode = DefaultRepository(args);
                    break;
                case "ForgetRepo":
                    exitCode = ForgetRepository(args);
                    break;
                case "ListRepo":
                    exitCode = ListRepositories();
                    break;
                case "ResetRepo":
                    exitCode = ResetRepository();
                    break;
                default:
                    exitCode = Exit.BadOpt;
                    break;
            }

            return exitCode;
        }

        /// <inheritdoc cref="ISubCommand.GetUsage"/>
        public string GetUsage(string prefix, string[] args)
        {
            if (args.Length == 1)
                return $"{prefix} {args[0]} <command> [options]";

            switch (args[1])
            {
                case "add":
                    return $"{prefix} {args[0]} {args[1]} [options] <name> <url>";
                case "default":
                    return $"{prefix} {args[0]} {args[1]} [options] <url>";
                case "forget":
                    return $"{prefix} {args[0]} {args[1]} [options] <name>";
                case "available":
                case "list":
                case "reset":
                    return $"{prefix} {args[0]} {args[1]} [options]";
                default:
                    return $"{prefix} {args[0]} <command> [options]";
            }
        }

        private int AddRepository(object args)
        {
            var opts = (RepoOptions.AddRepo)args;
            if (opts.Name == null)
            {
                _user.RaiseMessage("add <name> <url> - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            if (opts.Url == null)
            {
                RepositoryList repositoryList;
                try
                {
                    repositoryList = FetchMasterRepositoryList();
                }
                catch
                {
                    _user.RaiseError("Couldn't fetch CKAN repositories master list from \"{0}\".", _manager.CurrentInstance.game.RepositoryListURL.ToString());
                    return Exit.Error;
                }

                foreach (var candidate in repositoryList.repositories)
                {
                    if (string.Equals(candidate.name, opts.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        opts.Name = candidate.name;
                        opts.Url = candidate.uri.ToString();
                    }
                }

                // Nothing found in the master list?
                if (opts.Url == null)
                {
                    _user.RaiseMessage("add <name> <url> - argument(s) missing, perhaps you forgot it?");
                    return Exit.BadOpt;
                }
            }

            Log.DebugFormat("About to add the repository \"{0}\" - \"{1}\".", opts.Name, opts.Url);
            var manager = RegistryManager.Instance(MainClass.GetGameInstance(_manager));
            var repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(opts.Name))
            {
                _user.RaiseMessage("A repository with the name \"{0}\" already exists, aborting...", opts.Name);
                return Exit.BadOpt;
            }

            repositories.Add(opts.Name, new Repository(opts.Name, opts.Url));

            _user.RaiseMessage("Successfully added the repository \"{0}\" - \"{1}\".", opts.Name, opts.Url);
            manager.Save();

            return Exit.Ok;
        }

        private int AvailableRepositories()
        {
            _user.RaiseMessage("Listing all (canonical) available CKAN repositories:");
            RepositoryList repositories;
            try
            {
                repositories = FetchMasterRepositoryList();
            }
            catch
            {
                _user.RaiseError("Couldn't fetch CKAN repositories master list from \"{0}\"", MainClass.GetGameInstance(_manager).game.RepositoryListURL.ToString());
                return Exit.Error;
            }

            var maxNameLen = 0;
            foreach (var repository in repositories.repositories)
            {
                maxNameLen = Math.Max(maxNameLen, repository.name.Length);
            }

            foreach (var repository in repositories.repositories)
            {
                _user.RaiseMessage("  {0}: {1}", repository.name.PadRight(maxNameLen), repository.uri);
            }

            return Exit.Ok;
        }

        private int DefaultRepository(object args)
        {
            var opts = (RepoOptions.DefaultRepo)args;
            if (opts.Url == null)
            {
                _user.RaiseMessage("default <url> - argument missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            Log.DebugFormat("About to set \"{0}\" as the {1} repository.", opts.Url, Repository.default_ckan_repo_name);
            var manager = RegistryManager.Instance(MainClass.GetGameInstance(_manager));
            var repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(Repository.default_ckan_repo_name))
            {
                repositories.Remove(Repository.default_ckan_repo_name);
            }

            repositories.Add(Repository.default_ckan_repo_name, new Repository(Repository.default_ckan_repo_name, opts.Url));

            _user.RaiseMessage("Set the {0} repository to \"{1}\".", Repository.default_ckan_repo_name, opts.Url);
            manager.Save();

            return Exit.Ok;
        }

        private int ForgetRepository(object args)
        {
            var opts = (RepoOptions.ForgetRepo)args;
            if (opts.Name == null)
            {
                _user.RaiseMessage("forget <name> - argument missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            Log.DebugFormat("About to forget the repository \"{0}\".", opts.Name);
            var manager = RegistryManager.Instance(MainClass.GetGameInstance(_manager));
            var repositories = manager.registry.Repositories;

            var name = opts.Name;
            if (!repositories.ContainsKey(opts.Name))
            {
                name = repositories.Keys.FirstOrDefault(repo => repo.Equals(opts.Name, StringComparison.OrdinalIgnoreCase));
                if (name == null)
                {
                    _user.RaiseMessage("Couldn't find a repository with the name \"{0}\", aborting...", opts.Name);
                    return Exit.BadOpt;
                }

                _user.RaiseMessage("Removing insensitive match \"{0}\".", name);
            }

            repositories.Remove(name);

            _user.RaiseMessage("Successfully removed \"{0}\".", opts.Name);
            manager.Save();

            return Exit.Ok;
        }

        private int ListRepositories()
        {
            _user.RaiseMessage("Listing all known repositories:");
            var manager = RegistryManager.Instance(MainClass.GetGameInstance(_manager));
            var repositories = manager.registry.Repositories;

            var maxNameLen = 0;
            foreach (var repository in repositories.Values)
            {
                maxNameLen = Math.Max(maxNameLen, repository.name.Length);
            }

            foreach (var repository in repositories.Values)
            {
                _user.RaiseMessage("  {0}: {1}: {2}", repository.name.PadRight(maxNameLen), repository.priority, repository.uri);
            }

            return Exit.Ok;
        }

        private int ResetRepository()
        {
            Log.DebugFormat("About to reset the {0} repository.", Repository.default_ckan_repo_name);
            var manager = RegistryManager.Instance(MainClass.GetGameInstance(_manager));
            var repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(Repository.default_ckan_repo_name))
            {
                repositories.Remove(Repository.default_ckan_repo_name);
            }

            repositories.Add(Repository.default_ckan_repo_name, new Repository(Repository.default_ckan_repo_name, MainClass.GetGameInstance(_manager).game.DefaultRepositoryURL));

            _user.RaiseMessage("Reset the {0} repository to \"{1}\".", Repository.default_ckan_repo_name, MainClass.GetGameInstance(_manager).game.DefaultRepositoryURL);
            manager.Save();

            return Exit.Ok;
        }

        private RepositoryList FetchMasterRepositoryList(Uri masterUrl = null)
        {
            if (masterUrl == null)
            {
                masterUrl = MainClass.GetGameInstance(_manager).game.RepositoryListURL;
            }

            var json = Net.DownloadText(masterUrl);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }
    }

    public struct RepositoryList
    {
        public Repository[] repositories;
    }

    [Verb("repo", HelpText = "Manage CKAN repositories")]
    [ChildVerbs(typeof(AddRepo), typeof(AvailableRepo), typeof(DefaultRepo), typeof(ForgetRepo), typeof(ListRepo), typeof(ResetRepo))]
    internal class RepoOptions
    {
        [VerbExclude]
        [Verb("add", HelpText = "Add a repository")]
        internal class AddRepo : InstanceSpecificOptions
        {
            [Value(0, MetaName = "Name", HelpText = "The name of the new repository")]
            public string Name { get; set; }

            [Value(1, MetaName = "Url", HelpText = "The url of the new repository")]
            public string Url { get; set; }
        }

        [VerbExclude]
        [Verb("available", HelpText = "List (canonical) available repositories")]
        internal class AvailableRepo : CommonOptions { }

        [VerbExclude]
        [Verb("default", HelpText = "Set the default repository")]
        internal class DefaultRepo : InstanceSpecificOptions
        {
            [Value(0, MetaName = "Url", HelpText = "The url of the repository to make the default")]
            public string Url { get; set; }
        }

        [VerbExclude]
        [Verb("forget", HelpText = "Forget a repository")]
        internal class ForgetRepo : InstanceSpecificOptions
        {
            [Value(0, MetaName = "Name", HelpText = "The name of the repository to remove")]
            public string Name { get; set; }
        }

        [VerbExclude]
        [Verb("list", HelpText = "List repositories")]
        internal class ListRepo : InstanceSpecificOptions { }

        [VerbExclude]
        [Verb("reset", HelpText = "Set the default repository to the default")]
        internal class ResetRepo : InstanceSpecificOptions { }
    }
}
