using System;
using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Sources.Jenkins;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class JenkinsTransformerTests
    {
        [Test]
        public void Transform_WithBuilds_Works()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.EndsWith("/api/json")),
                                           It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns((Uri u, string? _, string? _) => $@"{{
                                            ""builds"": [
                                                {{
                                                    ""number"": 3,
                                                    ""url"":    ""{u.ToString().Replace("/api/json", "")}/3/""
                                                }},
                                                {{
                                                    ""number"": 2,
                                                    ""url"":    ""{u.ToString().Replace("/api/json", "")}/2/""
                                                }},
                                                {{
                                                    ""number"": 1,
                                                    ""url"":    ""{u.ToString().Replace("/api/json", "")}/1/""
                                                }}
                                            ]
                                        }}");
            http.Setup(h => h.DownloadText(It.Is<Uri>(u => u.AbsolutePath.EndsWith("1/api/json")
                                                           || u.AbsolutePath.EndsWith("2/api/json")
                                                           || u.AbsolutePath.EndsWith("3/api/json")), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns((Uri u, string? _, string? _) => $@"{{
                                        ""result"": ""SUCCESS"",
                                        ""url"":    ""{u.ToString().Replace("api/json", "")}"",
                                        ""artifacts"": [
                                            {{
                                                ""fileName"":     ""Project.zip"",
                                                ""relativePath"": ""Project.zip""
                                            }},
                                            {{
                                                ""fileName"":     ""Project.dll"",
                                                ""relativePath"": ""Project.dll""
                                            }}
                                        ],
                                        ""timestamp"": {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}
                                    }}");

            var sut      = new JenkinsTransformer(new JenkinsApi(http.Object));
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "identifier", "FakeMod"                                                     },
                { "$kref",      "#/ckan/jenkins/https://fake-server.com/jenkins/job/Project/" },
                {
                    "x_netkan_jenkins",
                    new JObject()
                    {
                        { "use_filename_version", true }
                    }
                }
            });

            // Act
            var result = sut.Transform(metadata, opts).First();

            // Assert
            Assert.IsNull(result.Kref);
            Assert.AreEqual(Enumerable.Repeat(new Uri("https://fake-server.com/jenkins/job/Project/3/artifact/Project.zip"), 1),
                            result.Download);
            Assert.IsNotNull(result.ReleaseDate);
            Assert.IsNotNull(result.Resources);
            Assert.IsTrue(result.Resources!.ContainsKey("ci"));
        }
    }
}
