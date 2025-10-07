using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram;
using CKAN.Games.KerbalSpaceProgram2;
using CKAN.IO;
using Tests.Data;

namespace Tests.Core.IO
{
    [TestFixture]
    public sealed class ModuleImporterTests
    {
        [TestCaseSource(nameof(ImportableArguments))]
        public void ImportFiles_InternalCkanFile_Works(string zipPath, IGame game)
        {
            // Arrange
            var user = new CapturingUser(true, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP("disposable", game))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var cacheDir = new TemporaryDirectory())
            {
                var registry  = CKAN.Registry.Empty(repoData.Manager);
                var cache     = new NetModuleCache(cacheDir);
                var files     = new HashSet<FileInfo> { new FileInfo(zipPath) };
                var toInstall = new List<CkanModule>();

                // Act
                var result = ModuleImporter.ImportFiles(files, user, toInstall.Add,
                                                        registry, inst.KSP, cache, false);

                // Assert
                Assert.IsTrue(result);
                CollectionAssert.IsEmpty(user.RaisedErrors);
                Assert.AreEqual(1, cacheDir.Directory.EnumerateFiles("*").Count());
                Assert.AreEqual(1, toInstall.Count);
            }
        }

        private static IEnumerable<TestCaseData> ImportableArguments()
        {
            yield return new TestCaseData(TestData.DogeCoinFlagImportableZip(),   new KerbalSpaceProgram());
            yield return new TestCaseData(TestData.BurnControllerImportableZip(), new KerbalSpaceProgram2());
        }
    }
}
