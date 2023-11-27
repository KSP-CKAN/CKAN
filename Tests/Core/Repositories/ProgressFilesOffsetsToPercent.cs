using System;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using CKAN;
#if NETFRAMEWORK
using CKAN.Extensions;
#endif

namespace Tests.Core.Repositories
{
    [TestFixture]
    public class ProgressFilesOffsetsToPercentTests
    {
        [Test,
            // Empty inputs
            TestCase(new long[] { },
                     new long[] { },
                     new int[]  { }),
            // Single file, a few offsets translated to percents with one redundant
            TestCase(new long[] { 2000                             },
                     new long[] { 200, 600, 1300, 1301, 1800, 2000 },
                     new int[]  {  10,  30,   65,   -1,   90,  100 }),
            // Multiple files with advancement(one redundant)
            TestCase(new long[] { 100,         200,     100          },
                     new long[] { 50, 100, -1, 100, -1,   0, 50,  -1 },
                     new int[]  { 12, 25,  -1,  50, 75,  -1, 87, 100 }),
        ]
        public void Report_WithFilesAndOffsets_CorrectPercents(
            long[] fileSizes,
            // Negative means advance to next file (nullables are not allowed in attributes)
            long[] offsets,
            // Negative means no update expected
            int[]  correctPercents)
        {
            // Arrange
            var  notifier        = new ManualResetEvent(false);
            int? lastProgress    = null;
            var  percentProgress = new Progress<int>(p =>
            {
                lastProgress = p;
                // Progress<> notifies in a separate thread, so we need to wait for its updates
                notifier.Set();
            });
            var  progress        = new ProgressFilesOffsetsToPercent(
                                       percentProgress, fileSizes);

            // Act / Assert
            foreach (var (offset, correctPercent) in offsets.Zip(correctPercents))
            {
                if (offset >= 0)
                {
                    progress.Report(offset);
                }
                else
                {
                    progress.NextFile();
                }

                if (correctPercent >= 0)
                {
                    notifier.WaitOne();
                    notifier.Reset();

                    Assert.AreEqual(correctPercent, lastProgress);
                }
            }
        }
    }
}
