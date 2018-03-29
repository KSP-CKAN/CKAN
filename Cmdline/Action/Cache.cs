using CommandLine;
using CommandLine.Text;
using log4net;

namespace CKAN.CmdLine
{
    public class Cache : ISubCommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Cache));

        private CKAN.KSP CurrentInstance { get; set; }
        private IUser User { get; set; }

        public Cache() { }

        internal class CacheSubOptions : VerbCommandOptions
        {
            [VerbOption("list", HelpText = "List the download cache directory")]
            public ListOptions ListOptions { get; set; }

            [VerbOption("set", HelpText = "Set the download cache directory")]
            public SetOptions SetOptions { get; set; }

            [HelpVerbOption]
            public string GetUsage(string verb)
            {
                HelpText ht = HelpText.AutoBuild(this, verb);
                // Add a usage prefix line
                ht.AddPreOptionsLine(" ");
                if (string.IsNullOrEmpty(verb))
                {
                    ht.AddPreOptionsLine("ckan cache - Manage the download cache directory of CKAN");
                    ht.AddPreOptionsLine($"Usage: ckan cache <command> [options]");
                }
                else
                {
                    ht.AddPreOptionsLine("cache " + verb + " - " + GetDescription(verb));
                    switch (verb)
                    {
                        // First the commands with one string argument
                        case "set":
                            ht.AddPreOptionsLine($"Usage: ckan cache {verb} [options] path");
                            break;

                        // Now the commands with only --flag type options
                        case "list":
                        default:
                            ht.AddPreOptionsLine($"Usage: ckan cache {verb} [options]");
                            break;
                    }
                }
                return ht;
            }
        }

        public class ListOptions : InstanceSpecificOptions { }

        public class SetOptions : InstanceSpecificOptions
        {
            [ValueOption(0)]
            public string Path { get; set; }
        }

        public int RunSubCommand(KSPManager manager, CommonOptions opts, SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();

            int exitCode = Exit.OK;
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new CacheSubOptions(), (string option, object suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    User = new ConsoleUser(options.Headless);
                    if (manager == null)
                    {
                        manager = new KSPManager(User);
                    }
                    CurrentInstance = manager.CurrentInstance;
                    exitCode = options.Handle(manager, User);
                    if (exitCode != Exit.OK)
                        return;

                    switch (option)
                    {
                        case "list":
                            exitCode = ListCacheDirectory((ListOptions)suboptions);
                            break;

                        case "set":
                            exitCode = SetCacheDirectory((SetOptions)suboptions);
                            break;

                        default:
                            User.RaiseMessage("Unknown command: cache {0}", option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private int ListCacheDirectory(ListOptions options)
        {
            User.RaiseMessage("Location for Cached mod downloads:");
            var registry = RegistryManager.Instance(CurrentInstance).registry;

            User.RaiseMessage(registry.DownloadCacheDir);

            return Exit.OK;
        }

        private int SetCacheDirectory(SetOptions options)
        {
            if (options.Path == null)
            {
                User.RaiseError("set <path> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            var registry = RegistryManager.Instance(CurrentInstance).registry;
            log.DebugFormat("About to set Download Cache Directory to '{0}'", options.Path);

            registry.DownloadCacheDir = KSPPathUtils.NormalizePath(options.Path);

            return Exit.OK;
        }
    }
}
