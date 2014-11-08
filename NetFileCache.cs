using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CKAN
{


    /*
     * This class allows us to cache downloads by URL
     * It works using two directories - one to store downloads in-progress, and one to commit finished downloads
     * URLs are cached by hashing them by taking the first 8 chars of the url's SHA1 hash.
     * 
     * To use this class the user would have to:
     * - Obtain a temporary download path by calling GetTemporaryPathForURL(url)
     * - Initiate his download in this temporary path
     * - Call CommitDownload(url, desired_filename) which will move the temporary file to the final location
     * - The final file will be named such as <hash>-<filename>.zip
     * - The user can call IsCached(url) to check if a particular url exists in the cache
     * and GetCachedFilename() to get its filename
     */
    public class NetFileCache
    {

        private string tempPath = "";
        private string cachePath = "";
   
        public NetFileCache(string _cachePath, string _tempPath = null)
        {
            if (_tempPath == null)
            {
                // to ensure a temp dir just for us we get a temp file, delete it and create a directory in its place
                var tempFile = Path.GetTempFileName();
                File.Delete(tempFile);
                Directory.CreateDirectory(tempFile);
                tempPath = Path.Combine(Path.GetTempPath(), tempFile);
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

        // returns true if a url is already in the cache
        public bool IsCached(Uri url)
        {
            var hash = CreateURLHash(url);

            foreach (var file in Directory.GetFiles(cachePath))
            {
                var filename = Path.GetFileName(file);
                if (filename.StartsWith(hash))
                {
                    return true;
                }
            }

            return false;
        }

        // returns true if a url is already in the cache
        // returns the filename in the outFilename parameter
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

        // returns the filename of an already cached url or null otherwise
        public string GetCachedFilename(Uri url)
        {
            var hash = CreateURLHash(url);

            foreach (var file in Directory.GetFiles(cachePath))
            {
                var filename = Path.GetFileName(file);
                if (filename.StartsWith(hash))
                {
                    return file;
                }
            }

            return null;
        }

        // returns a temporary file for a url to which the user can download
        public string GetTemporaryPathForURL(Uri url)
        {
            var hash = CreateURLHash(url);
            return Path.Combine(tempPath, hash);
        }

        // moves a finished download to the complete downloads directory with a desired filename
        public string CommitDownload(Uri url, string filename)
        {
            var sourcePath = GetTemporaryPathForURL(url);

            var hash = CreateURLHash(url);
            var fullName = String.Format("{0}-{1}", hash, Path.GetFileName(filename));
            var targetPath = Path.Combine(cachePath, fullName);

            File.Move(sourcePath, targetPath);
            return targetPath;
        }

        // stores an already existing file in the cache
        public string Store(Uri url, string path, bool copy = false)
        {
            var hash = CreateURLHash(url);
            var fullName = String.Format("{0}-{1}", hash, Path.GetFileName(path));
            var targetPath = Path.Combine(cachePath, fullName);

            if (copy)
            {
                File.Copy(path, targetPath);
            }
            else
            {
                File.Move(path, targetPath);
            }

            return targetPath;
        }

        // returns the 8-byte hash for a given url
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
