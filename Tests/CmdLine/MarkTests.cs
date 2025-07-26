using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class MarkTests
    {
        [Test]
        public void RunSubCommand_Auto_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager))
            {
                var         module  = TestData.DogeCoinFlag_101_module();
                regMgr.registry.RegisterModule(module, System.Array.Empty<string>(),
                                               inst.KSP, false);
                ISubCommand sut     = new Mark(manager, repoData.Manager, user);
                var         args    = new string[] { "mark", "auto", module.identifier };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.IsTrue(regMgr.registry.InstalledModule(module.identifier)
                                             ?.AutoInstalled);
            }
        }

        [Test]
        public void RunSubCommand_User_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager))
            {
                var         module  = TestData.DogeCoinFlag_101_module();
                regMgr.registry.RegisterModule(module, System.Array.Empty<string>(),
                                               inst.KSP, true);
                ISubCommand sut     = new Mark(manager, repoData.Manager, user);
                var         args    = new string[] { "mark", "user", module.identifier };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.IsFalse(regMgr.registry.InstalledModule(module.identifier)
                                              ?.AutoInstalled);
            }
        }
    }
}
