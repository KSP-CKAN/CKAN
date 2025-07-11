using System;
using System.Linq;

using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class GithubTransformerTests
    {
        [OneTimeSetUp]
        public void SetupMockUp()
        {
            httpSvcMockUp.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.StartsWith("/repos/")
                                                                    && !u.AbsolutePath.EndsWith("/releases")),
                                                    It.IsAny<string?>(), It.IsAny<string?>()))
                         .Returns(@"{
                                      ""name"": ""Example Project"",
                                      ""owner"": {
                                          ""login"": ""authoruser"",
                                          ""type"":  ""User""
                                      },
                                      ""license"": {
                                          ""spdx_id"": ""GPL-3.0""
                                      },
                                      ""html_url"": ""https://github.com/ExampleAccount/ExampleProject"",
                                  }");
            httpSvcMockUp.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.StartsWith("/repos/")
                                                                    && u.AbsolutePath.EndsWith("/releases")),
                                                    It.IsAny<string?>(), It.IsAny<string?>()))
                         .Returns(@"[
                                      {
                                          ""author"": {
                                              ""login"": ""ExampleProject""
                                          },
                                          ""tag_name"": ""1.0"",
                                          ""published_at"": ""2025-01-05T00:00:00Z"",
                                          ""assets"": [
                                              {
                                                  ""name"":                 ""download.zip"",
                                                  ""browser_download_url"": ""http://github.example/download/1.0""
                                              }
                                          ]
                                      },
                                      {
                                          ""author"": {
                                              ""login"": ""ExampleProject""
                                          },
                                          ""tag_name"": ""1.1"",
                                          ""published_at"": ""2025-01-04T00:00:00Z"",
                                          ""assets"": [
                                              {
                                                  ""name"":                 ""download.zip"",
                                                  ""browser_download_url"": ""http://github.example/download/1.1""
                                              }
                                          ]
                                      },
                                      {
                                          ""author"": {
                                              ""login"": ""ExampleProject""
                                          },
                                          ""tag_name"": ""1.2"",
                                          ""published_at"": ""2025-01-03T00:00:00Z"",
                                          ""assets"": [
                                              {
                                                  ""name"":                 ""ExampleProject_1.2-1.8.1.zip"",
                                                  ""browser_download_url"": ""http://github.example/download/1.2/ExampleProject_1.2-1.8.1.zip""
                                              }
                                          ]
                                      },
                                      {
                                          ""author"": {
                                              ""login"": ""ExampleProject""
                                          },
                                          ""tag_name"": ""1.3"",
                                          ""published_at"": ""2025-01-02T00:00:00Z"",
                                          ""assets"": [
                                              {
                                                  ""name"":                 ""ExampleProject_1.2-1.8.1.zip"",
                                                  ""browser_download_url"": ""http://github.example/download/1.2/ExampleProject_1.2-1.8.1.zip""
                                              },
                                              {
                                                  ""name"":                 ""ExampleProject_1.2-1.9.1.zip"",
                                                  ""browser_download_url"": ""http://github.example/download/1.2/ExampleProject_1.2-1.9.1.zip""
                                              }
                                          ]
                                      },
                                      {
                                          ""author"": {
                                              ""login"": ""DestructionEffects""
                                          },
                                          ""tag_name"": ""v1.8,0"",
                                          ""published_at"": ""2025-01-01T00:00:00Z"",
                                          ""assets"": [
                                              {
                                                  ""name"":                 ""DestructionEffects.1.8.0_0412018.zip"",
                                                  ""browser_download_url"": ""https://github.com/jrodrigv/DestructionEffects/releases/download/v1.8%2C0/DestructionEffects.1.8.0_0412018.zip""
                                              }
                                          ]
                                      }
                                  ]");
        }

        [Test]
        public void Transform_ExampleProject_SetsRepositoryResource()
        {
            // Arrange
            var sut      = new GithubTransformer(new GithubApi(httpSvcMockUp.Object), false);
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                             },
                { "identifier",   "ExampleProject1"                             },
                { "$kref",        "#/ckan/github/ExampleAccount/ExampleProject" },
            });

            // Act
            var result          = sut.Transform(metadata, opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual("https://github.com/ExampleAccount/ExampleProject",
                            (string?)transformedJson["resources"]?["repository"]);
        }

        [Test]
        public void Transform_DownloadURLWithEncodedCharacter_DontDoubleEncode()
        {
            // Arrange
            var sut      = new GithubTransformer(new GithubApi(httpSvcMockUp.Object), false);
            var opts     = new TransformOptions(1, 4, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                           },
                { "identifier",   "ExampleProject2"                           },
                { "$kref",        "#/ckan/github/jrodrigv/DestructionEffects" },
            });

            // Act
            var result          = sut.Transform(metadata, opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(
                "https://github.com/jrodrigv/DestructionEffects/releases/download/v1.8%2C0/DestructionEffects.1.8.0_0412018.zip",
                (string?)transformedJson["download"]);
        }

        [Test]
        public void Transform_MultipleReleases_TransformsAll()
        {
            // Arrange
            var sut      = new GithubTransformer(new GithubApi(httpSvcMockUp.Object), false);
            var opts     = new TransformOptions(2, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                             },
                { "identifier",   "ExampleProject3"                             },
                { "$kref",        "#/ckan/github/ExampleAccount/ExampleProject" },
            });

            // Act
            var results = sut.Transform(metadata, opts);
            var transformedJsons = results.Select(result => result.Json()).ToArray();

            // Assert
            Assert.AreEqual("http://github.example/download/1.0",
                            (string?)transformedJsons[0]["download"]);
            Assert.AreEqual("http://github.example/download/1.1",
                            (string?)transformedJsons[1]["download"]);

            Assert.AreEqual("1.0",
                            (string?)transformedJsons[0]["x_netkan_version_pieces"]?["tag"]);
            Assert.AreEqual("1.1",
                            (string?)transformedJsons[1]["x_netkan_version_pieces"]?["tag"]);
        }

        [Test]
        public void Transform_MultipleAssets_TransformsAll()
        {
            // Arrange
            var sut      = new GithubTransformer(new GithubApi(httpSvcMockUp.Object), false);
            var opts     = new TransformOptions(1, 1, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                                                                          },
                { "identifier",   "ExampleProject4"                                                                          },
                { "$kref",        "#/ckan/github/ExampleAccount/ExampleProject/version_from_asset/^.+_(?<version>.+)\\.zip$" },
            });

            // Act
            var results = sut.Transform(metadata, opts);
            var transformedJsons = results.Select(result => result.Json()).ToArray();

            // Assert
            Assert.AreEqual("http://github.example/download/1.2/ExampleProject_1.2-1.8.1.zip",
                            (string?)transformedJsons[0]["download"]);
            Assert.AreEqual("http://github.example/download/1.2/ExampleProject_1.2-1.9.1.zip",
                            (string?)transformedJsons[1]["download"]);

            Assert.AreEqual("1.2-1.8.1",
                            (string?)transformedJsons[0]["version"]);
            Assert.AreEqual("1.2-1.9.1",
                            (string?)transformedJsons[1]["version"]);
        }

        [Test]
        public void Transform_SkipReleases_SkipsCorrectly()
        {
            // Arrange
            var sut      = new GithubTransformer(new GithubApi(httpSvcMockUp.Object), false);
            var opts     = new TransformOptions(2, 1, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                             },
                { "identifier",   "ExampleProject5"                             },
                { "$kref",        "#/ckan/github/ExampleAccount/ExampleProject" },
            });

            // Act
            var results = sut.Transform(metadata, opts).ToArray();

            // Assert
            Assert.AreEqual("1.1",
                            results[0]?.Version?.ToString());
            Assert.AreEqual("1.2",
                            results[1]?.Version?.ToString());
        }

        private readonly Mock<IHttpService> httpSvcMockUp = new Mock<IHttpService>();
    }
}
