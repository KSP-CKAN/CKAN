using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using log4net;

namespace CKAN.CmdLine.Action
{
    /// <summary>
    /// Class for managing the importing of mods.
    /// </summary>
    public class Import : ICommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Import));

        private readonly GameInstanceManager _manager;
        private readonly IUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.Action.Import"/> class.
        /// </summary>
        /// <param name="manager">The manager to provide game instances.</param>
        /// <param name="user">The current <see cref="CKAN.IUser"/> to raise messages to the user.</param>
        public Import(GameInstanceManager manager, IUser user)
        {
            _manager = manager;
            _user = user;
        }

        /// <summary>
        /// Run the 'import' command.
        /// </summary>
        /// <inheritdoc cref="ICommand.RunCommand"/>
        public int RunCommand(CKAN.GameInstance inst, object args)
        {
            var opts = (ImportOptions)args;
            if (!opts.Paths.Any())
            {
                _user.RaiseMessage("import <path> [<path2> ...] - argument(s) missing, perhaps you forgot it?");
                return Exit.BadOpt;
            }

            var toImport = GetFiles(opts);
            try
            {
                Log.InfoFormat("Importing {0} files...", toImport.Count);
                var toInstall = new List<string>();
                var regMgr = RegistryManager.Instance(inst);
                var installer = new ModuleInstaller(inst, _manager.Cache, _user);

                installer.ImportFiles(toImport, _user, mod => toInstall.Add(mod.identifier), regMgr.registry, !opts.Headless);

                HashSet<string> possibleConfigOnlyDirs = null;
                if (toInstall.Count > 0)
                {
                    installer.InstallList(
                        toInstall,
                        new RelationshipResolverOptions(),
                        regMgr,
                        ref possibleConfigOnlyDirs
                    );
                }
            }
            catch (Exception ex)
            {
                _user.RaiseError("Import error: {0}", ex.Message);
                return Exit.Error;
            }

            _user.RaiseMessage("Successfully imported {0} files.", toImport.Count);
            return Exit.Ok;
        }

        private HashSet<FileInfo> GetFiles(ImportOptions options)
        {
            var files = new HashSet<FileInfo>();
            foreach (var fileName in options.Paths)
            {
                if (Directory.Exists(fileName))
                {
                    // Import everything in this folder
                    Log.InfoFormat("{0} is a directory. Adding contents...", fileName);
                    foreach (var dirFile in Directory.EnumerateFiles(fileName))
                    {
                        AddFile(files, dirFile);
                    }
                }
                else
                {
                    AddFile(files, fileName);
                }
            }

            return files;
        }

        private void AddFile(HashSet<FileInfo> files, string fileName)
        {
            if (File.Exists(fileName))
            {
                Log.InfoFormat("Attempting import of \"{0}\".", fileName);
                files.Add(new FileInfo(fileName));
            }
            else
            {
                _user.RaiseMessage("File not found: \"{0}\".", fileName);
            }
        }
    }

    [Verb("import", HelpText = "Import manually downloaded mods")]
    internal class ImportOptions : InstanceSpecificOptions
    {
        [Value(0, MetaName = "File path(s)", HelpText = "The path(s) of the files to import (can also be a directory)")]
        public IEnumerable<string> Paths { get; set; }
    }
}
