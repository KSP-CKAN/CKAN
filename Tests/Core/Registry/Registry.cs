using System.IO;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;
using CKAN;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Data;

namespace Tests.Core.Registry
{
    [TestFixture]
    public class Registry
    {
        private static readonly CkanModule module = TestData.kOS_014_module();
        private static readonly string identifier = module.identifier;
        private static readonly GameVersionCriteria v0_24_2 = new GameVersionCriteria(GameVersion.Parse("0.24.2"));
        private static readonly GameVersionCriteria v0_25_0 = new GameVersionCriteria (GameVersion.Parse("0.25.0"));

        private CKAN.Registry registry;

        [SetUp]
        public void Setup()
        {
            // Provide an empty registry before each test.
            registry = CKAN.Registry.Empty();
            Assert.IsNotNull(registry);
        }

        [Test]
        public void Empty()
        {
            CKAN.Registry registry = CKAN.Registry.Empty();
            Assert.IsInstanceOf<CKAN.Registry>(registry);

        }

        [Test]
        public void AddAvailable()
        {
            // We shouldn't have kOS in our registry.
            Assert.IsFalse(registry.available_modules.ContainsKey(module.identifier));

            // Register
            registry.AddAvailable(module);

            // Make sure it's now there.
            Assert.IsTrue(registry.available_modules.ContainsKey(module.identifier));
        }

        [Test]
        public void RemoveAvailableByName()
        {
            // Add our module and test it's there.
            registry.AddAvailable(module);
            Assert.IsNotNull(registry.LatestAvailable(identifier, v0_24_2));

            // Remove it, and make sure it's gone.
            registry.RemoveAvailable(identifier, module.version);

            Assert.IsNull(registry.LatestAvailable(identifier, v0_24_2));
        }

        [Test]
        public void RemoveAvailableByModule()
        {
            // Add our module and test it's there.
            registry.AddAvailable(module);
            Assert.IsNotNull(registry.LatestAvailable(identifier, v0_24_2));

            // Remove it, and make sure it's gone.
            registry.RemoveAvailable(module);

            Assert.IsNull(registry.LatestAvailable(identifier, v0_24_2));
        }

        [Test]
        public void LatestAvailable()
        {

            registry.AddAvailable(module);

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

        [Test]
        public void CompatibleModules_NoDLCInstalled_ExcludesModulesDependingOnMH()
        {
            // Arrange
            CkanModule DLCDepender = CkanModule.FromJson(@"{
                ""identifier"": ""DLC-Depender"",
                ""version"":    ""1.0.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [
                    { ""name"": ""MakingHistory-DLC"" }
                ]
            }");
            registry.AddAvailable(DLCDepender);

            // Act
            List<CkanModule> avail = registry.CompatibleModules(v0_24_2).ToList();

            // Assert
            Assert.IsFalse(avail.Contains(DLCDepender));
        }

        [Test]
        public void CompatibleModules_MHInstalled_IncludesModulesDependingOnMH()
        {
            // Arrange
            registry.RegisterDlc("MakingHistory-DLC", new UnmanagedModuleVersion("1.1.0"));

            CkanModule DLCDepender = CkanModule.FromJson(@"{
                ""identifier"": ""DLC-Depender"",
                ""version"":    ""1.0.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [
                    { ""name"": ""MakingHistory-DLC"" }
                ]
            }");
            registry.AddAvailable(DLCDepender);

            // Act
            List<CkanModule> avail = registry.CompatibleModules(v0_24_2).ToList();

            // Assert
            Assert.IsTrue(avail.Contains(DLCDepender));
        }

        [Test]
        public void CompatibleModules_MH110Installed_IncludesModulesDependingOnMH110()
        {
            // Arrange
            registry.RegisterDlc("MakingHistory-DLC", new UnmanagedModuleVersion("1.1.0"));

            CkanModule DLCDepender = CkanModule.FromJson(@"{
                ""identifier"": ""DLC-Depender"",
                ""version"":    ""1.0.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"": ""MakingHistory-DLC"",
                    ""version"": ""1.1.0""
                } ]
            }");
            registry.AddAvailable(DLCDepender);

            // Act
            List<CkanModule> avail = registry.CompatibleModules(v0_24_2).ToList();

            // Assert
            Assert.IsTrue(avail.Contains(DLCDepender));
        }

        [Test]
        public void CompatibleModules_MH100Installed_ExcludesModulesDependingOnMH110()
        {
            // Arrange
            registry.RegisterDlc("MakingHistory-DLC", new UnmanagedModuleVersion("1.0.0"));

            CkanModule DLCDepender = CkanModule.FromJson(@"{
                ""identifier"": ""DLC-Depender"",
                ""version"":    ""1.0.0"",
                ""download"":   ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""depends"": [ {
                    ""name"": ""MakingHistory-DLC"",
                    ""version"": ""1.1.0""
                } ]
            }");
            registry.AddAvailable(DLCDepender);

            // Act
            List<CkanModule> avail = registry.CompatibleModules(v0_24_2).ToList();

            // Assert
            Assert.IsFalse(avail.Contains(DLCDepender));
        }

        [Test]
        public void CompatibleModules_PastAndFutureCompatibility_ReturnsCurrentOnly()
        {
            // Arrange
            CkanModule modFor161 = CkanModule.FromJson(@"{
                ""identifier"":  ""TypicalMod"",
                ""version"":     ""0.9.0"",
                ""download"":    ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"": ""1.6.1""
            }");
            CkanModule modFor173 = CkanModule.FromJson(@"{
                ""identifier"":  ""TypicalMod"",
                ""version"":     ""1.0.0"",
                ""download"":    ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"": ""1.7.3""
            }");
            CkanModule modFor181 = CkanModule.FromJson(@"{
                ""identifier"":  ""TypicalMod"",
                ""version"":     ""1.1.0"",
                ""download"":    ""https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.01"",
                ""ksp_version"": ""1.8.1""
            }");
            registry.AddAvailable(modFor161);
            registry.AddAvailable(modFor173);
            registry.AddAvailable(modFor181);

            // Act
            GameVersionCriteria v173 = new GameVersionCriteria(GameVersion.Parse("1.7.3"));
            List<CkanModule> compat = registry.CompatibleModules(v173).ToList();

            // Assert
            Assert.IsFalse(compat.Contains(modFor161));
            Assert.IsTrue(compat.Contains(modFor173));
            Assert.IsFalse(compat.Contains(modFor181));
        }

        [Test]
        public void HasUpdate_WithUpgradeableManuallyInstalledMod_ReturnsTrue()
        {
            // Arrange
            using (var gameInstWrapper = new DisposableKSP())
            {
                CkanModule mod = CkanModule.FromJson(@"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""AutoDetectedMod"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/AD/1.0""
                }");
                registry.AddAvailable(mod);
                GameInstance gameInst = gameInstWrapper.KSP;
                registry.RegisterFile(
                    Path.Combine("GameData", $"{mod.identifier}.dll"),
                    mod.identifier);
                GameVersionCriteria crit = new GameVersionCriteria(mod.ksp_version);

                // Act
                bool has = registry.HasUpdate(mod.identifier, crit);

                // Assert
                Assert.IsTrue(has, "Can't upgrade manually installed DLL");
            }
        }

        [Test]
        public void HasUpdate_OtherModDependsOnCurrent_ReturnsFalse()
        {
            // Arrange
            using (var gameInstWrapper = new DisposableKSP())
            {
                CkanModule olderDepMod = CkanModule.FromJson(@"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""DependencyMod"",
                    ""version"":      ""1.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/DM/1.0""
                }");
                CkanModule newerDepMod = CkanModule.FromJson(@"{
                    ""spec_version"": ""v1.4"",
                    ""identifier"":   ""DependencyMod"",
                    ""version"":      ""2.0"",
                    ""ksp_version"":  ""1.11.1"",
                    ""download"":     ""https://mymods/DM/2.0""
                }");
                CkanModule dependingMod = CkanModule.FromJson(@"{
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
                }");
                registry.AddAvailable(olderDepMod);
                registry.AddAvailable(newerDepMod);
                registry.AddAvailable(dependingMod);
                GameInstance gameInst = gameInstWrapper.KSP;
                registry.RegisterModule(olderDepMod,  new string[0], gameInst, false);
                registry.RegisterModule(dependingMod, new string[0], gameInst, false);
                GameVersionCriteria crit = new GameVersionCriteria(olderDepMod.ksp_version);

                // Act
                bool has = registry.HasUpdate(olderDepMod.identifier, crit);

                // Assert
                Assert.IsFalse(has, "Upgrade allowed that would break another mod's dependency");
            }
        }

        [Test]
        public void TxEmbeddedCommit()
        {
            // Our registry should work when we initialise it inside our Tx and commit.

            CKAN.Registry reg;

            using (var scope = new TransactionScope())
            {
                reg = CKAN.Registry.Empty();
                reg.AddAvailable(module);
                Assert.AreEqual(identifier, reg.LatestAvailable(identifier, null).identifier);
                scope.Complete();
            }
            Assert.AreEqual(identifier, reg.LatestAvailable(identifier, null).identifier);
        }

        [Test]
        public void TxCommit()
        {
            // Our registry should work fine on committed transactions.

            using (var scope = new TransactionScope())
            {
                registry.AddAvailable(module);
                Assert.AreEqual(module.identifier, registry.LatestAvailable(identifier, null).identifier);

                scope.Complete();
            }
            Assert.AreEqual(module.identifier, registry.LatestAvailable(identifier, null).identifier);
        }

        [Test]
        public void TxRollback()
        {
            // Our registry should roll-back any changes it made during a transaction.

            using (var scope = new TransactionScope())
            {
                registry.AddAvailable(module);
                Assert.AreEqual(module.identifier, registry.LatestAvailable(identifier,null).identifier);

                scope.Dispose(); // Rollback, our module should no longer be available.
            }

            Assert.Throws<ModuleNotFoundKraken>(delegate
            {
                registry.LatestAvailable(identifier,null);
            });
        }

        [Test]
        public void TxNested()
        {
            // Our registry doesn't understand how to do nested transactions,
            // make sure it throws on these.

            using (var scope = new TransactionScope())
            {
                registry.AddAvailable(module);

                using (var scope2 = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    Assert.Throws<TransactionalKraken>(delegate
                    {
                        registry.AddAvailable(TestData.DogeCoinFlag_101_module());
                    });
                    scope2.Complete();
                }
                scope.Complete();
            }
        }

        [Test]
        public void TxAmbient()
        {
            // Our registry should be fine with ambient transactions, which join together.
            // Note the absence of TransactionScopeOption.RequiresNew

            using (var scope = new TransactionScope())
            {
                registry.AddAvailable(module);

                using (var scope2 = new TransactionScope())
                {
                    Assert.DoesNotThrow(delegate
                    {
                        registry.AddAvailable(TestData.DogeCoinFlag_101_module());
                    });
                    scope2.Complete();
                }
                scope.Complete();
            }
        }
    }
}
