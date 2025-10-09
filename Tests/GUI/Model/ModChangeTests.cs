#if NETFRAMEWORK || WINDOWS

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using NUnit.Framework;

using CKAN;
using CKAN.GUI;
using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.GUI
{
    #if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
    #endif
    [TestFixture]
    public class ModChangeTests
    {
        [Test]
        public void AllProperties_InstallingMM_Correct()
        {
            // Arrange
            var user = new NullUser();
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {

                // Act
                var sut = new ModChange(TestData.ModuleManagerModule(),
                                        GUIModChangeType.Install,
                                        config);

                // Assert
                Assert.AreEqual(true, sut.IsUserRequested);
                Assert.AreEqual("Requested by user", sut.Description);
                Assert.AreEqual("Install ModuleManager 2.5.1 (Requested by user)", sut.ToString());
                Assert.AreEqual(new ModChange(TestData.ModuleManagerModule(),
                                              GUIModChangeType.Install,
                                              config),
                                sut);
                Assert.AreNotEqual(new ModChange(TestData.ModuleManagerModule(),
                                                 GUIModChangeType.Update,
                                                 config),
                                   sut);
                Assert.AreNotEqual(new ModChange(TestData.BurnControllerModule(),
                                                 GUIModChangeType.Install,
                                                 config),
                                   sut);
            }
        }

        [TestCase(false, false, "Re-install (missing folders or files)"),
         TestCase(false, true,  "Re-install (metadata changed)"),
         TestCase(true,  false, "Re-install (user requested)"),
         TestCase(true,  true,  "Re-install (user requested)")]
        public void AllProperties_Upgrade_Correct(bool   userReinstall,
                                                  bool   metadataChanged,
                                                  string reason)
        {
            // Arrange
            var user = new NullUser();
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {

                // Act
                var sut = new ModUpgrade(TestData.ModuleManagerModule(),
                                         TestData.ModuleManagerModule(),
                                         userReinstall, metadataChanged,
                                         config);

                // Assert
                Assert.AreEqual(true, sut.IsUserRequested);
                Assert.AreEqual(reason, sut.Description);
                Assert.AreEqual($"Update ModuleManager 2.5.1 ({reason})",
                                sut.ToString());
                Assert.AreEqual(new ModUpgrade(TestData.ModuleManagerModule(),
                                               TestData.ModuleManagerModule(),
                                               userReinstall, metadataChanged,
                                               config),
                                sut);
                Assert.AreNotEqual(new ModUpgrade(TestData.BurnControllerModule(),
                                                  TestData.BurnControllerModule(),
                                                  userReinstall, metadataChanged,
                                                  config),
                                   sut);
            }
        }
    }
}

#endif
