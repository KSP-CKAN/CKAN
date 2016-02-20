using CommandLine;

namespace CKAN.NetKAN
{
    /// <summary>
    /// Options for the NetKAN client.
    /// </summary>
    internal class CmdLineOptions
    {
        [Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running.")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Show debugging level messages. Implies verbose")]
        public bool Debug { get; set; }

        [Option("debugger", HelpText = "Launch the debugger at start.")]
        public bool Debugger { get; set; }

        [Option("outputdir", DefaultValue = ".", HelpText = "Output directory")]
        public string OutputDir { get; set; }

        [Option("cachedir", HelpText = "Cache directory for downloaded mods")]
        public string CacheDir { get; set; }

        [Option("github-token", HelpText = "GitHub OAuth token for API access")]
        public string GitHubToken { get; set; }

        [Option("net-useragent", DefaultValue = null, HelpText = "Set the default user-agent string for HTTP requests")]
        public string NetUserAgent { get; set; }

        [Option("prerelease", HelpText = "Index GitHub Prereleases")]
        public bool PreRelease { get; set; }

        [Option("version", HelpText = "Display the netkan version number and exit.")]
        public bool Version { get; set; }

        [Option('T', "generate-torrents", DefaultValue = "", HelpText = "Generate .torrent files in specified directory, and write btih property to .ckan's")]
        public string GenerateTorrents { get; set; }

        // TODO: How do we mark this as required?
        [ValueOption(0)]
        public string File { get; set; }
    }
}

