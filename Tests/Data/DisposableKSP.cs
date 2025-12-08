using System;
using System.IO;

using CKAN;
using CKAN.Games;
using CKAN.Games.KerbalSpaceProgram;

namespace Tests.Data
{
    /// <summary>
    /// A disposable KSP instance. Use the `.KSP` property to access, will
    /// be automatically cleaned up on DisposableKSP falling out of using() scope.
    /// </summary>
    public class DisposableKSP : IDisposable
    {
        /// <summary>
        /// Creates a copy of the provided argument, or a known-good KSP install if passed null.
        /// Use .KSP to access the KSP object itself.
        /// </summary>
        public DisposableKSP()
            : this("disposable", new KerbalSpaceProgram())
        {
        }

        public DisposableKSP(string name, IGame game)
        {
            Utilities.CopyDirectory(TestData.good_ksp_dir(),
                                    _disposableDir,
                                    Array.Empty<string>(),
                                    Array.Empty<string>(),
                                    Array.Empty<string>(),
                                    Array.Empty<string>());
            KSP = new GameInstance(game, _disposableDir,
                                   name, new NullUser());
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
            _disposableDir.Dispose();

            GC.SuppressFinalize(this);
        }

        public GameInstance KSP { get; private set; }

        private readonly TemporaryDirectory _disposableDir = new TemporaryDirectory();
    }
}
