using NUnit.Framework;

using CKAN;
using CKAN.GUI;

namespace Tests.GUI
{
    [TestFixture]
    public class ReleaseStatusItemTests
    {
        [TestCase(ReleaseStatus.stable,      ExpectedResult = "Stable - Normal releases")]
        [TestCase(ReleaseStatus.testing,     ExpectedResult = "Testing - Pre-releases for adventurous users")]
        [TestCase(ReleaseStatus.development, ExpectedResult = "Development - Bleeding edge unstable")]
        public string ToString_AfterConstructor_Works(ReleaseStatus stat)
            => new ReleaseStatusItem(stat).ToString();
    }
}
