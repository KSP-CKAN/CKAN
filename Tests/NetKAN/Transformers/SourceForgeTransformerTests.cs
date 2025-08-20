using System;
using System.Linq;

using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Sources.SourceForge;
using CKAN.NetKAN.Services;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class SourceForgeTransformerTests
    {
        [Test]
        public void Transform_ExampleMod_Works()
        {
            // Arrange
            var http     = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.IsAny<Uri>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns($@"<?xml version=""1.0"" encoding=""utf-8""?>
                            <rss xmlns:content=""http://purl.org/rss/1.0/modules/content/"" version=""2.0"">
                              <channel>
                                <title>Test Mod</title>
                                <description>A hypothetical mod on SourceForge</description>
                                <pubDate>{DateTime.UtcNow:r}</pubDate>
                                <item>
                                  <title>Download-1.0.0.zip</title>
                                  <link>https://sourceforge.net/download</link>
                                  <pubDate>{DateTime.UtcNow:r}</pubDate>
                                </item>
                              </channel>
                            </rss>");
            var sut      = new SourceForgeTransformer(new SourceForgeApi(http.Object));
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "identifier", "FakeMod"                    },
                { "$kref",      "#/ckan/sourceforge/testmod" }
            });

            // Act
            var result = sut.Transform(metadata, opts).First();

            // Assert
            Assert.IsNull(result.Kref);
            Assert.IsNotNull(result.ReleaseDate);
            Assert.AreEqual("Test Mod",
                            (string?)result.AllJson["name"]);
            Assert.AreEqual(new Uri("https://sourceforge.net/download?use_mirror=master"),
                            result.Download?.First());
            CollectionAssert.IsSupersetOf(result.Resources!.Keys,
                                          new string[] { "repository", "bugtracker", "homepage" });
        }
    }
}
