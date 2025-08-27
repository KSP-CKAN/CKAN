using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests.Data
{
    public class TemporarySteamDirectory : TemporaryDirectory
    {
        public TemporarySteamDirectory(IReadOnlyCollection<(string acfFileName, int appId, string appName)> steamGames,
                                       IReadOnlyCollection<(string name, string absPath)>                   nonSteamGames)
            : base()
        {
                // Library folder
                var confDir = Directory.CreateSubdirectory("config");
                File.WriteAllLines(Path.Combine(confDir.FullName, "libraryfolders.vdf"),
                                   libraryContents(Enumerable.Repeat((index: 0,
                                                                      dir:   Directory),
                                                                     1)));

                // Steam games
                AppsDirectory = Directory.CreateSubdirectory("SteamApps");
                foreach ((string acfFileName, int appId, string appName) in steamGames)
                {
                    File.WriteAllLines(Path.Combine(AppsDirectory.FullName, acfFileName),
                                       acfContents(appId, appName));
                }

                // Non-Steam games
                var userConfDir = Directory.CreateSubdirectory(
                                      Path.Combine("userdata", "1", "config"));
                File.WriteAllBytes(Path.Combine(userConfDir.FullName,
                                                "shortcuts.vdf"),
                                   shortcutsContents(nonSteamGames.Select((tuple, i) =>
                                                                              (index:   i,
                                                                               tuple.name,
                                                                               path: tuple.absPath))
                                                                  .ToArray()));
        }

        public readonly DirectoryInfo AppsDirectory;

        private static string[] libraryContents(IEnumerable<(int index, DirectoryInfo dir)> libs)
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

        private static string[] acfContents(int appId, string name)
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

        private static byte[] shortcutsContents(params (int index, string name, string absPath)[] shortcuts)
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

        private static IEnumerable<byte> binVdfFile(IEnumerable<byte> contents)
            => contents.Concat(Encoding.ASCII.GetBytes("\x0008"));

        private static IEnumerable<byte> binVdfMap(string name, params IEnumerable<byte>[] groups)
            => Encoding.ASCII.GetBytes($"\x0000{name}\x0000")
                             .Concat(groups.SelectMany(g => g))
                             .Concat(Encoding.ASCII.GetBytes("\x0008"));

        private static IEnumerable<byte> binVdfStr(string name, string val)
            => Encoding.ASCII.GetBytes($"\x0001{name}\x0000{val}\x0000");

        private static IEnumerable<byte> binVdfInt(string name, int val)
            => Encoding.ASCII.GetBytes($"\x0002{name}\x0000")
                             .Concat(BitConverter.GetBytes(val));

    }
}
