using System;
using System.IO;
using System.Collections.Generic;
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
            var target     = new CKAN.Net.DownloadTarget(new List<Uri> { new Uri(fromPath) },
                                                         Path.GetTempFileName());
            var targets    = new CKAN.Net.DownloadTarget[] { target };
            var realSize   = new FileInfo(fromPath).Length;

            // Act
            try
            {
                downloader.DownloadAndWait(targets);

                // Assert
                Assert.IsTrue(File.Exists(target.filename));
                Assert.AreEqual(realSize, target.size);
            }
            finally
            {
                File.Delete(target.filename);
            }
        }

        [Test,
            TestCase("DogeCoinFlag-1.01.zip",
                     "DogeCoinFlag-1.01-avc.zip",
                     "DogeCoinFlag-extra-files.zip",
                     "DogeCoinFlag-1.01-corrupt.zip"),
        ]
        public void DownloadAndWait_WithValidFileUrls_SetsTargetsSize(params string[] pathsWithinTestData)
        {
            // Arrange
            var downloader = new NetAsyncDownloader(new NullUser());
            var fromPaths  = pathsWithinTestData.Select(p => TestData.DataDir(p)).ToArray();
            var targets    = fromPaths.Select(p => new CKAN.Net.DownloadTarget(new List<Uri> { new Uri(p) },
                                                                          Path.GetTempFileName()))
                                      .ToArray();
            var realSizes  = fromPaths.Select(p => new FileInfo(p).Length).ToArray();

            // Act
            try
            {
                downloader.DownloadAndWait(targets);

                // Assert
                foreach (var t in targets)
                {
                    Assert.IsTrue(File.Exists(t.filename));
                }
                CollectionAssert.AreEquivalent(realSizes, targets.Select(t => t.size).ToArray());
            }
            finally
            {
                foreach (var t in targets)
                {
                    File.Delete(t.filename);
                }
            }
        }
    }
}
