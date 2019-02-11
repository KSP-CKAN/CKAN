using System.IO;
using CKAN;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core
{
    [TestFixture] public class Utilities
    {
        string tempDir;
        string goodKspDir = TestData.good_ksp_dir();

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
            CKAN.Utilities.CopyDirectory(goodKspDir, tempDir, true);

            Assert.IsTrue(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "GameData", "README.md"),
                Path.Combine(tempDir,   "GameData", "README.md")));
            Assert.IsTrue(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "buildID.txt"),
                Path.Combine(tempDir,   "buildID.txt")));
            Assert.IsTrue(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "readme.txt"),
                Path.Combine(tempDir,   "readme.txt")));
        }

        [Test]
        public void CopyDirectory_NotRecursive_DestHasOnlyFirstLevelFiles()
        {
            CKAN.Utilities.CopyDirectory(goodKspDir, tempDir, false);

            Assert.IsFalse(File.Exists(Path.Combine(tempDir, "GameData", "README.md")));
            // The following assertion is per se already included in the above assertion,
            // but this also tests CompareFiles, so no harm in including this.
            Assert.IsFalse(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "GameData", "README.md"),
                Path.Combine(tempDir, "GameData", "README.md")));
            Assert.IsTrue(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "buildID.txt"),
                Path.Combine(tempDir, "buildID.txt")));
            Assert.IsTrue(UtilStatic.CompareFiles(
                Path.Combine(goodKspDir, "readme.txt"),
                Path.Combine(tempDir, "readme.txt")));
        }

        [Test]
        public void CopyDirectory_SourceNotExisting_ThrowsDirectoryNotFoundKraken()
        {
            var sourceDir = "/gibberish/DOESNTEXIST/hopefully";

            // Act and Assert
            Assert.Throws<DirectoryNotFoundKraken>(() => CKAN.Utilities.CopyDirectory(sourceDir, tempDir, true));
        }

        [Test]
        public void CopyDirectory_DestNotEmpty_ThrowsException()
        {
            File.WriteAllText(Path.Combine(tempDir, "thatsafile"), "not empty");

            Assert.Throws<PathErrorKraken>(() => CKAN.Utilities.CopyDirectory(goodKspDir, tempDir, true));
        }
    }
}
