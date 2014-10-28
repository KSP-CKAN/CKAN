using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NetKAN.KerbalStuffTests
{
    [TestFixture]
    public class KSMod
    {
        [Test]
        public void Inflate()
        {
            string json = @"{ 'foo': 'bar'}";
            JObject meta = JObject.Parse(json);

            // Sanity check.
            Assert.AreEqual((string) meta["foo"], "bar");

            // This should do nothing.
            CKAN.KerbalStuff.KSMod.Inflate(meta, "foo", "baz");
            Assert.AreEqual((string) meta["foo"], "bar");

            // We shouldn't have an author field.
            Assert.IsNull(meta["author"]);

            // This should add a key.
            CKAN.KerbalStuff.KSMod.Inflate(meta, "author", "Jeb");
            Assert.AreEqual((string) meta["author"], "Jeb");
        }

        [Test]
        public void KSHome()
        {
            var ks = new CKAN.KerbalStuff.KSMod();
            ks.name = "foo bar";
            ks.id = 123;

            Assert.AreEqual("https://kerbalstuff.com/mod/123/foo%20bar", ks.KSHome());
        }
    }
}