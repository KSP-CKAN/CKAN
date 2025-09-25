using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class FilterTests
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
                inst.KSP.InstallFilters = new string[]
                                          {
                                              "/MiniAVC.dll",
                                              "/MiniAVC.xml",
                                          };
                config.SetGlobalInstallFilters(inst.KSP.Game,
                                               new string[]
                                               {
                                                   "/MiniAVC-V2.dll",
                                                   "/MiniAVC-V2.dll.mdb",
                                               });
                ISubCommand sut     = new Filter(manager, user);
                var         args    = new string[] { "filter", "list" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "Global filters for KSP:",
                                              "	- /MiniAVC-V2.dll",
                                              "	- /MiniAVC-V2.dll.mdb",
                                              "",
                                              "Instance filters for disposable:",
                                              "	- /MiniAVC.dll",
                                              "	- /MiniAVC.xml",
                                          },
                                          user.RaisedMessages);
            }
        }

        [Test]
        public void RunSubCommand_Add_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut         = new Filter(manager, user);
                var         globArgs    = new string[] { "filter", "add", "--global", "--game", "KSP", "/MiniAVC.dll" };
                var         globsubOpts = new SubCommandOptions(globArgs);
                var         instArgs    = new string[] { "filter", "add", "/MiniAVC-V2.dll" };
                var         instsubOpts = new SubCommandOptions(instArgs);

                // Act
                sut.RunSubCommand(null, instsubOpts);
                sut.RunSubCommand(null, globsubOpts);

                // Assert
                CollectionAssert.AreEquivalent(new string[] { "/MiniAVC-V2.dll" },
                                               inst.KSP.InstallFilters);
                CollectionAssert.AreEquivalent(new string[] { "/MiniAVC.dll" },
                                               config.GetGlobalInstallFilters(inst.KSP.Game));
            }
        }

        [Test]
        public void RunSubCommand_Remove_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                inst.KSP.InstallFilters = new string[]
                                          {
                                              "/MiniAVC.dll",
                                              "/MiniAVC.xml",
                                          };
                config.SetGlobalInstallFilters(inst.KSP.Game,
                                               new string[]
                                               {
                                                   "/MiniAVC-V2.dll",
                                                   "/MiniAVC-V2.dll.mdb",
                                               });
                ISubCommand sut         = new Filter(manager, user);
                var         instArgs    = new string[] { "filter", "remove", "/MiniAVC.xml" };
                var         instSubOpts = new SubCommandOptions(instArgs);
                var         globArgs    = new string[] { "filter", "remove", "--global", "/MiniAVC-V2.dll.mdb" };
                var         globSubOpts = new SubCommandOptions(globArgs);

                // Act
                sut.RunSubCommand(null, instSubOpts);
                sut.RunSubCommand(null, globSubOpts);

                // Assert
                CollectionAssert.AreEquivalent(new string[] { "/MiniAVC.dll" },
                                               inst.KSP.InstallFilters);
                CollectionAssert.AreEquivalent(new string[] { "/MiniAVC-V2.dll" },
                                               config.GetGlobalInstallFilters(inst.KSP.Game));
            }
        }

        [TestCase("add",
                  new string[]
                  {
                      "filter add - Add install filters",
                      "Usage: ckan filter add [options] filter1 [filter2 ...]",
                  }),
         TestCase("remove",
                  new string[]
                  {
                      "filter remove - Remove install filters",
                      "Usage: ckan filter remove [options] filter1 [filter2 ...]",
                  }),
        ]
        public void RunSubCommand_MissingArguments_PrintsUsage(string verb, string[] help)
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut         = new Filter(manager, user);
                var         args        = new string[] { "filter", verb };
                var         subOpts     = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "argument missing, perhaps you forgot it?",
                                              " ",
                                          }.Concat(help),
                                          user.RaisedErrors);
            }
        }
    }
}
