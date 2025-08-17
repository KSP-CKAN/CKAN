using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;

using Tests.Data;
using Tests.Core.Configuration;
using System;

namespace Tests.CmdLine
{
    [TestFixture]
    public class ReplaceTests
    {
        [Test,
            TestCase(new string[]
                     {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""ReplaceableMod"",
                             ""version"":      ""1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ],
                             ""replaced_by"": { ""name"": ""ReplacementMod"" }
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""ReplacementMod"",
                             ""version"":      ""1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     "ReplaceableMod",
                     "1.0",
                     "ReplacementMod",
                     "1.0"),
        ]
        public void RunCommand_ReplaceableMod_Works(string[] modules,
                                                    string   ident1,
                                                    string   version1,
                                                    string   ident2,
                                                    string   version2)
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository(modules))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager))
            {
                manager.SetCurrentInstance(inst.KSP);
                regMgr.registry.RepositoriesClear();
                regMgr.registry.RepositoriesAdd(repo.repo);
                var module1 = regMgr.registry.GetModuleByVersion(ident1, version1)!;
                var module2 = regMgr.registry.GetModuleByVersion(ident2, version2)!;
                var opts = new ReplaceOptions()
                {
                    modules = new List<string> { ident1 },
                };

                // Act
                regMgr.registry.RegisterModule(module1, new string[] { }, inst.KSP, false);
                manager.Cache?.Store(module2, TestData.DogeCoinFlagZip(), null);
                ICommand cmd = new Replace(manager, repoData.Manager, user);
                cmd.RunCommand(inst.KSP, opts);

                // Assert
                Assert.Multiple(() =>
                {
                    CollectionAssert.AreEqual(Enumerable.Empty<string>(),
                                              user.RaisedErrors);
                    CollectionAssert.AreEqual(new CkanModule[] { module2 },
                                              regMgr.registry.InstalledModules.Select(m => m.Module));
                });
            }
        }
    }
}
