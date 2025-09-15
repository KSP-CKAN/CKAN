using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

using CKAN.Configuration;

namespace CKAN.CmdLine
{
    [ExcludeFromCodeCoverage]
    public class CacheSubOptions : VerbCommandOptions
    {
        [VerbOption("list", HelpText = "List the download cache path")]
        public CommonOptions? ListOptions { get; set; }

        [VerbOption("set", HelpText = "Set the download cache path")]
        public SetOptions? SetOptions { get; set; }

        [VerbOption("clear", HelpText = "Clear the download cache directory")]
        public CommonOptions? ClearOptions { get; set; }

        [VerbOption("reset", HelpText = "Set the download cache path to the default")]
        public CommonOptions? ResetOptions { get; set; }

        [VerbOption("showlimit", HelpText = "Show the cache size limit")]
        public CommonOptions? ShowLimitOptions { get; set; }

        [VerbOption("setlimit", HelpText = "Set the cache size limit")]
        public SetLimitOptions? SetLimitOptions { get; set; }

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

        [ExcludeFromCodeCoverage]
        public static IEnumerable<string> GetHelp(string verb)
        {
            // Add a usage prefix line
            yield return " ";
            if (string.IsNullOrEmpty(verb))
            {
                yield return $"ckan cache - {Properties.Resources.CacheHelpSummary}";
                yield return $"{Properties.Resources.Usage}: ckan cache <{Properties.Resources.Command}> [{Properties.Resources.Options}]";
            }
            else
            {
                yield return "cache " + verb + " - " + GetDescription(typeof(CacheSubOptions), verb);
                switch (verb)
                {
                    // First the commands with one string argument
                    case "set":
                        yield return $"{Properties.Resources.Usage}: ckan cache {verb} [{Properties.Resources.Options}] path";
                        break;
                    case "setlimit":
                        yield return $"{Properties.Resources.Usage}: ckan cache {verb} [{Properties.Resources.Options}] mebibytes";
                        break;

                    // Now the commands with only --flag type options
                    case "list":
                    case "clear":
                    case "reset":
                    case "showlimit":
                    default:
                        yield return $"{Properties.Resources.Usage}: ckan cache {verb} [{Properties.Resources.Options}]";
                        break;
                }
            }
        }
    }

    public class SetOptions : CommonOptions
    {
        [ValueOption(0)]
        public string? Path { get; set; }
    }

    public class SetLimitOptions : CommonOptions
    {
        [ValueOption(0)]
        public long Mebibytes { get; set; } = -1;
    }

    public class Cache : ISubCommand
    {
        public Cache(GameInstanceManager mgr,
                     IUser               user)
        {
            manager   = mgr;
            this.user = user;
        }

        /// <summary>
        /// Execute a cache subcommand
        /// </summary>
        /// <param name="mgr">GameInstanceManager object containing our instances and cache</param>
        /// <param name="opts">Command line options object</param>
        /// <param name="unparsed">Raw command line options</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunSubCommand(CommonOptions?    opts,
                                 SubCommandOptions unparsed)
        {
            string[] args = unparsed.options.ToArray();

            int exitCode = Exit.OK;
            // Parse and process our sub-verbs
            Parser.Default.ParseArgumentsStrict(args, new CacheSubOptions(), (option, suboptions) =>
            {
                // ParseArgumentsStrict calls us unconditionally, even with bad arguments
                if (!string.IsNullOrEmpty(option) && suboptions != null)
                {
                    CommonOptions options = (CommonOptions)suboptions;
                    options.Merge(opts);
                    exitCode = options.Handle(manager, user);
                    if (exitCode != Exit.OK)
                    {
                        return;
                    }

                    switch (option)
                    {
                        case "list":
                            exitCode = ListCacheDirectory();
                            break;

                        case "set":
                            exitCode = SetCacheDirectory((SetOptions)suboptions);
                            break;

                        case "clear":
                            exitCode = ClearCacheDirectory();
                            break;

                        case "reset":
                            exitCode = ResetCacheDirectory();
                            break;

                        case "showlimit":
                            exitCode = ShowCacheSizeLimit();
                            break;

                        case "setlimit":
                            exitCode = SetCacheSizeLimit((SetLimitOptions)suboptions);
                            break;

                        default:
                            user.RaiseMessage("{0}: cache {1}", Properties.Resources.UnknownCommand, option);
                            exitCode = Exit.BADOPT;
                            break;
                    }
                }
            }, () => { exitCode = MainClass.AfterHelp(user); });
            return exitCode;
        }

        private int ListCacheDirectory()
        {
            user.RaiseMessage("{0}", manager?.Configuration.DownloadCacheDir ?? "");
            printCacheInfo();
            return Exit.OK;
        }

        private int SetCacheDirectory(SetOptions options)
        {
            if (options.Path is not { Length: > 0 })
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                PrintUsage("set");
                return Exit.BADOPT;
            }

            if (manager.TrySetupCache(options.Path,
                                      new Progress<int>(p => {}),
                                      out string? failReason))
            {
                user.RaiseMessage(Properties.Resources.CacheSet,
                                  manager.Configuration.DownloadCacheDir ?? "");
                printCacheInfo();
                return Exit.OK;
            }
            else
            {
                user.RaiseError(Properties.Resources.CacheInvalidPath, failReason);
                return Exit.BADOPT;
            }
        }

        private int ClearCacheDirectory()
        {
            manager?.Cache?.RemoveAll();
            user.RaiseMessage(Properties.Resources.CacheCleared);
            printCacheInfo();
            return Exit.OK;
        }

        private int ResetCacheDirectory()
        {
            if (manager.TrySetupCache(null,
                                      new Progress<int>(p => {}),
                                      out string? failReason))
            {
                user.RaiseMessage(Properties.Resources.CacheReset,
                                  manager.Configuration.DownloadCacheDir ?? "");
                printCacheInfo();
            }
            else
            {
                user.RaiseError(Properties.Resources.CacheResetFailed, failReason);
            }
            return Exit.OK;
        }

        private int ShowCacheSizeLimit()
        {
            if (manager?.Configuration.CacheSizeLimit is long limit)
            {
                user.RaiseMessage("{0}", CkanModule.FmtSize(limit));
            }
            else
            {
                user.RaiseMessage(Properties.Resources.CacheUnlimited);
            }
            return Exit.OK;
        }

        private int SetCacheSizeLimit(SetLimitOptions options)
        {
            if (manager?.Configuration is IConfiguration cfg)
            {
                cfg.CacheSizeLimit = options.Mebibytes < 0
                    ? null
                    : (options.Mebibytes * 1024 * 1024);
            }
            return ShowCacheSizeLimit();
        }

        private void printCacheInfo()
        {
            if (manager?.Cache != null)
            {
                manager.Cache.GetSizeInfo(out int fileCount, out long bytes, out long? bytesFree);
                if (bytesFree.HasValue)
                {
                    user.RaiseMessage(Properties.Resources.CacheInfo,
                                      fileCount,
                                      CkanModule.FmtSize(bytes),
                                      CkanModule.FmtSize(bytesFree.Value));
                }
                else
                {
                    user.RaiseMessage(Properties.Resources.CacheInfoFreeSpaceUnknown,
                                      fileCount,
                                      CkanModule.FmtSize(bytes));
                }
            }
        }

        [ExcludeFromCodeCoverage]
        private void PrintUsage(string verb)
        {
            foreach (var h in CacheSubOptions.GetHelp(verb))
            {
                user.RaiseError("{0}", h);
            }
        }

        private readonly GameInstanceManager manager;
        private readonly IUser               user;
    }

}
