using System.CodeDom;
using System.IO;
using CKAN;
using CKAN.NetKAN;
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
            string json = TestData.KspAvcJsonOneLineVersion();

            var avc = JsonConvert.DeserializeObject<CKAN.NetKAN.AVC>(json);

            Assert.AreEqual("0.24.2", avc.ksp_version.ToString());
            Assert.AreEqual("0.24.0", avc.ksp_version_min.ToString());
            Assert.AreEqual("1.0.0", avc.ksp_version_max.ToString());
        }

        [Test]
        public void WildcardMajor_OutputsAnyVersion()
        {
            var converter = new JsonAvcToKspVersion();
            string json = @"{""MAJOR"":-1, ""MINOR"":-1, ""PATCH"":-1}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (KSPVersion) converter.ReadJson(reader, null, null, null);
            Assert.That(result.IsAny());
        }

        [Test]
        public void WildcardMinor_VersionOnlyHasMajor()
        {
            var converter = new JsonAvcToKspVersion();
            string json = @"{""MAJOR"":1, ""MINOR"":-1, ""PATCH"":-1}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (KSPVersion) converter.ReadJson(reader, null, null, null);
            Assert.That(result, Is.EqualTo(new KSPVersion("1")));
        }
        [Test]
        public void WildcardPatch_VersionOnlyHasMajorMinor()
        {
            var converter = new JsonAvcToKspVersion();
            string json = @"{""MAJOR"":1, ""MINOR"":5, ""PATCH"":-1}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (KSPVersion)converter.ReadJson(reader, null, null, null);
            Assert.That(result, Is.EqualTo(new KSPVersion("1.5")));
        }
    }
}