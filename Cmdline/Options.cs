using System.Collections.Generic;
using CommandLine;

namespace CKAN.CmdLine
{
    // Look, parsing options is so easy and beautiful I made
    // it into a special class for you to admire!

    public class Options
    {
        public string action { get; set; }
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
                    action = verb;
                    options = suboptions;
                },
                delegate
                {
                    throw (new BadCommandKraken());
                }
            );
        }
    }
        
    // Actions supported by our client go here.
    // TODO: Figure out how to do per action help screens.

    internal class Actions
    {

        [VerbOption("gui", HelpText = "Start the CKAN GUI")]
        public GuiOptions GuiOptions { get; set; }

        [VerbOption("gui-with-console", HelpText = "Start the CKAN GUI with the console visible")]
        public GuiOptions GuiConsoleOptions { get; set; }

        [VerbOption("search", HelpText = "Search for mods")]
        public SearchOptions SearchOptions { get; set; }

        [VerbOption("upgrade", HelpText = "Upgrade an installed mod")]
        public UpgradeOptions Upgrade { get; set; }

        [VerbOption("update", HelpText = "Update list of available mods")]
        public UpdateOptions Update { get; set; }

        [VerbOption("available", HelpText = "List available mods")]
        public AvailableOptions Available { get; set; }

        [VerbOption("install", HelpText = "Install a KSP mod")]
        public InstallOptions Install { get; set; }

        [VerbOption("remove", HelpText = "Remove an installed mod")]
        public RemoveOptions Remove { get; set; }

        [VerbOption("scan", HelpText = "Scan for manually installed KSP mods")]
        public ScanOptions Scan { get; set; }

        [VerbOption("list", HelpText = "List installed modules")]
        public ListOptions List { get; set; }

        [VerbOption("show", HelpText = "Show information about a mod")]
        public ShowOptions Show { get; set; }

        [VerbOption("clean", HelpText = "Clean away downloaded files from the cache")]
        public CleanOptions Clean { get; set; }

        [VerbOption("repair", HelpText = "Attempt various automatic repairs")]
        public SubCommandOptions Repair { get; set; }

        [VerbOption("repo", HelpText = "Manage CKAN repositories")]
        public SubCommandOptions KSP { get; set; }

        [VerbOption("ksp", HelpText = "Manage KSP installs")]
        public SubCommandOptions Repo { get; set; }

        [VerbOption("compare", HelpText = "Compare version strings")]
        public CompareOptions Compare { get; set; }

        [VerbOption("version", HelpText = "Show the version of the CKAN client being used.")]
        public VersionOptions Version { get; set; }
    }

    // Options common to all classes.

    public class CommonOptions
    {
        [Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Show debugging level messages. Implies verbose")]
        public bool Debug { get; set; }

        [Option("ksp", DefaultValue = null, HelpText = "KSP install to use")]
        public string KSP { get; set; }

        [Option("kspdir", DefaultValue = null, HelpText = "KSP dir to use")]
        public string KSPdir { get; set; }

        [Option("net-useragent", DefaultValue = null, HelpText = "Set the default user-agent string for HTTP requests")]
        public string NetUserAgent { get; set; }

        [Option("headless", DefaultValue = null, HelpText = "Set to disable all prompts")]
        public bool Headless { get; set; }

        [Option("asroot", DefaultValue = null, HelpText = "Allows CKAN to run as root on Linux- based systems")]
        public bool AsRoot { get; set; }
    }

    /// <summary>
    /// For things which are subcommands ('ksp', 'repair' etc), we just grab a list
    /// we can pass on.
    /// </summary>
    public class SubCommandOptions : CommonOptions
    {
        [ValueList(typeof(List<string>))]
        public List<string> options { get; set; }
    }

    // Each action defines its own options that it supports.
    // Don't forget to cast to this type when you're processing them later on.

    internal class InstallOptions : CommonOptions
    {
        [OptionArray('c', "ckanfiles", HelpText = "Local CKAN files to process")]
        public string[] ckan_files { get; set; }

        [Option("no-recommends", HelpText = "Do not install recommended modules")]
        public bool no_recommends { get; set; }

        [Option("with-suggests", HelpText = "Install suggested modules")]
        public bool with_suggests { get; set; }

        [Option("with-all-suggests", HelpText = "Install suggested modules all the way down")]
        public bool with_all_suggests { get; set; }

        // TODO: How do we provide helptext on this?
        [ValueList(typeof (List<string>))]
        public List<string> modules { get; set; }
    }

    internal class UpgradeOptions : CommonOptions
    {
        [Option('c', "ckanfile", HelpText = "Local CKAN file to process")]
        public string ckan_file { get; set; }

        [Option("no-recommends", HelpText = "Do not install recommended modules")]
        public bool no_recommends { get; set; }

        [Option("with-suggests", HelpText = "Install suggested modules")]
        public bool with_suggests { get; set; }

        [Option("with-all-suggests", HelpText = "Install suggested modules all the way down")]
        public bool with_all_suggests { get; set; }

        [Option("all", HelpText = "Upgrade all available updated modules")]
        public bool upgrade_all { get; set; }

        // TODO: How do we provide helptext on this?
        [ValueList(typeof (List<string>))]
        public List<string> modules { get; set; }
    }

    internal class ScanOptions : CommonOptions
    {
    }

    internal class ListOptions : CommonOptions
    {
        [Option("porcelain", HelpText = "Dump raw list of modules, good for shell scripting")]
        public bool porcelain { get; set; }
    }

    internal class VersionOptions : CommonOptions
    {
    }

    internal class CleanOptions : CommonOptions
    {
    }

    internal class AvailableOptions : CommonOptions
    {
    }

    internal class GuiOptions : CommonOptions
    {
        [Option("show-console", HelpText = "Shows the console while running the GUI")]
        public bool ShowConsole { get; set; }
    }

    internal class UpdateOptions : CommonOptions
    {
        // This option is really meant for devs testing their CKAN-meta forks.
        [Option('r', "repo", HelpText = "CKAN repository to use (experimental!)")]
        public string repo { get; set; }

        [Option("all", HelpText = "Upgrade all available updated modules")]
        public bool update_all { get; set; }

        [Option("list-changes", DefaultValue = false, HelpText = "List new and removed modules")]
        public bool list_changes { get; set; }
    }

    internal class RemoveOptions : CommonOptions
    {
        [Option("re", HelpText = "Parse arguments as regular expressions")]
        public bool regex { get; set; }

        [ValueList(typeof(List<string>))]
        public List<string> modules { get; set; }
    }

    internal class ShowOptions : CommonOptions
    {
        [ValueOption(0)]
        public string Modname { get; set; }
    }

    internal class ClearCacheOptions : CommonOptions
    {
    }

    internal class SearchOptions : CommonOptions
    {
        [ValueOption(0)]
        public string search_term { get; set; }
    }

    internal class CompareOptions : CommonOptions
    {
        [ValueOption(0)]
        public string Left { get; set; }

        [ValueOption(1)]
        public string Right { get; set; }
    }
}
