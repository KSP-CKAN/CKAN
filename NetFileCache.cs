using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CKAN
{

    class NetFileCache
    {

        public static NetFileCache _Instance = null;

        public static NetFileCache Instance
        {
            get
            {
                _Instance = _Instance ?? new NetFileCache();
                return _Instance;
            }
        }

        private string tempPath = "";
        private string downloadsPath = "";

        private NetFileCache()
        {
            tempPath = Path.Combine(KSPManager.CurrentInstance.CkanDir(), "temp");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            
            downloadsPath = KSPManager.CurrentInstance.DownloadCacheDir();
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
                return Convert.ToBase64String(hash).Substring(0, 6);
            }
        }

    }

}
