using System;
using System.IO;

using CKAN;

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

        public static TemporaryDirectory CopiedFromDir(string fromPath)
        {
            var dir = new TemporaryDirectory();
            Utilities.CopyDirectory(fromPath, dir,
                                    Array.Empty<string>(), Array.Empty<string>());
            return dir;
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
