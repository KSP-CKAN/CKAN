using CommandLine;

namespace CKAN.NetKAN
{
    /// <summary>
    /// Options for the NetKAN client.
    /// </summary>
    internal class CmdLineOptions
    {
        [Option('v', "verbose", DefaultValue = false, HelpText = "Show more of what's going on when running")]
        public bool Verbose { get; set; }

        [Option('d', "debug", DefaultValue = false, HelpText = "Show debugging level messages. Implies verbose")]
        public bool Debug { get; set; }

        [Option("debugger", HelpText = "Launch the debugger at start")]
        public bool Debugger { get; set; }

        [Option("outputdir", DefaultValue = ".", HelpText = "Output directory")]
        public string OutputDir { get; set; }

        [Option("cachedir", HelpText = "Cache directory for downloaded mods")]
        public string CacheDir { get; set; }

        [Option("github-token", HelpText = "GitHub OAuth token for API access")]
        public string GitHubToken { get; set; }

        [Option("gitlab-token", HelpText = "GitLab OAuth token for API access")]
        public string GitLabToken { get; set; }

        [Option("net-useragent", DefaultValue = null, HelpText = "Set the default User-Agent string for HTTP requests")]
        public string NetUserAgent { get; set; }

        [Option("releases", DefaultValue = "1", HelpText = "Number of releases to inflate, or 'all'")]
        public string Releases { get; set; }

        [Option("skip-releases", DefaultValue = "0", HelpText = "Number of releases to skip / index of release to inflate.")]
        public string SkipReleases { get; set; }

        [Option("prerelease", HelpText = "Index GitHub prereleases")]
        public bool PreRelease { get; set; }

        [Option("overwrite-cache", HelpText = "Overwrite cached files")]
        public bool OverwriteCache { get; set; }

        [Option("queues", HelpText = "Input,Output queue names for Queue Inflator mode")]
        public string Queues { get; set; }

        [Option("highest-version", HelpText = "Highest known version for auto-epoching")]
        public string HighestVersion { get; set; }

        [Option("validate-ckan", HelpText = "Name of .ckan file to check for errors")]
        public string ValidateCkan { get; set; }

        [Option("version", HelpText = "Display the netkan version number and exit")]
        public bool Version { get; set; }

        [Option("game", DefaultValue = "KSP", HelpText = "Short name of the game for which to inflate mods")]
        public string Game { get; set; }

        [ValueOption(0)]
        public string File { get; set; }
    }
}
