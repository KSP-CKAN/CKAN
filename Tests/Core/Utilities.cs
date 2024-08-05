using System;
using System.IO;

using NUnit.Framework;

using Tests.Data;
using CKAN;

namespace Tests.Core
{
    [TestFixture] public class Utilities
    {
        private string tempDir;
        private readonly string goodKspDir = TestData.good_ksp_dir();

        [SetUp]
        public void CreateTempDir()
        {
            tempDir = TestData.NewTempDir();
        }

        [TearDown]
        public void EmptyTempDir()
        {
            Directory.Delete(tempDir, true);
        }

        [Test]
        public void CopyDirectory_Recursive_DestHasAllContents()
        {
            CKAN.Utilities.CopyDirectory(goodKspDir, tempDir, Array.Empty<string>(), Array.Empty<string>());

            var fi = new FileInfo(Path.Combine(tempDir, "GameData"));
            Assert.IsFalse(fi.Attributes.HasFlag(FileAttributes.ReparsePoint),
                           "GameData should not be a symlink");
            Assert.IsTrue(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "GameData", "README.md"),
                Path.Combine(tempDir,    "GameData", "README.md")));
            Assert.IsTrue(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "buildID.txt"),
                Path.Combine(tempDir,    "buildID.txt")));
            Assert.IsTrue(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "readme.txt"),
                Path.Combine(tempDir,    "readme.txt")));
        }

        [Test]
        public void CopyDirectory_WithSymlinks_MakesSymlinks()
        {
            // Arrange / Act
            CKAN.Utilities.CopyDirectory(Path.Combine(TestData.DataDir(), "KSP"), tempDir,
                                         new string[] { "KSP-0.25/GameData" }, Array.Empty<string>());

            // Assert
            var fi1 = new FileInfo(Path.Combine(tempDir, "KSP-0.25"));
            Assert.IsFalse(fi1.Attributes.HasFlag(FileAttributes.ReparsePoint),
                           "KSP-0.25 should not be a symlink");
            var fi2 = new FileInfo(Path.Combine(tempDir, "KSP-0.25", "GameData"));
            Assert.IsTrue(fi2.Attributes.HasFlag(FileAttributes.ReparsePoint),
                          "KSP-0.25/GameData should be a symlink");
            var fi3 = new FileInfo(Path.Combine(tempDir, "KSP-0.25", "GameData", "README.md"));
            Assert.IsFalse(fi3.Attributes.HasFlag(FileAttributes.ReparsePoint),
                           "KSP-0.25/GameData/README.md should not be a symlink");

            DirectoryLink.Remove(fi2.FullName);
        }

        [Test]
        public void CopyDirectory_SourceNotExisting_ThrowsDirectoryNotFoundKraken()
        {
            var sourceDir = "/gibberish/DOESNTEXIST/hopefully";

            // Act and Assert
            Assert.Throws<DirectoryNotFoundKraken>(() => CKAN.Utilities.CopyDirectory(sourceDir, tempDir, Array.Empty<string>(), Array.Empty<string>()));
        }

        [Test]
        public void CopyDirectory_DestNotEmpty_ThrowsException()
        {
            File.WriteAllText(Path.Combine(tempDir, "thatsafile"), "not empty");

            Assert.Throws<PathErrorKraken>(() => CKAN.Utilities.CopyDirectory(goodKspDir, tempDir, Array.Empty<string>(), Array.Empty<string>()));
        }
    }
}
