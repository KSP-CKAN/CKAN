using System.Linq;
using NUnit.Framework;
using YamlDotNet.RepresentationModel;
using Newtonsoft.Json.Linq;

using CKAN.NetKAN.Extensions;

namespace Tests.NetKAN.Extensions
{
    [TestFixture]
    public sealed class YamlExtensionsTests
    {
        [Test]
        public void Parse_ValidInput_Works()
        {
            // Arrange
            string input = string.Join("\r\n", new string[]
            {
                "spec_version: v1.4",
                "identifier: Astrogator",
                "$kref: \"#/ckan/github/HebaruSan/Astrogator\"",
                "$vref: \"#/ckan/ksp-avc\"",
                "license: GPL-3.0",
                "tags:",
                "    - plugin",
                "    - information",
                "    - control",
                "resources:",
                "    homepage: https://forum.kerbalspaceprogram.com/index.php?/topic/155998-*",
                "    bugtracker: https://github.com/HebaruSan/Astrogator/issues",
                "    repository: https://github.com/HebaruSan/Astrogator",
                "recommends:",
                "    - name: ModuleManager",
                "    - name: LoadingTipsPlus",
            });

            // Act
            YamlMappingNode yaml = YamlExtensions.Parse(input).First();

            // Assert
            Assert.AreEqual("v1.4",                               (string)yaml["spec_version"]);
            Assert.AreEqual("Astrogator",                         (string)yaml["identifier"]);
            Assert.AreEqual("#/ckan/github/HebaruSan/Astrogator", (string)yaml["$kref"]);
            Assert.AreEqual("#/ckan/ksp-avc",                     (string)yaml["$vref"]);
            Assert.AreEqual("GPL-3.0",                            (string)yaml["license"]);

            CollectionAssert.AreEqual(
                new string[] { "plugin", "information", "control" },
                (yaml["tags"] as YamlSequenceNode).Children.Select(yn => (string)yn)
            );
            Assert.AreEqual(
                "https://forum.kerbalspaceprogram.com/index.php?/topic/155998-*",
                (string)yaml["resources"]["homepage"]
            );
            Assert.AreEqual(
                "https://github.com/HebaruSan/Astrogator/issues",
                (string)yaml["resources"]["bugtracker"]
            );
            Assert.AreEqual(
                "https://github.com/HebaruSan/Astrogator",
                (string)yaml["resources"]["repository"]
            );
            Assert.AreEqual("ModuleManager",   (string)yaml["recommends"][0]["name"]);
            Assert.AreEqual("LoadingTipsPlus", (string)yaml["recommends"][1]["name"]);
        }

        [Test]
        public void ToJObject_ValidInput_Works()
        {
            // Arrange
            var yaml = new YamlMappingNode()
            {
                { "spec_version", "v1.4" },
                { "identifier",   "Astrogator" },
                { "$kref",        "#/ckan/github/HebaruSan/Astrogator" },
                { "$vref",        "#/ckan/ksp-avc" },
                { "license",      "GPL-3.0" },
                {
                    "tags",
                    new YamlSequenceNode(
                        "plugin",
                        "information",
                        "control"
                    )
                },
                {
                    "resources",
                    new YamlMappingNode()
                    {
                        { "homepage", "https://forum.kerbalspaceprogram.com/index.php?/topic/155998-*" },
                        { "bugtracker", "https://github.com/HebaruSan/Astrogator/issues" },
                        { "repository", "https://github.com/HebaruSan/Astrogator" },
                    }
                },
                {
                    "recommends",
                    new YamlSequenceNode(
                        new YamlMappingNode()
                        {
                            { "name", "ModuleManager"   }
                        },
                        new YamlMappingNode()
                        {
                            { "name", "LoadingTipsPlus" }
                        }
                    )
                }
            };

            // Act
            JObject json = yaml.ToJObject();

            // Assert
            Assert.AreEqual("v1.4",                               (string)json["spec_version"]);
            Assert.AreEqual("Astrogator",                         (string)json["identifier"]);
            Assert.AreEqual("#/ckan/github/HebaruSan/Astrogator", (string)json["$kref"]);
            Assert.AreEqual("#/ckan/ksp-avc",                     (string)json["$vref"]);
            Assert.AreEqual("GPL-3.0",                            (string)json["license"]);

            CollectionAssert.AreEqual(
                new string[] { "plugin", "information", "control" },
                (json["tags"] as JArray).Select(elt => (string)elt)
            );
            Assert.AreEqual(
                "https://forum.kerbalspaceprogram.com/index.php?/topic/155998-*",
                (string)json["resources"]["homepage"]
            );
            Assert.AreEqual(
                "https://github.com/HebaruSan/Astrogator/issues",
                (string)json["resources"]["bugtracker"]
            );
            Assert.AreEqual(
                "https://github.com/HebaruSan/Astrogator",
                (string)json["resources"]["repository"]
            );
            Assert.AreEqual("ModuleManager",   (string)json["recommends"][0]["name"]);
            Assert.AreEqual("LoadingTipsPlus", (string)json["recommends"][1]["name"]);
        }
    }
}
