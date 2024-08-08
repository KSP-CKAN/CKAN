using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using CommandLine;
using log4net;

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
            var options = raw_options as InstallOptions;
            if (options.modules.Count == 0 && options.ckan_files == null)
            {
                user.RaiseError(Properties.Resources.ArgumentMissing);
                foreach (var h in Actions.GetHelp("install"))
                {
                    user.RaiseError(h);
                }
                return Exit.BADOPT;
            }

            var regMgr = RegistryManager.Instance(instance, repoData);
            List<CkanModule> modules = null;

            if (options.ckan_files != null)
            {
                // Install from CKAN files
                try
                {
                    var targets = options.ckan_files
                                         .Select(arg => new NetAsyncDownloader.DownloadTargetFile(getUri(arg)))
                                         .ToArray();
                    log.DebugFormat("Urls: {0}", targets.SelectMany(t => t.urls));
                    new NetAsyncDownloader(new NullUser()).DownloadAndWait(targets);
                    log.DebugFormat("Files: {0}", targets.Select(t => t.filename));
                    modules = targets.Select(t => MainClass.LoadCkanFromFile(t.filename))
                                     .ToList();
                }
                catch (FileNotFoundKraken kraken)
                {
                    user.RaiseError(Properties.Resources.InstallNotFound,
                                    kraken.file);
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
                var identifiers = options.modules;
                var registry    = regMgr.registry;
                var installed   = registry.InstalledModules
                                          .Select(im => im.Module)
                                          .ToArray();
                var crit        = instance.VersionCriteria();
                Search.AdjustModulesCase(instance, registry, identifiers);
                modules = identifiers.Select(arg => CkanModule.FromIDandVersion(
                                                        registry, arg,
                                                        options.allow_incompatible
                                                            ? null
                                                            : crit)
                                                    ?? registry.LatestAvailable(arg, crit,
                                                                                null, installed)
                                                    ?? registry.InstalledModule(arg)?.Module)
                                     .ToList();
            }

            var installer   = new ModuleInstaller(instance, manager.Cache, user);
            var install_ops = new RelationshipResolverOptions
            {
                with_all_suggests              = options.with_all_suggests,
                with_suggests                  = options.with_suggests,
                with_recommends                = !options.no_recommends,
                allow_incompatible             = options.allow_incompatible,
                without_toomanyprovides_kraken = user.Headless,
                without_enforce_consistency    = user.Headless,
            };

            for (bool done = false; !done; )
            {
                // Install everything requested. :)
                try
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    installer.InstallList(modules, install_ops, regMgr,
                                          ref possibleConfigOnlyDirs);
                    user.RaiseMessage("");
                    done = true;
                }
                catch (DependencyNotSatisfiedKraken ex)
                {
                    user.RaiseError("{0}", ex.Message);
                    user.RaiseMessage(Properties.Resources.InstallTryAgain);
                    return Exit.ERROR;
                }
                catch (ModuleNotFoundKraken ex)
                {
                    if (ex.version == null)
                    {
                        user.RaiseError(Properties.Resources.InstallUnversionedDependencyNotSatisfied,
                            ex.module, instance.game.ShortName);
                    }
                    else
                    {
                        user.RaiseError(Properties.Resources.InstallVersionedDependencyNotSatisfied,
                            ex.module, ex.version, instance.game.ShortName);
                    }
                    user.RaiseMessage(Properties.Resources.InstallTryAgain);
                    return Exit.ERROR;
                }
                catch (BadMetadataKraken ex)
                {
                    user.RaiseError(Properties.Resources.InstallBadMetadata, ex.module, ex.Message);
                    return Exit.ERROR;
                }
                catch (TooManyModsProvideKraken ex)
                {
                    // Request the user selects one of the mods
                    int result;
                    try
                    {
                        result = user.RaiseSelectionDialog(
                            ex.Message,
                            ex.modules
                                .Select(m => string.Format("{0} ({1})", m.identifier, m.name))
                                .ToArray());
                    }
                    catch (Kraken e)
                    {
                        user.RaiseMessage(e.Message);
                        return Exit.ERROR;
                    }

                    if (result < 0)
                    {
                        user.RaiseMessage("");
                        return Exit.ERROR;
                    }

                    // Add the module to the list.
                    modules.Add(ex.modules[result]);
                    // DON'T return so we can loop around and try again
                }
                catch (FileExistsKraken ex)
                {
                    if (ex.owningModule != null)
                    {
                        user.RaiseError(Properties.Resources.InstallFileConflictOwned,
                            ex.filename, ex.installingModule, ex.owningModule,
                            Meta.GetVersion(VersionFormat.Full));
                    }
                    else
                    {
                        user.RaiseError(Properties.Resources.InstallFileConflictUnowned,
                            ex.installingModule, ex.filename);
                    }

                    user.RaiseMessage(Properties.Resources.InstallGamedataReturned, instance.game.PrimaryModDirectoryRelative);
                    return Exit.ERROR;
                }
                catch (InconsistentKraken ex)
                {
                    user.RaiseError("{0}", ex.Message);
                    user.RaiseMessage(Properties.Resources.InstallCancelled);
                    return Exit.ERROR;
                }
                catch (CancelledActionKraken k)
                {
                    user.RaiseError(Properties.Resources.InstallAborted, k.Message);
                    return Exit.ERROR;
                }
                catch (MissingCertificateKraken kraken)
                {
                    // Another very pretty kraken.
                    user.RaiseError("{0}", kraken.ToString());
                    return Exit.ERROR;
                }
                catch (DownloadThrottledKraken kraken)
                {
                    user.RaiseError("{0}", kraken.ToString());
                    user.RaiseMessage(Properties.Resources.InstallTryAuthToken, kraken.infoUrl);
                    return Exit.ERROR;
                }
                catch (DownloadErrorsKraken)
                {
                    user.RaiseError(Properties.Resources.InstallDownloadFailed);
                    return Exit.ERROR;
                }
                catch (ModuleDownloadErrorsKraken kraken)
                {
                    user.RaiseError("{0}", kraken.ToString());
                    return Exit.ERROR;
                }
                catch (DirectoryNotFoundKraken kraken)
                {
                    user.RaiseError("{0}", kraken.Message);
                    return Exit.ERROR;
                }
                catch (ModuleIsDLCKraken kraken)
                {
                    user.RaiseError(Properties.Resources.InstallDLC, kraken.module.name);
                    var res = kraken?.module?.resources;
                    var storePagesMsg = new Uri[] { res?.store, res?.steamstore }
                        .Where(u => u != null)
                        .Aggregate("", (a, b) => $"{a}\r\n- {b}");
                    if (!string.IsNullOrEmpty(storePagesMsg))
                    {
                        user.RaiseMessage(Properties.Resources.InstallDLCStorePage, storePagesMsg);
                    }
                    return Exit.ERROR;
                }
            }

            return Exit.OK;
        }

        private Uri getUri(string arg)
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
        public string[] ckan_files { get; set; }

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
        public List<string> modules { get; set; }
    }

}
