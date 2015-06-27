using System.Collections.Generic;
using CKAN;
using CKAN.NetKAN;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Data;

namespace Tests.NetKAN
{
    [TestFixture] public class MainClassTests
    {
        [Test]
        public void FixVersionStringsUnharmed()
        {
            JObject metadata = JObject.Parse(TestData.DogeCoinFlag_101());

            Assert.AreEqual("1.01", (string) metadata["version"], "Original version as expected");

            metadata = MainClass.FixVersionStrings(metadata);
            Assert.AreEqual("1.01", (string) metadata["version"], "Version unharmed without x_netkan_force_v");
        }

        [TestCase(@"{""version"" : ""1.01""}", "1.01", "1.01")]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_force_v"" : ""true""}", "1.01", "v1.01")]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_force_v"" : ""false""}", "1.01", "1.01")]
        [TestCase(@"{""version"" : ""v1.01""}", "v1.01", "v1.01")]
        [TestCase(@"{""version"" : ""v1.01"", ""x_netkan_force_v"" : ""true""}", "v1.01", "v1.01")]
        [TestCase(@"{""version"" : ""v1.01"", ""x_netkan_force_v"" : ""false""}", "v1.01", "v1.01")]
        // Test with and without x_netkan_force_v, and with and without a 'v' prepended already.
        public void FixVersionStrings(string json, string orig_version, string new_version)
        {
            JObject metadata = JObject.Parse(json);

            Assert.AreEqual(orig_version, (string) metadata["version"], "JSON parsed as expected");

            metadata = MainClass.FixVersionStrings(metadata);

            Assert.AreEqual(new_version, (string) metadata["version"], "Output string as expected");
        }

        [TestCase(@"{""version"" : ""1.01""}", "1.01", "1.01")]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_epoch"" : ""0""}",
             "1.01", "1.01", Description = "Implicit 0")]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_epoch"" : ""1""}", "1.01", "1:1.01")]
        [TestCase(@"{""version"" : ""v1.01"", ""x_netkan_epoch"" : ""9""}", "v1.01", "9:v1.01")]
        public void ApplyEpochNumber(string json, string orig_version, string new_version)
        {
            JObject metadata = JObject.Parse(json);
            Assert.AreEqual(orig_version, (string) metadata["version"], "JSON parsed as expected");
            metadata = MainClass.FixVersionStrings(metadata);
            Assert.AreEqual(new_version, (string) metadata["version"], "Output string as expected");
        }

        [TestCase(@"{""version"" : ""1.01""}", false)]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_epoch"" : ""a""}", true)]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_epoch"" : ""-1""}", true)]
        [TestCase(@"{""version"" : ""1.01"", ""x_netkan_epoch"" : ""5.5""}", true)]
        public void Invaild(string json, bool expected_to_throw)
        {
            TestDelegate test_delegate = () => MainClass.FixVersionStrings(JObject.Parse(json));
            if (expected_to_throw)
                Assert.Throws<BadMetadataKraken>(test_delegate);
            else
                Assert.DoesNotThrow(test_delegate);
        }

        [TestCaseSource("StripNetkanMetadataTestCaseSource")]
        public void StripNetkanMetadata(string json, string expected)
        {
            var metadata = MainClass.StripNetkanMetadata(JObject.Parse(json));
            var expectedMetadata = JObject.Parse(expected);

            Assert.AreEqual(metadata, expectedMetadata);
        }

        private IEnumerable<object[]> StripNetkanMetadataTestCaseSource
        {
            get
            {
                yield return new object[]
                {
@"
{
    ""foo"": ""bar""
}
",
@"
{
    ""foo"": ""bar""
}
                    "
                };

                yield return new object[]
                {
@"
{
    ""foo"": ""bar"",
    ""x_netkan"": ""foobar""
}
",
@"
{
    ""foo"": ""bar""
}
                    "
                };


                yield return new object[]
                {
@"
{
    ""foo"": ""bar"",
    ""x_netkan"": ""foobar"",
    ""baz"": {
        ""x_netkan_foo"": ""foobar""
    }
}
",
@"
{
    ""foo"": ""bar"",
    ""baz"": {}
}
                    "
                };
            }
        }
    }
}