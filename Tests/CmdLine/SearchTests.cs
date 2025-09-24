using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;

namespace Tests.CmdLine
{
    [TestFixture]
    public class SearchTests
    {
        [Test]
        public void RunCommand_SearchForhangWithDetail_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user, new Dictionary<Repository, RepositoryData>
            {
                { repo, RepositoryData.FromJson(TestData.TestRepository(), null)! },
            }))
            using (var regMgr = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                         new Repository[] { repo }))
            {
                ICommand sut  = new Search(repoData.Manager, user);
                var      opts = new SearchOptions()
                                {
                                    search_term = "hang",
                                    all         = true,
                                    detail      = true,
                                };

                // Act
                sut.RunCommand(inst.KSP, opts);
            }

            // Assert
            CollectionAssert.AreEqual(
                new string[]
                {
                    @"Found 2 compatible and 2 incompatible mods matching ""hang""",
                    "Matching compatible mods:",
                    "* NavBallTextureExport (1.3) - Navball Texture Changer by xEvilReeperx - Replace stock navball texture with something better!",
                    "* TechManager (1.5) - TechManager by anonish - This mod will allow you to change the tech tree, including the creation of new tech nodes. ",
                    "Matching incompatible mods:",
                    "* FShangarExtender (2.0 - KSP 0.24.2) - Hangar Extender by Snjo - Extends the usable area when building in the SPH or VAB, so you can build outside or above the building. Useful for building large aircraft carriers or tall rockets.",
                    "* HangarExtender (2.0 - KSP 0.24.2) - Hangar Extender by Snjo - Extends the usable area when building in the SPH or VAB, so you can build outside or above the building. Useful for building large aircraft carriers or tall rockets.",
                },
                user.RaisedMessages);
        }

        [Test]
        public void RunCommand_SearchForhang_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user, new Dictionary<Repository, RepositoryData>
            {
                { repo, RepositoryData.FromJson(TestData.TestRepository(), null)! },
            }))
            using (var regMgr = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                         new Repository[] { repo }))
            {
                ICommand sut  = new Search(repoData.Manager, user);
                var      opts = new SearchOptions()
                                {
                                    search_term = "hang",
                                    all         = true,
                                };

                // Act
                sut.RunCommand(inst.KSP, opts);
            }

            // Assert
            CollectionAssert.AreEqual(
                new string[]
                {
                    @"Found 2 compatible and 2 incompatible mods matching ""hang""",
                    "FShangarExtender",
                    "HangarExtender",
                    "NavBallTextureExport",
                    "TechManager",
                },
                user.RaisedMessages);
        }

        [Test]
        public void RunCommand_AuthorSearch_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user, new Dictionary<Repository, RepositoryData>
            {
                { repo, RepositoryData.FromJson(TestData.TestRepository(), null)! },
            }))
            using (var regMgr = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                         new Repository[] { repo }))
            {
                ICommand sut  = new Search(repoData.Manager, user);
                var      opts = new SearchOptions()
                                {
                                    author_term = "sarbian",
                                    all         = true,
                                };

                // Act
                sut.RunCommand(inst.KSP, opts);
            }

            // Assert
            CollectionAssert.AreEqual(
                new string[]
                {
                    @"Found 3 compatible and 0 incompatible mods matching """" by ""sarbian""",
                    "DDSLoader",
                    "MechJeb2",
                    "ModuleManager",
                },
                user.RaisedMessages);
        }

        [Test]
        public void RunCommand_NoArguments_PrintsHelp()
        {
            // Arrange
            var      user = new CapturingUser(false, q => true, (msg, objs) => 0);
            ICommand sut  = new Search(new RepositoryDataManager(), user);
            using (var inst = new DisposableKSP())
            {
                var opts = new SearchOptions();

                // Act
                sut.RunCommand(inst.KSP, opts);
            }
            // Assert
            CollectionAssert.AreEqual(new string[]
                                      {
                                          "argument missing, perhaps you forgot it?",
                                          " ",
                                          "Usage: ckan search [options] substring"
                                      },
                                      user.RaisedErrors);
        }
    }
}
