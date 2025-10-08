// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

using System;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
#if WINDOWS && NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Autofac;
using log4net;
using log4net.Core;

using CKAN.Configuration;
using CKAN.Versioning;
using CKAN.Games;

namespace CKAN.CmdLine
{
    internal abstract class MainClass
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MainClass));

        /*
         * When the STAThread is applied, it changes the apartment state of the current thread to be single threaded.
         * Without getting into a huge discussion about COM and threading,
         * this attribute ensures the communication mechanism between the current thread an
         * other threads that may want to talk to it via COM.  When you're using Windows Forms,
         * depending on the feature you're using, it may be using COM interop in order to communicate with
         * operating system components.  Good examples of this are the Clipboard and the File Dialogs.
         */
        [STAThread]
        [ExcludeFromCodeCoverage]
        public static int Main(string[] args)
        {
            // Launch debugger if the "--debugger" flag is present in the command line arguments.
            // We want to do this as early as possible so just check the flag manually, rather than doing the
            // more robust argument parsing.
            if (args.Any(i => i == "--debugger"))
            {
                Debugger.Launch();
            }

            // Default to GUI if there are no command line args or if the only args are flags rather than commands.
            if (args.All(a => a is "--verbose"
                                or "--debug"
                                or "--asroot"
                                or "--show-console"))
            {
                var guiCommand = args.ToList();
                guiCommand.Insert(0, "gui");
                args = guiCommand.ToArray();
            }

            Logging.Initialize();
            // We need to load the game instance manager before parsing the args,
            // which is too late for debug flags if we want them active during the instance loading
            if (args.Contains("--debug"))
            {
                LogManager.GetRepository().Threshold = Level.Debug;
                log.Info("Debug logging enabled");
            }
            else if (args.Contains("--verbose"))
            {
                LogManager.GetRepository().Threshold = Level.Info;
                log.Info("Verbose logging enabled");
            }
            log.Info("CKAN started.");

            // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
            // This is on by default in .NET 4.6, but not in 4.5.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12
                                                  | SecurityProtocolType.Tls13;

            try
            {
                var user = new ConsoleUser(args.Contains("--headless"));
                return Execute(new GameInstanceManager(user, ServiceLocator.Container.Resolve<IConfiguration>()),
                               null, args, user);
            }
            finally
            {
                RegistryManager.DisposeAll();
            }
        }

        [ExcludeFromCodeCoverage]
        public static int Execute(GameInstanceManager manager,
                                  CommonOptions?      opts,
                                  string[]            args,
                                  IUser               user)
        {
            var repoData = ServiceLocator.Container.Resolve<RepositoryDataManager>();
            // We shouldn't instantiate Options if it's a subcommand.
            // It breaks command-specific help, for starters.
            try
            {
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "repair":
                            return (new Repair(manager, repoData, user)).RunSubCommand(opts, new SubCommandOptions(args));

                        case "instance":
                            return (new GameInstance(manager, user)).RunSubCommand(opts, new SubCommandOptions(args));

                        case "compat":
                            return (new Compat(manager, user)).RunSubCommand(opts, new SubCommandOptions(args));

                        case "repo":
                            return (new Repo(manager, repoData, user)).RunSubCommand(opts, new SubCommandOptions(args));

                        case "authtoken":
                            return (new AuthToken(manager, user)).RunSubCommand(opts, new SubCommandOptions(args));

                        case "cache":
                            return (new Cache(manager, user)).RunSubCommand(opts, new SubCommandOptions(args));

                        case "mark":
                            return (new Mark(manager, repoData, user)).RunSubCommand(opts, new SubCommandOptions(args));

                        case "filter":
                            return (new Filter(manager, user)).RunSubCommand(opts, new SubCommandOptions(args));

                        case "stability":
                            return (new Stability(manager, repoData, user)).RunSubCommand(opts, new SubCommandOptions(args));
                    }
                }
            }
            catch (NoGameInstanceKraken)
            {
                log.Info("CKAN exiting.");
                return printMissingInstanceError(user);
            }

            Options cmdline;
            try
            {
                cmdline = new Options(args);
            }
            catch (BadCommandKraken)
            {
                log.Info("CKAN exiting.");
                return AfterHelp(user);
            }

            // Process commandline options.
            CommonOptions options = (CommonOptions)cmdline.options;
            options.Merge(opts);
            if (manager == null)
            {
                manager = new GameInstanceManager(user, ServiceLocator.Container.Resolve<IConfiguration>());
            }
            else
            {
                manager.User = user;
            }

            try
            {
                int exitCode = options.Handle(manager, user);
                if (exitCode != Exit.OK)
                {
                    return exitCode;
                }
                // Don't bother with instances or registries yet because some commands don't need them.
                return RunSimpleAction(cmdline, options, args, user, manager);
            }
            finally
            {
                log.Info("CKAN exiting.");
            }
        }

        [ExcludeFromCodeCoverage]
        public static int AfterHelp(IUser user)
        {
            // Our help screen will already be shown. Let's add some extra data.
            user.RaiseMessage(Properties.Resources.MainVersion,
                              Meta.GetVersion(VersionFormat.Full));
            return Exit.BADOPT;
        }

        public static CKAN.GameInstance GetGameInstance(GameInstanceManager? manager)
        {
            var inst = manager?.CurrentInstance
                              ?? manager?.GetPreferredInstance();
            #pragma warning disable IDE0270
            if (inst == null)
            {
                throw new NoGameInstanceKraken();
            }
            #pragma warning restore IDE0270
            return inst;
        }

        /// <summary>
        /// Run whatever action the user has provided
        /// </summary>
        /// <returns>The exit status that should be returned to the system.</returns>
        [ExcludeFromCodeCoverage]
        private static int RunSimpleAction(Options             cmdline,
                                           CommonOptions       options,
                                           string[]            args,
                                           IUser               user,
                                           GameInstanceManager manager)
        {
            var repoData = ServiceLocator.Container.Resolve<RepositoryDataManager>();
            try
            {
                return options switch
                {
                    #if NETFRAMEWORK || WINDOWS
                        GuiOptions opts =>
                        #if NET6_0_OR_GREATER
                            Platform.IsWindows ?
                        #endif
                            Gui(manager, opts, args)
                        #if NET6_0_OR_GREATER
                            : Exit.ERROR
                        #endif
                        ,
                    #endif
                    ConsoleUIOptions   opts => ConsoleUi(manager, opts),
                    PromptOptions      opts => new Prompt(manager, repoData, user).RunCommand(opts),
                    VersionOptions     opts => Version(user),
                    UpdateOptions      opts => new Update(repoData, user, manager)
                                                   .RunCommand(opts,
                                                               opts.game == null
                                                                   ? KnownGames.knownGames.First()
                                                                   : KnownGames.GameByShortName(opts.game)),
                    AvailableOptions   opts => new Available(repoData, user)
                                                   .RunCommand(GetGameInstance(manager), opts),
                    InstallOptions     opts => new Install(manager, repoData, user)
                                                   .RunCommand(GetGameInstance(manager), opts),
                    ScanOptions        opts => Scan(GetGameInstance(manager), repoData),
                    ListOptions        opts => new List(repoData, user, Console.OpenStandardOutput())
                                                   .RunCommand(GetGameInstance(manager), opts),
                    ShowOptions        opts => new Show(repoData, user)
                                                   .RunCommand(GetGameInstance(manager), opts),
                    RepoListOptions    opts => new Replace(manager, repoData, user)
                                                   .RunCommand(GetGameInstance(manager), opts),
                    UpgradeOptions     opts => new Upgrade(manager, repoData, user)
                                                   .RunCommand(GetGameInstance(manager), opts),
                    SearchOptions      opts => new Search(repoData, user)
                                                   .RunCommand(GetGameInstance(manager), opts),
                    RemoveOptions      opts => new Remove(manager, repoData, user)
                                                   .RunCommand(GetGameInstance(manager), opts),
                    ImportOptions      opts => new Import(manager, repoData, user)
                                                   .RunCommand(GetGameInstance(manager), opts),
                    CleanOptions       opts => Clean(manager.Cache),
                    DeduplicateOptions opts => new Deduplicate(manager, repoData, user).RunCommand(),
                    CompareOptions     opts => new Compare(user).RunCommand(opts),
                    _                       => UnknownCommand(user),
                };
            }
            catch (NoGameInstanceKraken)
            {
                return printMissingInstanceError(user);
            }
            finally
            {
                RegistryManager.DisposeAll();
            }
        }

        [ExcludeFromCodeCoverage]
        private static int UnknownCommand(IUser user)
        {
            user.RaiseMessage(Properties.Resources.MainUnknownCommand);
            return Exit.BADOPT;
        }

        [ExcludeFromCodeCoverage]
        private static int printMissingInstanceError(IUser user)
        {
            user.RaiseMessage(Properties.Resources.MainMissingInstance);
            return Exit.ERROR;
        }

        #if NETFRAMEWORK || WINDOWS
        #if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
        #endif
        [ExcludeFromCodeCoverage]
        private static int Gui(GameInstanceManager manager, GuiOptions options, string[] args)
        {
            // TODO: Sometimes when the GUI exits, we get a System.ArgumentException,
            // but trying to catch it here doesn't seem to help. Dunno why.

            // GUI expects its first param to be an identifier, don't confuse it
            GUI.GUI.Main_(args.Except(new string[] {"--verbose", "--debug", "--show-console", "--asroot"})
                              .ToArray(),
                          options.NetUserAgent, manager, options.ShowConsole);

            return Exit.OK;
        }
        #endif

        [ExcludeFromCodeCoverage]
        private static int ConsoleUi(GameInstanceManager manager, ConsoleUIOptions opts)
        {
            // Debug/verbose output just messes up the screen
            LogManager.GetRepository().Threshold = Level.Warn;
            return ConsoleUI.ConsoleUI.Main_(manager,
                opts.Theme ?? Environment.GetEnvironmentVariable("CKAN_CONSOLEUI_THEME") ?? "default",
                opts.NetUserAgent, opts.Debug);
        }

        private static int Version(IUser user)
        {
            user.RaiseMessage("{0}", Meta.GetVersion(VersionFormat.Full));

            return Exit.OK;
        }

        /// <summary>
        /// Scans the game instance. Detects installed mods to mark as auto-detected and checks the consistency
        /// </summary>
        /// <param name="inst">The instance to scan</param>
        /// <param name="user"></param>
        /// <param name="next_command">Changes the output message if set.</param>
        /// <returns>Exit.OK if instance is consistent, Exit.ERROR otherwise </returns>
        private static int Scan(CKAN.GameInstance     inst,
                                RepositoryDataManager repoData)
        {
            RegistryManager.Instance(inst, repoData).ScanUnmanagedFiles();
            return Exit.OK;
        }

        private static int Clean(NetModuleCache? cache)
        {
            cache?.RemoveAll();
            return Exit.OK;
        }
    }

    [ExcludeFromCodeCoverage]
    public class NoGameInstanceKraken : Kraken
    {
        public NoGameInstanceKraken() { }
    }
}
