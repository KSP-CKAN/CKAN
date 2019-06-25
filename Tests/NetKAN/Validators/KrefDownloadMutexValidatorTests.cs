using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Validators;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public sealed class KrefDownloadMutexValidatorTests
    {
        [Test,
            TestCase(null, "https://mysite.org/mymod.zip"),
            TestCase("#/ckan/spacedock/1", null),
        ]
        public void Validate_OneWithoutTheOther_DoesNotThrow(string kref, string download)
        {
            Assert.DoesNotThrow(() => TryKrefDownload(kref, download));
        }

        [Test,
            TestCase(null, null),
            TestCase("#/ckan/spacedock/1", "https://mysite.org/mymod.zip"),
        ]
        public void Validate_NeitherOrBoth_Throws(string kref, string download)
        {
            Assert.Throws<CKAN.Kraken>(() => TryKrefDownload(kref, download));
        }

        private void TryKrefDownload(string kref, string download)
        {
            // Arrange
            var json = new JObject();
            json["spec_version"] = 1;
            json["identifier"]   = "AwesomeMod";
            if (kref != null)
            {
                json["$kref"] = kref;
            }
            if (download != null)
            {
                json["download"] = download;
            }

            // Act
            var val = new KrefDownloadMutexValidator();
            val.Validate(new Metadata(json));
        }
    }
}
