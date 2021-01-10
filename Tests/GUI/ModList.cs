using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CKAN;
using CKAN.Versioning;
using NUnit.Framework;
using Tests.Core.Configuration;
using Tests.Data;
using ModuleInstaller = CKAN.ModuleInstaller;

namespace Tests.GUI
{
    [TestFixture]
    public class ModListTests
    {
        [Test]
        public void OnCreation_HasDefaultFilters()
        {
            var item = new ModList(delegate { });
            Assert.AreEqual(GUIModFilter.Compatible, item.ModFilter, "ModFilter");
        }

        [Test]
        public void OnModTypeFilterChanges_CallsEventHandler()
        {
            var called_n = 0;
            var item = new ModList(delegate { called_n++; });
            Assert.That(called_n == 1);
            item.ModFilter = GUIModFilter.Installed;
            Assert.That(called_n == 2);
            item.ModFilter = GUIModFilter.Installed;
            Assert.That(called_n == 2);
        }

        [Test]
        public void ComputeChangeSetFromModList_WithEmptyList_HasEmptyChangeSet()
        {
            var item = new ModList(delegate { });
            Assert.That(item.ComputeUserChangeSet(null), Is.Empty);
        }

        [Test]
        public void IsVisible_WithAllAndNoNameFilter_ReturnsTrueForCompatible()
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

                var ckan_mod = TestData.FireSpitterModule();
                var registry = Registry.Empty();
                registry.AddAvailable(ckan_mod);
                var item = new ModList(delegate { });
                Assert.That(item.IsVisible(
                    new GUIMod(ckan_mod, registry, manager.CurrentInstance.VersionCriteria()),
                    manager.CurrentInstance.Name
                ));

                manager.Dispose();
                config.Dispose();
            }
        }

        [Test]
        public void CountModsByFilter_EmptyModList_ReturnsZeroForAllFilters()
        {
            var item = new ModList(delegate { });
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
                var config = new FakeConfiguration(tidy.KSP, tidy.KSP.Name);

                GameInstanceManager manager = new GameInstanceManager(
                    new NullUser(),
                    config
                ) {
                    CurrentInstance = tidy.KSP
                };
                var registry = Registry.Empty();
                registry.AddAvailable(TestData.FireSpitterModule());
                registry.AddAvailable(TestData.kOS_014_module());
                var main_mod_list = new ModList(null);
                var mod_list = main_mod_list.ConstructModList(
                    new List<GUIMod>
                    {
                        new GUIMod(TestData.FireSpitterModule(), registry, manager.CurrentInstance.VersionCriteria()),
                        new GUIMod(TestData.kOS_014_module(), registry, manager.CurrentInstance.VersionCriteria())
                    },
                    manager.CurrentInstance.Name
                );
                Assert.That(mod_list, Has.Count.EqualTo(2));

                manager.Dispose();
                config.Dispose();
            }
        }

    }
}
