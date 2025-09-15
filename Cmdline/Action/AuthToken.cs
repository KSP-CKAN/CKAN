using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

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
        public AuthToken(GameInstanceManager manager,
                         IUser               user)
        {
            this.manager = manager;
            this.user    = user;
        }

        /// <summary>
        /// Run the subcommand
        /// </summary>
        /// <param name="mgr">Manager to provide game instances</param>
        /// <param name="opts">Command line parameters paritally handled by parser</param>
        /// <param name="unparsed">Command line parameters not yet handled by parser</param>
        /// <returns>
        /// Exit code
        /// </returns>
        public int RunSubCommand(CommonOptions?    opts,
                                 SubCommandOptions unparsed)
        {
            string[] args     = unparsed.options.ToArray();
            int      exitCode = Exit.OK;

            Parser.Default.ParseArgumentsStrict(args, new AuthTokenSubOptions(), (option, suboptions) =>
            {
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    exitCode  = options.Handle(manager, user);
                    if (exitCode == Exit.OK)
                    {
                        switch (option)
                        {
                            case "list":
                                exitCode = listAuthTokens();
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
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private int listAuthTokens()
        {
            string hostHeader  = Properties.Resources.AuthTokenHostHeader;
            string tokenHeader = Properties.Resources.AuthTokenTokenHeader;
            List<string> hosts  = manager.Configuration.GetAuthTokenHosts().ToList();
            if (hosts.Count > 0)
            {
                int longestHostLen  = hostHeader.Length;
                int longestTokenLen = tokenHeader.Length;
                foreach (string host in hosts)
                {
                    longestHostLen = Math.Max(longestHostLen, host.Length);
                    if (manager.Configuration.TryGetAuthToken(host, out string? token))
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
                                   new string('-', longestTokenLen));
                foreach (string host in hosts)
                {
                    if (manager.Configuration.TryGetAuthToken(host, out string? token))
                    {
                        user.RaiseMessage(fmt, host, token);
                    }
                }
            }
            return Exit.OK;
        }

        private int addAuthToken(AddAuthTokenOptions opts)
        {
            if (opts.host is string h)
            {
                if (Uri.CheckHostName(h) != UriHostNameType.Unknown)
                {
                    manager.Configuration.SetAuthToken(h, opts.token);
                }
                else
                {
                    user.RaiseError(Properties.Resources.AuthTokenInvalidHostName, h);
                }
            }
            return Exit.OK;
        }

        private int removeAuthToken(RemoveAuthTokenOptions opts)
        {
            if (opts.host is string h)
            {
                manager.Configuration.SetAuthToken(h, null);
            }
            return Exit.OK;
        }

        private readonly GameInstanceManager manager;
        private readonly IUser               user;
    }

    [ExcludeFromCodeCoverage]
    internal class AuthTokenSubOptions : VerbCommandOptions
    {
        [VerbOption("list",   HelpText = "List auth tokens")]
        public CommonOptions?          ListOptions   { get; set; }

        [VerbOption("add",    HelpText = "Add an auth token")]
        public AddAuthTokenOptions?    AddOptions    { get; set; }

        [VerbOption("remove", HelpText = "Delete an auth token")]
        public RemoveAuthTokenOptions? RemoveOptions { get; set; }

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

        [ExcludeFromCodeCoverage]
        public static IEnumerable<string> GetHelp(string verb)
        {
            // Add a usage prefix line
            yield return " ";
            if (string.IsNullOrEmpty(verb))
            {
                yield return $"ckan authtoken - {Properties.Resources.AuthTokenHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan authtoken <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "authtoken " + verb + " - " + GetDescription(typeof(AuthTokenSubOptions), verb);
                switch (verb)
                {
                    case "add":
                        yield return $"{Properties.Resources.Usage}: ckan authtoken {verb} [{Properties.Resources.Options}] host token";
                        break;
                    case "remove":
                        yield return $"{Properties.Resources.Usage}: ckan authtoken {verb} [{Properties.Resources.Options}] host";
                        break;
                    case "list":
                        yield return $"{Properties.Resources.Usage}: ckan authtoken {verb} [{Properties.Resources.Options}]";
                        break;
                }
            }
        }
    }

    internal class AddAuthTokenOptions : CommonOptions
    {
        [ValueOption(0)] public string? host  { get; set; }
        [ValueOption(1)] public string? token { get; set; }
    }

    internal class RemoveAuthTokenOptions : CommonOptions
    {
        [ValueOption(0)] public string? host { get; set; }
    }

}
