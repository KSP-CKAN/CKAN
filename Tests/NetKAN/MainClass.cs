using CKAN;
using CKAN.NetKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Data;

namespace Tests.NetKAN
{
    [TestFixture]
    public class MainClassTests
    {
        [Test]
        public void FixVersionStringsUnharmed()
        {
            JObject metadata = JObject.Parse(TestData.DogeCoinFlag_101());

            Assert.AreEqual("1.01", (string)metadata["version"], "Original version as expected");

            metadata = new EpochTransformer().Transform(new Metadata(metadata)).Json();
            Assert.AreEqual("1.01", (string)metadata["version"], "Version unharmed without x_netkan_force_v");
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

            Assert.AreEqual(orig_version, (string)metadata["version"], "JSON parsed as expected");

            metadata = new ForcedVTransformer().Transform(new Metadata(metadata)).Json();

            Assert.AreEqual(new_version, (string)metadata["version"], "Output string as expected");
        }

        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01""}", "1.01", "1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01"", ""x_netkan_epoch"" : ""0""}",
             "1.01", "1.01", Description = "Implicit 0")]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01"", ""x_netkan_epoch"" : ""1""}", "1.01", "1:1.01")]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""v1.01"", ""x_netkan_epoch"" : ""9""}", "v1.01", "9:v1.01")]
        public void ApplyEpochNumber(string json, string orig_version, string new_version)
        {
            JObject metadata = JObject.Parse(json);
            Assert.AreEqual(orig_version, (string)metadata["version"], "JSON parsed as expected");
            metadata = new EpochTransformer().Transform(new Metadata(metadata)).Json();
            Assert.AreEqual(new_version, (string)metadata["version"], "Output string as expected");
        }

        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01""}", false)]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01"", ""x_netkan_epoch"" : ""a""}", true)]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01"", ""x_netkan_epoch"" : ""-1""}", true)]
        [TestCase(@"{""spec_version"": 1, ""version"" : ""1.01"", ""x_netkan_epoch"" : ""5.5""}", true)]
        public void Invaild(string json, bool expected_to_throw)
        {
            TestDelegate test_delegate = () => new EpochTransformer().Transform(new Metadata(JObject.Parse(json))).Json();
            if (expected_to_throw)
                Assert.Throws<BadMetadataKraken>(test_delegate);
            else
                Assert.DoesNotThrow(test_delegate);
        }
    }
}
