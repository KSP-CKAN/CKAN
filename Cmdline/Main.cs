// Reference CKAN client
// Paul '@pjf' Fenwick
//
// License: CC-BY 4.0, LGPL, or MIT (your choice)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using log4net;
using log4net.Config;
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
                List<string> guiCommand = args.ToList();
                guiCommand.Insert(0, "gui");
                args = guiCommand.ToArray();
            }

            BasicConfigurator.Configure();
            LogManager.GetRepository().Threshold = Level.Warn;
            log.Debug("CKAN started");

            Options cmdline;

            // If we're starting with no options then invoke the GUI instead.
            if (args.Length == 0)
            {
                return Gui(new GuiOptions(), args);
            }

            IUser user;
            try
            {
                cmdline = new Options(args);
            }
            catch (BadCommandKraken)
            {
                // Our help screen will already be shown. Let's add some extra data.
                user = new ConsoleUser(false);
                user.RaiseMessage("You are using CKAN version {0}", Meta.Version());

                return Exit.BADOPT;
            }

            // Process commandline options.

            var options = (CommonOptions)cmdline.options;
            user = new ConsoleUser(options.Headless);
            CheckMonoVersion(user, 3, 1, 0);

            // Processes in Docker containers normally run as root.
            // If we are running in a Docker container, do not require --asroot.
            // Docker creates a .dockerenv file in the root of each container.
            if ((Platform.IsUnix || Platform.IsMac) && CmdLineUtil.GetUID() == 0 && !File.Exists("/.dockerenv"))
            {
                if (!options.AsRoot)
                {
                    user.RaiseError(@"You are trying to run CKAN as root.
This is a bad idea and there is absolutely no good reason to do it. Please run CKAN from a user account (or use --asroot if you are feeling brave).");
                    return Exit.ERROR;
                }
                else
                {
                    user.RaiseMessage("Warning: Running CKAN as root!");
                }
            }

            if (options.Debug)
            {
                LogManager.GetRepository().Threshold = Level.Debug;
                log.Info("Debug logging enabled");
            }
            else if (options.Verbose)
            {
                LogManager.GetRepository().Threshold = Level.Info;
                log.Info("Verbose logging enabled");
            }

            // Assign user-agent string if user has given us one
            if (options.NetUserAgent != null)
            {
                Net.UserAgentString = options.NetUserAgent;
            }

            // User provided KSP instance

            if (options.KSPdir != null && options.KSP != null)
            {
                user.RaiseMessage("--ksp and --kspdir can't be specified at the same time");
                return Exit.BADOPT;
            }

            KSPManager manager= new KSPManager(user);

            if (options.KSP != null)
            {
                // Set a KSP directory by its alias.

                try
                {
                    manager.SetCurrentInstance(options.KSP);
                }
                catch (InvalidKSPInstanceKraken)
                {
                    user.RaiseMessage("Invalid KSP installation specified \"{0}\", use '--kspdir' to specify by path, or 'list-installs' to see known KSP installations", options.KSP);
                    return Exit.BADOPT;
                }
            }
            else if (options.KSPdir != null)
            {
                // Set a KSP directory by its path
                manager.SetCurrentInstanceByPath(options.KSPdir);
            }
            else if (! (cmdline.action == "ksp" || cmdline.action == "version" || cmdline.action == "gui"))
            {
                // Find whatever our preferred instance is.
                // We don't do this on `ksp/version/gui` commands, they don't need it.
                CKAN.KSP ksp = manager.GetPreferredInstance();

                if (ksp == null)
                {
                    user.RaiseMessage("I don't know where KSP is installed.");
                    user.RaiseMessage("Use 'ckan ksp help' for assistance on setting this.");
                    return Exit.ERROR;
                }
                else
                {
                    log.InfoFormat("Using KSP install at {0}",ksp.GameDir());
                }
            }

            #region Aliases

            switch (cmdline.action)
            {
                case "add":
                    cmdline.action = "install";
                    break;

                case "uninstall":
                    cmdline.action = "remove";
                    break;
            }

            #endregion

            var return_code = Exit.OK;

            switch (cmdline.action)
            {
                case "gui":
                    return_code = Gui((GuiOptions)options, args);
                    break;

                case "version":
                    return_code = Version(user);
                    break;

                case "update":
                    return_code = (new Update(user)).RunCommand(manager.CurrentInstance, (UpdateOptions)cmdline.options);
                    break;

                case "available":
                    return_code = Available(manager.CurrentInstance, user);
                    break;

                case "install":
                    Scan(manager.CurrentInstance, user, cmdline.action);
                    return_code = (new Install(user)).RunCommand(manager.CurrentInstance, (InstallOptions)cmdline.options);
                    break;

                case "scan":
                    return_code = Scan(manager.CurrentInstance,user);
                    break;

                case "list":
                    return_code = (new List(user)).RunCommand(manager.CurrentInstance, (ListOptions)cmdline.options);
                    break;

                case "show":
                    return_code = (new Show(user)).RunCommand(manager.CurrentInstance, (ShowOptions)cmdline.options);
                    break;

                case "search":
                    return_code = (new Search(user)).RunCommand(manager.CurrentInstance, options);
                    break;

                case "remove":
                    return_code = (new Remove(user)).RunCommand(manager.CurrentInstance, cmdline.options);
                    break;

                case "upgrade":
                    Scan(manager.CurrentInstance, user, cmdline.action);
                    return_code = (new Upgrade(user)).RunCommand(manager.CurrentInstance, cmdline.options);
                    break;

                case "clean":
                    return_code = Clean(manager.CurrentInstance);
                    break;

                case "repair":
                    var repair = new Repair(manager.CurrentInstance,user);
                    return_code = repair.RunSubCommand((SubCommandOptions) cmdline.options);
                    break;

                case "ksp":
                    var ksp = new KSP(manager, user);
                    return_code = ksp.RunSubCommand((SubCommandOptions) cmdline.options);
                    break;

                case "repo":
                    var repo = new Repo (manager, user);
                    return_code = repo.RunSubCommand((SubCommandOptions) cmdline.options);
                    break;

                case "compare":
                    return_code = (new Compare(user)).RunCommand(manager.CurrentInstance, cmdline.options);
                    break;

                default:
                    user.RaiseMessage("Unknown command, try --help");
                    return_code = Exit.BADOPT;
                    break;
            }

            manager.CurrentInstance.RegistryManager.Dispose();

            return return_code;
        }

        private static void CheckMonoVersion(IUser user, int rec_major, int rec_minor, int rec_patch)
        {
            try
            {
                Type type = Type.GetType("Mono.Runtime");
                if (type == null) return;

                MethodInfo display_name = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (display_name != null)
                {
                    var version_string = (string) display_name.Invoke(null, null);
                    var match = Regex.Match(version_string, @"^\D*(?<major>[\d]+)\.(?<minor>\d+)\.(?<revision>\d+).*$");

                    if (match.Success)
                    {
                        int major = Int32.Parse(match.Groups["major"].Value);
                        int minor = Int32.Parse(match.Groups["minor"].Value);
                        int patch = Int32.Parse(match.Groups["revision"].Value);

                        if (major < rec_major || (major == rec_major && minor < rec_minor))
                        {
                            user.RaiseMessage(
                                "Warning. Detected mono runtime of {0} is less than the recommended version of {1}\r\n",
                                String.Join(".", major, minor, patch),
                                String.Join(".", rec_major, rec_minor, rec_patch)
                                );
                            user.RaiseMessage("Update recommend\r\n");
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignored. This may be fragile and is just a warning method
            }
        }

        private static int Gui(GuiOptions options, string[] args)
        {
            // TODO: Sometimes when the GUI exits, we get a System.ArgumentException,
            // but trying to catch it here doesn't seem to help. Dunno why.

            GUI.Main_(args, options.ShowConsole);

            return Exit.OK;
        }

        private static int Version(IUser user)
        {
            user.RaiseMessage(Meta.Version());

            return Exit.OK;
        }

        private static int Available(CKAN.KSP current_instance, IUser user)
        {
            List<CkanModule> available = RegistryManager.Instance(current_instance).registry.Available(current_instance.Version());

            user.RaiseMessage("Mods available for KSP {0}", current_instance.Version());
            user.RaiseMessage("");

            var width = user.WindowWidth;

            foreach (CkanModule module in available)
            {
                string entry = String.Format("* {0} ({1}) - {2}", module.identifier, module.version, module.name);
                user.RaiseMessage(width > 0 ? entry.PadRight(width).Substring(0, width - 1) : entry);
            }

            return Exit.OK;
        }

        /// <summary>
        /// Scans the ksp instance. Detects installed mods to mark as auto-detected and checks the consistency
        /// </summary>
        /// <param name="ksp_instance">The instance to scan</param>
        /// <param name="user"></param>
        /// <param name="next_command">Changes the output message if set.</param>
        /// <returns>Exit.OK if instance is consistent, Exit.ERROR otherwise </returns>
        private static int Scan(CKAN.KSP ksp_instance, IUser user, string next_command=null)
        {
            try
            {
                ksp_instance.ScanGameData();
                return Exit.OK;
            }
            catch (InconsistentKraken kraken)
            {

                if (next_command==null)
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
