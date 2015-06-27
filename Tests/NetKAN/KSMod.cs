using System;
using CKAN.NetKAN;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Data;
using Version = CKAN.Version;

namespace Tests.NetKAN
{
    [TestFixture]
    public class KSMod
    {
        [Test]
        public void Inflate()
        {
            string json = @"{ 'foo': 'bar'}";
            JObject meta = JObject.Parse(json);

            // Sanity check.
            Assert.AreEqual((string) meta["foo"], "bar");

            // This should do nothing.
            CkanInflator.Inflate(meta, "foo", "baz");
            Assert.AreEqual((string) meta["foo"], "bar");

            // We shouldn't have an author field.
            Assert.IsNull(meta["author"]);

            // This should add a key.
            CkanInflator.Inflate(meta, "author", "Jeb");
            Assert.AreEqual((string) meta["author"], "Jeb");
        }

        [Test]
        public void KSHome()
        {
            var ks = new CKAN.NetKAN.KSMod {name = "foo bar", id = 123};

            // KSHome no longer escapes URLs.
            Assert.AreEqual("https://kerbalstuff.com/mod/123/foo bar", ks.KSHome().ToString());
        }

        [Test]
        // GH #199: Don't pre-fill KSP version fields if we see a ksp_min/max
        public void KSP_Version_Inflate_199()
        {
            JObject metadata = JObject.Parse(TestData.DogeCoinFlag_101());

            // Add our own field, and remove existing ones.
            metadata["ksp_version_min"] = "0.23.5";
            metadata["ksp_version"] = null;
            metadata["ksp_version_max"] = null;

            // Sanity check: make sure we don't have a ksp_version field to begin with.
            Assert.AreEqual(null, (string) metadata["ksp_version"]);

            CKAN.NetKAN.KSMod ksmod = test_ksmod();

            ksmod.InflateMetadata(metadata, TestData.DogeCoinFlagZip(), ksmod.versions[0]);

            // Make sure min is still there, and the rest unharmed.
            Assert.AreEqual(null, (string) metadata["ksp_version"]);
            Assert.AreEqual(null, (string) metadata["ksp_version_max"]);
            Assert.AreEqual("0.23.5", (string) metadata["ksp_version_min"]);

        }

        [Test]
        // GH #214: Make sure we pick up the right version
        public void KS_Version_Select_214()
        {
            CKAN.NetKAN.KSMod mod = KSAPI.Mod(TestData.KS_CustomAsteroids_string());
            Assert.AreEqual(711, mod.Latest().id, "GH #214 - Select default_version_id");
        }

        [Test]
        [TestCase("/mod/42/Example/download/1.23", Result="https://kerbalstuff.com/mod/42/Example/download/1.23")]
        [TestCase("/mod/42/Example%20With%20Spaces/download/1.23", Result="https://kerbalstuff.com/mod/42/Example%20With%20Spaces/download/1.23")]
        [TestCase("/mod/42/Example With Spaces/download/1.23", Result="https://kerbalstuff.com/mod/42/Example%20With%20Spaces/download/1.23")]
        [TestCase("/mod/79/Salyut%20Stations%20%26%20Soyuz%20Ferries/download/0.93",Result="https://kerbalstuff.com/mod/79/Salyut%20Stations%20%26%20Soyuz%20Ferries/download/0.93")]
        // GH #816: Ensure URLs with & are encoded correctly.
        public string KS_URL_encode_816(string path)
        {
            return KSAPI.ExpandPath(path).OriginalString;
        }

        [Test]
        [TestCase("1.0","1.0.0")]
        [TestCase("1.0.3","1.0.3")]
        public void KS_Expand_KSP_Version_1156(string original, string expected)
        {
            Assert.AreEqual(
                expected,
                CKAN.NetKAN.KSVersion.JsonConvertKSPVersion.ExpandVersionIfNeeded(original)
            );
        }

        public static CKAN.NetKAN.KSMod test_ksmod()
        {
            var ksmod = new CKAN.NetKAN.KSMod
            {
                license = "CC-BY",
                name = "Dogecoin Flag",
                short_description = "Such test. Very unit. Wow.",
                author = "pjf",
                versions = new KSVersion[1]
            };

            ksmod.versions[0] = new KSVersion
            {
                friendly_version = new Version("0.25"),
                download_path = new Uri("http://example.com/")
            };

            return ksmod;
        }

    }
}