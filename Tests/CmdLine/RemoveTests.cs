using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class RemoveTests
    {
        [Test]
        public void RunCommand_RemoveAll_RemovesAll()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager))
            {
                var      registry = regMgr.registry;
                ICommand sut      = new Remove(manager, repoData.Manager, user);
                var      opts     = new RemoveOptions() { rmall = true };

                // Act
                CollectionAssert.IsNotEmpty(registry.InstalledModules);
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                CollectionAssert.IsEmpty(registry.InstalledModules);
            }
        }

        [Test]
        public void RunCommand_Regex_Removes()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager))
            {
                var      registry = regMgr.registry;
                ICommand sut      = new Remove(manager, repoData.Manager, user);
                var      opts     = new RemoveOptions()
                                    {
                                        regex = true,
                                        modules = new List<string>
                                                  {
                                                      ".*"
                                                  },
                                    };

                // Act
                CollectionAssert.IsNotEmpty(registry.InstalledModules);
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                CollectionAssert.IsEmpty(registry.InstalledModules);
            }
        }

        [Test]
        public void RunCommand_NoArguments_PrintsUsage()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            {
                ICommand sut      = new Remove(manager, repoData.Manager, user);
                var      opts     = new RemoveOptions();

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "argument missing, perhaps you forgot it?",
                                              " ",
                                              "Usage: ckan remove [options] module [module2 ...]"
                                          },
                                          user.RaisedErrors);
            }
        }
    }
}
