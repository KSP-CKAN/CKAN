using System;
using System.Linq;
using CKAN;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Core;
using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.GUI
{
    [TestFixture]
    public class GUIModTests
    {

        // TODO: Work out what mocking framework the project uses and write some more tests.
        [Test]
        public void NewGuiModsAreNotSelectedForUpgrade()
        {
            using (var tidy = new DisposableKSP())
            {
                var config = new FakeConfiguration(tidy.KSP, tidy.KSP.Name);

                GameInstanceManager manager = new GameInstanceManager(
                    new NullUser(),
                    config
                ) {
                    CurrentInstance = tidy.KSP
                };
                var registry = Registry.Empty();
                var ckan_mod = TestData.kOS_014_module();
                registry.AddAvailable(ckan_mod);
                var mod = new GUIMod(ckan_mod, registry, manager.CurrentInstance.VersionCriteria());
                Assert.False(mod.IsUpgradeChecked);

                manager.Dispose();
                config.Dispose();
            }
        }

        [Test]
        public void HasUpdateReturnsTrueWhenUpdateAvailible()
        {
            using (var tidy = new DisposableKSP())
            {
                var generatror = new RandomModuleGenerator(new Random(0451));
                var old_version = generatror.GeneratorRandomModule(version: new ModuleVersion("0.24"), ksp_version: tidy.KSP.Version());
                var new_version = generatror.GeneratorRandomModule(version: new ModuleVersion("0.25"), ksp_version: tidy.KSP.Version(),
                    identifier:old_version.identifier);
                var registry = Registry.Empty();
                registry.RegisterModule(old_version, Enumerable.Empty<string>(), null, false);
                registry.AddAvailable(new_version);

                var mod = new GUIMod(old_version, registry, tidy.KSP.VersionCriteria());
                Assert.True(mod.HasUpdate);
            }
        }

        [Test]
        public void GameCompatibility_OutOfOrderGameVersions_TrueMaxVersion()
        {
            using (var tidy = new DisposableKSP())
            {
                // Arrange
                CkanModule mainVersion = CkanModule.FromJson(@"{
                    ""identifier"":  ""OutOfOrderMod"",
                    ""version"":     ""1.2.0"",
                    ""ksp_version"": ""0.90"",
                    ""download"":    ""http://www.ksp-ckan.space""
                }");
                CkanModule prevVersion = CkanModule.FromJson(@"{
                    ""identifier"":  ""OutOfOrderMod"",
                    ""version"":     ""1.1.0"",
                    ""ksp_version"": ""1.4.2"",
                    ""download"":    ""http://www.ksp-ckan.space""
                }");

                Registry registry = Registry.Empty();
                registry.AddAvailable(mainVersion);
                registry.AddAvailable(prevVersion);

                // Act
                GUIMod m = new GUIMod(mainVersion, registry, tidy.KSP.VersionCriteria(), false);

                // Assert
                Assert.AreEqual("1.4.2", m.GameCompatibility);
            }
        }

    }
}
