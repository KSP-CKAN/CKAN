using System;
using System.IO;

namespace CKAN.NetKAN.Services
{
    internal sealed class FileService : IFileService
    {
        public FileService(NetFileCache cache)
        {
            this.cache = cache;
        }

        public long GetSizeBytes(string filePath)
            => new FileInfo(filePath).Length;

        public string GetFileHashSha1(string filePath)
        // Use shared implementation from Core.
        // Also needs to be an instance method so it can be Moq'd for testing.
            => cache.GetFileHashSha1(filePath, new Progress<int>(percent => {}));

        public string GetFileHashSha256(string filePath)
        // Use shared implementation from Core.
        // Also needs to be an instance method so it can be Moq'd for testing.
            => cache.GetFileHashSha256(filePath, new Progress<int>(percent => {}));

        public string GetMimetype(string filePath)
        {
            string mimetype;

            switch (FileIdentifier.IdentifyFile(filePath))
            {
                case FileType.ASCII:
                    mimetype = "text/plain";
                    break;
                case FileType.GZip:
                    mimetype = "application/x-gzip";
                    break;
                case FileType.Tar:
                    mimetype = "application/x-tar";
                    break;
                case FileType.TarGz:
                    mimetype = "application/x-compressed-tar";
                    break;
                case FileType.Zip:
                    mimetype = "application/zip";
                    break;
                default:
                    mimetype = "application/octet-stream";
                    break;
            }

            return mimetype;
        }

        private readonly NetFileCache cache;
    }
}
