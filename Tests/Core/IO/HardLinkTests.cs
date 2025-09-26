using System.IO;
using System.Linq;

using NUnit.Framework;
using ChinhDo.Transactions.FileManager;

using CKAN.IO;
using Tests.Data;

namespace Tests.Core.IO
{
    [TestFixture]
    public class HardLinkTests
    {
        [Test]
        public void CreateOrCopy_AFile_MakesHardLink()
        {
            // Arrange
            var target = TestData.DogeCoinFlagZip();
            using (var dir = new TemporaryDirectory())
            {
                var link = Path.Combine(dir.Directory.FullName,
                                        "hardlink.txt");

                // Act
                HardLink.CreateOrCopy(target, link, new TxFileManager());

                // Assert
                FileAssert.Exists(link);
                CollectionAssert.AreEqual(new ulong[] { 2, 2 },
                                          HardLink.GetLinkCounts(new string[]
                                                                 {
                                                                     target,
                                                                     link,
                                                                 }));
            }
            CollectionAssert.AreEqual(new ulong[] { 1 },
                                      HardLink.GetLinkCounts(Enumerable.Repeat(target, 1)));
        }
    }
}
