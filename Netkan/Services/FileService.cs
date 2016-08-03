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
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            using (var sha1 = new SHA1Cng())
            {
                byte[] hash = sha1.ComputeHash(bs);

                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public string GetFileHashSha256(string filePath)
        {
            using (FileStream fs = new FileStream(@filePath, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            using (var sha256 = new SHA256Managed())
            {
                byte[] hash = sha256.ComputeHash(bs);

                return BitConverter.ToString(hash).Replace("-", "");
            }
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
