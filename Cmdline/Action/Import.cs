using System;
using System.IO;
using System.Collections.Generic;

using CommandLine;
using log4net;

namespace CKAN.CmdLine
{
    /// <summary>
    /// Handler for "ckan import" command.
    /// Imports manually downloaded ZIP files into the cache.
    /// </summary>
    public class Import : ICommand
    {
        /// <summary>
        /// Initialize the command
        /// </summary>
        /// <param name="user">IUser object for user interaction</param>
        public Import(GameInstanceManager mgr, RepositoryDataManager repoData, IUser user)
        {
            manager       = mgr;
            this.repoData = repoData;
            this.user     = user;
        }

        /// <summary>
        /// Execute an import command
        /// </summary>
        /// <param name="ksp">Game instance into which to import</param>
        /// <param name="options">Command line parameters from the user</param>
        /// <returns>
        /// Process exit code
        /// </returns>
        public int RunCommand(CKAN.GameInstance instance, object options)
        {
            try
            {
                ImportOptions opts = options as ImportOptions;
                HashSet<FileInfo> toImport = GetFiles(opts);
                if (toImport.Count < 1)
                {
                    user.RaiseError(Properties.Resources.ArgumentMissing);
                    foreach (var h in Actions.GetHelp("import"))
                    {
                        user.RaiseError(h);
                    }
                    return Exit.ERROR;
                }
                else
                {
                    log.InfoFormat("Importing {0} files", toImport.Count);
                    var toInstall = new List<CkanModule>();
                    var installer = new ModuleInstaller(instance, manager.Cache, user);
                    var regMgr    = RegistryManager.Instance(instance, repoData);
                    installer.ImportFiles(toImport, user, mod => toInstall.Add(mod), regMgr.registry, !opts.Headless);
                    HashSet<string> possibleConfigOnlyDirs = null;
                    if (toInstall.Count > 0)
                    {
                        installer.InstallList(toInstall,
                                              new RelationshipResolverOptions(),
                                              regMgr,
                                              ref possibleConfigOnlyDirs);
                    }
                    return Exit.OK;
                }
            }
            catch (Exception ex)
            {
                user.RaiseError(Properties.Resources.ImportError, ex.Message);
                return Exit.ERROR;
            }
        }

        private HashSet<FileInfo> GetFiles(ImportOptions options)
        {
            HashSet<FileInfo> files = new HashSet<FileInfo>();
            foreach (string filename in options.paths)
            {
                if (Directory.Exists(filename))
                {
                    // Import everything in this folder
                    log.InfoFormat("{0} is a directory", filename);
                    foreach (string dirfile in Directory.EnumerateFiles(filename))
                    {
                        AddFile(files, dirfile);
                    }
                }
                else
                {
                    AddFile(files, filename);
                }
            }
            return files;
        }

        private void AddFile(HashSet<FileInfo> files, string filename)
        {
            if (File.Exists(filename))
            {
                log.InfoFormat("Attempting import of {0}", filename);
                files.Add(new FileInfo(filename));
            }
            else
            {
                user.RaiseMessage(Properties.Resources.ImportNotFound, filename);
            }
        }

        private        readonly GameInstanceManager   manager;
        private        readonly RepositoryDataManager repoData;
        private        readonly IUser                 user;

        private static readonly ILog                  log = LogManager.GetLogger(typeof(Import));
    }

    internal class ImportOptions : InstanceSpecificOptions
    {
        [ValueList(typeof(List<string>))]
        public List<string> paths { get; set; }
    }

}
