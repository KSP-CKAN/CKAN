using System.Linq;

using NUnit.Framework;

using CKAN.NetKAN.Sources.Spacedock;
using Tests.Data;

namespace Tests.NetKAN.Sources.Spacedock
{
    [TestFixture]
    public class SpacedockModTests
    {
        [Test]
        public void GetPageUrl_WithSpace_EscapesURL()
        {
            // Arrange
            var sd = new SpacedockMod
            {
                name = "foo bar",
                id   = 123,
            };

            // Assert
            Assert.AreEqual("https://spacedock.info/mod/123/foo%20bar",
                            sd.GetPageUrl().OriginalString);
        }

        [Test]
        // GH #214: Make sure we pick up the right version
        public void All_DefaultNotFirst_DefaultBecomesFirst()
        {
            // Arrange
            var mod = SpacedockMod.FromJson(TestData.KS_CustomAsteroids_string());

            // Assert
            Assert.AreEqual(711, mod?.All().FirstOrDefault()?.id,
                            "GH #214 - Select default_version_id");
        }

        [Test]
        [TestCase("/mod/42/Example/download/1.23",
                  ExpectedResult="https://spacedock.info/mod/42/Example/download/1.23")]
        [TestCase("/mod/42/Example%20With%20Spaces/download/1.23",
                  ExpectedResult = "https://spacedock.info/mod/42/Example%20With%20Spaces/download/1.23")]
        [TestCase("/mod/42/Example With Spaces/download/1.23",
                  ExpectedResult = "https://spacedock.info/mod/42/Example%20With%20Spaces/download/1.23")]
        [TestCase("/mod/79/Salyut%20Stations%20%26%20Soyuz%20Ferries/download/0.93",
                  ExpectedResult = "https://spacedock.info/mod/79/Salyut%20Stations%20%26%20Soyuz%20Ferries/download/0.93")]
        // GH #816: Ensure URLs with & are encoded correctly.
        public string? FromJson_RelativeDownloadPath_GeneratesAbsoluteDownloadURL(string path)
            => SpacedockMod.FromJson(string.Format(
                    @"{{""name"":""Mod"",""id"":69420,""game"":""Kerbal Space Program"",""game_id"":3102,""short_description"":""A mod"",""description"":""A mod"",""downloads"":0,""followers"":0,""author"":""linuxgurugamer"",""default_version_id"":1,""shared_authors"":[],""background"":null,""bg_offset_y"":null,""license"":""MIT"",""website"":null,""donations"":null,""source_code"":null,""url"":""/mod/69420/Mod"",""versions"":[{{""friendly_version"":""1"",""game_version"":""1.12.2"",""id"":1,""created"":""2021-07-16T20:46:12.309009+00:00"",""download_path"":""{0}"",""changelog"":"""",""downloads"":0}}]}}",
                    path))?.versions?[0].download_path?.OriginalString;

        [Test]
        [TestCase("1.0",   ExpectedResult = "1.0.0")]
        [TestCase("1.0.3", ExpectedResult = "1.0.3")]
        public string ExpandVersionIfNeeded_TwoOrThreePieceGameVersions_ReturnsThreePieces(string original)
            => SpacedockVersion.JsonConvertGameVersion.ExpandVersionIfNeeded(original);
    }
}
