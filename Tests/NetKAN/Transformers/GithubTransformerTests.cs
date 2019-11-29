using System;
using System.Linq;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN;
using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class GithubTransformerTests
    {
        private TransformOptions opts = new TransformOptions(1, null);

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

            mApi.Setup(i => i.GetAllReleases(It.IsAny<GithubRef>()))
                .Returns(new GithubRelease[] { new GithubRelease(
                    "ExampleProject",
                    new ModuleVersion("1.0"),
                    new Uri("http://github.example/download"),
                    null
                )});

            var sut = new GithubTransformer(mApi.Object, false);

            // Act
            var result = sut.Transform(new Metadata(json), opts).First();
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

            mApi.Setup(i => i.GetAllReleases(It.IsAny<GithubRef>()))
                .Returns(new GithubRelease[] { new GithubRelease(
                    "DestructionEffects",
                    new ModuleVersion("v1.8,0"),
                    new Uri("https://github.com/jrodrigv/DestructionEffects/releases/download/v1.8%2C0/DestructionEffects.1.8.0_0412018.zip"),
                    null
                )});

            ITransformer sut = new GithubTransformer(mApi.Object, false);

            // Act
            Metadata result = sut.Transform(new Metadata(json), opts).First();
            JObject transformedJson = result.Json();

            // Assert
            Assert.AreEqual(
                "https://github.com/jrodrigv/DestructionEffects/releases/download/v1.8%2C0/DestructionEffects.1.8.0_0412018.zip",
                (string)transformedJson["download"]
            );
        }

    }
}
