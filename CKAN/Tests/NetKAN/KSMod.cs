using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NetKAN.KerbalStuffTests
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
            CKAN.NetKAN.KSMod.Inflate(meta, "foo", "baz");
            Assert.AreEqual((string) meta["foo"], "bar");

            // We shouldn't have an author field.
            Assert.IsNull(meta["author"]);

            // This should add a key.
            CKAN.NetKAN.KSMod.Inflate(meta, "author", "Jeb");
            Assert.AreEqual((string) meta["author"], "Jeb");
        }

        [Test]
        public void KSHome()
        {
            var ks = new CKAN.NetKAN.KSMod();
            ks.name = "foo bar";
            ks.id = 123;

            Assert.AreEqual("https://kerbalstuff.com/mod/123/foo%20bar", ks.KSHome());
        }

        [Test]
        // GH #199: Don't pre-fill KSP version fields if we see a ksp_min/max
        public void KSP_Version_Inflate_199()
        {
            JObject metadata = JObject.Parse(Tests.TestData.DogeCoinFlag_101());

            // Add our own field, and remove existing ones.
            metadata["ksp_version_min"] = "0.23.5";
            metadata["ksp_version"] = null;
            metadata["ksp_version_max"] = null;

            // Sanity check: make sure we don't have a ksp_version field to begin with.
            Assert.AreEqual(null, (string) metadata["ksp_version"]);

            CKAN.NetKAN.KSMod ksmod = test_ksmod();

            ksmod.InflateMetadata(metadata, Tests.TestData.DogeCoinFlagZip(), ksmod.versions[0]);

            // Make sure min is still there, and the rest unharmed.
            Assert.AreEqual(null, (string) metadata["ksp_version"]);
            Assert.AreEqual(null, (string) metadata["ksp_version_max"]);
            Assert.AreEqual("0.23.5", (string) metadata["ksp_version_min"]);

        }

        public CKAN.NetKAN.KSMod test_ksmod()
        {
            var ksmod = new CKAN.NetKAN.KSMod();
            ksmod.license = "CC-BY";
            ksmod.name = "Dogecoin Flag";
            ksmod.short_description = "Such test. Very unit. Wow.";
            ksmod.author = "pjf";

            ksmod.versions = new CKAN.NetKAN.KSVersion[1];
            ksmod.versions[0] = new CKAN.NetKAN.KSVersion();
            ksmod.versions[0].friendly_version = new CKAN.Version("0.25");
            ksmod.versions[0].download_path = "http://example.com/";

            return ksmod;
        }

    }
}