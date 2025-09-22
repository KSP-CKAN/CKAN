using System.Linq;

using NUnit.Framework;
using Newtonsoft.Json.Linq;

using CKAN;
using CKAN.NetKAN.Model;

namespace Tests.NetKAN.Model
{
    [TestFixture]
    public class MetadataTests
    {
        [Test,
            // https://stackoverflow.com/a/61403175/2422988
            TestCase((object)new string[]
                     {
                        @"{
                            ""spec_version"": ""v1.34"",
                            ""download_size"": 1000
                        }",
                        @"{
                            ""spec_version"": ""v1.34"",
                            ""download_size"": 1001
                        }",
                     }),
            TestCase((object)new string[]
                     {
                        @"{
                            ""spec_version"": ""v1.34"",
                            ""download_size"": 1000,
                            ""download_hash"": {
                                ""sha1"": ""DEADBEEFDEADBEEF"",
                                ""sha256"": ""DEADBEEFDEADBEEF""
                            }
                        }",
                        @"{
                            ""spec_version"": ""v1.34"",
                            ""download_size"": 1000,
                            ""download_hash"": {
                                ""sha1"": ""8008580085"",
                                ""sha256"": ""DEADBEEFDEADBEEF""
                            }
                        }",
                     }),
            TestCase((object)new string[]
                     {
                        @"{
                            ""spec_version"": ""v1.34"",
                            ""download_size"": 1000,
                            ""download_hash"": {
                                ""sha1"": ""DEADBEEFDEADBEEF"",
                                ""sha256"": ""DEADBEEFDEADBEEF""
                            }
                        }",
                        @"{
                            ""spec_version"": ""v1.34"",
                            ""download_size"": 1000,
                            ""download_hash"": {
                                ""sha1"": ""DEADBEEFDEADBEEF"",
                                ""sha256"": ""8008580085""
                            }
                        }",
                     }),
        ]
        public void Merge_MismatchedSizeOrHash_Throws(string[] moduleJsons)
        {
            // Arrange
            var modules = moduleJsons.Select(j => new Metadata(JObject.Parse(j)))
                                     .ToArray();

            // Act / Assert
            var exception = Assert.Throws<Kraken>(() => Metadata.Merge(modules));
            StringAssert.Contains("does not match download from", exception?.Message);
        }

        [Test,
            // Two modules with one URL each, merged to download list
            TestCase(new string[]
                     {
                         @"{
                             ""spec_version"": ""v1.32"",
                             ""download"": ""https://github.com/"",
                             ""download_size"": 1000,
                             ""download_hash"": {
                                 ""sha1"": ""DEADBEEFDEADBEEF"",
                                 ""sha256"": ""DEADBEEFDEADBEEF""
                             }
                         }",
                         @"{
                             ""spec_version"": ""v1.34"",
                             ""download"": ""https://spacedock.info/"",
                             ""download_size"": 1000,
                             ""download_hash"": {
                                 ""sha1"": ""DEADBEEFDEADBEEF"",
                                 ""sha256"": ""DEADBEEFDEADBEEF""
                             }
                         }",
                     },
                     @"{
                         ""spec_version"": ""v1.34"",
                         ""download"": [ ""https://github.com/"", ""https://spacedock.info/"" ],
                         ""download_size"": 1000,
                         ""download_hash"": {
                             ""sha1"": ""DEADBEEFDEADBEEF"",
                             ""sha256"": ""DEADBEEFDEADBEEF""
                         }
                     }"),
            // Two modules, one with download list, merged to list without duplicates
            TestCase(new string[]
                     {
                         @"{
                             ""spec_version"": ""v1.32"",
                             ""download"": ""https://github.com/"",
                             ""download_size"": 1000,
                             ""download_hash"": {
                                 ""sha1"": ""DEADBEEFDEADBEEF"",
                                 ""sha256"": ""DEADBEEFDEADBEEF""
                             }
                         }",
                         @"{
                             ""spec_version"": ""v1.34"",
                             ""download"": [ ""https://github.com/"", ""https://spacedock.info/"" ],
                             ""download_size"": 1000,
                             ""download_hash"": {
                                 ""sha1"": ""DEADBEEFDEADBEEF"",
                                 ""sha256"": ""DEADBEEFDEADBEEF""
                             }
                         }",
                     },
                     @"{
                         ""spec_version"": ""v1.34"",
                         ""download"": [ ""https://github.com/"", ""https://spacedock.info/"" ],
                         ""download_size"": 1000,
                         ""download_hash"": {
                             ""sha1"": ""DEADBEEFDEADBEEF"",
                             ""sha256"": ""DEADBEEFDEADBEEF""
                         }
                     }"),
        ]
        public void Merge_WithModules_ModuleWithMergedDownload(string[] moduleJsons,
                                                               string correctResult)
        {
            // Arrange
            var modules = moduleJsons.Select(j => new Metadata(JObject.Parse(j)))
                                     .ToArray();
            var correctJson = JObject.Parse(correctResult);

            // Act
            var mergedJson = Metadata.Merge(modules).Json();

            // Assert
            Assert.IsTrue(JToken.DeepEquals(correctJson, mergedJson),
                          "Expected {0}, got {1}",
                          correctJson, mergedJson);
        }

        [TestCase(@"{ ""license"": ""restricted"" }", ExpectedResult = false),
         TestCase(@"{ ""license"": ""GPL-3.0"" }",    ExpectedResult = true),
         TestCase(@"{ ""license"": ""MIT"" }",        ExpectedResult = true),
         TestCase(@"{ ""license"": ""CC-BY"" }",      ExpectedResult = true),
         TestCase(@"{ ""license"": ""CC0"" }",        ExpectedResult = true),
        ]
        public bool Redistributable_WithLicense_Correct(string json)
            => new Metadata(JObject.Parse(json)).Redistributable;

        [TestCase(@"{""download"": ""https://github.com/""}", new string[] { "github.com" }),
         TestCase(@"{""download"": [ ""https://github.com/"", ""https://spacedock.info"" ] }",
                  new string[] { "github.com", "spacedock.info" }),
        ]
        public void Hosts_WithURLs_Works(string json, string[] expectedHosts)
        {
            CollectionAssert.AreEqual(expectedHosts,
                                      new Metadata(JObject.Parse(json)).Hosts);
        }

        [TestCase(@"{
                      ""identifier"":    ""TestMod"",
                      ""version"":       ""1.0"",
                      ""license"":       ""public-domain"",
                      ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF"" }
                  }",
                  ExpectedResult = "https://archive.org/download/TestMod-1.0/DEADBEEF-TestMod-1.0.zip"),
         TestCase(@"{
                      ""identifier"":    ""TestMod"",
                      ""version"":       ""1.0"",
                      ""license"":       ""Unlicense"",
                      ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF"" }
                  }",
                  ExpectedResult = "https://archive.org/download/TestMod-1.0/DEADBEEF-TestMod-1.0.zip"),
         TestCase(@"{
                      ""identifier"":    ""TestMod"",
                      ""version"":       ""1.0"",
                      ""license"":       ""restricted"",
                      ""download_hash"": { ""sha1"": ""DEADBEEFDEADBEEF"" }
                  }",
                  ExpectedResult = null),
        ]
        public string? FallbackDownload_WithURLs_Works(string json)
            => new Metadata(JObject.Parse(json)).FallbackDownload?.OriginalString;

    }
}
