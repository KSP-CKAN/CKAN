namespace Tests {

    using NUnit.Framework;
    using System;
    using CKAN;

    [TestFixture()]
    public class Version {
        [Test()]
        public void Basic ()
        {
            var v0 = new CKAN.Version ("1.2.0");
            var v1 = new CKAN.Version ("1.2.0");
            var v2 = new CKAN.Version ("1.2.1");

            Assert.That (v1.IsLessThan (v2));
            Assert.That (v2.IsGreaterThan (v1));
            Assert.That (v1.IsEqualTo (v0));
        }

        [Test()]
        public void Epoch () {
            var v1 = new CKAN.Version ("1.2.0");
            var v2 = new CKAN.Version ("1:1.2.0");

            Assert.That (v1.IsLessThan (v2));
        }
    }
}

