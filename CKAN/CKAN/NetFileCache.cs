using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CKAN
{

    public class NetFileCache
    {

        private string tempPath = "";
        private string cachePath = "";
    
        public NetFileCache(string _cachePath, string _tempPath = null)
        {
            if (_tempPath == null)
            {
                tempPath = Path.Combine(Path.GetTempPath(), "ckan_temp");
            }
            else
            {
                tempPath = _tempPath;
            }

            cachePath = _cachePath;

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            // clean temp
            foreach (var file in Directory.GetFiles(tempPath))
            {
                try { File.Delete(file); }
                catch (Exception) {}
            }
        }

        public string GetCachePath()
        {
            return cachePath;
        }

        public bool IsCached(Uri url, out string outFilename)
        {
            var hash = CreateURLHash(url);

            foreach (var file in Directory.GetFiles(cachePath))
            {
                var filename = Path.GetFileName(file);
                if (filename.StartsWith(hash))
                {
                    outFilename = file;
                    return true;
                }
            }

            outFilename = "";
            return false;
        }

        public string GetTemporaryPathForURL(Uri url)
        {
            var hash = CreateURLHash(url);
            return Path.Combine(tempPath, hash);
        }

        public string CommitDownload(Uri url, string filename)
        {
            var sourcePath = GetTemporaryPathForURL(url);

            var hash = CreateURLHash(url);
            var fullName = String.Format("{0}-{1}", hash, Path.GetFileName(filename));
            var targetPath = Path.Combine(cachePath, fullName);

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
