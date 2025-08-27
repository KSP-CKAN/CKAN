using System;
using System.IO;

namespace Tests.Data
{
    public class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory(DirectoryInfo path)
        {
            Directory = path;
            Directory.Create();
        }

        public TemporaryDirectory(string path)
            : this(new DirectoryInfo(path))
        {
        }

        public TemporaryDirectory()
            : this(TestData.NewTempDir())
        {
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(true);
            }
            catch
            {
            }
            GC.SuppressFinalize(this);
        }

        public readonly DirectoryInfo Directory;
    }
}
