using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class EpochTransformerTests
    {
        [
            TestCase("1.1",     null,   null, false,    "1.1", false),
            TestCase("1.1",     null,   null,  true,    "1.1", false),
            TestCase("1.1",    "1.1",   null, false,    "1.1", false),
            TestCase("1.1",    "1.2",   null, false,  "1:1.1", true),
            TestCase("1.1",  "5:1.1",   null, false,  "5:1.1", false),
            TestCase("1.1",  "5:1.2",   null, false,  "6:1.1", true),
            TestCase("0.7",   "0.65",   null, false,  "1:0.7", true),
            TestCase("2.5",   "v2.4",   null, false,  "1:2.5", true),
            TestCase("2.5",   "V2.4",   null, false,  "1:2.5", true),
            TestCase("v2.5", "vV2.4",   null, false, "1:v2.5", true),

            // Non-prerelease with a prerelease on another host
            TestCase("1.0",    "1.0",  "2.0", false,    "1.0", false),
            // Prerelease with no normal releases
            TestCase("1.0",     null,  "1.0",  true,    "1.0", false),
            // Updating a prerelease to a normal release
            TestCase("2.0",    "1.0",  "2.0", false,    "2.0", false),
            // Prerelease needing an epoch boost
            TestCase("2.0",    "1.0",  "3.0",  true,  "1:2.0", true),
            // The previous prerelease is old
            TestCase("1.5",    "2.0",  "1.0",  true,  "1:1.5", true),
        ]
        public void Transform_WithHighVersionParam_MatchesExpected(string  version,
                                                                   string? highVer,
                                                                   string? highVerPre,
                                                                   bool    prerelease,
                                                                   string  expected,
                                                                   bool    staged)
        {
            // Arrange
            var json = new JObject()
            {
                { "spec_version",   "v1.4"          },
                { "identifier",     "AwesomeMod"    },
                { "version",        version         },
                { "release_status", prerelease
                                        ? "testing"
                                        : "stable"  },
            };
            ITransformer     sut  = new EpochTransformer();
            TransformOptions opts = new TransformOptions(
                1,
                null,
                highVer is null or ""
                    ? null
                    : new ModuleVersion(highVer),
                highVerPre is null or ""
                    ? null
                    : new ModuleVersion(highVerPre),
                false,
                null
            );

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(expected, (string?)transformedJson["version"]);
            Assert.AreEqual(staged,   opts.Staged);
        }
    }
}
