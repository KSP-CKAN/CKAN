using System;
using System.Collections.Generic;
using Autofac;
using CKAN.Configuration;
using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{
    /// <summary>
    /// Subcommand for managing authentication tokens
    /// </summary>
    public class AuthToken : ISubCommand
    {
        /// <summary>
        /// Initialize the subcommand
        /// </summary>
        public AuthToken() { }

        /// <summary>
        /// Run the subcommand
        /// </summary>
        /// <param name="mgr">Manager to provide game instances</param>
        /// <param name="opts">Command line parameters paritally handled by parser</param>
        /// <param name="unparsed">Command line parameters not yet handled by parser</param>
        /// <returns>
        /// Exit code
        /// </returns>
        public int RunSubCommand(GameInstanceManager manager, CommonOptions opts, SubCommandOptions unparsed)
        {
            string[] args     = unparsed.options.ToArray();
            int      exitCode = Exit.OK;

            Parser.Default.ParseArgumentsStrict(args, new AuthTokenSubOptions(), (string option, object suboptions) =>
            {
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    user                  = new ConsoleUser(options.Headless);
                    if (manager == null)
                    {
                        manager           = new GameInstanceManager(user);
                    }
                    exitCode              = options.Handle(manager, user);
                    if (exitCode == Exit.OK)
                    {
                        switch (option)
                        {
                            case "list":
                                exitCode = listAuthTokens(options);
                                break;
                            case "add":
                                exitCode = addAuthToken((AddAuthTokenOptions)options);
                                break;
                            case "remove":
                                exitCode = removeAuthToken((RemoveAuthTokenOptions)options);
                                break;
                        }
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private int listAuthTokens(CommonOptions opts)
        {
            string hostHeader  = Properties.Resources.AuthTokenHostHeader;
            string tokenHeader = Properties.Resources.AuthTokenTokenHeader;
            List<string> hosts  = new List<string>(ServiceLocator.Container.Resolve<IConfiguration>().GetAuthTokenHosts());
            if (hosts.Count > 0)
            {
                int longestHostLen  = hostHeader.Length;
                int longestTokenLen = tokenHeader.Length;
                foreach (string host in hosts)
                {
                    longestHostLen = Math.Max(longestHostLen, host.Length);
                    string token;
                    if (ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(host, out token))
                    {
                        longestTokenLen = Math.Max(longestTokenLen, token.Length);
                    }
                }
                // Create format string: {0,-longestHostLen}  {1,-longestTokenLen}
                string fmt = string.Format("{0}0,-{2}{1}  {0}1,-{3}{1}",
                    "{", "}", longestHostLen, longestTokenLen);
                user.RaiseMessage(fmt, hostHeader, tokenHeader);
                user.RaiseMessage(fmt,
                    new string('-', longestHostLen),
                    new string('-', longestTokenLen)
                );
                foreach (string host in hosts)
                {
                    string token;
                    if (ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(host, out token))
                    {
                        user.RaiseMessage(fmt, host, token);
                    }
                }
            }
            return Exit.OK;
        }

        private int addAuthToken(AddAuthTokenOptions opts)
        {
            if (Uri.CheckHostName(opts.host) != UriHostNameType.Unknown)
            {
                ServiceLocator.Container.Resolve<IConfiguration>().SetAuthToken(opts.host, opts.token);
            }
            else
            {
                user.RaiseError("Invalid host name: {0}", opts.host);
            }
            return Exit.OK;
        }

        private int removeAuthToken(RemoveAuthTokenOptions opts)
        {
            ServiceLocator.Container.Resolve<IConfiguration>().SetAuthToken(opts.host, null);
            return Exit.OK;
        }

        private                 IUser user;
        private static readonly ILog  log = LogManager.GetLogger(typeof(AuthToken));
    }

    internal class AuthTokenSubOptions : VerbCommandOptions
    {
        [VerbOption("list",   HelpText = "List auth tokens")]
        public CommonOptions          ListOptions   { get; set; }

        [VerbOption("add",    HelpText = "Add an auth token")]
        public AddAuthTokenOptions    AddOptions    { get; set; }

        [VerbOption("remove", HelpText = "Delete an auth token")]
        public RemoveAuthTokenOptions RemoveOptions { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            HelpText ht = HelpText.AutoBuild(this, verb);
            // Add a usage prefix line
            ht.AddPreOptionsLine(" ");
            if (string.IsNullOrEmpty(verb))
            {
                ht.AddPreOptionsLine($"ckan authtoken - {Properties.Resources.AuthTokenHelpSummary}");
                ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan authtoken <{Properties.Resources.Command}> [{Properties.Resources.Options}]");
            }
            else
            {
                ht.AddPreOptionsLine("authtoken " + verb + " - " + GetDescription(verb));
                switch (verb)
                {
                    case "add":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan authtoken {verb} [{Properties.Resources.Options}] host token");
                        break;
                    case "remove":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan authtoken {verb} [{Properties.Resources.Options}] host");
                        break;
                    case "list":
                        ht.AddPreOptionsLine($"{Properties.Resources.Usage}: ckan authtoken {verb} [{Properties.Resources.Options}]");
                        break;
                }
            }
            return ht;
        }
    }

    internal class AddAuthTokenOptions : CommonOptions
    {
        [ValueOption(0)] public string host  { get; set; }
        [ValueOption(1)] public string token { get; set; }
    }

    internal class RemoveAuthTokenOptions : CommonOptions
    {
        [ValueOption(0)] public string host { get; set; }
    }

}
