using System;
using System.Collections.Generic;

using NUnit.Framework;

using CKAN.Extensions;

namespace Tests.Core.Extensions
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [TestCaseSource(nameof(TimeSpanCases))]
        public TimeSpan Sum_Timespans_Works(TimeSpan[] spans)
            => spans.Sum();

        private static IEnumerable<TestCaseData> TimeSpanCases()
        {
            yield return new TestCaseData(new TimeSpan[] { })
                .Returns(TimeSpan.FromSeconds(0));
            yield return new TestCaseData(new TimeSpan[]
            {
                TimeSpan.FromSeconds(10),
            }).Returns(TimeSpan.FromSeconds(10));
            yield return new TestCaseData(new TimeSpan[]
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
                TimeSpan.FromSeconds(30),
            }).Returns(TimeSpan.FromSeconds(60));
            yield return new TestCaseData(new TimeSpan[]
            {
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(20),
                TimeSpan.FromSeconds(30),
            }).Returns(TimeSpan.FromSeconds(110));
        }
    }
}
