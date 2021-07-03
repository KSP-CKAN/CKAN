using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CommandLine;
using log4net;
using log4net.Core;

namespace CKAN.CmdLine
{
    /// <summary>
    /// Common options for all commands.
    /// </summary>
    internal class CommonOptions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CommonOptions));

        [Option('v', "verbose", HelpText = "Show more of what's going on when running")]
        public bool Verbose { get; set; }

        [Option('d', "debug", HelpText = "Show debugging level messages. Implies verbose")]
        public bool Debug { get; set; }

        [Option("debugger", HelpText = "Launch debugger at start")]
        public bool Debugger { get; set; }

        [Option("net-useragent", HelpText = "Set the default user-agent string for HTTP requests")]
        public string NetUserAgent { get; set; }

        [Option("headless", HelpText = "Set to disable all prompts")]
        public bool Headless { get; set; }

        [Option("asroot", HelpText = "Allows CKAN to run as root on Linux-based systems")]
        public bool AsRoot { get; set; }

        /// <summary>
        /// Handle the common options.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        /// <returns>An <see cref="CKAN.Exit"/> code.</returns>
        public virtual int Handle(GameInstanceManager manager, IUser user)
        {
            CheckMonoVersion(user, 3, 1, 0);

            // Processes in Docker containers normally run as root.
            // If we are running in a Docker container, do not require --asroot.
            // Docker creates a .dockerenv file in the root of each container.
            if ((Platform.IsUnix || Platform.IsMac) && GetUid() == 0 && !File.Exists("/.dockerenv"))
            {
                if (!AsRoot)
                {
                    user.RaiseError("You are trying to run CKAN as root.\r\nThis is a bad idea and there is absolutely no good reason to do it. Please run CKAN from an user account (or use --asroot if you are feeling brave).");
                    return Exit.Error;
                }

                user.RaiseMessage("Warning: Running CKAN as root!");
            }

            if (Debug)
            {
                LogManager.GetRepository().Threshold = Level.Debug;
                Log.Info("Debug logging enabled");
            }
            else if (Verbose)
            {
                LogManager.GetRepository().Threshold = Level.Info;
                Log.Info("Verbose logging enabled");
            }

            // Assign user-agent string if user has given us one
            if (!string.IsNullOrWhiteSpace(NetUserAgent))
            {
                Net.UserAgentString = NetUserAgent;
            }

            return Exit.Ok;
        }

        private static void CheckMonoVersion(IUser user, int recMajor, int recMinor, int recPatch)
        {
            try
            {
                var type = Type.GetType("Mono.Runtime");
                if (type == null)
                    return;

                var displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                {
                    var versionString = (string)displayName.Invoke(null, null);
                    var match = Regex.Match(versionString, @"^\D*(?<major>[\d]+)\.(?<minor>\d+)\.(?<revision>\d+).*$");

                    if (match.Success)
                    {
                        var major = int.Parse(match.Groups["major"].Value);
                        var minor = int.Parse(match.Groups["minor"].Value);
                        var patch = int.Parse(match.Groups["revision"].Value);

                        if (major < recMajor || major == recMajor && minor < recMinor)
                        {
                            user.RaiseMessage(
                                "Warning. Detected Mono version {0}, which is less than the recommended version of {1}.",
                                string.Join(".", major, minor, patch),
                                string.Join(".", recMajor, recMinor, recPatch)
                            );
                            user.RaiseMessage("Please update Mono via https://www.mono-project.com/download/stable/");
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignored. This may be fragile and is just a warning method
            }
        }

        private static uint GetUid()
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

    /// <summary>
    /// Instance specific options for commands dealing with mods.
    /// </summary>
    internal class InstanceSpecificOptions : CommonOptions
    {
        // Different 'SetName' properties make the options mutually exclusive
        [Option("ksp", HelpText = "KSP install to use", SetName = "ksp")]
        public string KSP { get; set; }

        [Option("kspdir", HelpText = "KSP directory to use", SetName = "dir")]
        public string KSPdir { get; set; }

        /// <summary>
        /// Handle the instance specific options. This also handles the common options.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        /// <returns>An <see cref="CKAN.Exit"/> code.</returns>
        public override int Handle(GameInstanceManager manager, IUser user)
        {
            var exitCode = base.Handle(manager, user);
            if (exitCode != Exit.Ok)
                return exitCode;

            try
            {
                if (!string.IsNullOrWhiteSpace(KSP))
                {
                    // Set a KSP directory by its alias.
                    manager.SetCurrentInstance(KSP);
                }
                else if (!string.IsNullOrWhiteSpace(KSPdir))
                {
                    // Set a KSP directory by its path
                    manager.SetCurrentInstanceByPath(KSPdir);
                }
            }
            catch (NotKSPDirKraken kraken)
            {
                user.RaiseMessage("Sorry, \"{0}\" does not appear to be a KSP directory.", kraken.path);
                return Exit.BadOpt;
            }
            catch (InvalidKSPInstanceKraken kraken)
            {
                user.RaiseMessage("Invalid KSP installation specified \"{0}\", use '--kspdir' to specify by path, or use 'ckan ksp list' to see known KSP installations.", kraken.instance);
                return Exit.BadOpt;
            }

            return exitCode;
        }
    }

    [Verb("consoleui", HelpText = "Start the CKAN console UI")]
    internal class ConsoleUiOptions : InstanceSpecificOptions
    {
        [Option("theme", HelpText = "Name of color scheme to use, falls back to environment variable CKAN_CONSOLEUI_THEME")]
        public string Theme { get; set; }
    }

    [Verb("gui", HelpText = "Start the CKAN GUI")]
    internal class GuiOptions : InstanceSpecificOptions
    {
        [Option("show-console", HelpText = "Shows the console while running the GUI")]
        public bool ShowConsole { get; set; }
    }

    [Verb("scan", HelpText = "Scan for manually installed KSP mods")]
    internal class ScanOptions : InstanceSpecificOptions { }
}
