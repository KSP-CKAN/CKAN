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
    }
}

