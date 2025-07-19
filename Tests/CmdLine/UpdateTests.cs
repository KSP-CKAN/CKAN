using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class UpdateTests
    {
        [Test]
        public void RunCommand_LocalRepo_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager))
            {
                regMgr.registry.RepositoriesClear();
                regMgr.registry.RepositoriesAdd(new Repository("test", TestData.TestKANTarGz()));
                // Not an ICommand because it can update CKAN itself
                var sut  = new Update(repoData.Manager, user, manager);
                var opts = new UpdateOptions();

                // Act
                sut.RunCommand(opts);

                // Assert
                CollectionAssert.IsNotEmpty(
                    regMgr.registry.CompatibleModules(
                        inst.KSP.StabilityToleranceConfig,
                        inst.KSP.VersionCriteria()));
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "Refreshing game version data",
                                              $"Downloading {TestData.TestKANTarGz()} ...",
                                              "Updated information on 37 modules."
                                          },
                                          user.RaisedMessages);
            }
        }
    }
}
