using System;
using System.IO;

using CKAN.CmdLine;

using NUnit.Framework;

using CKAN;
using Tests.Core.Configuration;
using Tests.Data;

namespace Tests.CmdLine
{
    [TestFixture]
    public class RepairTests
    {
        [Test]
        public void RunSubCommand_Registry_ReindexesInstalled()
        {
            // Arrange
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var modGen = new RandomModuleGenerator(new Random());
            var mod1   = modGen.GenerateRandomModule();
            var mod2   = modGen.GenerateRandomModule();
            var mod3   = modGen.GenerateRandomModule();
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user))
            using (var config   = new FakeConfiguration(inst.KSP, inst.KSP.Name))
            using (var manager  = new GameInstanceManager(new NullUser(), config))
            {
                var gamedata = inst.KSP.game.PrimaryModDirectory(inst.KSP);
                Assert.IsTrue(Path.IsPathRooted(gamedata));
                File.WriteAllText(Path.Combine(inst.KSP.CkanDir(), "registry.json"),
                                  $@"{{
                                      ""registry_version"": 3,
                                      ""installed_files"": {{ }},
                                      ""installed_modules"": {{
                                          ""Mod1"": {{
                                              ""source_module"": {mod1.ToJson()},
                                              ""installed_files"": {{
                                                  ""{gamedata}/Mod1.dll"": {{}}
                                              }}
                                          }},
                                          ""Mod2"": {{
                                              ""source_module"": {mod2.ToJson()},
                                              ""installed_files"": {{
                                                  ""{gamedata}/Mod2.dll"": {{}}
                                              }}
                                          }},
                                          ""Mod3"": {{
                                              ""source_module"": {mod3.ToJson()},
                                              ""installed_files"": {{
                                                  ""{gamedata}/Mod3.dll"": {{}}
                                              }}
                                          }}
                                      }}
                                  }}");

                using (var regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager))
                {
                    ISubCommand sut      = new Repair(manager, repoData.Manager, user);
                    var         args     = new string[] { "repair", "registry" };
                    var         subOpts  = new SubCommandOptions(args);
                    var         registry = regMgr.registry;

                    // Act
                    CollectionAssert.IsEmpty(registry.InstalledFileInfo());
                    sut.RunSubCommand(null, subOpts);

                    // Assert
                    CollectionAssert.IsNotEmpty(registry.InstalledFileInfo());
                }
            }
        }
    }
}
