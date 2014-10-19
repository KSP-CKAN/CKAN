using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class KSPVersion
    {
        private void BadTarget()
        {
            var any = new CKAN.KSPVersion(null);
            var vshort = new CKAN.KSPVersion("0.23");

            // We can't ask if something targets a non-real (short) version.
            any.Targets(vshort);
        }

        [Test]
        public void MinMax()
        {
            var min = new CKAN.KSPVersion("0.23");
            var max = new CKAN.KSPVersion("0.23");

            min.ToLongMin();
            max.ToLongMax();

            Assert.IsTrue(min.Version() == "0.23.0");
            Assert.IsTrue(max.Version() == "0.23.99"); // Ugh, magic version number.

            Assert.IsTrue(min < max);
            Assert.IsTrue(max > min);
        }

        [Test]
        public void Strings()
        {
            var any = new CKAN.KSPVersion(null);
            var vshort = new CKAN.KSPVersion("0.23");
            var vlong = new CKAN.KSPVersion("0.23.5");

            Assert.AreSame(any.ToString(), null);
            Assert.AreSame(vshort.ToString(), "0.23");
            Assert.AreSame(vlong.ToString(), "0.23.5");
        }

        [Test]
        public void Targets()
        {
            var any = new CKAN.KSPVersion(null);
            var vshort = new CKAN.KSPVersion("0.23");
            var vlong = new CKAN.KSPVersion("0.23.5");

            Assert.IsTrue(any.Targets(vlong));
            Assert.IsTrue(vshort.Targets(vlong));
            Assert.IsTrue(vlong.Targets(vlong));

            Assert.That(BadTarget, Throws.Exception);
        }

        [Test]
        public void Types()
        {
            var v1 = new CKAN.KSPVersion("any");
            Assert.IsTrue(v1.IsAny());
            Assert.IsFalse(v1.IsNotAny());

            var v2 = new CKAN.KSPVersion(null); // Same as "any"
            Assert.IsTrue(v2.IsAny());
            Assert.IsFalse(v2.IsNotAny());

            var vshort = new CKAN.KSPVersion("0.25");
            Assert.IsTrue(vshort.IsShortVersion());
            Assert.IsFalse(vshort.IsLongVersion());
            Assert.IsFalse(vshort.IsAny());

            var vlong = new CKAN.KSPVersion("0.25.2");
            Assert.IsTrue(vlong.IsLongVersion());
            Assert.IsFalse(vlong.IsShortVersion());
            Assert.IsFalse(vlong.IsAny());
        }
    }
}