using System;
using System.Linq;
using System.Collections.Generic;

using log4net;
using log4net.Core;
using CommandLine;
using CommandLine.Text;

namespace CKAN.CmdLine
{
    // Look, parsing options is so easy and beautiful I made
    // it into a special class for you to admire!

    public class Options
    {
        public string action  { get; set; }
        public object options { get; set; }

        /// <summary>
        /// Returns an options object on success. Prints a default help
        /// screen and throws a BadCommandKraken on failure.
        /// </summary>
        public Options(string[] args)
        {
            Parser.Default.ParseArgumentsStrict
            (
                args, new Actions(), (verb, suboptions) =>
                {
                    action  = verb;
                    options = suboptions;
                },
                delegate
                {
                    throw new BadCommandKraken();
                }
            );
        }
    }

    // Actions supported by our client go here.

    internal class Actions : VerbCommandOptions
    {
        #if NETFRAMEWORK || WINDOWS
        [VerbOption("gui", HelpText = "Start the CKAN GUI")]
        public GuiOptions GuiOptions { get; set; }
        #endif

        [VerbOption("consoleui", HelpText = "Start the CKAN console UI")]
        public ConsoleUIOptions ConsoleUIOptions { get; set; }

        [VerbOption("prompt", HelpText = "Run CKAN prompt for executing multiple commands in a row")]
        public CommonOptions PromptOptions { get; set; }

        [VerbOption("search", HelpText = "Search for mods")]
        public SearchOptions SearchOptions { get; set; }

        [VerbOption("upgrade", HelpText = "Upgrade an installed mod")]
        public UpgradeOptions Upgrade { get; set; }

        [VerbOption("update", HelpText = "Update list of available mods")]
        public UpdateOptions Update { get; set; }

        [VerbOption("available", HelpText = "List available mods")]
        public AvailableOptions Available { get; set; }

        [VerbOption("install", HelpText = "Install a mod")]
        public InstallOptions Install { get; set; }

        [VerbOption("remove", HelpText = "Remove an installed mod")]
        public RemoveOptions Remove { get; set; }

        [VerbOption("import", HelpText = "Import manually downloaded mods")]
        public ImportOptions Import { get; set; }

        [VerbOption("scan", HelpText = "Scan for manually installed mods")]
        public ScanOptions Scan { get; set; }

        [VerbOption("list", HelpText = "List installed modules")]
        public ListOptions List { get; set; }

        [VerbOption("show", HelpText = "Show information about a mod")]
        public ShowOptions Show { get; set; }

        [VerbOption("clean", HelpText = "Clean away downloaded files from the cache")]
        public CleanOptions Clean { get; set; }

        [VerbOption("repair", HelpText = "Attempt various automatic repairs")]
        public RepairSubOptions Repair { get; set; }

        [VerbOption("replace", HelpText = "Replace list of replaceable mods")]
        public ReplaceOptions Replace { get; set; }

        [VerbOption("repo", HelpText = "Manage CKAN repositories")]
        public RepoSubOptions Repo { get; set; }

        [VerbOption("mark", HelpText = "Edit flags on modules")]
        public MarkSubOptions Mark { get; set; }

        [VerbOption("instance", HelpText = "Manage game instances")]
        public InstanceSubOptions Instance { get; set; }

        [VerbOption("authtoken", HelpText = "Manage authentication tokens")]
        public AuthTokenSubOptions AuthToken { get; set; }

        [VerbOption("cache", HelpText = "Manage download cache path")]
        public CacheSubOptions Cache { get; set; }

        [VerbOption("compat", HelpText = "Manage game version compatibility")]
        public CompatSubOptions Compat { get; set; }

        [VerbOption("compare", HelpText = "Compare version strings")]
        public CompareOptions Compare { get; set; }

        [VerbOption("version", HelpText = "Show the version of the CKAN client being used")]
        public VersionOptions Version { get; set; }

        [VerbOption("filter", HelpText = "View or edit installation filters")]
        public FilterSubOptions Filter { get; set; }

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

        public static IEnumerable<string> GetHelp(string verb)
        {
            // Add a usage prefix line
            yield return " ";
            if (string.IsNullOrEmpty(verb))
            {
                yield return $"{Properties.Resources.Usage}: ckan <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                string descr = GetDescription(typeof(Action), verb);
                if (!string.IsNullOrEmpty(descr))
                {
                    yield return $"ckan {verb} - {descr}";
                }
                switch (verb)
                {
                    // Commands that don't need a header
                    case "help":
                        break;

                    // Commands that deal with mods
                    case "add":
                    case "install":
                    case "remove":
                    case "uninstall":
                    case "upgrade":
                    case "show":
                        yield return $"{Properties.Resources.Usage}: ckan {verb} [{Properties.Resources.Options}] module [module2 ...]";
                        break;

                    // Commands with other string arguments
                    case "search":
                        yield return $"{Properties.Resources.Usage}: ckan {verb} [{Properties.Resources.Options}] substring";
                        break;
                    case "compare":
                        yield return $"{Properties.Resources.Usage}: ckan {verb} [{Properties.Resources.Options}] version1 version2";
                        break;
                    case "import":
                        yield return $"{Properties.Resources.Usage}: ckan {verb} [{Properties.Resources.Options}] path [path2 ...]";
                        break;

                    // Commands with only --flag type options
                    case "gui":
                    case "available":
                    case "list":
                    case "update":
                    case "scan":
                    case "clean":
                    case "version":
                    default:
                        yield return $"{Properties.Resources.Usage}: ckan {verb} [{Properties.Resources.Options}]";
                        break;
                }
            }
        }

    }

    public abstract class VerbCommandOptions
    {
        protected string GetDescription(string verb)
            => GetDescription(GetType(), verb);

        protected static string GetDescription(Type t, string verb)
            => t.GetProperties()
                .Select(property => (BaseOptionAttribute)Attribute.GetCustomAttribute(
                    property, typeof(BaseOptionAttribute), false))
                .FirstOrDefault(attrib => attrib?.LongName == verb)
                ?.HelpText;
    }

    // Options common to all classes.

    public class CommonOptions
    {
        [Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Show debugging level messages. Implies verbose")]
        public bool Debug { get; set; }

        [Option("debugger", DefaultValue = false, HelpText = "Launch debugger at start")]
        public bool Debugger { get; set; }

        [Option("net-useragent", HelpText = "Set the default user-agent string for HTTP requests")]
        public string NetUserAgent { get; set; }

        [Option("headless", DefaultValue = false, HelpText = "Set to disable all prompts")]
        public bool Headless { get; set; }

        [Option("asroot", DefaultValue = false, HelpText = "Allow CKAN to run as administrator")]
        public bool AsRoot { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
            => HelpText.AutoBuild(this, verb);

        public virtual int Handle(GameInstanceManager manager, IUser user)
        {
            CheckMonoVersion(user);

            // Processes in Docker containers normally run as root.
            // If we are running in a Docker container, do not require --asroot.
            // Docker creates a .dockerenv file in the root of each container.
            if (Platform.IsAdministrator())
            {
                if (!AsRoot)
                {
                    user.RaiseError(Properties.Resources.OptionsRootError);
                    user.RaiseMessage("");
                    user.RaiseMessage(Properties.Resources.OptionsPressAnyKey);
                    Console.ReadKey(true);
                    return Exit.ERROR;
                }
                else
                {
                    user.RaiseMessage(Properties.Resources.OptionsRootWarning);
                }
            }

            if (Debug)
            {
                LogManager.GetRepository().Threshold = Level.Debug;
                log.Info("Debug logging enabled");
            }
            else if (Verbose)
            {
                LogManager.GetRepository().Threshold = Level.Info;
                log.Info("Verbose logging enabled");
            }

            // Assign user-agent string if user has given us one
            if (NetUserAgent != null)
            {
                Net.UserAgentString = NetUserAgent;
            }

            return Exit.OK;
        }

        /// <summary>
        /// Combine two options objects.
        /// This is mainly to ensure that --headless carries through for prompt.
        /// </summary>
        /// <param name="otherOpts">Options object to merge into this one</param>
        public void Merge(CommonOptions otherOpts)
        {
            if (otherOpts != null)
            {
                Verbose      = Verbose      || otherOpts.Verbose;
                Debug        = Debug        || otherOpts.Debug;
                Debugger     = Debugger     || otherOpts.Debugger;
                NetUserAgent = NetUserAgent ?? otherOpts.NetUserAgent;
                Headless     = Headless     || otherOpts.Headless;
                AsRoot       = AsRoot       || otherOpts.AsRoot;
            }
        }

        private static void CheckMonoVersion(IUser user)
        {
            if (Platform.MonoVersion != null
                && Platform.MonoVersion < Platform.RecommendedMonoVersion)
            {
                user.RaiseMessage(Properties.Resources.OptionsMonoWarning,
                                  Platform.MonoVersion.ToString(),
                                  Platform.RecommendedMonoVersion.ToString());
            }
        }

        protected static readonly ILog log = LogManager.GetLogger(typeof(CommonOptions));
    }

    public class InstanceSpecificOptions : CommonOptions
    {
        [Option("instance", HelpText = "Game instance to use")]
        public string Instance { get; set; }

        [Option("gamedir", HelpText = "Game dir to use")]
        public string Gamedir { get; set; }

        public override int Handle(GameInstanceManager manager, IUser user)
        {
            int exitCode = base.Handle(manager, user);
            if (exitCode == Exit.OK)
            {
                // User provided game instance
                if (Gamedir != null && Instance != null)
                {
                    user.RaiseMessage(Properties.Resources.OptionsInstanceAndGameDir);
                    return Exit.BADOPT;
                }

                try
                {
                    if (!string.IsNullOrEmpty(Instance))
                    {
                        // Set a game directory by its alias.
                        manager.SetCurrentInstance(Instance);
                    }
                    else if (!string.IsNullOrEmpty(Gamedir))
                    {
                        // Set a game directory by its path
                        manager.SetCurrentInstanceByPath(Gamedir);
                    }
                }
                catch (NotKSPDirKraken k)
                {
                    user.RaiseMessage(Properties.Resources.InstanceNotInstance, k.path);
                    return Exit.BADOPT;
                }
                catch (InvalidKSPInstanceKraken k)
                {
                    user.RaiseMessage(Properties.Resources.OptionsInvalidInstance, k.instance);
                    return Exit.BADOPT;
                }
            }
            return exitCode;
        }
    }

    /// <summary>
    /// For things which are subcommands ('instance', 'repair' etc), we just grab a list
    /// we can pass on.
    /// </summary>
    public class SubCommandOptions : CommonOptions
    {
        [ValueList(typeof(List<string>))]
        public List<string> options { get; set; }

        public SubCommandOptions() { }

        public SubCommandOptions(string[] args)
        {
            options = new List<string>(args).GetRange(1, args.Length - 1);
        }
    }

    // Each action defines its own options that it supports.
    // Don't forget to cast to this type when you're processing them later on.

    internal class ScanOptions : InstanceSpecificOptions { }

    internal class VersionOptions   : CommonOptions { }
    internal class CleanOptions     : InstanceSpecificOptions { }

    #if NETFRAMEWORK || WINDOWS
    internal class GuiOptions : InstanceSpecificOptions
    {
        [Option("show-console", HelpText = "Shows the console while running the GUI")]
        public bool ShowConsole { get; set; }
    }
    #endif

    internal class ConsoleUIOptions : InstanceSpecificOptions
    {
        [Option("theme", HelpText = "Name of color scheme to use, falls back to environment variable CKAN_CONSOLEUI_THEME")]
        public string Theme { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class AvailableIdentifiersAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class InstalledIdentifiersAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class GameInstancesAttribute : Attribute { }
}
