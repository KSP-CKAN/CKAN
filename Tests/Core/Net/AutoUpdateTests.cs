using System;
using System.Net;
using NUnit.Framework;
using CKAN;
using CKAN.Versioning;

namespace Tests.Core.AutoUpdateTests
{
    [TestFixture]
    public class AutoUpdateTests
    {
        // pjf's repo has no releases, so tests on this URL should fail
        private readonly Uri test_ckan_release = new Uri("https://api.github.com/repos/pjf/CKAN/releases/latest");

        [Test]
        [Category("Online")]
        // This could fail if run during a release, so it's marked as Flaky.
        [Category("FlakyNetwork")]
        public void FetchLatestReleaseInfo()
        {
            // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
            // This is on by default in .NET 4.6, but not in 4.5.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var updater = AutoUpdate.Instance;

            // Is is a *really* basic test to just make sure we get release info
            // if we ask for it.
            updater.FetchLatestReleaseInfo();
            Assert.IsNotNull(updater.latestUpdate.ReleaseNotes);
            Assert.IsNotNull(updater.latestUpdate.Version);
            Assert.IsTrue(updater.IsFetched());
        }

        [Test]
        [TestCase("aaa\r\n---\r\nbbb", "bbb", "Release note marker included")]
        [TestCase("aaa\r\nbbb", "aaa\r\nbbb", "No release note marker")]
        [TestCase("aaa\r\n---\r\nbbb\r\n---\r\nccc", "bbb\r\n---\r\nccc", "Multi release notes markers")]
        public void ExtractReleaseNotes(string body, string expected, string comment)
        {
            Assert.AreEqual(
                expected,
                CkanUpdate.ExtractReleaseNotes(body),
                comment
            );
        }

        [Test]
        public void CkanUpdate_NormalUpdate_ParsedCorrectly()
        {
            // Arrange
            const string releaseJSON = @"{
                ""name"": ""Wallops"",
                ""tag_name"": ""v1.25.0"",
                ""assets"": [ {
                    ""size"": 414208,
                    ""browser_download_url"": ""https://github.com/KSP-CKAN/CKAN/releases/download/v1.25.0/AutoUpdater.exe""
                }, {
                    ""size"": 6789120,
                    ""browser_download_url"": ""https://github.com/KSP-CKAN/CKAN/releases/download/v1.25.0/CKAN.dmg""
                }, {
                    ""size"": 6651392,
                    ""browser_download_url"": ""https://github.com/KSP-CKAN/CKAN/releases/download/v1.25.0/ckan.exe""
                }, {
                    ""size"": 660764,
                    ""browser_download_url"": ""https://github.com/KSP-CKAN/CKAN/releases/download/v1.25.0/ckan_1.25.0_all.deb""
                } ],
                ""body"": ""[![](https://upload.wikimedia.org/wikipedia/commons/thumb/2/24/NASA_Wallops_Flight_Facility%2C_2010.jpg/780px-NASA_Wallops_Flight_Facility%2C_2010.jpg)](https://en.wikipedia.org/wiki/Wallops_Flight_Facility)\r\n\r\n---\r\nGreatest release notes of all time""
            }";

            // Act
            CkanUpdate cu = new CkanUpdate(releaseJSON);

            // Assert
            Assert.AreEqual(new CkanModuleVersion("v1.25.0", "Wallops"), cu.Version);
            Assert.AreEqual("https://github.com/KSP-CKAN/CKAN/releases/download/v1.25.0/ckan.exe", cu.ReleaseDownload.ToString());
            Assert.AreEqual(6651392, cu.ReleaseSize);
            Assert.AreEqual("https://github.com/KSP-CKAN/CKAN/releases/download/v1.25.0/AutoUpdater.exe", cu.UpdaterDownload.ToString());
            Assert.AreEqual(414208, cu.UpdaterSize);
            Assert.AreEqual("Greatest release notes of all time", cu.ReleaseNotes);
        }
    }
}
