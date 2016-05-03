using System;
using System.IO;
using System.Security.Cryptography;

namespace CKAN.NetKAN.Services
{
    internal sealed class FileHash : IFileHash
    {
        public string GetFileHash(string filePath)
        {
            using (FileStream fs = new FileStream(@filePath, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            using (var sha1 = new SHA1Managed())
            {
                byte[] hash = sha1.ComputeHash(bs);

                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
