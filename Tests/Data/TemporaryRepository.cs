using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

using CKAN;

namespace Tests.Data
{
    /// <summary>
    /// A disposable repository backed by an auto-created ZIP file
    /// containing the given modules.
    /// Will be automatically cleaned up on falling out of using() scope.
    /// </summary>
    public class TemporaryRepository : IDisposable
    {
        public TemporaryRepository(params string[] fileContents)
        {
            path = Path.GetTempFileName();

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

        public Uri        uri  => new Uri(path);
        public Repository repo => new Repository("temp", uri);

        public void Dispose()
        {
            File.Delete(path);
        }

        private readonly string path;
    }
}
