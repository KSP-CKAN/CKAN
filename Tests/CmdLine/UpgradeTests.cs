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
                regMgr.registry.RegisterModule(fromModule, new List<string>(), inst.KSP, false);
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

        [Test,
            TestCase("No mods, do nothing without crashing",
                     new string[] { },
                     new string[] { },
                     new string[] { },
                     new string[] { }),
            TestCase("No mods, do nothing (--all) without crashing",
                     new string[] { },
                     new string[] { },
                     null,
                     new string[] { }),
            TestCase("Enforce version requirements of identifier=version specified mod",
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.0.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Depender""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Dependency""
                                 }
                             ]
                         }",
                     },
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.1.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.1.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Depender""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Dependency""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.2.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.2.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Depender""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.2.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Dependency""
                                 }
                             ]
                         }",
                     },
                     new string[] { "Depender=1.1.0", "Dependency" },
                     new string[] { "1.1.0", "1.1.0" }),
            // Installed and latest version of the depender has a version specific depends,
            // the current installed dependency is old, and we upgrade to an intermediate version
            // instead of the absolute latest
            // (lamont-granquist's use case with identifiers)
            TestCase("Should upgrade-with-identifiers to intermediate version when installed dependency blocks latest",
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.1.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
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
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.2.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     new string[] { "Dependency" },
                     new string[] { "1.0.0", "1.1.0" }),
            // Installed and latest version of the depender has a version specific depends,
            // the current installed dependency is old, and we upgrade to an intermediate version
            // instead of the absolute latest
            // (lamont-granquist's use case with --all)
            TestCase("Should upgrade-all to intermediate version when installed dependency blocks latest",
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.1.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
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
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.2.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     null,
                     new string[] { "1.0.0", "1.1.0" }),
            // Depender stops any upgrades at all (with identifiers)
            // (could be broken by naive fix for lamont-granquist's use case)
            TestCase("Depender stops any upgrades-with-identifiers at all",
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.0.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     new string[] { "Dependency" },
                     new string[] { "1.0.0", "1.0.0" }),
            // Depender stops any upgrades at all (--all)
            // (could be broken by naive fix for lamont-granquist's use case)
            TestCase("Depender stops any upgrades-all at all",
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.0.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     null,
                     new string[] { "1.0.0", "1.0.0" }),
            // Depender blocks latest dependency, but the latest available depender
            // doesn't have that limitation, and we upgrade both to latest
            // (could be broken by naive fix for lamont-granquist's use case)
            TestCase("Upgrade-with-identifiers both to bypass current version limitation",
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.0.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Depender""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Dependency""
                                 }
                             ]
                         }",
                     },
                     new string[] { "Depender", "Dependency" },
                     new string[] { "1.1.0", "1.1.0" }),
            // Depender blocks latest dependency, but the latest available depender
            // doesn't have that limitation, and we upgrade both to latest
            // (could be broken by naive fix for lamont-granquist's use case)
            TestCase("Upgrade-all both to bypass current version limitation",
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""depends"": [
                                 {
                                     ""name"":        ""Dependency"",
                                     ""max_version"": ""1.0.0""
                                 }
                             ],
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.0.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData""
                                 }
                             ]
                         }",
                     },
                     new string[] {
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Depender"",
                             ""version"":      ""1.1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Depender""
                                 }
                             ]
                         }",
                         @"{
                             ""spec_version"": 1,
                             ""identifier"":   ""Dependency"",
                             ""version"":      ""1.1.0"",
                             ""download"":     ""https://github.com/"",
                             ""install"": [
                                 {
                                     ""find"": ""DogeCoinFlag"",
                                     ""install_to"": ""GameData/Dependency""
                                 }
                             ]
                         }",
                     },
                     null,
                     new string[] { "1.1.0", "1.1.0" }),
        ]
        public void RunCommand_VersionDependsUpgrade_UpgradesCorrectly(string   description,
                                                                       string[] instModules,
                                                                       string[] addlModules,
                                                                       string[] upgradeIdentifiers,
                                                                       string[] versionsAfter)
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository(instModules.Concat(addlModules)
                                                                     .ToArray()))
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

                // Register installed mods
                var instMods = instModules.Select(CkanModule.FromJson)
                                          .ToArray();
                foreach (var fromModule in instMods)
                {
                    regMgr.registry.RegisterModule(fromModule,
                                                   new List<string>(),
                                                   inst.KSP, false);
                }
                // Pre-store mods that might be installed
                foreach (var toModule in addlModules.Select(CkanModule.FromJson))
                {
                    manager.Cache.Store(toModule, TestData.DogeCoinFlagZip(), null);
                }
                // Simulate passing `--all`
                var opts = upgradeIdentifiers != null
                    ? new UpgradeOptions()
                    {
                        modules = upgradeIdentifiers.ToList(),
                    }
                    : new UpgradeOptions()
                    {
                        modules     = new List<string>() {},
                        upgrade_all = true,
                    };

                // Act
                ICommand cmd = new Upgrade(manager, repoData.Manager, user);
                cmd.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(versionsAfter,
                                          instMods.Select(m => regMgr.registry
                                                                     .GetInstalledVersion(m.identifier)
                                                                     .version
                                                                     .ToString())
                                                  .ToArray(),
                                          description);

            }
        }
    }
}
