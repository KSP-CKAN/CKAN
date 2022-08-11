﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        [VerbOption("forget",    HelpText = "Forget a repository")]
        public RepoForgetOptions ForgetOptions { get; set; }

        [VerbOption("default",   HelpText = "Set the default repository")]
        public RepoDefaultOptions DefaultOptions { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            HelpText ht = HelpText.AutoBuild(this, verb);
            // Add a usage prefix line
            ht.AddPreOptionsLine(" ");
            if (string.IsNullOrEmpty(verb))
            {
                ht.AddPreOptionsLine($"ckan repo - {Properties.Resources.RepoHelpSummary}");
                ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan repo <{Properties.Resources.Command}> [{Properties.Resources.Options}]");
            }
            else
            {
                ht.AddPreOptionsLine("repo " + verb + " - " + GetDescription(verb));
                switch (verb)
                {
                    // First the commands with two arguments
                    case "add":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan repo {verb} [{Properties.Resources.Options}] name url");
                        break;

                    // Then the commands with one argument
                    case "remove":
                    case "forget":
                    case "default":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan repo {verb} [{Properties.Resources.Options}] name");
                        break;

                    // Now the commands with only --flag type options
                    case "available":
                    case "list":
                    default:
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan repo {verb} [{Properties.Resources.Options}]");
                        break;
                }
            }
            return ht;
        }
    }

    public class RepoAvailableOptions : CommonOptions { }
    public class RepoListOptions      : InstanceSpecificOptions { }

    public class RepoAddOptions : InstanceSpecificOptions
    {
        [ValueOption(0)] public string name { get; set; }
        [ValueOption(1)] public string uri { get; set; }
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
        public Repo() { }

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
                    User     = new ConsoleUser(options.Headless);
                    Manager  = manager ?? new GameInstanceManager(User);
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
                            exitCode = AddRepository((RepoAddOptions)suboptions);
                            break;

                        case "remove":
                        case "forget":
                            exitCode = ForgetRepository((RepoForgetOptions)suboptions);
                            break;

                        case "default":
                            exitCode = DefaultRepository((RepoDefaultOptions)suboptions);
                            break;

                        default:
                            User.RaiseMessage(Properties.Resources.RepoUnknownCommand, option);
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
            User.RaiseMessage(Properties.Resources.RepoAvailableHeader);
            RepositoryList repositories;

            try
            {
                repositories = FetchMasterRepositoryList();
            }
            catch
            {
                User.RaiseError(Properties.Resources.RepoAvailableFailed, MainClass.GetGameInstance(Manager).game.RepositoryListURL.ToString());
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
            User.RaiseMessage(Properties.Resources.RepoListHeader);
            SortedDictionary<string, Repository> repositories = manager.registry.Repositories;

            int maxNameLen = 0;
            foreach (Repository repository in repositories.Values)
            {
                maxNameLen = Math.Max(maxNameLen, repository.name.Length);
            }

            foreach (Repository repository in repositories.Values)
            {
                User.RaiseMessage("  {0}: {1}: {2}", repository.name.PadRight(maxNameLen), repository.priority, repository.uri);
            }

            return Exit.OK;
        }

        private int AddRepository(RepoAddOptions options)
        {
            RegistryManager manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager));

            if (options.name == null)
            {
                User.RaiseMessage("add <name> [ <uri> ] - {0}", Properties.Resources.ArgumentMissing);
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
                    User.RaiseError(Properties.Resources.RepoAvailableFailed, Manager.CurrentInstance.game.RepositoryListURL.ToString());
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
                    User.RaiseMessage(Properties.Resources.RepoAddNotFound, options.name);
                    return Exit.BADOPT;
                }
            }

            log.DebugFormat("About to add repository '{0}' - '{1}'", options.name, options.uri);
            SortedDictionary<string, Repository> repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(options.name))
            {
                User.RaiseMessage(Properties.Resources.RepoAddDuplicate, options.name);
                return Exit.BADOPT;
            }

            repositories.Add(options.name, new Repository(options.name, options.uri));

            User.RaiseMessage(Properties.Resources.RepoAdded, options.name, options.uri);
            manager.Save();

            return Exit.OK;
        }

        private int ForgetRepository(RepoForgetOptions options)
        {
            if (options.name == null)
            {
                User.RaiseError("forget <name> - {0}", Properties.Resources.ArgumentMissing);
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
                    User.RaiseMessage(Properties.Resources.RepoForgetNotFound, options.name);
                    return Exit.BADOPT;
                }
                User.RaiseMessage(Properties.Resources.RepoForgetRemoving, name);
            }

            registry.Repositories.Remove(name);
            User.RaiseMessage(Properties.Resources.RepoForgetRemoved, options.name);
            manager.Save();

            return Exit.OK;
        }

        private int DefaultRepository(RepoDefaultOptions options)
        {
            RegistryManager manager = RegistryManager.Instance(MainClass.GetGameInstance(Manager));

            if (options.uri == null)
            {
                User.RaiseMessage("default <uri> - {0}", Properties.Resources.ArgumentMissing);
                return Exit.BADOPT;
            }

            log.DebugFormat("About to add repository '{0}' - '{1}'", Repository.default_ckan_repo_name, options.uri);
            SortedDictionary<string, Repository> repositories = manager.registry.Repositories;

            if (repositories.ContainsKey(Repository.default_ckan_repo_name))
            {
                repositories.Remove(Repository.default_ckan_repo_name);
            }

            repositories.Add(Repository.default_ckan_repo_name, new Repository(
                    Repository.default_ckan_repo_name, options.uri));

            User.RaiseMessage(Properties.Resources.RepoSet, Repository.default_ckan_repo_name, options.uri);
            manager.Save();

            return Exit.OK;
        }

        private GameInstanceManager Manager { get; set; }
        private IUser               User    { get; set; }

        private static readonly ILog log = LogManager.GetLogger(typeof (Repo));
    }

    public struct RepositoryList
    {
        public Repository[] repositories;
    }

}
