using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;
using Tests.Core.Configuration;
using System.IO;

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
                var         dirLen  = inst.KSP.GameDir.Length;
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
                        $"disposable  KSP   0.25.0.642  Yes      {Platform.FormatPath(inst.KSP.GameDir)}",
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
                var         args    = new string[]
                                      {
                                          "instance", "add",
                                          "test", inst2.KSP.GameDir,
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.Contains("test", manager.Instances.Keys.ToArray());
            }
        }

        [Test]
        public void RunSubCommand_AddDuplicateName_Fails()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var inst2   = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[]
                                      {
                                          "instance", "add",
                                          "disposable", inst2.KSP.GameDir,
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              @"Install with name ""disposable"" already exists, aborting",
                                          },
                                          user.RaisedErrors);
            }
        }

        [Test]
        public void RunSubCommand_AddInvalid_Fails()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var nonInst = new TemporaryDirectory())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[]
                                      {
                                          "instance", "add",
                                          "test", nonInst,
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              $"Sorry, {nonInst} does not appear to be a game instance",
                                          },
                                          user.RaisedErrors);
            }
        }

        [Test]
        public void RunSubCommand_CloneName_Works()
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
                                          "myClone", targDir,
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.Contains("myClone", manager.Instances.Keys.ToArray());
            }
        }

        [Test]
        public void RunSubcommand_ClonePath_Works()
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
                                          "instance", "clone", inst.KSP.GameDir,
                                          "myClone", targDir,
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.Contains("myClone", manager.Instances.Keys.ToArray());
            }
        }

        [Test]
        public void RunSubCommand_CloneInvalid_Fails()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var notInst = new TemporaryDirectory())
            using (var targDir = new TemporaryDirectory())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[]
                                      {
                                          "instance", "clone", notInst,
                                          "myClone", targDir,
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              $"The instance is not valid: {notInst}",
                                          },
                                          user.RaisedErrors);
            }
        }

        [Test]
        public void RunSubCommand_CloneSameName_Fails()
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
                                          "instance", "clone", inst.KSP.GameDir,
                                          "disposable", targDir,
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              @"This instance name is already taken: disposable"
                                          },
                                          user.RaisedErrors);
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
        public void RunSubCommand_RenameWrongName_Fails()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[] { "instance", "rename", "wrongname", "newname" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              @"Couldn't find install with name ""wrongname"", aborting",
                                          },
                                          user.RaisedErrors);
            }
        }

        [Test]
        public void RunSubCommand_RenameDuplicateName_Fails()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[] { "instance", "rename", "disposable", "disposable" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              @"Install with name ""disposable"" already exists, aborting"
                                          },
                                          user.RaisedErrors);
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
        public void RunSubCommand_ForgetWrongName_Fails()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[] { "instance", "forget", "wrongname" };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              @"Couldn't find install with name ""wrongname"", aborting",
                                          },
                                          user.RaisedErrors);
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
                                                                                             inst1.KSP.GameDir,
                                                                                             inst1.KSP.Game.ShortName),
                                                           new Tuple<string, string, string>("inst2",
                                                                                             inst2.KSP.GameDir,
                                                                                             inst2.KSP.Game.ShortName),
                                                       },
                                                       null,
                                                       null))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut = new GameInstance(manager, user);

                // Act / Assert
                Assert.IsNull(config.AutoStartInstance);
                sut.RunSubCommand(null, new SubCommandOptions(new string[] { "instance", "default" }));
                Assert.IsTrue(asked);
                Assert.AreEqual("inst1", config.AutoStartInstance);
                sut.RunSubCommand(null, new SubCommandOptions(new string[] { "instance", "default", "inst2" }));
                Assert.AreEqual("inst2", config.AutoStartInstance);
                sut.RunSubCommand(null, new SubCommandOptions(new string[] { "instance", "use", "inst1" }));
                Assert.AreEqual("inst1", config.AutoStartInstance);
            }
        }

        [Test]
        public void RunSubCommand_DefaultBadName_Fails()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var fakeDir = new TemporaryDirectory())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut = new GameInstance(manager, user);

                // Act
                sut.RunSubCommand(null, new SubCommandOptions(new string[] { "instance", "default", "nonexistent" }));

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              @"Couldn't find install with name ""nonexistent"", aborting"
                                          },
                                          user.RaisedErrors);
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
                                          "fakeInstance", fakeDir, "1.12.5",
                                          "--MakingHistory", "1.0.0",
                                          "--BreakingGround", "1.0.0",
                                          "--set-default",
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                Assert.Contains("fakeInstance", manager.Instances.Keys.ToArray());
                Assert.AreEqual("fakeInstance", config.AutoStartInstance);
            }
        }

        [Test]
        public void RunSubCommand_FakeDuplicateName_Fails()
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
                                          "disposable", fakeDir, "1.12.5",
                                          "--MakingHistory", "1.0.0",
                                          "--BreakingGround", "1.0.0",
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "This instance name is already taken: disposable",
                                              "--Error: bad argument(s)--",
                                          },
                                          user.RaisedErrors);
            }
        }

        [Test]
        public void RunSubCommand_FakeDestinationNotEmpty_Fails()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var fakeDir = new TemporaryDirectory())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                File.WriteAllText(Path.Combine(fakeDir, "dummy.txt"),
                                  "File that already exists in destination directory");
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[]
                                      {
                                          "instance", "fake",
                                          "fakeInstance", fakeDir, "1.12.5",
                                          "--MakingHistory", "1.0.0",
                                          "--BreakingGround", "1.0.0",
                                      };
                var         subOpts = new SubCommandOptions(args);

                // Act
                sut.RunSubCommand(null, subOpts);

                // Assert
                CollectionAssert.AreEqual(new string[]
                                          {
                                              "The specified folder already exists and is not empty",
                                              "--Error: bad argument(s)--",
                                          },
                                          user.RaisedErrors);
            }
        }

        [TestCase("add",
                  new string[]
                  {
                      "instance add - Add a game instance",
                      "Usage: ckan instance add [options] name url",
                  }),
         TestCase("clone",
                  new string[]
                  {
                      "instance clone - Clone an existing game instance",
                      "Usage: ckan instance clone [options] instanceNameOrPath newname newpath",
                  }),
         TestCase("rename",
                  new string[]
                  {
                      "instance rename - Rename a game instance",
                      "Usage: ckan instance rename [options] oldname newname",
                  }),
         TestCase("forget",
                  new string[]
                  {
                      "instance forget - Forget a game instance",
                      "Usage: ckan instance forget [options] name",
                  }),
         TestCase("fake",
                  new string[]
                  {
                      "instance fake - Fake a game instance",
                      "Usage: ckan instance fake [options] name path version [--game KSP|KSP2] [--MakingHistory <version>] [--BreakingGround <version>]",
                  }),
        ]
        public void RunSubCommand_NoArguments_PrintsHelp(string verb, string[] help)
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            using (var inst    = new DisposableKSP())
            using (var fakeDir = new TemporaryDirectory())
            using (var config  = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager = new GameInstanceManager(user, config))
            {
                ISubCommand sut     = new GameInstance(manager, user);
                var         args    = new string[] { "instance", verb };
                var         subOpts = new SubCommandOptions(args);

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
