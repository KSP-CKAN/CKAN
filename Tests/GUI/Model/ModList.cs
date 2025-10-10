#if NETFRAMEWORK || WINDOWS

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

using CKAN;
using CKAN.IO;
using CKAN.GUI;
using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [TestFixture]
    public class ModListTests
    {
        [Test]
        public void IsVisible_WithAllAndNoNameFilter_ReturnsTrueForCompatible()
        {
            var user = new NullUser();
            using (var repo     = new TemporaryRepository(TestData.FireSpitterModule().ToJson()))
            using (var tidy     = new DisposableKSP())
            using (var config   = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir))
            {
                var registry = new Registry(repoData.Manager, repo.repo);
                var ckan_mod = registry.GetModuleByVersion("Firespitter", "6.3.5");
                Assert.IsNotNull(ckan_mod);

                var item = new ModList(Array.Empty<GUIMod>(), tidy.KSP,
                                       ModuleLabelList.GetDefaultLabels(), new ModuleTagList(),
                                       config, new GUIConfiguration());
                Assert.That(item.IsVisible(
                    new GUIMod(ckan_mod!, repoData.Manager, registry,
                               tidy.KSP.StabilityToleranceConfig, tidy.KSP, cache,
                               null, false, false),
                    tidy.KSP, registry));
            }
        }

        private static Array GetFilters()
            => Enum.GetValues(typeof(GUIModFilter));

        [TestCaseSource(nameof(GetFilters))]
        public void CountModsByFilter_EmptyModList_ReturnsZero(GUIModFilter filter)
        {
            using (var tidy = new DisposableKSP())
            using (var config = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            {
                var item = new ModList(Array.Empty<GUIMod>(), tidy.KSP,
                                       ModuleLabelList.GetDefaultLabels(), new ModuleTagList(),
                                       config, new GUIConfiguration());
                Assert.That(item.CountModsByFilter(tidy.KSP, filter), Is.EqualTo(0));
            }
        }

        [Test]
        [Category("Display")]
        public void Constructor_NumberOfRows_IsEqualToNumberOfMods()
        {
            var user = new NullUser();
            using (var repo = new TemporaryRepository(TestData.FireSpitterModule().ToJson(),
                                                      TestData.kOS_014()))
            using (var tidy     = new DisposableKSP())
            using (var config   = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir))
            {
                var registry = new Registry(repoData.Manager, repo.repo);
                var main_mod_list = new ModList(
                    new List<GUIMod>
                    {
                        new GUIMod(TestData.FireSpitterModule(), repoData.Manager, registry,
                                   tidy.KSP.StabilityToleranceConfig, tidy.KSP, cache,
                                   null, false, false),
                        new GUIMod(TestData.kOS_014_module(), repoData.Manager, registry,
                                   tidy.KSP.StabilityToleranceConfig, tidy.KSP, cache,
                                   null, false, false)
                    },
                    tidy.KSP,
                    ModuleLabelList.GetDefaultLabels(), new ModuleTagList(),
                    config, new GUIConfiguration()
                );
                Assert.That(main_mod_list.full_list_of_mod_rows.Values, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void SetSearches_WithSearches_ModFiltersUpdatedInvoked()
        {
            // Arrange
            using (var inst   = new DisposableKSP())
            using (var config = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            {
                var guiConfig = new GUIConfiguration();
                var modlist   = new ModList(Array.Empty<GUIMod>(), inst.KSP,
                                            ModuleLabelList.GetDefaultLabels(), new ModuleTagList(),
                                            config, guiConfig);
                bool called   = false;
                modlist.ModFiltersUpdated += () => { called = true; };
                var nonEmptySearches = new List<ModSearch>
                                       {
                                           ModSearch.Parse(ModuleLabelList.GetDefaultLabels(), inst.KSP,
                                                           "apollo")!
                                       };

                // Act
                modlist.SetSearches(new List<ModSearch>());

                // Assert
                Assert.IsFalse(called);
                Assert.IsNull(guiConfig.DefaultSearches);

                // Act
                modlist.SetSearches(nonEmptySearches);

                // Assert
                Assert.IsTrue(called);
                CollectionAssert.AreEquivalent(new string[] { "apollo" }, guiConfig.DefaultSearches);
            }
        }

        [TestCase(GUIModFilter.All,                      "Filter (All)"),
         TestCase(GUIModFilter.Cached,                   "Filter (Cached)"),
         TestCase(GUIModFilter.Compatible,               "Filter (Compatible)"),
         TestCase(GUIModFilter.CustomLabel,              "Label (TestLabel)"),
         TestCase(GUIModFilter.Incompatible,             "Filter (Incompatible)"),
         TestCase(GUIModFilter.Installed,                "Filter (Installed)"),
         TestCase(GUIModFilter.InstalledUpdateAvailable, "Filter (Upgradeable)"),
         TestCase(GUIModFilter.NewInRepository,          "Filter (New)"),
         TestCase(GUIModFilter.NotInstalled,             "Filter (Not installed)"),
         TestCase(GUIModFilter.Replaceable,              "Filter (Replaceable)"),
         TestCase(GUIModFilter.Tag,                      "Tag (Untagged)"),
         TestCase(GUIModFilter.Uncached,                 "Filter (Uncached)"),
        ]
        public void FilterToSavedSearch_WithEachFilter_NameCorrect(GUIModFilter filter, string name)
        {
            // Arrange
            using (var inst = new DisposableKSP())
            {
                // Act
                var search = ModList.FilterToSavedSearch(inst.KSP, filter, ModuleLabelList.GetDefaultLabels(), null,
                                                         new ModuleLabel("TestLabel"));

                // Assert
                Assert.AreEqual(name, string.Format(search.Name, "TestLabel"));
            }
        }

        [Test]
        public void ReapplyLabels_AddModToFavorites_Works()
        {
            // Ensure the default locale is used
            CultureInfo.DefaultThreadCurrentUICulture =
                Thread.CurrentThread.CurrentUICulture =
                    new CultureInfo("en-GB");

            // Arrange
            var user = new NullUser();
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(
                                      user,
                                      new Dictionary<Repository, RepositoryData>
                                      {
                                          {
                                              repo,
                                              RepositoryData.FromJson(TestData.TestRepository(), null)!
                                          },
                                      }))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo }))
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir))
            {
                var registry = regMgr.registry;
                var labels   = ModuleLabelList.GetDefaultLabels();
                var favLbl   = labels.Labels.First(l => l.Name == "Favourites");
                var mods     = ModList.GetGUIMods(registry, repoData.Manager,
                                                  inst.KSP, labels, cache, new GUIConfiguration())
                                      .ToArray();
                var modlist  = new ModList(mods, inst.KSP,
                                           labels, new ModuleTagList(),
                                           config, new GUIConfiguration());
                var mod      = mods.First();

                // Act
                favLbl.Add(inst.KSP.Game, mod.Identifier);
                var row = modlist.ReapplyLabels(mod, false, inst.KSP, registry);

                // Assert
                Assert.AreEqual(favLbl.Color, row?.DefaultCellStyle.BackColor);
            }
        }

        [Test]
        public void ResetHasUpdate_NoUpgrades_False()
        {
            // Arrange
            var user = new NullUser();
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(
                                      user,
                                      new Dictionary<Repository, RepositoryData>
                                      {
                                          {
                                              repo,
                                              RepositoryData.FromJson(TestData.TestRepository(), null)!
                                          },
                                      }))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo }))
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir))
            {
                var registry = regMgr.registry;
                var labels   = ModuleLabelList.GetDefaultLabels();
                var mods     = ModList.GetGUIMods(registry, repoData.Manager,
                                                  inst.KSP, labels, cache, new GUIConfiguration())
                                      .ToArray();
                var modlist = new ModList(mods, inst.KSP,
                                          labels, new ModuleTagList(),
                                          config, new GUIConfiguration());
                var grid = new DataGridView();
                grid.Columns.AddRange(StandardColumns);
                grid.Rows.AddRange(modlist.full_list_of_mod_rows.Values.ToArray());

                // Act
                var result = modlist.ResetHasUpdate(inst.KSP, registry, null, grid.Rows);

                // Assert
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void GetGUIMods_WithLiveRegistry_Works()
        {
            // Arrange
            var user = new NullUser();
            var repo = new Repository("test", "https://github.com/");
            using (var repoData = new TemporaryRepositoryData(
                                      user,
                                      new Dictionary<Repository, RepositoryData>
                                      {
                                          {
                                              repo,
                                              RepositoryData.FromJson(TestData.TestRepository(), null)!
                                          },
                                      }))
            using (var instance = new DisposableKSP())
            using (var regMgr   = RegistryManager.Instance(instance.KSP, repoData.Manager,
                                                           new Repository[] { repo }))
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir))
            {
                var registry = regMgr.registry;

                // Act
                var mods = ModList.GetGUIMods(registry, repoData.Manager,
                                              instance.KSP, ModuleLabelList.GetDefaultLabels(),
                                              cache, new GUIConfiguration());

                // Assert
                CollectionAssert.AreEquivalent(new string[]
                                               {
                                                   "AdjustableLandingGear",
                                                   "AdvancedFlyByWire",
                                                   "AdvancedFlyByWire-Linux",
                                                   "AerojetKerbodyne",
                                                   "AGExt",
                                                   "AJE",
                                                   "AlternateResourcePanel",
                                                   "AMEG",
                                                   "AntennaRange",
                                                   "AutoAsparagus",
                                                   "B9",
                                                   "BackgroundProcessing",
                                                   "BahamutoDynamicsPartsPack",
                                                   "BargainRocketParts",
                                                   "BDAnimationModules",
                                                   "BDArmory",
                                                   "BiggerLaunchpads",
                                                   "CameraTools",
                                                   "Chatterer",
                                                   "CIT-Util",
                                                   "CoherentContracts",
                                                   "CommunityResourcePack",
                                                   "CommunityTechTree",
                                                   "ConnectedLivingSpace",
                                                   "CrewFiles",
                                                   "CrossFeedEnabler",
                                                   "CustomAsteroids",
                                                   "CustomAsteroids-Pops-Stock-Inner",
                                                   "CustomAsteroids-Pops-Stock-Outer",
                                                   "CustomBiomes",
                                                   "CustomBiomes-Data-RSS",
                                                   "CustomBiomes-Data-Stock",
                                                   "DDSLoader",
                                                   "DeadlyReentry",
                                                   "DevHelper",
                                                   "DistantObject",
                                                   "DistantObject-default",
                                                   "DMagicOrbitalScience",
                                                   "DockingPortAlignmentIndicator",
                                                   "DogeCoinFlag",
                                                   "EditorExtensions",
                                                   "EngineIgnitor-Unofficial-Repack",
                                                   "ExtraPlanetaryLaunchpads",
                                                   "ExtraPlanetaryLaunchpads-KarboniteAdaptation",
                                                   "ExtraPlanetaryLaunchpads-KarboniteAdaptationAltWorkshop",
                                                   "FASA",
                                                   "FerramAerospaceResearch",
                                                   "FinalFrontier",
                                                   "FinePrint",
                                                   "FinePrint-Config-Stock",
                                                   "Firespitter",
                                                   "FirespitterCore",
                                                   "FMRS",
                                                   "FShangarExtender",
                                                   "HangarExtender",
                                                   "HaystackContinued",
                                                   "HooliganLabsAirships",
                                                   "HotRockets",
                                                   "HyperEdit",
                                                   "ImpossibleInnovations",
                                                   "InfernalRobotics",
                                                   "Karbonite",
                                                   "KarbonitePlus",
                                                   "KAS",
                                                   "KDEX",
                                                   "KerbalAlarmClock",
                                                   "KerbalConstructionTime",
                                                   "KerbalFlightData",
                                                   "KerbalFlightIndicators",
                                                   "KerbalJointReinforcement",
                                                   "KerbalKonstructs",
                                                   "KerbalStats",
                                                   "KerbinSide",
                                                   "Kethane",
                                                   "KineTechAnimation",
                                                   "KlockheedMartian-Gimbal",
                                                   "kOS",
                                                   "KronalVesselViewer",
                                                   "KWRocketry",
                                                   "LandingHeight",
                                                   "LargeStructuralComponents",
                                                   "LazTekSpaceXExploration",
                                                   "LazTekSpaceXExploration-HD",
                                                   "LazTekSpaceXExploration-LD",
                                                   "LazTekSpaceXHistoric",
                                                   "LazTekSpaceXHistoric-HD",
                                                   "LazTekSpaceXHistoric-LD",
                                                   "LazTekSpaceXLaunch",
                                                   "LazTekSpaceXLaunch-HD",
                                                   "LazTekSpaceXLaunch-LD",
                                                   "MechJeb2",
                                                   "ModernChineseFlagPack",
                                                   "ModuleFixer",
                                                   "ModuleManager",
                                                   "ModuleRCSFX",
                                                   "NavballDockingIndicator",
                                                   "NavBallTextureExport",
                                                   "NavHud",
                                                   "NBody",
                                                   "NEAR",
                                                   "NearFutureConstruction",
                                                   "NearFutureElectrical",
                                                   "NearFutureExampleCraft",
                                                   "NearFutureProps",
                                                   "NearFuturePropulsion",
                                                   "NearFutureSolar",
                                                   "NearFutureSpacecraft",
                                                   "NebulaEVAHandrails",
                                                   "notes",
                                                   "NovaPunch",
                                                   "ORSX",
                                                   "PartCatalog",
                                                   "PlanetShine",
                                                   "PorkjetHabitats",
                                                   "PreciseNode",
                                                   "ProceduralDynamics",
                                                   "ProceduralFairings",
                                                   "ProceduralParts",
                                                   "QuickRevert",
                                                   "RandSCapsuledyne",
                                                   "RasterPropMonitor",
                                                   "RasterPropMonitor-Core",
                                                   "RCSLandAid",
                                                   "RealChute",
                                                   "RealFuels",
                                                   "RealismOverhaul",
                                                   "RealRoster",
                                                   "RealSolarSystem",
                                                   "RemoteTech",
                                                   "RemoteTech-Config-RSS",
                                                   "ResGen",
                                                   "ResourceOverview",
                                                   "RetroFuture",
                                                   "RFStockalike",
                                                   "RLA-Stockalike",
                                                   "RocketdyneF-1",
                                                   "RoversNOthers-Set1",
                                                   "RoversNOthers-Set2",
                                                   "RoversNOthers-Set3",
                                                   "RSSTextures2048",
                                                   "RSSTextures4096",
                                                   "RSSTextures8192",
                                                   "SCANsat",
                                                   "SelectRoot",
                                                   "Service-Compartments-6S",
                                                   "ShipManifest",
                                                   "SRL",
                                                   "StageRecovery",
                                                   "StationPartsExpansion",
                                                   "SXT",
                                                   "TacFuelBalancer",
                                                   "TACLS",
                                                   "TACLS-Config-RealismOverhaul",
                                                   "TACLS-Config-Stock",
                                                   "Tantares",
                                                   "TechManager",
                                                   "TextureReplacer",
                                                   "TimeControl",
                                                   "Toolbar",
                                                   "Trajectories",
                                                   "TweakableEverything",
                                                   "TweakScale",
                                                   "TWR1",
                                                   "UKS",
                                                   "UniversalStorage",
                                                   "UniversalStorage-ECLSS",
                                                   "UniversalStorage-IFI",
                                                   "UniversalStorage-KAS",
                                                   "UniversalStorage-SNACKS",
                                                   "UniversalStorage-TAC",
                                                   "USI-ART",
                                                   "USI-EXP",
                                                   "USI-FTT",
                                                   "USI-SRV",
                                                   "USITools",
                                                   "VerticalPropulsionEmporium",
                                                   "VesselView",
                                                   "VesselView-UI-RasterPropMonitor",
                                                   "VesselView-UI-Toolbar",
                                                   "VirginKalactic-NodeToggle",
                                               },
                                               mods.Select(m => m.Identifier).Order());
            }
        }

        [Test]
        public void ComputeUserChangeSet_FiveModsSelected_Works()
        {
            // Arrange
            var user = new NullUser();
            var repo = new Repository("test", "https://github.com/");
            using (var instance = new DisposableKSP())
            using (var config   = new FakeConfiguration(instance.KSP, instance.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(
                                      user,
                                      new Dictionary<Repository, RepositoryData>
                                      {
                                          {
                                              repo,
                                              RepositoryData.FromJson(TestData.TestRepository(), null)!
                                          },
                                      }))
            using (var regMgr   = RegistryManager.Instance(instance.KSP, repoData.Manager,
                                                           new Repository[] { repo }))
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir))
            {
                var registry = regMgr.registry;
                var mods     = ModList.GetGUIMods(registry, repoData.Manager,
                                                  instance.KSP, ModuleLabelList.GetDefaultLabels(),
                                                  cache, new GUIConfiguration())
                                      .ToArray();
                var modlist  = new ModList(mods, instance.KSP,
                                           ModuleLabelList.GetDefaultLabels(), new ModuleTagList(),
                                           config, new GUIConfiguration());

                // Act
                foreach (var mod in mods.OrderBy(m => m.Identifier).Take(5))
                {
                    mod.SelectedMod = mod.LatestCompatibleMod;
                }
                var changeset = modlist.ComputeUserChangeSet(registry, instance.KSP, null, null);

                // Assert
                CollectionAssert.AreEquivalent(new string[]
                                               {
                                                   "AdjustableLandingGear",
                                                   "AdvancedFlyByWire",
                                                   "AdvancedFlyByWire-Linux",
                                                   "AerojetKerbodyne",
                                                   "AGExt",
                                               },
                                               changeset.Select(ch => ch.Mod.identifier));
            }
        }

        [Test]
        public void ComputeUserChangeSet_WithEmptyList_HasEmptyChangeSet()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var tidy = new DisposableKSP())
            using (var config = new FakeConfiguration(tidy.KSP, tidy.KSP.Name))
            {
                var item = new ModList(Array.Empty<GUIMod>(), tidy.KSP,
                                       ModuleLabelList.GetDefaultLabels(), new ModuleTagList(),
                                       config, new GUIConfiguration());
                Assert.That(item.ComputeUserChangeSet(Registry.Empty(repoData.Manager), tidy.KSP, null, null), Is.Empty);
            }
        }

        [Test]
        public void ComputeFullChangeSetFromUserChangeSet_B9_GetsAllDependencies()
        {
            // Arrange
            var user      = new NullUser();
            var repo      = new Repository("test", "https://github.com/");
            var guiConfig = new GUIConfiguration();
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var repoData = new TemporaryRepositoryData(
                                      user,
                                      new Dictionary<Repository, RepositoryData>
                                      {
                                          {
                                              repo,
                                              RepositoryData.FromJson(TestData.TestRepository(), null)!
                                          },
                                      }))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo }))
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir))
            {
                var registry  = regMgr.registry;
                var labels    = ModuleLabelList.GetDefaultLabels();
                var mods      = ModList.GetGUIMods(registry, repoData.Manager, inst.KSP, labels, cache, guiConfig)
                                       .ToArray();
                var modlist   = new ModList(mods, inst.KSP,
                                            labels, new ModuleTagList(),
                                            config, guiConfig);

                // Act
                var b9         = mods.First(m => m.Identifier == "B9");
                b9.SelectedMod = b9.LatestCompatibleMod;
                var changes    = modlist.ComputeUserChangeSet(registry, inst.KSP, null, null);
                var full       = modlist.ComputeFullChangeSetFromUserChangeSet(registry, changes,
                                                                               inst.KSP.Game,
                                                                               inst.KSP.StabilityToleranceConfig,
                                                                               inst.KSP.VersionCriteria());

                // Assert
                CollectionAssert.AreEquivalent(new string[]
                                               {
                                                   "B9",
                                                   "CrossFeedEnabler",
                                                   "FirespitterCore",
                                                   "KineTechAnimation",
                                                   "KlockheedMartian-Gimbal",
                                                   "ModuleManager",
                                                   "RasterPropMonitor-Core",
                                                   "ResGen",
                                                   "VirginKalactic-NodeToggle",
                                               },
                                               full.Item1.Select(ch => ch.Mod.identifier).Order());
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
        [Category("Display")]
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
            using (var config   = new FakeConfiguration(instance.KSP, instance.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(instance.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            using (var cacheDir = new TemporaryDirectory())
            using (var cache    = new NetModuleCache(cacheDir))
            {
                manager.SetCurrentInstance(instance.KSP);
                var registry = regMgr.registry;
                // A module with a ksp_version of "any" to repro our issue
                var anyVersionModule = registry.GetModuleByVersion("DogeCoinFlag", "1.01")!;
                Assert.IsNotNull(anyVersionModule, "DogeCoinFlag 1.01 should exist");
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
                    regMgr,
                    ref possibleConfigOnlyDirs,
                    null, null,
                    downloader);

                // TODO: Refactor the column header code to allow mocking of the GUI without creating columns
                listGui.Columns.AddRange(StandardColumns);

                // Assert (and Act a bit more)

                Assert.IsNotNull(instance.KSP);
                Assert.IsNotNull(manager);

                var modules = repoData.Manager.GetAllAvailableModules(Enumerable.Repeat(repo.repo, 1))
                    .Select(mod => new GUIMod(mod.Latest(instance.KSP.StabilityToleranceConfig)!, repoData.Manager, registry,
                                              instance.KSP.StabilityToleranceConfig, instance.KSP, cache,
                                              null, false, false))
                    .ToList();

                var modList = new ModList(modules, instance.KSP,
                                          ModuleLabelList.GetDefaultLabels(), new ModuleTagList(),
                                          config, new GUIConfiguration());
                Assert.IsFalse(modList.HasVisibleInstalled());

                listGui.Rows.AddRange(modList.full_list_of_mod_rows.Values.ToArray());
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
                            modList.ComputeUserChangeSet(Registry.Empty(repoData.Manager), inst2.KSP, null, null)
                                   .Select(change => change.Mod)
                                   .ToList(),
                            new RelationshipResolverOptions(inst2.KSP.StabilityToleranceConfig),
                            regMgr,
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

        private const int numCheckboxCols = 4;

        private const int numTextCols     = 11;

        private static DataGridViewColumn[] StandardColumns
            => Enumerable.Range(1, numCheckboxCols)
                         .Select(i => (DataGridViewColumn)new DataGridViewCheckBoxColumn())
                         .Concat(Enumerable.Range(1, numTextCols)
                         .Select(i => new DataGridViewTextBoxColumn()))
                         .ToArray();

    }
}

#endif
