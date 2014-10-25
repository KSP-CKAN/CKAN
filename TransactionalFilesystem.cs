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

        private static readonly TxFileManager fileManager = new TxFileManager();

        /// <summary>
        /// Creates a new FilesystemTransaction object.
        /// The path provided will be used to store temporary files, and
        /// will be created if it does not already exist.
        /// </summary>
        public FilesystemTransaction(string path)
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
            ReportProgress("Committing filesystem transaction", 0);
            scope.Complete();
            ReportProgress("Done!", 100);
        }

        public void Rollback()
        {
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
