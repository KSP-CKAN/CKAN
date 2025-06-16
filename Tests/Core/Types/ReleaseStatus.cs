using NUnit.Framework;

using CKAN;

namespace Tests.Core.Types
{
    [TestFixture]
    public class ReleaseStatus
    {
        [TestCase("stable"),
         TestCase("testing"),
         TestCase("development"),
         TestCase("alpha", "development"),
         TestCase("beta",  "testing")]
        public void Deserialize_GoodString_DoesNotThrow(string  status,
                                                        string? aliasValue = null)
        {
            Assert.DoesNotThrow(() =>
            {
                var module = CkanModule.FromJson(
                    Relationships.RelationshipResolverTests.MergeWithDefaults(
                        $@"{{
                            ""identifier"": ""aMod"",
                            ""release_status"": ""{status}""
                        }}"));
                Assert.AreEqual(aliasValue ?? status,
                                module.release_status.ToString());
            });
        }

        [TestCase("cheese"),
         TestCase("some thing I wrote last night"),
         TestCase(""),
         TestCase("yo dawg I heard you like tests"),
         TestCase("42")]
        public void Deserialize_BadString_Throws(string status)
        {
            Assert.Throws<BadMetadataKraken>(delegate
            {
                var module = CkanModule.FromJson(
                    Relationships.RelationshipResolverTests.MergeWithDefaults(
                        $@"{{
                            ""identifier"": ""aMod"",
                            ""release_status"": ""{status}""
                        }}"));
            });
        }

        [Test]
        public void Deserialize_Absent_DoesNotThrow()
        {
            // According to the spec, no release status means "stable"
            Assert.DoesNotThrow(() =>
            {
                var module = CkanModule.FromJson(
                    Relationships.RelationshipResolverTests.MergeWithDefaults(
                        $@"{{
                            ""identifier"": ""aMod""
                        }}"));
                Assert.AreEqual("stable", module.release_status.ToString());
            });
        }

        [Test]
        public void Deserialize_NullOrEmpty_DoesNotThrow()
        {
            // According to the spec, no release status means "stable"
            Assert.DoesNotThrow(() =>
            {
                var module = CkanModule.FromJson(
                    Relationships.RelationshipResolverTests.MergeWithDefaults(
                        $@"{{
                            ""identifier"": ""aMod"",
                            ""release_status"": null
                        }}"));
                Assert.AreEqual("stable", module.release_status.ToString());
            });
        }
    }
}
