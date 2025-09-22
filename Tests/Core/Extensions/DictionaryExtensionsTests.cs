using System.Collections.Generic;

using Newtonsoft.Json;
using NUnit.Framework;

using CKAN.Extensions;

namespace Tests.Core.Extensions
{
    [TestFixture]
    public class DictionaryExtensionsTests
    {
        [TestCase("", "", ExpectedResult = true),
         TestCase("{}", "{}", ExpectedResult = true),
         TestCase(@"{""a"":""1""}",
                  @"{""a"":""1""}",
                  ExpectedResult = true),
         TestCase(@"{""a"":""1""}",
                  @"{""a"":""1"", ""b"":""2""}",
                  ExpectedResult = false),
         TestCase(@"{""a"":""1""}",
                  @"{""b"":""1""}",
                  ExpectedResult = false),
         TestCase(@"{""a"":""1""}",
                  @"{""a"":""2""}",
                  ExpectedResult = false),
        ]
        public bool DictionaryEquals_VariousDictionaries_Correct(string json1, string json2)
        {
            // Arrange
            var dict1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(json1);
            var dict2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(json2);

            // Act
            return dict1.DictionaryEquals(dict2);
        }
    }
}
