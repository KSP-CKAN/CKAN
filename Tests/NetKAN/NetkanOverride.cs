using CKAN;
using CKAN.NetKAN;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Data;

namespace Tests.NetKAN
{
    [TestFixture]
    public class NetkanOverride
    {
        JObject such_metadata;

        [SetUp]
        public void Setup()
        {
            such_metadata = JObject.Parse(TestData.DogeCoinFlag_101());
        }

        [Test]
        public void ExactOverride()
        {
            // Make sure our author starts as expected.
            OriginalAuthorUnchanged();

            // Add our override and processs.
            JObject new_metadata = ProcessOverrides(
                such_metadata,
                @"[{
                    ""version"" : ""1.01"",
                    ""override"" : {
                        ""author"" : ""doge""
                    }
                }]");

            Assert.AreEqual("doge",new_metadata["author"].ToString(),"Override processed");

            // Make sure our original metadata iddn't change.
            OriginalAuthorUnchanged();
        }

        [Test]
        public void UntriggeredOverride()
        {
            JObject new_metadata = ProcessOverrides(
                such_metadata,
                @"[{
                    ""version"" : ""1.02"",
                    ""override"" : {
                        ""author"" : ""doge""
                    }
                }]");

            Assert.AreEqual("pjf", new_metadata["author"].ToString(), "Untrigged override changes nothing");
        }

        [Test]
        [
            TestCase("range with spaces", "doge",
                @"[{
                    ""version"" : [ ""> 1.00"", ""< 1.02"" ],
                    ""override"" : { ""author"" : ""doge"" }
                }]"),

            TestCase("exact with spaces", "doge",
                @"[{
                    ""version"" : [ ""= 1.01"" ],
                    ""override"" : { ""author"" : ""doge"" }
                }]"),

            TestCase("range without spaces", "doge",
                @"[{
                    ""version"" : [ "">1.00"", ""<1.02"" ],
                    ""override"" : { ""author"" : ""doge"" }
                }]"),

            TestCase("exact without spaces", "doge",
                @"[{
                    ""version"" : [ ""=1.01"" ],
                    ""override"" : { ""author"" : ""doge"" }
                }]"),

            TestCase("inclusive range", "doge",
                @"[{
                    ""version"" : [ "">=1.01"", ""<2.00"" ],
                    ""override"" : { ""author"" : ""doge"" }
                }]"),

            TestCase("non-matching", "pjf",
                @"[{ ""version"" : [ ""< 1.00"", ""> 1.02"" ],
                    ""override"" : { ""author"" : ""doge"" }
                }]")
        ]
        public void RangedOverride(string label, string expected_author, string override_json)
        {
            JObject new_metadata = ProcessOverrides(
                such_metadata, override_json);

            Assert.AreEqual(expected_author, new_metadata["author"].ToString(), label);

            OriginalAuthorUnchanged();
        }

        /// <summary>
        /// Sanity to check to make sure the original metadata hasn't changed during testing.
        /// </summary>
        private void OriginalAuthorUnchanged()
        {
            Assert.AreEqual("pjf", such_metadata["author"].ToString(), "Sanity check");
        }

        /// <summary>
        /// Process overrides (if present)
        /// </summary>
        private JObject ProcessOverrides(JObject metadata)
        {
            return new CKAN.NetKAN.NetkanOverride(such_metadata).ProcessOverrides();
        }

        /// <summary>
        /// Process overrides using the JSON string specified.
        /// </summary>
        private JObject ProcessOverrides(JObject metadata, string overrides)
        {
            metadata["x_netkan_override"] = JArray.Parse(overrides);
            return ProcessOverrides(metadata);
        }
    }
}

