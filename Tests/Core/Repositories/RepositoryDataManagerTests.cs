using System.Linq;

using NUnit.Framework;

using Tests.Data;

using CKAN;
using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram;

namespace Tests.Core.Repositories
{
    [TestFixture]
    public class RepositoryDataManagerTests
    {
        [Test]
        public void UpdateRegistryTarGz()
        {
            // Arrange
            var user     = new NullUser();
            var testRepo = new Repository("testRepo", TestData.TestKANTarGz());
            using (var repoData = new TemporaryRepositoryData(user, testRepo))
            {
                var crit = new GameVersionCriteria(GameVersion.Parse("0.25.0"));

                // Act
                var versions = repoData.Manager.GetAvailableModules(Enumerable.Repeat(testRepo, 1),
                                                                    "FerramAerospaceResearch")
                                               .Select(am => am.Latest(ReleaseStatus.stable, crit)?.version.ToString())
                                               .ToArray();

                // Assert
                CollectionAssert.AreEquivalent(new string[] { "v0.14.3.2" },
                                               versions);
            }
        }

        [Test]
        public void UpdateRegistryZip()
        {
            // Arrange
            var user     = new NullUser();
            var testRepo = new Repository("testRepo", TestData.TestKANZip());
            using (var repoData = new TemporaryRepositoryData(user, testRepo))
            {
                var crit = new GameVersionCriteria(GameVersion.Parse("0.25.0"));

                // Act
                var versions = repoData.Manager.GetAvailableModules(Enumerable.Repeat(testRepo, 1),
                                                                    "FerramAerospaceResearch")
                                               .Select(am => am.Latest(ReleaseStatus.stable, crit)?.version.ToString())
                                               .ToArray();

                // Assert
                CollectionAssert.AreEquivalent(new string[] { "v0.14.3.2" },
                                               versions);
            }
        }

        [Test]
        public void BadKanTarGz()
        {
            Assert.DoesNotThrow(delegate
            {
                var user = new NullUser();
                var badRepo = new Repository("badRepo", TestData.BadKANTarGz());
                using (var repoData = new TemporaryRepositoryData(user, badRepo))
                {
                }
            });
        }

        [Test]
        public void BadKanZip()
        {
            Assert.DoesNotThrow(delegate
            {
                var user = new NullUser();
                var badRepo = new Repository("badRepo", TestData.BadKANZip());
                using (var repoData = new TemporaryRepositoryData(user, badRepo))
                {
                }
            });
        }

        [Test]
        public void Prepopulate_PreviouslyLoadedDir_LoadsMetadata()
        {
            // Arrange
            var user  = new NullUser();
            var repos = new Repository[] { new Repository("TestRepo", TestData.TestKANTarGz()) };
            using (var reposDir = new TemporaryDirectory())
            {
                var prev  = new RepositoryDataManager(reposDir.Directory.FullName);
                prev.Update(repos, new KerbalSpaceProgram(), true,
                            new NetAsyncDownloader(user, () => null ), user);
                var sut = new RepositoryDataManager(reposDir.Directory.FullName);

                // Act
                sut.Prepopulate(repos, null);

                // Assert
                CollectionAssert.IsNotEmpty(sut.GetAllAvailableModules(repos));
            }
        }
    }
}
