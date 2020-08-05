using System;
using System.Collections.Generic;
using Autofac;
using CKAN.Configuration;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing authentication tokens.
    /// </summary>
    public class AuthToken : ISubCommand
    {
        private GameInstanceManager _manager;
        private IUser _user;

        /// <summary>
        /// Run the 'authtoken' command.
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
                case "AddAuthToken":
                    exitCode = AddAuthToken(args);
                    break;
                case "ListAuthToken":
                    exitCode = ListAuthTokens();
                    break;
                case "RemoveAuthToken":
                    exitCode = RemoveAuthToken(args);
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
                    return $"{prefix} {args[0]} {args[1]} [options] <host> <token>";
                case "list":
                    return $"{prefix} {args[0]} {args[1]} [options]";
                case "remove":
                    return $"{prefix} {args[0]} {args[1]} [options] <host>";
                default:
                    return $"{prefix} {args[0]} <command> [options]";
            }
        }

        private int AddAuthToken(object args)
        {
            var opts = (AuthTokenOptions.AddAuthToken)args;
            if (opts.Host == null || opts.Token == null)
            {
                _user.RaiseMessage("add <host> <token> - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            if (Uri.CheckHostName(opts.Host) != UriHostNameType.Unknown)
            {
                ServiceLocator.Container.Resolve<IConfiguration>().SetAuthToken(opts.Host, opts.Token);
                _user.RaiseMessage("Successfully added \"{0}\".", opts.Host);
            }
            else
            {
                _user.RaiseMessage("Invalid host name.");
                return Exit.BadOpt;
            }

            return Exit.Ok;
        }

        private int ListAuthTokens()
        {
            const string hostHeader = "Host";
            const string tokenHeader = "Token";

            var hosts = new List<string>(ServiceLocator.Container.Resolve<IConfiguration>().GetAuthTokenHosts());
            if (hosts.Count > 0)
            {
                var hostWidth = hostHeader.Length;
                var tokenWidth = tokenHeader.Length;
                foreach (var host in hosts)
                {
                    hostWidth = Math.Max(hostWidth, host.Length);
                    if (ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(host, out string token) && token != null)
                    {
                        tokenWidth = Math.Max(tokenWidth, token.Length);
                    }
                }

                _user.RaiseMessage("{0}  {1}",
                    hostHeader.PadRight(hostWidth),
                    tokenHeader.PadRight(tokenWidth)
                );

                _user.RaiseMessage("{0}  {1}",
                    new string('-', hostWidth),
                    new string('-', tokenWidth)
                );

                foreach (var host in hosts)
                {
                    if (ServiceLocator.Container.Resolve<IConfiguration>().TryGetAuthToken(host, out string token))
                    {
                        _user.RaiseMessage("{0}  {1}",
                            host.PadRight(hostWidth),
                            token.PadRight(tokenWidth)
                        );
                    }
                }
            }

            return Exit.Ok;
        }

        private int RemoveAuthToken(object args)
        {
            var opts = (AuthTokenOptions.RemoveAuthToken)args;
            if (opts.Host == null)
            {
                _user.RaiseMessage("remove <host> - argument missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            var hosts = new List<string>(ServiceLocator.Container.Resolve<IConfiguration>().GetAuthTokenHosts());
            if (hosts.Contains(opts.Host))
            {
                ServiceLocator.Container.Resolve<IConfiguration>().SetAuthToken(opts.Host, null);
                _user.RaiseMessage("Successfully removed \"{0}\".", opts.Host);
            }
            else
            {
                _user.RaiseMessage("There is no host with the name \"{0}\".", opts.Host);
                _user.RaiseMessage("Use 'ckan authtoken list' to view a list of hosts.");
                return Exit.BadOpt;
            }

            return Exit.Ok;
        }
    }

    [Verb("authtoken", HelpText = "Manage authentication tokens")]
    [ChildVerbs(typeof(AddAuthToken), typeof(ListAuthToken), typeof(RemoveAuthToken))]
    internal class AuthTokenOptions
    {
        [VerbExclude]
        [Verb("add", HelpText = "Add an authentication token")]
        internal class AddAuthToken : CommonOptions
        {
            [Value(0, MetaName = "Host", HelpText = "The host (DNS / IP) to authenticate with")]
            public string Host { get; set; }

            [Value(1, MetaName = "Token", HelpText = "The token to authenticate with")]
            public string Token { get; set; }
        }

        [VerbExclude]
        [Verb("list", HelpText = "List authentication tokens")]
        internal class ListAuthToken : CommonOptions { }

        [VerbExclude]
        [Verb("remove", HelpText = "Remove an authentication token")]
        internal class RemoveAuthToken : CommonOptions
        {
            [Value(0, MetaName = "Host", HelpText = "The host (DNS / IP) to remove")]
            public string Host { get; set; }
        }
    }
}
