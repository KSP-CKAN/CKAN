using System;
using System.IO;

using NUnit.Framework;

using CKAN;
using CKAN.Games.KerbalSpaceProgram;

using Tests.Data;

namespace Tests.Core
{
    [TestFixture]
    public class GameInstanceTests
    {
        private GameInstance ksp;
        private string ksp_dir;
        private IUser nullUser;

        [SetUp]
        public void Setup()
        {
            ksp_dir = TestData.NewTempDir();
            nullUser = new NullUser();
            CKAN.Utilities.CopyDirectory(TestData.good_ksp_dir(), ksp_dir, Array.Empty<string>(), Array.Empty<string>());
            ksp = new GameInstance(new KerbalSpaceProgram(), ksp_dir, "test", nullUser);
        }

        [TearDown]
        public void TearDown()
        {
            if (ksp != null)
            {
                // Manually dispose of RegistryManager
                // For some reason the KSP instance doesn't do this itself causing test failures because the registry
                // lock file is still in use. So just dispose of it ourselves.
                RegistryManager.DisposeInstance(ksp);
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
            Assert.IsTrue(game.AllowInstallationIn("Tutorial", out string dest));
            Assert.AreEqual(
                new DirectoryInfo(ksp.ToAbsoluteGameDir(dest)),
                new DirectoryInfo(canonicalPath)
            );
        }

        [Test]
        public void ToAbsolute()
        {
            Assert.AreEqual(
                CKANPathUtils.NormalizePath(
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
            CKAN.Utilities.CopyDirectory(TestData.good_ksp_dir(), gamedir, Array.Empty<string>(), Array.Empty<string>());
            File.Delete(buildid);
            File.Delete(readme);

            // Save GameDir/CKAN/compatible_ksp_versions.json
            Directory.CreateDirectory(ckandir);
            File.WriteAllText(jsonpath, compatible_ksp_versions_json);

            // Act
            GameInstance my_ksp = new GameInstance(new KerbalSpaceProgram(), gamedir, "missing-ver-test", nullUser);

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
            CKAN.Utilities.CopyDirectory(TestData.good_ksp_dir(), gamedir, Array.Empty<string>(), Array.Empty<string>());
            File.Delete(buildid);
            File.Delete(readme);

            // Save GameDir/CKAN/compatible_ksp_versions.json
            Directory.CreateDirectory(ckandir);
            File.WriteAllText(jsonpath, compatible_ksp_versions_json);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                GameInstance my_ksp = new GameInstance(new KerbalSpaceProgram(), gamedir, "null-compat-ver-test", nullUser);
            });

            Directory.Delete(gamedir, true);
        }

    }
}
