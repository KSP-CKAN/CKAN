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

        [Option("outputdir", DefaultValue = ".", HelpText = "Output directory (defaults to '.')")]
        public string OutputDir { get; set; }

        // TODO: How do we mark this as required?
        [ValueOption(0)]
        public string File { get; set; }
    }
}

