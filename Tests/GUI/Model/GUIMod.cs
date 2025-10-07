using System;
using System.Collections.Generic;
using System.Linq;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using NUnit.Framework;

using CKAN;
using CKAN.Configuration;
using CKAN.GUI;
using CKAN.Versioning;

using Tests.Core.Configuration;
using Tests.Data;
using System.Windows.Forms;

namespace Tests.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [TestFixture]
    public class GUIModTests
    {

        [Test]
        public void NewGuiModsAreNotSelectedForUpgrade()
        {
            var user = new NullUser();
            using (var tidy = new DisposableKSP())
            using (var repo = new TemporaryRepository(TestData.kOS_014()))
            using (var config = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var cacheDir = new TemporaryDirectory())
            {
                var cache = new NetModuleCache(cacheDir);
                var registry = new Registry(repoData.Manager, repo.repo);
                var ckan_mod = registry.GetModuleByVersion("kOS", "0.14");

                var mod = new GUIMod(ckan_mod!, repoData.Manager, registry,
                                     tidy.KSP.StabilityToleranceConfig, tidy.KSP, cache,
                                     null, false, false);
                Assert.True(mod.SelectedMod == mod.InstalledMod?.Module);
            }
        }

        [Test]
        public void HasUpdate_UpdateAvailable_ReturnsTrue()
        {
            var user = new NullUser();
            using (var tidy = new DisposableKSP())
            {
                var generator = new RandomModuleGenerator(new Random(0451));
                var old_version = generator.GenerateRandomModule(version: new ModuleVersion("0.24"),
                                                                 ksp_version: tidy.KSP.Version());
                var new_version = generator.GenerateRandomModule(version: new ModuleVersion("0.25"),
                                                                 ksp_version: tidy.KSP.Version(),
                                                                 identifier: old_version.identifier);

                using (var repo = new TemporaryRepository(old_version.ToJson(),
                                                          new_version.ToJson()))
                using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var cacheDir = new TemporaryDirectory())
            {
                var cache = new NetModuleCache(cacheDir);
                    var registry = new Registry(repoData.Manager, repo.repo);

                    registry.RegisterModule(old_version, new List<string>(), tidy.KSP, false);
                    var upgradeableGroups = registry.CheckUpgradeable(tidy.KSP,
                                                                      new HashSet<string>());

                    var mod = new GUIMod(old_version, repoData.Manager, registry,
                                         tidy.KSP.StabilityToleranceConfig, tidy.KSP, cache,
                                         null, false, false)
                    {
                        HasUpdate = upgradeableGroups[true].Any(m => m.identifier == old_version.identifier),
                    };
                    Assert.True(mod.HasUpdate);
                }
            }
        }

        [Test]
        public void GameCompatibility_OutOfOrderGameVersions_TrueMaxVersion()
        {
            // Arrange
            var user = new NullUser();
            using (var tidy = new DisposableKSP())
            using (var repo = new TemporaryRepository(
                @"{
                    ""spec_version"": 1,
                    ""identifier"":   ""OutOfOrderMod"",
                    ""author"":       ""OutOfOrderModder"",
                    ""version"":      ""1.2.0"",
                    ""ksp_version"":  ""0.90"",
                    ""download"":     ""http://www.ksp-ckan.space""
                }",
                @"{
                    ""spec_version"": 1,
                    ""identifier"":  ""OutOfOrderMod"",
                    ""author"":       ""OutOfOrderModder"",
                    ""version"":     ""1.1.0"",
                    ""ksp_version"": ""1.4.2"",
                    ""download"":    ""http://www.ksp-ckan.space""
                }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var cacheDir = new TemporaryDirectory())
            {
                var cache = new NetModuleCache(cacheDir);
                var registry = new Registry(repoData.Manager, repo.repo);

                var mainVersion = registry.GetModuleByVersion("OutOfOrderMod", "1.2.0");
                var prevVersion = registry.GetModuleByVersion("OutOfOrderMod", "1.1.0");

                // Act
                GUIMod m = new GUIMod(mainVersion!, repoData.Manager, registry,
                                      tidy.KSP.StabilityToleranceConfig, tidy.KSP, cache,
                                      null, false, false);

                // Assert
                Assert.AreEqual("1.4.2", m.GameCompatibilityVersion?.ToString());
            }
        }

        [Test]
        public void UpdateIsCached_AfterStore_CallbackCalled()
        {
            // Arrange
            using (var cacheDir = new TemporaryDirectory())
            using (var inst = new DisposableKSP())
            {
                var cache    = new NetModuleCache(cacheDir);
                var repoData = new RepositoryDataManager();
                var sut      = new GUIMod(TestData.ModuleManagerModule(),
                                          repoData,
                                          new Registry(repoData),
                                          new StabilityToleranceConfig(""),
                                          inst.KSP, cache,
                                          false, true, true);
                bool notified = false;
                sut.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsCached")
                    {
                        notified = true;
                    }
                };

                // Act
                Assert.IsFalse(sut.IsCached);
                cache.Store(TestData.ModuleManagerModule(),
                            TestData.ModuleManagerZip(), null);
                sut.UpdateIsCached(cache);

                // Assert
                Assert.IsTrue(sut.IsCached);
                Assert.IsTrue(notified);
            }
        }

        [Test]
        public void SetAutoInstallChecked_Toggle_Notifies()
        {
            // Arrange
            var module   = TestData.ModuleManagerModule();
            var cell     = new DataGridViewCheckBoxCell() { Value = false };
            var row      = new DataGridViewRow() { Tag = module };
            var col      = new DataGridViewColumn(cell);
            var grid     = new DataGridView();
            var instMod  = new InstalledModule(null, module, Array.Empty<string>(), false);
            var repoData = new RepositoryDataManager();
            using (var inst     = new DisposableKSP())
            using (var cacheDir = new TemporaryDirectory())
            {
                var cache   = new NetModuleCache(cacheDir);
                var sut     = new GUIMod(instMod, repoData, Registry.Empty(repoData),
                                         inst.KSP.StabilityToleranceConfig, inst.KSP,
                                         cache, false, true, true);
                var callbackCount = 0;
                sut.PropertyChanged += (sender, e) => ++callbackCount;

                // Act / Assert
                row.Cells.AddRange(cell);
                grid.Columns.Add(col);
                grid.Rows.Add(row);
                sut.SetAutoInstallChecked(row, col, true);
                Assert.AreEqual(1, callbackCount);
                Assert.IsTrue(cell.Value as bool?);
                sut.SetAutoInstallChecked(row, col, false);
                Assert.AreEqual(2, callbackCount);
                Assert.IsFalse(cell.Value as bool?);
                sut.SetAutoInstallChecked(row, col, true);
                Assert.AreEqual(3, callbackCount);
                Assert.IsTrue(cell.Value as bool?);
            }
        }

        [Test]
        public void Equals_Object_Correct()
        {
            // Arrange
            var repoData = new RepositoryDataManager();
            var module   = TestData.ModuleManagerModule();
            using (var inst     = new DisposableKSP())
            using (var cacheDir = new TemporaryDirectory())
            {
                var    cache  = new NetModuleCache(cacheDir);
                var    sut    = new GUIMod(module, repoData, Registry.Empty(repoData),
                                           inst.KSP.StabilityToleranceConfig, inst.KSP,
                                           cache, false, true, true);
                object other1 = new GUIMod(module, repoData, Registry.Empty(repoData),
                                           inst.KSP.StabilityToleranceConfig, inst.KSP,
                                           cache, false, true, true);
                object other2 = new GUIMod(TestData.MissionModule(),
                                           repoData, Registry.Empty(repoData),
                                           inst.KSP.StabilityToleranceConfig, inst.KSP,
                                           cache, false, true, true);

                // Act / Assert
                Assert.IsTrue(sut.Equals(other1));
                Assert.IsFalse(sut.Equals(other2));
            }
        }
    }
}
