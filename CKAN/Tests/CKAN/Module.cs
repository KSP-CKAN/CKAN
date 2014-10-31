using NUnit.Framework;
using System;
using CKAN;
using Tests;

namespace CKANTests
{
    [TestFixture()]
    public class Module
    {
        [Test]
        public void CompatibleWith()
        {
            CkanModule module = CkanModule.FromJson(TestData.kOS_014());

            Assert.IsTrue(module.IsCompatibleKSP("0.24.2"));
        }

        [Test]
        public void StandardName()
        {
            CkanModule module = CkanModule.FromJson(TestData.kOS_014());

            Assert.AreEqual(module.StandardName(), "kOS-0.14.zip");
        }

        [Test]
        public void MetaData()
        {
            CkanModule module = CkanModule.FromJson (TestData.kOS_014 ());

            // TODO: Test all the metadata here!
            Assert.AreEqual("https://github.com/KSP-KOS/KOS/issues", module.resources.bugtracker.ToString());
        }

        [Test]
        public void FilterRead()
        {
            CkanModule module = CkanModule.FromJson(TestData.DogeCoinFlag_101());

            // Assert known things about this mod.
            Assert.IsNotNull(module.install[0].filter);
            Assert.IsNotNull(module.install[0].filter_regexp);

            Assert.AreEqual(2, module.install[0].filter.Count);
        }

    }
}

