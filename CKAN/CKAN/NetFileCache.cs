using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CKAN
{

    public class NetFileCache
    {

        private string tempPath = "";
        private string downloadsPath = "";
        public NetFileCache(string _downloadsPath, string _tempPath)
        {
            tempPath = _tempPath;
            downloadsPath = _downloadsPath;

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
        }

        public string GetCachePath()
        {
            return downloadsPath;
        }

        public bool IsCached(Uri url, out string outFilename)
        {
            var hash = CreateURLHash(url);

            foreach (var file in Directory.GetFiles(downloadsPath))
            {
                if (file.StartsWith(hash))
                {
                    outFilename = file;
                    return true;
                }
            }

            outFilename = "";
            return false;
        }

        public string CreateTemporaryPathForURL(Uri url)
        {
            var hash = CreateURLHash(url);
            return Path.Combine(tempPath, hash);
        }

        public string CommitDownload(Uri url, string filename)
        {
            var hash = CreateURLHash(url);
            var fullName = String.Format("{0}-{1}", hash, Path.GetFileName(filename));
            var targetPath = Path.Combine(downloadsPath, fullName);
            var sourcePath = CreateTemporaryPathForURL(url);
            File.Move(sourcePath, targetPath);
            return targetPath;
        }

        public static string CreateURLHash(Uri url)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(url.ToString()));
                return Convert.ToBase64String(hash).Substring(0, 8);
            }
        }

    }

}
