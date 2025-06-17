using System.IO;

using NUnit.Framework;

using CKAN;
using CKAN.Configuration;
using Tests.Data;

namespace Tests.Core.Configuration
{
    [TestFixture]
    public sealed class StabilityToleranceConfigTests
    {
        [Test]
        public void Load_FromSave_Works()
        {
            // Arrange
            using (var dir = new TemporaryDirectory())
            {
                var path = Path.Combine(dir.Path.FullName, "stc.json");

                // Act
                var orig = new StabilityToleranceConfig(path)
                {
                    OverallStabilityTolerance = ReleaseStatus.development
                };
                var changedCount = 0;
                orig.Changed += (ident, val) => ++changedCount;
                orig.SetModStabilityTolerance("mod1", ReleaseStatus.stable);
                orig.SetModStabilityTolerance("mod2", ReleaseStatus.testing);
                orig.SetModStabilityTolerance("mod3", null);
                var loaded = new StabilityToleranceConfig(path);

                // Assert
                Assert.AreEqual(3, changedCount);
                Assert.AreEqual(ReleaseStatus.development, loaded.OverallStabilityTolerance);
                Assert.AreEqual(ReleaseStatus.stable,      loaded.ModStabilityTolerance("mod1"));
                Assert.AreEqual(ReleaseStatus.testing,     loaded.ModStabilityTolerance("mod2"));
                Assert.AreEqual(null,                      loaded.ModStabilityTolerance("mod3"));
            }
        }
    }
}
