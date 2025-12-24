using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using CKAN;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class EpochTransformerTests
    {
        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01""}", "1.01", "1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01"", ""x_netkan_epoch"": ""0""}",
             "1.01", "1.01", Description = "Implicit 0")]
        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01"", ""x_netkan_epoch"": ""1""}", "1.01", "1:1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"": ""v1.01"", ""x_netkan_epoch"": ""9""}", "v1.01", "9:v1.01")]
        public void Transform_WithAndWithoutEpochs_EpochAppliedCorrectly(string json, string orig_version, string new_version)
        {
            JObject metadata = JObject.Parse(json);
            var opts = new TransformOptions(1, null, null, null, false, null);

            Assert.AreEqual(orig_version, (string?)metadata["version"], "JSON parsed as expected");
            metadata = new EpochTransformer().Transform(new Metadata(metadata), opts).First().Json();
            Assert.AreEqual(new_version, (string?)metadata["version"], "Output string as expected");
        }

        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01""}", false)]
        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01"", ""x_netkan_epoch"": ""1""}", false)]
        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01"", ""x_netkan_epoch"": ""3""}", false)]
        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01"", ""x_netkan_epoch"": ""a""}", true)]
        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01"", ""x_netkan_epoch"": ""-1""}", true)]
        [TestCase(@"{""spec_version"": 1, ""version"": ""1.01"", ""x_netkan_epoch"": ""5.5""}", true)]
        public void Transform_ValidOrInvalidEpoch_ThrowsIffInvalid(string json, bool expected_to_throw)
        {
            var opts = new TransformOptions(1, null, null, null, false, null);
            TestDelegate test_delegate = () => new EpochTransformer().Transform(new Metadata(JObject.Parse(json)), opts).First().Json();
            if (expected_to_throw)
            {
                Assert.Throws<BadMetadataKraken>(test_delegate);
            }
            else
            {
                Assert.DoesNotThrow(test_delegate);
            }
        }

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

        [TestCase("v1.0.7",    "1.0.8", null, true),
         TestCase("v1.0.7",    "1.0.8", 1,    false),
         TestCase("v.1.0.7",   "1.0.8", null, true),
         TestCase("v.1.0.7",   "1.0.8", 1,    false),
         TestCase("1:v.1.0.7", "1.0.8", 1,    true),
         TestCase("1:v.1.0.7", "1.0.8", 2,    false),
        ]
        public void Transform_OnUnreliableHost_ThrowsIffOutOfOrder(string highest,
                                                                   string version,
                                                                   int?   epoch,
                                                                   bool   throws)
        {
            // Arrange
            var opts = new TransformOptions(1, null, new ModuleVersion(highest),
                                            null, false, null)
                       {
                           FlakyAPI = true,
                       };
            var sut  = new EpochTransformer();
            var json = new JObject() { { "version", version } };
            if (epoch != null)
            {
                json["x_netkan_epoch"] = epoch;
            }
            var metadata = new Metadata(json);
            var vWithEpoch = epoch != null ? $"{epoch}:{version}"     : version;
            var vWithNext  = epoch != null ? $"{epoch + 1}:{version}" : $"1:{version}";

            // Act / Assert
            if (throws)
            {
                var exc = Assert.Throws<Kraken>(() => sut.Transform(metadata, opts).Single())!;
                Assert.AreEqual($"Out-of-order version found on unreliable host: {vWithEpoch} < {highest} < {vWithNext}",
                                exc.Message);
            }
            else
            {
                var result = sut.Transform(metadata, opts).Single();
                Assert.AreEqual(vWithEpoch, result.Version?.ToString(false, false));
            }
        }
    }
}
