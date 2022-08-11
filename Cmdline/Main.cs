﻿// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

using System;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using log4net;
using log4net.Core;

namespace CKAN.CmdLine
{
    internal class MainClass
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
            if (args.All(a => a == "--verbose" || a == "--debug" || a == "--asroot" || a == "--show-console"))
            {
                var guiCommand = args.ToList();
                guiCommand.Insert(0, "gui");
                args = guiCommand.ToArray();
            }

            Logging.Initialize();
            log.Info("CKAN started.");

            // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
            // This is on by default in .NET 4.6, but not in 4.5.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            try
            {
                return Execute(null, null, args);
            }
            finally
            {
                RegistryManager.DisposeAll();
            }
        }

        public static int Execute(GameInstanceManager manager, CommonOptions opts, string[] args)
        {
            // We shouldn't instantiate Options if it's a subcommand.
            // It breaks command-specific help, for starters.
            try
            {
                switch (args[0])
                {
                    case "repair":
                        return (new Repair()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "instance":
                        return (new GameInstance()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "compat":
                        return (new Compat()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "repo":
                        return (new Repo()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "authtoken":
                        return (new AuthToken()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "cache":
                        return (new Cache()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "mark":
                        return (new Mark()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "filter":
                        return (new Filter()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                }
            }
            catch (NoGameInstanceKraken)
            {
                return printMissingInstanceError(new ConsoleUser(false));
            }
            finally
            {
                log.Info("CKAN exiting.");
            }

            Options cmdline;
            try
            {
                cmdline = new Options(args);
            }
            catch (BadCommandKraken)
            {
                return AfterHelp();
            }
            finally
            {
                log.Info("CKAN exiting.");
            }

            // Process commandline options.
            CommonOptions options = (CommonOptions)cmdline.options;
            options.Merge(opts);
            IUser user = new ConsoleUser(options.Headless);
            if (manager == null)
            {
                manager = new GameInstanceManager(user);
            }
            else
            {
                manager.User = user;
            }

            try
            {
                int exitCode = options.Handle(manager, user);
                if (exitCode != Exit.OK)
                    return exitCode;
                // Don't bother with instances or registries yet because some commands don't need them.
                return RunSimpleAction(cmdline, options, args, user, manager);
            }
            finally
            {
                log.Info("CKAN exiting.");
            }
        }

        public static int AfterHelp()
        {
            // Our help screen will already be shown. Let's add some extra data.
            new ConsoleUser(false).RaiseMessage(
                Properties.Resources.MainVersion, Meta.GetVersion(VersionFormat.Full));
            return Exit.BADOPT;
        }

        public static CKAN.GameInstance GetGameInstance(GameInstanceManager manager)
        {
            CKAN.GameInstance inst = manager.CurrentInstance
                ?? manager.GetPreferredInstance();
            if (inst == null)
            {
                throw new NoGameInstanceKraken();
            }
            return inst;
        }

        /// <summary>
        /// Run whatever action the user has provided
        /// </summary>
        /// <returns>The exit status that should be returned to the system.</returns>
        private static int RunSimpleAction(Options cmdline, CommonOptions options, string[] args, IUser user, GameInstanceManager manager)
        {
            try
            {
                switch (cmdline.action)
                {
                    case "gui":
                        return Gui(manager, (GuiOptions)options, args);

                    case "consoleui":
                        return ConsoleUi(manager, (ConsoleUIOptions)options, args);

                    case "prompt":
                        return new Prompt(manager).RunCommand(cmdline.options);

                    case "version":
                        return Version(user);

                    case "update":
                        return (new Update(manager, user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "available":
                        return (new Available(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "add":
                    case "install":
                        Scan(GetGameInstance(manager), user, cmdline.action);
                        return (new Install(manager, user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "scan":
                        return Scan(GetGameInstance(manager), user);

                    case "list":
                        return (new List(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "show":
                        return (new Show(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "replace":
                        Scan(GetGameInstance(manager), user, cmdline.action);
                        return (new Replace(manager, user)).RunCommand(GetGameInstance(manager), (ReplaceOptions)cmdline.options);

                    case "upgrade":
                        Scan(GetGameInstance(manager), user, cmdline.action);
                        return (new Upgrade(manager, user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "search":
                        return (new Search(user)).RunCommand(GetGameInstance(manager), options);

                    case "uninstall":
                    case "remove":
                        return (new Remove(manager, user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "import":
                        return (new Import(manager, user)).RunCommand(GetGameInstance(manager), options);

                    case "clean":
                        return Clean(manager.Cache);

                    case "compare":
                        return (new Compare(user)).RunCommand(cmdline.options);

                    default:
                        user.RaiseMessage(Properties.Resources.MainUnknownCommand);
                        return Exit.BADOPT;
                }
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

        internal static CkanModule LoadCkanFromFile(CKAN.GameInstance current_instance, string ckan_file)
        {
            CkanModule module = CkanModule.FromFile(ckan_file);

            // We'll need to make some registry changes to do this.
            RegistryManager registry_manager = RegistryManager.Instance(current_instance);

            // Remove this version of the module in the registry, if it exists.
            registry_manager.registry.RemoveAvailable(module);

            // Sneakily add our version in...
            registry_manager.registry.AddAvailable(module);

            return module;
        }

        private static int printMissingInstanceError(IUser user)
        {
            user.RaiseMessage(Properties.Resources.MainMissingInstance);
            return Exit.ERROR;
        }

        private static int Gui(GameInstanceManager manager, GuiOptions options, string[] args)
        {
            // TODO: Sometimes when the GUI exits, we get a System.ArgumentException,
            // but trying to catch it here doesn't seem to help. Dunno why.

            GUI.GUI.Main_(args, manager, options.ShowConsole);

            return Exit.OK;
        }

        private static int ConsoleUi(GameInstanceManager manager, ConsoleUIOptions opts, string[] args)
        {
            // Debug/verbose output just messes up the screen
            LogManager.GetRepository().Threshold = Level.Warn;
            return CKAN.ConsoleUI.ConsoleUI.Main_(args, manager,
                opts.Theme ?? Environment.GetEnvironmentVariable("CKAN_CONSOLEUI_THEME") ?? "default",
                opts.Debug);
        }

        private static int Version(IUser user)
        {
            user.RaiseMessage(Meta.GetVersion(VersionFormat.Full));

            return Exit.OK;
        }

        /// <summary>
        /// Scans the game instance. Detects installed mods to mark as auto-detected and checks the consistency
        /// </summary>
        /// <param name="inst">The instance to scan</param>
        /// <param name="user"></param>
        /// <param name="next_command">Changes the output message if set.</param>
        /// <returns>Exit.OK if instance is consistent, Exit.ERROR otherwise </returns>
        private static int Scan(CKAN.GameInstance inst, IUser user, string next_command = null)
        {
            try
            {
                inst.Scan();
                return Exit.OK;
            }
            catch (InconsistentKraken kraken)
            {

                if (next_command == null)
                {
                    user.RaiseError(kraken.InconsistenciesPretty);
                    user.RaiseError(Properties.Resources.ScanNotSaved);
                }
                else
                {
                    user.RaiseMessage(Properties.Resources.ScanPreliminaryInconsistent, next_command);
                }

                return Exit.ERROR;
            }
        }

        private static int Clean(NetModuleCache cache)
        {
            cache.RemoveAll();
            return Exit.OK;
        }
    }

    public class NoGameInstanceKraken : Kraken
    {
        public NoGameInstanceKraken() { }
    }

    public class CmdLineUtil
    {
        public static uint GetUID()
        {
            if (Platform.IsUnix || Platform.IsMac)
            {
                return getuid();
            }

            return 1;
        }

        [DllImport("libc")]
        private static extern uint getuid();
    }
}
