using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace CKAN
{

    public delegate void FilesystemTransactionProgressReport(string message, int percent);

    public class FilesystemTransaction
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (FilesystemTransaction));

        private string TempPath;
        private readonly List<string> directoriesToCreate = new List<string>();
        private readonly List<string> directoriesToRemove = new List<string>();
        private readonly List<string> filesToRemove = new List<string>();
        private Dictionary<string, TransactionalFileWriter> files = new Dictionary<string, TransactionalFileWriter>();
        public string uuid = null;
        public FilesystemTransactionProgressReport onProgressReport = null;

        /// <summary>
        /// Creates a new FilesystemTransaction object.
        /// The path provided will be used to store temporary files, and
        /// will be created if it does not already exist.
        /// </summary>
        public FilesystemTransaction(string path)
        {
            TempPath = path;
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }

            uuid = Guid.NewGuid().ToString();
        }

        private void ReportProgress(string message, int percent)
        {
            if (onProgressReport != null)
            {
                onProgressReport(message, percent);
            }
        }

        public void Commit()
        {
            ReportProgress("Creating directories", 0);

            int i = 0;
            int count = directoriesToCreate.Count;

            foreach (string directory in directoriesToCreate)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                ReportProgress("Creating directories", (i * 100) / count);

                i++;
            }

            ReportProgress("Validating files", 0);
            i = 0;
            count = files.Count;

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

                if (!file.neverOverwrite)
                {
                    File.Open(file.path, FileMode.Create).Close();
                    File.Delete(file.path);
                }

                ReportProgress("Validating files", (i * 100) / count);

                i++;
            }

            ReportProgress("Moving files", 0);
            i = 0;

            foreach (var pair in files)
            {
                TransactionalFileWriter file = pair.Value;

                bool fileExists = File.Exists(file.path);
                if (fileExists && file.neverOverwrite)
                {
                    log.WarnFormat("Skipping \"{0}\", file exists but overwrite disabled.", file.path);
                    File.Delete(file.TemporaryPath);
                    continue;
                }
                else if (fileExists)
                {
                    File.Delete(file.path);
                }

                File.Move(file.TemporaryPath, file.path);
                ReportProgress("Moving files", (i * 100) / count);

                i++;
            }

            ReportProgress("Removing files", 0);
            i = 0;

            foreach (string path in filesToRemove)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                ReportProgress("Removing files", (i * 100) / count);
                i++;
            }

            ReportProgress("Removing directories", 0);
            i = 0;

            foreach (string path in directoriesToRemove)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                }

                ReportProgress("Removing directories", (i * 100) / count);
                i++;
            }

            ReportProgress("Done!", 100);
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

        public string GetTempPath()
        {
            return TempPath;
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

            temporaryPath = Path.Combine(transaction.GetTempPath(),
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