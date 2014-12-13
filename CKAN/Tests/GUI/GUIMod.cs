using System;
using System.Linq;
using CKAN;
using NUnit.Framework;
using Tests;

namespace CKANTests
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
                CKAN.KSPManager._CurrentInstance = tidy.KSP;
                var registry = CKAN.Registry.Empty();
                var ckanMod = TestData.kOS_014_module();
                registry.AddAvailable(ckanMod);
                var mod = new GUIMod(ckanMod, registry);
                Assert.False(mod.IsUpgradeChecked);
            }            
        }
        [Test]
        public void HasUpdateReturnsTrueWhenUpdateAvailible()
        {
            using (var tidy = new DisposableKSP())
            {
                
                CKAN.KSPManager._CurrentInstance = tidy.KSP;
                var generatror = new RandomModuleGenerator(new Random(0451));
                var oldVersion = generatror.GeneratorRandomModule(version: new CKAN.Version("0.24"));
                var newVersion = generatror.GeneratorRandomModule(version: new CKAN.Version("0.25"),
                    identifier:oldVersion.identifier);
                var registry = CKAN.Registry.Empty();
                registry.RegisterModule(oldVersion, Enumerable.Empty<string>(), null);
                registry.AddAvailable(newVersion);


                var mod = new GUIMod(oldVersion, registry);
                Assert.True(mod.HasUpdate);
            }
        }
    }
}
