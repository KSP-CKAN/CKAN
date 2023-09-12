using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using NUnit.Framework;

using CKAN;

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
            using (var ksp = new DisposableKSP())
            {
                // TODO: Give the RegistryManager a way to read-only query
                // the registry location.
                var manager = RegistryManager.Instance(ksp.KSP);

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
                if (lockfileStream != null)
                {
                    lockfileStream.Close();
                }
            }
        }

        [Test]
        public void DisposeInstance_AfterCreating_Disposes()
        {
            // Arrange
            using (var gameInst = new DisposableKSP())
            {
                var jsonPath = Path.Combine(gameInst.KSP.CkanDir(), "registry.json");
                var lockPath = Path.Combine(gameInst.KSP.CkanDir(), "registry.locked");

                // Act / Assert
                RegistryManager.Instance(gameInst.KSP);
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
            DisposableKSP dispksp;
            CKAN.GameInstance      ksp;

            // Act
            dispksp = new DisposableKSP(null, registryPath);
            ksp     = dispksp.KSP;

            // Assert
            var reg = RegistryManager.Instance(ksp).registry;
            Assert.IsNotNull(reg);
            // These lists should all be empty, copied from CKAN.Registry.Empty()
            Assert.IsFalse(reg.InstalledModules.Any());
            Assert.IsFalse(reg.InstalledDlls.Any());
            Assert.IsFalse(reg.HasAnyAvailable());
            // installed_files isn't exposed for testing
            // A default repo is set during load
            Assert.IsTrue(reg.Repositories.Any());

            dispksp.Dispose();
        }

    }
}
