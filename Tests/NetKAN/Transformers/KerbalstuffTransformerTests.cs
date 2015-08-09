using System;
using CKAN.NetKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Kerbalstuff;
using CKAN.NetKAN.Transformers;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class KerbalstuffTransformerTests
    {
        // GH #199: Don't pre-fill KSP version fields if we see a ksp_min/max
        [Test]
        public void DoesNotReplaceKspVersionProperties()
        {
            // Arrange
            var mApi = new Mock<IKerbalstuffApi>();
            mApi.Setup(i => i.GetMod(It.IsAny<int>()))
                .Returns(MakeTestMod());

            var sut = new KerbalstuffTransformer(mApi.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/kerbalstuff/1";
            json["ksp_version_min"] = "0.23.5";

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(null, (string)transformedJson["ksp_version"]);
            Assert.AreEqual(null, (string)transformedJson["ksp_version_max"]);
            Assert.AreEqual("0.23.5", (string)transformedJson["ksp_version_min"]);
        }

        private static KerbalstuffMod MakeTestMod()
        {
            var ksmod = new KerbalstuffMod
            {
                license = "CC-BY",
                name = "Dogecoin Flag",
                short_description = "Such test. Very unit. Wow.",
                author = "pjf",
                versions = new KSVersion[1]
            };

            ksmod.versions[0] = new KSVersion
            {
                friendly_version = new CKAN.Version("0.25"),
                download_path = new Uri("http://example.com/")
            };

            return ksmod;
        }
    }
}
