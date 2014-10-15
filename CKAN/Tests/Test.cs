using NUnit.Framework;
using System;
using CKAN;
using System.Text.RegularExpressions;

namespace Tests
{
    [TestFixture()]
    public class Module {

        // Oh dear. The horror. We should load this from a file. Please.
        static string ckan_string = @"
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
}";

        [Test()]
        public void StandardName() {

            CkanModule module = CkanModule.from_string (ckan_string);

            Assert.AreEqual (module.StandardName (), "kOS-0.14.zip");
        }

        [Test()]
        public void CompatibleWith() {
            var module = CkanModule.from_string (ckan_string);

            Assert.IsTrue (module.IsCompatibleKSP ("0.24.2"));
        }
    }


    [TestFixture()]
    public class KSP
    {

        // Disabled, because Travis machines don't have a KSP intall.
        // TODO: How do we mark tests as 'TODO' in Nunit? Does it even have that?
        // [Test()]
        public void TestCase ()
        {

            string gameData = CKAN.KSP.GameData ();

            Assert.IsTrue (Regex.IsMatch (gameData, "GameData/?$", RegexOptions.IgnoreCase));
        }
    }
}

