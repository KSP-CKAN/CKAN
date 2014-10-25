using System;
using System.IO;
using System.Collections.Generic;

namespace Tests
{
    static public class TestData
    {
        public static string DataDir()
        {
            // TODO: Have this actually walk our directory structure and find
            // t/data. This means we can relocate our test executable and
            // things will still work.
            string current = System.IO.Directory.GetCurrentDirectory();

            return Path.Combine(current, "../../../../t/data");
        }

        public static string DogeCoinFlagZip()
        {
            string such_zip_very_currency_wow = Path.Combine(DataDir(), "DogeCoinFlag-1.01.zip");

            return such_zip_very_currency_wow;
        }

        public static Uri TestKAN()
        {
            return new Uri("https://github.com/KSP-CKAN/CKAN-meta/archive/testkan.zip");
        }

        public static string good_ksp_dir()
        {
            return Path.Combine(DataDir(), "KSP/KSP-0.25");
        }

        public static List<string> bad_ksp_dirs()
        {
            var dirs = new List<string>();
            dirs.Add(Path.Combine(DataDir(), "KSP/bad-ksp"));
            dirs.Add(Path.Combine(DataDir(), "KSP/missing-gamedata"));

            return dirs;
        }

        public static string kOS_014()
        {
            return @"
                {
                    ""spec_version"": 1,
                    ""name""     : ""kOS - Kerbal OS"",
                    ""identifier"" : ""kOS"",
                    ""abstract"" : ""A programming and automation environment for KSP craft."",
                    ""download"" : ""https://github.com/KSP-KOS/KOS/releases/download/v0.14/kOS.v14.zip"",
                    ""license""  : ""GPLv3"",
                    ""version""  : ""0.14"",
                    ""release_status"" : ""stable"",
                    ""ksp_version"" : ""0.24.2"",
                    ""resources"" : {
                        ""homepage"" : ""http://forum.kerbalspaceprogram.com/threads/68089-0-23-kOS-Scriptable-Autopilot-System-v0-11-2-13"",
                        ""manual""   : ""http://ksp-kos.github.io/KOS_DOC/"",
                        ""bugtracker"": ""https://github.com/KSP-KOS/KOS/issues"",
                        ""github""   : {
                            ""url""      : ""https://github.com/KSP-KOS/KOS"",
                            ""releases"" : true
                        }
                    },
                    ""install"" : [
                        {
                            ""file""       : ""GameData/kOS"",
                            ""install_to"" : ""GameData""
                        }
                    ]
                }"
            ;
        }

        public static CKAN.CkanModule kOS_014_module()
        {
            return CKAN.CkanModule.FromJson(kOS_014());
        }

    }
}

