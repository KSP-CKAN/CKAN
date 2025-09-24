using System.Linq;

using NUnit.Framework;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Validators;
using CKAN.NetKAN.Model;
using Tests.Data;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public class TagsValidatorTests
    {
        [TestCase(false),
         TestCase(true),
        ]
        public void Validate_WithOrWithoutTags_WarnsOrDoesnt(bool withTags)
        {
            // Arrange
            var jobj = new JObject()
            {
                { "identifier", "TestMod" },
                { "version",    "1.0"     },
            };
            if (withTags)
            {
                jobj["tags"] = new JArray("a", "b", "c");
            }
            var metadata = new Metadata(jobj);
            var sut      = new TagsValidator();

            using (var appender = new TemporaryWarningCapturer(nameof(SpaceWarpInfoValidator)))
            {
                // Act
                sut.Validate(metadata);

                // Assert
                if (withTags)
                {
                    CollectionAssert.IsEmpty(appender.Warnings);
                }
                else
                {
                    CollectionAssert.AreEqual(Enumerable.Repeat("Tags not found, see https://github.com/KSP-CKAN/CKAN/wiki/Suggested-Tags", 1),
                                              appender.Warnings);
                }
            }
        }
    }
}
