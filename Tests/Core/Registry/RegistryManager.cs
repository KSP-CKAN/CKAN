using System;
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
                string registryPath = ksp.KSP.RegistryManager.lockfile_path;

                // Let's try opening the same registry file a second time...

                Assert.IsTrue(TestLock(registryPath));

                // And after we dispose the registry manager, the lock should be gone...
                ksp.KSP.RegistryManager.Dispose();

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
    }
}