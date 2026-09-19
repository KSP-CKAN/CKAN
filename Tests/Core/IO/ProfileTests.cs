using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;
using CKAN.IO;

using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.Core.IO
{
    [TestFixture]
    public class ProfileTests
    {
        private DisposableKSP instance = null!;
        private TemporaryRepository repository = null!;
        private TemporaryRepositoryData data = null!;
        private FakeConfiguration config = null!;
        private GameInstanceManager instances = null!;
        private RegistryManager manager = null!;
        private ModuleInstaller installer = null!;
        private Mock<IDownloader> downloader = null!;
        private readonly IUser user = new NullUser();

        [SetUp]
        public void SetUp()
        {
            instance = new DisposableKSP();
            repository = new TemporaryRepository(Meta("RootA", "Shared", "RootB"),
                                                 Meta("RootB", "Shared", "RootA"),
                                                 Meta("RootC", "NewDependency"),
                                                 Meta("NewDependency", "Shared"),
                                                 Meta("Shared", "Leaf"),
                                                 Meta("Shared", "Leaf", version: "2"),
                                                 Meta("Leaf"),
                                                 TestData.DogeCoinFlag_101());
            // Use an explicit file URI so Json.NET preserves it through rollback.
            repository.repo.uri = new Uri(repository.repo.uri.AbsoluteUri);
            data = new TemporaryRepositoryData(user, repository.repo);
            config = new FakeConfiguration(instance.KSP, instance.KSP.Name);
            instances = new GameInstanceManager(user, config);
            instances.SetCurrentInstance(instance.KSP);
            manager = RegistryManager.Instance(instance.KSP, data.Manager, new[] { repository.repo });
            installer = new ModuleInstaller(instance.KSP, instances.Cache!, config, user);
            downloader = new Mock<IDownloader>();
        }

        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
            instances.Dispose();
            config.Dispose();
            data.Dispose();
            repository.Dispose();
            instance.Dispose();
        }

        [Test]
        public void OrdinaryEditsCaptureRootsAndRoundTrip()
        {
            Assert.IsEmpty(manager.registry.ProfileNames);
            Assert.IsNull(manager.registry.ActiveProfile);
            Assert.IsNull(JObject.Parse(JsonConvert.SerializeObject(manager.registry))["profiles"]);
            manager.registry.CreateProfile("Default");
            Install("RootA");
            CollectionAssert.AreEqual(new[] { "RootA" }, manager.registry.ProfileRoots("Default"));
            manager.registry.InstalledModule("Shared")!.AutoInstalled = false;
            manager.Save();
            var loaded = CKAN.Registry.FromJson(instance.KSP, data.Manager, JsonConvert.SerializeObject(manager.registry));
            CollectionAssert.AreEqual(new[] { "RootA", "Shared" }, loaded.ProfileRoots("Default"));
            Assert.AreEqual("Default", loaded.ActiveProfile);
            HashSet<string>? dirs = null;
            installer.UninstallList(new[] { "RootA" }, ref dirs, manager, false);
            CollectionAssert.AreEqual(new[] { "Shared" }, manager.registry.ProfileRoots("Default"));
        }

        [Test]
        public void SwitchBothDirectionsRetainsSharedDependencies()
        {
            MakeProfiles();
            installer.SwitchProfile("A", manager, downloader.Object);
            var shared = manager.registry.InstalledModule("Shared");
            var leaf = manager.registry.InstalledModule("Leaf");
            var plan = installer.PlanProfile("B", manager.registry);
            CollectionAssert.AreEquivalent(new[] { "RootA" }, plan.Remove.Select(m => m.identifier));
            CollectionAssert.AreEquivalent(new[] { "RootB" }, plan.Install.Select(m => m.identifier));
            installer.SwitchProfile("B", manager, downloader.Object);
            Assert.AreSame(shared, manager.registry.InstalledModule("Shared"));
            Assert.AreSame(leaf, manager.registry.InstalledModule("Leaf"));
            Assert.IsNull(manager.registry.InstalledModule("RootA"));
            Assert.IsFalse(manager.registry.InstalledModule("RootB")!.AutoInstalled);
            Assert.IsTrue(manager.registry.InstalledModule("Shared")!.AutoInstalled);
            installer.SwitchProfile("A", manager, downloader.Object);
            Assert.AreSame(shared, manager.registry.InstalledModule("Shared"));
            Assert.AreEqual("A", manager.registry.ActiveProfile);
            installer.SwitchProfile("Empty", manager, downloader.Object);
            Assert.IsEmpty(manager.registry.InstalledModules);
        }

        [Test]
        public void NewlyInstalledDependencyRetainsOlderSharedVersion()
        {
            var shared = manager.registry.AvailableByIdentifier("Shared")
                                         .First(m => m.version.ToString() == "1");
            HashSet<string>? dirs = null;
            installer.InstallList(new[] { shared },
                                  RelationshipResolverOptions.DependsOnlyOpts(instance.KSP.StabilityToleranceConfig),
                                  manager, ref dirs, downloader: downloader.Object,
                                  autoInstalled: new[] { shared,
                                      manager.registry.LatestAvailable("Leaf", instance.KSP.StabilityToleranceConfig, null)! }.ToHashSet(),
                                  ConfirmPrompt: false);
            manager.registry.CreateProfile("Old");
            Install("RootA");
            manager.registry.CreateProfile("New");
            var json = JObject.Parse(JsonConvert.SerializeObject(manager.registry));
            json["profiles"]!["New"] = new JArray("RootC");
            manager.registry = CKAN.Registry.FromJson(instance.KSP, data.Manager, json.ToString());
            var oldShared = manager.registry.InstalledModule("Shared");
            var plan = installer.PlanProfile("New", manager.registry);
            Assert.IsFalse(plan.Remove.Any(m => m.identifier == "Shared"));
            Assert.IsFalse(plan.Install.Any(m => m.identifier == "Shared"));
            installer.SwitchProfile("New", manager, downloader.Object);
            Assert.AreSame(oldShared, manager.registry.InstalledModule("Shared"));
            Assert.AreEqual("1", manager.registry.InstalledModule("Shared")!.Module.version.ToString());
        }

        [Test]
        public void CurrentProfileDoesNotDownloadOrSave()
        {
            manager.registry.CreateProfile("Default");
            manager.Save();
            var before = File.ReadAllText(Path.Combine(instance.KSP.CkanDir, "registry.json"));
            installer.SwitchProfile("Default", manager, downloader.Object);
            downloader.Verify(d => d.DownloadModules(It.IsAny<IEnumerable<CkanModule>>()), Times.Never);
            Assert.AreEqual(before, File.ReadAllText(Path.Combine(instance.KSP.CkanDir, "registry.json")));
        }

        [Test]
        public void FirstProfileCreationAndSwitchRollBack()
        {
            var originalRepos = manager.registry.Repositories.Values.Select(r => r.uri).ToArray();
            using (CkanTransaction.CreateTransactionScope())
            {
                manager.registry.CreateProfile("Temporary");
                manager.Save();
            }
            Assert.IsEmpty(manager.registry.ProfileNames);
            Assert.IsNull(manager.registry.ActiveProfile);
            CollectionAssert.AreEqual(originalRepos, manager.registry.Repositories.Values.Select(r => r.uri));
            MakeProfiles();
            var before = JsonConvert.SerializeObject(manager.registry);
            var disk = File.ReadAllText(Path.Combine(instance.KSP.CkanDir, "registry.json"));
            using (CkanTransaction.CreateTransactionScope())
            {
                installer.SwitchProfile("A", manager, downloader.Object);
                Assert.AreEqual("A", manager.registry.ActiveProfile);
            }
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(before),
                                            JToken.Parse(JsonConvert.SerializeObject(manager.registry))));
            Assert.AreEqual(disk, File.ReadAllText(Path.Combine(instance.KSP.CkanDir, "registry.json")));
        }

        [Test]
        public void DownloadFailureAndCancellationLeaveProfileUnchanged()
        {
            MakeProfiles();
            var before = JsonConvert.SerializeObject(manager.registry);
            downloader.Setup(d => d.DownloadModules(It.IsAny<IEnumerable<CkanModule>>()))
                      .Throws(new IOException("Download failed"));
            Assert.Throws<IOException>(() => installer.SwitchProfile("A", manager, downloader.Object));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(before),
                                            JToken.Parse(JsonConvert.SerializeObject(manager.registry))));
            using (var cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                var canceled = new ModuleInstaller(instance.KSP, instances.Cache!, config, user, cancellation.Token);
                Assert.Throws<CancelledActionKraken>(() => canceled.SwitchProfile("A", manager, new Mock<IDownloader>().Object));
                Assert.AreEqual(before, JsonConvert.SerializeObject(manager.registry));
            }
        }

        [Test]
        public void UnmanagedFilesSurviveSwitchAndCollisionAbortsBeforeRemoval()
        {
            manager.registry.CreateProfile("Empty");
            manager.registry.CreateProfile("Doge");
            installer.SwitchProfile("Doge", manager, downloader.Object);
            instances.Cache!.Store(TestData.DogeCoinFlag_101_module(), TestData.DogeCoinFlagZip(), null);
            Install("DogeCoinFlag");
            var modFile = Path.Combine(instance.KSP.GameDir, "GameData/DogeCoinFlag/Flags/dogecoin.png");
            var external = Path.Combine(instance.KSP.GameDir, "GameData/DogeCoinFlag/PluginData/personal.cfg");
            Directory.CreateDirectory(Path.GetDirectoryName(external)!);
            File.WriteAllText(external, "keep me");
            installer.SwitchProfile("Empty", manager, downloader.Object);
            Assert.AreEqual("keep me", File.ReadAllText(external));
            FileAssert.DoesNotExist(modFile);
            Install("RootA");
            Directory.CreateDirectory(Path.GetDirectoryName(modFile)!);
            File.WriteAllText(modFile, "unmanaged collision");
            var before = JsonConvert.SerializeObject(manager.registry);
            Assert.Throws<FileExistsKraken>(() => installer.SwitchProfile("Doge", manager, downloader.Object));
            Assert.AreEqual(before, JsonConvert.SerializeObject(manager.registry));
            Assert.AreEqual("unmanaged collision", File.ReadAllText(modFile));
            Assert.IsNotNull(manager.registry.InstalledModule("RootA"));
            File.Delete(modFile);
            installer.SwitchProfile("Doge", manager, downloader.Object);
            FileAssert.Exists(modFile);
            Assert.AreEqual("keep me", File.ReadAllText(external));
        }

        [Test]
        public void InvalidTargetFailsWithoutChanges()
        {
            MakeProfiles();
            var json = JObject.Parse(JsonConvert.SerializeObject(manager.registry));
            json["profiles"]!["Invalid"] = new JArray("RootA", "RootB");
            json["profiles"]!["Missing"] = new JArray("UnavailableCustomMod");
            manager.registry = CKAN.Registry.FromJson(instance.KSP, data.Manager, json.ToString());
            var before = JsonConvert.SerializeObject(manager.registry);
            Assert.Catch<Kraken>(() => installer.SwitchProfile("Invalid", manager, downloader.Object));
            Assert.Throws<ModuleNotFoundKraken>(() => installer.SwitchProfile("Missing", manager, downloader.Object));
            Assert.AreEqual(before, JsonConvert.SerializeObject(manager.registry));
        }

        [Test]
        public void NamesAndCopiedRegistryAreIndependent()
        {
            manager.registry.CreateProfile("Default");
            Assert.Throws<ArgumentException>(() => manager.registry.CreateProfile("default"));
            Assert.Throws<ArgumentException>(() => manager.registry.CreateProfile(" "));
            Assert.Throws<ArgumentException>(() => manager.registry.DeleteProfile("Default"));
            manager.registry.CreateProfile("Other");
            manager.registry.RenameProfile("Default", "Renamed");
            var copy = CKAN.Registry.FromJson(instance.KSP, data.Manager, JsonConvert.SerializeObject(manager.registry));
            copy.DeleteProfile("Other");
            Assert.AreEqual("Renamed", copy.ActiveProfile);
            CollectionAssert.AreEquivalent(new[] { "Renamed", "Other" }, manager.registry.ProfileNames);
        }

        [Test]
        public void CloneHasIndependentProfilesOnDisk()
        {
            MakeProfiles();
            using (var directory = new TemporaryDirectory())
            {
                var clone = instances.CloneInstance(instance.KSP, "Clone", directory, Array.Empty<string>());
                using (var cloned = RegistryManager.Instance(clone, data.Manager))
                {
                    Assert.AreEqual("B", cloned.registry.ActiveProfile);
                    cloned.registry.RenameProfile("B", "Clone only");
                    cloned.Save();
                    Assert.AreEqual("B", manager.registry.ActiveProfile);
                    var original = JObject.Parse(File.ReadAllText(Path.Combine(instance.KSP.CkanDir, "registry.json")));
                    Assert.AreEqual("B", (string?)original["active_profile"]);
                }
            }
        }

        [Test]
        public void FailureAfterFileInstallationRestoresFilesAndProfile()
        {
            manager.registry.CreateProfile("Empty");
            manager.registry.CreateProfile("Doge");
            installer.SwitchProfile("Doge", manager, downloader.Object);
            instances.Cache!.Store(TestData.DogeCoinFlag_101_module(), TestData.DogeCoinFlagZip(), null);
            Install("DogeCoinFlag");
            installer.SwitchProfile("Empty", manager, downloader.Object);
            Install("RootA");
            var before = JsonConvert.SerializeObject(manager.registry);
            var disk = File.ReadAllText(Path.Combine(instance.KSP.CkanDir, "registry.json"));
            installer.OneComplete += mod => throw new IOException("Simulated failure after writing files");
            Assert.Throws<IOException>(() => installer.SwitchProfile("Doge", manager, downloader.Object));
            Assert.IsTrue(JToken.DeepEquals(JToken.Parse(before),
                                            JToken.Parse(JsonConvert.SerializeObject(manager.registry))));
            Assert.AreEqual(disk, File.ReadAllText(Path.Combine(instance.KSP.CkanDir, "registry.json")));
            FileAssert.DoesNotExist(Path.Combine(instance.KSP.GameDir, "GameData/DogeCoinFlag/Flags/dogecoin.png"));
            Assert.IsNotNull(manager.registry.InstalledModule("RootA"));
        }

        [Test]
        public void OlderRegistryReaderIgnoresProfilesAndPreservesInstalledState()
        {
            manager.registry.CreateProfile("Default");
            Install("RootA");
            // The pre-profile reader uses Json.NET's default unknown-member handling.
            var old = JsonConvert.DeserializeObject<OldRegistry>(JsonConvert.SerializeObject(manager.registry))!;
            Assert.AreEqual(3, old.installed_modules.Count);
            var rewritten = JsonConvert.SerializeObject(old);
            Assert.IsNull(JObject.Parse(rewritten)["profiles"]);
            CollectionAssert.AreEquivalent(JObject.Parse(JsonConvert.SerializeObject(manager.registry))["installed_modules"]!.Children<JProperty>().Select(p => p.Name),
                                          JObject.Parse(rewritten)["installed_modules"]!.Children<JProperty>().Select(p => p.Name));
        }

        private sealed class OldRegistry
        {
            public Dictionary<string, JObject> installed_modules = new Dictionary<string, JObject>();
        }

        private void MakeProfiles()
        {
            manager.registry.CreateProfile("Empty");
            manager.registry.CreateProfile("A");
            installer.SwitchProfile("A", manager, downloader.Object);
            Install("RootA");
            installer.SwitchProfile("Empty", manager, downloader.Object);
            manager.registry.CreateProfile("B");
            installer.SwitchProfile("B", manager, downloader.Object);
            Install("RootB");
        }

        private void Install(string id)
        {
            HashSet<string>? dirs = null;
            var mod = manager.registry.LatestAvailable(id, instance.KSP.StabilityToleranceConfig, instance.KSP.VersionCriteria())!;
            installer.InstallList(new[] { mod }, RelationshipResolverOptions.DependsOnlyOpts(instance.KSP.StabilityToleranceConfig),
                                  manager, ref dirs, downloader: downloader.Object,
                                  autoInstalled: new[] { "Shared", "Leaf" }.Select(dep => manager.registry.LatestAvailable(dep, instance.KSP.StabilityToleranceConfig, null)!).ToHashSet(),
                                  ConfirmPrompt: false);
        }

        private static string Meta(string id, string? dependency = null,
                                   string? conflict = null, string version = "1")
        {
            var mod = new JObject
            {
                ["spec_version"] = "v1.6", ["identifier"] = id, ["name"] = id,
                ["abstract"] = "Profile test", ["author"] = "CKAN", ["license"] = "MIT",
                ["version"] = version, ["kind"] = "metapackage",
            };
            if (dependency != null)
            {
                mod["depends"] = new JArray(new JObject { ["name"] = dependency });
            }
            if (conflict != null)
            {
                mod["conflicts"] = new JArray(new JObject { ["name"] = conflict });
            }
            return mod.ToString();
        }
    }
}
