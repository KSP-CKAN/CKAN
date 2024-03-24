using System.IO;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Tests.Data;

using CKAN;
using CKAN.Versioning;

namespace Tests.Core.Registry
{
    [TestFixture]
    public class RegistryTests
    {
        private string repoDataDir;

        private static readonly GameVersionCriteria v0_24_2 = new GameVersionCriteria(GameVersion.Parse("0.24.2"));
        private static readonly GameVersionCriteria v0_25_0 = new GameVersionCriteria(GameVersion.Parse("0.25.0"));

        [SetUp]
        public void Setup()
        {
            repoDataDir = TestData.NewTempDir();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(repoDataDir, true);
        }

        [Test]
        public void Empty()
        {
            CKAN.Registry registry = CKAN.Registry.Empty();
            Assert.IsInstanceOf<CKAN.Registry>(registry);
        }

        [Test]
        public void LatestAvailable()
        {
            var user = new NullUser();
            using (var repo = new TemporaryRepository(TestData.kOS_014()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);

                var identifier = "kOS";
                var module = registry.GetModuleByVersion(identifier, "0.14");

                // Make sure it's there for 0.24.2
                Assert.AreEqual(module.ToString(), registry.LatestAvailable(identifier, v0_24_2).ToString());

                // But not for 0.25.0
                Assert.IsNull(registry.LatestAvailable(identifier, v0_25_0));

                // And that we fail if we ask for something we don't know.
                Assert.Throws<ModuleNotFoundKraken>(delegate
                {
                    registry.LatestAvailable("ToTheMun", v0_24_2);
                });
            }
        }

        [Test]
        public void CompatibleModules_NoDLCInstalled_ExcludesModulesDependingOnMH()
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(@"{
                ""identifier"": ""DLC-Depender"",
                ""version"":    ""1.0.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [
                    { ""name"": ""MakingHistory-DLC"" }
                ]
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var DLCDepender = registry.GetModuleByVersion("DLC-Depender", "1.0.0");

                // Act
                List<CkanModule> avail = registry.CompatibleModules(v0_24_2).ToList();

                // Assert
                Assert.IsFalse(avail.Contains(DLCDepender));
            }
        }

        [Test]
        public void CompatibleModules_MHInstalled_IncludesModulesDependingOnMH()
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(@"{
                ""identifier"": ""DLC-Depender"",
                ""version"":    ""1.0.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [
                    { ""name"": ""MakingHistory-DLC"" }
                ]
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(new Dictionary<string, ModuleVersion>()
                {
                    { "MakingHistory-DLC", new UnmanagedModuleVersion("1.1.0") }
                });
                var DLCDepender = registry.GetModuleByVersion("DLC-Depender", "1.0.0");

                // Act
                List<CkanModule> avail = registry.CompatibleModules(v0_24_2).ToList();

                // Assert
                Assert.IsTrue(avail.Contains(DLCDepender));
            }
        }

        [Test]
        public void CompatibleModules_MH110Installed_IncludesModulesDependingOnMH110()
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(@"{
                ""identifier"": ""DLC-Depender"",
                ""version"":    ""1.0.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"": ""MakingHistory-DLC"",
                    ""version"": ""1.1.0""
                } ]
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(new Dictionary<string, ModuleVersion>()
                {
                    { "MakingHistory-DLC", new UnmanagedModuleVersion("1.1.0") }
                });
                var DLCDepender = registry.GetModuleByVersion("DLC-Depender", "1.0.0");

                // Act
                List<CkanModule> avail = registry.CompatibleModules(v0_24_2).ToList();

                // Assert
                Assert.IsTrue(avail.Contains(DLCDepender));
            }
        }

        [Test]
        public void CompatibleModules_MH100Installed_ExcludesModulesDependingOnMH110()
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(@"{
                ""identifier"": ""DLC-Depender"",
                ""version"":    ""1.0.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"": ""MakingHistory-DLC"",
                    ""version"": ""1.1.0""
                } ]
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(new Dictionary<string, ModuleVersion>()
                {
                    { "MakingHistory-DLC", new UnmanagedModuleVersion("1.0.0") }
                });
                var DLCDepender = registry.GetModuleByVersion("DLC-Depender", "1.0.0");

                // Act
                List<CkanModule> avail = registry.CompatibleModules(v0_24_2).ToList();

                // Assert
                Assert.IsFalse(avail.Contains(DLCDepender));
            }
        }

        [Test]
        public void SetDLCs_NullVersion_DoesNotThrow()
        {
            var registry = CKAN.Registry.Empty();
            Assert.DoesNotThrow(() =>
            {
                registry.SetDlcs(new Dictionary<string, ModuleVersion>
                {
                    { "MissingVersion", null },
                });
            }, "Missing readme.txt in a DLC shouldn't trigger an exception");
        }

        [Test]
        public void CompatibleModules_PastAndFutureCompatibility_ReturnsCurrentOnly()
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(@"{
                ""identifier"":  ""TypicalMod"",
                ""version"":     ""0.9.0"",
                ""download"":    ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"": ""1.6.1""
            }",
            @"{
                ""identifier"":  ""TypicalMod"",
                ""version"":     ""1.0.0"",
                ""download"":    ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"": ""1.7.3""
            }",
            @"{
                ""identifier"":  ""TypicalMod"",
                ""version"":     ""1.1.0"",
                ""download"":    ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"": ""1.8.1""
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var modFor161 = registry.GetModuleByVersion("TypicalMod", "0.9.0");
                var modFor173 = registry.GetModuleByVersion("TypicalMod", "1.0.0");
                var modFor181 = registry.GetModuleByVersion("TypicalMod", "1.1.0");

                // Act
                GameVersionCriteria v173 = new GameVersionCriteria(GameVersion.Parse("1.7.3"));
                List<CkanModule> compat = registry.CompatibleModules(v173).ToList();

                // Assert
                Assert.IsFalse(compat.Contains(modFor161));
                Assert.IsTrue(compat.Contains(modFor173));
                Assert.IsFalse(compat.Contains(modFor181));
            }
        }

        [Test]
        public void HasUpdate_WithUpgradeableManuallyInstalledMod_ReturnsTrue()
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(@"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""AutoDetectedMod"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/AD/1.0""
                }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var gameInstWrapper = new DisposableKSP())
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var mod = registry.GetModuleByVersion("AutoDetectedMod", "1.0");

                GameInstance gameInst = gameInstWrapper.KSP;
                gameInst.SetCompatibleVersions(new List<GameVersion> { mod.ksp_version });
                registry.SetDlls(new Dictionary<string, string>()
                {
                    {
                        mod.identifier,
                        gameInst.ToRelativeGameDir(Path.Combine(gameInst.GameDir(),
                                                                "GameData", $"{mod.identifier}.dll"))
                    }
                });

                // Act
                bool has = registry.HasUpdate(mod.identifier, gameInst, out _);

                // Assert
                Assert.IsTrue(has, "Can't upgrade manually installed DLL");
            }
        }

        [Test]
        public void HasUpdate_OtherModDependsOnCurrent_ReturnsFalse()
        {
            // Arrange
            var user = new NullUser();
            using (var gameInstWrapper = new DisposableKSP())
            using (var repo = new TemporaryRepository(@"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""DependencyMod"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/DM/1.0""
                }",
                @"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""DependencyMod"",
                    ""version"":      ""2.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/DM/2.0""
                }",
                @"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""DependingMod"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/DM/2.0"",
                    ""depends"": [
                        {
                            ""name"":    ""DependencyMod"",
                            ""version"": ""1.0""
                        }
                    ]
                }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                CkanModule olderDepMod = registry.GetModuleByVersion("DependencyMod", "1.0");
                CkanModule newerDepMod = registry.GetModuleByVersion("DependencyMod", "2.0");
                CkanModule dependingMod = registry.GetModuleByVersion("DependingMod", "1.0");

                GameInstance gameInst = gameInstWrapper.KSP;
                registry.RegisterModule(olderDepMod,  new List<string>(), gameInst, false);
                registry.RegisterModule(dependingMod, new List<string>(), gameInst, false);
                GameVersionCriteria crit = new GameVersionCriteria(olderDepMod.ksp_version);

                // Act
                bool has = registry.HasUpdate(olderDepMod.identifier, gameInst, out _,
                                              registry.InstalledModules
                                                      .Select(im => im.Module)
                                                      .ToList());

                // Assert
                Assert.IsFalse(has, "Upgrade allowed that would break another mod's dependency");
            }
        }

        [Test,
            // Empty registry, return nothing
            TestCase(new string[] { },
                     new string[] { }),
            // One per host, sort by alphanumeric
            TestCase(new string[]
                     {
                         @"{
                            ""identifier"": ""ModA"",
                            ""version"": ""1.0"",
                            ""download"": [
                                ""https://archive.org/"",
                                ""https://spacedock.info/"",
                                ""https://github.com/""
                            ]
                         }",
                     },
                     new string[]
                     {
                         "archive.org", "github.com", "spacedock.info"
                     }),
            // Multiple per host, sort by frequency
            TestCase(new string[]
                     {
                         @"{
                            ""identifier"": ""ModA"",
                            ""version"": ""1.0"",
                            ""download"": [
                                ""https://archive.org/"",
                                ""https://spacedock.info/"",
                                ""https://github.com/""
                            ]
                         }",
                         @"{
                            ""identifier"": ""ModB"",
                            ""version"": ""1.0"",
                            ""download"": [
                                ""https://spacedock.info/"",
                                ""https://github.com/""
                            ]
                         }",
                         @"{
                            ""identifier"": ""ModC"",
                            ""version"": ""1.0"",
                            ""download"": [
                                ""https://github.com/""
                            ]
                         }",
                     },
                     new string[]
                     {
                         "github.com", "spacedock.info", "archive.org"
                     }),
        ]
        public void GetAllHosts_WithModules_ReturnsCorrectList(string[] modules,
                                                               string[] correctAnswer)
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(modules))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                // Act
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var allHosts = registry.GetAllHosts().ToArray();

                // Assert
                Assert.AreEqual(correctAnswer, allHosts);
            }
        }

        [Test]
        public void TxEmbeddedCommit()
        {
            // Our registry should work when we initialise it inside our Tx and commit.
            // This one seemingly just makes sure adding a mod adds it
            // when the registry is created inside the transaction
            var module = CkanModule.FromJson(@"{
                             ""spec_version"": ""v1.4"",
                             ""identifier"":   ""InstalledMod"",
                             ""version"":      ""1.0"",
                             ""download"":     ""https://github.com/""
                         }");
            CKAN.Registry reg;

            using (var gameInstWrapper = new DisposableKSP())
            using (var tScope = new TransactionScope())
            {
                reg = CKAN.Registry.Empty();
                reg.RegisterModule(module, new List<string>(),
                                   gameInstWrapper.KSP, false);

                CollectionAssert.AreEqual(
                    Enumerable.Repeat(module, 1),
                    reg.InstalledModules.Select(im => im.Module));

                tScope.Complete();
            }
            CollectionAssert.AreEqual(
                Enumerable.Repeat(module, 1),
                reg.InstalledModules.Select(im => im.Module));
        }

        [Test]
        public void TxCommit()
        {
            // Our registry should work fine on committed transactions.
            // This one seemingly just makes sure adding a mod adds it
            // when the registry is created outside the transaction
            var module = CkanModule.FromJson(@"{
                             ""spec_version"": ""v1.4"",
                             ""identifier"":   ""InstalledMod"",
                             ""version"":      ""1.0"",
                             ""download"":     ""https://github.com/""
                         }");
            var registry = CKAN.Registry.Empty();

            using (var gameInstWrapper = new DisposableKSP())
            using (var tScope = new TransactionScope())
            {
                registry.RegisterModule(module, new List<string>(),
                                        gameInstWrapper.KSP, false);

                CollectionAssert.AreEqual(
                    Enumerable.Repeat(module, 1),
                    registry.InstalledModules.Select(im => im.Module));

                tScope.Complete();
            }
            CollectionAssert.AreEqual(
                Enumerable.Repeat(module, 1),
                registry.InstalledModules.Select(im => im.Module));
        }

        [Test]
        public void TxRollback()
        {
            // Our registry should roll-back any changes it made during a transaction.
            // This one makes sure that aborting the transaction rolls back the change
            var registry = CKAN.Registry.Empty();

            using (var gameInstWrapper = new DisposableKSP())
            using (var tScope = new TransactionScope())
            {
                var module = CkanModule.FromJson(@"{
                                 ""spec_version"": ""v1.4"",
                                 ""identifier"":   ""InstalledMod"",
                                 ""version"":      ""1.0"",
                                 ""download"":     ""https://github.com/""
                             }");
                registry.RegisterModule(module, new List<string>(),
                                        gameInstWrapper.KSP, false);

                CollectionAssert.AreEqual(
                    Enumerable.Repeat(module, 1),
                    registry.InstalledModules.Select(im => im.Module));

                // Rollback, our module should no longer be registered
                tScope.Dispose();
            }

            CollectionAssert.AreEqual(
                Enumerable.Empty<CkanModule>(),
                registry.InstalledModules.Select(im => im.Module));
        }

        [Test]
        public void TxNested()
        {
            // Our registry doesn't understand how to do nested transactions,
            // make sure it throws on these.
            // This one makes sure that one transaction inside another both work
            // (except it doesn't check it? just that nothing throws?)
            var registry = CKAN.Registry.Empty();

            using (var gameInstWrapper = new DisposableKSP())
            using (var tScope = new TransactionScope())
            {
                var module = CkanModule.FromJson(@"{
                                 ""spec_version"": ""v1.4"",
                                 ""identifier"":   ""InstalledMod"",
                                 ""version"":      ""1.0"",
                                 ""download"":     ""https://github.com/""
                             }");
                registry.RegisterModule(module, new List<string>(),
                                        gameInstWrapper.KSP, false);

                using (var tScope2 = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    Assert.Throws<TransactionalKraken>(delegate
                    {
                        var module2 = CkanModule.FromJson(@"{
                                          ""spec_version"": ""v1.4"",
                                          ""identifier"":   ""InstalledMod2"",
                                          ""version"":      ""1.0"",
                                          ""download"":     ""https://github.com/""
                                      }");
                        registry.RegisterModule(module2, new List<string>(),
                                                gameInstWrapper.KSP, false);
                    });
                    tScope2.Complete();
                }
                tScope.Complete();
            }
        }

        [Test]
        public void TxAmbient()
        {
            // Our registry should be fine with ambient transactions, which join together.
            // Note the absence of TransactionScopeOption.RequiresNew
            var registry = CKAN.Registry.Empty();

            using (var gameInstWrapper = new DisposableKSP())
            using (var tScope = new TransactionScope())
            {
                var module = CkanModule.FromJson(@"{
                                 ""spec_version"": ""v1.4"",
                                 ""identifier"":   ""InstalledMod"",
                                 ""version"":      ""1.0"",
                                 ""download"":     ""https://github.com/""
                             }");
                registry.RegisterModule(module, new List<string>(),
                                        gameInstWrapper.KSP, false);

                using (var tScope2 = new TransactionScope())
                {
                    Assert.DoesNotThrow(delegate
                    {
                        var module2 = CkanModule.FromJson(@"{
                                          ""spec_version"": ""v1.4"",
                                          ""identifier"":   ""InstalledMod2"",
                                          ""version"":      ""1.0"",
                                          ""download"":     ""https://github.com/""
                                      }");
                        registry.RegisterModule(module2, new List<string>(),
                                                gameInstWrapper.KSP, false);
                    });
                    tScope2.Complete();
                }
                tScope.Complete();
            }
        }
    }
}
