using System;
using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Moq;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.Curse;
using CKAN.NetKAN.Transformers;
using CKAN.NetKAN.Services;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class CurseTransformerTests
    {
        [Test]
        public void Transform_HasGameVersion_DoesNotReplace()
        {
            // Arrange
            var http = new Mock<IHttpService>();
            http.Setup(h => h.DownloadText(It.IsAny<Uri>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns(@"{
                             ""title"":       ""Dogecoin Flag"",
                             ""description"": ""Such test. Very unit. Wow."",
                             ""license"":     ""CC-BY"",
                             ""members"": [
                                 {
                                     ""username"": ""pjf""
                                 }
                             ],
                             ""files"": [
                                 {
                                     ""version"": ""0.25"",
                                     ""url"":     ""http://example.com/download.zip""
                                 }
                             ]
                         }");

            var sut      = new CurseTransformer(new CurseApi(http.Object, null));
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "spec_version",    1                },
                { "$kref",           "#/ckan/curse/1" },
                { "ksp_version_min", "0.23.5"         },
            });

            // Act
            var result          = sut.Transform(metadata, opts).First();
            var transformedJson = result.Json();

            // Assert
            Assert.AreEqual(null,     (string?)transformedJson["ksp_version"],
                            "ksp_version should not be filled if ksp_version_min is already defined");
            Assert.AreEqual(null,     (string?)transformedJson["ksp_version_max"],
                            "ksp_version_max should not be filled if ksp_version_min is already defined");
            Assert.AreEqual("0.23.5", (string?)transformedJson["ksp_version_min"]);
        }
    }
}
