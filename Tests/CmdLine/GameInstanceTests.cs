using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;

namespace Tests.CmdLine
{
    using GameInstance = CKAN.CmdLine.GameInstance;

    [TestFixture]
    public class GameInstanceTests
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
                var         dirLen  = inst.KSP.GameDir().Length;
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[] { "instance", "list" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(
                    new string[]
                    {
                        $"Name        Game  Version     Default  {"Path".PadRight(dirLen)}",
                        $"----------  ----  ----------  -------  {new string('-', dirLen)}",
                        $"disposable  KSP   0.25.0.642  Yes      {Platform.FormatPath(inst.KSP.GameDir())}",
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
            using (var inst2   = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[] { "instance", "add",
                                                     "test", inst2.KSP.GameDir() };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.Contains("test", manager.Instances.Keys.ToArray());
            }
        }

        [Test]
        public void RunSubCommand_Clone_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var targDir = new TemporaryDirectory())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[]
                                      {
                                          "instance", "clone", "disposable",
                                          "myClone", targDir.Directory.FullName,
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.Contains("myClone", manager.Instances.Keys.ToArray());
            }
        }

        [Test]
        public void RunSubCommand_Rename_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[] { "instance", "rename", "disposable", "renamed" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.Contains("renamed", manager.Instances.Keys.ToArray());
            }
        }

        [Test]
        public void RunSubCommand_Forget_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[] { "instance", "forget", "disposable" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.DoesNotContain(manager.Instances.Keys.ToArray(), "renamed");
            }
        }

        [Test]
        public void RunSubCommand_Default_Works()
        {
            // Arrange
            var asked = false;
            var user = new CapturingUser(false, q => true,
                                         (msg, objs) =>
                                         {
                                             asked = true;
                                             return 0;
                                         });
            using (var inst1   = new DisposableKSP())
            using (var inst2   = new DisposableKSP())
            using (var fakeDir = new TemporaryDirectory())
            using (var config  = new FakeConfiguration(new List<Tuple<string, string, string>>()
                                                       {
                                                           new Tuple<string, string, string>("inst1",
                                                                                             inst1.KSP.GameDir(),
                                                                                             inst1.KSP.game.ShortName),
                                                           new Tuple<string, string, string>("inst2",
                                                                                             inst2.KSP.GameDir(),
                                                                                             inst2.KSP.game.ShortName),
                                                       },
                                                       null,
                                                       null))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut = new GameInstance(manager, user);

                // Act / Assert
                Assert.IsNull(manager.AutoStartInstance);
                sut.RunSubCommand(null, new SubCommandOptions(new string[] { "instance", "default" }));
                Assert.IsTrue(asked);
                Assert.AreEqual("inst1", manager.AutoStartInstance);
                sut.RunSubCommand(null, new SubCommandOptions(new string[] { "instance", "default", "inst2" }));
                Assert.AreEqual("inst2", manager.AutoStartInstance);
                sut.RunSubCommand(null, new SubCommandOptions(new string[] { "instance", "use", "inst1" }));
                Assert.AreEqual("inst1", manager.AutoStartInstance);
            }
        }

        [Test]
        public void RunSubCommand_Fake_Works()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var fakeDir = new TemporaryDirectory())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[]
                                      {
                                          "instance", "fake",
                                          "fakeInstance", fakeDir.Directory.FullName, "1.12.5",
                                          "--MakingHistory", "1.0.0",
                                          "--BreakingGround", "1.0.0",
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.Contains("fakeInstance", manager.Instances.Keys.ToArray());
            }
        }
    }
}
