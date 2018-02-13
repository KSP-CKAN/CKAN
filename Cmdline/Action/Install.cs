using System;
using System.IO;
using log4net;

namespace CKAN.CmdLine
{
    public class Install : ICommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Install));

        public IUser user { get; set; }

        public Install(IUser user)
        {
            this.user = user;
        }

        public int RunCommand(CKAN.KSP ksp, object raw_options)
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
                        CkanModule m = LoadCkanFromFile(ksp, filename);
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

            // Install everything requested. :)
            try
            {
                var installer = ModuleInstaller.GetInstance(ksp, user);
                installer.InstallList(options.modules, install_ops);
            }
            catch (DependencyNotSatisfiedKraken ex)
            {
                if (ex.version == null)
                {
                    user.RaiseMessage("{0} requires {1} but it is not listed in the index, or not available for your version of KSP.", ex.parent, ex.module);
                }
                else
                {
                    user.RaiseMessage("{0} requires {1} {2} but it is not listed in the index, or not available for your version of KSP.", ex.parent, ex.module, ex.version);
                }
                user.RaiseMessage("If you're lucky, you can do a `ckan update` and try again.");
                user.RaiseMessage("Try `ckan install --no-recommends` to skip installation of recommended modules.");
                user.RaiseMessage("Or `ckan install --allow-incompatible` to ignore module compatibility.");
                return Exit.ERROR;
            }
            catch (ModuleNotFoundKraken ex)
            {
                if (ex.version == null)
                {
                    user.RaiseMessage("Module {0} required but it is not listed in the index, or not available for your version of KSP.", ex.module);
                }
                else
                {
                    user.RaiseMessage("Module {0} {1} required but it is not listed in the index, or not available for your version of KSP.", ex.module, ex.version);
                }
                user.RaiseMessage("If you're lucky, you can do a `ckan update` and try again.");
                user.RaiseMessage("Try `ckan install --no-recommends` to skip installation of recommended modules.");
                user.RaiseMessage("Or `ckan install --allow-incompatible` to ignore module compatibility.");
                return Exit.ERROR;
            }
            catch (BadMetadataKraken ex)
            {
                user.RaiseMessage("Bad metadata detected for module {0}.", ex.module);
                user.RaiseMessage(ex.Message);
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
                options.modules.Add(ex.modules[result].identifier);

                return (new Install(user).RunCommand(ksp, options));
            }
            catch (FileExistsKraken ex)
            {
                if (ex.owningModule != null)
                {
                    user.RaiseMessage(
                        "\r\nOh no! We tried to overwrite a file owned by another mod!\r\n"+
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
                    user.RaiseMessage(
                        "\r\n\r\nOh no!\r\n\r\n"+
                        "It looks like you're trying to install a mod which is already installed,\r\n"+
                        "or which conflicts with another mod which is already installed.\r\n\r\n"+
                        "As a safety feature, the CKAN will *never* overwrite or alter a file\r\n"+
                        "that it did not install itself.\r\n\r\n"+
                        "If you wish to install {0} via the CKAN,\r\n"+
                        "then please manually uninstall the mod which owns:\r\n\r\n"+
                        "{1}\r\n\r\n"+"and try again.\r\n",
                        ex.installingModule, ex.filename
                    );
                }

                user.RaiseMessage("Your GameData has been returned to its original state.\r\n");
                return Exit.ERROR;
            }
            catch (InconsistentKraken ex)
            {
                // The prettiest Kraken formats itself for us.
                user.RaiseMessage(ex.InconsistenciesPretty);
                user.RaiseMessage("Install canceled. Your files have been returned to their initial state.");
                return Exit.ERROR;
            }
            catch (CancelledActionKraken)
            {
                user.RaiseMessage("Installation canceled at user request.");
                return Exit.ERROR;
            }
            catch (MissingCertificateKraken kraken)
            {
                // Another very pretty kraken.
                user.RaiseMessage(kraken.ToString());
                return Exit.ERROR;
            }
            catch (DownloadThrottledKraken kraken)
            {
                user.RaiseMessage(kraken.ToString());
                user.RaiseMessage($"Try the authtoken command. See {kraken.infoUrl} for details.");
                return Exit.ERROR;
            }
            catch (DownloadErrorsKraken)
            {
                user.RaiseMessage("One or more files failed to download, stopped.");
                return Exit.ERROR;
            }
            catch (ModuleDownloadErrorsKraken kraken)
            {
                user.RaiseMessage(kraken.ToString());
                return Exit.ERROR;
            }
            catch (DirectoryNotFoundKraken kraken)
            {
                user.RaiseMessage("\r\n{0}", kraken.Message);
                return Exit.ERROR;
            }

            return Exit.OK;
        }

        internal static CkanModule LoadCkanFromFile(CKAN.KSP current_instance, string ckan_file)
        {
            CkanModule module = CkanModule.FromFile(ckan_file);

            // We'll need to make some registry changes to do this.
            RegistryManager registry_manager = RegistryManager.Instance(current_instance);

            // Remove this version of the module in the registry, if it exists.
            registry_manager.registry.RemoveAvailable(module);

            // Sneakily add our version in...
            registry_manager.registry.AddAvailable(module);

            return module;
        }
    }
}
