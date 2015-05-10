using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.AreEqual(GUIModFilter.Compatible, item.ModFilter, "ModFilter");
            Assert.AreEqual(String.Empty, item.ModNameFilter, "ModNameFilter");
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
        [Category("Display")]
        public void ComputeChangeSetFromModList_WithConflictingMods_HasEmptyChangeSet()
        {
            using (var tidy = new DisposableKSP())
            {
                KSPManager manager = new KSPManager(new NullUser(), new FakeWin32Registry(tidy.KSP)) { CurrentInstance = tidy.KSP };

                var registry = Registry.Empty();
                var module = TestData.FireSpitterModule();
                module.conflicts = new List<RelationshipDescriptor>() { new RelationshipDescriptor { name = "kOS" }};
                registry.AddAvailable(TestData.FireSpitterModule());
                registry.AddAvailable(TestData.kOS_014_module());
                registry.RegisterModule(module,Enumerable.Empty<string>(), tidy.KSP );

                var main_mod_list = new MainModList(null);
                var mod = new GUIMod(TestData.FireSpitterModule(), registry, manager.CurrentInstance.Version());
                var mod2 = new GUIMod(TestData.kOS_014_module(), registry, manager.CurrentInstance.Version());
                mod.IsInstallChecked = true;
                mod2.IsInstallChecked = true;
                var mod_list = main_mod_list.ConstructModList(new List<GUIMod>
                {
                    mod,
                    mod2
                });
                Assert.That(main_mod_list.ComputeChangeSetFromModList(registry, tidy.KSP), Is.Null);
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
                var main_mod_list = new MainModList(null);
                var mod_list = main_mod_list.ConstructModList(new List<GUIMod>
                {
                    new GUIMod(TestData.FireSpitterModule(), registry, manager.CurrentInstance.Version()),
                    new GUIMod(TestData.kOS_014_module(), registry, manager.CurrentInstance.Version())
                });
                Assert.That(mod_list, Has.Count.EqualTo(2));
            }
        }        
    }
}
