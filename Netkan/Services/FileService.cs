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
            using (FileStream fs = new FileStream(@filePath, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            using (var sha1 = new SHA1Managed())
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
    }
}
