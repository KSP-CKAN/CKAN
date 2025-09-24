using System.Linq;

using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Moq;

using CKAN.Games.KerbalSpaceProgram;
using CKAN.NetKAN.Validators;
using CKAN.NetKAN.Model;
using CKAN.NetKAN.Services;
using Tests.Data;

namespace Tests.NetKAN.Validators
{
    [TestFixture]
    public class VrefValidatorTests
    {
        [TestCase(false, false),
         TestCase(false, true),
         TestCase(true,  false),
         TestCase(true,  true),
        ]
        public void Validate_WithOrWithoutVrefAndAvc_WarnsOrDoesnt(bool withVref,
                                                                   bool withAvc)
        {
            // Arrange
            var jobj = new JObject()
            {
                { "identifier", "TestMod" },
                { "version",    "1.0"     },
                { "download",   "https://testmod.com/download" },
                { "install",    new JArray(new JObject() { { "find",       "DogeCoinFlag" },
                                                           { "install_to", "GameData"     } }) },
            };
            if (withVref)
            {
                jobj["$vref"] = new JValue("#/ckan/ksp-avc");
            }
            var metadata = new Metadata(jobj);
            var zip      = withAvc ? TestData.DogeCoinFlagAvcZip()
                                   : TestData.DogeCoinFlagZip();
            var http     = new Mock<IHttpService>();
            http.Setup(h => h.DownloadModule(It.IsAny<Metadata>()))
                .Returns(zip);
            var modSvc   = new ModuleService(new KerbalSpaceProgram());
            var sut      = new VrefValidator(http.Object, modSvc);

            using (var appender = new TemporaryWarningCapturer(nameof(VrefValidator)))
            {
                // Act
                sut.Validate(metadata);

                // Assert
                if (withVref && !withAvc)
                {
                    Assert.AreEqual("$vref is ksp-avc, version file missing",
                                    appender.Warnings.Single());
                }
                else if (!withVref && withAvc)
                {
                    Assert.AreEqual("$vref is not ksp-avc, version file present: DogeCoinFlag-1.01/GameData/DogeCoinFlag/ksp-avc.version",
                                    appender.Warnings.Single());
                }
                else
                {
                    CollectionAssert.IsEmpty(appender.Warnings);
                }
            }
        }

        [TestCase(false, false),
         TestCase(false, true),
         TestCase(true,  false),
         TestCase(true,  true),
        ]
        public void Validate_WithOrWithoutVrefAndSwinfo_WarnsOrDoesnt(bool withVref,
                                                                      bool withSwinfo)
        {
            // Arrange
            var jobj = new JObject()
            {
                { "identifier", "TestMod" },
                { "version",    "1.0"     },
                { "download",   "https://testmod.com/download" },
                { "install",    new JArray(new JObject() { { "find",       "BurnController"  },
                                                           { "install_to", "BepInEx/plugins" } }) },
            };
            if (withVref)
            {
                jobj["$vref"] = new JValue("#/ckan/space-warp");
            }
            var metadata = new Metadata(jobj);
            var zip      = withSwinfo ? TestData.BurnControllerZip()
                                      : TestData.BurnControllerNoSwinfoZip();
            var http     = new Mock<IHttpService>();
            http.Setup(h => h.DownloadModule(It.IsAny<Metadata>()))
                .Returns(zip);
            var modSvc   = new ModuleService(new KerbalSpaceProgram());
            var sut      = new VrefValidator(http.Object, modSvc);

            using (var appender = new TemporaryWarningCapturer(nameof(VrefValidator)))
            {
                // Act
                sut.Validate(metadata);

                // Assert
                if (withVref && !withSwinfo)
                {
                    Assert.AreEqual("$vref is space-warp, swinfo.json file missing",
                                    appender.Warnings.Single());
                }
                else if (!withVref && withSwinfo)
                {
                    Assert.AreEqual("$vref is not space-warp, swinfo.json file present",
                                    appender.Warnings.Single());
                }
                else
                {
                    CollectionAssert.IsEmpty(appender.Warnings);
                }
            }
        }
    }
}
