using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace CKAN
{
    public class FilesystemTransaction
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (FilesystemTransaction));

        private static string tempPath = "temp/";
        private readonly List<string> directoriesToCreate = new List<string>();
        private readonly List<string> directoriesToRemove = new List<string>();
        private readonly List<string> filesToRemove = new List<string>();
        private Dictionary<string, TransactionalFileWriter> files = new Dictionary<string, TransactionalFileWriter>();
        public string uuid = null;

        public FilesystemTransaction()
        {
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }

            uuid = Guid.NewGuid().ToString();
        }

        public static string TempPath
        {
            get { return Path.Combine(KSP.CkanDir(), tempPath); }
        }

        public void Commit()
        {
            foreach (string directory in directoriesToCreate)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            foreach (var pair in files)
            {
                TransactionalFileWriter file = pair.Value;
                file.Close();

                // verify that all files can be copied
                if (!File.Exists(file.TemporaryPath))
                {
                    log.ErrorFormat("Commit failed because {0} is missing", file.TemporaryPath);
                    return;
                }
            }

            foreach (var pair in files)
            {
                TransactionalFileWriter file = pair.Value;

                if (File.Exists(file.path) && file.neverOverwrite)
                {
                    log.WarnFormat("Skipping \"{0}\", file exists but overwrite disabled.", file.path);
                    File.Delete(file.TemporaryPath);
                    continue;
                }

                File.Copy(file.TemporaryPath, file.path);
                File.Delete(file.TemporaryPath);
            }

            foreach (string path in filesToRemove)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            foreach (string path in directoriesToRemove)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                }
            }

            files = null;
        }

        public void Rollback()
        {
            foreach (var pair in files)
            {
                TransactionalFileWriter file = pair.Value;
                file.Close();
                File.Delete(file.TemporaryPath);
            }

            files = new Dictionary<string, TransactionalFileWriter>();
        }

        public TransactionalFileWriter OpenFileWrite(string path, bool neverOverwrite = true)
        {
            if (files.ContainsKey(path))
            {
                return files[path];
            }

            files.Add(path, new TransactionalFileWriter(path, this, neverOverwrite));
            return files[path];
        }

        public void RemoveFile(string path)
        {
            filesToRemove.Add(path);
        }

        public void CreateDirectory(string path)
        {
            directoriesToCreate.Add(path);
        }

        public void DeleteDirectory(string path)
        {
            directoriesToRemove.Add(path);
        }
    }

    public class TransactionalFileWriter
    {
        private readonly string temporaryPath;
        public bool neverOverwrite = true;
        public string path = null;
        private FileStream temporaryStream;
        public string uuid = null;

        public TransactionalFileWriter
            (
            string _path,
            FilesystemTransaction transaction,
            bool _neverOverwrite
            )
        {
            path = _path;
            uuid = Guid.NewGuid().ToString();

            temporaryPath = Path.Combine(FilesystemTransaction.TempPath,
                String.Format("{0}_{1}", transaction.uuid, uuid));
            temporaryStream = null; //File.Create(temporaryPath);
            neverOverwrite = _neverOverwrite;
        }

        public FileStream Stream
        {
            get
            {
                if (temporaryStream == null)
                {
                    temporaryStream = File.Create(temporaryPath);
                }

                return temporaryStream;
            }
        }

        public string TemporaryPath
        {
            get { return temporaryPath; }
        }

        public void Close()
        {
            if (temporaryStream != null)
            {
                temporaryStream.Close();
                temporaryStream = null;
            }
        }
    }
}