using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{
    public class RepoSubOptions : VerbCommandOptions
    {
        [VerbOption("available", HelpText = "List (canonical) available repositories")]
        public RepoAvailableOptions AvailableOptions { get; set; }

        [VerbOption("list",      HelpText = "List repositories")]
        public RepoListOptions ListOptions { get; set; }

        [VerbOption("add",       HelpText = "Add a repository")]
        public RepoAddOptions AddOptions { get; set; }

        [VerbOption("priority",  HelpText = "Set repository priority")]
        public RepoPriorityOptions PriorityOptions { get; set; }

        [VerbOption("forget",    HelpText = "Forget a repository")]
        public RepoForgetOptions ForgetOptions { get; set; }

        [VerbOption("default",   HelpText = "Set the default repository")]
        public RepoDefaultOptions DefaultOptions { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var ht = HelpText.AutoBuild(this, verb);
            foreach (var h in GetHelp(verb))
            {
                ht.AddPreOptionsLine(h);
            }
            return ht;
        }

        public static IEnumerable<string> GetHelp(string verb)
        {
            // Add a usage prefix line
            yield return " ";
            if (string.IsNullOrEmpty(verb))
            {
                yield return $"ckan repo - {Properties.Resources.RepoHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan repo <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "repo " + verb + " - " + GetDescription(typeof(RepoSubOptions), verb);
                switch (verb)
                {
                    // First the commands with two arguments
                    case "add":
                        yield return $"{Properties.Resources.Usage}: ckan repo {verb} [{Properties.Resources.Options}] name url";
                        break;

                    case "priority":
                        yield return $"{Properties.Resources.Usage}: ckan repo {verb} [{Properties.Resources.Options}] name priority";
                        break;

                    // Then the commands with one argument
                    case "remove":
                    case "forget":
                    case "default":
                        yield return $"{Properties.Resources.Usage}: ckan repo {verb} [{Properties.Resources.Options}] name";
                        break;

                    // Now the commands with only --flag type options
                    case "available":
                    case "list":
                    default:
                        yield return $"{Properties.Resources.Usage}: ckan repo {verb} [{Properties.Resources.Options}]";
                        break;
                }
            }
        }
    }

    public class RepoAvailableOptions : CommonOptions { }
    public class RepoListOptions      : InstanceSpecificOptions { }

    public class RepoAddOptions : InstanceSpecificOptions
    {
        [ValueOption(0)] public string name { get; set; }
        [ValueOption(1)] public string uri { get; set; }
    }

    public class RepoPriorityOptions : InstanceSpecificOptions
    {
        [ValueOption(0)] public string name     { get; set; }
        [ValueOption(1)] public int    priority { get; set; }
    }

    public class RepoDefaultOptions : InstanceSpecificOptions
    {
        [ValueOption(0)] public string uri { get; set; }
    }

    public class RepoForgetOptions : InstanceSpecificOptions
    {
        [ValueOption(0)] public string name { get; set; }
    }

    public class Repo : ISubCommand
    {
        public Repo(RepositoryDataManager repoData)
        {
            this.repoData = repoData;
        }

        // This is required by ISubCommand
        public int RunSubCommand(GameInstanceManager manager, CommonOptions opts, SubCommandOptions unparsed)
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
                    user     = new ConsoleUser(options.Headless);
                    Manager  = manager ?? new GameInstanceManager(user);
                    exitCode = options.Handle(Manager, user);
                    if (exitCode != Exit.OK)
                    {
                        return;
                    }

                    switch (option)
                    {
                        case "available":
                            exitCode = AvailableRepositories();
                            break;

                        case "list":
                            exitCode = ListRepositories();
                            break;

                        case "add":
                            exitCode = AddRepository((RepoAddOptions)suboptions);
                            break;

                        case "priority":
                            exitCode = SetRepositoryPriority((RepoPriorityOptions)suboptions);
                            break;

                        case "remove":
                        case "forget":
                            exitCode = ForgetRepository((RepoForgetOptions)suboptions);
                            break;

                        case "default":
                            exitCode = DefaultRepository((RepoDefaultOptions)suboptions);
                            break;

                        default:
                            user.RaiseMessage(Properties.Resources.RepoUnknownCommand, option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private RepositoryList FetchMasterRepositoryList(Uri master_uri = null)
        {
            if (master_uri == null)
            {
                master_uri = MainClass.GetGameInstance(Manager).game.RepositoryListURL;
            }

            string json = Net.DownloadText(master_uri);
            return JsonConvert.DeserializeObject<RepositoryList>(json);
        }

        private int AvailableRepositories()
        {
            user.RaiseMessage(Properties.Resources.RepoAvailableHeader);
            RepositoryList repositories;

            try
            {
                repositories = FetchMasterRepositoryList();
            }
            catch
            {
                user.RaiseError(Properties.Resources.RepoAvailableFailed, MainClass.GetGameInstance(Manager).game.RepositoryListURL.ToString());
                return Exit.ERROR;
            }

            int maxNameLen = 0;
            foreach (Repository repository in repositories.repositories)
            {
                maxNameLen = Math.Max(maxNameLen, repository.name.Length);
            }

            foreach (Repository repository in repositories.repositories)
            {
                user.RaiseMessage("  {0}: {1}", repository.name.PadRight(maxNameLen), repository.uri);
            }

            return Exit.OK;
        }

        private int ListRepositories()
        {
            var repositories = RegistryManager.Instance(MainClass.GetGameInstance(Manager), repoData).registry.Repositories;

            string priorityHeader = Properties.Resources.RepoListPriorityHeader;
            string nameHeader     = Properties.Resources.RepoListNameHeader;
            string urlHeader      = Properties.Resources.RepoListURLHeader;

            var priorityWidth = Enumerable.Repeat(priorityHeader, 1)
                                          .Concat(repositories.Values.Select(r => $"{r.priority}"))
                                          .Max(str => str.Length);
            var nameWidth     = Enumerable.Repeat(nameHeader, 1)
                                          .Concat(repositories.Values.Select(r => r.name))
                                          .Max(str => str.Length);
            var urlWidth      = Enumerable.Repeat(urlHeader, 1)
                                          .Concat(repositories.Values.Select(r => $"{r.uri}"))
                                          .Max(str => str.Length);

            const string columnFormat = "{0}  {1}  {2}";

            user.RaiseMessage(columnFormat,
                              priorityHeader.PadRight(priorityWidth),
                              nameHeader.PadRight(nameWidth),
                              urlHeader.PadRight(urlWidth));
            user.RaiseMessage(columnFormat,
                              new string('-', priorityWidth),
                              new string('-', nameWidth),
                              new string('-', urlWidth));
            foreach (Repository repository in repositories.Values.OrderBy(r => r.priority))
            {
                user.RaiseMessage(columnFormat,
                                  repository.priority.ToString().PadRight(priorityWidth),
                                  repository.name.PadRight(nameWidth),
                                  repository.uri);
            }
            return Exit.OK;
        }

        private int AddRepository(RepoAddOptions options)
        {
            RegistryManager manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager), repoData);

            if (options.name == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("add");
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
                    user.RaiseError(Properties.Resources.RepoAvailableFailed, Manager.CurrentInstance.game.RepositoryListURL.ToString());
                    return Exit.ERROR;
                }

                foreach (Repository candidate in repositoryList.repositories)
                {
                    if (string.Equals(candidate.name, options.name, StringComparison.OrdinalIgnoreCase))
                    {
                        options.name = candidate.name;
                        options.uri = candidate.uri.ToString();
                    }
                }

                // Nothing found in the master list?
                if (options.uri == null)
                {
                    user.RaiseMessage(Properties.Resources.RepoAddNotFound, options.name);
                    return Exit.BADOPT;
                }
            }

            log.DebugFormat("About to add repository '{0}' - '{1}'", options.name, options.uri);
            var repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(options.name))
            {
                user.RaiseMessage(Properties.Resources.RepoAddDuplicate, options.name);
                return Exit.BADOPT;
            }
            if (repositories.Values.Any(r => r.uri.ToString() == options.uri))
            {
                user.RaiseMessage(Properties.Resources.RepoAddDuplicateURL, options.uri);
                return Exit.BADOPT;
            }

            manager.registry.RepositoriesAdd(new Repository(options.name, options.uri,
                                                            manager.registry.Repositories.Count));

            user.RaiseMessage(Properties.Resources.RepoAdded, options.name, options.uri);
            manager.Save();

            return Exit.OK;
        }

        private int SetRepositoryPriority(RepoPriorityOptions options)
        {
            if (options.name == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("priority");
                return Exit.BADOPT;
            }
            var manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager), repoData);
            if (options.priority < 0 || options.priority >= manager.registry.Repositories.Count)
            {
                user.RaiseMessage(Properties.Resources.RepoPriorityInvalid,
                    options.priority, manager.registry.Repositories.Count - 1);
                return Exit.BADOPT;
            }

            if (manager.registry.Repositories.TryGetValue(options.name, out Repository repo))
            {
                if (options.priority != repo.priority)
                {
                    var sortedRepos = manager.registry.Repositories.Values
                        .OrderBy(r => r.priority)
                        .ToList();
                    // Shift other repos up or down by 1 to make room in the list
                    if (options.priority < repo.priority)
                    {
                        for (int i = options.priority; i < repo.priority; ++i)
                        {
                            sortedRepos[i].priority = i + 1;
                        }
                    }
                    else
                    {
                        for (int i = repo.priority + 1; i <= options.priority; ++i)
                        {
                            sortedRepos[i].priority = i - 1;
                        }
                    }
                    // Move chosen repo into new spot and save
                    repo.priority = options.priority;
                    manager.Save();
                }
                return ListRepositories();
            }
            else
            {
                user.RaiseMessage(Properties.Resources.RepoPriorityNotFound, options.name);
                return Exit.BADOPT;
            }
        }

        private int ForgetRepository(RepoForgetOptions options)
        {
            if (options.name == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("forget");
                return Exit.BADOPT;
            }

            RegistryManager manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager), repoData);
            log.DebugFormat("About to forget repository '{0}'", options.name);

            var repos = manager.registry.Repositories;

            string name = options.name;
            if (!repos.ContainsKey(options.name))
            {
                name = repos.Keys.FirstOrDefault(repo => repo.Equals(options.name, StringComparison.OrdinalIgnoreCase));
                if (name == null)
                {
                    user.RaiseMessage(Properties.Resources.RepoForgetNotFound, options.name);
                    return Exit.BADOPT;
                }
                user.RaiseMessage(Properties.Resources.RepoForgetRemoving, name);
            }

            manager.registry.RepositoriesRemove(name);
            var remaining = repos.Values.OrderBy(r => r.priority).ToArray();
            for (int i = 0; i < remaining.Length; ++i)
            {
                remaining[i].priority = i;
            }
            user.RaiseMessage(Properties.Resources.RepoForgetRemoved, options.name);
            manager.Save();

            return Exit.OK;
        }

        private int DefaultRepository(RepoDefaultOptions options)
        {
            var inst = MainClass.GetGameInstance(Manager);
            var uri = options.uri ?? inst.game.DefaultRepositoryURL.ToString();

            log.DebugFormat("About to add repository '{0}' - '{1}'", Repository.default_ckan_repo_name, uri);
            RegistryManager manager = RegistryManager.Instance(inst, repoData);
            var repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(Repository.default_ckan_repo_name))
            {
                manager.registry.RepositoriesRemove(Repository.default_ckan_repo_name);
            }

            manager.registry.RepositoriesAdd(
                new Repository(Repository.default_ckan_repo_name, uri, repositories.Count));

            user.RaiseMessage(Properties.Resources.RepoSet, Repository.default_ckan_repo_name, uri);
            manager.Save();

            return Exit.OK;
        }

        private void PrintUsage(string verb)
        {
            foreach (var h in RepoSubOptions.GetHelp(verb))
            {
                user.RaiseError(h);
            }
        }

        private GameInstanceManager   Manager;
        private readonly RepositoryDataManager repoData;
        private IUser                 user;

        private static readonly ILog log = LogManager.GetLogger(typeof (Repo));
    }

    public struct RepositoryList
    {
        public Repository[] repositories;
    }

}
