using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using CKAN;

using Tests.Data;

namespace Tests.Core.Net
{
    [TestFixture]
    public class NetAsyncDownloaderTests
    {
        [Test,
            TestCase("DogeCoinFlag-1.01.zip"),
            TestCase("DogeCoinFlag-1.01-avc.zip"),
            TestCase("DogeCoinFlag-extra-files.zip"),
            TestCase("DogeCoinFlag-1.01-corrupt.zip"),
        ]
        public void DownloadAndWait_WithValidFileUrl_SetsTargetSize(string pathWithinTestData)
        {
            // Arrange
            var downloader = new NetAsyncDownloader(new NullUser());
            var fromPath   = TestData.DataDir(pathWithinTestData);
            var target     = new NetAsyncDownloader.DownloadTargetFile(new Uri(fromPath),
                                                                       Path.GetTempFileName());
            var targets    = new NetAsyncDownloader.DownloadTarget[] { target };
            var origSize   = new FileInfo(fromPath).Length;

            // Act
            try
            {
                downloader.DownloadAndWait(targets);
                var realSize = new FileInfo(target.filename).Length;

                // Assert
                Assert.Multiple(() =>
                {
                    Assert.IsTrue(File.Exists(target.filename));
                    Assert.AreEqual(origSize, realSize,    "Size on disk should match original");
                    Assert.AreEqual(realSize, target.size, "Target size should match size on disk");
                    Assert.AreEqual(origSize, target.size, "Target size should match original");
                });
            }
            finally
            {
                File.Delete(target.filename);
            }
        }

        [Test,
            TestCase("gh221.zip",
                     "ModuleManager-2.5.1.zip",
                     "ZipWithUnicodeChars.zip",
                     "DogeCoinPlugin.zip",
                     "DogeCoinFlag-1.01-corrupt.zip",
                     "CKAN-meta-testkan.zip",
                     "ZipWithBadChars.zip",
                     "DogeCoinFlag-1.01-no-dir-entries.zip",
                     "DogeTokenFlag-1.01.zip",
                     "DogeCoinFlag-1.01.zip",
                     "DogeCoinFlag-1.01-LZMA.zip",
                     "DogeCoinFlag-1.01-avc.zip",
                     "DogeCoinFlag-extra-files.zip"),
        ]
        public void DownloadAndWait_WithValidFileUrls_SetsTargetsSize(params string[] pathsWithinTestData)
        {
            // Arrange
            var downloader = new NetAsyncDownloader(new NullUser());
            var fromPaths  = pathsWithinTestData.Select(p => TestData.DataDir(p)).ToArray();
            var targets    = fromPaths.Select(p => new NetAsyncDownloader.DownloadTargetFile(new Uri(p),
                                                                                             Path.GetTempFileName()))
                                      .ToArray();
            var origSizes  = fromPaths.Select(p => new FileInfo(p).Length).ToArray();

            // Act
            try
            {
                downloader.DownloadAndWait(targets);
                var realSizes   = targets.Select(t => new FileInfo(t.filename).Length).ToArray();
                var targetSizes = targets.Select(t => t.size).ToArray();

                // Assert
                Assert.Multiple(() =>
                {
                    foreach (var t in targets)
                    {
                        Assert.IsTrue(File.Exists(t.filename));
                    }
                    CollectionAssert.AreEquivalent(origSizes, realSizes,   "Sizes on disk should match originals");
                    CollectionAssert.AreEquivalent(realSizes, targetSizes, "Target sizes should match sizes on disk");
                    CollectionAssert.AreEquivalent(origSizes, targetSizes, "Target sizes should match originals");
                });
            }
            finally
            {
                foreach (var t in targets)
                {
                    File.Delete(t.filename);
                }
            }
        }

        [Test,
            // Only one bad URL
            TestCase("DoesNotExist.zip"),
            // First URL is bad
            TestCase("DoesNotExist.zip",
                     "gh221.zip",
                     "ModuleManager-2.5.1.zip",
                     "ZipWithUnicodeChars.zip",
                     "DogeCoinPlugin.zip",
                     "DogeCoinFlag-1.01-corrupt.zip",
                     "CKAN-meta-testkan.zip",
                     "ZipWithBadChars.zip",
                     "DogeCoinFlag-1.01-no-dir-entries.zip",
                     "DogeTokenFlag-1.01.zip",
                     "DogeCoinFlag-1.01.zip",
                     "DogeCoinFlag-1.01-LZMA.zip",
                     "DogeCoinFlag-1.01-avc.zip",
                     "DogeCoinFlag-extra-files.zip"),
            // Last URL is bad
            TestCase("gh221.zip",
                     "ModuleManager-2.5.1.zip",
                     "ZipWithUnicodeChars.zip",
                     "DogeCoinPlugin.zip",
                     "DogeCoinFlag-1.01-corrupt.zip",
                     "CKAN-meta-testkan.zip",
                     "ZipWithBadChars.zip",
                     "DogeCoinFlag-1.01-no-dir-entries.zip",
                     "DoesNotExist.zip",
                     "DogeTokenFlag-1.01.zip",
                     "DogeCoinFlag-1.01.zip",
                     "DogeCoinFlag-1.01-LZMA.zip",
                     "DogeCoinFlag-1.01-avc.zip",
                     "DogeCoinFlag-extra-files.zip"),
            // A URL in the middle is bad
            TestCase("gh221.zip",
                     "ModuleManager-2.5.1.zip",
                     "ZipWithUnicodeChars.zip",
                     "DogeCoinPlugin.zip",
                     "DogeCoinFlag-1.01-corrupt.zip",
                     "CKAN-meta-testkan.zip",
                     "ZipWithBadChars.zip",
                     "DogeCoinFlag-1.01-no-dir-entries.zip",
                     "DogeTokenFlag-1.01.zip",
                     "DogeCoinFlag-1.01.zip",
                     "DogeCoinFlag-1.01-LZMA.zip",
                     "DogeCoinFlag-1.01-avc.zip",
                     "DogeCoinFlag-extra-files.zip",
                     "DoesNotExist.zip"),
            // Every other URL is bad
            TestCase("DoesNotExist.zip",
                     "gh221.zip",
                     "DoesNotExist.zip",
                     "ModuleManager-2.5.1.zip",
                     "DoesNotExist.zip",
                     "ZipWithUnicodeChars.zip",
                     "DoesNotExist.zip",
                     "DogeCoinPlugin.zip",
                     "DoesNotExist.zip",
                     "DogeCoinFlag-1.01-corrupt.zip",
                     "DoesNotExist.zip",
                     "CKAN-meta-testkan.zip",
                     "DoesNotExist.zip",
                     "ZipWithBadChars.zip",
                     "DoesNotExist.zip",
                     "DogeCoinFlag-1.01-no-dir-entries.zip",
                     "DoesNotExist.zip",
                     "DogeTokenFlag-1.01.zip",
                     "DoesNotExist.zip",
                     "DogeCoinFlag-1.01.zip",
                     "DoesNotExist.zip",
                     "DogeCoinFlag-1.01-LZMA.zip",
                     "DoesNotExist.zip",
                     "DogeCoinFlag-1.01-avc.zip",
                     "DoesNotExist.zip",
                     "DogeCoinFlag-extra-files.zip",
                     "DoesNotExist.zip"),
        ]
        public void DownloadAndWait_WithSomeInvalidUrls_ThrowsDownloadErrorsKraken(
            params string[] pathsWithinTestData)
        {
            // Arrange
            var downloader   = new NetAsyncDownloader(new NullUser());
            var fromPaths    = pathsWithinTestData.Select(p => Path.GetFullPath(TestData.DataDir(p))).ToArray();
            var targets      = fromPaths.Select(p => new NetAsyncDownloader.DownloadTargetFile(new Uri(p),
                                                                                               Path.GetTempFileName()))
                                        .ToArray();
            var badIndices   = fromPaths.Select((p, i) => new Tuple<int, bool>(i, File.Exists(p)))
                                        .Where(tuple => !tuple.Item2)
                                        .Select(tuple => tuple.Item1)
                                        .ToArray();
            var validTargets = targets.Where((t, i) => !badIndices.Contains(i));

            // Act / Assert
            var exception = Assert.Throws<DownloadErrorsKraken>(() =>
            {
                downloader.DownloadAndWait(targets);
            });
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEquivalent(badIndices, exception.Exceptions.Select(kvp => kvp.Key).ToArray());
                foreach (var kvp in exception.Exceptions)
                {
                    var baseExc = kvp.Value.GetBaseException() as FileNotFoundException;
                    Assert.AreEqual(fromPaths[kvp.Key], baseExc.FileName);
                }
                foreach (var t in validTargets)
                {
                    Assert.IsTrue(File.Exists(t.filename));
                }
            });
        }
    }
}
