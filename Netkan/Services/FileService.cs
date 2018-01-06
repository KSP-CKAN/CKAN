using System;
using System.IO;
using System.Security.Cryptography;

namespace CKAN.NetKAN.Services
{
    internal sealed class FileService : IFileService
    {
        public long GetSizeBytes(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        public string GetFileHashSha1(string filePath)
        {
            // Use shared implementation from Core.
            // Also needs to be an instance method so it can be Moq'd for testing.
            return NetModuleCache.GetFileHashSha1(filePath);
        }

        public string GetFileHashSha256(string filePath)
        {
            // Use shared implementation from Core.
            // Also needs to be an instance method so it can be Moq'd for testing.
            return NetModuleCache.GetFileHashSha256(filePath);
        }

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
    }
}
