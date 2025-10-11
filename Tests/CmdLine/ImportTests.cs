using System.IO;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.IO;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class ImportTests
    {
        [Test]
        public void RunCommand_WithZIP_Works()
        {
            // Arrange
            var user = new CapturingUser(true, q => true, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var repo     = new TemporaryRepository(TestData.DogeCoinPlugin()))
            using (var repoData = new TemporaryRepositoryData(new NullUser(), repo.repo))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            using (var zipDir   = new TemporaryDirectory())
            {
                ICommand sut  = new Import(manager, repoData.Manager, user);
                File.Copy(TestData.DogeCoinPluginZip(),
                          Path.Combine(zipDir,
                                       Path.GetFileName(TestData.DogeCoinPluginZip())));

                // Act
                var opts = new ImportOptions() { Headless = true, paths = new List<string> { zipDir } };
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                CollectionAssert.AreEqual(new string[]
                                          {
                                              " ",
                                              "About to install:",
                                              "",
                                              " * Dogecoin Core Plugin 1.01 (cached)",
                                              "Installing Dogecoin Core Plugin 1.01...",
                                              "Finished installing Dogecoin Core Plugin 1.01",
                                          },
                                          user.RaisedMessages);
                Assert.AreEqual("3108C916-DogeCoinPlugin-1.01.zip",
                                CKANPathUtils.ToRelative(manager.Cache!.GetCachedFilename(TestData.DogeCoinPlugin_module())!,
                                                         config.DownloadCacheDir!));
                Assert.AreEqual("DogeCoinPlugin",
                                regMgr.registry.InstalledModules.Single().identifier);


                // Act
                user.RaisedMessages.Clear();
                user.RaisedErrors.Clear();
                opts.paths = new List<string> { zipDir, TestData.ModuleManagerZip() };
                sut.RunCommand(inst.KSP, opts);
                CollectionAssert.IsEmpty(user.RaisedErrors);
                CollectionAssert.AreEqual(new string[]
                                          {
                                              " ",
                                              "Not found in index: ModuleManager-2.5.1.zip",
                                              " ",
                                              "Already cached: DogeCoinPlugin 1.01 (DogeCoinPlugin.zip)",
                                              "About to install:",
                                              "",
                                              " * Dogecoin Core Plugin 1.01 (cached)",
                                              "Dogecoin Core Plugin 1.01 already installed, skipped"
                                          },
                                          user.RaisedMessages);


                // Act
                user.RaisedMessages.Clear();
                user.RaisedErrors.Clear();
                opts.paths = new List<string> { TestData.ModuleManagerZip() };
                sut.RunCommand(inst.KSP, opts);
                CollectionAssert.AreEqual(new string[] { "Not found in index: ModuleManager-2.5.1.zip" },
                                          user.RaisedErrors);
                CollectionAssert.IsEmpty(user.RaisedMessages);

            }
        }

        [Test]
        public void RunCommand_NoArguments_PrintsHelp()
        {
            // Arrange
            var user = new CapturingUser(true, q => false, (msg, objs) => 0);
            using (var inst     = new DisposableKSP())
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(user, config))
            using (var repo     = new TemporaryRepository())
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            {
                ICommand sut  = new Import(manager, repoData.Manager, user);
                var      opts = new ImportOptions();

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                               "argument missing, perhaps you forgot it?",
                                               " ",
                                               "Usage: ckan import [options] path [path2 ...]"
                                          },
                                          user.RaisedErrors);
            }
        }
    }
}
