using System.Collections.Generic;

using NUnit.Framework;

using CKAN;
using CKAN.CmdLine;
using Tests.Data;

namespace Tests.CmdLine
{
    [TestFixture]
    public class SearchTests
    {
        [Test]
        public void RunCommand_AGExt_Works()
        {
            var user = new CapturingUser(false, q => true, (msg, objs) => 0);
            var repo = new Repository("test", "https://github.com/");
            using (var inst     = new DisposableKSP())
            using (var repoData = new TemporaryRepositoryData(user, new Dictionary<Repository, RepositoryData>
            {
                { repo, RepositoryData.FromJson(TestData.TestRepository(), null)! },
            }))
            using (var regMgr = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                         new Repository[] { repo }))
            {
                ICommand sut  = new Search(repoData.Manager, user);
                var      opts = new SearchOptions()
                                {
                                    detail      = true,
                                    search_term = "AGExt",
                                };

                // Act
                sut.RunCommand(inst.KSP, opts);

                // Assert
                CollectionAssert.AreEqual(
                    new string[]
                    {
                        "Found 1 compatible mods matching \"AGExt\"",
                        "Matching compatible mods:",
                        "* AGExt (1.24a) - Action Groups Extended by Diazo - Increases the number of action groups to 250 and allows in-flight editing.",
                    },
                    user.RaisedMessages);
            }
        }
    }
}
