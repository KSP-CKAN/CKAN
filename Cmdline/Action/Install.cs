using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using log4net;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing the installations of mods.
    /// </summary>
    public class Install : ICommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Install));

        private readonly GameInstanceManager _manager;
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Install"/> class.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Install(GameInstanceManager manager, IUser user)
        {
            _manager = manager;
            _user = user;
        }

        /// <summary>
        /// Run the 'install' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (InstallOptions)args;
            if (!opts.Mods.Any())
            {
                _user.RaiseMessage("install <mod> [<mod2> ...] - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            if (opts.CkanFiles.Any())
            {
                // Ooh! We're installing from a CKAN file
                foreach (var ckanFile in opts.CkanFiles)
                {
                    Uri ckanUri;

                    // Check if the argument is a well formatted Uri
                    if (!Uri.IsWellFormedUriString(ckanFile, UriKind.Absolute))
                    {
                        // Assume it's a local file, check if the file exists
                        if (File.Exists(ckanFile))
                        {
                            // Get the full path of the file
                            ckanUri = new Uri(Path.GetFullPath(ckanFile));
                        }
                        else
                        {
                            // We have no further ideas as what we can do with this Uri, tell the user
                            _user.RaiseError("Can't find file \"{0}\".\r\nExiting.", ckanFile);
                            return Exit.Error;
                        }
                    }
                    else
                    {
                        ckanUri = new Uri(ckanFile);
                    }

                    string fileName;

                    // If it's a local file, we already know the filename. If it's remote, create a temporary file and download the remote resource
                    if (ckanUri.IsFile)
                    {
                        fileName = ckanUri.LocalPath;
                        Log.InfoFormat("Installing from local CKAN file \"{0}\".", fileName);
                    }
                    else
                    {
                        Log.InfoFormat("Installing from remote CKAN file \"{0}\".", ckanUri);
                        fileName = Net.Download(ckanUri, null, _user);

                        Log.DebugFormat("Temporary file for \"{0}\" is at \"{1}\".", ckanUri, fileName);
                    }

                    // Parse the JSON file
                    try
                    {
                        var m = MainClass.LoadCkanFromFile(inst, fileName);
                        opts.Mods.ToList().Add(string.Format("{0}={1}", m.identifier, m.version));
                    }
                    catch (Kraken kraken)
                    {
                        _user.RaiseError(kraken.InnerException == null
                            ? kraken.Message
                            : string.Format("{0}: {1}", kraken.Message, kraken.InnerException.Message));
                    }
                }

                // At times RunCommand() calls itself recursively - in this case we do
                // not want to be doing this again, so "consume" the option
                opts.CkanFiles = null;
            }
            else
            {
                Search.AdjustModulesCase(inst, opts.Mods.ToList());
            }

            // Prepare options. Can these all be done in the new() somehow?
            var installOpts = new RelationshipResolverOptions
            {
                with_all_suggests = opts.WithAllSuggests,
                with_suggests = opts.WithSuggests,
                with_recommends = !opts.NoRecommends,
                allow_incompatible = opts.AllowIncompatible
            };

            if (_user.Headless)
            {
                installOpts.without_toomanyprovides_kraken = true;
                installOpts.without_enforce_consistency = true;
            }

            var regMgr = RegistryManager.Instance(inst);
            var modules = opts.Mods.ToList();

            for (var done = false; !done;)
            {
                // Install everything requested
                try
                {
                    HashSet<string> possibleConfigOnlyDirs = null;
                    var installer = new ModuleInstaller(inst, _manager.Cache, _user);
                    installer.InstallList(modules, installOpts, regMgr, ref possibleConfigOnlyDirs);
                    _user.RaiseMessage("");
                    done = true;
                }
                catch (DependencyNotSatisfiedKraken kraken)
                {
                    _user.RaiseError(kraken.Message);
                    _user.RaiseMessage("If you're lucky, you can do a 'ckan update' and try again.");
                    _user.RaiseMessage("Try 'ckan install --no-recommends' to skip installation of recommended mods.");
                    _user.RaiseMessage("Or 'ckan install --allow-incompatible' to ignore mod compatibility.");
                    return Exit.Error;
                }
                catch (ModuleNotFoundKraken kraken)
                {
                    if (kraken.version == null)
                    {
                        _user.RaiseError("The mod \"{0}\" is required but it is not listed in the index, or not available for your version of {1}.", kraken.module, inst.game.ShortName);
                    }
                    else
                    {
                        _user.RaiseError("The mod \"{0}\" {1} is required but it is not listed in the index, or not available for your version of {2}.", kraken.module, kraken.version, inst.game.ShortName);
                    }

                    _user.RaiseMessage("If you're lucky, you can do a 'ckan update' and try again.");
                    _user.RaiseMessage("Try 'ckan install --no-recommends' to skip installation of recommended mods.");
                    _user.RaiseMessage("Or 'ckan install --allow-incompatible' to ignore mod compatibility.");
                    return Exit.Error;
                }
                catch (BadMetadataKraken kraken)
                {
                    _user.RaiseError("Bad metadata detected for mod {0}.\r\n{1}", kraken.module, kraken.Message);
                    return Exit.Error;
                }
                catch (TooManyModsProvideKraken kraken)
                {
                    // Request the user to select one of the mods
                    var mods = new string[kraken.modules.Count];

                    for (var i = 0; i < kraken.modules.Count; i++)
                    {
                        mods[i] = string.Format("{0} ({1})", kraken.modules[i].identifier, kraken.modules[i].name);
                    }

                    var message = string.Format("Too many mods provide \"{0}\". Please pick one from the following mods:\r\n", kraken.requested);

                    int result;
                    try
                    {
                        result = _user.RaiseSelectionDialog(message, mods);
                    }
                    catch (Kraken k)
                    {
                        _user.RaiseMessage(k.Message);
                        return Exit.Error;
                    }

                    if (result < 0)
                    {
                        _user.RaiseMessage(string.Empty); // Looks tidier
                        return Exit.Error;
                    }

                    // Add the module to the list
                    modules.Add(string.Format("{0}={1}",
                        kraken.modules[result].identifier,
                        kraken.modules[result].version));
                }
                catch (FileExistsKraken kraken)
                {
                    if (kraken.owningModule != null)
                    {
                        _user.RaiseError(
                            "Oh no! We tried to overwrite a file owned by another mod!\r\n" +
                            "Please try a 'ckan update' and try again.\r\n\r\n" +
                            "If this problem re-occurs, then it may be a packaging bug.\r\n" +
                            "Please report it at:\r\n\r\n" +
                            "https://github.com/KSP-CKAN/NetKAN/issues/new/choose\r\n\r\n" +
                            "Please include the following information in your report:\r\n\r\n" +
                            "File           : {0}\r\n" +
                            "Installing Mod : {1}\r\n" +
                            "Owning Mod     : {2}\r\n" +
                            "CKAN Version   : {3}\r\n",
                            kraken.filename, kraken.installingModule, kraken.owningModule,
                            Meta.GetVersion(VersionFormat.Full)
                        );
                    }
                    else
                    {
                        _user.RaiseError(
                            "Oh no!\r\n\r\n" +
                            "It looks like you're trying to install a mod which is already installed,\r\n" +
                            "or which conflicts with another mod which is already installed.\r\n\r\n" +
                            "As a safety feature, CKAN will *never* overwrite or alter a file\r\n" +
                            "that it did not install itself.\r\n\r\n" +
                            "If you wish to install {0} via CKAN,\r\n" +
                            "then please manually uninstall the mod which owns:\r\n\r\n" +
                            "{1}\r\n\r\n" +
                            "and try again.\r\n",
                            kraken.installingModule, kraken.filename
                        );
                    }

                    _user.RaiseMessage("Your GameData has been returned to its original state.");
                    return Exit.Error;
                }
                catch (InconsistentKraken kraken)
                {
                    // The prettiest Kraken formats itself for us
                    _user.RaiseError(kraken.InconsistenciesPretty);
                    _user.RaiseMessage("Install canceled. Your files have been returned to their initial state.");
                    return Exit.Error;
                }
                catch (CancelledActionKraken kraken)
                {
                    _user.RaiseError("Installation aborted: {0}", kraken.Message);
                    return Exit.Error;
                }
                catch (MissingCertificateKraken kraken)
                {
                    // Another very pretty kraken
                    _user.RaiseError(kraken.ToString());
                    return Exit.Error;
                }
                catch (DownloadThrottledKraken kraken)
                {
                    _user.RaiseError(kraken.ToString());
                    _user.RaiseMessage("Try the authtoken command. See \"{0}\" for more details.", kraken.infoUrl);
                    return Exit.Error;
                }
                catch (DownloadErrorsKraken)
                {
                    _user.RaiseError("One or more files failed to download, stopped.");
                    return Exit.Error;
                }
                catch (ModuleDownloadErrorsKraken kraken)
                {
                    _user.RaiseError(kraken.ToString());
                    return Exit.Error;
                }
                catch (DirectoryNotFoundKraken kraken)
                {
                    _user.RaiseError("\r\n{0}", kraken.Message);
                    return Exit.Error;
                }
                catch (ModuleIsDLCKraken kraken)
                {
                    _user.RaiseError("Can't install the expansion \"{0}\".", kraken.module.name);
                    var res = kraken.module?.resources;
                    var storePagesMsg = new[] { res?.store, res?.steamstore }
                        .Where(u => u != null)
                        .Aggregate("", (a, b) => $"{a}\r\n- {b}");

                    if (!string.IsNullOrEmpty(storePagesMsg))
                    {
                        _user.RaiseMessage("To install this expansion, purchase it from one of its store pages:\r\n   {0}", storePagesMsg);
                    }

                    return Exit.Error;
                }
            }

            _user.RaiseMessage("Successfully installed requested mods.");
            return Exit.Ok;
        }
    }

    [Verb("install", HelpText = "Install a mod")]
    internal class InstallOptions : InstanceSpecificOptions
    {
        [Option('c', "ckanfiles", HelpText = "Local CKAN files to process")]
        public IEnumerable<string> CkanFiles { get; set; }

        [Option("no-recommends", HelpText = "Do not install recommended mods")]
        public bool NoRecommends { get; set; }

        [Option("with-suggests", HelpText = "Install suggested mods")]
        public bool WithSuggests { get; set; }

        [Option("with-all-suggests", HelpText = "Install suggested mods all the way down")]
        public bool WithAllSuggests { get; set; }

        [Option("allow-incompatible", HelpText = "Install mods that are not compatible with the current game version")]
        public bool AllowIncompatible { get; set; }

        [Value(0, MetaName = "Mod name(s)", HelpText = "The mod name(s) to install")]
        public IEnumerable<string> Mods { get; set; }
    }
}
