using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;

using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class InstallTests
    {
        [TestCase(new string[]
                  {
                      @"{
                          ""spec_version"": 1,
                          ""identifier"":   ""InstallableMod"",
                          ""version"":      ""1.0.0"",
                          ""download"":     ""https://github.com/"",
                          ""install"": [
                              {
                                  ""find"": ""DogeCoinFlag"",
                                  ""install_to"": ""GameData""
                              }
                          ]
                      }",
                      @"{
                          ""spec_version"": 1,
                          ""identifier"":   ""InstallableMod"",
                          ""version"":      ""1.1.0"",
                          ""download"":     ""https://github.com/"",
                          ""install"": [
                              {
                                  ""find"": ""DogeCoinFlag"",
                                  ""install_to"": ""GameData""
                              }
                          ]
                      }",
                      @"{
                          ""spec_version"": 1,
                          ""identifier"":   ""InstallableMod"",
                          ""version"":      ""1.2.0"",
                          ""download"":     ""https://github.com/"",
                          ""install"": [
                              {
                                  ""find"": ""DogeCoinFlag"",
                                  ""install_to"": ""GameData""
                              }
                          ]
                      }"
                  },
                  "InstallableMod",
                  "1.1.0"),
        ]
        public void RunCommand_IdentifierEqualsVersionSyntax_InstallsCorrectVersion(
            string[] modules,
            string   identifier,
            string   version)
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository(modules))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                manager.SetCurrentInstance(inst.KSP);
                var module = regMgr.registry.GetModuleByVersion(identifier, version)!;
                manager.Cache?.Store(module, TestData.DogeCoinFlagZip(), null);
                var opts = new InstallOptions()
                {
                    modules = new List<string> { $"{identifier}={version}" },
                };
                ICommand cmd = new Install(manager, repoData.Manager, user);

                // Act
                cmd.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                CollectionAssert.AreEqual(new CkanModule[] { module },
                                          regMgr.registry.InstalledModules.Select(m => m.Module));
            }
        }

        [Test]
        public void RunCommand_FromCkanFile_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository())
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                manager.SetCurrentInstance(inst.KSP);
                manager.Cache?.Store(TestData.ModuleManagerModule(),
                                     TestData.ModuleManagerZip(), null);
                var opts = new InstallOptions()
                {
                    ckan_files = new string[] { TestData.ModuleManagerModuleCkan() },
                };
                ICommand cmd = new Install(manager, repoData.Manager, user);

                // Act
                cmd.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                CollectionAssert.AreEqual(new CkanModule[] { TestData.ModuleManagerModule() },
                                          regMgr.registry.InstalledModules.Select(m => m.Module));
            }
        }

        [Test]
        public void RunCommand_NoArguments_PrintsHelp()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository())
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                manager.SetCurrentInstance(inst.KSP);
                var opts = new InstallOptions()
                {
                    modules = new List<string>(),
                };
                ICommand cmd = new Install(manager, repoData.Manager, user);

                // Act
                cmd.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "argument missing, perhaps you forgot it?",
                                              " ",
                                              "Usage: ckan install [options] module [module2 ...]"
                                          },
                                          user.RaisedErrors);
            }
        }
    }
}
