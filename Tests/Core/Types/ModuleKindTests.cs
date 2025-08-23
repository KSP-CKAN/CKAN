using System;
using System.Collections.Generic;

using NUnit.Framework;
using Newtonsoft.Json.Linq;

using CKAN;
using CKAN.Versioning;

namespace Tests.Core.Types
{
    [TestFixture]
    public class ModuleKindTests
    {
        [Test]
        public void Serialize_NotNull_SavesName()
        {
            // Arrange
            var module = new CkanModule(new ModuleVersion("1"),
                                        "Amod",
                                        "A mod",
                                        "Abstract of the mod",
                                        "Description of the mod",
                                        new List<string> { "ModAuthor" },
                                        new List<License> { License.UnknownLicense },
                                        new ModuleVersion("1"),
                                        new List<Uri>(),
                                        ModuleKind.metapackage);

            // Act
            var serialized = module.ToJson();
            var rawJson    = JObject.Parse(serialized);

            // Assert
            Assert.AreEqual("metapackage", (string?)rawJson["kind"]);
        }
    }
}
