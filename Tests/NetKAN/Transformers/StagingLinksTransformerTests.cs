using System;
using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class StagingLinksTransformerTests
    {
        [Test]
        public void Transform_Staged_TableAdded()
        {
            // Arrange
            var sut      = new StagingLinksTransformer();
            var opts     = new TransformOptions(1, null, null, null,
                                                true, "Test staging links");
            var metadata = new Metadata(new JObject()
            {
                {
                    "resources",
                    new JObject()
                    {
                        { "homepage",   "https://github.com/" },
                        { "repository", "https://github.com/" },
                        { "bugtracker", "https://github.com/" },
                        { "manual",     "https://github.com/" },
                    }
                }
            });

            // Act
            sut.Transform(metadata, opts).First();
            var allReasons = string.Join(Environment.NewLine,
                                         opts.StagingReasons);

            // Assert
            Assert.IsTrue(allReasons.Contains("homepage"));
            Assert.IsTrue(allReasons.Contains("repository"));
            Assert.IsTrue(allReasons.Contains("bugtracker"));
            Assert.IsTrue(allReasons.Contains("manual"));
        }
    }
}
