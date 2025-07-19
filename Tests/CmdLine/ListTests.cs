using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.Extensions;
using CKAN.CmdLine;
using Tests.Data;
using System;
using System.IO;

namespace Tests.CmdLine
{
    using List = CKAN.CmdLine.List;

    [TestFixture]
    public class ListTests
    {
        [Test]
        public void RunCommand_Normal_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
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
            {
                regMgr.registry.RepositoriesAdd(repo);
                ICommand sut  = new List(repoData.Manager, user, Console.OpenStandardOutput());
                var      opts = new ListOptions();

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "",
                                              $"KSP found at: {inst.KSP.GameDir()}",
                                              "",
                                              "KSP version: 0.25.0.642",
                                              "",
                                              "Installed modules:",
                                              "",
                                              "^ AGExt 1.23c",
                                              "^ AJE 1.6.4",
                                              "^ AlternateResourcePanel 2.6.1.0",
                                              "^ Chatterer 0.7.1",
                                              "^ CIT-Util 1.0.4-unofficial",
                                              "^ CommunityResourcePack 0.2.3",
                                              "^ CommunityTechTree 1.1",
                                              "^ CrossFeedEnabler v3.1",
                                              "^ CustomBiomes 1.6.8",
                                              "^ CustomBiomes-Data-RSS v8.2.1-actually-v8.3preview",
                                              "^ DDSLoader 1.7.0.0",
                                              "^ DeadlyReentry v6.2.1",
                                              "^ DMagicOrbitalScience 0.9.0.1",
                                              "^ DogeCoinFlag 1.02",
                                              "^ EngineIgnitor-Unofficial-Repack 3.4.1.1",
                                              "? EVE-Overhaul-Core 0.0.2014.11.25",
                                              "^ FerramAerospaceResearch v0.14.4",
                                              "^ FinalFrontier 0.5.9-177",
                                              "^ FinePrint 0.59",
                                              "^ FirespitterCore 7.0.5398.27328",
                                              "^ HotRockets 7.25",
                                              "^ InfernalRobotics 0.19.2",
                                              "^ Karbonite 0.4.4",
                                              "^ KAS 0.4.9",
                                              "^ KerbalAlarmClock v3.0.5.0",
                                              "^ KerbalConstructionTime 1.0.3",
                                              "^ KerbalJointReinforcement v2.4.4",
                                              "^ kOS 0.15.3.0",
                                              "^ KronalVesselViewer 0.0.4_0.25",
                                              "^ MechJeb2 2.4.0",
                                              "^ ModuleManager 2.5.1",
                                              "^ ModuleRCSFX v3.3",
                                              "? NathanKell-RVE-Haxx 0.0.2014.11.25",
                                              "^ ORSX 0.1.3",
                                              "^ PartCatalog 3.0_RC8",
                                              "^ PlanetShine 0.2.2",
                                              "^ PreciseNode 1.1.1",
                                              "^ ProceduralDynamics 0.9.1",
                                              "^ ProceduralFairings v3.10",
                                              "^ ProceduralParts v0.9.20",
                                              "^ RealChute 1.2.6",
                                              "- RealFuels rf-v8.1-really-v8.2-pre",
                                              "^ RealismOverhaul v7.0.2",
                                              "^ RealSolarSystem v8.2.1-actually-v8.3preview",
                                              "^ RemoteTech v1.5.1",
                                              "^ RemoteTech-Config-RSS 0.0",
                                              "X RP-0 v0.13",
                                              "^ RSSTextures4096 1.0",
                                              "^ SCANsat v8.0",
                                              "X ScienceAlert 1.8rc1",
                                              "^ Service-Compartments-6S 1.2",
                                              "^ ShipManifest 0.25.0_3.3.2b",
                                              "^ StageRecovery 1.5.1",
                                              "^ SXT 18.6",
                                              "^ TACLS v0.10.1",
                                              "^ TACLS-Config-RealismOverhaul v7.0.2",
                                              "^ TechManager 1.4",
                                              "^ Toolbar 1.7.7",
                                              "^ TweakScale v1.44",
                                              "^ UKS 0.21.3",
                                              "^ UniversalStorage 0.9.4",
                                              "^ UniversalStorage-KAS 0.9.0.14",
                                              "^ UniversalStorage-TAC 0.9.2.7",
                                              "^ USITools 0.2.4", "",
                                              "Legend: -: Up to date. +:Auto-installed. X: Incompatible. ^: Upgradable. >: Replaceable\r\n        A: Autodetected. ?: Unknown. *: Broken."
                                          },
                                          user.RaisedMessages);
            }
        }

        [Test]
        public void RunCommand_Porcelain_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
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
            {
                regMgr.registry.RepositoriesAdd(repo);
                ICommand sut  = new List(repoData.Manager, user, Console.OpenStandardOutput());
                var      opts = new ListOptions() { porcelain = true };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "^ AGExt 1.23c",
                                              "^ AJE 1.6.4",
                                              "^ AlternateResourcePanel 2.6.1.0",
                                              "^ Chatterer 0.7.1",
                                              "^ CIT-Util 1.0.4-unofficial",
                                              "^ CommunityResourcePack 0.2.3",
                                              "^ CommunityTechTree 1.1",
                                              "^ CrossFeedEnabler v3.1",
                                              "^ CustomBiomes 1.6.8",
                                              "^ CustomBiomes-Data-RSS v8.2.1-actually-v8.3preview",
                                              "^ DDSLoader 1.7.0.0",
                                              "^ DeadlyReentry v6.2.1",
                                              "^ DMagicOrbitalScience 0.9.0.1",
                                              "^ DogeCoinFlag 1.02",
                                              "^ EngineIgnitor-Unofficial-Repack 3.4.1.1",
                                              "? EVE-Overhaul-Core 0.0.2014.11.25",
                                              "^ FerramAerospaceResearch v0.14.4",
                                              "^ FinalFrontier 0.5.9-177",
                                              "^ FinePrint 0.59",
                                              "^ FirespitterCore 7.0.5398.27328",
                                              "^ HotRockets 7.25",
                                              "^ InfernalRobotics 0.19.2",
                                              "^ Karbonite 0.4.4",
                                              "^ KAS 0.4.9",
                                              "^ KerbalAlarmClock v3.0.5.0",
                                              "^ KerbalConstructionTime 1.0.3",
                                              "^ KerbalJointReinforcement v2.4.4",
                                              "^ kOS 0.15.3.0",
                                              "^ KronalVesselViewer 0.0.4_0.25",
                                              "^ MechJeb2 2.4.0",
                                              "^ ModuleManager 2.5.1",
                                              "^ ModuleRCSFX v3.3",
                                              "? NathanKell-RVE-Haxx 0.0.2014.11.25",
                                              "^ ORSX 0.1.3",
                                              "^ PartCatalog 3.0_RC8",
                                              "^ PlanetShine 0.2.2",
                                              "^ PreciseNode 1.1.1",
                                              "^ ProceduralDynamics 0.9.1",
                                              "^ ProceduralFairings v3.10",
                                              "^ ProceduralParts v0.9.20",
                                              "^ RealChute 1.2.6",
                                              "- RealFuels rf-v8.1-really-v8.2-pre",
                                              "^ RealismOverhaul v7.0.2",
                                              "^ RealSolarSystem v8.2.1-actually-v8.3preview",
                                              "^ RemoteTech v1.5.1",
                                              "^ RemoteTech-Config-RSS 0.0",
                                              "X RP-0 v0.13",
                                              "^ RSSTextures4096 1.0",
                                              "^ SCANsat v8.0",
                                              "X ScienceAlert 1.8rc1",
                                              "^ Service-Compartments-6S 1.2",
                                              "^ ShipManifest 0.25.0_3.3.2b",
                                              "^ StageRecovery 1.5.1",
                                              "^ SXT 18.6",
                                              "^ TACLS v0.10.1",
                                              "^ TACLS-Config-RealismOverhaul v7.0.2",
                                              "^ TechManager 1.4",
                                              "^ Toolbar 1.7.7",
                                              "^ TweakScale v1.44",
                                              "^ UKS 0.21.3",
                                              "^ UniversalStorage 0.9.4",
                                              "^ UniversalStorage-KAS 0.9.0.14",
                                              "^ UniversalStorage-TAC 0.9.2.7",
                                              "^ USITools 0.2.4",
                                          },
                                          user.RaisedMessages);
            }
        }

        [Test]
        public void RunCommand_ExportCkan_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
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
            using (var output   = new MemoryStream())
            {
                regMgr.registry.RepositoriesAdd(repo);
                ICommand sut    = new List(repoData.Manager, user, output);
                var      opts   = new ListOptions() { export = "ckan" };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                            "{",
                                            "    \"spec_version\": \"v1.6\",",
                                            "    \"identifier\": \"installed-disposable\",",
                                            "    \"name\": \"installed-disposable\",",
                                            "    \"abstract\": \"A list of modules installed on the disposable KSP instance\",",
                                            "    \"ksp_version_min\": \"0.25\",",
                                            "    \"ksp_version_max\": \"0.25\",",
                                            "    \"license\": \"unknown\",",
                                            "    \"depends\": [",
                                            "        {",
                                            "            \"name\": \"ModuleManager\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"RealSolarSystem\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"RSSTextures4096\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"FerramAerospaceResearch\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"AJE\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"KerbalJointReinforcement\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"ModuleRCSFX\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"RealFuels\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"RealismOverhaul\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"USITools\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"FirespitterCore\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"TechManager\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"CommunityResourcePack\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"CommunityTechTree\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"ORSX\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"ProceduralFairings\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"SXT\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"TACLS\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"TACLS-Config-RealismOverhaul\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"UniversalStorage\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"CIT-Util\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"CustomBiomes\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"CustomBiomes-Data-RSS\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"KAS\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"RP-0\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"RemoteTech\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"RemoteTech-Config-RSS\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"Toolbar\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"Service-Compartments-6S\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"AGExt\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"AlternateResourcePanel\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"Chatterer\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"CrossFeedEnabler\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"DDSLoader\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"DeadlyReentry\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"DMagicOrbitalScience\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"DogeCoinFlag\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"EngineIgnitor-Unofficial-Repack\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"FinalFrontier\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"FinePrint\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"HotRockets\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"Karbonite\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"KerbalAlarmClock\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"KerbalConstructionTime\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"kOS\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"KronalVesselViewer\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"InfernalRobotics\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"MechJeb2\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"PartCatalog\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"PlanetShine\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"PreciseNode\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"ProceduralParts\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"ProceduralDynamics\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"RealChute\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"SCANsat\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"ScienceAlert\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"ShipManifest\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"StageRecovery\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"TweakScale\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"UniversalStorage-KAS\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"UniversalStorage-TAC\"",
                                            "        },",
                                            "        {",
                                            "            \"name\": \"UKS\"",
                                            "        }",
                                            "    ],",
                                            "    \"kind\": \"metapackage\"",
                                            "}",
                                          },
                                          ReadAllLines(output)
                                              // Ignore lines that contain users or timestamps
                                              .ExceptContainsAny(@"""author"":",
                                                                 @"""version"":",
                                                                 @"""release_date"":")
                                              .ToArray());
            }
        }

        [Test]
        public void RunCommand_ExportText_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
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
            using (var output   = new MemoryStream())
            {
                regMgr.registry.RepositoriesAdd(repo);
                ICommand sut    = new List(repoData.Manager, user, output);
                var      opts   = new ListOptions() { export = "text" };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                            "6S Service Compartment Tubes (Service-Compartments-6S 1.2)",
                                            "Action Groups Extended (AGExt 1.23c)",
                                            "Advanced Jet Engine (AJE) (AJE 1.6.4)",
                                            "Alternate Resource Panel (AlternateResourcePanel 2.6.1.0)",
                                            "Chatterer (Chatterer 0.7.1)",
                                            "CIT Utils (CIT-Util 1.0.4-unofficial)",
                                            "Community Resource Pack (CommunityResourcePack 0.2.3)",
                                            "Community Tech Tree (CommunityTechTree 1.1)",
                                            "Crossfeed Enabler (CrossFeedEnabler v3.1)",
                                            "Custom Biomes (CustomBiomes 1.6.8)",
                                            "Custom Biomes (Real Solar System data) (CustomBiomes-Data-RSS v8.2.1-actually-v8.3preview)",
                                            "DDSLoader (DDSLoader 1.7.0.0)",
                                            "Deadly Reentry Continued (DeadlyReentry v6.2.1)",
                                            "DMagic Orbital Science (DMagicOrbitalScience 0.9.0.1)",
                                            "Dogecoin Flag (DogeCoinFlag 1.02)",
                                            "Engine Ignitor (repack) (EngineIgnitor-Unofficial-Repack 3.4.1.1)",
                                            "EVE Overhaul - Core (EVE-Overhaul-Core 0.0.2014.11.25)",
                                            "Ferram Aerospace Research (FerramAerospaceResearch v0.14.4)",
                                            "Final Frontier (FinalFrontier 0.5.9-177)",
                                            "Fine Print (FinePrint 0.59)",
                                            "Firespitter Core (FirespitterCore 7.0.5398.27328)",
                                            "HotRockets! Particle FX Replacement (HotRockets 7.25)",
                                            "Karbonite (Karbonite 0.4.4)",
                                            "Kerbal Alarm Clock (KerbalAlarmClock v3.0.5.0)",
                                            "Kerbal Attachement System (KAS 0.4.9)",
                                            "Kerbal Construction Time (KerbalConstructionTime 1.0.3)",
                                            "Kerbal Joint Reinforcement (KerbalJointReinforcement v2.4.4)",
                                            "kOS: Scriptable Autopilot System (kOS 0.15.3.0)",
                                            "Kronal Vessel Viewer (KVV) - Exploded (Orthographic) ship view (KronalVesselViewer 0.0.4_0.25)",
                                            "Magic Smoke Industries Infernal Robotics (InfernalRobotics 0.19.2)",
                                            "MechJeb 2 (MechJeb2 2.4.0)",
                                            "Module Manager (ModuleManager 2.5.1)",
                                            "ModuleRCSFX (ModuleRCSFX v3.3)",
                                            "NathanKell's RVE haxx (NathanKell-RVE-Haxx 0.0.2014.11.25)",
                                            "Open Resource System Fork (ORSX 0.1.3)",
                                            "Part Catalog (PartCatalog 3.0_RC8)",
                                            "PlanetShine (PlanetShine 0.2.2)",
                                            "Precise Node (PreciseNode 1.1.1)",
                                            "Procedural Fairings (ProceduralFairings v3.10)",
                                            "Procedural Parts (ProceduralParts v0.9.20)",
                                            "Procedural Wings (ProceduralDynamics 0.9.1)",
                                            "Real Fuels (RealFuels rf-v8.1-really-v8.2-pre)",
                                            "Real Solar System (RealSolarSystem v8.2.1-actually-v8.3preview)",
                                            "Real Solar System Textures - 4096 x 2048 (RSSTextures4096 1.0)",
                                            "RealChute Parachute Systems (RealChute 1.2.6)",
                                            "Realism Overhaul (RealismOverhaul v7.0.2)",
                                            "Realistic Progression Zero (RP-0 v0.13)",
                                            "RemoteTech (RemoteTech v1.5.1)",
                                            "RemoteTech RSS Configuration (RemoteTech-Config-RSS 0.0)",
                                            "SCANsat (SCANsat v8.0)",
                                            "Science Alert (ScienceAlert 1.8rc1)",
                                            "Ship Manifest (ShipManifest 0.25.0_3.3.2b)",
                                            "StageRecovery (StageRecovery 1.5.1)",
                                            "SXT - Stock eXTension (SXT 18.6)",
                                            "TAC Life Support (TACLS) (TACLS v0.10.1)",
                                            "TAC Life Support (TACLS) - Realism Overhaul Config (TACLS-Config-RealismOverhaul v7.0.2)",
                                            "TechManager (TechManager 1.4)",
                                            "Toolbar (Toolbar 1.7.7)",
                                            "TweakScale (TweakScale v1.44)",
                                            "Umbra Space Industries Tools (USITools 0.2.4)",
                                            "Universal Storage (UniversalStorage 0.9.4)",
                                            "Universal Storage KAS Pack (UniversalStorage-KAS 0.9.0.14)",
                                            "Universal Storage TAC Pack (UniversalStorage-TAC 0.9.2.7)",
                                            "USI Kolonization Systems (MKS/OKS) (UKS 0.21.3)",
                                          },
                                          ReadAllLines(output).ToArray());
            }
        }

        [Test]
        public void RunCommand_ExportMarkdown_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
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
            using (var output   = new MemoryStream())
            {
                regMgr.registry.RepositoriesAdd(repo);
                ICommand sut    = new List(repoData.Manager, user, output);
                var      opts   = new ListOptions() { export = "markdown" };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                            "- **6S Service Compartment Tubes** `Service-Compartments-6S 1.2`",
                                            "- **Action Groups Extended** `AGExt 1.23c`",
                                            "- **Advanced Jet Engine (AJE)** `AJE 1.6.4`",
                                            "- **Alternate Resource Panel** `AlternateResourcePanel 2.6.1.0`",
                                            "- **Chatterer** `Chatterer 0.7.1`",
                                            "- **CIT Utils** `CIT-Util 1.0.4-unofficial`",
                                            "- **Community Resource Pack** `CommunityResourcePack 0.2.3`",
                                            "- **Community Tech Tree** `CommunityTechTree 1.1`",
                                            "- **Crossfeed Enabler** `CrossFeedEnabler v3.1`",
                                            "- **Custom Biomes** `CustomBiomes 1.6.8`",
                                            "- **Custom Biomes (Real Solar System data)** `CustomBiomes-Data-RSS v8.2.1-actually-v8.3preview`",
                                            "- **DDSLoader** `DDSLoader 1.7.0.0`",
                                            "- **Deadly Reentry Continued** `DeadlyReentry v6.2.1`",
                                            "- **DMagic Orbital Science** `DMagicOrbitalScience 0.9.0.1`",
                                            "- **Dogecoin Flag** `DogeCoinFlag 1.02`",
                                            "- **Engine Ignitor (repack)** `EngineIgnitor-Unofficial-Repack 3.4.1.1`",
                                            "- **EVE Overhaul - Core** `EVE-Overhaul-Core 0.0.2014.11.25`",
                                            "- **Ferram Aerospace Research** `FerramAerospaceResearch v0.14.4`",
                                            "- **Final Frontier** `FinalFrontier 0.5.9-177`",
                                            "- **Fine Print** `FinePrint 0.59`",
                                            "- **Firespitter Core** `FirespitterCore 7.0.5398.27328`",
                                            "- **HotRockets! Particle FX Replacement** `HotRockets 7.25`",
                                            "- **Karbonite** `Karbonite 0.4.4`",
                                            "- **Kerbal Alarm Clock** `KerbalAlarmClock v3.0.5.0`",
                                            "- **Kerbal Attachement System** `KAS 0.4.9`",
                                            "- **Kerbal Construction Time** `KerbalConstructionTime 1.0.3`",
                                            "- **Kerbal Joint Reinforcement** `KerbalJointReinforcement v2.4.4`",
                                            "- **kOS: Scriptable Autopilot System** `kOS 0.15.3.0`",
                                            "- **Kronal Vessel Viewer (KVV) - Exploded (Orthographic) ship view** `KronalVesselViewer 0.0.4_0.25`",
                                            "- **Magic Smoke Industries Infernal Robotics** `InfernalRobotics 0.19.2`",
                                            "- **MechJeb 2** `MechJeb2 2.4.0`",
                                            "- **Module Manager** `ModuleManager 2.5.1`",
                                            "- **ModuleRCSFX** `ModuleRCSFX v3.3`",
                                            "- **NathanKell's RVE haxx** `NathanKell-RVE-Haxx 0.0.2014.11.25`",
                                            "- **Open Resource System Fork** `ORSX 0.1.3`",
                                            "- **Part Catalog** `PartCatalog 3.0_RC8`",
                                            "- **PlanetShine** `PlanetShine 0.2.2`",
                                            "- **Precise Node** `PreciseNode 1.1.1`",
                                            "- **Procedural Fairings** `ProceduralFairings v3.10`",
                                            "- **Procedural Parts** `ProceduralParts v0.9.20`",
                                            "- **Procedural Wings** `ProceduralDynamics 0.9.1`",
                                            "- **Real Fuels** `RealFuels rf-v8.1-really-v8.2-pre`",
                                            "- **Real Solar System** `RealSolarSystem v8.2.1-actually-v8.3preview`",
                                            "- **Real Solar System Textures - 4096 x 2048** `RSSTextures4096 1.0`",
                                            "- **RealChute Parachute Systems** `RealChute 1.2.6`",
                                            "- **Realism Overhaul** `RealismOverhaul v7.0.2`",
                                            "- **Realistic Progression Zero** `RP-0 v0.13`",
                                            "- **RemoteTech** `RemoteTech v1.5.1`",
                                            "- **RemoteTech RSS Configuration** `RemoteTech-Config-RSS 0.0`",
                                            "- **SCANsat** `SCANsat v8.0`",
                                            "- **Science Alert** `ScienceAlert 1.8rc1`",
                                            "- **Ship Manifest** `ShipManifest 0.25.0_3.3.2b`",
                                            "- **StageRecovery** `StageRecovery 1.5.1`",
                                            "- **SXT - Stock eXTension** `SXT 18.6`",
                                            "- **TAC Life Support (TACLS)** `TACLS v0.10.1`",
                                            "- **TAC Life Support (TACLS) - Realism Overhaul Config** `TACLS-Config-RealismOverhaul v7.0.2`",
                                            "- **TechManager** `TechManager 1.4`",
                                            "- **Toolbar** `Toolbar 1.7.7`",
                                            "- **TweakScale** `TweakScale v1.44`",
                                            "- **Umbra Space Industries Tools** `USITools 0.2.4`",
                                            "- **Universal Storage** `UniversalStorage 0.9.4`",
                                            "- **Universal Storage KAS Pack** `UniversalStorage-KAS 0.9.0.14`",
                                            "- **Universal Storage TAC Pack** `UniversalStorage-TAC 0.9.2.7`",
                                            "- **USI Kolonization Systems (MKS/OKS)** `UKS 0.21.3`",
                                          },
                                          ReadAllLines(output).ToArray());
            }
        }

        [Test]
        public void RunCommand_ExportBBCode_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
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
            using (var output   = new MemoryStream())
            {
                regMgr.registry.RepositoriesAdd(repo);
                ICommand sut    = new List(repoData.Manager, user, output);
                var      opts   = new ListOptions() { export = "bbcode" };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "[LIST]",
                                              "[*][B]6S Service Compartment Tubes[/B] (Service-Compartments-6S 1.2)",
                                              "[*][B]Action Groups Extended[/B] (AGExt 1.23c)",
                                              "[*][B]Advanced Jet Engine (AJE)[/B] (AJE 1.6.4)",
                                              "[*][B]Alternate Resource Panel[/B] (AlternateResourcePanel 2.6.1.0)",
                                              "[*][B]Chatterer[/B] (Chatterer 0.7.1)",
                                              "[*][B]CIT Utils[/B] (CIT-Util 1.0.4-unofficial)",
                                              "[*][B]Community Resource Pack[/B] (CommunityResourcePack 0.2.3)",
                                              "[*][B]Community Tech Tree[/B] (CommunityTechTree 1.1)",
                                              "[*][B]Crossfeed Enabler[/B] (CrossFeedEnabler v3.1)",
                                              "[*][B]Custom Biomes[/B] (CustomBiomes 1.6.8)",
                                              "[*][B]Custom Biomes (Real Solar System data)[/B] (CustomBiomes-Data-RSS v8.2.1-actually-v8.3preview)",
                                              "[*][B]DDSLoader[/B] (DDSLoader 1.7.0.0)",
                                              "[*][B]Deadly Reentry Continued[/B] (DeadlyReentry v6.2.1)",
                                              "[*][B]DMagic Orbital Science[/B] (DMagicOrbitalScience 0.9.0.1)",
                                              "[*][B]Dogecoin Flag[/B] (DogeCoinFlag 1.02)",
                                              "[*][B]Engine Ignitor (repack)[/B] (EngineIgnitor-Unofficial-Repack 3.4.1.1)",
                                              "[*][B]EVE Overhaul - Core[/B] (EVE-Overhaul-Core 0.0.2014.11.25)",
                                              "[*][B]Ferram Aerospace Research[/B] (FerramAerospaceResearch v0.14.4)",
                                              "[*][B]Final Frontier[/B] (FinalFrontier 0.5.9-177)",
                                              "[*][B]Fine Print[/B] (FinePrint 0.59)",
                                              "[*][B]Firespitter Core[/B] (FirespitterCore 7.0.5398.27328)",
                                              "[*][B]HotRockets! Particle FX Replacement[/B] (HotRockets 7.25)",
                                              "[*][B]Karbonite[/B] (Karbonite 0.4.4)",
                                              "[*][B]Kerbal Alarm Clock[/B] (KerbalAlarmClock v3.0.5.0)",
                                              "[*][B]Kerbal Attachement System[/B] (KAS 0.4.9)",
                                              "[*][B]Kerbal Construction Time[/B] (KerbalConstructionTime 1.0.3)",
                                              "[*][B]Kerbal Joint Reinforcement[/B] (KerbalJointReinforcement v2.4.4)",
                                              "[*][B]kOS: Scriptable Autopilot System[/B] (kOS 0.15.3.0)",
                                              "[*][B]Kronal Vessel Viewer (KVV) - Exploded (Orthographic) ship view[/B] (KronalVesselViewer 0.0.4_0.25)",
                                              "[*][B]Magic Smoke Industries Infernal Robotics[/B] (InfernalRobotics 0.19.2)",
                                              "[*][B]MechJeb 2[/B] (MechJeb2 2.4.0)",
                                              "[*][B]Module Manager[/B] (ModuleManager 2.5.1)",
                                              "[*][B]ModuleRCSFX[/B] (ModuleRCSFX v3.3)",
                                              "[*][B]NathanKell's RVE haxx[/B] (NathanKell-RVE-Haxx 0.0.2014.11.25)",
                                              "[*][B]Open Resource System Fork[/B] (ORSX 0.1.3)",
                                              "[*][B]Part Catalog[/B] (PartCatalog 3.0_RC8)",
                                              "[*][B]PlanetShine[/B] (PlanetShine 0.2.2)",
                                              "[*][B]Precise Node[/B] (PreciseNode 1.1.1)",
                                              "[*][B]Procedural Fairings[/B] (ProceduralFairings v3.10)",
                                              "[*][B]Procedural Parts[/B] (ProceduralParts v0.9.20)",
                                              "[*][B]Procedural Wings[/B] (ProceduralDynamics 0.9.1)",
                                              "[*][B]Real Fuels[/B] (RealFuels rf-v8.1-really-v8.2-pre)",
                                              "[*][B]Real Solar System[/B] (RealSolarSystem v8.2.1-actually-v8.3preview)",
                                              "[*][B]Real Solar System Textures - 4096 x 2048[/B] (RSSTextures4096 1.0)",
                                              "[*][B]RealChute Parachute Systems[/B] (RealChute 1.2.6)",
                                              "[*][B]Realism Overhaul[/B] (RealismOverhaul v7.0.2)",
                                              "[*][B]Realistic Progression Zero[/B] (RP-0 v0.13)",
                                              "[*][B]RemoteTech[/B] (RemoteTech v1.5.1)",
                                              "[*][B]RemoteTech RSS Configuration[/B] (RemoteTech-Config-RSS 0.0)",
                                              "[*][B]SCANsat[/B] (SCANsat v8.0)",
                                              "[*][B]Science Alert[/B] (ScienceAlert 1.8rc1)",
                                              "[*][B]Ship Manifest[/B] (ShipManifest 0.25.0_3.3.2b)",
                                              "[*][B]StageRecovery[/B] (StageRecovery 1.5.1)",
                                              "[*][B]SXT - Stock eXTension[/B] (SXT 18.6)",
                                              "[*][B]TAC Life Support (TACLS)[/B] (TACLS v0.10.1)",
                                              "[*][B]TAC Life Support (TACLS) - Realism Overhaul Config[/B] (TACLS-Config-RealismOverhaul v7.0.2)",
                                              "[*][B]TechManager[/B] (TechManager 1.4)",
                                              "[*][B]Toolbar[/B] (Toolbar 1.7.7)",
                                              "[*][B]TweakScale[/B] (TweakScale v1.44)",
                                              "[*][B]Umbra Space Industries Tools[/B] (USITools 0.2.4)",
                                              "[*][B]Universal Storage[/B] (UniversalStorage 0.9.4)",
                                              "[*][B]Universal Storage KAS Pack[/B] (UniversalStorage-KAS 0.9.0.14)",
                                              "[*][B]Universal Storage TAC Pack[/B] (UniversalStorage-TAC 0.9.2.7)",
                                              "[*][B]USI Kolonization Systems (MKS/OKS)[/B] (UKS 0.21.3)",
                                              "[/LIST]",
                                          },
                                          ReadAllLines(output).ToArray());
            }
        }

        [Test]
        public void RunCommand_ExportCSV_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
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
            using (var output   = new MemoryStream())
            {
                regMgr.registry.RepositoriesAdd(repo);
                ICommand sut    = new List(repoData.Manager, user, output);
                var      opts   = new ListOptions() { export = "csv" };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                            "identifier,version,name,abstract,description,author,kind,download,download_size,ksp_version,ksp_version_min,ksp_version_max,license,release_status,repository,homepage,bugtracker,discussions,spacedock,curse",
                                            "Service-Compartments-6S,1.2,6S Service Compartment Tubes,\"It's a tube.. and a hull... and a cargo bay... and a.. well, just about everything. And it is only 2 PARTS!!!\",,nothke,package,http://addons.cursecdn.com/files/2201/755/tubes.zip,0,0.25,,,CC-BY-NC-SA,stable,,http://forum.kerbalspaceprogram.com/threads/61040-0-23-6S-Service-Compartment-Tubes-Design-smooth!,,,,",
                                            "AGExt,1.23c,Action Groups Extended,Increases the number of action groups to 250 and allows in-flight editing.,,Diazo,package,https://github.com/SirDiazo/AGExt/releases/download/1.23c/ActionGroupsExtended123c.zip,63784,0.25,,,GPL-3.0,stable,https://github.com/SirDiazo/AGExt,http://forum.kerbalspaceprogram.com/threads/74195,,,,",
                                            "AJE,1.6.4,Advanced Jet Engine (AJE),Realistic jet engines for KSP,,,package,https://github.com/camlost2/AJE/archive/1.6.4.zip,0,0.25,,,LGPL-2.1,stable,,http://forum.kerbalspaceprogram.com/threads/70008,,,,",
                                            "AlternateResourcePanel,2.6.1.0,Alternate Resource Panel,An alternate view of the resources app with lots of shiny bells and whistles,,TriggerAu,package,https://kerbalstuff.com/mod/195/Alternate%20Resource%20Panel/download/2.6.1.0,302728,0.25,,,MIT,stable,,http://forum.kerbalspaceprogram.com/threads/60227-KSP-Alternate-Resource-Panel,,,,",
                                            "Chatterer,0.7.1,Chatterer,\"A plugin for Kerbal Space Program from SQUAD, which adds some SSTV, beeps, and nonsensical radio chatter between your crewed command pods and Mission Control. There is also some environmental sounds as : wind in atmosphere, breathing on EVA and background noises inside space-crafts.\",,Athlonic,package,https://kerbalstuff.com/mod/124/Chatterer/download/0.7.1,6119898,0.25,,,GPL-3.0,stable,,http://forum.kerbalspaceprogram.com/threads/92324-0-24-2-Chatterer-v-0-6-0-Aug-29-2014,,,,",
                                            "CIT-Util,1.0.4-unofficial,CIT Utils,Caelum Ire Technologies shared utilities,,marce,package,https://github.com/pjf/KSP-CIT-Util/releases/download/1.0.4-unofficial/CITUtil_1.0.4.zip,14914,0.25,,,CC-BY-NC-SA-4.0,stable,https://github.com/marce155/KSP-CIT-Util,https://ksp.marce.at/,,,,",
                                            "CommunityResourcePack,0.2.3,Community Resource Pack,Common resources for KSP mods,,RoverDude,package,https://github.com/BobPalmer/CommunityResourcePack/releases/download/0.2.3/CRP_0.2.3.zip,16354356,0.25,,,CC-BY-NC-SA-4.0,stable,https://github.com/BobPalmer/CommunityResourcePack,http://forum.kerbalspaceprogram.com/threads/91998,,,,",
                                            "CommunityTechTree,1.1,Community Tech Tree,An extension for the stock technology tree designed to support many mods,,Nertea,package,https://kerbalstuff.com/mod/345/Community%20Tech%20Tree/download/1.1,11425,0.25,,,CC-BY-NC-4.0,stable,,,,,,",
                                            "CrossFeedEnabler,v3.1,Crossfeed Enabler,\"Adds toggleable fuel crossfeed between the part it's added to, and the part this part is surface-attached to. Use it for radial tanks.\",,NathanKell,package,https://github.com/NathanKell/CrossFeedEnabler/releases/download/v3.1/CrossFeedEnabler_v3.1.zip,30773,0.25,,,CC-BY-SA,stable,https://github.com/NathanKell/CrossFeedEnabler,,,,,",
                                            "CustomBiomes,1.6.8,Custom Biomes,Add or replace biomes to any celestial body in KSP,,,package,http://addons.cursecdn.com/files/2217%5C373/CustomBiomes_1_6_8.zip,0,0.25,,,CC-BY-NC-SA-3.0,stable,,http://forum.kerbalspaceprogram.com/threads/66256,,,,",
                                            "CustomBiomes-Data-RSS,v8.2.1-actually-v8.3preview,Custom Biomes (Real Solar System data),Custom biomes for the Real Solar System,,NathanKell,package,https://github.com/NathanKell/RealSolarSystem/releases/download/v8.3preview/RealSolarSystem_v8.3.zip,0,,,,CC-BY-NC-SA,stable,https://github.com/NathanKell/RealSolarSystem,http://forum.kerbalspaceprogram.com/threads/55145,,,,",
                                            "DDSLoader,1.7.0.0,DDSLoader,Loads DDS Textures boringly fast!,,sarbian,package,https://ksp.sarbian.com/jenkins/job/DDSLoader/lastSuccessfulBuild/artifact/DDSLoader-1.7.0.0.zip,0,0.25,,,CC-BY-NC-SA-4.0,stable,https://github.com/sarbian/DDSLoader/,http://forum.kerbalspaceprogram.com/threads/96729,,,,",
                                            "DeadlyReentry,v6.2.1,Deadly Reentry Continued,Makes re-entry much more dangerous,,Starwaster,package,https://github.com/Starwaster/DeadlyReentry/releases/download/v6.2.1/DeadlyReentry_v6.2.1.zip,1949669,0.25,,,CC-BY-SA,stable,https://github.com/Starwaster/DeadlyReentry,http://forum.kerbalspaceprogram.com/threads/54954,,,,",
                                            "DMagicOrbitalScience,0.9.0.1,DMagic Orbital Science,New science parts and experiments,,DMagic,package,https://kerbalstuff.com/mod/5/DMagic%20Orbital%20Science/download/0.9.0.1,9331846,0.25,,,BSD-3-clause,stable,,http://forum.kerbalspaceprogram.com/threads/64972,,,,",
                                            "DogeCoinFlag,1.02,Dogecoin Flag,Such flag. Very currency. To the mun! Wow!,,pjf,package,https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.02,53359,,,,CC-BY,stable,,https://www.reddit.com/r/dogecoin/comments/1tdlgg/i_made_a_more_accurate_dogecoin_and_a_ksp_flag/,,,,",
                                            "EngineIgnitor-Unofficial-Repack,3.4.1.1,Engine Ignitor (repack),Engine ignition and throttles now work like real engines.,,HoneyFox (repacked by pjf),package,https://github.com/pjf/EngineIgnitor/releases/download/3.4.1.1/EngineIgnitor-3.4.1.1.zip,652429,0.25.0,,,MIT,stable,https://github.com/pjf/EngineIgnitor,http://forum.kerbalspaceprogram.com/threads/51880,,,,",
                                            "EVE-Overhaul-Core,0.0.2014.11.25,EVE Overhaul - Core,EVE h4xx0red by pjf for the CKAN. This may eat your system,,rbray89,package,https://github.com/rbray89/EnvironmentalVisualEnhancements/raw/Overhaul/x64-Release.zip,0,0.25.0,,,MIT,development,,,,,,",
                                            "FerramAerospaceResearch,v0.14.4,Ferram Aerospace Research,FAR replaces KSP's stock mass-based aerodynamics model with one based on real-life physics.,,ferram4,package,https://kerbalstuff.com/mod/52/Ferram%20Aerospace%20Research/download/v0.14.4,209252,0.25,,,GPL-3.0,stable,https://github.com/ferram4/Ferram-Aerospace-Research,http://kerbalspaceprogram.com/forum/showthread.php/20451,,,,",
                                            "FinalFrontier,0.5.9-177,Final Frontier,\"The Final Frontier plugin will handle ribbons for extraordinary merits for you. And the number of missions flown (i.e. vessel recovered) is recorded, too.\",,Nereid,package,http://addons.curse.cursecdn.com/files/2216/741/FinalFrontier0.5.9-177.zip,2442489,0.25,,,BSD-2-clause,stable,,http://forum.kerbalspaceprogram.com/threads/67246,,,,",
                                            "FinePrint,0.59,Fine Print, Fine Print gives purpose to many formerly meaningless activities like driving rovers and building stations by more than doubling the amount of contract types in the game.,,Arsonide,package,https://kerbalstuff.com/mod/81/Fine%20Print/download/0.59,210864,0.25,,,GPL-3.0,stable,,http://forum.kerbalspaceprogram.com/threads/88445-0-24-2-Fine-Print-v0-5a-Formerly-Extra-Contracts-%28July-27%29,,,,",
                                            "FirespitterCore,7.0.5398.27328,Firespitter Core,Core Firespitter.dll. Install `Firespitter` for the whole shebang,,Snjo,package,http://firespitter.s3.amazonaws.com/FirespitterCore-7.0.5398.27328.zip,0,,,,restricted,stable,,http://forum.kerbalspaceprogram.com/threads/24551-Firespitter-propeller-plane-and-helicopter-parts,,,,",
                                            "HotRockets,7.25,HotRockets! Particle FX Replacement,Replacement for stock engine particle FX,,Nazari1382,package,http://addons.cursecdn.com/files/2216%5C167/HotRockets_7.25_CorePack.zip,0,0.25,,,CC-BY-NC,stable,,http://forum.kerbalspaceprogram.com/threads/65754,,,,",
                                            "Karbonite,0.4.4,Karbonite,\"A newly discovered mineral that has hydrocarbon-like properties and is perfect for processing into a fuel. It's easily mined from the surface, and can be found in liquid or gaseous forms on certain planets.\",,RoverDude,package,https://github.com/BobPalmer/Karbonite/releases/download/0.4.4/Karbonite_0.4.4.zip,39015866,0.25,,,CC-BY-NC-SA,stable,https://github.com/BobPalmer/Karbonite,http://forum.kerbalspaceprogram.com/threads/87335,,,,",
                                            "KerbalAlarmClock,v3.0.5.0,Kerbal Alarm Clock,Create reminder alarms to help manage flights and not warp past important times,,TriggerAu,package,https://github.com/TriggerAu/KerbalAlarmClock/releases/download/v3.0.5.0/KerbalAlarmClock_3.0.5.0.zip,406598,0.25,,,MIT,stable,https://github.com/TriggerAu/KerbalAlarmClock,http://forum.kerbalspaceprogram.com/threads/24786-0-25-0-Kerbal-Alarm-Clock-v3-0-1-0-%28Oct-26%29,,,,",
                                            "KAS,0.4.9,Kerbal Attachement System,\"KAS introduce new gameplay mechanics by adding winches, containers, dynamic struts/pipes and grabable & attachable parts.\",,KospY;Winn75;zzz;Majiir;a.g.,package,http://kerbal.curseforge.com/ksp-mods/223900-kerbal-attachment-system-kas/files/2216138/download,0,0.25,,,restricted,stable,,,,,,",
                                            "KerbalConstructionTime,1.0.3,Kerbal Construction Time,Unrapid Planned Assembly,,magico13,package,https://kerbalstuff.com/mod/125/Kerbal%20Construction%20Time/download/1.0.3,574652,0.25,,,GPL-3.0,stable,,http://forum.kerbalspaceprogram.com/threads/92377-0-24-2-Kerbal-Construction-Time-Release-v1-0-0-%288-29-14%29,,,,",
                                            "KerbalJointReinforcement,v2.4.4,Kerbal Joint Reinforcement,KJR tightens up the joints between parts and adds some physics-adjusting parameters to make vehicles more stable when loading on the launchpad or coming out of timewarp.,,ferram4,package,https://kerbalstuff.com/mod/53/Kerbal%20Joint%20Reinforcement/download/v2.4.4,36034,0.25,,,GPL-3.0,stable,https://github.com/ferram4/Kerbal-Joint-Reinforcement/,http://forum.kerbalspaceprogram.com/threads/55657,,,,",
                                            "kOS,0.15.3.0,kOS: Scriptable Autopilot System,kOS is a scriptable autopilot Mod for Kerbal Space Program. It allows you write small programs that automate specific tasks.,,erendrake,package,https://kerbalstuff.com/mod/86/kOS:%20Scriptable%20Autopilot%20System/download/0.15.3.0,2526552,0.25,,,GPL-3.0,stable,,http://ksp-kos.github.io/KOS_DOC/,,,,",
                                            "KronalVesselViewer,0.0.4_0.25,Kronal Vessel Viewer (KVV) - Exploded (Orthographic) ship view,Plugin that creates blue-print-like (Orthographic) images (into your Screenshots/ directory) from the KSP Editor (VAB/SPH),,bigorangemachine,package,https://kerbalstuff.com/mod/211/Kronal%20Vessel%20Viewer%20%28KVV%29%20-%20Exploded%20%28Orthographic%29%20ship%20view/download/0.0.4_0.25,1261756,0.25,,,unrestricted,stable,,https://github.com/bigorangemachine/ksp-kronalutils/releases,,,,",
                                            "InfernalRobotics,0.19.2,Magic Smoke Industries Infernal Robotics,Making stuff move.,,sirkut,package,https://kerbalstuff.com/mod/8/Magic%20Smoke%20Industries%20Infernal%20Robotics/download/0.19.2,11664244,0.25,,,GPL-3.0,stable,https://github.com/sirkut/InfernalRobotics,http://forum.kerbalspaceprogram.com/threads/37707-0-23-5-Magic-Smoke-Industries-Infernal-Robotics-0-16-4,,,,",
                                            "MechJeb2,2.4.0,MechJeb 2,Anatid Robotics and Multiversal Mechatronics proudly presents the first flight assistant autopilot: MechJeb,,sarbian,package,http://addons.curse.cursecdn.com/files/2216/245/MechJeb2-2.4.0.0.zip,0,0.25,,,GPL-3.0,stable,,http://forum.kerbalspaceprogram.com/threads/12384-PART-0-25-Anatid-Robotics-MuMech-MechJeb-Autopilot-v2-4-0,,,,",
                                            "ModuleManager,2.5.1,Module Manager,Modify KSP configs without conflict,,ialdabaoth;Sarbian,package,https://ksp.sarbian.com/jenkins/job/ModuleManager/lastSuccessfulBuild/artifact/ModuleManager-2.5.1.zip,0,0.25,,,CC-BY-SA,stable,,,,,,",
                                            "ModuleRCSFX,v3.3,ModuleRCSFX,A fixed version of the stock RCS module,,ialdabaoth;NathanKell,package,https://github.com/NathanKell/ModuleRCSFX/releases/download/v3.3/ModuleRCSFX_v3.3.zip,5598,0.25,,,CC-BY-SA,stable,https://github.com/NathanKell/ModuleRCSFX,http://forum.kerbalspaceprogram.com/threads/92290,,,,",
                                            "NathanKell-RVE-Haxx,0.0.2014.11.25,NathanKell's RVE haxx,Nathan's files h4xx0red by pjf for the CKAN. This may eat your system,,unknown,package,https://nabaal.net/files/ksp/nathankell/RealSolarSystem/Textures/MyRVE.zip,0,0.25.0,,,unknown,development,,,,,,",
                                            "ORSX,0.1.3,Open Resource System Fork,An ORS Fork,,RoverDude,package,https://github.com/BobPalmer/ORSX/releases/download/0.1.3/ORSX_0.1.3.zip,64566,0.25,,,BSD-3-clause,stable,https://github.com/BobPalmer/ORSX,,,,,",
                                            "PartCatalog,3.0_RC8,Part Catalog,The PartCatalog allows you categorize your parts into Tags in order to make your building experience less frustrating by reducing the time you're looking for that one part.,,BlackNecro,package,https://dl.dropboxusercontent.com/u/11467249/PartCatalog/PartCatalog3.0_RC8.zip,0,0.25,,,CC-BY-SA-3.0,stable,,http://forum.kerbalspaceprogram.com/threads/35018-0-25-PartCatalog-3-0-RC8-%282014-10-08%29,,,,",
                                            "PlanetShine,0.2.2,PlanetShine,Planets and moons reflects their light to your vessel + other ambient light improvements,,Valerian,package,https://kerbalstuff.com/mod/266/PlanetShine/download/0.2.2,41869,0.25,,,Apache-2.0,stable,,http://forum.kerbalspaceprogram.com/threads/96497,,,,",
                                            "PreciseNode,1.1.1,Precise Node,Provides a more precise widget for maneuver node editing,,,package,http://blizzy.de/precise-node/PreciseNode-1.1.1.zip,0,0.25,,,BSD-2-clause,stable,,http://forum.kerbalspaceprogram.com/threads/47863-0-25-0-Precise-Node-1-1-1-Precisely-edit-your-maneuver-nodes,,,,",
                                            "ProceduralFairings,v3.10,Procedural Fairings,Fairings that automatically reshape for any attached payload,,e-dog,package,https://github.com/e-dog/ProceduralFairings/releases/download/v3.10/ProcFairings_3.10.zip,1776825,0.25,,,CC-BY-3.0,stable,https://github.com/e-dog/ProceduralFairings,,,,,",
                                            "ProceduralParts,v0.9.20,Procedural Parts,ProceduralParts allows you to procedurally generate a number of different parts in a range of sizes and shapes.,,NathanKell,package,https://github.com/Swamp-Ig/ProceduralParts/releases/download/v0.9.20/ProceduralParts-0.9.20.zip,7403194,0.25,,,CC-BY-3.0,stable,https://github.com/Swamp-Ig/ProceduralParts,,,,,",
                                            "ProceduralDynamics,0.9.1,Procedural Wings,\"Procedural Wings, makes wings more procedural.\",,DYJ,package,https://kerbalstuff.com/mod/64/Procedural%20Wings/download/0.9.1,1236752,0.25,,,restricted,stable,,http://forum.kerbalspaceprogram.com/threads/29862-0-24-Procedural-Dynamics-Procedural-Wing-0-8,,,,",
                                            "RealFuels,rf-v8.1-really-v8.2-pre,Real Fuels,Real fuels and tanks for KSP,,ialdabaoth;NathanKell,package,https://github.com/NathanKell/ModularFuelSystem/releases/download/rf-v8.2pre/RealFuels_v8.2.zip,0,0.25,,,CC-BY-SA,stable,https://github.com/NathanKell/ModularFuelSystem,http://forum.kerbalspaceprogram.com/threads/64118,,,,",
                                            "RealSolarSystem,v8.2.1-actually-v8.3preview,Real Solar System,Resizes and rearranges the Kerbal system to more closely resemble the Solar System,,NathanKell,package,https://github.com/NathanKell/RealSolarSystem/releases/download/v8.3preview/RealSolarSystem_v8.3.zip,0,0.25.0,,,CC-BY-NC-SA,stable,https://github.com/NathanKell/RealSolarSystem,http://forum.kerbalspaceprogram.com/threads/55145,,,,",
                                            "RSSTextures4096,1.0,Real Solar System Textures - 4096 x 2048,Textures for Real Solar Systems,,,package,https://nabaal.net/files/ksp/nathankell/RealSolarSystem/Textures/4096.zip,0,,,,CC-BY-NC-SA,stable,,,,,,",
                                            "RealChute,1.2.6,RealChute Parachute Systems,RealChute is a complete rework of the stock parachute module to fix a few of it's inconveniences and get more realistic results out of parachutes!,,stupid_chris,package,https://kerbalstuff.com/mod/71/RealChute%20Parachute%20Systems/download/1.2.6,2672129,,0.25.0,0.25.0,restricted,stable,https://github.com/StupidChris/RealChute,http://forum.kerbalspaceprogram.com/threads/57988,,,,",
                                            "RealismOverhaul,v7.0.2,Realism Overhaul,Multipatch to KSP to give things realistic stats and sizes,,Felger,package,https://github.com/KSP-RO/RealismOverhaul/releases/download/v7.0.2/RealismOverhaul7_0_2.zip,1395210,0.25.0,,,CC-BY-SA,stable,https://github.com/KSP-RO/RealismOverhaul,http://forum.kerbalspaceprogram.com/threads/99966,,,,",
                                            "RP-0,v0.13,Realistic Progression Zero,Realistic Progression Zero - The lightweight Realism Overhaul tech tree,,RP-0 Group,package,https://github.com/KSP-RO/RP-0/releases/download/v0.13/RP-0-v0.13.zip,40798,,,,CC-BY-4.0,development,https://github.com/KSP-RO/RP-0,,,,,",
                                            "RemoteTech,v1.5.1,RemoteTech,Adds new rules for communicating with unmanned probes.,RemoteTech allows you to construct vast relay networks of communication satellites and remotely controlled unmanned vehicles. Your unmanned vessels require an uplink to a command station to be controlled. This adds a new layer of difficulty that compensates for the lack of live crew members.,Remote Technologies Group,package,https://kerbalstuff.com/mod/134/RemoteTech/download/v1.5.1,5418104,0.25,,,restricted,stable,https://github.com/RemoteTechnologiesGroup/RemoteTech,http://forum.kerbalspaceprogram.com/threads/83305,https://github.com/RemoteTechnologiesGroup/RemoteTech/issues,,,",
                                            "RemoteTech-Config-RSS,0.0,RemoteTech RSS Configuration,Adds ground stations to launch sites in Real Solar System,,CerberusRCAF,package,https://www.dropbox.com/s/ohqv9r9mwng2500/RemoteTech_RSS_Settings.zip?dl=1,0,0.25,,,CC-BY-4.0,stable,,http://forum.kerbalspaceprogram.com/threads/99966-0-25-0-Realism-Overhaul-6-1-2c,,,,",
                                            "SCANsat,v8.0,SCANsat,\"SCANsat: Real Scanning, Real Science, Warp Speed!\",,DMagic,package,https://kerbalstuff.com/mod/249/SCANsat/download/v8.0,1961163,0.25,,,restricted,stable,,http://forum.kerbalspaceprogram.com/threads/80369,,,,",
                                            "ScienceAlert,1.8rc1,Science Alert,Is it time for science? Get alerts when it is!,,xEvilReeperx,package,https://bitbucket.org/xEvilReeperx/ksp_sciencealert/downloads/KSP-25.0-ScienceAlert-1.8rc1.zip,0,0.25,,,GPL-3.0,testing,,http://forum.kerbalspaceprogram.com/threads/76793,,,,",
                                            "ShipManifest,0.25.0_3.3.2b,Ship Manifest,\"Ship Manifest is a tool to move your ship's \"things\".  Ship Manifest moves crew, Science and Resources around from part to part within your ship or station.\",,Papa_Joe,package,https://kerbalstuff.com/mod/261/Ship%20Manifest/download/0.25.0_3.3.2b,546564,0.25,,,CC-BY-NC-SA-4.0,stable,https://github.com/PapaJoesSoup/ShipManifest,http://forum.kerbalspaceprogram.com/threads/62270,,,,",
                                            "StageRecovery,1.5.1,StageRecovery,Recover Funds+ from Dropped Stages,,magico13,package,https://kerbalstuff.com/mod/97/StageRecovery/download/1.5.1,40331,0.25,,,GPL-3.0,stable,,http://forum.kerbalspaceprogram.com/threads/86677-0-24-2-StageRecovery-Recover-Funds-from-Dropped-Stages-v1-3-%288-5-14%29,,,,",
                                            "SXT,18.6,SXT - Stock eXTension,A collection of (vaguely) stockalike parts that I find useful or just wanted to have.,,Lack,package,https://kerbalstuff.com/mod/338/SXT%20-%20Stock%20eXTension/download/18.6,10463727,0.25,,,CC-BY-NC-SA-4.0,stable,,http://forum.kerbalspaceprogram.com/threads/24906,,,,",
                                            "TACLS,v0.10.1,TAC Life Support (TACLS),Adds life support requirements and resources to all kerbals,,taraniselsu,package,https://github.com/taraniselsu/TacLifeSupport/releases/download/v0.10.1/TacLifeSupport_0.10.1.13.zip,5997448,0.25,,,CC-BY-NC-SA-3.0,stable,https://github.com/taraniselsu/TacLifeSupport,,,,,",
                                            "TACLS-Config-RealismOverhaul,v7.0.2,TAC Life Support (TACLS) - Realism Overhaul Config,Realism Overhaul config for TACLS,,Felger,package,https://github.com/KSP-RO/RealismOverhaul/releases/download/v7.0.2/RealismOverhaul7_0_2.zip,1395210,,,,CC-BY-SA,stable,https://github.com/KSP-RO/RealismOverhaul,,,,,",
                                            "TechManager,1.4,TechManager,\"This mod will allow you to change the tech tree, including the creation of new tech nodes. \",,anonish,package,https://kerbalstuff.com/mod/297/TechManager/download/1.4,39592,0.25,,,MIT,stable,,http://forum.kerbalspaceprogram.com/threads/98293-0-25-TechManager-Version-1-0,,,,",
                                            "Toolbar,1.7.7,Toolbar,API for third-party plugins to provide toolbar buttons,,,package,http://blizzy.de/toolbar/Toolbar-1.7.7.zip,0,0.25,,,BSD-2-clause,stable,,http://forum.kerbalspaceprogram.com/threads/60863,,,,",
                                            "TweakScale,v1.44,TweakScale,Rescale everything!,,Biotronic,package,https://github.com/Biotronic/TweakScale/releases/download/v1.44/TweakScale_1.44.zip,127391,0.25,,,CC-BY-NC-SA-4.0,stable,https://github.com/Biotronic/TweakScale,http://forum.kerbalspaceprogram.com/threads/80234,,,,",
                                            "USITools,0.2.4,Umbra Space Industries Tools,Common libraries for USI mods,,RoverDude,package,https://github.com/BobPalmer/UmbraSpaceIndustries/releases/download/0.2.4/USITools_0.2.4.zip,46094,0.25,,,CC-BY-NC-SA-4.0,stable,https://github.com/BobPalmer/UmbraSpaceIndustries,,,,,",
                                            "UniversalStorage,0.9.4,Universal Storage,Modular resource and processor parts to build custom service modules.,,Paul_Kingtiger,package,https://kerbalstuff.com/mod/250/Universal%20Storage/download/0.9.4,6025862,0.25,,,restricted,stable,,,,,,",
                                            "UniversalStorage-KAS,0.9.0.14,Universal Storage KAS Pack,Universal storage - Kerbal Attachment System extensions,,Paul Kingtiger,package,http://www.kingtiger.co.uk/kingtiger/wordpress/wp-content/uploads/2014/11/US_KAS_0.9.0.14.zip,0,0.25,,,restricted,stable,,http://forum.kerbalspaceprogram.com/threads/75129,,,,",
                                            "UniversalStorage-TAC,0.9.2.7,Universal Storage TAC Pack,Universal storage - TACLS extensions,,Paul Kingtiger,package,http://www.kingtiger.co.uk/kingtiger/wordpress/wp-content/uploads/2014/11/US_TAC_0.9.2.7.zip,0,0.25,,,restricted,stable,,http://forum.kerbalspaceprogram.com/threads/75129,,,,",
                                            "UKS,0.21.3,USI Kolonization Systems (MKS/OKS),\"a series of interlocking modules for building long-term, self sustaining colonies in orbit and on other planets and moons.\",\"MKS introduces a series of parts specifically designed to provide self-sustaining life support for multiple Kerbals for users of TAC-Life Support. While the parts will function without TAC-LS, the design is centered around providing an appropriate challenge where players will weigh the costs, risks, and rewards of supplying missions exclusively with carried life support supplies, or in making the investment in a permanent colony. That being said, this is not just a single greenhouse part (although food production is part of the mod). Rather, it brings an entire colonization end-game to KSP in a fun and challenging way.\",BobPalmer,package,https://github.com/BobPalmer/MKS/releases/download/0.21.3/MKS_0.21.3.zip,38513664,0.25,,,CC-BY-NC-SA-4.0,stable,https://github.com/BobPalmer/MKS,http://forum.kerbalspaceprogram.com/threads/79588-0-25-USI-Kolonization-Systems-%28MKS-OKS%29-%280-21-2%29-2014-10-07,,,,"
                                          },
                                          ReadAllLines(output).ToArray());
            }
        }

        [Test]
        public void RunCommand_ExportTSV_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP(TestData.TestRegistry()))
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
            using (var output   = new MemoryStream())
            {
                regMgr.registry.RepositoriesAdd(repo);
                ICommand sut    = new List(repoData.Manager, user, output);
                var      opts   = new ListOptions() { export = "tsv" };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                            "identifier	version	name	abstract	description	author	kind	download	download_size	ksp_version	ksp_version_min	ksp_version_max	license	release_status	repository	homepage	bugtracker	discussions	spacedock	curse",
                                            "Service-Compartments-6S	1.2	6S Service Compartment Tubes	It's a tube.. and a hull... and a cargo bay... and a.. well, just about everything. And it is only 2 PARTS!!!		nothke	package	http://addons.cursecdn.com/files/2201/755/tubes.zip	0	0.25			CC-BY-NC-SA	stable		http://forum.kerbalspaceprogram.com/threads/61040-0-23-6S-Service-Compartment-Tubes-Design-smooth!				",
                                            "AGExt	1.23c	Action Groups Extended	Increases the number of action groups to 250 and allows in-flight editing.		Diazo	package	https://github.com/SirDiazo/AGExt/releases/download/1.23c/ActionGroupsExtended123c.zip	63784	0.25			GPL-3.0	stable	https://github.com/SirDiazo/AGExt	http://forum.kerbalspaceprogram.com/threads/74195				",
                                            "AJE	1.6.4	Advanced Jet Engine (AJE)	Realistic jet engines for KSP			package	https://github.com/camlost2/AJE/archive/1.6.4.zip	0	0.25			LGPL-2.1	stable		http://forum.kerbalspaceprogram.com/threads/70008				",
                                            "AlternateResourcePanel	2.6.1.0	Alternate Resource Panel	An alternate view of the resources app with lots of shiny bells and whistles		TriggerAu	package	https://kerbalstuff.com/mod/195/Alternate%20Resource%20Panel/download/2.6.1.0	302728	0.25			MIT	stable		http://forum.kerbalspaceprogram.com/threads/60227-KSP-Alternate-Resource-Panel				",
                                            "Chatterer	0.7.1	Chatterer	A plugin for Kerbal Space Program from SQUAD, which adds some SSTV, beeps, and nonsensical radio chatter between your crewed command pods and Mission Control. There is also some environmental sounds as : wind in atmosphere, breathing on EVA and background noises inside space-crafts.		Athlonic	package	https://kerbalstuff.com/mod/124/Chatterer/download/0.7.1	6119898	0.25			GPL-3.0	stable		http://forum.kerbalspaceprogram.com/threads/92324-0-24-2-Chatterer-v-0-6-0-Aug-29-2014				",
                                            "CIT-Util	1.0.4-unofficial	CIT Utils	Caelum Ire Technologies shared utilities		marce	package	https://github.com/pjf/KSP-CIT-Util/releases/download/1.0.4-unofficial/CITUtil_1.0.4.zip	14914	0.25			CC-BY-NC-SA-4.0	stable	https://github.com/marce155/KSP-CIT-Util	https://ksp.marce.at/				",
                                            "CommunityResourcePack	0.2.3	Community Resource Pack	Common resources for KSP mods		RoverDude	package	https://github.com/BobPalmer/CommunityResourcePack/releases/download/0.2.3/CRP_0.2.3.zip	16354356	0.25			CC-BY-NC-SA-4.0	stable	https://github.com/BobPalmer/CommunityResourcePack	http://forum.kerbalspaceprogram.com/threads/91998				",
                                            "CommunityTechTree	1.1	Community Tech Tree	An extension for the stock technology tree designed to support many mods		Nertea	package	https://kerbalstuff.com/mod/345/Community%20Tech%20Tree/download/1.1	11425	0.25			CC-BY-NC-4.0	stable						",
                                            "CrossFeedEnabler	v3.1	Crossfeed Enabler	Adds toggleable fuel crossfeed between the part it's added to, and the part this part is surface-attached to. Use it for radial tanks.		NathanKell	package	https://github.com/NathanKell/CrossFeedEnabler/releases/download/v3.1/CrossFeedEnabler_v3.1.zip	30773	0.25			CC-BY-SA	stable	https://github.com/NathanKell/CrossFeedEnabler					",
                                            "CustomBiomes	1.6.8	Custom Biomes	Add or replace biomes to any celestial body in KSP			package	http://addons.cursecdn.com/files/2217%5C373/CustomBiomes_1_6_8.zip	0	0.25			CC-BY-NC-SA-3.0	stable		http://forum.kerbalspaceprogram.com/threads/66256				",
                                            "CustomBiomes-Data-RSS	v8.2.1-actually-v8.3preview	Custom Biomes (Real Solar System data)	Custom biomes for the Real Solar System		NathanKell	package	https://github.com/NathanKell/RealSolarSystem/releases/download/v8.3preview/RealSolarSystem_v8.3.zip	0				CC-BY-NC-SA	stable	https://github.com/NathanKell/RealSolarSystem	http://forum.kerbalspaceprogram.com/threads/55145				",
                                            "DDSLoader	1.7.0.0	DDSLoader	Loads DDS Textures boringly fast!		sarbian	package	https://ksp.sarbian.com/jenkins/job/DDSLoader/lastSuccessfulBuild/artifact/DDSLoader-1.7.0.0.zip	0	0.25			CC-BY-NC-SA-4.0	stable	https://github.com/sarbian/DDSLoader/	http://forum.kerbalspaceprogram.com/threads/96729				",
                                            "DeadlyReentry	v6.2.1	Deadly Reentry Continued	Makes re-entry much more dangerous		Starwaster	package	https://github.com/Starwaster/DeadlyReentry/releases/download/v6.2.1/DeadlyReentry_v6.2.1.zip	1949669	0.25			CC-BY-SA	stable	https://github.com/Starwaster/DeadlyReentry	http://forum.kerbalspaceprogram.com/threads/54954				",
                                            "DMagicOrbitalScience	0.9.0.1	DMagic Orbital Science	New science parts and experiments		DMagic	package	https://kerbalstuff.com/mod/5/DMagic%20Orbital%20Science/download/0.9.0.1	9331846	0.25			BSD-3-clause	stable		http://forum.kerbalspaceprogram.com/threads/64972				",
                                            "DogeCoinFlag	1.02	Dogecoin Flag	Such flag. Very currency. To the mun! Wow!		pjf	package	https://kerbalstuff.com/mod/269/Dogecoin%20Flag/download/1.02	53359				CC-BY	stable		https://www.reddit.com/r/dogecoin/comments/1tdlgg/i_made_a_more_accurate_dogecoin_and_a_ksp_flag/				",
                                            "EngineIgnitor-Unofficial-Repack	3.4.1.1	Engine Ignitor (repack)	Engine ignition and throttles now work like real engines.		HoneyFox (repacked by pjf)	package	https://github.com/pjf/EngineIgnitor/releases/download/3.4.1.1/EngineIgnitor-3.4.1.1.zip	652429	0.25.0			MIT	stable	https://github.com/pjf/EngineIgnitor	http://forum.kerbalspaceprogram.com/threads/51880				",
                                            "EVE-Overhaul-Core	0.0.2014.11.25	EVE Overhaul - Core	EVE h4xx0red by pjf for the CKAN. This may eat your system		rbray89	package	https://github.com/rbray89/EnvironmentalVisualEnhancements/raw/Overhaul/x64-Release.zip	0	0.25.0			MIT	development						",
                                            "FerramAerospaceResearch	v0.14.4	Ferram Aerospace Research	FAR replaces KSP's stock mass-based aerodynamics model with one based on real-life physics.		ferram4	package	https://kerbalstuff.com/mod/52/Ferram%20Aerospace%20Research/download/v0.14.4	209252	0.25			GPL-3.0	stable	https://github.com/ferram4/Ferram-Aerospace-Research	http://kerbalspaceprogram.com/forum/showthread.php/20451				",
                                            "FinalFrontier	0.5.9-177	Final Frontier	The Final Frontier plugin will handle ribbons for extraordinary merits for you. And the number of missions flown (i.e. vessel recovered) is recorded, too.		Nereid	package	http://addons.curse.cursecdn.com/files/2216/741/FinalFrontier0.5.9-177.zip	2442489	0.25			BSD-2-clause	stable		http://forum.kerbalspaceprogram.com/threads/67246				",
                                            "FinePrint	0.59	Fine Print	 Fine Print gives purpose to many formerly meaningless activities like driving rovers and building stations by more than doubling the amount of contract types in the game.		Arsonide	package	https://kerbalstuff.com/mod/81/Fine%20Print/download/0.59	210864	0.25			GPL-3.0	stable		http://forum.kerbalspaceprogram.com/threads/88445-0-24-2-Fine-Print-v0-5a-Formerly-Extra-Contracts-%28July-27%29				",
                                            "FirespitterCore	7.0.5398.27328	Firespitter Core	Core Firespitter.dll. Install `Firespitter` for the whole shebang		Snjo	package	http://firespitter.s3.amazonaws.com/FirespitterCore-7.0.5398.27328.zip	0				restricted	stable		http://forum.kerbalspaceprogram.com/threads/24551-Firespitter-propeller-plane-and-helicopter-parts				",
                                            "HotRockets	7.25	HotRockets! Particle FX Replacement	Replacement for stock engine particle FX		Nazari1382	package	http://addons.cursecdn.com/files/2216%5C167/HotRockets_7.25_CorePack.zip	0	0.25			CC-BY-NC	stable		http://forum.kerbalspaceprogram.com/threads/65754				",
                                            "Karbonite	0.4.4	Karbonite	A newly discovered mineral that has hydrocarbon-like properties and is perfect for processing into a fuel. It's easily mined from the surface, and can be found in liquid or gaseous forms on certain planets.		RoverDude	package	https://github.com/BobPalmer/Karbonite/releases/download/0.4.4/Karbonite_0.4.4.zip	39015866	0.25			CC-BY-NC-SA	stable	https://github.com/BobPalmer/Karbonite	http://forum.kerbalspaceprogram.com/threads/87335				",
                                            "KerbalAlarmClock	v3.0.5.0	Kerbal Alarm Clock	Create reminder alarms to help manage flights and not warp past important times		TriggerAu	package	https://github.com/TriggerAu/KerbalAlarmClock/releases/download/v3.0.5.0/KerbalAlarmClock_3.0.5.0.zip	406598	0.25			MIT	stable	https://github.com/TriggerAu/KerbalAlarmClock	http://forum.kerbalspaceprogram.com/threads/24786-0-25-0-Kerbal-Alarm-Clock-v3-0-1-0-%28Oct-26%29				",
                                            "KAS	0.4.9	Kerbal Attachement System	KAS introduce new gameplay mechanics by adding winches, containers, dynamic struts/pipes and grabable & attachable parts.		KospY;Winn75;zzz;Majiir;a.g.	package	http://kerbal.curseforge.com/ksp-mods/223900-kerbal-attachment-system-kas/files/2216138/download	0	0.25			restricted	stable						",
                                            "KerbalConstructionTime	1.0.3	Kerbal Construction Time	Unrapid Planned Assembly		magico13	package	https://kerbalstuff.com/mod/125/Kerbal%20Construction%20Time/download/1.0.3	574652	0.25			GPL-3.0	stable		http://forum.kerbalspaceprogram.com/threads/92377-0-24-2-Kerbal-Construction-Time-Release-v1-0-0-%288-29-14%29				",
                                            "KerbalJointReinforcement	v2.4.4	Kerbal Joint Reinforcement	KJR tightens up the joints between parts and adds some physics-adjusting parameters to make vehicles more stable when loading on the launchpad or coming out of timewarp.		ferram4	package	https://kerbalstuff.com/mod/53/Kerbal%20Joint%20Reinforcement/download/v2.4.4	36034	0.25			GPL-3.0	stable	https://github.com/ferram4/Kerbal-Joint-Reinforcement/	http://forum.kerbalspaceprogram.com/threads/55657				",
                                            "kOS	0.15.3.0	kOS: Scriptable Autopilot System	kOS is a scriptable autopilot Mod for Kerbal Space Program. It allows you write small programs that automate specific tasks.		erendrake	package	https://kerbalstuff.com/mod/86/kOS:%20Scriptable%20Autopilot%20System/download/0.15.3.0	2526552	0.25			GPL-3.0	stable		http://ksp-kos.github.io/KOS_DOC/				",
                                            "KronalVesselViewer	0.0.4_0.25	Kronal Vessel Viewer (KVV) - Exploded (Orthographic) ship view	Plugin that creates blue-print-like (Orthographic) images (into your Screenshots/ directory) from the KSP Editor (VAB/SPH)		bigorangemachine	package	https://kerbalstuff.com/mod/211/Kronal%20Vessel%20Viewer%20%28KVV%29%20-%20Exploded%20%28Orthographic%29%20ship%20view/download/0.0.4_0.25	1261756	0.25			unrestricted	stable		https://github.com/bigorangemachine/ksp-kronalutils/releases				",
                                            "InfernalRobotics	0.19.2	Magic Smoke Industries Infernal Robotics	Making stuff move.		sirkut	package	https://kerbalstuff.com/mod/8/Magic%20Smoke%20Industries%20Infernal%20Robotics/download/0.19.2	11664244	0.25			GPL-3.0	stable	https://github.com/sirkut/InfernalRobotics	http://forum.kerbalspaceprogram.com/threads/37707-0-23-5-Magic-Smoke-Industries-Infernal-Robotics-0-16-4				",
                                            "MechJeb2	2.4.0	MechJeb 2	Anatid Robotics and Multiversal Mechatronics proudly presents the first flight assistant autopilot: MechJeb		sarbian	package	http://addons.curse.cursecdn.com/files/2216/245/MechJeb2-2.4.0.0.zip	0	0.25			GPL-3.0	stable		http://forum.kerbalspaceprogram.com/threads/12384-PART-0-25-Anatid-Robotics-MuMech-MechJeb-Autopilot-v2-4-0				",
                                            "ModuleManager	2.5.1	Module Manager	Modify KSP configs without conflict		ialdabaoth;Sarbian	package	https://ksp.sarbian.com/jenkins/job/ModuleManager/lastSuccessfulBuild/artifact/ModuleManager-2.5.1.zip	0	0.25			CC-BY-SA	stable						",
                                            "ModuleRCSFX	v3.3	ModuleRCSFX	A fixed version of the stock RCS module		ialdabaoth;NathanKell	package	https://github.com/NathanKell/ModuleRCSFX/releases/download/v3.3/ModuleRCSFX_v3.3.zip	5598	0.25			CC-BY-SA	stable	https://github.com/NathanKell/ModuleRCSFX	http://forum.kerbalspaceprogram.com/threads/92290				",
                                            "NathanKell-RVE-Haxx	0.0.2014.11.25	NathanKell's RVE haxx	Nathan's files h4xx0red by pjf for the CKAN. This may eat your system		unknown	package	https://nabaal.net/files/ksp/nathankell/RealSolarSystem/Textures/MyRVE.zip	0	0.25.0			unknown	development						",
                                            "ORSX	0.1.3	Open Resource System Fork	An ORS Fork		RoverDude	package	https://github.com/BobPalmer/ORSX/releases/download/0.1.3/ORSX_0.1.3.zip	64566	0.25			BSD-3-clause	stable	https://github.com/BobPalmer/ORSX					",
                                            "PartCatalog	3.0_RC8	Part Catalog	The PartCatalog allows you categorize your parts into Tags in order to make your building experience less frustrating by reducing the time you're looking for that one part.		BlackNecro	package	https://dl.dropboxusercontent.com/u/11467249/PartCatalog/PartCatalog3.0_RC8.zip	0	0.25			CC-BY-SA-3.0	stable		http://forum.kerbalspaceprogram.com/threads/35018-0-25-PartCatalog-3-0-RC8-%282014-10-08%29				",
                                            "PlanetShine	0.2.2	PlanetShine	Planets and moons reflects their light to your vessel + other ambient light improvements		Valerian	package	https://kerbalstuff.com/mod/266/PlanetShine/download/0.2.2	41869	0.25			Apache-2.0	stable		http://forum.kerbalspaceprogram.com/threads/96497				",
                                            "PreciseNode	1.1.1	Precise Node	Provides a more precise widget for maneuver node editing			package	http://blizzy.de/precise-node/PreciseNode-1.1.1.zip	0	0.25			BSD-2-clause	stable		http://forum.kerbalspaceprogram.com/threads/47863-0-25-0-Precise-Node-1-1-1-Precisely-edit-your-maneuver-nodes				",
                                            "ProceduralFairings	v3.10	Procedural Fairings	Fairings that automatically reshape for any attached payload		e-dog	package	https://github.com/e-dog/ProceduralFairings/releases/download/v3.10/ProcFairings_3.10.zip	1776825	0.25			CC-BY-3.0	stable	https://github.com/e-dog/ProceduralFairings					",
                                            "ProceduralParts	v0.9.20	Procedural Parts	ProceduralParts allows you to procedurally generate a number of different parts in a range of sizes and shapes.		NathanKell	package	https://github.com/Swamp-Ig/ProceduralParts/releases/download/v0.9.20/ProceduralParts-0.9.20.zip	7403194	0.25			CC-BY-3.0	stable	https://github.com/Swamp-Ig/ProceduralParts					",
                                            "ProceduralDynamics	0.9.1	Procedural Wings	Procedural Wings, makes wings more procedural.		DYJ	package	https://kerbalstuff.com/mod/64/Procedural%20Wings/download/0.9.1	1236752	0.25			restricted	stable		http://forum.kerbalspaceprogram.com/threads/29862-0-24-Procedural-Dynamics-Procedural-Wing-0-8				",
                                            "RealFuels	rf-v8.1-really-v8.2-pre	Real Fuels	Real fuels and tanks for KSP		ialdabaoth;NathanKell	package	https://github.com/NathanKell/ModularFuelSystem/releases/download/rf-v8.2pre/RealFuels_v8.2.zip	0	0.25			CC-BY-SA	stable	https://github.com/NathanKell/ModularFuelSystem	http://forum.kerbalspaceprogram.com/threads/64118				",
                                            "RealSolarSystem	v8.2.1-actually-v8.3preview	Real Solar System	Resizes and rearranges the Kerbal system to more closely resemble the Solar System		NathanKell	package	https://github.com/NathanKell/RealSolarSystem/releases/download/v8.3preview/RealSolarSystem_v8.3.zip	0	0.25.0			CC-BY-NC-SA	stable	https://github.com/NathanKell/RealSolarSystem	http://forum.kerbalspaceprogram.com/threads/55145				",
                                            "RSSTextures4096	1.0	Real Solar System Textures - 4096 x 2048	Textures for Real Solar Systems			package	https://nabaal.net/files/ksp/nathankell/RealSolarSystem/Textures/4096.zip	0				CC-BY-NC-SA	stable						",
                                            "RealChute	1.2.6	RealChute Parachute Systems	RealChute is a complete rework of the stock parachute module to fix a few of it's inconveniences and get more realistic results out of parachutes!		stupid_chris	package	https://kerbalstuff.com/mod/71/RealChute%20Parachute%20Systems/download/1.2.6	2672129		0.25.0	0.25.0	restricted	stable	https://github.com/StupidChris/RealChute	http://forum.kerbalspaceprogram.com/threads/57988				",
                                            "RealismOverhaul	v7.0.2	Realism Overhaul	Multipatch to KSP to give things realistic stats and sizes		Felger	package	https://github.com/KSP-RO/RealismOverhaul/releases/download/v7.0.2/RealismOverhaul7_0_2.zip	1395210	0.25.0			CC-BY-SA	stable	https://github.com/KSP-RO/RealismOverhaul	http://forum.kerbalspaceprogram.com/threads/99966				",
                                            "RP-0	v0.13	Realistic Progression Zero	Realistic Progression Zero - The lightweight Realism Overhaul tech tree		RP-0 Group	package	https://github.com/KSP-RO/RP-0/releases/download/v0.13/RP-0-v0.13.zip	40798				CC-BY-4.0	development	https://github.com/KSP-RO/RP-0					",
                                            "RemoteTech	v1.5.1	RemoteTech	Adds new rules for communicating with unmanned probes.	RemoteTech allows you to construct vast relay networks of communication satellites and remotely controlled unmanned vehicles. Your unmanned vessels require an uplink to a command station to be controlled. This adds a new layer of difficulty that compensates for the lack of live crew members.	Remote Technologies Group	package	https://kerbalstuff.com/mod/134/RemoteTech/download/v1.5.1	5418104	0.25			restricted	stable	https://github.com/RemoteTechnologiesGroup/RemoteTech	http://forum.kerbalspaceprogram.com/threads/83305	https://github.com/RemoteTechnologiesGroup/RemoteTech/issues			",
                                            "RemoteTech-Config-RSS	0.0	RemoteTech RSS Configuration	Adds ground stations to launch sites in Real Solar System		CerberusRCAF	package	https://www.dropbox.com/s/ohqv9r9mwng2500/RemoteTech_RSS_Settings.zip?dl=1	0	0.25			CC-BY-4.0	stable		http://forum.kerbalspaceprogram.com/threads/99966-0-25-0-Realism-Overhaul-6-1-2c				",
                                            "SCANsat	v8.0	SCANsat	SCANsat: Real Scanning, Real Science, Warp Speed!		DMagic	package	https://kerbalstuff.com/mod/249/SCANsat/download/v8.0	1961163	0.25			restricted	stable		http://forum.kerbalspaceprogram.com/threads/80369				",
                                            "ScienceAlert	1.8rc1	Science Alert	Is it time for science? Get alerts when it is!		xEvilReeperx	package	https://bitbucket.org/xEvilReeperx/ksp_sciencealert/downloads/KSP-25.0-ScienceAlert-1.8rc1.zip	0	0.25			GPL-3.0	testing		http://forum.kerbalspaceprogram.com/threads/76793				",
                                            "ShipManifest	0.25.0_3.3.2b	Ship Manifest	Ship Manifest is a tool to move your ship's \"things\".  Ship Manifest moves crew, Science and Resources around from part to part within your ship or station.		Papa_Joe	package	https://kerbalstuff.com/mod/261/Ship%20Manifest/download/0.25.0_3.3.2b	546564	0.25			CC-BY-NC-SA-4.0	stable	https://github.com/PapaJoesSoup/ShipManifest	http://forum.kerbalspaceprogram.com/threads/62270				",
                                            "StageRecovery	1.5.1	StageRecovery	Recover Funds+ from Dropped Stages		magico13	package	https://kerbalstuff.com/mod/97/StageRecovery/download/1.5.1	40331	0.25			GPL-3.0	stable		http://forum.kerbalspaceprogram.com/threads/86677-0-24-2-StageRecovery-Recover-Funds-from-Dropped-Stages-v1-3-%288-5-14%29				",
                                            "SXT	18.6	SXT - Stock eXTension	A collection of (vaguely) stockalike parts that I find useful or just wanted to have.		Lack	package	https://kerbalstuff.com/mod/338/SXT%20-%20Stock%20eXTension/download/18.6	10463727	0.25			CC-BY-NC-SA-4.0	stable		http://forum.kerbalspaceprogram.com/threads/24906				",
                                            "TACLS	v0.10.1	TAC Life Support (TACLS)	Adds life support requirements and resources to all kerbals		taraniselsu	package	https://github.com/taraniselsu/TacLifeSupport/releases/download/v0.10.1/TacLifeSupport_0.10.1.13.zip	5997448	0.25			CC-BY-NC-SA-3.0	stable	https://github.com/taraniselsu/TacLifeSupport					",
                                            "TACLS-Config-RealismOverhaul	v7.0.2	TAC Life Support (TACLS) - Realism Overhaul Config	Realism Overhaul config for TACLS		Felger	package	https://github.com/KSP-RO/RealismOverhaul/releases/download/v7.0.2/RealismOverhaul7_0_2.zip	1395210				CC-BY-SA	stable	https://github.com/KSP-RO/RealismOverhaul					",
                                            "TechManager	1.4	TechManager	This mod will allow you to change the tech tree, including the creation of new tech nodes. 		anonish	package	https://kerbalstuff.com/mod/297/TechManager/download/1.4	39592	0.25			MIT	stable		http://forum.kerbalspaceprogram.com/threads/98293-0-25-TechManager-Version-1-0				",
                                            "Toolbar	1.7.7	Toolbar	API for third-party plugins to provide toolbar buttons			package	http://blizzy.de/toolbar/Toolbar-1.7.7.zip	0	0.25			BSD-2-clause	stable		http://forum.kerbalspaceprogram.com/threads/60863				",
                                            "TweakScale	v1.44	TweakScale	Rescale everything!		Biotronic	package	https://github.com/Biotronic/TweakScale/releases/download/v1.44/TweakScale_1.44.zip	127391	0.25			CC-BY-NC-SA-4.0	stable	https://github.com/Biotronic/TweakScale	http://forum.kerbalspaceprogram.com/threads/80234				",
                                            "USITools	0.2.4	Umbra Space Industries Tools	Common libraries for USI mods		RoverDude	package	https://github.com/BobPalmer/UmbraSpaceIndustries/releases/download/0.2.4/USITools_0.2.4.zip	46094	0.25			CC-BY-NC-SA-4.0	stable	https://github.com/BobPalmer/UmbraSpaceIndustries					",
                                            "UniversalStorage	0.9.4	Universal Storage	Modular resource and processor parts to build custom service modules.		Paul_Kingtiger	package	https://kerbalstuff.com/mod/250/Universal%20Storage/download/0.9.4	6025862	0.25			restricted	stable						",
                                            "UniversalStorage-KAS	0.9.0.14	Universal Storage KAS Pack	Universal storage - Kerbal Attachment System extensions		Paul Kingtiger	package	http://www.kingtiger.co.uk/kingtiger/wordpress/wp-content/uploads/2014/11/US_KAS_0.9.0.14.zip	0	0.25			restricted	stable		http://forum.kerbalspaceprogram.com/threads/75129				",
                                            "UniversalStorage-TAC	0.9.2.7	Universal Storage TAC Pack	Universal storage - TACLS extensions		Paul Kingtiger	package	http://www.kingtiger.co.uk/kingtiger/wordpress/wp-content/uploads/2014/11/US_TAC_0.9.2.7.zip	0	0.25			restricted	stable		http://forum.kerbalspaceprogram.com/threads/75129				",
                                            "UKS	0.21.3	USI Kolonization Systems (MKS/OKS)	a series of interlocking modules for building long-term, self sustaining colonies in orbit and on other planets and moons.	MKS introduces a series of parts specifically designed to provide self-sustaining life support for multiple Kerbals for users of TAC-Life Support. While the parts will function without TAC-LS, the design is centered around providing an appropriate challenge where players will weigh the costs, risks, and rewards of supplying missions exclusively with carried life support supplies, or in making the investment in a permanent colony. That being said, this is not just a single greenhouse part (although food production is part of the mod). Rather, it brings an entire colonization end-game to KSP in a fun and challenging way.	BobPalmer	package	https://github.com/BobPalmer/MKS/releases/download/0.21.3/MKS_0.21.3.zip	38513664	0.25			CC-BY-NC-SA-4.0	stable	https://github.com/BobPalmer/MKS	http://forum.kerbalspaceprogram.com/threads/79588-0-25-USI-Kolonization-Systems-%28MKS-OKS%29-%280-21-2%29-2014-10-07				"
                                          },
                                          ReadAllLines(output).ToArray());
            }
        }

        private static IEnumerable<string> ReadAllLines(Stream s)
        {
            s.Position = 0;
            var reader = new StreamReader(s);
            while (reader.ReadLine() is string line)
            {
                yield return line;
            }
        }
    }
}
