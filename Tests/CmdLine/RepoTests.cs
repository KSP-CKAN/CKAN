using System;
using System.IO;
using System.Collections.Generic;

using NUnit.Framework;
using Moq;

using CKAN;
using CKAN.Games;
using CKAN.Versioning;
using CKAN.CmdLine;
using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.CmdLine
{
    [TestFixture]
    public class RepoTests
    {
        [Test]
        public void RunSubCommand_Available_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var instDir  = new TemporaryDirectory())
            using (var config   = new FakeConfiguration(new List<Tuple<string, string, string>>(),
                                                        null, null))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var manager  = new GameInstanceManager(new NullUser(), config))
            {
                var         gv       = new GameVersion(1, 0, 0);
                var         gameMock = FakeGame(TestData.TestRepositoriesURL(),
                                                TestData.TestKANTarGz(), gv);
                ISubCommand sut      = new Repo(manager, repoData.Manager, user);
                var         args     = new string[] { "repo", "available" };
                var         subOpts  = new SubCommandOptions(args);

                // Act
                manager.FakeInstance(gameMock.Object, "test", instDir.Path.FullName, gv);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(
                    new string[]
                    {
                        "Listing all (canonical) available CKAN repositories:",
                        "  first : https://host1.com/metadata.tar.gz",
                        "  second: https://host2.com/metadata.tar.gz",
                        "  third : https://host3.com/metadata.tar.gz",
                    },
                    user.RaisedMessages);
            }
        }

        [Test]
        public void RunSubCommand_List_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var instDir  = new TemporaryDirectory())
            using (var config   = new FakeConfiguration(new List<Tuple<string, string, string>>(),
                                                        null, null))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var manager  = new GameInstanceManager(new NullUser(), config))
            {
                var         gv       = new GameVersion(1, 0, 0);
                var         gameMock = FakeGame(TestData.TestRepositoriesURL(),
                                                TestData.TestKANTarGz(), gv);
                var         repoURLLen = TestData.TestKANTarGz().ToString().Length;
                ISubCommand sut      = new Repo(manager, repoData.Manager, user);
                var         args     = new string[] { "repo", "list" };
                var         subOpts  = new SubCommandOptions(args);

                // Act
                manager.FakeInstance(gameMock.Object, "test", instDir.Path.FullName, gv);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(
                    new string[]
                    {
                        $"Priority  Name              {"URL".PadRight(repoURLLen)}",
                        $"--------  ----------------  {new string('-', repoURLLen)}",
                        $"0         FakeGame-default  {TestData.TestKANTarGz()}",
                    },
                    user.RaisedMessages);
            }
        }

        [Test]
        public void RunSubCommand_Add_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var instDir  = new TemporaryDirectory())
            using (var config   = new FakeConfiguration(new List<Tuple<string, string, string>>(),
                                                        null, null))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var manager  = new GameInstanceManager(new NullUser(), config))
            {
                var         gv       = new GameVersion(1, 0, 0);
                var         gameMock = FakeGame(TestData.TestRepositoriesURL(),
                                                TestData.TestKANTarGz(), gv);
                ISubCommand sut      = new Repo(manager, repoData.Manager, user);
                var         args     = new string[] { "repo", "add", "second" };
                var         subOpts  = new SubCommandOptions(args);

                // Act
                manager.FakeInstance(gameMock.Object, "test", instDir.Path.FullName, gv);
                manager.SetCurrentInstance("test");
                using (var regMgr = RegistryManager.Instance(manager.CurrentInstance!,
                                                             repoData.Manager))
                {
                    regMgr.registry.RepositoriesClear();
                    sut.RunSubCommand(null, subOpts);

                    // Assert
                    CollectionAssert.AreEqual(new Repository[]
                                              {
                                                  new Repository("second", "https://host2.com/metadata.tar.gz"),
                                              },
                                              regMgr.registry.Repositories.Values);
                }
            }
        }

        [Test]
        public void RunSubCommand_Priority_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var instDir  = new TemporaryDirectory())
            using (var config   = new FakeConfiguration(new List<Tuple<string, string, string>>(),
                                                        null, null))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var manager  = new GameInstanceManager(new NullUser(), config))
            {
                var         gv       = new GameVersion(1, 0, 0);
                var         gameMock = FakeGame(TestData.TestRepositoriesURL(),
                                                TestData.TestKANTarGz(), gv);
                ISubCommand sut      = new Repo(manager, repoData.Manager, user);
                var         args     = new string[] { "repo", "priority", "FakeGame-default", "1" };
                var         subOpts  = new SubCommandOptions(args);

                // Act
                manager.FakeInstance(gameMock.Object, "test", instDir.Path.FullName, gv);
                manager.SetCurrentInstance("test");
                using (var regMgr = RegistryManager.Instance(manager.CurrentInstance!,
                                                             repoData.Manager))
                {
                    regMgr.registry.RepositoriesAdd(new Repository("alternate", "https://github.com/"));
                    sut.RunSubCommand(null, subOpts);

                    // Assert
                    Assert.AreEqual(1, regMgr.registry.Repositories["FakeGame-default"].priority);
                }
            }
        }

        [Test]
        public void RunSubCommand_Forget_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var instDir  = new TemporaryDirectory())
            using (var config   = new FakeConfiguration(new List<Tuple<string, string, string>>(),
                                                        null, null))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var manager  = new GameInstanceManager(new NullUser(), config))
            {
                var         gv       = new GameVersion(1, 0, 0);
                var         gameMock = FakeGame(TestData.TestRepositoriesURL(),
                                                TestData.TestKANTarGz(), gv);
                ISubCommand sut      = new Repo(manager, repoData.Manager, user);
                var         args     = new string[] { "repo", "forget", "FakeGame-default" };
                var         subOpts  = new SubCommandOptions(args);

                // Act
                manager.FakeInstance(gameMock.Object, "test", instDir.Path.FullName, gv);
                sut.RunSubCommand(null, subOpts);

                // Assert
                using (var regMgr = RegistryManager.Instance(manager.CurrentInstance!,
                                                             repoData.Manager))
                {
                    CollectionAssert.IsEmpty(regMgr.registry.Repositories.Values);
                }
            }
        }

        [Test]
        public void RunSubCommand_Default_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var instDir  = new TemporaryDirectory())
            using (var config   = new FakeConfiguration(new List<Tuple<string, string, string>>(),
                                                        null, null))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var manager  = new GameInstanceManager(new NullUser(), config))
            {
                var         gv       = new GameVersion(1, 0, 0);
                var         gameMock = FakeGame(TestData.TestRepositoriesURL(),
                                                TestData.TestKANTarGz(), gv);
                ISubCommand sut      = new Repo(manager, repoData.Manager, user);
                var         args     = new string[] { "repo", "default" };
                var         subOpts  = new SubCommandOptions(args);

                // Act
                manager.FakeInstance(gameMock.Object, "test", instDir.Path.FullName, gv);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[] { $"Set default repository to '{TestData.TestKANTarGz()}'" },
                                          user.RaisedMessages);
            }
        }

        private static Mock<IGame> FakeGame(Uri         repoListURL,
                                            Uri         defaultRepoURL,
                                            GameVersion gv)
        {
            var mock = new Mock<IGame>();
            mock.Setup(g => g.ShortName).Returns("FakeGame");
            mock.Setup(g => g.RepositoryListURL).Returns(repoListURL);
            mock.Setup(g => g.DefaultRepositoryURL).Returns(defaultRepoURL);
            mock.Setup(g => g.GameInFolder(It.IsAny<DirectoryInfo>())).Returns(true);
            mock.Setup(g => g.CompatibleVersionsFile).Returns("dummy.txt");
            mock.Setup(g => g.PrimaryModDirectoryRelative).Returns("GameData");
            mock.Setup(g => g.DetectVersion(It.IsAny<DirectoryInfo>())).Returns(gv);
            mock.Setup(g => g.KnownVersions).Returns(new List<GameVersion> { gv });
            return mock;
        }
    }
}
