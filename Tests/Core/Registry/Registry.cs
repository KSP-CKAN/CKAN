using System;
using System.IO;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Tests.Data;

using CKAN;
using CKAN.Configuration;
using CKAN.Versioning;

namespace Tests.Core.Registry
{
    [TestFixture]
    public class RegistryTests
    {
        private string? repoDataDir;
        private readonly StabilityToleranceConfig stabilityTolerance = new StabilityToleranceConfig("");

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
            if (repoDataDir != null)
            {
                Directory.Delete(repoDataDir, true);
            }
        }

        [Test]
        public void Empty()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            {
                CKAN.Registry registry = CKAN.Registry.Empty(repoData.Manager);
                Assert.IsInstanceOf<CKAN.Registry>(registry);
            }
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
                Assert.AreEqual(module?.ToString(), registry.LatestAvailable(identifier, stabilityTolerance, v0_24_2)?.ToString());

                // But not for 0.25.0
                Assert.IsNull(registry.LatestAvailable(identifier, stabilityTolerance, v0_25_0));

                // And that we fail if we ask for something we don't know.
                Assert.Throws<ModuleNotFoundKraken>(delegate
                {
                    registry.LatestAvailable("ToTheMun", stabilityTolerance, v0_24_2);
                });
            }
        }

        [Test]
        public void CompatibleModules_NoDLCInstalled_ExcludesModulesDependingOnMH()
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(@"{
                ""spec_version"": 1,
                ""identifier"":   ""DLC-Depender"",
                ""author"":       ""Modder"",
                ""version"":      ""1.0.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [
                    { ""name"": ""MakingHistory-DLC"" }
                ]
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var DLCDepender = registry.GetModuleByVersion("DLC-Depender", "1.0.0");

                // Act
                var avail = registry.CompatibleModules(stabilityTolerance, v0_24_2).OfType<CkanModule?>().ToList();

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
                ""spec_version"": 1,
                ""identifier"":   ""DLC-Depender"",
                ""author"":       ""Modder"",
                ""version"":      ""1.0.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [
                    { ""name"": ""MakingHistory-DLC"" }
                ]
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(new Dictionary<string, UnmanagedModuleVersion>()
                {
                    { "MakingHistory-DLC", new UnmanagedModuleVersion("1.1.0") }
                });
                var DLCDepender = registry.GetModuleByVersion("DLC-Depender", "1.0.0");

                // Act
                var avail = registry.CompatibleModules(stabilityTolerance, v0_24_2).OfType<CkanModule?>().ToList();

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
                ""spec_version"": 1,
                ""identifier"":   ""DLC-Depender"",
                ""author"":       ""Modder"",
                ""version"":      ""1.0.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"": ""MakingHistory-DLC"",
                    ""version"": ""1.1.0""
                } ]
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(new Dictionary<string, UnmanagedModuleVersion>()
                {
                    { "MakingHistory-DLC", new UnmanagedModuleVersion("1.1.0") }
                });
                var DLCDepender = registry.GetModuleByVersion("DLC-Depender", "1.0.0");

                // Act
                var avail = registry.CompatibleModules(stabilityTolerance, v0_24_2).OfType<CkanModule?>().ToList();

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
                ""spec_version"": 1,
                ""identifier"":   ""DLC-Depender"",
                ""author"":       ""Modder"",
                ""version"":      ""1.0.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"": ""MakingHistory-DLC"",
                    ""version"": ""1.1.0""
                } ]
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                registry.SetDlcs(new Dictionary<string, UnmanagedModuleVersion>()
                {
                    { "MakingHistory-DLC", new UnmanagedModuleVersion("1.0.0") }
                });
                var DLCDepender = registry.GetModuleByVersion("DLC-Depender", "1.0.0");

                // Act
                var avail = registry.CompatibleModules(stabilityTolerance, v0_24_2).OfType<CkanModule?>().ToList();

                // Assert
                Assert.IsFalse(avail.Contains(DLCDepender));
            }
        }

        [Test]
        public void InstalledDlc_BothDLCsSerializedDeserialized_StillThere()
        {
            // Arrange
            using (var instance = new DisposableKSP())
            using (var repo     = new TemporaryRepository())
            using (var repoData = new TemporaryRepositoryData(new NullUser(), repo.repo))
            {
                using (var manager = RegistryManager.Instance(instance.KSP, repoData.Manager,
                                                              new Repository[] { repo.repo }))
                {
                    // Act
                    manager.registry.SetDlcs(new Dictionary<string, UnmanagedModuleVersion>()
                    {
                        { "MakingHistory-DLC",  new UnmanagedModuleVersion("1.1.0") },
                        { "BreakingGround-DLC", new UnmanagedModuleVersion("1.1.0") },
                    });
                    manager.Save();
                }
                using (var manager = RegistryManager.Instance(instance.KSP, repoData.Manager,
                                                              new Repository[] { repo.repo }))
                {
                    // Assert
                    CollectionAssert.IsSupersetOf(manager.registry.InstalledDlc.Keys,
                                                  new string[]
                                                  {
                                                      "MakingHistory-DLC",
                                                      "BreakingGround-DLC",
                                                  });
                }
            }
        }

        [Test]
        public void CompatibleModules_PastAndFutureCompatibility_ReturnsCurrentOnly()
        {
            // Arrange
            var user = new NullUser();
            using (var repo = new TemporaryRepository(@"{
                ""spec_version"": 1,
                ""identifier"":   ""TypicalMod"",
                ""author"":       ""Modder"",
                ""version"":      ""0.9.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"":  ""1.6.1""
            }",
            @"{
                ""spec_version"": 1,
                ""identifier"":   ""TypicalMod"",
                ""author"":       ""Modder"",
                ""version"":      ""1.0.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"":  ""1.7.3""
            }",
            @"{
                ""spec_version"": 1,
                ""identifier"":   ""TypicalMod"",
                ""author"":       ""Modder"",
                ""version"":      ""1.1.0"",
                ""download"":     ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"":  ""1.8.1""
            }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var modFor161 = registry.GetModuleByVersion("TypicalMod", "0.9.0");
                var modFor173 = registry.GetModuleByVersion("TypicalMod", "1.0.0");
                var modFor181 = registry.GetModuleByVersion("TypicalMod", "1.1.0");

                // Act
                GameVersionCriteria v173 = new GameVersionCriteria(GameVersion.Parse("1.7.3"));
                var compat = registry.CompatibleModules(stabilityTolerance, v173).OfType<CkanModule?>().ToList();

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
                gameInst.SetCompatibleVersions(new List<GameVersion> { mod?.ksp_version! });
                registry.SetDlls(new Dictionary<string, string>()
                {
                    {
                        mod!.identifier,
                        gameInst.ToRelativeGameDir(Path.Combine(gameInst.GameDir(),
                                                                "GameData", $"{mod!.identifier}.dll"))
                    }
                });

                // Act
                bool has = registry.HasUpdate(mod.identifier, stabilityTolerance, gameInst, new HashSet<string>(), false, out _);

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
                    ""author"":       ""Modder"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/DM/1.0""
                }",
                @"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""DependencyMod"",
                    ""author"":       ""Modder"",
                    ""version"":      ""2.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/DM/2.0""
                }",
                @"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""DependingMod"",
                    ""author"":       ""Modder"",
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
                var olderDepMod = registry.GetModuleByVersion("DependencyMod", "1.0");
                var newerDepMod = registry.GetModuleByVersion("DependencyMod", "2.0");
                var dependingMod = registry.GetModuleByVersion("DependingMod", "1.0");

                GameInstance gameInst = gameInstWrapper.KSP;
                registry.RegisterModule(olderDepMod!,  new List<string>(), gameInst, false);
                registry.RegisterModule(dependingMod!, new List<string>(), gameInst, false);
                GameVersionCriteria crit = new GameVersionCriteria(olderDepMod?.ksp_version);

                // Act
                bool has = registry.HasUpdate(olderDepMod?.identifier!, stabilityTolerance, gameInst, new HashSet<string>(), false,
                                              out _,
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
                            ""spec_version"": 1,
                            ""identifier"": ""ModA"",
                            ""author"": ""Modder"",
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
                            ""spec_version"": 1,
                            ""identifier"": ""ModA"",
                            ""author"": ""Modder"",
                            ""version"": ""1.0"",
                            ""download"": [
                                ""https://archive.org/"",
                                ""https://spacedock.info/"",
                                ""https://github.com/""
                            ]
                         }",
                         @"{
                            ""spec_version"": 1,
                            ""identifier"": ""ModB"",
                            ""author"": ""Modder"",
                            ""version"": ""1.0"",
                            ""download"": [
                                ""https://spacedock.info/"",
                                ""https://github.com/""
                            ]
                         }",
                         @"{
                            ""spec_version"": 1,
                            ""identifier"": ""ModC"",
                            ""author"": ""Modder"",
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
        public void FindRemovableAutoInstalled_StillNeeded_Kept()
        {
            // Arrange
            var user = new NullUser();
            using (var gameInstWrapper = new DisposableKSP())
            using (var repo            = new TemporaryRepository(TestData.OuterPlanetsLibraryMetadata))
            using (var repoData        = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var chosen   = registry.GetModuleByVersion("OuterPlanetsMod", "1.0")!;
                var kopt     = registry.GetModuleByVersion("KopernicusTech",  "1.0")!;
                var mm       = registry.GetModuleByVersion("ModuleManager",   "1.0")!;
                registry.RegisterModule(chosen, Array.Empty<string>(), gameInstWrapper.KSP, false);
                registry.RegisterModule(kopt,   Array.Empty<string>(), gameInstWrapper.KSP, true);
                registry.RegisterModule(mm,     Array.Empty<string>(), gameInstWrapper.KSP, true);
                var autoInstDep = registry.InstalledModule(kopt.identifier)!;
                var installed   = new InstalledModule[] { registry.InstalledModule(chosen.identifier)!,
                                                          registry.InstalledModule(kopt.identifier)!, };
                var installing  = Array.Empty<CkanModule>();

                // Act
                var removable = registry.FindRemovableAutoInstalled(
                                    installed, installing,
                                    gameInstWrapper.KSP.game,
                                    stabilityTolerance,
                                    gameInstWrapper.KSP.VersionCriteria());

                // Assert
                CollectionAssert.AreEquivalent(Array.Empty<InstalledModule>(),
                                               removable);
            }
        }

        [Test]
        public void FindRemovableAutoInstalled_StillNeeded2_Kept()
        {
            // Arrange
            var user = new NullUser();
            using (var gameInstWrapper = new DisposableKSP())
            using (var repo            = new TemporaryRepository(TestData.OuterPlanetsLibraryMetadata))
            using (var repoData        = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new CKAN.Registry(repoData.Manager, repo.repo);
                var chosen   = registry.GetModuleByVersion("OuterPlanetsMod",         "2.0")!;
                var kop      = registry.GetModuleByVersion("Kopernicus",              "1.0")!;
                var mfi      = registry.GetModuleByVersion("ModularFlightIntegrator", "1.0")!;
                var mm       = registry.GetModuleByVersion("ModuleManager",           "1.0")!;
                var instChosen = registry.RegisterModule(chosen, Array.Empty<string>(), gameInstWrapper.KSP, false);
                var instKop    = registry.RegisterModule(kop,    Array.Empty<string>(), gameInstWrapper.KSP, true);
                var instMfi    = registry.RegisterModule(mfi,    Array.Empty<string>(), gameInstWrapper.KSP, true);
                var instMM     = registry.RegisterModule(mm,     Array.Empty<string>(), gameInstWrapper.KSP, true);
                var installed  = new InstalledModule[] { instChosen, instKop, instMfi, instMM, };
                var installing = Array.Empty<CkanModule>();

                // Act
                var removable = registry.FindRemovableAutoInstalled(
                                    installed, installing,
                                    gameInstWrapper.KSP.game,
                                    stabilityTolerance,
                                    gameInstWrapper.KSP.VersionCriteria());

                // Assert
                CollectionAssert.AreEquivalent(Array.Empty<InstalledModule>(),
                                               removable);
            }
        }

        [Test]
        public void FindRemovableAutoInstalled_InstallingDepWithConflict_FindsOldConflictingDep()
        {
            // Arrange
            var user = new NullUser();
            using (var gameInstWrapper = new DisposableKSP())
            using (var repo            = new TemporaryRepository(TestData.OuterPlanetsLibraryMetadata))
            using (var repoData        = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry     = new CKAN.Registry(repoData.Manager, repo.repo);
                var firstChosen  = registry.GetModuleByVersion("OuterPlanetsMod", "1.0")!;
                var kopt         = registry.GetModuleByVersion("KopernicusTech",  "1.0")!;
                var secondChosen = registry.GetModuleByVersion("OuterPlanetsMod", "2.0")!;
                var mm           = registry.GetModuleByVersion("ModuleManager",   "1.0")!;
                registry.RegisterModule(firstChosen, Array.Empty<string>(), gameInstWrapper.KSP, false);
                var instKopt = registry.RegisterModule(kopt, Array.Empty<string>(), gameInstWrapper.KSP, true);
                var instMM   = registry.RegisterModule(mm,   Array.Empty<string>(), gameInstWrapper.KSP, true);
                var autoInstDeps = new InstalledModule[] { instKopt, };
                var installed    = new InstalledModule[] { instKopt, instMM, };
                var installing   = new CkanModule[] { secondChosen };

                // Act
                var removable = registry.FindRemovableAutoInstalled(
                                    installed, installing,
                                    gameInstWrapper.KSP.game,
                                    stabilityTolerance,
                                    gameInstWrapper.KSP.VersionCriteria());

                // Assert
                CollectionAssert.AreEquivalent(autoInstDeps,
                                               removable);
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
                             ""author"":       ""InstalledModder"",
                             ""version"":      ""1.0"",
                             ""download"":     ""https://github.com/""
                         }");
            CKAN.Registry reg;

            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var gameInstWrapper = new DisposableKSP())
            using (var tScope = new TransactionScope())
            {
                reg = CKAN.Registry.Empty(repoData.Manager);
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
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            {
                // Our registry should work fine on committed transactions.
                // This one seemingly just makes sure adding a mod adds it
                // when the registry is created outside the transaction
                var module = CkanModule.FromJson(@"{
                                 ""spec_version"": ""v1.4"",
                                 ""identifier"":   ""InstalledMod"",
                                 ""author"":       ""InstalledModder"",
                                 ""version"":      ""1.0"",
                                 ""download"":     ""https://github.com/""
                             }");
                var registry = CKAN.Registry.Empty(repoData.Manager);

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
        }

        [Test]
        public void TxRollback()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            {
                // Our registry should roll-back any changes it made during a transaction.
                // This one makes sure that aborting the transaction rolls back the change
                var registry = CKAN.Registry.Empty(repoData.Manager);

                using (var gameInstWrapper = new DisposableKSP())
                using (var tScope = new TransactionScope())
                {
                    var module = CkanModule.FromJson(@"{
                                     ""spec_version"": ""v1.4"",
                                     ""identifier"":   ""InstalledMod"",
                                     ""author"":       ""InstalledModder"",
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
        }

        [Test]
        public void TxNested()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            {
                // Our registry doesn't understand how to do nested transactions,
                // make sure it throws on these.
                // This one makes sure that one transaction inside another both work
                // (except it doesn't check it? just that nothing throws?)
                var registry = CKAN.Registry.Empty(repoData.Manager);

                using (var gameInstWrapper = new DisposableKSP())
                using (var tScope = new TransactionScope())
                {
                    var module = CkanModule.FromJson(@"{
                                     ""spec_version"": ""v1.4"",
                                     ""identifier"":   ""InstalledMod"",
                                     ""author"":       ""InstalledModder"",
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
                                              ""author"":       ""InstalledModder"",
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

        [Test]
        public void TxAmbient()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            {
                // Our registry should be fine with ambient transactions, which join together.
                // Note the absence of TransactionScopeOption.RequiresNew
                var registry = CKAN.Registry.Empty(repoData.Manager);

                using (var gameInstWrapper = new DisposableKSP())
                using (var tScope = new TransactionScope())
                {
                    var module = CkanModule.FromJson(@"{
                                     ""spec_version"": ""v1.4"",
                                     ""identifier"":   ""InstalledMod"",
                                     ""author"":       ""InstalledModder"",
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
                                              ""author"":       ""InstalledModder"",
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
}
