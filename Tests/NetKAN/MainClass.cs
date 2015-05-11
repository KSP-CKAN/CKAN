using CKAN.NetKAN;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace NetKAN
{
    [TestFixture]
    public class MainClassTests
    {

        [Test]
        public void FixVersionStringsUnharmed()
        {
            JObject metadata = JObject.Parse(Tests.TestData.DogeCoinFlag_101());

            Assert.AreEqual("1.01", (string) metadata["version"], "Original version as expected");

            metadata = MainClass.FixVersionStrings(metadata);
            Assert.AreEqual("1.01", (string) metadata["version"], "Version unharmed without x_netkan_force_v");
        }

        [Test]
        // Test with and without x_netkan_force_v, and with and without a 'v' prepended already.
        [TestCase(@"{""version"" : ""1.01""}", "1.01", "1.01")]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_force_v"" : ""true""}", "1.01", "v1.01")]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_force_v"" : ""false""}", "1.01", "1.01")]
        [TestCase(@"{""version"" : ""v1.01""}", "v1.01", "v1.01")]
        [TestCase(@"{""version"" : ""v1.01"", ""x_netkan_force_v"" : ""true""}", "v1.01", "v1.01")]
        [TestCase(@"{""version"" : ""v1.01"", ""x_netkan_force_v"" : ""false""}", "v1.01", "v1.01")]
        public void FixVersionStrings(string json, string orig_version, string new_version)
        {
            JObject metadata = JObject.Parse(json);

            Assert.AreEqual(orig_version, (string) metadata["version"], "JSON parsed as expected");

            metadata = MainClass.FixVersionStrings(metadata);

            Assert.AreEqual(new_version, (string) metadata["version"], "Output string as expected");
        }

    }
}

