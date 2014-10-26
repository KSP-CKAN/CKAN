using NUnit.Framework;
using System;
using CKAN;

namespace CKANTests
{
    [TestFixture()]
    public class KSP
    {
        [Test()]
        public void IsGameDir()
        {
            Assert.IsTrue(CKAN.KSP.IsKspDir(Tests.TestData.good_ksp_dir()));

            foreach (string dir in Tests.TestData.bad_ksp_dirs())
            {
                Assert.IsFalse(CKAN.KSP.IsKspDir(dir));
            }
        }

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

