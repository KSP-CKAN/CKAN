using System;
using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Services;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class HttpTransformerTests
    {
        [TestCase("#/ckan/github/foo/bar")]
        [TestCase("#/ckan/netkan/http://awesomemod.example/awesomemod.netkan")]
        [TestCase("#/ckan/spacedock/1")]
        [TestCase("#/ckan/curse/1")]
        [TestCase("#/ckan/foo")]
        public void Transform_NonMatching_NoChanges(string kref)
        {
            // Arrange
            var http = new Mock<IHttpService>();
            var sut  = new HttpTransformer(http.Object);
            var opts = new TransformOptions(1, null, null, null, false, null);
            var json = new JObject()
            {
                { "spec_version", 1    },
                { "$kref",        kref },
            };
            var metadata = new Metadata(json);

            // Act
            var result          = sut.Transform(metadata, opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.That(transformedJson, Is.EqualTo(json),
                        "HttpTransformer should not alter the metatadata when it does not match the $kref.");
        }

        [Test]
        public void Transform_HttpKref_ResolvesRedirect()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.ResolveRedirect(It.IsAny<Uri>(), It.IsAny<string?>()))
                .Returns(new Uri("https://fake-web-site.com/redirected"));
            var sut      = new HttpTransformer(http.Object);
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version", 1                                                           },
                { "$kref",        "#/ckan/http/https://fake-web-site.com/fakepathpreredirect" },
            });

            // Act
            var result = sut.Transform(metadata, opts).First();

            // Assert
            Assert.AreEqual("https://fake-web-site.com/redirected",
                            result.Download?.First().ToString());
        }
    }
}
