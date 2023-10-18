using System.Collections.Generic;

using NUnit.Framework;
using Newtonsoft.Json;

using CKAN;
using CKAN.Games.KerbalSpaceProgram;

using Tests.Data;

namespace Tests.Core.Types
{
    [TestFixture]
    public class ModuleInstallDescriptorTests
    {
        [Test]
        // NOTE: I've *never* got these to fail. The problem I'm trying to reproduce
        // seems to involve saving to the registry and back. It's now fixed in
        // JsonSingleOrArrayConverter.cs, but these tests remain, because tests are good.
        public void Null_Filters()
        {
            // We had a bug whereby we could end up with a filter list of a single null.
            // Make sure that doesn't happen ever again.

            // We want a module that doesn't specify filters.
            CkanModule mod = TestData.kOS_014_module();

            test_filter(mod.install[0].filter, "kOS/filter");
            test_filter(mod.install[0].filter_regexp, "kOS/filter_regexp");

            // And Firespitter seems to trigger it.

            CkanModule firespitter = TestData.FireSpitterModule();

            foreach (var stanza in firespitter.install)
            {
                test_filter(stanza.filter, "Firespitter/filter");
                test_filter(stanza.filter_regexp, "Firespitter/filter_regexp");
            }
        }

        private static void test_filter(List<string> filter, string message)
        {
            if (filter != null)
            {
                Assert.IsFalse(filter.Contains(null), message);
            }
        }

        [TestCase("GameData/kOS", "GameData/kOS/Plugins/kOS.dll", "GameData", null, "GameData/kOS/Plugins/kOS.dll")]
        [TestCase("kOS-1.1/GameData/kOS", "kOS-1.1/GameData/kOS/Plugins/kOS.dll", "GameData", null, "GameData/kOS/Plugins/kOS.dll")]
        [TestCase("ModuleManager.2.5.1.dll", "ModuleManager.2.5.1.dll", "GameData", null, "GameData/ModuleManager.2.5.1.dll")]


        [TestCase("Ships", "Ships/SPH/FAR Firehound.craft", "SomeDir/Ships", null, "SomeDir/Ships/SPH/FAR Firehound.craft")]


        [TestCase("GameData/kOS", "GameData/kOS/Plugins/kOS.dll", "GameData", "kOS-Renamed", "GameData/kOS-Renamed/Plugins/kOS.dll")]
        [TestCase("kOS-1.1/GameData/kOS", "kOS-1.1/GameData/kOS/Plugins/kOS.dll", "GameData", "kOS-Renamed", "GameData/kOS-Renamed/Plugins/kOS.dll")]
        [TestCase("ModuleManager.2.5.1.dll", "ModuleManager.2.5.1.dll", "GameData", "ModuleManager-Renamed.dll", "GameData/ModuleManager-Renamed.dll")]
        [TestCase("GameData", "GameData/kOS/Plugins/kOS.dll", "GameData", "GameData-Renamed", "GameData/GameData-Renamed/kOS/Plugins/kOS.dll")]
        [TestCase("Ships", "Ships/SPH/FAR Firehound.craft", "SomeDir/Ships", "Ships-Renamed", "SomeDir/Ships/Ships-Renamed/SPH/FAR Firehound.craft")]
        public void TransformOutputName(string file, string outputName, string installDir, string @as, string expected)
        {
            // Arrange
            var stanza = JsonConvert.DeserializeObject<ModuleInstallDescriptor>(
                $"{{\"file\": \"{file}\"}}");

            // Act
            var result = stanza.TransformOutputName(new KerbalSpaceProgram(), outputName, installDir, @as);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("GameData/kOS", "GameData/kOS/Plugins/kOS.dll", "GameData", "kOS/Renamed")]
        [TestCase("kOS-1.1/GameData/kOS", "kOS-1.1/GameData/kOS/Plugins/kOS.dll", "GameData", "kOS/Renamed")]
        [TestCase("ModuleManager.2.5.1.dll", "ModuleManager.2.5.1.dll", "GameData", "Renamed/ModuleManager.dll")]
        public void TransformOutputNameThrowsOnInvalidParameters(string file, string outputName, string installDir, string @as)
        {
            // Arrange
            var stanza = JsonConvert.DeserializeObject<ModuleInstallDescriptor>(
                $"{{\"file\": \"{file}\"}}");

            // Act
            TestDelegate act = () => stanza.TransformOutputName(new KerbalSpaceProgram(), outputName, installDir, @as);

            // Assert
            Assert.That(act, Throws.Exception);
        }

    }
}
