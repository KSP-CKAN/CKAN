using System;
using System.IO;

using NUnit.Framework;

using CKAN;
using CKAN.Games.KerbalSpaceProgram;

namespace Tests.Data
{
    /// <summary>
    /// A disposable KSP instance. Use the `.KSP` property to access, will
    /// be automatically cleaned up on DisposableKSP falling out of using() scope.
    /// </summary>
    public class DisposableKSP : IDisposable
    {
        private const string _failureMessage = "Unexpected exception trying to delete disposable test container.";
        private readonly string _goodKsp = TestData.good_ksp_dir();
        private readonly string _disposableDir;

        public GameInstance KSP { get; private set; }

        /// <summary>
        /// Creates a copy of the provided argument, or a known-good KSP install if passed null.
        /// Use .KSP to access the KSP object itself.
        /// </summary>
        public DisposableKSP()
        {
            _disposableDir = TestData.NewTempDir();
            Utilities.CopyDirectory(_goodKsp, _disposableDir, Array.Empty<string>(), Array.Empty<string>());
            KSP = new GameInstance(new KerbalSpaceProgram(), _disposableDir, "disposable", new NullUser());
            Logging.Initialize();
        }

        public DisposableKSP(string registryFile)
            : this()
        {
            var registryDir = Path.Combine(_disposableDir, "CKAN");
            Directory.CreateDirectory(registryDir);
            File.Copy(registryFile, Path.Combine(registryDir, "registry.json"), true);
        }

        public void Dispose()
        {
            RegistryManager.DisposeInstance(KSP);

            var i = 6;
            while (--i > 0)
            {
                try
                {
                    // Now that the lockfile is closed, we can remove the directory
                    Directory.Delete(_disposableDir, true);
                }
                catch (IOException)
                {
                    // We silently catch this exception because we expect failures
                }
                catch (Exception ex)
                {
                    throw new AssertionException(_failureMessage, ex);
                }
            }

            KSP = null;
        }
    }
}
