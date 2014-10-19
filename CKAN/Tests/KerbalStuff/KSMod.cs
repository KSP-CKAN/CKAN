using NUnit.Framework;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CKAN.KerbalStuff;

namespace Tests
{
    [TestFixture()]
    public class KSMod
    {
        [Test()]
        public void Inflate()
        {
            string json = @"{ 'foo': 'bar'}";
            var meta = JObject.Parse(json);

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
    }
}

