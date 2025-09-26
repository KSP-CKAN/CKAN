using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;
using CKAN.IO;
using CKAN.Games.KerbalSpaceProgram;
using Tests.Core.IO;

namespace Tests.CmdLine
{
    [TestFixture]
    public class DeduplicateTests
    {
        [Test]
        public void RunCommand_InstalledMissionInTwoInstances_Deduplicates()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst1    = new DisposableKSP("inst1", new KerbalSpaceProgram()))
            using (var inst2    = new DisposableKSP("inst2", new KerbalSpaceProgram()))
            using (var config   = new FakeConfiguration(
                                      new List<Tuple<string, string, string>>
                                      {
                                          new Tuple<string, string, string>(
                                              inst1.KSP.Name,
                                              inst1.KSP.GameDir,
                                              inst1.KSP.Game.ShortName),
                                          new Tuple<string, string, string>(
                                              inst2.KSP.Name,
                                              inst2.KSP.GameDir,
                                              inst2.KSP.Game.ShortName),
                                      },
                                      null, null))
            using (var manager  = new GameInstanceManager(user, config))
            using (var repo     = new TemporaryRepository())
            using (var repoData = new TemporaryRepositoryData(new NullUser(), repo.repo))
            using (var regMgr1  = RegistryManager.Instance(inst1.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            using (var regMgr2  = RegistryManager.Instance(inst2.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                var installer1 = new ModuleInstaller(inst1.KSP, manager.Cache!, config, new NullUser());
                var installer2 = new ModuleInstaller(inst2.KSP, manager.Cache!, config, new NullUser());
                HashSet<string>? possibleConfigOnlyDirs1 = null;
                HashSet<string>? possibleConfigOnlyDirs2 = null;
                var opts = RelationshipResolverOptions.DependsOnlyOpts(inst1.KSP.StabilityToleranceConfig);
                var modules = new List<CkanModule> { TestData.MissionModule() };
                manager.Cache!.Store(TestData.MissionModule(),
                                     TestData.MissionZip(), null);
                var sut = new Deduplicate(manager, repoData.Manager, user);

                // Act
                installer1.InstallList(modules, opts, regMgr1, ref possibleConfigOnlyDirs1);
                installer2.InstallList(modules, opts, regMgr2, ref possibleConfigOnlyDirs2);
                var allPaths = ModuleInstallerTests.AbsoluteInstalledPaths(inst1.KSP, regMgr1.registry)
                                   .Concat(ModuleInstallerTests.AbsoluteInstalledPaths(inst2.KSP, regMgr2.registry))
                                   .Order()
                                   .ToArray();
                Assert.AreEqual(0, ModuleInstallerTests.MultiLinkedFileCount(allPaths));
                sut.RunCommand();

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                // There are 3 files >128 KiB in this mod, each installed twice
                Assert.AreEqual(6, ModuleInstallerTests.MultiLinkedFileCount(allPaths));
            }
        }

    }
}
