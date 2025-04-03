using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using Tests.Data;

namespace Tests.NetKAN
{
    [TestFixture]
    public class ForcedVTransformerTests
    {
        private readonly TransformOptions opts = new TransformOptions(1, null, null, null, false, null);

        [Test]
        public void FixVersionStringsUnharmed()
        {
            JObject metadata = JObject.Parse(TestData.DogeCoinFlag_101());

            Assert.AreEqual("1.01", (string?)metadata["version"], "Original version as expected");

            metadata = new ForcedVTransformer().Transform(new Metadata(metadata), opts).First().Json();
            Assert.AreEqual("1.01", (string?)metadata["version"], "Version unharmed without x_netkan_force_v");
        }

        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01""}", "1.01", "1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01"", ""x_netkan_force_v"" : ""true""}", "1.01", "v1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01"", ""x_netkan_force_v"" : ""false""}", "1.01", "1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""v1.01""}", "v1.01", "v1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""v1.01"", ""x_netkan_force_v"" : ""true""}", "v1.01", "v1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""v1.01"", ""x_netkan_force_v"" : ""false""}", "v1.01", "v1.01")]
        // Test with and without x_netkan_force_v, and with and without a 'v' prepended already.
        public void FixVersionStrings(string json, string orig_version, string new_version)
        {
            JObject metadata = JObject.Parse(json);

            Assert.AreEqual(orig_version, (string?)metadata["version"], "JSON parsed as expected");

            metadata = new ForcedVTransformer().Transform(new Metadata(metadata), opts).First().Json();

            Assert.AreEqual(new_version, (string?)metadata["version"], "Output string as expected");
        }

    }
}
