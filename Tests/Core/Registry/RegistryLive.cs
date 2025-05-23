using System.Collections.Generic;

using NUnit.Framework;
using Tests.Data;

using CKAN;
using CKAN.Configuration;
using CKAN.Versioning;

namespace Tests.Core.Registry
{
    /// <summary>
    /// These are tests on a live registry extracted from one of the developers'
    /// systems.
    /// </summary>

    [TestFixture]
    public class RegistryLive
    {
        [Test]
        public void LatestAvailable()
        {
            var user = new NullUser();
            var repo = new Repository("test", "https://github.com/");
            using (var temp_ksp = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user, new Dictionary<Repository, RepositoryData>
            {
                { repo, RepositoryData.FromJson(TestData.TestRepository(), null)! },
            }))
            using (var regMgr = RegistryManager.Instance(temp_ksp.KSP, repoData.Manager,
                                                         new Repository[] { repo }))
            {
                var registry = regMgr.registry;
                var module = registry.LatestAvailable("AGExt", new StabilityToleranceConfig(""),
                                                               new GameVersionCriteria(temp_ksp.KSP.Version()));

                Assert.AreEqual("AGExt", module?.identifier);
                Assert.AreEqual("1.24a", module?.version.ToString());
            }
        }
    }
}
