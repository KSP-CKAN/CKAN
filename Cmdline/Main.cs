// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

using System;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CKAN.CmdLine.Action;
using CommandLine;
using CommandLine.Text;
using log4net;
using log4net.Core;

namespace CKAN.CmdLine
{
    public class MainClass
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainClass));

        private static GameInstanceManager _manager;
        private static IUser _user;

        /*
         * When the STAThread is applied, it changes the apartment state of the current thread to be single threaded.
         * Without getting into a huge discussion about COM and threading,
         * this attribute ensures the communication mechanism between the current thread and
         * other threads that may want to talk to it via COM.  When you're using Windows Forms,
         * depending on the feature you're using, it may be using COM interop in order to communicate with
         * operating system components.  Good examples of this are the Clipboard and the File Dialogs.
         */
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                // Launch debugger if the "--debugger" flag is present in the command line arguments.
                // We want to do this as early as possible so just check the flag manually, rather than doing the
                // more robust argument parsing.
                if (args.Any(i => i == "--debugger"))
                {
                    Debugger.Launch();
                }

                // Default to GUI if there are no command line args or if the only args are flags rather than commands.
                if (args.All(a => a == "-v" || a == "--verbose" || a == "-d" || a == "--debug" || a == "--asroot" || a == "--show-console" || a == "--debugger"))
                {
                    var guiCommand = args.ToList();
                    guiCommand.Insert(0, "gui");
                    args = guiCommand.ToArray();
                }

                var types = LoadVerbs();
                var parser = new Parser(c => c.HelpWriter = null).ParseVerbs(args, types);
                var result = parser.MapResult(opts => Execute(_manager, opts, args), errs =>
                {
                    if (errs.IsVersion())
                    {
                        Console.WriteLine(Meta.GetVersion(VersionFormat.Full));
                    }
                    else
                    {
                        var ht = HelpText.AutoBuild(parser, h =>
                        {
                            h.AddDashesToOption = true;                                // Add dashes to options
                            h.AddNewLineBetweenHelpSections = true;                    // Add blank line between heading and usage
                            h.AutoHelp = false;                                        // Hide built-in help option
                            h.AutoVersion = false;                                     // Hide built-in version option
                            h.Heading = $"CKAN {Meta.GetVersion(VersionFormat.Full)}"; // Create custom heading
                            h.Copyright = $"Copyright © 2014-{DateTime.Now.Year}";     // Create custom copyright
                            h.AddPreOptionsLine(GetUsage(args));                       // Show usage
                            return HelpText.DefaultParsingErrorsHandler(parser, h);
                        }, e => e, true);

                        Console.WriteLine(ht);
                    }

                    return Exit.Ok;
                });

                return result;
            }
            finally
            {
                RegistryManager.DisposeAll();
            }
        }

        /// <summary>
        /// This is purely made for the tests to be able to pass over a <see cref="CKAN.GameInstanceManager"/>.
        /// ONLY FOR INTERNAL USE !!!
        /// </summary>
        /// <param name="args">The command line arguments handled by the parser.</param>
        /// <param name="manager">The dummy manager to provide dummy game instances.</param>
        /// <returns>An <see cref="CKAN.Exit"/> code.</returns>
        public static int MainForTests(string[] args, GameInstanceManager manager = null)
        {
            _manager = manager;
            return Main(args);
        }

        private static Type[] LoadVerbs()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null)
                .Except(Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.GetCustomAttribute<VerbExclude>() != null)
                    .ToArray())
                .ToArray();
        }

        /// <summary>
        /// Executes the provided command.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="args">The command line arguments handled by the parser.</param>
        /// <returns>An <see cref="CKAN.Exit"/> code.</returns>
        public static int Execute(GameInstanceManager manager, object args, string[] argStrings)
        {
            var s = args.ToString();
            var opts = s.Replace(s.Substring(0, s.LastIndexOf('.') + 1), "").Split('+');

            try
            {
                Logging.Initialize();
                Log.Info("CKAN started.");

                // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
                // This is on by default in .NET 4.6, but not in 4.5.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                switch (opts[0])
                {
                    case "AuthTokenOptions":
                        return new AuthToken().RunCommand(manager, args);
                    case "CacheOptions":
                        return new Cache().RunCommand(manager, args);
                    case "CompatOptions":
                        return new Compat().RunCommand(manager, args);
                    case "KspOptions":
                        return new Action.GameInstance().RunCommand(manager, args);
                    case "MarkOptions":
                        return new Mark().RunCommand(manager, args);
                    case "RepairOptions":
                        return new Repair().RunCommand(manager, args);
                    case "RepoOptions":
                        return new Action.Repo().RunCommand(manager, args);
                }
            }
            catch (NoGameInstanceKraken)
            {
                return PrintMissingInstanceError(new ConsoleUser(false));
            }
            finally
            {
                Log.Info("CKAN exiting.");
            }

            CommonOptions options = new CommonOptions();
            _user = new ConsoleUser(options.Headless);
            if (manager == null)
            {
                manager = new GameInstanceManager(_user);
            }
            else
            {
                manager.User = _user;
            }

            try
            {
                var exitCode = options.Handle(manager, _user);
                if (exitCode != Exit.Ok)
                    return exitCode;

                var instance = GetGameInstance(manager);
                switch (opts[0])
                {
                    case "AvailableOptions":
                        return new Available(_user).RunCommand(instance, args);
                    case "CompareOptions":
                        return new Compare(_user).RunCommand(instance, args);
                    case "ConsoleUiOptions":
                        return ConsoleUi(manager, args);
                    case "GuiOptions":
                        return Gui(manager, args, argStrings);
                    case "ImportOptions":
                        return new Import(manager, _user).RunCommand(instance, args);
                    case "InstallOptions":
                        Scan(instance, _user, "install");
                        return new Install(manager, _user).RunCommand(instance, args);
                    case "ListOptions":
                        return new List(_user).RunCommand(instance, args);
                    case "PromptOptions":
                        return new Prompt().RunCommand(manager, args, argStrings);
                    case "RemoveOptions":
                        return new Remove(manager, _user).RunCommand(instance, args);
                    case "ReplaceOptions":
                        Scan(instance, _user, "replace");
                        return new Replace(manager, _user).RunCommand(instance, args);
                    case "ScanOptions":
                        return Scan(instance, _user);
                    case "SearchOptions":
                        return new Search(_user).RunCommand(instance, args);
                    case "ShowOptions":
                        return new Show(_user).RunCommand(instance, args);
                    case "UpdateOptions":
                        return new Update(manager, _user).RunCommand(instance, args);
                    case "UpgradeOptions":
                        Scan(instance, _user, "upgrade");
                        return new Upgrade(manager, _user).RunCommand(instance, args);
                    default:
                        return Exit.BadOpt;
                }
            }
            finally
            {
                Log.Info("CKAN exiting.");
            }
        }

        private static string GetUsage(string[] args)
        {
            const string prefix = "USAGE:\r\n  ckan";
            switch (args[0])
            {
                case "authtoken":
                    return new AuthToken().GetUsage(prefix, args);
                case "cache":
                    return new Cache().GetUsage(prefix, args);
                case "compat":
                    return new Compat().GetUsage(prefix, args);
                case "ksp":
                    return new Action.GameInstance().GetUsage(prefix, args);
                case "mark":
                    return new Mark().GetUsage(prefix, args);
                case "repair":
                    return new Repair().GetUsage(prefix, args);
                case "repo":
                    return new Action.Repo().GetUsage(prefix, args);
                case "install":
                case "remove":
                case "replace":
                case "upgrade":
                    return $"{prefix} {args[0]} [options] <mod> [<mod2> ...]";
                case "show":
                    return $"{prefix} {args[0]} [options] <mod>";
                case "compare":
                    return $"{prefix} {args[0]} [options] <version1> <version2>";
                case "import":
                    return $"{prefix} {args[0]} [options] <path> [<path2> ...]";
                case "search":
                    return $"{prefix} {args[0]} [options] <string>";
                case "available":
                case "consoleui":
                case "gui":
                case "list":
                case "prompt":
                case "scan":
                case "update":
                    return $"{prefix} {args[0]} [options]";
                default:
                    return $"{prefix} <command> [options]";
            }
        }

        private static int PrintMissingInstanceError(IUser user)
        {
            user.RaiseMessage("I don't know where KSP is installed.");
            user.RaiseMessage("Use 'ckan ksp --help' for assistance in setting this.");
            return Exit.Error;
        }

        /// <summary>
        /// Gets the current, or preferred, game instance to manipulate.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <returns>The current <see cref="CKAN.GameInstance"/> instance.</returns>
        /// <exception cref="CKAN.NoGameInstanceKraken">Throws if no valid <see cref="CKAN.GameInstance"/> instance was found.</exception>
        public static GameInstance GetGameInstance(GameInstanceManager manager)
        {
            GameInstance instance = manager.CurrentInstance ?? manager.GetPreferredInstance();
            if (instance == null)
            {
                throw new NoGameInstanceKraken(null);
            }

            return instance;
        }

        private static int ConsoleUi(GameInstanceManager manager, object args)
        {
            // Debug/verbose output just messes up the screen
            LogManager.GetRepository().Threshold = Level.Warn;

            var opts = (ConsoleUiOptions)args;
            _user.RaiseMessage("Starting ConsoleUI, please wait...");
            return ConsoleUI.ConsoleUI.Main_(args.ToString().Split(), manager, opts.Theme ?? Environment.GetEnvironmentVariable("CKAN_CONSOLEUI_THEME") ?? "default", opts.Debug);
        }

        private static int Gui(GameInstanceManager manager, object args, string[] argStrings)
        {
            // TODO: Sometimes when the GUI exits, we get a System.ArgumentException,
            // but trying to catch it here doesn't seem to help. Dunno why.

            var opts = (GuiOptions)args;
            _user.RaiseMessage("Starting GUI, please wait...");
            GUI.Main_(argStrings, manager, opts.ShowConsole);
            return Exit.Ok;
        }

        private static int Scan(GameInstance instance, IUser user, string nextCommand = null)
        {
            try
            {
                instance.Scan();
                return Exit.Ok;
            }
            catch (InconsistentKraken kraken)
            {
                if (nextCommand == null)
                {
                    user.RaiseError(kraken.InconsistenciesPretty);
                    user.RaiseError("The repo has not been saved.");
                }
                else
                {
                    user.RaiseMessage("Preliminary scanning shows that the install is in an inconsistent state.");
                    user.RaiseMessage("Use 'ckan scan --help' for more details.");
                    user.RaiseMessage("Proceeding with {0} in case it fixes it.\r\n", nextCommand);
                }

                return Exit.Error;
            }
        }

        /// <summary>
        /// Loads a .ckan file from the given path, reads it and creates a <see cref="CKAN.CkanModule"/> from it.
        /// </summary>
        /// <param name="currentInstance">The current <see cref="CKAN.GameInstance"/> instance to modify the module.</param>
        /// <param name="ckanFile">The path to the .ckan file.</param>
        /// <returns>A <see cref="CKAN.CkanModule"/>.</returns>
        internal static CkanModule LoadCkanFromFile(GameInstance currentInstance, string ckanFile)
        {
            CkanModule module = CkanModule.FromFile(ckanFile);

            // We'll need to make some registry changes to do this
            RegistryManager registryManager = RegistryManager.Instance(currentInstance);

            // Remove this version of the module in the registry, if it exists
            registryManager.registry.RemoveAvailable(module);

            // Sneakily add our version in...
            registryManager.registry.AddAvailable(module);

            return module;
        }
    }
}
