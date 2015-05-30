using System;
using System.Linq;
using CKAN;
using NUnit.Framework;
using Tests.Core;
using Tests.Data;
using Version = CKAN.Version;

namespace Tests.GUI
{
    [TestFixture]
    public class GUIModTests
    {
        //TODO Work out what mocking framework the project uses and write some more tests.
        [Test]
        public void NewGuiModsAreNotSelectedForUpgrade()
        {
            using (var tidy = new DisposableKSP())
            {
                KSPManager manager = new KSPManager(new NullUser(), new FakeWin32Registry(tidy.KSP)){CurrentInstance = tidy.KSP};
                var registry = Registry.Empty();
                var ckan_mod = TestData.kOS_014_module();
                registry.AddAvailable(ckan_mod);
                var mod = new GUIMod(ckan_mod, registry, manager.CurrentInstance.Version());
                Assert.False(mod.IsUpgradeChecked);
            }            
        }
        [Test]
        public void HasUpdateReturnsTrueWhenUpdateAvailible()
        {
            using (var tidy = new DisposableKSP())
            {
                var generatror = new RandomModuleGenerator(new Random(0451));
                var old_version = generatror.GeneratorRandomModule(version: new Version("0.24"), ksp_version: tidy.KSP.Version());
                var new_version = generatror.GeneratorRandomModule(version: new Version("0.25"), ksp_version: tidy.KSP.Version(),
                    identifier:old_version.identifier);
                var registry = Registry.Empty();
                registry.RegisterModule(old_version, Enumerable.Empty<string>(), null);
                registry.AddAvailable(new_version);
                
                var mod = new GUIMod(old_version, registry, tidy.KSP.Version());
                Assert.True(mod.HasUpdate);
            }
        }
    }
}
