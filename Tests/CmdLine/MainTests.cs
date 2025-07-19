using System.IO;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class MainTests
    {
        [Test]
        public void Execute_Version_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                var opts = new CommonOptions();
                var args = new string[] { "version" };

                // Act
                MainClass.Execute(manager, opts, args, user);

                // Assert
                CollectionAssert.AreEqual(new string[] { Meta.ReleaseVersion.ToString() },
                                          user.RaisedMessages);
            }
        }

        [Test]
        public void Execute_Scan_FindsDLLsAndDLCs()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var repo     = new TemporaryRepository())
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var dll = new FileInfo(Path.Combine(inst.KSP.game.PrimaryModDirectory(inst.KSP),
                                                    "ModuleManager.1.2.3.dll"));
                var dlc = new FileInfo(Path.Combine(inst.KSP.game.PrimaryModDirectory(inst.KSP),
                                                    "SquadExpansion", "MakingHistory",
                                                    "readme.txt"));
                var opts = new CommonOptions();
                var args = new string[] { "scan" };

                // Act
                dll.Directory?.Create();
                dll.Create().Dispose();
                dlc.Directory?.Create();
                File.WriteAllText(dlc.FullName, "Version 1.0.0");
                MainClass.Execute(manager, opts, args, user);

                // Assert
                CollectionAssert.AreEquivalent(new string[] { "ModuleManager" },
                                               regMgr.registry.InstalledDlls);
                CollectionAssert.AreEquivalent(new string[] { "MakingHistory-DLC" },
                                               regMgr.registry.InstalledDlc.Keys);
            }
        }

        [Test]
        public void Execute_Clean_PurgesCache()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                var module = TestData.DogeCoinFlag_101_module();
                var opts   = new CommonOptions();
                var args   = new string[] { "clean" };

                // Act
                manager.Cache?.Store(module, TestData.DogeCoinFlagZip(), null);
                Assert.IsNotNull(manager.Cache?.GetCachedFilename(module));
                MainClass.Execute(manager, opts, args, user);

                // Assert
                Assert.IsNull(manager.Cache?.GetCachedFilename(module));
            }
        }
    }
}
