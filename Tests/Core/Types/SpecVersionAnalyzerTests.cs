using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;

namespace Tests.Core.Types
{
    [TestFixture]
    public class SpecVersionAnalyzerTests
    {
        [TestCase(@"{ ""release_status"": ""testing"" }",
                  ExpectedResult = "v1.36"),
         TestCase(@"{ ""download_hash"": { ""sha1"": ""DEADBEEF"" } }",
                  ExpectedResult = "v1.35"),
         TestCase(@"{ ""download_hash"": { ""sha256"": ""DEADBEEF"" } }",
                  ExpectedResult = "v1.35"),
         TestCase(@"{ ""download"": [ ] }",
                  ExpectedResult = "v1.34"),
         TestCase(@"{ ""depends"": [ { ""any_of"": [], ""choice_help_text"": """" } ] }",
                  ExpectedResult = "v1.31"),
         TestCase(@"{ ""license"": ""MPL-2.0"" }",
                  ExpectedResult = "v1.30"),
         TestCase(@"{ ""install"": [ { ""install_to"": ""Ships/Script"" } ] }",
                  ExpectedResult = "v1.29"),
         TestCase(@"{ ""kind"": ""dlc"" }",
                  ExpectedResult = "v1.28"),
         TestCase(@"{ ""replaced_by"": {} }",
                  ExpectedResult = "v1.26"),
         TestCase(@"{ ""depends"": [ { ""any_of"": [] } ] }",
                  ExpectedResult = "v1.26"),
         TestCase(@"{ ""install"": [ { ""install_to"": ""Missions"" } ] }",
                  ExpectedResult = "v1.25"),
         TestCase(@"{ ""install"": [ { ""include_only"": [] } ] }",
                  ExpectedResult = "v1.24"),
         TestCase(@"{ ""install"": [ { ""include_only_regexp"": [] } ] }",
                  ExpectedResult = "v1.24"),
         TestCase(@"{ ""license"": ""Unlicense"" }",
                  ExpectedResult = "v1.18"),
         TestCase(@"{ ""install"": [ { ""as"": """" } ] }",
                  ExpectedResult = "v1.18"),
         TestCase(@"{ ""ksp_version_strict"": true }",
                  ExpectedResult = "v1.16"),
         TestCase(@"{ ""install"": [ { ""install_to"": ""Ships/@thumbs"" } ] }",
                  ExpectedResult = "v1.16"),
         TestCase(@"{ ""install"": [ { ""find_matches_files"": true } ] }",
                  ExpectedResult = "v1.16"),
         TestCase(@"{ ""install"": [ { ""install_to"": ""Scenarios"" } ] }",
                  ExpectedResult = "v1.14"),
         TestCase(@"{ ""install"": [ { ""install_to"": ""Ships/VAB"" } ] }",
                  ExpectedResult = "v1.12"),
         TestCase(@"{ ""install"": [ { ""find_regexp"": """" } ] }",
                  ExpectedResult = "v1.10"),
         TestCase(@"{ ""license"": [] }",
                  ExpectedResult = "v1.8"),
         TestCase(@"{ ""kind"": ""metapackage"" }",
                  ExpectedResult = "v1.6"),
         TestCase(@"{ ""install"": [ { ""find"": """" } ] }",
                  ExpectedResult = "v1.4"),
         TestCase(@"{ ""license"": ""WTFPL"" }",
                  ExpectedResult = "v1.2"),
         TestCase(@"{ ""supports"": [] }",
                  ExpectedResult = "v1.2"),
         TestCase(@"{ ""install"": [ { ""install_to"": ""GameData/Subfolder"" } ] }",
                  ExpectedResult = "v1.2"),
         TestCase(@"{ }",
                  ExpectedResult = "v1.0"),
        ]
        public string MinimumSpecVersion(string rawJson)
            => SpecVersionAnalyzer.MinimumSpecVersion(JObject.Parse(rawJson)).ToString();
    }
}
