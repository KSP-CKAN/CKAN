using System;
using System.IO;
using System.Linq;

using log4net;
using log4net.Config;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using CKAN.NetKAN.Utils;
using Tests.Data;

namespace Tests.NetKAN.Services
{
    [TestFixture]
    public sealed class CacheServiceTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            BasicConfigurator.Configure(LogManager.GetRepository());
        }

        [Test]
        public void ExecuteCached_SecondRunNotModified_ReturnsSavedEntryWithoutRunningAction()
        {
            using (var dir = new TemporaryDirectory())
            {
                // Arrange
                var lastModified = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc);
                var metadata = new Metadata(new JObject
                {
                    ["identifier"] = "Mod",
                    ["download"]   = "https://example.com/mod.zip",
                });
                var output = new JObject
                {
                    ["identifier"]   = "Mod",
                    ["release_date"] = "2024-05-06T07:08:09.123+02:00",
                };
                var http = new Mock<IHttpService>();
                http.Setup(h => h.DownloadModule(It.IsAny<Metadata>(), null))
                    .Returns(new DownloadResult("file.zip", false, "\"abc\"", lastModified));
                http.Setup(h => h.DownloadModule(It.IsAny<Metadata>(),
                                                 It.Is<DownloadOptions?>(o => o != null
                                                                           && o.ETag == "\"abc\""
                                                                           && o.LastModified == lastModified
                                                                           && o.LastModified.Value.Kind == DateTimeKind.Utc)))
                    .Returns(new DownloadResult(null, true, "\"abc\"", lastModified));
                var sut  = new CacheService(dir.Directory.FullName, http.Object);
                int runs = 0;

                // Act
                var first = sut.ExecuteCached(metadata, typeof(CacheServiceTests),
                                              path =>
                                              {
                                                  ++runs;
                                                  log.Warn("first warning");
                                                  log.Warn("second \"quoted\" warning");
                                                  return new[] { new Metadata(output) };
                                              })
                               .ToArray();
                Metadata[] second;
                using (var collector = new WarningCollector())
                {
                    second = sut.ExecuteCached(metadata, typeof(CacheServiceTests),
                                               path => { ++runs; return Array.Empty<Metadata>(); })
                                .ToArray();
                    CollectionAssert.AreEqual(new[] { "first warning", "second \"quoted\" warning" },
                                              collector.Warnings);
                }

                // Assert
                Assert.AreEqual(1, runs);
                Assert.AreEqual(1, second.Length);
                Assert.IsTrue(JToken.DeepEquals(output, second[0].AllJson),
                              second[0].AllJson.ToString());
                Assert.AreEqual(1, Directory.GetFiles(dir.Directory.FullName).Length);
            }
        }

        [Test]
        public void ExecuteCached_DifferentInputs_UseDifferentEntries()
        {
            using (var dir = new TemporaryDirectory())
            {
                var http = new Mock<IHttpService>();
                http.Setup(h => h.DownloadModule(It.IsAny<Metadata>(), It.IsAny<DownloadOptions?>()))
                    .Returns(new DownloadResult("file.zip", false, "\"abc\"", null));
                var sut = new CacheService(dir.Directory.FullName, http.Object);
                Metadata Mod(string version)
                    => new Metadata(new JObject
                    {
                        ["identifier"] = "Mod",
                        ["download"]   = "https://example.com/mod.zip",
                        ["x_test"]     = version,
                    });
                foreach (var (md, caller) in new[]
                {
                    (Mod("a"), typeof(CacheServiceTests)),
                    (Mod("a"), typeof(CacheServiceTests)),  // same as the first
                    (Mod("b"), typeof(CacheServiceTests)),
                    (Mod("a"), typeof(string)),
                })
                {
                    sut.ExecuteCached(md, caller, path => Array.Empty<Metadata>()).ToArray();
                }

                Assert.AreEqual(3, Directory.GetFiles(dir.Directory.FullName).Length);
            }
        }

        [Test]
        public void ExecuteCached_OtherType_RoundTrips()
        {
            using (var dir = new TemporaryDirectory())
            {
                // Arrange
                var metadata = new Metadata(new JObject
                {
                    ["identifier"] = "Mod",
                    ["download"]   = "https://example.com/mod.zip",
                });
                var http = new Mock<IHttpService>();
                http.Setup(h => h.DownloadModule(It.IsAny<Metadata>(), null))
                    .Returns(new DownloadResult("file.zip", false, "\"abc\"", null));
                http.Setup(h => h.DownloadModule(It.IsAny<Metadata>(), It.IsNotNull<DownloadOptions>()))
                    .Returns(new DownloadResult(null, true, "\"abc\"", null));
                var sut   = new CacheService(dir.Directory.FullName, http.Object);
                var inner = new JObject
                {
                    ["identifier"] = "Mod",
                    ["x_extra"]    = "not a typed field",
                };
                int runs = 0;

                // Act
                sut.ExecuteCached(metadata, typeof(CacheServiceTests),
                                  path => { ++runs; return new CachedThing(42, new Metadata(inner)); });
                var second = sut.ExecuteCached(metadata, typeof(CacheServiceTests),
                                               path => { ++runs; return new CachedThing(0, null); });

                // Assert
                Assert.AreEqual(1, runs);
                Assert.AreEqual(42, second.Number);
                Assert.IsNotNull(second.Module);
                Assert.IsTrue(JToken.DeepEquals(inner, second.Module!.AllJson),
                              second.Module.AllJson.ToString());
            }
        }

        [Test]
        public void ExecuteCached_DifferentResultTypes_UseDifferentEntries()
        {
            using (var dir = new TemporaryDirectory())
            {
                var metadata = new Metadata(new JObject
                {
                    ["identifier"] = "Mod",
                    ["download"]   = "https://example.com/mod.zip",
                });
                var http = new Mock<IHttpService>();
                http.Setup(h => h.DownloadModule(It.IsAny<Metadata>(), It.IsAny<DownloadOptions?>()))
                    .Returns(new DownloadResult("file.zip", false, "\"abc\"", null));
                var sut = new CacheService(dir.Directory.FullName, http.Object);

                sut.ExecuteCached(metadata, typeof(CacheServiceTests), path => 1);
                sut.ExecuteCached(metadata, typeof(CacheServiceTests), path => "one");

                Assert.AreEqual(2, Directory.GetFiles(dir.Directory.FullName).Length);
            }
        }

        internal sealed class CachedThing
        {
            public CachedThing(int number, Metadata? module)
            {
                Number = number;
                Module = module;
            }

            public int       Number { get; }
            public Metadata? Module { get; }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CacheServiceTests));
    }
}
