using NUnit.Framework;
using Newtonsoft.Json;
using System;

namespace Tests.NetKAN
{
    [TestFixture()]
    public class AVC
    {
        [Test()]
        public void Json()
        {
            string json = Tests.TestData.KspAvcJson();

            var avc = JsonConvert.DeserializeObject<CKAN.NetKAN.AVC>(json);

            Assert.AreEqual("0.24.2", avc.ksp_version.ToString());
            Assert.AreEqual("0.24.0", avc.ksp_version_min.ToString());
            Assert.AreEqual("0.24.2", avc.ksp_version_max.ToString());
        }
    }
}

