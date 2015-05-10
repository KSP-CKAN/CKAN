using NUnit.Framework;

namespace CKANTests
{
    [TestFixture]
    public class Version
    {
        [Test]
        public void Alpha()
        {
            var v1 = new CKAN.Version("apple");
            var v2 = new CKAN.Version("banana");

            // alphabetical test
            Assert.That(v1.IsLessThan(v2));
        }

        [Test]
        public void Basic()
        {
            var v0 = new CKAN.Version("1.2.0");
            var v1 = new CKAN.Version("1.2.0");
            var v2 = new CKAN.Version("1.2.1");

            Assert.That(v1.IsLessThan(v2));
            Assert.That(v2.IsGreaterThan(v1));
            Assert.That(v1.IsEqualTo(v0));
        }

        [Test]
        public void Complex()
        {
            var v1 = new CKAN.Version("v6a12");
            var v2 = new CKAN.Version("v6a5");
            Assert.That(v2.IsLessThan(v1));
            Assert.That(v1.IsGreaterThan(v2));
            Assert.That(! v1.IsEqualTo(v2));
        }

        [Test]
        public void Epoch()
        {
            var v1 = new CKAN.Version("1.2.0");
            var v2 = new CKAN.Version("1:1.2.0");

            Assert.That(v1.IsLessThan(v2));
        }

        [Test]
        public void DllVersion()
        {
            var v1 = new CKAN.DllVersion();
            Assert.AreEqual("autodetected dll", v1.ToString());
        }

        [Test]
        public void ProvidesVersion()
        {
            var v1 = new CKAN.ProvidesVersion("SomeModule");
            Assert.AreEqual("provided by SomeModule", v1.ToString());
        }

        [Test]
        public void AGExt()
        {
            var v1 = new CKAN.Version("1.20");
            var v2 = new CKAN.Version("1.22a");

            Assert.That(v2.IsGreaterThan(v1));
        }

        [Test]
        public void StringComparison()
        {
            var str = CKAN.Version.StringComp("", "");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.StringComp("foo", "foo");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.StringComp("foobar", "foobaz");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.StringComp("barbaz", "foobar");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.StringComp("foobaz", "foobar");

            Assert.That(str.compare_to, Is.GreaterThan(0));            
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.StringComp("foo12", "foo51");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("12", str.remainder1);
            Assert.AreEqual("51", str.remainder2);

            str = CKAN.Version.StringComp("foo51", "foo12");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("51", str.remainder1);
            Assert.AreEqual("12", str.remainder2);

            str = CKAN.Version.StringComp("42bar", "bar42");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual("42bar", str.remainder1);
            Assert.AreEqual("42", str.remainder2);

            str = CKAN.Version.StringComp("foo0bar", "foo1bar");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("0bar", str.remainder1);
            Assert.AreEqual("1bar", str.remainder2);

            str = CKAN.Version.StringComp("f0bar", "foo1bar");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual("0bar", str.remainder1);
            Assert.AreEqual("1bar", str.remainder2);

            str = CKAN.Version.StringComp(".25.0", ".25.0");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("25.0", str.remainder1);
            Assert.AreEqual("25.0", str.remainder2);

            str = CKAN.Version.StringComp(".25.0", ".25.99");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("25.0", str.remainder1);
            Assert.AreEqual("25.99", str.remainder2);
        }

        [Test]
        public void NumberComparison()
        {
            var str = CKAN.Version.NumComp("0", "0");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.NumComp("1", "2");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.NumComp("2", "1");

            Assert.That(str.compare_to, Is.GreaterThan(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.NumComp("001", "02");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.NumComp("02", "001");

            Assert.That(str.compare_to, Is.GreaterThan(0));
            Assert.AreEqual("", str.remainder1);
            Assert.AreEqual("", str.remainder2);

            str = CKAN.Version.NumComp("0foo", "0bar");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("foo", str.remainder1);
            Assert.AreEqual("bar", str.remainder2);

            str = CKAN.Version.NumComp("3foo", "7bar");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual("foo", str.remainder1);
            Assert.AreEqual("bar", str.remainder2);

            str = CKAN.Version.NumComp("7foo", "3bar");

            Assert.That(str.compare_to, Is.GreaterThan(0));
            Assert.AreEqual("foo", str.remainder1);
            Assert.AreEqual("bar", str.remainder2);

            str = CKAN.Version.NumComp("00foo11", "11foo00");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual("foo11", str.remainder1);
            Assert.AreEqual("foo00", str.remainder2);

            str = CKAN.Version.NumComp("1337stuff", "1337stuff");

            Assert.That(str.compare_to, Is.EqualTo(0));
            Assert.AreEqual("stuff", str.remainder1);
            Assert.AreEqual("stuff", str.remainder2);

            str = CKAN.Version.NumComp("13.5a", "12.8.2b");

            Assert.That(str.compare_to, Is.GreaterThan(0));
            Assert.AreEqual(".5a", str.remainder1);
            Assert.AreEqual(".8.2b", str.remainder2);

            str = CKAN.Version.NumComp("101.0", "12.2");

            Assert.That(str.compare_to, Is.GreaterThan(0));
            Assert.AreEqual(".0", str.remainder1);
            Assert.AreEqual(".2", str.remainder2);

            str = CKAN.Version.NumComp("12.2", "101.0");

            Assert.That(str.compare_to,Is.LessThan(0));
            Assert.AreEqual(".2", str.remainder1);
            Assert.AreEqual(".0", str.remainder2);
        }

    }
}