using System;
using System.IO;

using Newtonsoft.Json;
using NUnit.Framework;

using CKAN.NetKAN.Sources.Avc;
using CKAN.Versioning;
using Tests.Data;

namespace Tests.NetKAN.Sources.Avc
{
    [TestFixture]
    public class AVC
    {
        [Test]
        public void Json()
        {
            string json = TestData.KspAvcJson();

            var avc = JsonConvert.DeserializeObject<AvcVersion>(json);

            Assert.AreEqual("0.24.2", avc?.ksp_version?.ToString());
            Assert.AreEqual("0.24.0", avc?.ksp_version_min?.ToString());
            Assert.AreEqual("0.24.2", avc?.ksp_version_max?.ToString());
        }

        [Test]
        public void JsonOneLineVersion()
        {
            string json = TestData.KspAvcJsonOneLineVersion();

            var avc = JsonConvert.DeserializeObject<AvcVersion>(json);

            Assert.AreEqual("0.24.2", avc?.ksp_version?.ToString());
            Assert.AreEqual("0.24.0", avc?.ksp_version_min?.ToString());
            Assert.AreEqual("1.0.0",  avc?.ksp_version_max?.ToString());
        }

        [Test]
        public void WildcardMajor_OutputsAnyVersion()
        {
            var converter = new JsonAvcToGameVersion();
            string json = @"{""MAJOR"":-1, ""MINOR"":-1, ""PATCH"":-1}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (GameVersion)converter.ReadJson(reader, null, null, null)!;
            Assert.That(!result.IsMajorDefined);
        }

        [Test]
        public void WildcardMinor_VersionOnlyHasMajor()
        {
            var converter = new JsonAvcToGameVersion();
            string json = @"{""MAJOR"":1, ""MINOR"":-1, ""PATCH"":-1}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (GameVersion)converter.ReadJson(reader, null, null, null)!;
            Assert.That(result, Is.EqualTo(GameVersion.Parse("1")));
        }

        [Test]
        public void WildcardPatch_VersionOnlyHasMajorMinor()
        {
            var converter = new JsonAvcToGameVersion();
            string json = @"{""MAJOR"":1, ""MINOR"":5, ""PATCH"":-1}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (GameVersion)converter.ReadJson(reader, null, null, null)!;
            Assert.That(result, Is.EqualTo(GameVersion.Parse("1.5")));
        }

        [Test]
        public void MissingMajor_OutputsAnyVersion()
        {
            var converter = new JsonAvcToGameVersion();
            string json = @"{}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (GameVersion)converter.ReadJson(reader, null, null, null)!;
            Assert.That(!result.IsMajorDefined);
        }

        [Test]
        public void MissingMinor_VersionOnlyHasMajor()
        {
            var converter = new JsonAvcToGameVersion();
            string json = @"{""MAJOR"":1}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (GameVersion)converter.ReadJson(reader, null, null, null)!;
            Assert.That(result, Is.EqualTo(GameVersion.Parse("1")));
        }

        [Test]
        public void MissingPatch_VersionOnlyHasMajorMinor()
        {
            var converter = new JsonAvcToGameVersion();
            string json = @"{""MAJOR"":1, ""MINOR"":5}";
            var reader = new JsonTextReader(new StringReader(json));
            var result = (GameVersion)converter.ReadJson(reader, null, null, null)!;
            Assert.That(result, Is.EqualTo(GameVersion.Parse("1.5")));
        }

        [TestCase("02024", "7",  "13"),
         TestCase("2024",  "07", "13"),
         TestCase("2024",  "7",  "013")]
        public void ModVersionConverterReadJson_ValidOctalLiteral_DoesNotThrow(
            string major, string minor, string patch)
        {
            // Arrange
            var json = $@"{{
                              ""MAJOR"": {major},
                              ""MINOR"": {minor},
                              ""PATCH"": {patch}
                          }}";
            var reader = new JsonTextReader(new StringReader(json));
            var converter = new JsonAvcToVersion();
            var correct = new ModuleVersion(string.Join(".",
                                                        FromMaybeOctal(major),
                                                        FromMaybeOctal(minor),
                                                        FromMaybeOctal(patch)));

            // Act / Assert
            Assert.DoesNotThrow(() =>
            {
                var result = converter.ReadJson(reader, null, null, null);
                Assert.AreEqual(correct, result);
            });
        }

        private int FromMaybeOctal(string val)
            => val.StartsWith("0")
                ? Convert.ToInt32(val, 8)
                : int.Parse(val);

        [TestCase("02028", "10",  "9"),
         TestCase("2024",  "080", "9"),
         TestCase("2024",  "10",  "08"),
         TestCase("02029", "10",  "9"),
         TestCase("2024",  "090", "9"),
         TestCase("2024",  "10",  "09")]
        public void ModVersionConverterReadJson_InvalidOctalLiteral_Throws(
            string major, string minor, string patch)
        {
            // Arrange
            var json = $@"{{
                              ""MAJOR"": {major},
                              ""MINOR"": {minor},
                              ""PATCH"": {patch}
                          }}";
            var reader = new JsonTextReader(new StringReader(json));
            var converter = new JsonAvcToVersion();

            // Act / Assert
            Assert.Throws<JsonReaderException>(() =>
            {
                var result = converter.ReadJson(reader, null, null, null);
            });
        }

        [TestCase("02028", "10",  "9"),
         TestCase("2024",  "080", "9"),
         TestCase("2024",  "10",  "08"),
         TestCase("02029", "10",  "9"),
         TestCase("2024",  "090", "9"),
         TestCase("2024",  "10",  "09")]
        public void ModVersionConverterReadJson_QuotedInvalidOctalNumber_DoesNotThrow(
            string major, string minor, string patch)
        {
            // Arrange
            var json = $@"{{
                              ""MAJOR"": ""{major}"",
                              ""MINOR"": ""{minor}"",
                              ""PATCH"": ""{patch}""
                          }}";
            var reader = new JsonTextReader(new StringReader(json));
            var converter = new JsonAvcToVersion();
            var correct = new ModuleVersion(string.Join(".", major, minor, patch));

            // Act / Assert
            Assert.DoesNotThrow(() =>
            {
                var result = converter.ReadJson(reader, null, null, null);
                Assert.AreEqual(correct, result);
            });
        }

    }
}
