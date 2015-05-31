using Newtonsoft.Json;
using NUnit.Framework;
using Tests.Data;

namespace Tests.NetKAN
{
    [TestFixture]
    public class AVC
    {
        [Test]
        public void Json()
        {
            string json = TestData.KspAvcJson();

            var avc = JsonConvert.DeserializeObject<CKAN.NetKAN.AVC>(json);

            Assert.AreEqual("0.24.2", avc.ksp_version.ToString());
            Assert.AreEqual("0.24.0", avc.ksp_version_min.ToString());
            Assert.AreEqual("0.24.2", avc.ksp_version_max.ToString());
        }

        [Test]
        public void JsonOneLineVersion()
        {
            string json = TestData.KspAvcJsonOneLineVersion ();

            var avc = JsonConvert.DeserializeObject<CKAN.NetKAN.AVC>(json);

            Assert.AreEqual("0.24.2", avc.ksp_version.ToString());
            Assert.AreEqual("0.24.0", avc.ksp_version_min.ToString());
            Assert.AreEqual("1.0.0", avc.ksp_version_max.ToString());
        }
    }
}

