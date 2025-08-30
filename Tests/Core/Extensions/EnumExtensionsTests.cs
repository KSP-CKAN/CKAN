using NUnit.Framework;

using CKAN;
using CKAN.Extensions;

namespace Tests.Core.Extensions
{
    [TestFixture]
    public sealed class EnumExtensionsTests
    {
        [Test]
        public void HasAnyFlag_DoesHaveIt_ReturnsTrue()
        {
            // Arrange / Act
            var val = OptionalRelationships.Recommendations
                    | OptionalRelationships.Suggestions;

            // Assert
            Assert.IsTrue(val.HasAnyFlag(OptionalRelationships.AllSuggestions,
                                         OptionalRelationships.Recommendations));
            Assert.IsTrue(val.HasAnyFlag(OptionalRelationships.Suggestions,
                                         OptionalRelationships.AllSuggestions));
        }

        [Test]
        public void HasAnyFlag_DoesNotHaveIt_ReturnsFalse()
        {
            // Arrange / Act
            var val = OptionalRelationships.Recommendations;

            // Assert
            Assert.IsFalse(val.HasAnyFlag(OptionalRelationships.Suggestions,
                                          OptionalRelationships.AllSuggestions));
        }
    }
}
