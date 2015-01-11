using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                    log.InfoFormat("Installing from CKAN file {0}", ckan_file);
                    options.modules.Add(LoadCkanFromFile(ksp, ckan_file).identifier);
                }
                // At times RunCommand() calls itself recursively - in this case we do
                // not want to be doing this again, so "consume" the option
                options.ckan_files = null;
            }

            if (options.modules.Count == 0)
            {
                // What? No files specified?
                user.RaiseMessage(
                    "Usage: ckan install [--with-suggests] [--with-all-suggests] [--no-recommends] Mod [Mod2, ...]");
                return Exit.BADOPT;
            }

            // Prepare options. Can these all be done in the new() somehow?
            var install_ops = new RelationshipResolverOptions
            {
                with_all_suggests = options.with_all_suggests,
                with_suggests = options.with_suggests,
                with_recommends = !options.no_recommends
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
            catch (ModuleNotFoundKraken ex)
            {
                user.RaiseMessage("Module {0} required, but not listed in index, or not available for your version of KSP", ex.module);
                user.RaiseMessage("If you're lucky, you can do a `ckan update` and try again.");
                user.RaiseMessage("Try `ckan install --no-recommends` to skip installation of recommended modules");
                return Exit.ERROR;
            }
            catch (BadMetadataKraken ex)
            {
                user.RaiseMessage("Bad metadata detected for module {0}", ex.module);
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

                string message = String.Format("Too many mods provide {0}. Please pick from the following:\n", ex.requested);

                int result = -1;

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
                if (ex.owning_module != null)
                {
                    user.RaiseMessage(
                        "\nOh no! We tried to overwrite a file owned by another mod!\n"+
                        "Please try a `ckan update` and try again.\n\n"+
                        "If this problem re-occurs, then it maybe a packaging bug.\n"+
                        "Please report it at:\n\n" +
                        "https://github.com/KSP-CKAN/CKAN-meta/issues/new\n\n"+
                        "Please including the following information in your report:\n\n" +
                        "File           : {0}\n" +
                        "Installing Mod : {1}\n" +
                        "Owning Mod     : {2}\n" +
                        "CKAN Version   : {3}\n",
                        ex.filename, ex.installing_module, ex.owning_module,
                        Meta.Version()
                    );
                }
                else
                {
                    user.RaiseMessage(
                        "\n\nOh no!\n\n"+
                        "It looks like you're trying to install a mod which is already installed,\n"+
                        "or which conflicts with another mod which is already installed.\n\n"+
                        "As a safety feature, the CKAN will *never* overwrite or alter a file\n"+
                        "that it did not install itself.\n\n"+
                        "If you wish to install {0} via the CKAN,\n"+
                        "then please manually uninstall the mod which owns:\n\n"+
                        "{1}\n\n"+"and try again.\n",
                        ex.installing_module, ex.filename
                    );
                }

                user.RaiseMessage("Your GameData has been returned to its original state.\n");
                return Exit.ERROR;
            }
            catch (InconsistentKraken ex)
            {
                // The prettiest Kraken formats itself for us.
                user.RaiseMessage(ex.InconsistenciesPretty);
                return Exit.ERROR;
            }
            catch (CancelledActionKraken)
            {
                user.RaiseMessage("Installation cancelled at user request.");
                return Exit.ERROR;
            }
            catch (MissingCertificateKraken kraken)
            {
                // Another very pretty kraken.
                user.RaiseMessage(kraken.ToString());
                return Exit.ERROR;
            }
            catch (DownloadErrorsKraken)
            {
                user.RaiseMessage("One or more files failed to download, stopped.");
                return Exit.ERROR;
            }
            catch (DirectoryNotFoundKraken kraken)
            {
                user.RaiseMessage("\n{0}", kraken.Message);
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

