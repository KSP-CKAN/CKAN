using NUnit.Framework;
using System;
using CKAN;

namespace CKANTests
{
    [TestFixture()]
    public class Registry
    {
        [Test()]
        public void Empty()
        {
            CKAN.Registry registry = CKAN.Registry.Empty();
            Assert.IsInstanceOf<CKAN.Registry>(registry);

        }
    }
}

