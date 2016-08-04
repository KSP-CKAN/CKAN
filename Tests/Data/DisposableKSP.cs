using CKAN;
using System;
using System.IO;

namespace Tests.Data
{
    /// <summary>
    /// A disposable KSP instance. Use the `.KSP` property to access, will
    /// be automatically cleaned up on DisposableKSP falling out of using() scope.
    /// </summary>
    public class DisposableKSP : IDisposable
    {
        private readonly string good_ksp = TestData.good_ksp_dir();
        private readonly string disposable_dir;

        public KSP KSP { get; private set; }

        /// <summary>
        /// Creates a copy of the provided argument, or a known-good KSP install if passed null.
        /// Use .KSP to access the KSP object itself.
        /// </summary>
        public DisposableKSP(string directory_to_clone = null, string registry_file = null)
        {
            directory_to_clone = directory_to_clone ?? good_ksp;
            disposable_dir = TestData.NewTempDir();
            TestData.CopyDirectory(directory_to_clone, disposable_dir);

            // If we've been given a registry file, then copy it into position before
            // creating our KSP object.

            if (registry_file != null)
            {
                string registry_dir = Path.Combine(disposable_dir, "CKAN");
                string registry_path = Path.Combine(registry_dir, "registry.json");
                Directory.CreateDirectory(registry_dir);
                File.Copy(registry_file, registry_path, true);
            }

            KSP = new KSP(disposable_dir, NullUser.User);
        }

        public void Dispose()
        {
            Directory.Delete(disposable_dir, true);
            KSP.Dispose();
            KSP = null; // In case .Dispose() was called manually.
        }
    }
}