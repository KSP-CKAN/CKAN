using System;
using System.Collections.Generic;
using System.Linq;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using NUnit.Framework;

using CKAN;
using CKAN.GUI;
using CKAN.Versioning;

using Tests.Core.Configuration;
using Tests.Data;

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
            using (var manager = new GameInstanceManager(user, config)
                {
                    CurrentInstance = tidy.KSP
                })
            {
                var registry = new Registry(repoData.Manager, repo.repo);
                var ckan_mod = registry.GetModuleByVersion("kOS", "0.14");

                var mod = new GUIMod(ckan_mod, repoData.Manager, registry, manager.CurrentInstance.VersionCriteria(),
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
                var old_version = generator.GeneratorRandomModule(version: new ModuleVersion("0.24"),
                                                                  ksp_version: tidy.KSP.Version());
                var new_version = generator.GeneratorRandomModule(version: new ModuleVersion("0.25"),
                                                                  ksp_version: tidy.KSP.Version(),
                                                                  identifier: old_version.identifier);

                using (var repo = new TemporaryRepository(CkanModule.ToJson(old_version),
                                                          CkanModule.ToJson(new_version)))
                using (var repoData = new TemporaryRepositoryData(user, repo.repo))
                {
                    var registry = new Registry(repoData.Manager, repo.repo);

                    registry.RegisterModule(old_version, new List<string>(), null, false);
                    var upgradeableGroups = registry.CheckUpgradeable(tidy.KSP,
                                                                      new HashSet<string>());

                    var mod = new GUIMod(old_version, repoData.Manager, registry, tidy.KSP.VersionCriteria(),
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
                    ""identifier"":  ""OutOfOrderMod"",
                    ""version"":     ""1.2.0"",
                    ""ksp_version"": ""0.90"",
                    ""download"":    ""http://www.ksp-ckan.space""
                }",
                @"{
                    ""identifier"":  ""OutOfOrderMod"",
                    ""version"":     ""1.1.0"",
                    ""ksp_version"": ""1.4.2"",
                    ""download"":    ""http://www.ksp-ckan.space""
                }"))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new Registry(repoData.Manager, repo.repo);

                CkanModule mainVersion = registry.GetModuleByVersion("OutOfOrderMod", "1.2.0");
                CkanModule prevVersion = registry.GetModuleByVersion("OutOfOrderMod", "1.1.0");

                // Act
                GUIMod m = new GUIMod(mainVersion, repoData.Manager, registry, tidy.KSP.VersionCriteria(),
                                      null, false, false);

                // Assert
                Assert.AreEqual("1.4.2", m.GameCompatibilityVersion.ToString());
            }
        }

    }
}
