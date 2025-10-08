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
            Directory.Delete(true);
            GC.SuppressFinalize(this);
        }

        public static implicit operator string(TemporaryDirectory td)
            => td.ToString();

        public override string ToString()
            => Directory.FullName;

        public readonly DirectoryInfo Directory;
    }
}
