using NUnit.Framework;
using log4net.Core;
using CKAN.NetKAN;

namespace Tests.NetKAN
{
    [TestFixture]
    public class CmdLineOptionsTests
    {
        [Test]
        public void GetLogLevel_EachLevel_Works()
        {
            Assert.AreEqual(Level.Warn,  new CmdLineOptions().GetLogLevel());
            Assert.AreEqual(Level.Info,  new CmdLineOptions() {Verbose = true}.GetLogLevel());
            Assert.AreEqual(Level.Debug, new CmdLineOptions() {Debug = true}.GetLogLevel());
        }

        [TestCase("all", ExpectedResult = null)]
        [TestCase(null,  ExpectedResult = 1)]
        [TestCase("1",   ExpectedResult = 1)]
        [TestCase("2",   ExpectedResult = 2)]
        [TestCase("3",   ExpectedResult = 3)]
        public int? ParseReleases_WithValues_Works(string? val)
            => new CmdLineOptions() {Releases = val}.ParseReleases();

        [TestCase(null, ExpectedResult = 0)]
        [TestCase("1",  ExpectedResult = 1)]
        [TestCase("2",  ExpectedResult = 2)]
        [TestCase("3",  ExpectedResult = 3)]
        public int ParseSkipReleases_WithValues_Works(string? val)
            => new CmdLineOptions() {SkipReleases = val}.ParseSkipReleases();

        [TestCase(null)]
        [TestCase("1.0")]
        public void ParseHighestVersion_WithValues_Works(string? val)
        {
            var result = new CmdLineOptions() {HighestVersion = val}.ParseHighestVersion();
            Assert.AreEqual(val, result?.ToString());
        }

        [TestCase(null)]
        [TestCase("1.0")]
        public void ParseHighestPrereleaseVersion_WithValues_Works(string? val)
        {
            var result = new CmdLineOptions() {HighestVersionPrerelease = val}.ParseHighestPrereleaseVersion();
            Assert.AreEqual(val, result?.ToString());
        }
    }
}
