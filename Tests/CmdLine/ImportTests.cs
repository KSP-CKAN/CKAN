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
            using (var repoData = new TemporaryRepositoryData(user, repo.repo))
            using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                           new Repository[] { repo.repo }))
            using (var zipDir   = new TemporaryDirectory())
            {
                ICommand sut  = new Import(manager, repoData.Manager, user);
                var      opts = new ImportOptions()
                                {
                                    Headless = true,
                                    paths    = new List<string>
                                               {
                                                   zipDir.Directory.FullName,
                                               },
                                };
                File.Copy(TestData.DogeCoinPluginZip(),
                          Path.Combine(zipDir.Directory.FullName,
                                       Path.GetFileName(TestData.DogeCoinPluginZip())));

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.IsEmpty(user.RaisedErrors);
                Assert.AreEqual("3108C916-DogeCoinPlugin-1.01.zip",
                                CKANPathUtils.ToRelative(manager.Cache!.GetCachedFilename(TestData.DogeCoinPlugin_module())!,
                                                         config.DownloadCacheDir!));
                Assert.AreEqual("DogeCoinPlugin",
                                regMgr.registry.InstalledModules.Single().identifier);
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
