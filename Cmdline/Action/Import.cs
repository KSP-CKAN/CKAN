using System;
using System.IO;
using System.Collections.Generic;
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
        public Import(IUser user)
        {
            this.user = user;
        }

        /// <summary>
        /// Execute an import command
        /// </summary>
        /// <param name="ksp">Game instance into which to import</param>
        /// <param name="options">Command line parameters from the user</param>
        /// <returns>
        /// Process exit code
        /// </returns>
        public int RunCommand(CKAN.KSP ksp, object options)
        {
            try
            {
                ImportOptions opts = options as ImportOptions;
                HashSet<FileInfo> toImport = GetFiles(opts);
                if (toImport.Count < 1)
                {
                    user.RaiseMessage("Usage: ckan import path [path2, ...]");
                    return Exit.ERROR;
                }
                else
                {
                    log.InfoFormat("Importing {0} files", toImport.Count);
                    List<string>    toInstall = new List<string>();
                    ModuleInstaller inst      = ModuleInstaller.GetInstance(ksp, user);
                    inst.ImportFiles(toImport, user, id => toInstall.Add(id), !opts.Headless);
                    if (toInstall.Count > 0)
                    {
                        inst.InstallList(
                            toInstall,
                            new RelationshipResolverOptions()
                        );
                    }
                    return Exit.OK;
                }
            }
            catch (Exception ex)
            {
                user.RaiseError("Import error: {0}", ex.Message);
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
                user.RaiseMessage("File not found: {0}", filename);
            }
        }

        private        readonly IUser user;
        private static readonly ILog  log = LogManager.GetLogger(typeof(Import));
    }

}
