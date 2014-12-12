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
    }
}
