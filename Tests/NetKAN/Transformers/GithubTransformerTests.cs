using System;
using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Transformers;
using CKAN.Versioning;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class GithubTransformerTests
    {
        [Test]
        public void CalculatesRepositoryUrlCorrectly()
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/github/ExampleAccount/ExampleProject";

            var mApi = new Mock<IGithubApi>();
            mApi.Setup(i => i.GetRepo(It.IsAny<GithubRef>()))
                .Returns(new GithubRepo
                {
                    HtmlUrl = "https://github.com/ExampleAccount/ExampleProject"
                });

            mApi.Setup(i => i.GetLatestRelease(It.IsAny<GithubRef>()))
                .Returns(new GithubRelease(
                    "ExampleProject",
                    new ModuleVersion("1.0"),
                    new Uri("http://github.example/download"),
                    null
                ));

            var sut = new GithubTransformer(mApi.Object, matchPreleases: false);

            // Act
            var result = sut.Transform(new Metadata(json));
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(
                "https://github.com/ExampleAccount/ExampleProject",
                (string)transformedJson["resources"]["repository"]
            );
        }

        [Test]
        public void Transform_DownloadURLWithEncodedCharacter_DontDoubleEncode()
        {
            // Arrange
            JObject json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/github/jrodrigv/DestructionEffects";

            var mApi = new Mock<IGithubApi>();
            mApi.Setup(i => i.GetRepo(It.IsAny<GithubRef>()))
                .Returns(new GithubRepo()
                {
                    HtmlUrl = "https://github.com/jrodrigv/DestructionEffects"
                });

            mApi.Setup(i => i.GetLatestRelease(It.IsAny<GithubRef>()))
                .Returns(new GithubRelease(
                    "DestructionEffects",
                    new ModuleVersion("v1.8,0"),
                    new Uri("https://github.com/jrodrigv/DestructionEffects/releases/download/v1.8%2C0/DestructionEffects.1.8.0_0412018.zip"),
                    null
                ));

            ITransformer sut = new GithubTransformer(mApi.Object, matchPreleases: false);

            // Act
            Metadata result = sut.Transform(new Metadata(json));
            JObject transformedJson = result.Json();

            // Assert
            Assert.AreEqual(
                "https://github.com/jrodrigv/DestructionEffects/releases/download/v1.8%2C0/DestructionEffects.1.8.0_0412018.zip",
                (string)transformedJson["download"]
            );
        }

    }
}
