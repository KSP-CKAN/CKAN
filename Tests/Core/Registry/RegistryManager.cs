using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Tests.Data;
using System.IO;

namespace Tests.Core.RegistryManager
{
    [TestFixture]
    public class RegistryManager
    {
        // Test lockfiles, see #1265
        [Test]
        public void Locking()
        {
            using (var ksp = new DisposableKSP())
            {
                // TODO: Give the RegistryManager a way to read-only query
                // the registry location.
                var manager = CKAN.RegistryManager.Instance(ksp.KSP);

                Assert.IsNotNull(manager);

                var registryPath = CKAN.RegistryManager.Instance(ksp.KSP).lockfilePath;

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
                    path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 512, FileOptions.DeleteOnClose
                );

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
        public void Registry_ZeroByteRegistryJson_EmptyRegistryWithoutCrash()
        {
            // Arrange
            string registryPath = TestData.DataDir("zero-byte-registry.json");
            DisposableKSP dispksp;
            CKAN.KSP      ksp;

            // Act
            dispksp = new DisposableKSP(null, registryPath);
            ksp     = dispksp.KSP;

            // Assert
            CKAN.Registry reg = CKAN.RegistryManager.Instance(ksp).registry;
            Assert.IsNotNull(reg);
            // These lists should all be empty, copied from CKAN.Registry.Empty()
            Assert.IsFalse(reg.InstalledModules.Any());
            Assert.IsFalse(reg.InstalledDlls.Any());
            Assert.IsFalse(reg.HasAnyAvailable());
            // installed_files isn't exposed for testing
            // A default repo is set during load
            Assert.IsTrue(reg.Repositories.Any());
        }

    }
}
