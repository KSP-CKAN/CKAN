using NUnit.Framework;

namespace Tests.Core.Registry
{
    [TestFixture]
    public class RegistryMultipleRepos
    {
        [Test]
        public void Empty()
        {
            CKAN.Registry registry = CKAN.Registry.Empty();
            Assert.IsInstanceOf<CKAN.Registry>(registry);
        }
    }
}

