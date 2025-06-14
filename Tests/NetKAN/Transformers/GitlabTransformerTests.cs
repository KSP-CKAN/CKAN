using System;
using System.Linq;

using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Sources.Gitlab;
using CKAN.Versioning;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class GitlabTransformerTests
    {
        [Test]
        public void Transform_ExampleMod_Works()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.Host == "gitlab.com"
                                                           && u.AbsolutePath.StartsWith("/api/v4/projects/")
                                                           && !u.AbsolutePath.EndsWith("/releases")),
                                           It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns((Uri u, string? token, string? mimeType) => $@"{{
                                        ""name"":           ""A Mod"",
                                        ""description"":    ""A simulated mod on GitLab"",
                                        ""web_url"":        ""https://gitlab.com/homepage/"",
                                        ""issues_enabled"": true,
                                        ""readme_url"":     ""https://gitlab.com/readme/""
                                    }}");
            http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.Host == "gitlab.com"
                                                           && u.AbsolutePath.StartsWith("/api/v4/projects/")
                                                           && u.AbsolutePath.EndsWith("/releases")),
                                           It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns((Uri u, string? token, string? mimeType) => $@"[
                                        {{
                                            ""tag_name"":    ""v1.0.0"",
                                            ""released_at"": ""{DateTime.UtcNow}"",
                                            ""author"": {{
                                                ""name"": ""A Modder""
                                            }},
                                            ""assets"": {{
                                                ""count"": 1,
                                                ""sources"": [
                                                    {{
                                                        ""format"": ""zip"",
                                                        ""url"":    ""https://gitlab.com/download""
                                                    }}
                                                ]
                                            }}
                                        }}
                                    ]");

            var sut      = new GitlabTransformer(new GitlabApi(http.Object));
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "identifier", "FakeMod"                    },
                { "$kref",      "#/ckan/gitlab/AModder/AMod" },
                {
                    "x_netkan_gitlab",
                    new JObject()
                    {
                        { "use_source_archive", true }
                    }
                }
            });

            // Act
            var result = sut.Transform(metadata, opts).First();

            // Assert
            Assert.IsNull(result.Kref);
            Assert.AreEqual("A Mod",                                (string?)result.AllJson["name"]);
            Assert.AreEqual(new Uri("https://gitlab.com/download"), result.Download?.First());
            Assert.AreEqual(new ModuleVersion("v1.0.0"),            result.Version);
            CollectionAssert.IsSupersetOf(result.Resources!.Keys,
                                          new string[] { "repository", "bugtracker", "manual" });
        }
    }
}
