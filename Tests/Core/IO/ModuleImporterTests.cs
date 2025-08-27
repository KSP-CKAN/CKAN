using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.IO;
using Tests.Data;

namespace Tests.Core.IO
{
    [TestFixture]
    public sealed class ModuleImporterTests
    {
        [Test]
        public void ImportFiles_InternalCkanFile_Works()
        {
            // Arrange
            var nullUser = new NullUser();
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(nullUser))
            using (var cacheDir = new TemporaryDirectory())
            {
                var registry = CKAN.Registry.Empty(repoData.Manager);
                var cache = new NetModuleCache(cacheDir.Directory.FullName);
                var files = new HashSet<FileInfo>
                {
                    new FileInfo(TestData.DogeCoinFlagImportableZip())
                };

                // Act
                var toInstall = new List<CkanModule>();
                var result    = ModuleImporter.ImportFiles(files, nullUser, toInstall.Add,
                                                           registry, inst.KSP, cache, false);

                // Assert
                Assert.IsTrue(result);
                Assert.AreEqual(1, cacheDir.Directory.EnumerateFiles("*").Count());
                Assert.AreEqual(1, toInstall.Count);
            }
        }
    }
}
