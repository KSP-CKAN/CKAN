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
        [Test,
            TestCase("1.1",     null,    "1.1"),
            TestCase("1.1",    "1.1",    "1.1"),
            TestCase("1.1",    "1.2",  "1:1.1"),
            TestCase("1.1",  "5:1.1",  "5:1.1"),
            TestCase("1.1",  "5:1.2",  "6:1.1"),
            TestCase("0.7",   "0.65",  "1:0.7"),
            TestCase("2.5",   "v2.4",  "1:2.5"),
            TestCase("2.5",   "V2.4",  "1:2.5"),
            TestCase("v2.5", "vV2.4", "1:v2.5"),
        ]
        public void Transform_WithHighVersionParam_MatchesExpected(string version, string highVer, string expected)
        {
            // Arrange
            var json = new JObject()
            {
                { "spec_version", "v1.4"       },
                { "identifier",   "AwesomeMod" },
                { "version",      version      },
            };
            ITransformer     sut  = new EpochTransformer();
            TransformOptions opts = new TransformOptions(
                1,
                null,
                string.IsNullOrEmpty(highVer)
                    ? null
                    : new ModuleVersion(highVer),
                false,
                null
            );

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(expected, (string)transformedJson["version"]);
        }
    }
}
