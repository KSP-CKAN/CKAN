using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class InstallValidatorTests
    {
        [TestCase("1",     "v1.2",  "GameData/something"),
         TestCase("v1.11", "v1.12", "Ships/something"),
         TestCase("v1.15", "v1.16", "Ships/@thumbs"),
         TestCase("v1.13", "v1.14", "Scenarios"),
         TestCase("v1.24", "v1.25", "Missions"),
         TestCase("v1.28", "v1.29", "Ships/Script"),
        ]
        public void Validate_SpecVersionSpecificInstallTo_OnlyThrowsBeforeVersion(string throwsSpecVersion,
                                                                                  string doesNotThrowSpecVersion,
                                                                                  string install_to)
        {
            Assert.Throws<Kraken>(() => TryInstallTo(throwsSpecVersion, install_to));
            Assert.DoesNotThrow(() => TryInstallTo(doesNotThrowSpecVersion, install_to));
        }

        [TestCase("1",     null,    @"{ ""file"": ""something"" }"),
         TestCase("1",     null,    @"{ ""file"": ""something"", ""install_to"": ""GameData\\Backslash"" }"),
         TestCase("1",     null,    @"{ ""file"": ""TrailingSlash/"", ""install_to"": ""GameData"" }"),
         TestCase("v1.4",  null,    @"{ ""find"": ""TrailingSlash/"", ""install_to"": ""GameData"" }"),
         TestCase("v1.3",  "v1.4",  @"{ ""find"": ""something"", ""install_to"": ""GameData"" }"),
         TestCase("v1.9",  "v1.10", @"{ ""find_regexp"": ""something"", ""install_to"": ""GameData"" }"),
         TestCase("v1.9",  "v1.10", @"{ ""find"": ""something"", ""filter_regexp"": ""blah"", ""install_to"": ""GameData"" }"),
         TestCase("v1.15", "v1.16", @"{ ""find_matches_files"": true, ""find"": ""something"", ""install_to"": ""GameData"" }"),
         TestCase("v1.17", "v1.18", @"{ ""as"": ""somethingelse"", ""find"": ""something"", ""install_to"": ""GameData"" }"),
         TestCase("v1.23", "v1.24", @"{ ""find"": ""something"", ""include_only"": ""blah"", ""install_to"": ""GameData"" }"),
         TestCase("v1.23", "v1.24", @"{ ""find"": ""something"", ""include_only_regexp"": ""blah"", ""install_to"": ""GameData"" }"),
        ]
        public void Validate_SpecVersionSpecificInstallStanza_OnlyThrowsBeforeVersion(string  throwsSpecVersion,
                                                                                      string? doesNotThrowSpecVersion,
                                                                                      string  installStanza)
        {
            Assert.Throws<Kraken>(() => TryInstallStanza(throwsSpecVersion, installStanza));
            if (doesNotThrowSpecVersion != null)
            {
                Assert.DoesNotThrow(() => TryInstallStanza(doesNotThrowSpecVersion, installStanza));
            }
        }

        private static void TryInstallTo(string spec_version, string install_to)
        {
            // Arrange
            var json = new JObject()
            {
                { "spec_version", spec_version },
                { "identifier",   "AwesomeMod" },
                { "install",      new JArray() {
                    new JObject() {
                        { "file",       "something" },
                        { "install_to", install_to  }
                    }
                } },
            };
            var val = new InstallValidator();

            // Act
            val.Validate(new Metadata(json));
        }

        private static void TryInstallStanza(string spec_version, string installStanza)
        {
            // Arrange
            var json = new JObject()
            {
                { "spec_version", spec_version },
                { "identifier",  "AwesomeMod"  },
                { "install",     new JArray() {
                    JObject.Parse(installStanza)
                } },
            };
            var val = new InstallValidator();

            // Act
            val.Validate(new Metadata(json));
        }
    }
}
