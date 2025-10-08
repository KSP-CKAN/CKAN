using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using CommandLine;
using log4net;

using CKAN.IO;

namespace CKAN.CmdLine
{
    public class Install : ICommand
    {
        /// <summary>
        /// Initialize the install command object
        /// </summary>
        /// <param name="mgr">GameInstanceManager containing our instances</param>
        /// <param name="user">IUser object for interaction</param>
        public Install(GameInstanceManager mgr, RepositoryDataManager repoData, IUser user)
        {
            manager       = mgr;
            this.repoData = repoData;
            this.user     = user;
        }

        /// <summary>
        /// Installs a module, if available
        /// </summary>
        /// <param name="instance">Game instance into which to install</param>
        /// <param name="raw_options">Command line options object</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunCommand(CKAN.GameInstance instance, object raw_options)
        {
            RegistryManager.Instance(instance, repoData).ScanUnmanagedFiles();

            var options = raw_options as InstallOptions;
            if (options?.modules?.Count == 0 && options.ckan_files == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                foreach (var h in Actions.GetHelp("install"))
                {
                    user.RaiseError("{0}", h);
                }
                return Exit.BADOPT;
            }

            var regMgr = RegistryManager.Instance(instance, repoData);
            List<CkanModule>? modules = null;

            if (options?.ckan_files != null)
            {
                // Install from CKAN files
                try
                {
                    var targets = options.ckan_files
                                         .Select(arg => new NetAsyncDownloader.DownloadTargetFile(getUri(arg)))
                                         .ToArray();
                    log.DebugFormat("Urls: {0}", targets.SelectMany(t => t.urls));
                    new NetAsyncDownloader(new NullUser(), () => null, options.NetUserAgent).DownloadAndWait(targets);
                    log.DebugFormat("Files: {0}", targets.Select(t => t.filename));
                    modules = targets.Select(t => CkanModule.FromFile(t.filename))
                                     .ToList();
                }
                catch (FileNotFoundKraken kraken)
                {
                    user.RaiseError(Properties.Resources.InstallNotFound,
                                    kraken.file ?? "");
                    return Exit.ERROR;
                }
                catch (Kraken kraken)
                {
                    user.RaiseError("{0}",
                        kraken.InnerException == null
                            ? kraken.Message
                            : $"{kraken.Message}: {kraken.InnerException.Message}");
                    return Exit.ERROR;
                }
            }
            else
            {
                var identifiers = options?.modules ?? new List<string> { };
                var registry    = regMgr.registry;
                var installed   = registry.InstalledModules
                                          .Select(im => im.Module)
                                          .ToArray();
                var crit        = instance.VersionCriteria();
                Search.AdjustModulesCase(instance, registry, identifiers);
                modules = identifiers.Select(arg => CkanModule.FromIDandVersion(
                                                        registry, arg,
                                                        (options?.allow_incompatible ?? false)
                                                            ? null
                                                            : crit)
                                                    ?? registry.LatestAvailable(arg,
                                                                                instance.StabilityToleranceConfig,
                                                                                crit, null, installed)
                                                    ?? registry.InstalledModule(arg)?.Module)
                                     .OfType<CkanModule>()
                                     .ToList();
            }

            if (manager.Cache == null)
            {
                return Exit.ERROR;
            }

            var installer   = new ModuleInstaller(instance, manager.Cache,
                                                  manager.Configuration, user);
            var install_ops = new RelationshipResolverOptions(instance.StabilityToleranceConfig)
            {
                with_all_suggests              = options?.with_all_suggests ?? false,
                with_suggests                  = options?.with_suggests ?? false,
                with_recommends                = !options?.no_recommends ?? true,
                allow_incompatible             = options?.allow_incompatible ?? false,
                without_toomanyprovides_kraken = user.Headless,
                without_enforce_consistency    = user.Headless,
            };

            for (bool done = false; !done; )
            {
                // Install everything requested. :)
                try
                {
                    HashSet<string>? possibleConfigOnlyDirs = null;
                    installer.InstallList(modules, install_ops, regMgr,
                                          ref possibleConfigOnlyDirs,
                                          new InstalledFilesDeduplicator(instance,
                                                                         manager.Instances.Values,
                                                                         repoData),
                                          options?.NetUserAgent);
                    user.RaiseMessage("");
                    done = true;
                }
                catch (TooManyModsProvideKraken ex)
                {
                    // Request the user selects one of the mods
                    int result;
                    var choices = ex.modules.OrderByDescending(m => repoData.GetDownloadCount(regMgr.registry.Repositories.Values,
                                                                                              m.identifier)
                                                                    ?? 0)
                                            .ThenByDescending(m => m.identifier == ex.requested)
                                            .ThenBy(m => m.identifier)
                                            .ToArray();
                    try
                    {
                        result = user.RaiseSelectionDialog(
                            ex.Message,
                            choices.Select(m => string.Format("{0} ({1})",
                                                              m.identifier, m.name))
                                   .ToArray());
                    }
                    catch (Kraken e)
                    {
                        user.RaiseMessage("{0}", e.Message);
                        return Exit.ERROR;
                    }

                    if (result < 0)
                    {
                        user.RaiseMessage("");
                        return Exit.ERROR;
                    }

                    // Add the module to the list.
                    modules.Add(choices[result]);
                    // DON'T return so we can loop around and try again
                }
                catch (CancelledActionKraken k)
                {
                    user.RaiseError(Properties.Resources.InstallAborted,
                                    k.Message);
                    return Exit.ERROR;
                }
                catch (RequestThrottledKraken kraken)
                {
                    user.RaiseError("{0}", kraken.Message);
                    user.RaiseMessage(Properties.Resources.InstallTryAuthToken,
                                      kraken.infoUrl);
                    return Exit.ERROR;
                }
                catch (Kraken kraken)
                {
                    // Show nice message for mod problems
                    user.RaiseError("{0}", kraken.Message);
                    user.RaiseMessage("{0}", kraken switch
                    {
                        DependenciesNotSatisfiedKraken or ModuleNotFoundKraken => Properties.Resources.InstallTryAgain,
                        _ => Properties.Resources.InstallCancelled,
                    });
                    return Exit.ERROR;
                }
                catch (Exception exc)
                {
                    // Show stack trace for code problems
                    user.RaiseError("{0}", exc.ToString());
                    user.RaiseMessage(Properties.Resources.InstallCancelled);
                    return Exit.ERROR;
                }
            }

            return Exit.OK;
        }

        private static Uri getUri(string arg)
            => Uri.IsWellFormedUriString(arg, UriKind.Absolute)
                ? new Uri(arg)
                : File.Exists(arg)
                    ? new Uri(Path.GetFullPath(arg))
                    : throw new FileNotFoundKraken(arg);

        private readonly GameInstanceManager   manager;
        private readonly RepositoryDataManager repoData;
        private readonly IUser                 user;

        private static readonly ILog log = LogManager.GetLogger(typeof(Install));
    }

    internal class InstallOptions : InstanceSpecificOptions
    {
        [OptionArray('c', "ckanfiles", HelpText = "Local CKAN files or URLs to process")]
        public string[]? ckan_files { get; set; }

        [Option("no-recommends", DefaultValue = false, HelpText = "Do not install recommended modules")]
        public bool no_recommends { get; set; }

        [Option("with-suggests", DefaultValue = false, HelpText = "Install suggested modules")]
        public bool with_suggests { get; set; }

        [Option("with-all-suggests", DefaultValue = false, HelpText = "Install suggested modules all the way down")]
        public bool with_all_suggests { get; set; }

        [Option("allow-incompatible", DefaultValue = false, HelpText = "Install modules that are not compatible with the current game version")]
        public bool allow_incompatible { get; set; }

        [ValueList(typeof(List<string>))]
        [AvailableIdentifiers]
        public List<string>? modules { get; set; }
    }

}
