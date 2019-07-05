using System;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using log4net;
using CKAN.Versioning;
using CKAN.Win32Registry;
using Autofac;

namespace CKAN.CmdLine
{
    public class Cache : ISubCommand
    {
        public Cache() { }

        private class CacheSubOptions : VerbCommandOptions
        {
            [VerbOption("list", HelpText = "List the download cache path")]
            public CommonOptions ListOptions { get; set; }

            [VerbOption("set", HelpText = "Set the download cache path")]
            public SetOptions SetOptions { get; set; }

            [VerbOption("clear", HelpText = "Clear the download cache directory")]
            public CommonOptions ClearOptions { get; set; }

            [VerbOption("reset", HelpText = "Set the download cache path to the default")]
            public CommonOptions ResetOptions { get; set; }

            [VerbOption("showlimit", HelpText = "Show the cache size limit")]
            public CommonOptions ShowLimitOptions { get; set; }

            [VerbOption("setlimit", HelpText = "Set the cache size limit")]
            public SetLimitOptions SetLimitOptions { get; set; }

            [HelpVerbOption]
            public string GetUsage(string verb)
            {
                HelpText ht = HelpText.AutoBuild(this, verb);
                // Add a usage prefix line
                ht.AddPreOptionsLine(" ");
                if (string.IsNullOrEmpty(verb))
                {
                    ht.AddPreOptionsLine("ckan cache - Manage the download cache path of CKAN");
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
                        case "setlimit":
                            ht.AddPreOptionsLine($"Usage: ckan cache {verb} [options] megabytes");
                            break;

                        // Now the commands with only --flag type options
                        case "list":
                        case "clear":
                        case "reset":
                        case "showlimit":
                        default:
                            ht.AddPreOptionsLine($"Usage: ckan cache {verb} [options]");
                            break;
                    }
                }
                return ht;
            }
        }

        private class SetOptions : CommonOptions
        {
            [ValueOption(0)]
            public string Path { get; set; }
        }

        private class SetLimitOptions : CommonOptions
        {
            [ValueOption(0)]
            public long Megabytes { get; set; } = -1;
        }

        /// <summary>
        /// Execute a cache subcommand
        /// </summary>
        /// <param name="mgr">KSPManager object containing our instances and cache</param>
        /// <param name="opts">Command line options object</param>
        /// <param name="unparsed">Raw command line options</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunSubCommand(KSPManager mgr, CommonOptions opts, SubCommandOptions unparsed)
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
                    user     = new ConsoleUser(options.Headless);
                    manager  = mgr ?? new KSPManager(user);
                    exitCode = options.Handle(manager, user);
                    if (exitCode != Exit.OK)
                        return;

                    switch (option)
                    {
                        case "list":
                            exitCode = ListCacheDirectory((CommonOptions)suboptions);
                            break;

                        case "set":
                            exitCode = SetCacheDirectory((SetOptions)suboptions);
                            break;

                        case "clear":
                            exitCode = ClearCacheDirectory((CommonOptions)suboptions);
                            break;

                        case "reset":
                            exitCode = ResetCacheDirectory((CommonOptions)suboptions);
                            break;

                        case "showlimit":
                            exitCode = ShowCacheSizeLimit((CommonOptions)suboptions);
                            break;

                        case "setlimit":
                            exitCode = SetCacheSizeLimit((SetLimitOptions)suboptions);
                            break;

                        default:
                            user.RaiseMessage("Unknown command: cache {0}", option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(); });
            return exitCode;
        }

        private int ListCacheDirectory(CommonOptions options)
        {
            IWin32Registry winReg = ServiceLocator.Container.Resolve<IWin32Registry>();
            user.RaiseMessage(winReg.DownloadCacheDir);
            printCacheInfo();
            return Exit.OK;
        }

        private int SetCacheDirectory(SetOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                user.RaiseError("set <path> - argument missing, perhaps you forgot it?");
                return Exit.BADOPT;
            }

            string failReason;
            if (manager.TrySetupCache(options.Path, out failReason))
            {
                IWin32Registry winReg = ServiceLocator.Container.Resolve<IWin32Registry>();
                user.RaiseMessage($"Download cache set to {winReg.DownloadCacheDir}");
                printCacheInfo();
                return Exit.OK;
            }
            else
            {
                user.RaiseError($"Invalid path: {failReason}");
                return Exit.BADOPT;
            }
        }

        private int ClearCacheDirectory(CommonOptions options)
        {
            manager.Cache.RemoveAll();
            user.RaiseMessage("Download cache cleared.");
            printCacheInfo();
            return Exit.OK;
        }

        private int ResetCacheDirectory(CommonOptions options)
        {
            string failReason;
            if (manager.TrySetupCache("", out failReason))
            {
                IWin32Registry winReg = ServiceLocator.Container.Resolve<IWin32Registry>();
                user.RaiseMessage($"Download cache reset to {winReg.DownloadCacheDir}");
                printCacheInfo();
            }
            else
            {
                user.RaiseError($"Can't reset cache path: {failReason}");
            }
            return Exit.OK;
        }

        private int ShowCacheSizeLimit(CommonOptions options)
        {
            IWin32Registry winReg = ServiceLocator.Container.Resolve<IWin32Registry>();
            if (winReg.CacheSizeLimit.HasValue)
            {
                user.RaiseMessage(CkanModule.FmtSize(winReg.CacheSizeLimit.Value));
            }
            else
            {
                user.RaiseMessage("Unlimited");
            }
            return Exit.OK;
        }

        private int SetCacheSizeLimit(SetLimitOptions options)
        {
            IWin32Registry winReg = ServiceLocator.Container.Resolve<IWin32Registry>();
            if (options.Megabytes < 0)
            {
                winReg.CacheSizeLimit = null;
            }
            else
            {
                winReg.CacheSizeLimit = options.Megabytes * (long)1024 * (long)1024;
            }
            return ShowCacheSizeLimit(null);
        }

        private void printCacheInfo()
        {
            int fileCount;
            long bytes;
            manager.Cache.GetSizeInfo(out fileCount, out bytes);
            user.RaiseMessage($"{fileCount} files, {CkanModule.FmtSize(bytes)}");
        }

        private KSPManager manager;
        private IUser      user;

        private static readonly ILog log = LogManager.GetLogger(typeof(Cache));
    }

}
