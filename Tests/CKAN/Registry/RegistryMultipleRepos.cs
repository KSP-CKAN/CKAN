using CKAN;
using NUnit.Framework;
using System;

namespace CKANTests
{
    [TestFixture]
    public class RegistryMultipleRepos
    {
        private CKAN.Registry registry;

        [SetUp]
        public void Setup()
        {
            // Provide an empty registry before each test.
            registry = CKAN.Registry.Empty();
            Assert.IsNotNull(registry);
        }

        [Test]
        public void Empty()
        {
            CKAN.Registry registry = CKAN.Registry.Empty();
            Assert.IsInstanceOf<CKAN.Registry>(registry);
        }

    }
}

