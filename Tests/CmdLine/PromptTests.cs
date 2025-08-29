using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class PromptTests
    {
        [TestCase("simpletextstring",       ExpectedResult = new string[] { "simpletextstring" })]
        [TestCase("several unquoted words", ExpectedResult = new string[] { "several", "unquoted", "words" })]
        [TestCase(@"""quoted""",            ExpectedResult = new string[] { "quoted" })]
        [TestCase(@"""with space""",        ExpectedResult = new string[] { "with space" })]
        [TestCase(@" surrounding spaces ",  ExpectedResult = new string[] { "surrounding", "spaces", "" })]
        public string[] ParseTextField_WithInput_Works(string input)
            => Prompt.ParseTextField(input);

        [TestCase("", ExpectedResult = new string[]
                                       {
                                           "authtoken", "available", "cache",     "clean",
                                           "compare",   "compat",    "consoleui", "dedup",
                                           "exit",      "filter",
                                           #if NETFRAMEWORK || WINDOWS
                                           "gui",
                                           #endif
                                           "help",    "import",  "install",   "instance",
                                           "list",    "mark",    "prompt",    "remove",
                                           "repair",  "replace", "repo",      "scan",
                                           "search",  "show",    "stability", "update",
                                           "upgrade", "version",
                                       }),
         TestCase("authtoken ", ExpectedResult = new string[] { "add", "list", "remove" }),
         TestCase("cache ",     ExpectedResult = new string[] { "clear", "list", "reset", "set", "setlimit", "showlimit" }),
         TestCase("compat ",    ExpectedResult = new string[] { "add", "clear", "forget", "list", "set" }),
         TestCase("filter ",    ExpectedResult = new string[] { "add", "list", "remove" }),
         TestCase("instance ",  ExpectedResult = new string[] { "add", "clone", "default", "fake", "forget", "list", "rename" }),
         TestCase("mark ",      ExpectedResult = new string[] { "auto", "user"}),
         TestCase("repair ",    ExpectedResult = new string[] { "registry" }),
         TestCase("repo ",      ExpectedResult = new string[] { "add", "available", "default", "forget", "list", "priority" }),
         TestCase("stability ", ExpectedResult = new string[] { "list", "set" }),
         TestCase("install ", ExpectedResult = new string[]
                                               {
                                                   "AdjustableLandingGear", "AdvancedFlyByWire", "AdvancedFlyByWire-Linux",
                                                   "AerojetKerbodyne", "AGExt", "AJE", "AlternateResourcePanel", "AMEG",
                                                   "AntennaRange", "AutoAsparagus", "B9", "BackgroundProcessing",
                                                   "BargainRocketParts", "BDAnimationModules", "BDArmory",
                                                   "BiggerLaunchpads", "CameraTools", "Chatterer", "CIT-Util",
                                                   "CoherentContracts", "CommunityResourcePack", "CommunityTechTree",
                                                   "ConnectedLivingSpace", "CrewFiles", "CrossFeedEnabler", "CustomAsteroids",
                                                   "CustomAsteroids-Pops-Stock-Inner", "CustomAsteroids-Pops-Stock-Outer",
                                                   "CustomBiomes", "CustomBiomes-Data-RSS", "CustomBiomes-Data-Stock",
                                                   "DDSLoader", "DeadlyReentry", "DistantObject", "DistantObject-default",
                                                   "DMagicOrbitalScience", "DockingPortAlignmentIndicator", "DogeCoinFlag",
                                                   "EditorExtensions", "EngineIgnitor-Unofficial-Repack", "ExtraPlanetaryLaunchpads",
                                                   "ExtraPlanetaryLaunchpads-KarboniteAdaptation",
                                                   "ExtraPlanetaryLaunchpads-KarboniteAdaptationAltWorkshop", "FASA",
                                                   "FerramAerospaceResearch", "FinalFrontier", "FinePrint", "FinePrint-Config-Stock",
                                                   "Firespitter", "FirespitterCore", "FMRS", "HaystackContinued",
                                                   "HooliganLabsAirships", "HotRockets", "HyperEdit", "ImpossibleInnovations",
                                                   "InfernalRobotics", "Karbonite", "KarbonitePlus", "KAS", "KDEX", "KerbalAlarmClock",
                                                   "KerbalConstructionTime", "KerbalFlightData", "KerbalFlightIndicators",
                                                   "KerbalJointReinforcement", "KerbalKonstructs", "KerbalStats", "KerbinSide",
                                                   "Kethane", "KineTechAnimation", "KlockheedMartian-Gimbal", "kOS",
                                                   "KronalVesselViewer", "KWRocketry", "LandingHeight", "LargeStructuralComponents",
                                                   "LazTekSpaceXExploration", "LazTekSpaceXExploration-HD",
                                                   "LazTekSpaceXExploration-LD", "LazTekSpaceXHistoric", "LazTekSpaceXHistoric-HD",
                                                   "LazTekSpaceXHistoric-LD", "LazTekSpaceXLaunch", "LazTekSpaceXLaunch-HD",
                                                   "LazTekSpaceXLaunch-LD", "MechJeb2", "ModernChineseFlagPack", "ModuleFixer",
                                                   "ModuleManager", "ModuleRCSFX", "NavballDockingIndicator", "NavBallTextureExport",
                                                   "NavHud", "NBody", "NEAR", "NearFutureConstruction", "NearFutureElectrical",
                                                   "NearFutureExampleCraft", "NearFutureProps", "NearFuturePropulsion",
                                                   "NearFutureSolar", "NearFutureSpacecraft", "NebulaEVAHandrails", "notes",
                                                   "NovaPunch", "ORSX", "PartCatalog", "PlanetShine", "PorkjetHabitats", "PreciseNode",
                                                   "ProceduralDynamics", "ProceduralFairings", "ProceduralParts", "QuickRevert",
                                                   "RandSCapsuledyne", "RasterPropMonitor", "RasterPropMonitor-Core", "RCSLandAid",
                                                   "RealChute", "RealFuels", "RealismOverhaul", "RealRoster", "RealSolarSystem",
                                                   "RemoteTech", "RemoteTech-Config-RSS", "ResGen", "ResourceOverview", "RetroFuture",
                                                   "RFStockalike", "RLA-Stockalike", "RocketdyneF-1", "RoversNOthers-Set1",
                                                   "RoversNOthers-Set2", "RoversNOthers-Set3", "RSSTextures2048", "RSSTextures4096",
                                                   "RSSTextures8192", "SCANsat", "SelectRoot", "Service-Compartments-6S",
                                                   "ShipManifest", "SRL", "StageRecovery", "StationPartsExpansion", "SXT",
                                                   "TacFuelBalancer", "TACLS", "TACLS-Config-RealismOverhaul", "TACLS-Config-Stock",
                                                   "Tantares", "TechManager", "TextureReplacer", "TimeControl", "Toolbar",
                                                   "Trajectories", "TweakableEverything", "TweakScale", "TWR1", "UKS",
                                                   "UniversalStorage", "UniversalStorage-KAS", "UniversalStorage-TAC", "USI-ART",
                                                   "USI-EXP", "USI-FTT", "USI-SRV", "USITools", "VerticalPropulsionEmporium",
                                                   "VesselView", "VesselView-UI-RasterPropMonitor", "VesselView-UI-Toolbar",
                                                   "VirginKalactic-NodeToggle",
                                               }),
         TestCase("remove ", ExpectedResult = new string[]
                                              {
                                                  "AGExt", "AJE", "AlternateResourcePanel",
                                                  "Chatterer", "CIT-Util",
                                                  "CommunityResourcePack", "CommunityTechTree",
                                                  "CrossFeedEnabler", "CustomBiomes",
                                                  "CustomBiomes-Data-RSS", "DDSLoader",
                                                  "DeadlyReentry", "DMagicOrbitalScience",
                                                  "DogeCoinFlag",
                                                  "EngineIgnitor-Unofficial-Repack",
                                                  "EVE-Overhaul-Core", "FerramAerospaceResearch",
                                                  "FinalFrontier", "FinePrint", "FirespitterCore",
                                                  "HotRockets", "InfernalRobotics", "Karbonite",
                                                  "KAS", "KerbalAlarmClock",
                                                  "KerbalConstructionTime",
                                                  "KerbalJointReinforcement", "kOS",
                                                  "KronalVesselViewer", "MechJeb2",
                                                  "ModuleManager", "ModuleRCSFX",
                                                  "NathanKell-RVE-Haxx", "ORSX", "PartCatalog",
                                                  "PlanetShine", "PreciseNode",
                                                  "ProceduralDynamics", "ProceduralFairings",
                                                  "ProceduralParts", "RealChute", "RealFuels",
                                                  "RealismOverhaul", "RealSolarSystem",
                                                  "RemoteTech", "RemoteTech-Config-RSS", "RP-0",
                                                  "RSSTextures4096", "SCANsat", "ScienceAlert",
                                                  "Service-Compartments-6S", "ShipManifest",
                                                  "StageRecovery", "SXT", "TACLS",
                                                  "TACLS-Config-RealismOverhaul", "TechManager",
                                                  "Toolbar", "TweakScale", "UKS",
                                                  "UniversalStorage", "UniversalStorage-KAS",
                                                  "UniversalStorage-TAC", "USITools",
                                              }),
         TestCase("install --", ExpectedResult = new string[] { "--allow-incompatible", "--asroot", "--ckanfiles", "--debug",
                                                                "--debugger", "--gamedir", "--headless", "--instance",
                                                                "--net-useragent", "--no-recommends", "--verbose",
                                                                "--with-all-suggests", "--with-suggests" }),
         TestCase("install -",  ExpectedResult = new string[] { "-c", "-d", "-v" }),
         TestCase("instance forget ", ExpectedResult = new string[] { "disposable" }),
        ]
        public string[]? GetSuggestions_WithInputs_Works(string text)
        {
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var repoData = new TemporaryRepositoryData(user, new Dictionary<Repository, RepositoryData>
            {
                { repo, RepositoryData.FromJson(TestData.TestRepository(), null)! },
            }))
            using (var regMgr = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                         new Repository[] { repo }))
            {
                // Arrange
                regMgr.registry.RepositoriesAdd(repo);
                var sut = new Prompt(manager, repoData.Manager, user);

                // Act
                return sut.GetSuggestions(text, 0);
            }
        }
    }
}
