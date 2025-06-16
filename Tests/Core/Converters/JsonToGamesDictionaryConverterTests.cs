using System.Collections.Generic;

using NUnit.Framework;
using Newtonsoft.Json;

using CKAN;

namespace Tests.Core.Converters
{
    [TestFixture]
    public sealed class JsonToGamesDictionaryConverterTests
    {
        private class ChangedJsonConverter : JsonPropertyNamesChangedConverter
        {
            protected override Dictionary<string, string> mapping
                => new Dictionary<string, string>
                {
                    { "GlobalInstallFilters", "GlobalInstallFiltersByGame" },
                };
        }

        [JsonConverter(typeof(ChangedJsonConverter))]
        private class ChangedJson
        {
            [JsonProperty("GlobalInstallFiltersByGame")]
            [JsonConverter(typeof(JsonToGamesDictionaryConverter))]
            public Dictionary<string, string[]>? GlobalInstallFilters { get; set; } = new Dictionary<string, string[]>();
        }

        [Test]
        public void ReadJson_WithoutGames_DuplicatedWithGames()
        {
            // Arrange
            var json = @"{
                ""GlobalInstallFilters"": [
                    ""value1"",
                    ""value2""
                ]
            }";

            // Act
            var deserialized = JsonConvert.DeserializeObject<ChangedJson>(json)!;

            // Assert
            CollectionAssert.AreEquivalent(new string[] { "KSP", "KSP2" },
                                           deserialized.GlobalInstallFilters?.Keys);
            CollectionAssert.AreEquivalent(new string[] { "value1", "value2" },
                                           deserialized.GlobalInstallFilters?["KSP"]);
            CollectionAssert.AreEquivalent(new string[] { "value1", "value2" },
                                           deserialized.GlobalInstallFilters?["KSP2"]);
        }
    }
}
