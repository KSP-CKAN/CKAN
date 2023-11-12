using System.Collections.Generic;
using System.Linq;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Curse;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class CurseTransformerTests
    {
        private readonly TransformOptions opts = new TransformOptions(1, null, null, false, null);

        // GH #199: Don't pre-fill KSP version fields if we see a ksp_min/max
        [Test]
        public void DoesNotReplaceGameVersionProperties()
        {
            // Arrange
            var mApi = new Mock<ICurseApi>();
            mApi.Setup(i => i.GetMod(It.IsAny<string>()))
                .Returns(MakeTestMod());

            var sut = new CurseTransformer(mApi.Object);

            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/curse/1";
            json["ksp_version_min"] = "0.23.5";

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(null, (string)transformedJson["ksp_version"]);
            Assert.AreEqual(null, (string)transformedJson["ksp_version_max"]);
            Assert.AreEqual("0.23.5", (string)transformedJson["ksp_version_min"]);
        }

        private static CurseMod MakeTestMod()
        {
            var cmod = new CurseMod
            {
                license = "CC-BY",
                title = "Dogecoin Flag",
                description = "Such test. Very unit. Wow.",
                members = new List<CurseModMember> { new CurseModMember { username = "pjf" } },
                files = new List<CurseFile>() { new CurseFile() }
            };
            cmod.files[0].SetFileVersion("0.25");
            cmod.files[0].SetDownloadUrl("http://example.com/download.zip");

            return cmod;
        }
    }
}
