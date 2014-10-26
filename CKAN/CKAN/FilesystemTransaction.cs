using System;
using System.Collections.Generic;
using System.IO;
using System.Transactions;
using ChinhDo.Transactions;
using log4net;

namespace CKAN
{

    public delegate void FilesystemTransactionProgressReport(string message, int percent);

    public class FilesystemTransaction
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (FilesystemTransaction));

        public FilesystemTransactionProgressReport onProgressReport = null;
        private TransactionScope scope = new TransactionScope();
        private Dictionary<string, string> TempFiles = new Dictionary<string, string>();

        private TxFileManager fileManager = new TxFileManager();

        /// <summary>
        /// Creates a new FilesystemTransaction object.
        /// The path provided will be used to store temporary files, and
        /// will be created if it does not already exist.
        /// </summary>
        public FilesystemTransaction()
        {
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
            if (scope == null)
            {
                log.ErrorFormat("Trying to commit a transaction twice or transaction was rolled-back");
                return;
            }

            ReportProgress("Committing filesystem transaction", 0);

            int count = 0;
            foreach (var pair in TempFiles)
            {
                var targetPath = pair.Key;
                var tempPath = pair.Value;

                fileManager.Move(tempPath, targetPath);
                ReportProgress("Moving files", (count * 100) / TempFiles.Count);
                count++;
            }

            TempFiles.Clear();
            
            scope.Complete();
            ReportProgress("Done!", 100);

            scope.Dispose();
            scope = null;
        }

        public void Rollback()
        {
            if (scope == null)
            {
                log.ErrorFormat("Trying to rollback a transaction twice or transaction already committed");
            }

            scope.Dispose();
            scope = null;
        }

        public void Snapshot(string path)
        {
            fileManager.Snapshot(path);
        }

        public FileStream OpenFileWrite(string path, bool neverOverwrite = true)
        {
            if (TempFiles.ContainsKey(path))
            {
                return File.OpenWrite(TempFiles[path]);
            }

            var tempFilename = fileManager.GetTempFileName();
            TempFiles.Add(path, tempFilename);
            return File.Create(tempFilename);
        }

        public void RemoveFile(string path)
        {
            fileManager.Delete(path);
        }

        public void CreateDirectory(string path)
        {
            fileManager.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            fileManager.DeleteDirectory(path);
        }
    }

}
