using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Sources.SourceForge;
using CKAN.NetKAN.Services;

using Tests.Data;

namespace Tests.NetKAN.Sources.SourceForge
{
    [TestFixture]
    [Category("Online")]
    public sealed class SourceForgeApiTests
    {
        [Test]
        public void GetMod_KSRe_Works()
        {
            // Arrange
            using (var dir   = new TemporaryDirectory())
            using (var cache = new NetFileCache(dir.Path.FullName))
            {
                var sut            = new SourceForgeApi(new CachingHttpService(cache));
                var sourceForgeRef = new SourceForgeRef(new RemoteRef("#/ckan/sourceforge/ksp1.ksre.p"));

                // Act
                var mod = sut.GetMod(sourceForgeRef);

                // Assert
                Assert.IsNotNull(mod.Title);
                Assert.IsNotNull(mod.Description);
                Assert.IsNotNull(mod.HomepageLink);
                Assert.IsNotNull(mod.RepositoryLink);
                Assert.That(mod.Versions.Length, Is.GreaterThanOrEqualTo(1));
                var first = mod.Versions.First();
                Assert.IsNotNull(first.Title);
                Assert.IsNotNull(first.Timestamp);
                Assert.IsNotNull(first.Link);
            }
        }
    }
}
