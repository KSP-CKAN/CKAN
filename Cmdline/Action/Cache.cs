using Autofac;
using CKAN.Configuration;
using CommandLine;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing the CKAN cache.
    /// </summary>
    public class Cache : ISubCommand
    {
        private GameInstanceManager _manager;
        private IUser _user;

        /// <summary>
        /// Run the 'cache' command.
        /// </summary>
        /// <inheritdoc cref="ISubCommand.RunCommand"/>
        public int RunCommand(GameInstanceManager manager, object args)
        {
            var s = args.ToString();
            var opts = s.Replace(s.Substring(0, s.LastIndexOf('.') + 1), "").Split('+');

            CommonOptions options = new CommonOptions();
            _user = new ConsoleUser(options.Headless);
            _manager = manager ?? new GameInstanceManager(_user);
            var exitCode = options.Handle(_manager, _user);

            if (exitCode != Exit.Ok)
                return exitCode;

            switch (opts[1])
            {
                case "ClearCache":
                    exitCode = ClearCacheDirectory();
                    break;
                case "ListCache":
                    exitCode = ListCacheDirectory();
                    break;
                case "ResetCache":
                    exitCode = ResetCacheDirectory();
                    break;
                case "SetCache":
                    exitCode = SetCacheDirectory(args);
                    break;
                case "SetCacheLimit":
                    exitCode = SetCacheSizeLimit(args);
                    break;
                case "ShowCacheLimit":
                    exitCode = ShowCacheSizeLimit();
                    break;
                default:
                    exitCode = Exit.BadOpt;
                    break;
            }

            return exitCode;
        }

        /// <inheritdoc cref="ISubCommand.GetUsage"/>
        public string GetUsage(string prefix, string[] args)
        {
            if (args.Length == 1)
                return $"{prefix} {args[0]} <command> [options]";

            switch (args[1])
            {
                case "set":
                    return $"{prefix} {args[0]} {args[1]} [options] <path>";
                case "setlimit":
                    return $"{prefix} {args[0]} {args[1]} [options] <megabytes>";
                case "clear":
                case "list":
                case "reset":
                case "showlimit":
                    return $"{prefix} {args[0]} {args[1]} [options]";
                default:
                    return $"{prefix} {args[0]} <command> [options]";
            }
        }

        private int ClearCacheDirectory()
        {
            _manager.Cache.RemoveAll();
            _user.RaiseMessage("Cleared download cache.");
            return Exit.Ok;
        }

        private int ListCacheDirectory()
        {
            var cfg = ServiceLocator.Container.Resolve<IConfiguration>();
            _user.RaiseMessage("Download cache is set to \"{0}\".", cfg.DownloadCacheDir);
            PrintCacheInfo();
            return Exit.Ok;
        }

        private int ResetCacheDirectory()
        {
            if (_manager.TrySetupCache("", out string failReason))
            {
                var cfg = ServiceLocator.Container.Resolve<IConfiguration>();
                _user.RaiseMessage("Download cache reset to \"{0}\".", cfg.DownloadCacheDir);
                PrintCacheInfo();
            }
            else
            {
                _user.RaiseError("Can't reset cache path: {0}.", failReason);
                return Exit.Error;
            }

            return Exit.Ok;
        }

        private int SetCacheDirectory(object args)
        {
            var opts = (CacheOptions.SetCache)args;
            if (opts.Path == null)
            {
                _user.RaiseMessage("set <path> - argument missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            if (_manager.TrySetupCache(opts.Path, out string failReason))
            {
                var cfg = ServiceLocator.Container.Resolve<IConfiguration>();
                _user.RaiseMessage("Download cache set to \"{0}\".", cfg.DownloadCacheDir);
                PrintCacheInfo();
            }
            else
            {
                _user.RaiseError("Invalid path: {0}.", failReason);
                return Exit.Error;
            }

            return Exit.Ok;
        }

        private int SetCacheSizeLimit(object args)
        {
            var opts = (CacheOptions.SetCacheLimit)args;
            var cfg = ServiceLocator.Container.Resolve<IConfiguration>();
            if (opts.Megabytes < 0)
            {
                cfg.CacheSizeLimit = null;
            }
            else
            {
                cfg.CacheSizeLimit = opts.Megabytes * 1024 * 1024;
            }

            ShowCacheSizeLimit();
            return Exit.Ok;
        }

        private int ShowCacheSizeLimit()
        {
            var cfg = ServiceLocator.Container.Resolve<IConfiguration>();
            var limit = cfg.CacheSizeLimit.HasValue
                ? CkanModule.FmtSize(cfg.CacheSizeLimit.Value)
                : "Unlimited";

            _user.RaiseMessage("Cache limit set to {0}.", limit);
            return Exit.Ok;
        }

        private void PrintCacheInfo()
        {
            _manager.Cache.GetSizeInfo(out int fileCount, out long bytes);
            _user.RaiseMessage("Cache currently has {0} files that use {1}.", fileCount, CkanModule.FmtSize(bytes));
        }
    }

    [Verb("cache", HelpText = "Manage download cache path")]
    [ChildVerbs(typeof(ClearCache), typeof(ListCache), typeof(ResetCache), typeof(SetCache), typeof(SetCacheLimit), typeof(ShowCacheLimit))]
    internal class CacheOptions
    {
        [VerbExclude]
        [Verb("clear", HelpText = "Clear the download cache directory")]
        internal class ClearCache : CommonOptions { }

        [VerbExclude]
        [Verb("list", HelpText = "List the download cache path")]
        internal class ListCache : CommonOptions { }

        [VerbExclude]
        [Verb("reset", HelpText = "Set the download cache path to the default")]
        internal class ResetCache : CommonOptions { }

        [VerbExclude]
        [Verb("set", HelpText = "Set the download cache path")]
        internal class SetCache : CommonOptions
        {
            [Value(0, MetaName = "Path", HelpText = "The path to set the download cache to")]
            public string Path { get; set; }
        }

        [VerbExclude]
        [Verb("setlimit", HelpText = "Set the cache size limit")]
        internal class SetCacheLimit : CommonOptions
        {
            [Value(0, MetaName = "MB", HelpText = "The max amount of MB the download cache stores files")]
            public long Megabytes { get; set; } = -1;
        }

        [VerbExclude]
        [Verb("showlimit", HelpText = "Show the cache size limit")]
        internal class ShowCacheLimit : CommonOptions { }
    }
}
