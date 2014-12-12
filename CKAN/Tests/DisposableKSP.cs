using System;
using System.IO;
using CKAN;

namespace Tests
{
    /// <summary>
    /// A disposable KSP instance. Use the `.KSP` property to access, will
    /// be automatically cleaned up on DisposableKSP falling out of using() scope.
    /// </summary>
    public class DisposableKSP : IDisposable
    {
        private readonly string good_ksp = Tests.TestData.good_ksp_dir();
        private KSP _ksp;
        private string disposable_dir;

        public KSP KSP
        {
            get
            {
                return _ksp;
            }
        }

        /// <summary>
        /// Creates a copy of the provided argument, or a known-good KSP install if passed null.
        /// Use .KSP to access the KSP object itself.
        /// </summary>
        public DisposableKSP(string directory_to_clone = null, string registry_file = null)
        {
            directory_to_clone = directory_to_clone ?? good_ksp;
            disposable_dir = Tests.TestData.NewTempDir();
            Tests.TestData.CopyDirectory(directory_to_clone, disposable_dir);

            // If we've been given a registry file, then copy it into position before
            // creating our KSP object.

            if (registry_file != null)
            {
                string registry_dir = Path.Combine(disposable_dir, "CKAN");
                string registry_path = Path.Combine(registry_dir, "registry.json");
                Directory.CreateDirectory(registry_dir);
                File.Copy(registry_file, registry_path, true);
            }

            _ksp = new KSP(disposable_dir, NullUser.User);
        }

        public void Dispose()
        {
            Directory.Delete(disposable_dir, true);
            _ksp = null; // In case .Dispose() was called manually.
        }
    }
}

