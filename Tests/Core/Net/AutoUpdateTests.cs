using System.IO;
using System.Net;

using NUnit.Framework;
using Newtonsoft.Json;

using CKAN;
using Tests.Data;

namespace Tests.Core.AutoUpdateTests
{
    [TestFixture]
    public class AutoUpdateTests
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category("Online")]
        // This could fail if run during a release, so it's marked as Flaky.
        [Category("FlakyNetwork")]
        public void GetUpdate_DevBuildOrStable_Works(bool devBuild)
        {
            // Force-allow TLS 1.2 for HTTPS URLs, because GitHub requires it.
            // This is on by default in .NET 4.6, but not in 4.5.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var updater = new AutoUpdate();
            var update  = updater.GetUpdate(devBuild);

            // Is is a *really* basic test to just make sure we get release info
            // if we ask for it.
            Assert.IsNotNull(update.ReleaseNotes);
            Assert.IsNotNull(update.Version);
        }

        [Test]
        [TestCase("aaa\r\n---\r\nbbb", "bbb", "Release note marker included")]
        [TestCase("aaa\r\nbbb", "aaa\r\nbbb", "No release note marker")]
        [TestCase("aaa\r\n---\r\nbbb\r\n---\r\nccc", "bbb\r\n---\r\nccc", "Multi release notes markers")]
        public void ExtractReleaseNotes(string body, string expected, string comment)
        {
            Assert.AreEqual(expected,
                            GitHubReleaseCkanUpdate.ExtractReleaseNotes(body),
                            comment);
        }

        [Test]
        public void GitHubReleaseCkanUpdate_NormalUpdate_ParsedCorrectly()
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
            var relInfo = JsonConvert.DeserializeObject<GitHubReleaseInfo>(releaseJSON);
            var upd     = new GitHubReleaseCkanUpdate(relInfo);

            // Assert
            Assert.AreEqual("v1.25.0", relInfo.tag_name);
            Assert.AreEqual("Wallops", relInfo.name);
            Assert.AreEqual("https://github.com/KSP-CKAN/CKAN/releases/download/v1.25.0/ckan.exe",
                            upd.ReleaseDownload.ToString());
            Assert.AreEqual(6651392, upd.ReleaseSize);
            Assert.AreEqual("https://github.com/KSP-CKAN/CKAN/releases/download/v1.25.0/AutoUpdater.exe",
                            upd.UpdaterDownload.ToString());
            Assert.AreEqual(414208, upd.UpdaterSize);
            Assert.AreEqual("Greatest release notes of all time", upd.ReleaseNotes);
        }

        [Test]
        public void S3BuildCkanUpdate_Constructor_ParsedCorrectly()
        {
            // Arrange / Act
            var upd = new S3BuildCkanUpdate(
                JsonConvert.DeserializeObject<S3BuildVersionInfo>(
                    File.ReadAllText(TestData.DataDir("version.json"))));

            // Assert
            Assert.AreEqual("v1.34.5.24015 aka dev",
                            upd.Version.ToString());
            Assert.AreEqual("### Internal\n\n- [Policy] Fix #3518 rewrite de-indexing policy (#3993 by: JonnyOThan; reviewed: HebaruSan)",
                            upd.ReleaseNotes);
            Assert.AreEqual("https://ksp-ckan.s3-us-west-2.amazonaws.com/ckan.exe",
                            upd.ReleaseDownload.ToString());
            Assert.AreEqual("https://ksp-ckan.s3-us-west-2.amazonaws.com/AutoUpdater.exe",
                            upd.UpdaterDownload.ToString());
        }

    }
}
