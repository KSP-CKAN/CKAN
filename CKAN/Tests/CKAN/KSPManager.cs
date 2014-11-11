using NUnit.Framework;
using System;

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
            CKAN.KSPManager.LoadInstancesFromRegistry();
            Assert.IsTrue(CKAN.KSPManager.instances_loaded);
        }
    }
}

