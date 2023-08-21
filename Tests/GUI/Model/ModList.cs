using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Framework;
using Tests.Core.Configuration;
using Tests.Data;

using CKAN;
using CKAN.GUI;
using CKAN.Versioning;

namespace Tests.GUI
{
    [TestFixture]
    public class ModListTests
    {
        [Test]
        public void ComputeFullChangeSetFromUserChangeSet_WithEmptyList_HasEmptyChangeSet()
        {
            var item = new ModList();
            Assert.That(item.ComputeUserChangeSet(null, null), Is.Empty);
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
                var item = new ModList();
                Assert.That(item.IsVisible(
                    new GUIMod(ckan_mod, registry, manager.CurrentInstance.VersionCriteria()),
                    manager.CurrentInstance.Name,
                    manager.CurrentInstance.game
                ));

                manager.Dispose();
                config.Dispose();
            }
        }

        public static Array GetFilters()
        {
            return Enum.GetValues(typeof(GUIModFilter));
        }

        [TestCaseSource("GetFilters")]
        public void CountModsByFilter_EmptyModList_ReturnsZero(GUIModFilter filter)
        {
            var item = new ModList();
            Assert.That(item.CountModsByFilter(filter), Is.EqualTo(0));
        }

        [Test]
        [NUnit.Framework.Category("Display")]
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
                var main_mod_list = new ModList();
                var mod_list = main_mod_list.ConstructModList(
                    new List<GUIMod>
                    {
                        new GUIMod(TestData.FireSpitterModule(), registry, manager.CurrentInstance.VersionCriteria()),
                        new GUIMod(TestData.kOS_014_module(), registry, manager.CurrentInstance.VersionCriteria())
                    },
                    manager.CurrentInstance.Name,
                    manager.CurrentInstance.game
                );
                Assert.That(mod_list, Has.Count.EqualTo(2));

                manager.Dispose();
                config.Dispose();
            }
        }

        /// <summary>
        /// Sort the GUI table by Max KSP Version
        /// and then perform a repo operation.
        /// Attempts to reproduce:
        /// https://github.com/KSP-CKAN/CKAN/issues/1803
        /// https://github.com/KSP-CKAN/CKAN/issues/1875
        /// https://github.com/KSP-CKAN/CKAN/pull/1866
        /// https://github.com/KSP-CKAN/CKAN/pull/1882
        /// </summary>
        [Test]
        [NUnit.Framework.Category("Display")]
        public void InstallAndSortByCompat_WithAnyCompat_NoCrash()
        {
            /*
            // An exception would be thrown at the bottom of this.
            var main = new Main(null, new GUIUser(), false);
            main.Manager = _manager;
            // First sort by name
            main.configuration.SortByColumnIndex = 2;
            // Now sort by version
            main.configuration.SortByColumnIndex = 6;
            main.MarkModForInstall("kOS");

            // Make sure we have one requested change
            var changeList = main.mainModList.ComputeUserChangeSet()
                .Select((change) => change.Mod.ToCkanModule()).ToList();

            // Do the install
            new ModuleInstaller(_instance.KSP, main.currentUser).InstallList(
                changeList,
                new RelationshipResolverOptions(),
                new NetAsyncModulesDownloader(main.currentUser)
            );
            */

            // Arrange

            DisposableKSP instance = new DisposableKSP();
            RegistryManager registryManager = RegistryManager.Instance(instance.KSP);
            Registry registry = Registry.Empty();
            FakeConfiguration config = new FakeConfiguration(instance.KSP, instance.KSP.Name);
            GameInstanceManager manager = new GameInstanceManager(new NullUser(), config);
            // A module with a ksp_version of "any" to repro our issue
            CkanModule anyVersionModule = TestData.DogeCoinFlag_101_module();
            ModList modList = new ModList();
            DataGridView listGui = new DataGridView();
            CKAN.ModuleInstaller installer = new CKAN.ModuleInstaller(instance.KSP, manager.Cache, manager.User);
            NetAsyncModulesDownloader downloader = new NetAsyncModulesDownloader(manager.User, manager.Cache);

            // Act

            // Install module and set it as pre-installed
            manager.Cache.Store(TestData.DogeCoinFlag_101_module(), TestData.DogeCoinFlagZip(), new Progress<long>(bytes => {}));
            registry.RegisterModule(anyVersionModule, new string[] { }, instance.KSP, false);
            registry.AddAvailable(anyVersionModule);

            HashSet<string> possibleConfigOnlyDirs = null;
            installer.InstallList(
                new List<CkanModule> { anyVersionModule },
                new RelationshipResolverOptions(),
                registryManager,
                ref possibleConfigOnlyDirs,
                downloader
            );

            // This module is not for "any" version,
            // to provide another to sort against
            registry.AddAvailable(TestData.kOS_014_module());

            // TODO: Refactor the column header code to allow mocking of the GUI without creating columns
            const int numCheckboxCols = 4;
            const int numTextCols     = 11;
            listGui.Columns.AddRange(
                Enumerable.Range(1, numCheckboxCols)
                    .Select(i => (DataGridViewColumn)new DataGridViewCheckBoxColumn())
                    .Concat(Enumerable.Range(1, numTextCols)
                        .Select(i => new DataGridViewTextBoxColumn()))
                    .ToArray());

            // Assert (and Act a bit more)

            Assert.IsNotNull(instance.KSP);
            Assert.IsNotNull(manager);
            Assert.IsNotNull(modList);

            var modules = registry.available_modules
                .Select(mod => new GUIMod(mod.Value.Latest(), registry, instance.KSP.VersionCriteria()))
                .ToList();

            listGui.Rows.AddRange(modList.ConstructModList(modules, null, instance.KSP.game).ToArray());
            // The header row adds one to the count
            Assert.AreEqual(modules.Count + 1, listGui.Rows.Count);

            // Sort by game compatibility, this is the fuse-lighting
            listGui.Sort(listGui.Columns[8], ListSortDirection.Descending);

            // Mark the mod for install, after completion we will get an exception
            var otherModule = modules.First(mod => mod.Identifier.Contains("kOS"));
            otherModule.IsInstallChecked = true;

            Assert.IsTrue(otherModule.IsInstallChecked);
            Assert.IsFalse(otherModule.IsInstalled);

            Assert.DoesNotThrow(() =>
            {
                // Install the "other" module
                installer.InstallList(
                    modList.ComputeUserChangeSet(null, null).Select(change => change.Mod).ToList(),
                    new RelationshipResolverOptions(),
                    registryManager,
                    ref possibleConfigOnlyDirs,
                    downloader
                );

                // Now we need to sort
                // Make sure refreshing the GUI state does not throw a NullReferenceException
                listGui.Refresh();
            });

            instance.Dispose();
            manager.Dispose();
            config.Dispose();
        }

    }
}
