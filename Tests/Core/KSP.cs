using System;
using System.IO;
using NUnit.Framework;
using Tests.Data;
using CKAN;
using CKAN.Versioning;
using CKAN.Games;

namespace Tests.Core
{
    [TestFixture]
    public class KSP
    {
        private CKAN.GameInstance ksp;
        private string ksp_dir;
        private IUser nullUser;

        [SetUp]
        public void Setup()
        {
            ksp_dir = TestData.NewTempDir();
            nullUser = new NullUser();
            CKAN.Utilities.CopyDirectory(TestData.good_ksp_dir(), ksp_dir, true);
            ksp = new CKAN.GameInstance(new KerbalSpaceProgram(), ksp_dir, "test", nullUser);
        }

        [TearDown]
        public void TearDown()
        {
            if (ksp != null)
            {
                // Manually dispose of RegistryManager
                // For some reason the KSP instance doesn't do this itself causing test failures because the registry
                // lock file is still in use. So just dispose of it ourselves.
                CKAN.RegistryManager.Instance(ksp).Dispose();
            }

            Directory.Delete(ksp_dir, true);
        }

        [Test]
        public void IsGameDir()
        {
            var game = new KerbalSpaceProgram();

            // Our test data directory should be good.
            Assert.IsTrue(game.GameInFolder(new DirectoryInfo(TestData.good_ksp_dir())));

            // As should our copied folder.
            Assert.IsTrue(game.GameInFolder(new DirectoryInfo(ksp_dir)));

            // And the one from our KSP instance.
            Assert.IsTrue(game.GameInFolder(new DirectoryInfo(ksp.GameDir())));

            // All these ones should be bad.
            foreach (string dir in TestData.bad_ksp_dirs())
            {
                Assert.IsFalse(game.GameInFolder(new DirectoryInfo(dir)));
            }
        }

        [Test]
        public void Tutorial()
        {
            //Use Uri to avoid issues with windows vs linux line separators.
            var canonicalPath = new Uri(Path.Combine(ksp_dir, "saves", "training")).LocalPath;
            var game = new KerbalSpaceProgram();
            string dest;
            Assert.IsTrue(game.AllowInstallationIn("Tutorial", out dest));
            Assert.AreEqual(
                new DirectoryInfo(ksp.ToAbsoluteGameDir(dest)),
                new DirectoryInfo(canonicalPath)
            );
        }

        [Test]
        public void ScanDlls()
        {
            string path = Path.Combine(ksp.game.PrimaryModDirectory(ksp), "Example.dll");
            var registry = CKAN.RegistryManager.Instance(ksp).registry;

            Assert.IsFalse(registry.IsInstalled("Example"), "Example should start uninstalled");

            File.WriteAllText(path, "Not really a DLL, are we?");

            ksp.Scan();

            Assert.IsTrue(registry.IsInstalled("Example"), "Example installed");

            ModuleVersion version = registry.InstalledVersion("Example");
            Assert.IsInstanceOf<UnmanagedModuleVersion>(version, "DLL detected as a DLL, not full mod");

            // Now let's do the same with different case.

            string path2 = Path.Combine(ksp.game.PrimaryModDirectory(ksp), "NewMod.DLL");

            Assert.IsFalse(registry.IsInstalled("NewMod"));
            File.WriteAllText(path2, "This text is irrelevant. You will be assimilated");

            ksp.Scan();

            Assert.IsTrue(registry.IsInstalled("NewMod"));
        }

        [Test]
        public void ToAbsolute()
        {
            Assert.AreEqual(
                CKAN.CKANPathUtils.NormalizePath(
                    Path.Combine(ksp_dir, "GameData/HydrazinePrincess")
                ),
                ksp.ToAbsoluteGameDir("GameData/HydrazinePrincess")
            );
        }

        [Test]
        public void ToRelative()
        {
            string absolute = Path.Combine(ksp_dir, "GameData/HydrazinePrincess");

            Assert.AreEqual(
                "GameData/HydrazinePrincess",
                ksp.ToRelativeGameDir(absolute)
            );
        }

        [Test]
        public void Valid_MissingVersionData_False()
        {
            // Arrange
            string gamedir  = TestData.NewTempDir();
            string ckandir  = Path.Combine(gamedir, "CKAN");
            string buildid  = Path.Combine(gamedir, "buildID.txt");
            string readme   = Path.Combine(gamedir, "readme.txt");
            string jsonpath = Path.Combine(ckandir, "compatible_ksp_versions.json");
            const string compatible_ksp_versions_json = @"{
                ""VersionOfKspWhenWritten"": ""1.4.3"",
                ""CompatibleGameVersions"":   [""1.4""]
            }";

            // Generate a valid game dir except for missing buildID.txt and readme.txt
            CKAN.Utilities.CopyDirectory(TestData.good_ksp_dir(), gamedir, true);
            File.Delete(buildid);
            File.Delete(readme);

            // Save GameDir/CKAN/compatible_ksp_versions.json
            Directory.CreateDirectory(ckandir);
            File.WriteAllText(jsonpath, compatible_ksp_versions_json);

            // Act
            CKAN.GameInstance my_ksp = new CKAN.GameInstance(new KerbalSpaceProgram(), gamedir, "missing-ver-test", nullUser);

            // Assert
            Assert.IsFalse(my_ksp.Valid);

            Directory.Delete(gamedir, true);
        }

        [Test]
        public void Constructor_NullMainCompatVer_NoCrash()
        {
            // Arrange
            string gamedir  = TestData.NewTempDir();
            string ckandir  = Path.Combine(gamedir, "CKAN");
            string buildid  = Path.Combine(gamedir, "buildID.txt");
            string readme   = Path.Combine(gamedir, "readme.txt");
            string jsonpath = Path.Combine(ckandir, "compatible_ksp_versions.json");
            const string compatible_ksp_versions_json = @"{
                ""VersionOfKspWhenWritten"": null,
                ""CompatibleGameVersions"":   [""1.4""]
            }";

            // Generate a valid game dir except for missing buildID.txt and readme.txt
            CKAN.Utilities.CopyDirectory(TestData.good_ksp_dir(), gamedir, true);
            File.Delete(buildid);
            File.Delete(readme);

            // Save GameDir/CKAN/compatible_ksp_versions.json
            Directory.CreateDirectory(ckandir);
            File.WriteAllText(jsonpath, compatible_ksp_versions_json);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                CKAN.GameInstance my_ksp = new CKAN.GameInstance(new KerbalSpaceProgram(), gamedir, "null-compat-ver-test", nullUser);
            });

            Directory.Delete(gamedir, true);
        }

    }
}
