using System;
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

        [TestCase(-1, new string[] {}),
         TestCase( 0, new string[] { "InstallingMod", "Provider1" }),
         TestCase( 1, new string[] { "InstallingMod", "Provider2" }),
        ]
        public void RunCommand_VirtualDepends_PromptsUser(int answer, string[] expectedInstalled)
        {
            // Arrange
            var     asked    = false;
            string? question = null;
            var user = new CapturingUser(false, q => true,
                                         (msg, objs) =>
                                         {
                                             asked    = true;
                                             question = msg;
                                             return answer;
                                         });
            using (var inst     = new DisposableKSP())
            using (var repo     = new TemporaryRepository(Core.Relationships.RelationshipResolverTests.MergeWithDefaults(
                                      @"{
                                          ""identifier"": ""InstallingMod"",
                                          ""depends"":    [ { ""name"": ""Virtual"" } ],
                                          ""install"":    [
                                              {
                                                  ""find"":       ""DogeCoinFlag"",
                                                  ""install_to"": ""GameData"",
                                                  ""as"":         ""InstallingMod""
                                              }
                                          ]
                                      }",
                                      @"{
                                          ""identifier"": ""Provider1"",
                                          ""provides"":   [ ""Virtual"" ],
                                          ""install"":    [
                                              {
                                                  ""find"":       ""DogeCoinFlag"",
                                                  ""install_to"": ""GameData"",
                                                  ""as"":         ""Provider1""
                                              }
                                          ]
                                      }",
                                      @"{
                                          ""identifier"": ""Provider2"",
                                          ""provides"":   [ ""Virtual"" ],
                                          ""install"":    [
                                              {
                                                  ""find"":       ""DogeCoinFlag"",
                                                  ""install_to"": ""GameData"",
                                                  ""as"":         ""Provider2""
                                              }
                                          ]
                                      }")
                                      .ToArray()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                manager.SetCurrentInstance(inst.KSP);
                var opts = new InstallOptions()
                {
                    modules = new List<string> { "InstallingMod" },
                };
                ICommand cmd = new Install(manager, repoData.Manager, user);

                // Act
                foreach (var module in regMgr.registry.CompatibleModules(inst.KSP.StabilityToleranceConfig,
                                                                         inst.KSP.VersionCriteria()))
                {
                    manager.Cache!.Store(module, TestData.DogeCoinFlagZip(), null);
                }
                cmd.RunCommand(inst.KSP, opts);

                // Assert
                Assert.Multiple(() =>
                {
                    CollectionAssert.IsEmpty(user.RaisedErrors);
                    Assert.IsTrue(asked);
                    CollectionAssert.AreEqual(new string[]
                                              {
                                                  "A mod or modpack requires Virtual, which can be provided by multiple mods.",
                                                  "",
                                                  "Which Virtual provider would you like to install?",
                                              },
                                              question?.Split(new string[] { "\r\n" },
                                                              StringSplitOptions.None));
                    CollectionAssert.AreEquivalent(expectedInstalled,
                                                   regMgr.registry.InstalledModules.Select(im => im.identifier));
                });
            }
        }
    }
}
