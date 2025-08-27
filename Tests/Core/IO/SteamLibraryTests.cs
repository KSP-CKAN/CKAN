using System;
using System.IO;
using System.Linq;
using System.Reflection;

using NUnit.Framework;
using log4net;
using log4net.Core;

using CKAN.IO;
using Tests.Data;

namespace Tests.Core.IO
{
    [TestFixture]
    public sealed class SteamLibraryTests
    {
        [Test]
        public void Constructor_WithValidLibrary_Works()
        {
            // Arrange
            using (var dir = new TemporarySteamDirectory(
                                 new (string acfFileName, int appId, string appName)[]
                                 {
                                     (acfFileName: "appmanifest_220200.acf",
                                      appId:       220200,
                                      appName:     "Kerbal Space Program"),
                                     (acfFileName: "appmanifest_954850.acf",
                                      appId:       954850,
                                      appName:     "Kerbal Space Program 2"),
                                 },
                                 new (string name, string absPath)[]
                                 {
                                     (name:    "Test Instance",
                                      absPath: Path.GetFullPath(TestData.good_ksp_dir())),
                                 }))
            {
                // Act
                var lib = new SteamLibrary(dir.Directory.FullName);

                // Assert
                CollectionAssert.AreEquivalent(new string[]
                                               {
                                                   "Kerbal Space Program",
                                                   "Kerbal Space Program 2",
                                                   "Test Instance",
                                               },
                                               lib.Games.Select(g => g.Name));
            }
        }

        [Test]
        public void Constructor_WithCorruptedLibrary_Works()
        {
            // Arrange
            LogManager.GetRepository(Assembly.GetExecutingAssembly()).Threshold = Level.Off;
            using (var dir = new TemporarySteamDirectory(
                                 new (string acfFileName, int appId, string appName)[]
                                 {
                                     (acfFileName: "appmanifest_220200.acf",
                                      appId:       220200,
                                      appName:     "Kerbal Space Program"),
                                     (acfFileName: "appmanifest_954850.acf",
                                      appId:       954850,
                                      appName:     "Kerbal Space Program 2"),
                                 },
                                 new (string name, string absPath)[]
                                 {
                                     (name:    "Test Instance",
                                      absPath: Path.GetFullPath(TestData.good_ksp_dir())),
                                     (name:    "Empty StartDir",
                                      absPath: ""),
                                 }))
            {
                // Empty
                File.WriteAllBytes(Path.Combine(dir.AppsDirectory.FullName, "appmanifest_72850.acf"),
                                   Array.Empty<byte>());
                // All null bytes
                File.WriteAllBytes(Path.Combine(dir.AppsDirectory.FullName, "appmanifest_253250.acf"),
                                   Enumerable.Repeat((byte)0, 128).ToArray());

                // Act
                var lib = new SteamLibrary(dir.Directory.FullName);

                // Assert
                CollectionAssert.AreEquivalent(new string[]
                                               {
                                                   "Kerbal Space Program",
                                                   "Kerbal Space Program 2",
                                                   "Test Instance",
                                                   "Empty StartDir",
                                               },
                                               lib.Games.Select(g => g.Name));
            }
        }

        [TestCase("./KSP.x86_64 -single-instance", ExpectedResult = false)]
        [TestCase("KSP2_x64.exe -single-instance", ExpectedResult = false)]
        [TestCase("steam://rungameid/220200",      ExpectedResult = true)]
        [TestCase("steam://rungameid/954850",      ExpectedResult = true)]
        public bool IsSteamCmdLine(string command) => SteamLibrary.IsSteamCmdLine(command);

    }
}
