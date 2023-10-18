using System;
using System.IO;
using System.Text;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

using CKAN;

namespace Tests.Data
{
    /// <summary>
    /// A disposable repository backed by an auto-created tar.gz file
    /// containing the given modules.
    /// Will be automatically cleaned up on falling out of using() scope.
    /// </summary>
    public class TemporaryRepository : IDisposable
    {
        public TemporaryRepository(int priority, params string[] fileContents)
        {
            path = Path.GetTempFileName();
            repo = new Repository("temp", path, priority);

            using (var outputStream = File.OpenWrite(path))
            using (var gzipStream   = new GZipOutputStream(outputStream))
            using (var tarStream    = new TarOutputStream(gzipStream, Encoding.UTF8))
            {
                int i = 0;
                foreach (var contents in fileContents)
                {
                    var entry = TarEntry.CreateTarEntry($"{++i}.ckan");
                    entry.Size = contents.Length;
                    byte[] buffer = new byte[contents.Length];
                    tarStream.PutNextEntry(entry);
                    tarStream.Write(Encoding.UTF8.GetBytes(contents), 0, contents.Length);
                    tarStream.CloseEntry();
                }
                tarStream.Finish();
            }
        }

        public TemporaryRepository(params string[] fileContents)
            : this(0, fileContents)
        { }

        public          Uri        uri  => new Uri(path);
        public readonly Repository repo;

        public void Dispose()
        {
            File.Delete(path);
        }

        private readonly string path;
    }
}
