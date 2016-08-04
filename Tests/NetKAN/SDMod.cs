using CKAN.NetKAN.Sources.Spacedock;
using NUnit.Framework;
using Tests.Data;

namespace Tests.NetKAN
{
    [TestFixture]
    public class SDMod
    {
        [Test]
        public void SDHome()
        {
            var sd = new SpacedockMod { name = "foo bar", id = 123 };

            // SDHome no longer escapes URLs.
            Assert.AreEqual("https://spacedock.info/mod/123/foo bar", sd.GetPageUrl().ToString());
        }

        [Test]
        // GH #214: Make sure we pick up the right version
        public void SD_Version_Select_214()
        {
            var mod = SpacedockMod.FromJson(TestData.KS_CustomAsteroids_string());
            Assert.AreEqual(711, mod.Latest().id, "GH #214 - Select default_version_id");
        }

        [Test]
        [TestCase("/mod/42/Example/download/1.23", Result = "https://spacedock.info/mod/42/Example/download/1.23")]
        [TestCase("/mod/42/Example%20With%20Spaces/download/1.23", Result = "https://spacedock.info/mod/42/Example%20With%20Spaces/download/1.23")]
        [TestCase("/mod/42/Example With Spaces/download/1.23", Result = "https://spacedock.info/mod/42/Example%20With%20Spaces/download/1.23")]
        [TestCase("/mod/79/Salyut%20Stations%20%26%20Soyuz%20Ferries/download/0.93", Result = "https://spacedock.info/mod/79/Salyut%20Stations%20%26%20Soyuz%20Ferries/download/0.93")]
        // GH #816: Ensure URLs with & are encoded correctly.
        public string SD_URL_encode_816(string path)
        {
            return SpacedockApi.ExpandPath(path).OriginalString;
        }

        [Test]
        [TestCase("1.0", "1.0.0")]
        [TestCase("1.0.3", "1.0.3")]
        public void SD_Expand_KSP_Version_1156(string original, string expected)
        {
            Assert.AreEqual(
                expected,
                SDVersion.JsonConvertKSPVersion.ExpandVersionIfNeeded(original)
            );
        }
    }
}