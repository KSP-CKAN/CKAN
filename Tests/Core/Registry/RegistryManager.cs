using System.Linq;
using System.IO;

using NUnit.Framework;

using CKAN;
using CKAN.Versioning;
using CKAN.Games.KerbalSpaceProgram;

using Tests.Data;

namespace Tests.Core.Registry
{
    [TestFixture]
    public class RegistryManagerTests
    {
        // Test lockfiles, see #1265
        [Test]
        public void Locking()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var ksp = new DisposableKSP())
            {
                // TODO: Give the RegistryManager a way to read-only query
                // the registry location.
                var manager = RegistryManager.Instance(ksp.KSP, repoData.Manager);

                Assert.IsNotNull(manager);

                var registryPath = manager.lockfilePath;

                // Let's try opening the same registry file a second time...

                Assert.IsTrue(TestLock(registryPath));

                // And after we dispose the registry manager, the lock should be gone...
                manager.Dispose();

                Assert.IsFalse(TestLock(registryPath));
            }
        }

        /// <summary>
        /// Tests if <c>path</c> is locked. Returns true if it is.
        /// </summary>
        private bool TestLock(string path)
        {
            FileStream lockfileStream = null;

            try
            {
                lockfileStream = new FileStream(
                    path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 512, FileOptions.DeleteOnClose);

                // Uh oh, the file *wasn't* locked.
                return false;
            }
            catch (IOException)
            {
                // It *was* locked.
                return true;
            }
            finally
            {
                lockfileStream?.Close();
            }
        }

        [Test]
        public void DisposeInstance_AfterCreating_Disposes()
        {
            // Arrange
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var gameInst = new DisposableKSP())
            {
                var jsonPath = Path.Combine(gameInst.KSP.CkanDir(), "registry.json");
                var lockPath = Path.Combine(gameInst.KSP.CkanDir(), "registry.locked");

                // Act / Assert
                RegistryManager.Instance(gameInst.KSP, repoData.Manager);
                Assert.IsTrue(File.Exists(jsonPath));
                Assert.IsTrue(File.Exists(lockPath));
                RegistryManager.DisposeInstance(gameInst.KSP);

                // Assert
                Assert.IsTrue(File.Exists(jsonPath));
                Assert.IsFalse(File.Exists(lockPath));
            }
        }

        [Test]
        public void DisposeInstance_BeforeCreating_DoesNotCreate()
        {
            // Arrange
            using (var gameInst = new DisposableKSP())
            {
                // Act
                RegistryManager.DisposeInstance(gameInst.KSP);

                // Assert
                Assert.IsFalse(File.Exists(Path.Combine(gameInst.KSP.CkanDir(), "registry.json")));
                Assert.IsFalse(File.Exists(Path.Combine(gameInst.KSP.CkanDir(), "registry.locked")));
            }
        }

        [Test]
        public void Registry_ZeroByteRegistryJson_EmptyRegistryWithoutCrash()
        {
            // Arrange
            string registryPath = TestData.DataDir("zero-byte-registry.json");

            // Act
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var dispksp = new DisposableKSP(registryPath))
            {
                // Assert
                var reg = RegistryManager.Instance(dispksp.KSP, repoData.Manager).registry;
                Assert.IsNotNull(reg);
                // These lists should all be empty, copied from Registry.Empty()
                Assert.IsFalse(reg.InstalledModules.Any());
                Assert.IsFalse(reg.InstalledDlls.Any());
                Assert.IsFalse(reg.HasAnyAvailable());
                // installed_files isn't exposed for testing
                // A default repo is set during load
                CollectionAssert.AreEqual(
                    new Repository[]
                    {
                        new Repository("default", new KerbalSpaceProgram().DefaultRepositoryURL)
                    },
                    reg.Repositories.Values);
            }
        }

        [Test]
        public void ScanUnmanagedFiles_WithUnregisteredDLLs_FindsThem()
        {
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var dispInst = new DisposableKSP())
            {
                var gameInst = dispInst.KSP;
                var regMgr = RegistryManager.Instance(gameInst, repoData.Manager);
                var path = Path.Combine(gameInst.game.PrimaryModDirectory(gameInst), "Example.dll");
                var registry = regMgr.registry;

                Assert.IsFalse(registry.IsInstalled("Example"), "Example should start uninstalled");

                File.WriteAllText(path, "Not really a DLL, are we?");

                regMgr.ScanUnmanagedFiles();

                Assert.IsTrue(registry.IsInstalled("Example"), "Example installed");

                var version = registry.InstalledVersion("Example");
                Assert.IsInstanceOf<UnmanagedModuleVersion>(version, "DLL detected as a DLL, not full mod");

                // Now let's do the same with different case.

                var path2 = Path.Combine(gameInst.game.PrimaryModDirectory(gameInst), "NewMod.DLL");

                Assert.IsFalse(registry.IsInstalled("NewMod"));
                File.WriteAllText(path2, "This text is irrelevant. You will be assimilated");

                regMgr.ScanUnmanagedFiles();

                Assert.IsTrue(registry.IsInstalled("NewMod"));
            }
        }

        [Test,
            // No files
            TestCase(new string[] { },
                     new string[] { }),

            // Only unregistered files
            TestCase(new string[] { },
                     new string[] { "GameData/test3.dll", "GameData/test4.dll" }),

            // Only registered files
            TestCase(new string[] { "GameData/test1.dll", "GameData/test2.dll" },
                     new string[] { }),

            // Some registered, some unregistered
            TestCase(new string[] { "GameData/test1.dll", "GameData/test2.dll" },
                     new string[] { "GameData/test3.dll", "GameData/test4.dll" })
        ]
        public void ScanUnmanagedFiles_WithDLLs_FindsUnregisteredOnly(string[] registered,
                                                                      string[] unregistered)
        {
            // Arrange
            var user = new NullUser();
            using (var repoData = new TemporaryRepositoryData(user))
            using (var dispInst = new DisposableKSP())
            {
                var gameInst = dispInst.KSP;
                var regMgr   = RegistryManager.Instance(gameInst, repoData.Manager);
                var registry = regMgr.registry;
                var absReg   = registered.Select(p => gameInst.ToAbsoluteGameDir(p))
                                                .ToList();
                var absUnreg = unregistered.Select(p => gameInst.ToAbsoluteGameDir(p))
                                                  .ToArray();

                // Create all the files
                foreach (var filename in absReg.Concat(absUnreg))
                {
                    File.WriteAllText(filename, "Not really a DLL, are we?");
                }

                // Mark registered files as belonging to a module
                registry.RegisterModule(CkanModule.FromJson(@"{
                                            ""spec_version"": ""v1.4"",
                                            ""identifier"":   ""InstalledMod"",
                                            ""version"":      ""1.0"",
                                            ""download"":     ""https://github.com/""
                                        }"),
                                        absReg, gameInst, false);

                // Act
                regMgr.ScanUnmanagedFiles();

                // Assert
                CollectionAssert.AreEquivalent(
                    unregistered,
                    registry.InstalledDlls.Select(ident => registry.DllPath(ident))
                                          .ToArray());
            }
        }
    }
}
