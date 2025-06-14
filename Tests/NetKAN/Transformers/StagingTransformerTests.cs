using System.Linq;

using Newtonsoft.Json.Linq;
using NUnit.Framework;

using CKAN.NetKAN.Model;
using CKAN.NetKAN.Transformers;
using CKAN.Games.KerbalSpaceProgram;

namespace Tests.NetKAN.Transformers
{
    [TestFixture]
    public sealed class StagingTransformerTests
    {
        [Test]
        public void Transform_LatestGameVersion_Unstaged()
        {
            // Arrange
            var sut      = new StagingTransformer(new KerbalSpaceProgram());
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "ksp_version", "1.12.5" },
            });

            // Act
            sut.Transform(metadata, opts).First();

            // Assert
            Assert.IsFalse(opts.Staged);
            CollectionAssert.IsEmpty(opts.StagingReasons);
        }

        [Test]
        public void Transform_OldGameVersion_Staged()
        {
            // Arrange
            var sut      = new StagingTransformer(new KerbalSpaceProgram());
            var opts     = new TransformOptions(1, null, null, null, false, null);
            var metadata = new Metadata(new JObject()
            {
                { "ksp_version", "1.12.4" },
            });

            // Act
            sut.Transform(metadata, opts).First();

            // Assert
            Assert.IsTrue(opts.Staged);
            CollectionAssert.IsNotEmpty(opts.StagingReasons);
        }
    }
}
