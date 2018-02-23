// Reference CKAN client
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
using CKAN.CmdLine.Action;
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

            if (args.Length == 1 && args.Any(i => i == "--verbose" || i == "--debug"))
            {
                // Start the gui with logging enabled #437
                var guiCommand = args.ToList();
                guiCommand.Insert(0, "gui");
                args = guiCommand.ToArray();
            }

            Logging.Initialize();
            log.Info("CKAN started.");

            // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
            // This is on by default in .NET 4.6, but not in 4.5.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // If we're starting with no options then invoke the GUI instead.
            if (args.Length == 0)
            {
                return Gui(new GuiOptions(), args);
            }

            try
            {
                return Execute(null, null, args);
            }
            finally
            {
                RegistryManager.DisposeAll();
            }
        }

        public static int Execute(KSPManager manager, CommonOptions opts, string[] args)
        {
            // We shouldn't instantiate Options if it's a subcommand.
            // It breaks command-specific help, for starters.
            try
            {
                switch (args[0])
                {
                    case "repair":
                        return (new Repair()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "ksp":
                        return (new KSP()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "compat":
                        return (new Compat()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "repo":
                        return (new Repo()).RunSubCommand(manager, opts, new SubCommandOptions(args));

                    case "authtoken":
                        return (new AuthToken()).RunSubCommand(manager, opts, new SubCommandOptions(args));
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
                manager = new KSPManager(user);
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
            new ConsoleUser(false).RaiseMessage("You are using CKAN version {0}", Meta.GetVersion(VersionFormat.Full));
            return Exit.BADOPT;
        }

        public static CKAN.KSP GetGameInstance(KSPManager manager)
        {
            CKAN.KSP ksp = manager.CurrentInstance
                ?? manager.GetPreferredInstance();
            if (ksp == null)
            {
                throw new NoGameInstanceKraken();
            }
            return ksp;
        }

        /// <summary>
        /// Run whatever action the user has provided
        /// </summary>
        /// <returns>The exit status that should be returned to the system.</returns>
        private static int RunSimpleAction(Options cmdline, CommonOptions options, string[] args, IUser user, KSPManager manager)
        {
            try
            {
                switch (cmdline.action)
                {
                    case "gui":
                        return Gui((GuiOptions)options, args);

                    case "consoleui":
                        return ConsoleUi(options, args);

                    case "prompt":
                        return new Prompt().RunCommand(manager, cmdline.options);

                    case "version":
                        return Version(user);

                    case "update":
                        return (new Update(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "available":
                        return (new Available(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "add":
                    case "install":
                        Scan(GetGameInstance(manager), user, cmdline.action);
                        return (new Install(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "scan":
                        return Scan(GetGameInstance(manager), user);

                    case "list":
                        return (new List(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "show":
                        return (new Show(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "search":
                        return (new Search(user)).RunCommand(GetGameInstance(manager), options);

                    case "uninstall":
                    case "remove":
                        return (new Remove(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "upgrade":
                        Scan(GetGameInstance(manager), user, cmdline.action);
                        return (new Upgrade(user)).RunCommand(GetGameInstance(manager), cmdline.options);

                    case "import":
                        return (new Import(user)).RunCommand(GetGameInstance(manager), options);

                    case "clean":
                        return Clean(GetGameInstance(manager));

                    case "compare":
                        return (new Compare(user)).RunCommand(cmdline.options);

                    default:
                        user.RaiseMessage("Unknown command, try --help");
                        return Exit.BADOPT;
                }
            }
            catch (NoGameInstanceKraken)
            {
                return printMissingInstanceError(user);
            }
        }

        private static int printMissingInstanceError(IUser user)
        {
            user.RaiseMessage("I don't know where KSP is installed.");
            user.RaiseMessage("Use 'ckan ksp help' for assistance in setting this.");
            return Exit.ERROR;
        }

        private static int Gui(GuiOptions options, string[] args)
        {
            // TODO: Sometimes when the GUI exits, we get a System.ArgumentException,
            // but trying to catch it here doesn't seem to help. Dunno why.

            GUI.Main_(args, options.ShowConsole);

            return Exit.OK;
        }

        private static int ConsoleUi(CommonOptions opts, string[] args)
        {
            // Debug/verbose output just messes up the screen
            LogManager.GetRepository().Threshold = Level.Warn;
            return CKAN.ConsoleUI.ConsoleUI.Main_(args, opts.Debug);
        }

        private static int Version(IUser user)
        {
            user.RaiseMessage(Meta.GetVersion(VersionFormat.Full));

            return Exit.OK;
        }

        /// <summary>
        /// Scans the ksp instance. Detects installed mods to mark as auto-detected and checks the consistency
        /// </summary>
        /// <param name="ksp_instance">The instance to scan</param>
        /// <param name="user"></param>
        /// <param name="next_command">Changes the output message if set.</param>
        /// <returns>Exit.OK if instance is consistent, Exit.ERROR otherwise </returns>
        private static int Scan(CKAN.KSP ksp_instance, IUser user, string next_command = null)
        {
            try
            {
                ksp_instance.ScanGameData();
                return Exit.OK;
            }
            catch (InconsistentKraken kraken)
            {

                if (next_command == null)
                {
                    user.RaiseError(kraken.InconsistenciesPretty);
                    user.RaiseError("The repo has not been saved.");
                }
                else
                {
                    user.RaiseMessage("Preliminary scanning shows that the install is in a inconsistent state.");
                    user.RaiseMessage("Use ckan.exe scan for more details");
                    user.RaiseMessage("Proceeding with {0} in case it fixes it.\r\n", next_command);
                }

                return Exit.ERROR;
            }
        }

        private static int Clean(CKAN.KSP current_instance)
        {
            current_instance.CleanCache();
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
