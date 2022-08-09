using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CKAN.CmdLine
{
    public class Install : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Install));

        public IUser user { get; set; }
        private GameInstanceManager manager;

        /// <summary>
        /// Initialize the install command object
        /// </summary>
        /// <param name="mgr">GameInstanceManager containing our instances</param>
        /// <param name="user">IUser object for interaction</param>
        public Install(GameInstanceManager mgr, IUser user)
        {
            manager   = mgr;
            this.user = user;
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
            InstallOptions options = (InstallOptions) raw_options;

            if (options.ckan_files != null)
            {
                // Oooh! We're installing from a CKAN file.
                foreach (string ckan_file in options.ckan_files)
                {
                    Uri ckan_uri;

                    // Check if the argument if a wellformatted Uri.
                    if (!Uri.IsWellFormedUriString(ckan_file, UriKind.Absolute))
                    {
                        // Assume it is a local file, check if the file exists.
                        if (File.Exists(ckan_file))
                        {
                            // Get the full path of the file.
                            ckan_uri = new Uri(Path.GetFullPath(ckan_file));
                        }
                        else
                        {
                            // We have no further ideas as what we can do with this Uri, tell the user.
                            user.RaiseError(Properties.Resources.InstallNotFound, ckan_file);
                            return Exit.ERROR;
                        }
                    }
                    else
                    {
                        ckan_uri = new Uri(ckan_file);
                    }

                    string filename = String.Empty;

                    // If it is a local file, we already know the filename. If it is remote, create a temporary file and download the remote resource.
                    if (ckan_uri.IsFile)
                    {
                        filename = ckan_uri.LocalPath;
                        log.InfoFormat("Installing from local CKAN file \"{0}\"", filename);
                    }
                    else
                    {
                        log.InfoFormat("Installing from remote CKAN file \"{0}\"", ckan_uri);
                        filename = Net.Download(ckan_uri, null, user);

                        log.DebugFormat("Temporary file for \"{0}\" is at \"{1}\".", ckan_uri, filename);
                    }

                    // Parse the JSON file.
                    try
                    {
                        CkanModule m = MainClass.LoadCkanFromFile(instance, filename);
                        options.modules.Add($"{m.identifier}={m.version}");
                    }
                    catch (Kraken kraken)
                    {
                        user.RaiseError(kraken.InnerException == null
                            ? kraken.Message
                            : $"{kraken.Message}: {kraken.InnerException.Message}");
                    }
                }

                // At times RunCommand() calls itself recursively - in this case we do
                // not want to be doing this again, so "consume" the option
                options.ckan_files = null;
            }
            else
            {
                Search.AdjustModulesCase(instance, options.modules);
            }

            if (options.modules.Count == 0)
            {
                // What? No files specified?
                user.RaiseMessage(
                    $"{Properties.Resources.Usage}: ckan install [--with-suggests] [--with-all-suggests] [--no-recommends] [--headless] Mod [Mod2, ...]");
                return Exit.BADOPT;
            }

            // Prepare options. Can these all be done in the new() somehow?
            var install_ops = new RelationshipResolverOptions
            {
                with_all_suggests  = options.with_all_suggests,
                with_suggests      = options.with_suggests,
                with_recommends    = !options.no_recommends,
                allow_incompatible = options.allow_incompatible
            };

            if (user.Headless)
            {
                install_ops.without_toomanyprovides_kraken = true;
                install_ops.without_enforce_consistency = true;
            }

            RegistryManager regMgr = RegistryManager.Instance(instance);
            List<string> modules = options.modules;

            for (bool done = false; !done; )
            {
                // Install everything requested. :)
                try
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    var installer = new ModuleInstaller(instance, manager.Cache, user);
                    installer.InstallList(modules, install_ops, regMgr, ref possibleConfigOnlyDirs);
                    user.RaiseMessage("");
                    done = true;
                }
                catch (DependencyNotSatisfiedKraken ex)
                {
                    user.RaiseError(ex.Message);
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
                    modules.Add($"{ex.modules[result].identifier}={ex.modules[result].version}");
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
                    // The prettiest Kraken formats itself for us.
                    user.RaiseError(ex.InconsistenciesPretty);
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
                    user.RaiseError(kraken.ToString());
                    return Exit.ERROR;
                }
                catch (DownloadThrottledKraken kraken)
                {
                    user.RaiseError(kraken.ToString());
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
                    user.RaiseError(kraken.ToString());
                    return Exit.ERROR;
                }
                catch (DirectoryNotFoundKraken kraken)
                {
                    user.RaiseError(kraken.Message);
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
    }
}
