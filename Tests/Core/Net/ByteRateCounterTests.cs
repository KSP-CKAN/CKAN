using System;
using System.Threading;

using NUnit.Framework;

using CKAN;

namespace Tests.Core
{
    [TestFixture]
    public sealed class ByteRateCounterTests
    {
        [Test]
        public void PercentTimeLeftSummary_WithData_Correct()
        {
            // Arrange
            var brc = new ByteRateCounter()
            {
                Size      = 100000,
                BytesLeft =  90000,
            };
            var bytesPerSec = 10000 / 3;
            var secondsLeft = 90000 / bytesPerSec;
            var timeLeft    = TimeSpan.FromSeconds(secondsLeft);

            // Act
            brc.Start();
            Thread.Sleep(3500);
            brc.Stop();

            // Assert
            Assert.AreEqual(10,                    brc.Percent,        2);
            Assert.AreEqual(bytesPerSec,           brc.BytesPerSecond, 50);
            Assert.AreEqual(timeLeft.TotalSeconds, brc.TimeLeft.TotalSeconds, 5);
            CollectionAssert.Contains(new string[]
                                      {
                                          $"{secondsLeft-1} sec",
                                          $"{secondsLeft} sec",
                                      },
                                      brc.TimeLeftString);
            CollectionAssert.Contains(new string[]
                                      {
                                          $"{CkanModule.FmtSize(brc.BytesPerSecond)}/sec - {CkanModule.FmtSize(90000)} ({secondsLeft-1} sec) left - 10%",
                                          $"{CkanModule.FmtSize(brc.BytesPerSecond)}/sec - {CkanModule.FmtSize(90000)} ({secondsLeft} sec) left - 10%",
                                      },
                                      brc.Summary);
        }
    }
}
