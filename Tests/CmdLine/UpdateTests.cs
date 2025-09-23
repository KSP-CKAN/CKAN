using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class UpdateTests
    {
        [TestCase(false),
         TestCase(true)]
        public void RunCommand_LocalRepo_Works(bool listChanges)
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { new Repository("test", TestData.TestKANTarGz()) }))
            {
                // Not an ICommand because it can update CKAN itself
                var sut  = new Update(repoData.Manager, user, manager);
                var opts = new UpdateOptions() { list_changes = listChanges };

                // Act
                sut.RunCommand(opts);

                // Assert
                CollectionAssert.IsNotEmpty(regMgr.registry.CompatibleModules(inst.KSP.StabilityToleranceConfig,
                                                                              inst.KSP.VersionCriteria()));
                CollectionAssert.AreEqual(listChanges
                                          ? new string[]
                                          {
                                              "Refreshing game version data",
                                              $"Downloading {TestData.TestKANTarGz()} ...",
                                              "Updated information on 37 modules.",
                                              "Found 37 new modules, 0 removed modules, and 0 updated modules.",
                                              "New modules [Name (CKAN identifier)]:",
                                          }
                                              .Concat(compatible)
                                              .Append("")
                                          : new string[]
                                          {
                                              "Refreshing game version data",
                                              $"Downloading {TestData.TestKANTarGz()} ...",
                                              "Updated information on 37 modules.",
                                          },
                                          user.RaisedMessages);
            }
        }

        [TestCase(false),
         TestCase(true)]
        public void RunCommand_RepoURLParam_Works(bool listChanges)
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { new Repository("test", TestData.TestKANTarGz()) }))
            {
                // Not an ICommand because it can update CKAN itself
                var sut  = new Update(repoData.Manager, user, manager);
                var opts = new UpdateOptions()
                           {
                               list_changes   = listChanges,
                               game           = inst.KSP.Game.ShortName,
                               repositoryURLs = new string[]
                                                {
                                                    TestData.TestKANTarGz().OriginalString,
                                                },
                           };

                // Act
                sut.RunCommand(opts);

                // Assert
                CollectionAssert.IsNotEmpty(regMgr.registry.CompatibleModules(inst.KSP.StabilityToleranceConfig,
                                                                              inst.KSP.VersionCriteria()));
                CollectionAssert.AreEqual(listChanges
                                          ? new string[]
                                          {
                                              "Refreshing game version data",
                                              $"Downloading {TestData.TestKANTarGz()} ...",
                                              "Updated information on 41 modules.",
                                              "Found 39 new modules, 0 removed modules, and 0 updated modules.",
                                              "New modules [Name (CKAN identifier)]:",
                                          }
                                              .Concat(compatible.Concat(incompatible)
                                                                .Order())
                                              .Append("")
                                          : new string[]
                                          {
                                              "Refreshing game version data",
                                              $"Downloading {TestData.TestKANTarGz()} ...",
                                              "Updated information on 41 modules.",
                                          },
                                          user.RaisedMessages);
            }
        }

        private static readonly string[] compatible = new string[]
        {
            "Advanced Fly-By-Wire (ksp-advanced-flybywire)",
            "Advanced Jet Engine (AJE) (AJE)",
            "AMEG (AMEG)",
            "Community Resource Pack (CommunityResourcePack)",
            "CrewFiles (CrewFiles)",
            "Cross Feed Enabler (CrossFeedEnabler)",
            "Custom Asteroids (CustomAsteroids)",
            "Custom Biomes (CustomBiomes)",
            "Custom Biomes (Kerbal data) (CustomBiomesKerbal)",
            "Custom Biomes (Real Solar System data) (CustomBiomesRSS)",
            "DangIt! (DangIt)",
            "Deadly Reentry Continued (DeadlyReentry)",
            "Dogecoin Core Plugin (DogeCoinPlugin)",
            "Dogecoin Flag (DogeCoinFlag)",
            "Ferram Aerospace Research (FerramAerospaceResearch)",
            "Firespitter (Firespitter)",
            "Firespitter Core (FirespitterCore)",
            "HotRockets! Particle FX Replacement (HotRockets)",
            "kOS: Scriptable Autopilot System (kOS)",
            "Magic Smoke Industries Infernal Robotics (InfernalRobotics)",
            "Module Manager (ModuleManager)",
            "ModuleRCSFX (ModuleRCSFX)",
            "Open Resource Standard Fork (ORSX)",
            "Professor Phineas Kerbenstein's wonderous vertical propulsion emporium. (Kerb-fu)",
            "QuickRevert (QuickRevert)",
            "Real Solar System (RealSolarSystem)",
            "Real Solar System Textures - 2048 x 1024 (RSSTextures2048)",
            "Real Solar System Textures - 4096 x 2048 (RSSTextures4096)",
            "Real Solar System Textures - 8192 x 4096 (RSSTextures8192)",
            "RealChute Parachute Systems (RealChute)",
            "RemoteTech (RemoteTech)",
            "Retro-Future Planes (RetroFuture)",
            "Simulate, Revert & Launch (SRL)",
            "TechManager (TechManager)",
            "Toolbar (Toolbar)",
            "TweakScale (TweakScale)",
            "Umbra Space Industries Tools (USITools)",
        };

        private static readonly string[] incompatible = new string[]
        {
            "MechJeb (MechJeb2)",
            "Real Fuels (RealFuels)",
        };
    }
}
