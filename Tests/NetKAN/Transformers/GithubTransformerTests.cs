using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class GithubTransformerTests
    {
        private readonly TransformOptions opts = new TransformOptions(1, null, null, false, null);

        private Mock<IGithubApi> apiMockUp;
        [OneTimeSetUp]
        public void setupApiMockup()
        {
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
                    new List<GithubReleaseAsset>
                    {
                        new GithubReleaseAsset(
                            "download",
                            new Uri("http://github.example/download"),
                            null
                        )
                    }
                ));

            mApi.Setup(i => i.GetAllReleases(It.IsAny<GithubRef>()))
                .Returns(new GithubRelease[] {
                    new GithubRelease(
                        "ExampleProject",
                        new ModuleVersion("1.0"),
                        new List<GithubReleaseAsset>
                        {
                            new GithubReleaseAsset(
                                "download",
                                new Uri("http://github.example/download/1.0"),
                                null
                            )
                        }
                    ),
                    new GithubRelease("ExampleProject",
                        new ModuleVersion("1.1"),
                        new List<GithubReleaseAsset>
                        {
                            new GithubReleaseAsset(
                                "download",
                                new Uri("http://github.example/download/1.1"),
                                null
                            )
                        }
                    ),
                    new GithubRelease("ExampleProject",
                        new ModuleVersion("1.2"),
                        new List<GithubReleaseAsset>
                        {
                            new GithubReleaseAsset(
                                "ExampleProject_1.2-1.8.1.zip",
                                new Uri("http://github.example/download/1.2/ExampleProject_1.2-1.8.1.zip"),
                                null
                            )
                        }
                    ),
                    new GithubRelease("ExampleProject",
                        new ModuleVersion("1.3"),
                        new List<GithubReleaseAsset>
                        {
                            new GithubReleaseAsset(
                                "ExampleProject_1.2-1.8.1.zip",
                                new Uri("http://github.example/download/1.2/ExampleProject_1.2-1.8.1.zip"),
                                null
                            ),
                            new GithubReleaseAsset(
                                "ExampleProject_1.2-1.9.1.zip",
                                new Uri("http://github.example/download/1.2/ExampleProject_1.2-1.9.1.zip"),
                                null
                            )
                        }
                    ),
                });

            apiMockUp = mApi;
        }

        [Test]
        public void CalculatesRepositoryUrlCorrectly()
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/github/ExampleAccount/ExampleProject";
            json["identifier"] = "ExampleProject1";

            var sut = new GithubTransformer(apiMockUp.Object, false);

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
            json["identifier"] = "ExampleProject2";

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
                    new List<GithubReleaseAsset>
                    {
                        new GithubReleaseAsset(
                            "DestructionEffects.1.8.0_0412018.zip",
                            new Uri("https://github.com/jrodrigv/DestructionEffects/releases/download/v1.8%2C0/DestructionEffects.1.8.0_0412018.zip"),
                            null
                        )
                    }
                ));

            mApi.Setup(i => i.GetAllReleases(It.IsAny<GithubRef>()))
                .Returns(new GithubRelease[] { new GithubRelease(
                    "DestructionEffects",
                    new ModuleVersion("v1.8,0"),
                    new List<GithubReleaseAsset>
                    {
                        new GithubReleaseAsset(
                            "DestructionEffects.1.8.0_0412018.zip",
                            new Uri("https://github.com/jrodrigv/DestructionEffects/releases/download/v1.8%2C0/DestructionEffects.1.8.0_0412018.zip"),
                            null
                        )
                    }
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

        [Test]
        public void Transform_MultipleReleases_TransformsAll()
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/github/ExampleAccount/ExampleProject";
            json["identifier"] = "ExampleProject3";

            var sut = new GithubTransformer(apiMockUp.Object, false);

            // Act
            var results = sut.Transform(
                new Metadata(json),
                new TransformOptions(2, null, null, false, null)
            );
            var transformedJsons = results.Select(result => result.Json()).ToArray();

            // Assert
            Assert.AreEqual(
                "http://github.example/download/1.0",
                (string)transformedJsons[0]["download"]
            );
            Assert.AreEqual(
                "http://github.example/download/1.1",
                (string)transformedJsons[1]["download"]
            );

            Assert.AreEqual(
                "1.0",
                (string)transformedJsons[0]["x_netkan_version_pieces"]["tag"]
            );
            Assert.AreEqual(
                "1.1",
                (string)transformedJsons[1]["x_netkan_version_pieces"]["tag"]
            );
        }

        [Test]
        public void Transform_MultipleAssets_TransformsAll()
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/github/ExampleAccount/ExampleProject/version_from_asset/^.+_(?<version>.+)\\.zip$";
            json["identifier"] = "ExampleProject4";

            var sut = new GithubTransformer(apiMockUp.Object, false);

            // Act
            var results = sut.Transform(
                new Metadata(json),
                new TransformOptions(1, 3, null, false, null)
            );
            var transformedJsons = results.Select(result => result.Json()).ToArray();

            // Assert
            Assert.AreEqual(
                "http://github.example/download/1.2/ExampleProject_1.2-1.8.1.zip",
                (string)transformedJsons[0]["download"]
            );
            Assert.AreEqual(
                "http://github.example/download/1.2/ExampleProject_1.2-1.9.1.zip",
                (string)transformedJsons[1]["download"]
            );

            Assert.AreEqual(
                "1.2-1.8.1",
                (string)transformedJsons[0]["version"]
            );
            Assert.AreEqual(
                "1.2-1.9.1",
                (string)transformedJsons[1]["version"]
            );
        }

        [Test]
        public void Transform_SkipReleases_SkipsCorrectly()
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["$kref"] = "#/ckan/github/ExampleAccount/ExampleProject";
            json["identifier"] = "ExampleProject5";

            var sut = new GithubTransformer(apiMockUp.Object, false);

            // Act
            var results = sut.Transform(
                new Metadata(json),
                new TransformOptions(2, 1, null, false, null)
            ).ToArray();

            // Assert
            Assert.AreEqual(
                "1.1",
                results[0].Version.ToString()
            );
            Assert.AreEqual(
                "1.2",
                results[1].Version.ToString()
            );
        }
    }
}
