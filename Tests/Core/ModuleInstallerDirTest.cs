using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using CKAN;

using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.Core
{
    /// <summary>
    /// Tests the AddParentDirectories method in CKAN.ModuleInstaller
    /// </summary>
    [TestFixture]
    public class ModuleInstallerDirTest
    {
        private GameInstanceManager _manager;
        private DisposableKSP       _instance;
        private FakeConfiguration   _config;
        private RegistryManager     _registryManager;
        private CKAN.Registry       _registry;
        private ModuleInstaller     _installer;
        private CkanModule          _testModule;
        private string              _gameDir;
        private string              _gameDataDir;
        private IUser               _nullUser;

        private TemporaryRepository repo;
        private TemporaryRepositoryData repoData;

        /// <summary>
        /// Prep environment by setting up a single mod in
        /// a disposable KSP instance.
        /// </summary>
        [OneTimeSetUp]
        public void SetUp()
        {
            _nullUser  = new NullUser();
            _instance  = new DisposableKSP();
            repo = new TemporaryRepository(TestData.DogeCoinFlag_101());
            repoData = new TemporaryRepositoryData(_nullUser, repo.repo);

            _config    = new FakeConfiguration(_instance.KSP, _instance.KSP.Name);
            _manager   = new GameInstanceManager(_nullUser, _config);
            _registryManager = RegistryManager.Instance(_instance.KSP, repoData.Manager);
            _registry  = _registryManager.registry;
            _registry.RepositoriesClear();
            _registry.RepositoriesAdd(repo.repo);
            _testModule = _registry.GetModuleByVersion("DogeCoinFlag", "1.01");
            Assert.IsNotNull(_testModule, "DogeCoinFlag 1.01 should exist");

            _installer = new ModuleInstaller(_instance.KSP, _manager.Cache, _nullUser);

            _gameDir = _instance.KSP.GameDir();
            _gameDataDir = _instance.KSP.game.PrimaryModDirectory(_instance.KSP);
            var testModFile = TestData.DogeCoinFlagZip();
            _manager.Cache.Store(_testModule, testModFile, new Progress<int>(percent => {}));
            HashSet<string> possibleConfigOnlyDirs = null;
            _installer.InstallList(
                new List<CkanModule>() { _testModule },
                new RelationshipResolverOptions(),
                _registryManager,
                ref possibleConfigOnlyDirs);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _manager.Dispose();
            _config.Dispose();
            _instance.Dispose();
            repo.Dispose();
            repoData.Dispose();
        }

        /// <summary>
        /// Make sure that the GameRoot directory is not included and doesn't throw an exception.
        /// </summary>
        [Test]
        public void TestGameRoot()
        {
            var result = _installer
                .AddParentDirectories(new HashSet<string>() { _gameDir })
                .ToList();

            Assert.IsEmpty(result);
        }

        /// <summary>
        /// Make sure that the GameData directory is not included.
        /// </summary>
        [Test]
        public void TestGameData()
        {
            var result = _installer
                .AddParentDirectories(new HashSet<string>() { _gameDataDir })
                .ToList();

            Assert.IsEmpty(result);
        }

        /// <summary>
        /// These are sanity tests that attempt to throw exceptions
        /// when invoking AddParentDirectories()
        /// </summary>
        [Test]
        public void TestNullPath()
        {
            Assert.DoesNotThrow(delegate ()
            {
                _installer.AddParentDirectories(null);
                _installer.AddParentDirectories(new HashSet<string>());
                _installer.AddParentDirectories(new HashSet<string>() { string.Empty });
                _installer.AddParentDirectories(new HashSet<string>() { Path.GetPathRoot(Environment.CurrentDirectory) });
            });
        }

        /// <summary>
        /// Make sure that the list of directories is
        /// always normalized and deduplicated.
        /// </summary>
        [Test]
        public void TestSlashVariants()
        {
            var rawInstallDir = Path.Combine(_gameDataDir, _testModule.identifier);
            var normalizedInstallDir = CKANPathUtils.NormalizePath(rawInstallDir);
            var windowsInstallDir = normalizedInstallDir.Replace('/', '\\');

            Assert.DoesNotThrow(delegate ()
            {
                var result = _installer.AddParentDirectories(new HashSet<string>()
                {
                    rawInstallDir,
                    windowsInstallDir,
                    normalizedInstallDir
                }).ToList();

                // should only contain one path
                Assert.AreEqual(1, result.Count);
                Assert.Contains(normalizedInstallDir, result);
            });
        }

        /// <summary>
        /// Try to add the same path with multiple casings, ensure no PathErrorKraken.
        /// Only runs on Windows because filesystem paths are case sensitive on Unix.
        /// </summary>
        [Test]
        [Platform("Win")]
        public void TestCaseSensitivity()
        {
            var paths = new HashSet<string>()
            {
                // add in all-uppercase and all-lowercase version
                Path.Combine(_gameDataDir.ToUpper(), _testModule.identifier),
                Path.Combine(_gameDataDir.ToLower(), _testModule.identifier),
            };
            // here we are looking for no PathErrorKraken
            Assert.DoesNotThrow(delegate()
            {
                var size = _installer.AddParentDirectories(paths).Count;
                // each directory adds two directories to the result
                // two directories each set { GAMEDATA and gamedata } = 4 objects in result array
                if (size != 4)
                {
                    throw new InvalidOperationException("Directories have case-sensitive differences");
                }
            });
        }
    }
}
