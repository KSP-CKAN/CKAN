using System;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Moq;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class AvcKrefTransformerTests
    {
        private TransformOptions opts = new TransformOptions(1, null, null);

        [Test,
            TestCase(
                "https://mysite.org/AwesomeMod.version",
                "1.0.0",
                "1.4.1",
                "https://mysite.org/AwesomeMod.zip",
                @"{
                    ""NAME"":     ""AwesomeMod"",
                    ""URL"":      ""https://mysite.org/AwesomeMod.version"",
                    ""DOWNLOAD"": ""https://mysite.org/AwesomeMod.zip"",
                    ""VERSION"": {
                        ""MAJOR"": 1,
                        ""MINOR"": 0,
                        ""PATCH"": 0
                    },
                    ""KSP_VERSION"": {
                        ""MAJOR"": 1,
                        ""MINOR"": 4,
                        ""PATCH"": 1
                    }
                }"
            )
        ]
        public void Transform_SimpleVersionFile_PropertiesSet(string remoteUrl, string version, string kspVersion, string download, string remoteAvc)
        {
            // Arrange
            var mHttp = new Mock<IHttpService>();
            mHttp.Setup(i => i.DownloadText(It.IsAny<Uri>()))
                .Returns(remoteAvc);

            // Act
            Metadata m = null;
            Assert.DoesNotThrow(() => m = TryKref(mHttp.Object, $"#/ckan/ksp-avc/{remoteUrl}"));

            // Assert
            var json = m.Json();
            Assert.AreEqual((string)json["version"],     version);
            Assert.AreEqual((string)json["ksp_version"], kspVersion);
            Assert.AreEqual((string)json["download"],    download);
        }

        private Metadata TryKref(IHttpService http, string kref)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"]   = "AwesomeMod";
            if (kref != null)
            {
                json["$kref"] = kref;
            }

            // Act
            var tran = new AvcKrefTransformer(http, null);
            return tran.Transform(new Metadata(json), opts).First();
        }

    }
}
