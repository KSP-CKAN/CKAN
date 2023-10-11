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
        [Test,
            TestCase(new string[]
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
            using (var manager  = new GameInstanceManager(user, config)
                {
                    CurrentInstance = inst.KSP,
                })
            {
                var regMgr = RegistryManager.Instance(inst.KSP, repoData.Manager);
                regMgr.registry.RepositoriesClear();
                regMgr.registry.RepositoriesAdd(repo.repo);
                var module = regMgr.registry.GetModuleByVersion(identifier, version);
                manager.Cache.Store(module, TestData.DogeCoinFlagZip(), null);
                var opts = new InstallOptions()
                {
                    modules = new List<string> { $"{identifier}={version}" },
                };

                // Act
                ICommand cmd = new Install(manager, repoData.Manager, user);
                cmd.RunCommand(inst.KSP, opts);

                // Assert
                Assert.Multiple(() =>
                {
                    CollectionAssert.AreEqual(Enumerable.Empty<string>(),
                                              user.RaisedErrors);
                    CollectionAssert.AreEqual(new CkanModule[] { module },
                                              regMgr.registry.InstalledModules.Select(m => m.Module));
                });
            }
        }
    }
}
