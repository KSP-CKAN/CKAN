using System.IO;

using ChinhDo.Transactions.FileManager;
using NUnit.Framework;

using CKAN.IO;
using Tests.Data;

namespace Tests.Core.IO
{
    [TestFixture]
    public class DirectoryLinkTests
    {
        [Test]
        public void CreateGetRemove_WithTestDir_Works()
        {
            using (var dir = new TemporaryDirectory())
            {
                // Arrange
                var target    = new DirectoryInfo(Path.Combine(dir.Directory.FullName,
                                                               "targetdir"));
                var dirlink   = new DirectoryInfo(Path.Combine(dir.Directory.FullName,
                                                               "dirlink"));
                var file1Orig = new FileInfo(Path.Combine(target.FullName,  "file1.txt"));
                var file2Orig = new FileInfo(Path.Combine(target.FullName,  "file2.txt"));
                var file1Link = new FileInfo(Path.Combine(dirlink.FullName, "file1.txt"));
                var file2Link = new FileInfo(Path.Combine(dirlink.FullName, "file2.txt"));
                var tx        = new TxFileManager();

                // Act
                target.Create();
                DirectoryLink.Create(target.FullName, dirlink.FullName, tx);
                Assert.IsTrue(dirlink.Exists);
                File.WriteAllText(file1Orig.FullName, "");
                File.WriteAllText(file2Link.FullName, "");

                // Assert
                Assert.IsTrue(file1Link.Exists);
                Assert.IsTrue(file2Orig.Exists);

                // Act / Assert
                Assert.IsTrue(DirectoryLink.TryGetTarget(dirlink.FullName,
                                                         out string? linkTarget));

                // Assert
                Assert.AreEqual(target.FullName, linkTarget);

                // Act
                DirectoryLink.Remove(dirlink.FullName);

                // Assert
                Assert.IsFalse(File.Exists(dirlink.FullName));
                Assert.IsFalse(Directory.Exists(dirlink.FullName));
            }
        }
    }
}
