using CKAN;
using NUnit.Framework;

namespace CKANTests
{
    [TestFixture]
    public class KSPManager
    {
        [Test]
        public void InstanceAccessorState()
        {
            // Make sure that LoadInstancesFromRegistry sets the
            // internal instances_loaded variable correctly.
            CKAN.KSPManager manager = new CKAN.KSPManager(NullUser.User);            
            Assert.IsTrue(manager.instances_loaded);
        }
    }
}

