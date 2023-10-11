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
    public class UpgradeTests
    {
        [Test,
            TestCase(new string[]
                     {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""UpgradableMod"",
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
                             ""identifier"":   ""UpgradableMod"",
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
                             ""identifier"":   ""UpgradableMod"",
                             ""version"":      ""1.2.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }"},
                     "UpgradableMod",
                     "1.0.0",
                     "1.1.0"),
        ]
        public void RunCommand_IdentifierEqualsVersionSyntax_UpgradesToCorrectVersion(
            string[] modules,
            string   identifier,
            string   fromVersion,
            string   toVersion)
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository(modules))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config)
                {
                    CurrentInstance = inst.KSP,
                })
            {
                var regMgr = RegistryManager.Instance(inst.KSP, repoData.Manager);
                regMgr.registry.RepositoriesClear();
                regMgr.registry.RepositoriesAdd(repo.repo);
                var fromModule = regMgr.registry.GetModuleByVersion(identifier, fromVersion);
                var toModule   = regMgr.registry.GetModuleByVersion(identifier, toVersion);
                regMgr.registry.RegisterModule(fromModule, Enumerable.Empty<string>(), inst.KSP, false);
                manager.Cache.Store(toModule, TestData.DogeCoinFlagZip(), null);
                var opts = new UpgradeOptions()
                {
                    modules = new List<string> { $"{identifier}={toVersion}" },
                };

                // Act
                ICommand cmd = new Upgrade(manager, repoData.Manager, user);
                cmd.RunCommand(inst.KSP, opts);

                // Assert
                Assert.Multiple(() =>
                {
                    CollectionAssert.AreEqual(Enumerable.Empty<string>(),
                                              user.RaisedErrors);
                    CollectionAssert.AreEqual(new CkanModule[] { toModule },
                                              regMgr.registry.InstalledModules.Select(m => m.Module));
                });
            }
        }
    }
}
