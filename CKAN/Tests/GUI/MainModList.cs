using System;
using System.Collections.Generic;
using CKAN;
using NUnit.Framework;
using Tests;

namespace CKANTests
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
            var calledN = 0;
            var item = new MainModList(delegate { calledN++; });
            Assert.That(calledN == 1);
            item.ModNameFilter = "randomString";
            Assert.That(calledN == 2);
            item.ModNameFilter = "randomString";
            Assert.That(calledN == 2);
        }
        [Test]
        public void OnModTypeFilterChanges_CallsEventHandler()
        {
            var calledN = 0;
            var item = new MainModList(delegate { calledN++; });
            Assert.That(calledN == 1);
            item.ModFilter = GUIModFilter.Installed;
            Assert.That(calledN == 2);
            item.ModFilter = GUIModFilter.Installed;
            Assert.That(calledN == 2);
        }

        [Test]
        public void ComputeChangeSetFromModList_WithEmptyList_HasEmptyChangeSet()
        {
            using (var tidy = new DisposableKSP())
            {
                CKAN.KSPManager manager = new CKAN.KSPManager(new NullUser()) { _CurrentInstance = tidy.KSP };                
                var item = new MainModList(delegate { });
                Assert.That(item.ComputeChangeSetFromModList(CKAN.Registry.Empty(), manager.CurrentInstance), Is.Empty);
            }
        }

        [Test]
        public void IsVisible_WithAllAndNoNameFilter_ReturnsTrueForCompatible()
        {
            using (var tidy = new DisposableKSP())
            {
                CKAN.KSPManager manager = new CKAN.KSPManager(new NullUser()) { _CurrentInstance = tidy.KSP };

                var ckanMod = TestData.FireSpitterModule();
                var registry = CKAN.Registry.Empty();
                registry.AddAvailable(ckanMod);
                var item = new MainModList(delegate { });
                Assert.That(item.IsVisible(new GUIMod(ckanMod, registry, manager.CurrentInstance.Version())));
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
                CKAN.KSPManager manager = new CKAN.KSPManager(new NullUser()) { _CurrentInstance = tidy.KSP };
                var registry = CKAN.Registry.Empty();
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
    }
}
