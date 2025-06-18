using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            using (var dir = new TemporaryDirectory())
            {
                // Library folder
                var confDir = dir.Path.CreateSubdirectory("config");
                File.WriteAllLines(Path.Combine(confDir.FullName, "libraryfolders.vdf"),
                                   libraryContents(Enumerable.Repeat((index: 0,
                                                                      dir:   dir.Path),
                                                                     1)));

                // Steam games
                var appsDir = dir.Path.CreateSubdirectory("SteamApps");
                File.WriteAllLines(Path.Combine(appsDir.FullName, "appmanifest_220200.acf"),
                                   acfContents(220200, "Kerbal Space Program"));
                File.WriteAllLines(Path.Combine(appsDir.FullName, "appmanifest_954850.acf"),
                                   acfContents(954850, "Kerbal Space Program 2"));

                // Non-Steam games
                var userConfDir = dir.Path.CreateSubdirectory(
                                      Path.Combine("userdata", "1", "config"));
                File.WriteAllBytes(Path.Combine(userConfDir.FullName,
                                                "shortcuts.vdf"),
                                   shortcutsContents((index:   0,
                                                      name:    "Test Instance",
                                                      absPath: Path.GetFullPath(TestData.good_ksp_dir()))));

                // Act
                var lib = new SteamLibrary(dir.Path.FullName);

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
            using (var dir = new TemporaryDirectory())
            {
                // Library folder - Non-consecutive indexes
                var confDir = dir.Path.CreateSubdirectory("config");
                File.WriteAllLines(Path.Combine(confDir.FullName, "libraryfolders.vdf"),
                                   libraryContents(Enumerable.Repeat((index: 99,
                                                                      dir:   dir.Path),
                                                                     1)));

                // Steam games
                var appsDir = dir.Path.CreateSubdirectory("SteamApps");
                File.WriteAllLines(Path.Combine(appsDir.FullName, "appmanifest_220200.acf"),
                                   acfContents(220200, "Kerbal Space Program"));
                File.WriteAllLines(Path.Combine(appsDir.FullName, "appmanifest_954850.acf"),
                                   acfContents(954850, "Kerbal Space Program 2"));
                // Empty
                File.WriteAllBytes(Path.Combine(appsDir.FullName, "appmanifest_72850.acf"),
                                   Array.Empty<byte>());
                // All null bytes
                File.WriteAllBytes(Path.Combine(appsDir.FullName, "appmanifest_253250.acf"),
                                   Enumerable.Repeat((byte)0, 128).ToArray());

                // Non-Steam games
                var userConfDir = dir.Path.CreateSubdirectory(
                                      Path.Combine("userdata", "1", "config"));
                File.WriteAllBytes(Path.Combine(userConfDir.FullName,
                                                "shortcuts.vdf"),
                                   shortcutsContents((index:   0,
                                                      name:    "Test Instance",
                                                      absPath: Path.GetFullPath(TestData.good_ksp_dir())),
                                                      // Empty StartDir
                                                     (index:   1,
                                                      name:    "Empty StartDir",
                                                      absPath: "")));

                // Act
                var lib = new SteamLibrary(dir.Path.FullName);

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

        private string[] libraryContents(IEnumerable<(int index, DirectoryInfo dir)> libs)
            => new string[]
               {
                   @"""libraryfolders""",
                   "{",
               }
               .Concat(libs.SelectMany(lib => new string[]
               {
                   $@"	""{lib.index}""",
                   "	{",
                   $@"		""path""		""{Path.GetFullPath(lib.dir.FullName)}""",
                   "	}",
               })).Concat(new string[]
               {
                   "}",
                   "",
               })
               .ToArray();

        private string[] acfContents(int appId, string name)
            => new string[]
               {
                   @"""AppState""",
                   "{",
                   $@"	""appid""		""{appId}""",
                   $@"	""name""		""{name}""",
                   $@"	""installdir""		""{name}""",
                   "}",
                   "",
               };

        private byte[] shortcutsContents(params (int index, string name, string absPath)[] shortcuts)
            => binVdfFile(
                   binVdfMap("shortcuts", shortcuts.SelectMany(s =>
                       binVdfMap($"{s.index}",
                           binVdfStr("appname",            s.name),
                           binVdfStr("exe",                $"\"{s.absPath}/KSP.x86_64\""),
                           binVdfStr("StartDir",           $"\"{s.absPath}\""),
                           binVdfStr("icon",               ""),
                           binVdfStr("ShortcutPath",       ""),
                           binVdfStr("LaunchOptions",      ""),
                           binVdfInt("IsHidden",           0),
                           binVdfInt("AllowDesktopConfig", 1),
                           binVdfInt("OpenVR",             0),
                           binVdfInt("LastPlayTime",       0),
                           binVdfMap("tags",               Enumerable.Empty<byte>())))))
                   .ToArray();

        private IEnumerable<byte> binVdfFile(IEnumerable<byte> contents)
            => contents.Concat(Encoding.ASCII.GetBytes("\x0008"));

        private IEnumerable<byte> binVdfMap(string name, params IEnumerable<byte>[] groups)
            => Encoding.ASCII.GetBytes($"\x0000{name}\x0000")
                             .Concat(groups.SelectMany(g => g))
                             .Concat(Encoding.ASCII.GetBytes("\x0008"));

        private IEnumerable<byte> binVdfStr(string name, string val)
            => Encoding.ASCII.GetBytes($"\x0001{name}\x0000{val}\x0000");

        private IEnumerable<byte> binVdfInt(string name, int val)
            => Encoding.ASCII.GetBytes($"\x0002{name}\x0000")
                             .Concat(BitConverter.GetBytes(val));

        [TestCase("./KSP.x86_64 -single-instance", ExpectedResult = false)]
        [TestCase("KSP2_x64.exe -single-instance", ExpectedResult = false)]
        [TestCase("steam://rungameid/220200",      ExpectedResult = true)]
        [TestCase("steam://rungameid/954850",      ExpectedResult = true)]
        public bool IsSteamCmdLine(string command) => SteamLibrary.IsSteamCmdLine(command);

    }
}
