using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CKAN
{


    class FilesystemTransaction
    {

        public FilesystemTransaction() {
            uuid = Guid.NewGuid().ToString();
        }

        public void Commit() {
            foreach (var directory in directoriesToCreate) {
                Directory.CreateDirectory(directory);
            }

            foreach (var pair in files) {
                var file = pair.Value;
                file.Close();
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

            files = new Dictionary<string, TransactionalFile>();
        }

        public TransactionalFile OpenFile(string path) {
            if (files.ContainsKey(path))
            {
                return files[path];
            }

            files.Add(path, new TransactionalFile(path, this));
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

        private Dictionary<string, TransactionalFile> files = new Dictionary<string, TransactionalFile>();
        private List<string> filesToRemove = new List<string>();
        private List<string> directoriesToCreate = new List<string>(); 
        private List<string> directoriesToRemove = new List<string>(); 

        public string uuid = null;

    }

    class TransactionalFile {

        public TransactionalFile(string _path, FilesystemTransaction transaction) {
            path = _path;
            uuid = Guid.NewGuid().ToString();

            temporaryPath = Path.Combine(Path.Combine(KSP.GameDir(), "CKAN"), String.Format("{0}_{1}", transaction.uuid, uuid));
            temporaryStream = File.Create(temporaryPath);
        }

        public void Close() {
            temporaryStream.Close();
            temporaryStream = null;
        }

        public FileStream Stream {
            get { return temporaryStream; }
        }

        public string TemporaryPath {
            get { return temporaryPath;  }
        }

        public string path = null;
        public string uuid = null;

        private string temporaryPath = null;
        private FileStream temporaryStream = null;

    }

}
