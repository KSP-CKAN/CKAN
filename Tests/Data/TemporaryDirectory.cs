using System;
using System.IO;

namespace Tests.Data
{
    public class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory(DirectoryInfo path)
        {
            Path = path;
            Path.Create();
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
                Path.Delete(true);
            }
            catch
            {
            }
            GC.SuppressFinalize(this);
        }

        public readonly DirectoryInfo Path;
    }
}
