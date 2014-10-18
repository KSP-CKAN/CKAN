using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace CKAN
{

    public class FilesystemTransaction
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(FilesystemTransaction));

        private static string tempPath = "temp/";

        public static string TempPath
        {
            get { return Path.Combine(KSP.CkanDir(), tempPath); }
        }

        public FilesystemTransaction() {
            if (!Directory.Exists(TempPath)) {
                Directory.CreateDirectory(TempPath);
            }

            uuid = Guid.NewGuid().ToString();
        }

        public void Commit() {
            foreach (var directory in directoriesToCreate) {
                Directory.CreateDirectory(directory);
            }

            foreach (var pair in files) {
                var file = pair.Value;
                file.Close();

                if (System.IO.File.Exists(file.path) && file.neverOverwrite)
                {
                    log.WarnFormat("Skipping \"{0}\", file exists but overwrite disabled.");
                    File.Delete(file.TemporaryPath);
                    continue;
                }

                File.Copy(file.TemporaryPath, file.path);
                File.Delete(file.TemporaryPath);
            }

            foreach (var path in filesToRemove) {
                File.Delete(path);
            }

            foreach (var path in directoriesToRemove) {
                Directory.Delete(path);
            }

            files = null;
        }

        public void Rollback() {
            foreach (var pair in files)  {
                var file = pair.Value;
                file.Close();
                File.Delete(file.TemporaryPath);
            }

            files = new Dictionary<string, TransactionalFileWriter>();
        }

        public TransactionalFileWriter OpenFileWrite(string path, bool neverOverwrite = true) {
            if (files.ContainsKey(path))
            {
                return files[path];
            }

            files.Add(path, new TransactionalFileWriter(path, this, neverOverwrite));
            return files[path];
        }

        public void RemoveFile(string path) {
            filesToRemove.Add(path);
        }

        public void CreateDirectory(string path) {
            directoriesToCreate.Add(path);
        }

        public void DeleteDirectory(string path) {
            directoriesToRemove.Add(path);
        }

        private Dictionary<string, TransactionalFileWriter> files = new Dictionary<string, TransactionalFileWriter>();
        private List<string> filesToRemove = new List<string>();
        private List<string> directoriesToCreate = new List<string>(); 
        private List<string> directoriesToRemove = new List<string>(); 

        public string uuid = null;

    }

    public class TransactionalFileWriter {

        public TransactionalFileWriter
        (
            string _path,
            FilesystemTransaction transaction,
            bool _neverOverwrite
        ) {
            path = _path;
            uuid = Guid.NewGuid().ToString();

            temporaryPath = Path.Combine(FilesystemTransaction.TempPath, String.Format("{0}_{1}", transaction.uuid, uuid));
            temporaryStream = null;//File.Create(temporaryPath);
            neverOverwrite = _neverOverwrite;
        }

        public void Close() {
            if (temporaryStream != null)
            {
                temporaryStream.Close();
                temporaryStream = null;
            }
        }

        public FileStream Stream {
            get
            {
                if (temporaryStream == null) {
                    temporaryStream = File.Create(temporaryPath);
                }

                return temporaryStream;
            }
        }

        public string TemporaryPath {
            get { return temporaryPath;  }
        }

        public string path = null;
        public string uuid = null;
        public bool neverOverwrite = true;

        private string temporaryPath = null;
        private FileStream temporaryStream = null;

    }

}
