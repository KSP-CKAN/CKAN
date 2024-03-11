using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN
{
    /// <summary>
    /// Accepts progress updates in terms of offsets within a group of files
    /// and translates them into percentages across the whole operation.
    /// </summary>
    public class ProgressFilesOffsetsToPercent : IProgress<long>
    {
        /// <summary>
        /// Initialize an offset-to-percent progress translator
        /// </summary>
        /// <param name="percentProgress">The upstream progress object expecting percentages</param>
        /// <param name="sizes">Sequence of sizes of files in our group</param>
        public ProgressFilesOffsetsToPercent(IProgress<int> percentProgress,
                                             IEnumerable<long> sizes)
        {
            this.percentProgress = percentProgress;
            this.sizes           = sizes.ToArray();
            totalSize            = this.sizes.Sum();
        }

        /// <summary>
        /// The IProgress member called when we advance within the current file
        /// </summary>
        /// <param name="currentFileOffset">How far into the current file we are</param>
        public void Report(long currentFileOffset)
        {
            if (totalSize > 0)
            {
                var percent = basePercent + (int)(100 * currentFileOffset / totalSize);
                // Only report each percentage once, to avoid spamming UI calls
                if (percent > lastPercent)
                {
                    percentProgress.Report(percent);
                    lastPercent = percent;
                }
            }
        }

        /// <summary>
        /// Call this when you move on from one file to the next
        /// </summary>
        public void NextFile()
        {
            doneSize += sizes[currentIndex];
            if (totalSize > 0)
            {
                basePercent = (int)(100 * doneSize / totalSize);
            }
            ++currentIndex;
            if (basePercent > lastPercent)
            {
                percentProgress.Report(basePercent);
                lastPercent = basePercent;
            }
        }

        private readonly IProgress<int> percentProgress;
        private readonly long[]         sizes;
        private readonly long           totalSize;
        private          long           doneSize     = 0;
        private          int            currentIndex = 0;
        private          int            basePercent  = 0;
        private          int            lastPercent  = -1;
    }
}
