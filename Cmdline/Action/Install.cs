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
        /// <param name="ksp">Game instance into which to install</param>
        /// <param name="raw_options">Command line options object</param>
        /// <returns>
        /// Exit code for shell environment
        /// </returns>
        public int RunCommand(CKAN.GameInstance ksp, object raw_options)
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
                            user.RaiseError("Can not find file \"{0}\".", ckan_file);
                            user.RaiseError("Exiting.");
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
                        CkanModule m = MainClass.LoadCkanFromFile(ksp, filename);
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
                Search.AdjustModulesCase(ksp, options.modules);
            }

            if (options.modules.Count == 0)
            {
                // What? No files specified?
                user.RaiseMessage(
                    "Usage: ckan install [--with-suggests] [--with-all-suggests] [--no-recommends] [--headless] Mod [Mod2, ...]");
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

            RegistryManager regMgr = RegistryManager.Instance(ksp);
            List<string> modules = options.modules;

            for (bool done = false; !done; )
            {
                // Install everything requested. :)
                try
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    var installer = ModuleInstaller.GetInstance(ksp, manager.Cache, user);
                    installer.InstallList(modules, install_ops, regMgr, ref possibleConfigOnlyDirs);
                    user.RaiseMessage("");
                    done = true;
                }
                catch (DependencyNotSatisfiedKraken ex)
                {
                    user.RaiseError(ex.Message);
                    user.RaiseMessage("If you're lucky, you can do a `ckan update` and try again.");
                    user.RaiseMessage("Try `ckan install --no-recommends` to skip installation of recommended modules.");
                    user.RaiseMessage("Or `ckan install --allow-incompatible` to ignore module compatibility.");
                    return Exit.ERROR;
                }
                catch (ModuleNotFoundKraken ex)
                {
                    if (ex.version == null)
                    {
                        user.RaiseError("Module {0} required but it is not listed in the index, or not available for your version of KSP.",
                            ex.module);
                    }
                    else
                    {
                        user.RaiseError("Module {0} {1} required but it is not listed in the index, or not available for your version of KSP.",
                            ex.module, ex.version);
                    }
                    user.RaiseMessage("If you're lucky, you can do a `ckan update` and try again.");
                    user.RaiseMessage("Try `ckan install --no-recommends` to skip installation of recommended modules.");
                    user.RaiseMessage("Or `ckan install --allow-incompatible` to ignore module compatibility.");
                    return Exit.ERROR;
                }
                catch (BadMetadataKraken ex)
                {
                    user.RaiseError("Bad metadata detected for module {0}: {1}",
                        ex.module, ex.Message);
                    return Exit.ERROR;
                }
                catch (TooManyModsProvideKraken ex)
                {
                    // Request the user selects one of the mods.
                    string[] mods = new string[ex.modules.Count];

                    for (int i = 0; i < ex.modules.Count; i++)
                    {
                        mods[i] = String.Format("{0} ({1})", ex.modules[i].identifier, ex.modules[i].name);
                    }

                    string message = String.Format("Too many mods provide {0}. Please pick from the following:\r\n", ex.requested);

                    int result;

                    try
                    {
                        result = user.RaiseSelectionDialog(message, mods);
                    }
                    catch (Kraken e)
                    {
                        user.RaiseMessage(e.Message);

                        return Exit.ERROR;
                    }

                    if (result < 0)
                    {
                        user.RaiseMessage(String.Empty); // Looks tidier.

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
                        user.RaiseError(
                            "Oh no! We tried to overwrite a file owned by another mod!\r\n"+
                            "Please try a `ckan update` and try again.\r\n\r\n"+
                            "If this problem re-occurs, then it maybe a packaging bug.\r\n"+
                            "Please report it at:\r\n\r\n" +
                            "https://github.com/KSP-CKAN/NetKAN/issues/new\r\n\r\n" +
                            "Please including the following information in your report:\r\n\r\n" +
                            "File           : {0}\r\n" +
                            "Installing Mod : {1}\r\n" +
                            "Owning Mod     : {2}\r\n" +
                            "CKAN Version   : {3}\r\n",
                            ex.filename, ex.installingModule, ex.owningModule,
                            Meta.GetVersion(VersionFormat.Full)
                        );
                    }
                    else
                    {
                        user.RaiseError(
                            "Oh no!\r\n\r\n"+
                            "It looks like you're trying to install a mod which is already installed,\r\n"+
                            "or which conflicts with another mod which is already installed.\r\n\r\n"+
                            "As a safety feature, CKAN will *never* overwrite or alter a file\r\n"+
                            "that it did not install itself.\r\n\r\n"+
                            "If you wish to install {0} via CKAN,\r\n"+
                            "then please manually uninstall the mod which owns:\r\n\r\n"+
                            "{1}\r\n\r\n"+"and try again.\r\n",
                            ex.installingModule, ex.filename
                        );
                    }

                    user.RaiseMessage("Your GameData has been returned to its original state.");
                    return Exit.ERROR;
                }
                catch (InconsistentKraken ex)
                {
                    // The prettiest Kraken formats itself for us.
                    user.RaiseError(ex.InconsistenciesPretty);
                    user.RaiseMessage("Install canceled. Your files have been returned to their initial state.");
                    return Exit.ERROR;
                }
                catch (CancelledActionKraken k)
                {
                    user.RaiseError("Installation aborted: {0}", k.Message);
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
                    user.RaiseMessage("Try the authtoken command. See {0} for details.",
                        kraken.infoUrl);
                    return Exit.ERROR;
                }
                catch (DownloadErrorsKraken)
                {
                    user.RaiseError("One or more files failed to download, stopped.");
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
                    user.RaiseError("CKAN can't install expansion '{0}' for you.",
                        kraken.module.name);
                    var res = kraken?.module?.resources;
                    var storePagesMsg = new Uri[] { res?.store, res?.steamstore }
                        .Where(u => u != null)
                        .Aggregate("", (a, b) => $"{a}\r\n- {b}");
                    if (!string.IsNullOrEmpty(storePagesMsg))
                    {
                        user.RaiseMessage($"To install this expansion, purchase it from one of its store pages:\r\n{storePagesMsg}");
                    }
                    return Exit.ERROR;
                }
            }

            return Exit.OK;
        }
    }
}
