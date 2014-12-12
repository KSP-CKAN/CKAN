using NUnit.Framework;
using System;
using CKAN;

namespace CKANTests
{
    [TestFixture()]
    public class KSPManager
    {
        [Test()]
        public void InstanceAccessorState()
        {
            // Make sure that LoadInstancesFromRegistry sets the
            // internal instances_loaded variable correctly.
            Assert.IsFalse(CKAN.KSPManager.instances_loaded);
            CKAN.KSPManager.LoadInstancesFromRegistry(NullUser.User);
            Assert.IsTrue(CKAN.KSPManager.instances_loaded);
        }
    }
}

