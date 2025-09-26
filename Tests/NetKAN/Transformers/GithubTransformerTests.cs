using System;
using System.Linq;

using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN;
using CKAN.Versioning;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Github;
using CKAN.NetKAN.Transformers;
using Tests.Data;

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
                                      ""description"": ""A fake project for testing"",
                                      ""owner"": {
                                          ""login"": ""authoruser"",
                                          ""type"":  ""User""
                                      },
                                      ""license"": {
                                          ""spdx_id"": ""GPL-3.0""
                                      },
                                      ""html_url"": ""https://github.com/ExampleAccount/ExampleProject"",
                                      ""homepage"": ""https://exampleproject.com/"",
                                      ""has_issues"": true,
                                      ""has_discussions"": true,
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
        public void Transform_ExampleProject_SetsResources()
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
            var result          = sut.Transform(metadata, opts).Single();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual("https://github.com/ExampleAccount/ExampleProject",
                            (string?)transformedJson["resources"]?["repository"]);
            Assert.AreEqual("https://exampleproject.com/",
                            (string?)transformedJson["resources"]?["homepage"]);
            Assert.AreEqual("https://github.com/ExampleAccount/ExampleProject/issues",
                            (string?)transformedJson["resources"]?["bugtracker"]);
            Assert.AreEqual("https://github.com/ExampleAccount/ExampleProject/discussions",
                            (string?)transformedJson["resources"]?["discussions"]);
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
            var result          = sut.Transform(metadata, opts).Single();
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

        [Test]
        public void Transform_NoRepo_Throws()
        {
            // Arrange
            var ghApi = new Mock<IGithubApi>();
            ghApi.Setup(gh => gh.GetRepo(It.IsAny<GithubRef>()))
                 .Returns((GithubRepo?)null);
            var sut      = new GithubTransformer(ghApi.Object, null);
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                             },
                { "identifier",   "ExampleProject6"                             },
                { "$kref",        "#/ckan/github/ExampleAccount/ExampleProject" },
            });

            // Act / Assert
            var exc = Assert.Throws<Kraken>(() => sut.Transform(metadata, opts).ToArray())!;

            // Assert
            Assert.AreEqual("Failed to get GitHub repo info!", exc.Message);
        }

        [Test]
        public void Transform_RepoArchived_Warns()
        {
            // Arrange
            var ghApi = new Mock<IGithubApi>();
            ghApi.Setup(gh => gh.GetRepo(It.IsAny<GithubRef>()))
                 .Returns(new GithubRepo() { Archived = true });
            ghApi.Setup(gh => gh.GetAllReleases(It.IsAny<GithubRef>(), It.IsAny<bool?>()))
                 .Returns(new GithubRelease[]
                          {
                              new GithubRelease()
                              {
                                  Tag    = new ModuleVersion("1.0"),
                                  Assets = new GithubReleaseAsset[]
                                           {
                                               new GithubReleaseAsset()
                                               {
                                                   Name     = "file.zip",
                                                   Download = new Uri("https://examplemod.com/download"),
                                               },
                                           },
                              },
                          });
            var sut      = new GithubTransformer(ghApi.Object, null);
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                             },
                { "identifier",   "ExampleProject7"                             },
                { "$kref",        "#/ckan/github/ExampleAccount/ExampleProject" },
            });

            using (var appender = new TemporaryWarningCapturer(nameof(GithubTransformer)))
            {
                // Act / Assert
                var results = sut.Transform(metadata, opts).ToArray();

                // Assert
                Assert.AreEqual("Repo is archived, consider freezing",
                                appender.Warnings.Single());
            }
        }

        [Test]
        public void Transform_UseSourceArchive_Works()
        {
            // Arrange
            var download = new Uri("https://examplemod.com/source");
            var ghApi    = new Mock<IGithubApi>();
            ghApi.Setup(gh => gh.GetRepo(It.IsAny<GithubRef>()))
                 .Returns(new GithubRepo());
            ghApi.Setup(gh => gh.GetAllReleases(It.IsAny<GithubRef>(), It.IsAny<bool?>()))
                 .Returns(new GithubRelease[]
                          {
                              new GithubRelease()
                              {
                                  Tag           = new ModuleVersion("1.0"),
                                  SourceArchive = download,
                              },
                          });
            var sut      = new GithubTransformer(ghApi.Object, null);
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version",    1                                                },
                { "identifier",      "ExampleProject8"                                },
                { "$kref",           "#/ckan/github/ExampleAccount/ExampleProject"    },
                { "x_netkan_github", new JObject() { { "use_source_archive", true } } },
            });

            // Act / Assert
            var result = sut.Transform(metadata, opts).Single();

            // Assert
            CollectionAssert.AreEqual(new Uri[] { download }, result.Download);
        }

        [Test]
        public void Transform_VersionFromAssetMissingCapturingGroup_Throws()
        {
            // Arrange
            var sut      = new GithubTransformer(new GithubApi(httpSvcMockUp.Object), false);
            var opts     = new TransformOptions(1, 1, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                                                                          },
                { "identifier",   "ExampleProject9"                                                                          },
                { "$kref",        "#/ckan/github/ExampleAccount/ExampleProject/version_from_asset/^.+_(?<name>.+)\\.zip$" },
            });

            // Act / Assert
            var exc = Assert.Throws<Kraken>(() =>
            {
                var results = sut.Transform(metadata, opts).ToArray();
            })!;

            // Assert
            Assert.AreEqual("version_from_asset contains no 'version' capturing group", exc.Message);
        }

        [Test]
        public void Transform_NoReleases_Warns()
        {
            // Arrange
            var ghApi = new Mock<IGithubApi>();
            ghApi.Setup(gh => gh.GetRepo(It.IsAny<GithubRef>()))
                 .Returns(new GithubRepo());
            ghApi.Setup(gh => gh.GetAllReleases(It.IsAny<GithubRef>(), It.IsAny<bool?>()))
                 .Returns(new GithubRelease[] { });
            var sut      = new GithubTransformer(ghApi.Object, null);
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                             },
                { "identifier",   "ExampleProject10"                            },
                { "$kref",        "#/ckan/github/ExampleAccount/ExampleProject" },
            });

            using (var appender = new TemporaryWarningCapturer(nameof(GithubTransformer)))
            {
                // Act / Assert
                var results = sut.Transform(metadata, opts).ToArray();

                // Assert
                Assert.AreEqual("No releases found for ExampleAccount/ExampleProject",
                                appender.Warnings.Single());
            }
        }

        private readonly Mock<IHttpService> httpSvcMockUp = new Mock<IHttpService>();
    }
}
