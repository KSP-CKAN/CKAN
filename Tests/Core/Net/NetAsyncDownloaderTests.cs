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
            var downloader = new NetAsyncDownloader(new NullUser(), () => null);
            var fromPath   = TestData.DataFile(pathWithinTestData);
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
                    FileAssert.Exists(target.filename);
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
            var downloader = new NetAsyncDownloader(new NullUser(), () => null);
            var fromPaths  = pathsWithinTestData.Select(TestData.DataFile).ToArray();
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
                        FileAssert.Exists(t.filename);
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
            // A URL in the middle is bad
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
            // Last URL is bad
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
            TestCase("DoesNotExist1.zip",
                     "gh221.zip",
                     "DoesNotExist2.zip",
                     "ModuleManager-2.5.1.zip",
                     "DoesNotExist3.zip",
                     "ZipWithUnicodeChars.zip",
                     "DoesNotExist4.zip",
                     "DogeCoinPlugin.zip",
                     "DoesNotExist5.zip",
                     "DogeCoinFlag-1.01-corrupt.zip",
                     "DoesNotExist6.zip",
                     "CKAN-meta-testkan.zip",
                     "DoesNotExist7.zip",
                     "ZipWithBadChars.zip",
                     "DoesNotExist8.zip",
                     "DogeCoinFlag-1.01-no-dir-entries.zip",
                     "DoesNotExist9.zip",
                     "DogeTokenFlag-1.01.zip",
                     "DoesNotExist10.zip",
                     "DogeCoinFlag-1.01.zip",
                     "DoesNotExist11.zip",
                     "DogeCoinFlag-1.01-LZMA.zip",
                     "DoesNotExist12.zip",
                     "DogeCoinFlag-1.01-avc.zip",
                     "DoesNotExist13.zip",
                     "DogeCoinFlag-extra-files.zip",
                     "DoesNotExist14.zip"),
        ]
        public void DownloadAndWait_WithSomeInvalidUrls_ThrowsDownloadErrorsKraken(
            params string[] pathsWithinTestData)
        {
            // Arrange
            var downloader   = new NetAsyncDownloader(new NullUser(), () => null);
            var fromPaths    = pathsWithinTestData.Select(p => Path.GetFullPath(TestData.DataFile(p))).ToArray();
            var targets      = fromPaths.Select(p => new NetAsyncDownloader.DownloadTargetFile(new Uri(p),
                                                                                               Path.GetTempFileName()))
                                        .ToArray();
            var badTargets   = targets.Zip(fromPaths)
                                      .Where(tuple => !File.Exists(tuple.Second))
                                      .Select(tuple => tuple.First)
                                      .ToArray();
            var validTargets = targets.Except(badTargets);

            // Act / Assert
            var exception = Assert.Throws<DownloadErrorsKraken>(() =>
            {
                downloader.DownloadAndWait(targets);
            });
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEquivalent(badTargets,
                                               exception?.Exceptions.Select(kvp => kvp.Key).ToArray());
                foreach (var kvp in exception?.Exceptions!)
                {
                    var baseExc = kvp.Value.GetBaseException() as FileNotFoundException;
                    Assert.AreEqual(fromPaths[Array.IndexOf(targets, kvp.Key)],
                                    baseExc?.FileName);
                }
                foreach (var t in validTargets)
                {
                    FileAssert.Exists(t.filename);
                }
            });
            foreach (var t in targets)
            {
                File.Delete(t.filename);
            }
        }
    }
}
