using System;
using System.IO;

namespace Tests.Data
{
    public class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory(DirectoryInfo path)
        {
            Path = path;
            Directory.CreateDirectory(Path.FullName);
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
            Directory.Delete(Path.FullName, true);
            GC.SuppressFinalize(this);
        }

        public readonly DirectoryInfo Path;
    }
}
