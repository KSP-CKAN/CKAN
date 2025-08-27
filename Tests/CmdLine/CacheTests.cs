using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class CacheTests
    {
        [Test]
        public void RunSubCommand_List_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new Cache(manager, user);
                var         args    = new string[] { "cache", "list" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.AreEqual(2, user.RaisedMessages.Count);
                var path     = user.RaisedMessages[0];
                Assert.AreEqual(config.DownloadCacheDir, path);
                var contents = user.RaisedMessages[1];
                StringAssert.Contains("files", contents);
                StringAssert.Contains("free",  contents);
            }
        }

        [Test]
        public void RunSubCommand_Set_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            using (var altDir  = new TemporaryDirectory())
            {
                ISubCommand sut     = new Cache(manager, user);
                var         args    = new string[] { "cache", "set", altDir.Directory.FullName };
                var         subOpts = new SubCommandOptions(args);

                // Act
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.AreEqual(altDir.Directory.FullName,
                                config.DownloadCacheDir);
            }
        }

        [Test]
        public void RunSubCommand_Clear_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new Cache(manager, user);
                var         args    = new string[] { "cache", "clear" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                manager.Cache?.Store(TestData.DogeCoinFlag_101_module(),
                                     TestData.DogeCoinFlagZip(), null);
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                manager.Cache!.GetSizeInfo(out int numFiles, out long numBytes, out _);
                Assert.AreEqual(0, numFiles);
                Assert.AreEqual(0, numBytes);
            }
        }

        [Test]
        public void RunSubCommand_Reset_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                var         origPath = config.DownloadCacheDir;
                ISubCommand sut      = new Cache(manager, user);
                var         args     = new string[] { "cache", "reset" };
                var         subOpts  = new SubCommandOptions(args);

                // Act
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                Assert.AreNotEqual(origPath, config.DownloadCacheDir);
                // The value in the config is null for the default path in case the user migrates
                // the CKAN dir to a new computer
                Assert.AreEqual(null, config.DownloadCacheDir);
            }
        }

        [Test]
        public void RunSubCommand_ShowLimit_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name)
                                 {
                                     CacheSizeLimit = null,
                                 })
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new Cache(manager, user);
                var         args    = new string[] { "cache", "showlimit" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[] { "Unlimited" },
                                          user.RaisedMessages);
            }
        }

        [Test]
        public void RunSubCommand_SetLimit_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new Cache(manager, user);
                var         args    = new string[] { "cache", "setlimit", "1000" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.AreEqual(1000 * 1024 * 1024,
                                config.CacheSizeLimit);
            }
        }
    }
}
