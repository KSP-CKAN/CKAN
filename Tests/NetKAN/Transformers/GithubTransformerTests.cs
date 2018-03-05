using System;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Transformers;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Version = CKAN.Version;

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
                    new Version("1.0"),
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
    }
}
