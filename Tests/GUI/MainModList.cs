using System;
using CKAN;
using NUnit.Framework;

namespace Tests.GUI
{
    [TestFixture]
    public class MainModListTests
    {
        [Test]
        public void OnCreation_HasDefaultFilters()
        {
            var item = new MainModList(delegate { });
            Assert.That(item.ModFilter.Equals(GUIModFilter.All));
            Assert.That(item.ModNameFilter.Equals(String.Empty));
        }

        [Test]
        public void OnModTextFilterChanges_CallsEventHandler()
        {
            var called_n = 0;
            var item = new MainModList(delegate { called_n++; });
            Assert.That(called_n == 1);
            item.ModNameFilter = "randomString";
            Assert.That(called_n == 2);
            item.ModNameFilter = "randomString";
            Assert.That(called_n == 2);
        }
        [Test]
        public void OnModTypeFilterChanges_CallsEventHandler()
        {
            var called_n = 0;
            var item = new MainModList(delegate { called_n++; });
            Assert.That(called_n == 1);
            item.ModFilter = GUIModFilter.Installed;
            Assert.That(called_n == 2);
            item.ModFilter = GUIModFilter.Installed;
            Assert.That(called_n == 2);
        }

        [Test]
        public void ComputeChangeSetFromModList_WithEmptyList_HasEmptyChangeSet()
        {
            using (var tidy = new DisposableKSP())
            {
                KSPManager manager = new KSPManager(new NullUser(), new FakeWin32Registry(tidy.KSP)) { CurrentInstance = tidy.KSP };
                var item = new MainModList(delegate { });
                Assert.That(item.ComputeChangeSetFromModList(Registry.Empty(), manager.CurrentInstance), Is.Empty);
            }
        }

        [Test]
        public void IsVisible_WithAllAndNoNameFilter_ReturnsTrueForCompatible()
        {
            using (var tidy = new DisposableKSP())
            {
                KSPManager manager = new KSPManager(new NullUser(), new FakeWin32Registry(tidy.KSP)) { CurrentInstance = tidy.KSP };

                var ckan_mod = TestData.FireSpitterModule();
                var registry = Registry.Empty();
                registry.AddAvailable(ckan_mod);
                var item = new MainModList(delegate { });
                Assert.That(item.IsVisible(new GUIMod(ckan_mod, registry, manager.CurrentInstance.Version())));
            }
        }

        [Test]
        public void CountModsByFilter_EmptyModList_ReturnsZeroForAllFilters()
        {
            var item = new MainModList(delegate { });
            foreach (GUIModFilter filter in Enum.GetValues(typeof(GUIModFilter)))
            {
                Assert.That(item.CountModsByFilter(filter), Is.EqualTo(0));
            }

        }
        // TODO Ask nlight/hakan42 if the Xvfb Plugin for jenkins fixes this test
        /*
        [Test]
        [Category("Display")]
        public void ConstructModList_NumberOfRows_IsEqualToNumberOfMods()
        {
            using (var tidy = new DisposableKSP())
            {
                KSPManager manager = new KSPManager(new NullUser(), new FakeWin32Registry(tidy.KSP)) { CurrentInstance = tidy.KSP };
                var registry = Registry.Empty();
                registry.AddAvailable(TestData.FireSpitterModule());
                registry.AddAvailable(TestData.kOS_014_module());
                var modList = MainModList.ConstructModList(new List<GUIMod>
                {
                    new GUIMod(TestData.FireSpitterModule(), registry, manager.CurrentInstance.Version()),
                    new GUIMod(TestData.kOS_014_module(), registry, manager.CurrentInstance.Version())
                });
                Assert.That(modList, Has.Count.EqualTo(2));
            }
        }
        */
    }
}
