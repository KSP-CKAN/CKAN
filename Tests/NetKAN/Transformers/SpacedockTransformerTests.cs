using System;
using System.Linq;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Spacedock;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class SpacedockTransformerTests
    {
        private readonly TransformOptions opts = new TransformOptions(1, null, null, false, null);

        // GH #199: Don't pre-fill KSP version fields if we see a ksp_min/max
        [Test]
        public void DoesNotReplaceGameVersionProperties()
        {
            // Arrange
            var mApi = new Mock<ISpacedockApi>();
            mApi.Setup(i => i.GetMod(It.IsAny<int>()))
                .Returns(new SpacedockMod()
                {
                    name              = "Dogecoin Flag",
                    short_description = "Such test. Very unit. Wow.",
                    author            = "pjf",
                    license           = "CC-BY",
                    versions          = new SDVersion[1]
                    {
                        new SDVersion()
                        {
                            friendly_version = new ModuleVersion("0.25"),
                            download_path    = new Uri("http://example.com/")
                        }
                    }
                });

            var mGhApi = new Mock<IGithubApi>();
            mGhApi.Setup(i => i.GetRepo(It.IsAny<GithubRef>()))
                .Returns(new GithubRepo
                {
                    HtmlUrl = "https://github.com/ExampleAccount/ExampleProject"
                });

            ITransformer sut = new SpacedockTransformer(mApi.Object, mGhApi.Object);

            JObject json            = new JObject();
            json["spec_version"]    = 1;
            json["$kref"]           = "#/ckan/spacedock/1";
            json["ksp_version_min"] = "0.23.5";

            // Act
            Metadata result          = sut.Transform(new Metadata(json), opts).First();
            JObject  transformedJson = result.Json();

            // Assert
            Assert.AreEqual(null,     (string)transformedJson["ksp_version"]);
            Assert.AreEqual(null,     (string)transformedJson["ksp_version_max"]);
            Assert.AreEqual("0.23.5", (string)transformedJson["ksp_version_min"]);
        }

    }
}
