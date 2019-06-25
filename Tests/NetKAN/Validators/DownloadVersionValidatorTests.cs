using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class DownloadVersionValidatorTests
    {
        [Test,
            TestCase("https://mysite.org/mymod.zip", "1.2.3"),
            TestCase(null, null),
            TestCase(null, "4.5.6"),
        ]
        public void Validate_NoDownloadWithoutVersion_DoesNotThrow(string download, string version)
        {
            Assert.DoesNotThrow(() => TryDownloadVersion(download, version));
        }

        [Test,
            TestCase("https://mysite.org/mymod.zip", null),
        ]
        public void Validate_DownloadWithoutVersion_Throws(string download, string version)
        {
            Assert.Throws<CKAN.Kraken>(() => TryDownloadVersion(download, version));
        }

        private void TryDownloadVersion(string download, string version)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"]   = "AwesomeMod";
            if (download != null)
            {
                json["download"] = download;
            }
            if (version != null)
            {
                json["version"] = version;
            }

            // Act
            var val = new DownloadVersionValidator();
            val.Validate(new Metadata(json));
        }
    }
}
