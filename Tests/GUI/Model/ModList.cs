using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using NUnit.Framework;

using Tests.Core.Configuration;
using Tests.Data;

using CKAN;
using CKAN.IO;
using CKAN.Versioning;
using CKAN.GUI;

namespace Tests.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [TestFixture]
    public class ModListTests
    {
        private static readonly GameVersionCriteria crit = new GameVersionCriteria(null);

        [Test]
        public void ComputeFullChangeSetFromUserChangeSet_WithEmptyList_HasEmptyChangeSet()
        {
            var item = new ModList();
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var tidy = new DisposableKSP())
            {
                Assert.That(item.ComputeUserChangeSet(Registry.Empty(repoData.Manager), crit, tidy.KSP, null, null), Is.Empty);
            }
        }

        [Test]
        public void IsVisible_WithAllAndNoNameFilter_ReturnsTrueForCompatible()
        {
            var user = new NullUser();
            using (var repo     = new TemporaryRepository(TestData.FireSpitterModule().ToJson()))
            using (var tidy     = new DisposableKSP())
            using (var config   = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new Registry(repoData.Manager, repo.repo);
                var ckan_mod = registry.GetModuleByVersion("Firespitter", "6.3.5");
                Assert.IsNotNull(ckan_mod);

                var item = new ModList();
                Assert.That(item.IsVisible(
                    new GUIMod(ckan_mod!, repoData.Manager, registry,
                               tidy.KSP.StabilityToleranceConfig, tidy.KSP.VersionCriteria(),
                               null, false, false),
                    tidy.KSP.Name,
                    tidy.KSP.game,
                    registry));
            }
        }

        private static Array GetFilters()
            => Enum.GetValues(typeof(GUIModFilter));

        [TestCaseSource(nameof(GetFilters))]
        public void CountModsByFilter_EmptyModList_ReturnsZero(GUIModFilter filter)
        {
            using (var tidy = new DisposableKSP())
            {
                var item = new ModList();
                Assert.That(item.CountModsByFilter(tidy.KSP, filter), Is.EqualTo(0));
            }
        }

        [Test]
        [NUnit.Framework.Category("Display")]
        public void ConstructModList_NumberOfRows_IsEqualToNumberOfMods()
        {
            var user = new NullUser();
            using (var repo = new TemporaryRepository(TestData.FireSpitterModule().ToJson(),
                                                      TestData.kOS_014()))
            using (var tidy = new DisposableKSP())
            using (var config = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            {
                var registry = new Registry(repoData.Manager, repo.repo);
                var main_mod_list = new ModList();
                var mod_list = main_mod_list.ConstructModList(
                    new List<GUIMod>
                    {
                        new GUIMod(TestData.FireSpitterModule(), repoData.Manager, registry,
                                   tidy.KSP.StabilityToleranceConfig, tidy.KSP.VersionCriteria(),
                                   null, false, false),
                        new GUIMod(TestData.kOS_014_module(), repoData.Manager, registry,
                                   tidy.KSP.StabilityToleranceConfig, tidy.KSP.VersionCriteria(),
                                   null, false, false)
                    },
                    tidy.KSP.Name,
                    tidy.KSP.game
                );
                Assert.That(mod_list, Has.Count.EqualTo(2));
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
            var user = new NullUser();
            using (var repo = new TemporaryRepository(TestData.DogeCoinFlag_101(),
                                                      // This module is not for "any" version,
                                                      // to provide another to sort against
                                                      TestData.kOS_014()))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var instance = new DisposableKSP())
            using (var config = new FakeConfiguration(instance.KSP, instance.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                manager.SetCurrentInstance(instance.KSP);
                var registryManager = RegistryManager.Instance(instance.KSP, repoData.Manager);
                var registry = registryManager.registry;
                registry.RepositoriesClear();
                registry.RepositoriesAdd(repo.repo);
                // A module with a ksp_version of "any" to repro our issue
                var anyVersionModule = registry.GetModuleByVersion("DogeCoinFlag", "1.01")!;
                Assert.IsNotNull(anyVersionModule, "DogeCoinFlag 1.01 should exist");
                var modList = new ModList();
                var listGui = new DataGridView();
                var installer = new ModuleInstaller(instance.KSP, manager.Cache!, config, manager.User);
                var downloader = new NetAsyncModulesDownloader(user, manager.Cache!);

                // Act

                // Install module and set it as pre-installed
                manager.Cache?.Store(TestData.DogeCoinFlag_101_module(), TestData.DogeCoinFlagZip(), new Progress<long>(bytes => {}));
                registry.RegisterModule(anyVersionModule, new List<string>(), instance.KSP, false);

                HashSet<string>? possibleConfigOnlyDirs = null;
                installer.InstallList(
                    new List<CkanModule> { anyVersionModule },
                    new RelationshipResolverOptions(instance.KSP.StabilityToleranceConfig),
                    registryManager,
                    ref possibleConfigOnlyDirs,
                    null, null,
                    downloader);

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

                var modules = repoData.Manager.GetAllAvailableModules(Enumerable.Repeat(repo.repo, 1))
                    .Select(mod => new GUIMod(mod.Latest(instance.KSP.StabilityToleranceConfig)!, repoData.Manager, registry,
                                              instance.KSP.StabilityToleranceConfig, instance.KSP.VersionCriteria(),
                                              null, false, false))
                    .ToList();

                listGui.Rows.AddRange(modList.ConstructModList(modules, null, instance.KSP.game).ToArray());
                // The header row adds one to the count
                Assert.AreEqual(modules.Count + 1, listGui.Rows.Count);

                // Sort by game compatibility, this is the fuse-lighting
                listGui.Sort(listGui.Columns[8], ListSortDirection.Descending);

                // Mark the mod for install, after completion we will get an exception
                var otherModule = modules.First(mod => mod.Identifier.Contains("kOS"));
                otherModule.SelectedMod = otherModule.LatestAvailableMod;

                Assert.IsTrue(otherModule.SelectedMod == otherModule.LatestAvailableMod);
                Assert.IsFalse(otherModule.IsInstalled);

                using (var inst2 = new DisposableKSP())
                {
                    Assert.DoesNotThrow(() =>
                    {
                        // Install the "other" module
                        installer.InstallList(
                            modList.ComputeUserChangeSet(Registry.Empty(repoData.Manager), crit, inst2.KSP, null, null)
                                   .Select(change => change.Mod)
                                   .ToList(),
                            new RelationshipResolverOptions(inst2.KSP.StabilityToleranceConfig),
                            registryManager,
                            ref possibleConfigOnlyDirs,
                            null, null,
                            downloader);

                        // Now we need to sort
                        // Make sure refreshing the GUI state does not throw a NullReferenceException
                        listGui.Refresh();
                    });
                }
            }
        }
    }
}
