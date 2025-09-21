using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.Versioning;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    [TestFixture]
    public class CompatTests
    {
        [Test]
        public void RunSubCommand_List_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst   = new DisposableKSP())
            using (var config = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            {
                var         manager = new GameInstanceManager(user, config);
                ISubCommand sut     = new Compat(manager, user);
                var         args    = new string[] { "compat", "list" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                inst.KSP.SetCompatibleVersions(new List<GameVersion>()
                {
                    new GameVersion(1, 0),
                    new GameVersion(1, 1),
                    new GameVersion(1, 2),
                });
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "Version     Actual",
                                              "----------  ------",
                                              // DisposableKSP is on KSP 0.25.0
                                              "0.25.0.642  True  ",
                                              "1.2         False ",
                                              "1.1         False ",
                                              "1.0         False ",
                                          },
                                          user.RaisedMessages);
            }
        }

        [Test]
        public void RunSubCommand_Clear_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst   = new DisposableKSP())
            using (var config = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            {
                var         manager = new GameInstanceManager(user, config);
                ISubCommand sut     = new Compat(manager, user);
                var         args    = new string[] { "compat", "clear" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                inst.KSP.SetCompatibleVersions(new List<GameVersion>()
                {
                    new GameVersion(1, 0),
                    new GameVersion(1, 1),
                    new GameVersion(1, 2),
                });
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.IsEmpty(inst.KSP.CompatibleVersions);
            }
        }

        [Test]
        public void RunSubCommand_Add_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst   = new DisposableKSP())
            using (var config = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            {
                var         manager = new GameInstanceManager(user, config);
                ISubCommand sut     = new Compat(manager, user);
                var         args    = new string[] { "compat", "add", "1.0" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                // Clear the default compat list
                inst.KSP.SetCompatibleVersions(new List<GameVersion>());
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEquivalent(new GameVersion[]
                                               {
                                                   new GameVersion(1, 0),
                                               },
                                               inst.KSP.CompatibleVersions);
            }
        }

        [Test]
        public void RunSubCommand_Forget_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst   = new DisposableKSP())
            using (var config = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            {
                var         manager = new GameInstanceManager(user, config);
                ISubCommand sut     = new Compat(manager, user);
                var         args    = new string[] { "compat", "forget", "1.1" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                inst.KSP.SetCompatibleVersions(new List<GameVersion>()
                {
                    new GameVersion(1, 0),
                    new GameVersion(1, 1),
                    new GameVersion(1, 2),
                });
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEquivalent(new GameVersion[]
                                               {
                                                   new GameVersion(1, 0),
                                                   new GameVersion(1, 2),
                                               },
                                               inst.KSP.CompatibleVersions);
            }
        }

        [Test]
        public void RunSubCommand_Set_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst   = new DisposableKSP())
            using (var config = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            {
                var         manager = new GameInstanceManager(user, config);
                ISubCommand sut     = new Compat(manager, user);
                var         args    = new string[] { "compat", "set", "1.3", "1.4", "1.5" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                inst.KSP.SetCompatibleVersions(new List<GameVersion>()
                {
                    new GameVersion(1, 0),
                    new GameVersion(1, 1),
                    new GameVersion(1, 2),
                });
                manager.SetCurrentInstance(inst.KSP);
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEquivalent(new GameVersion[]
                                               {
                                                   new GameVersion(1, 3),
                                                   new GameVersion(1, 4),
                                                   new GameVersion(1, 5),
                                               },
                                               inst.KSP.CompatibleVersions);
            }
        }
    }
}
