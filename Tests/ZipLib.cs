using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ZipLib
    {
        [Test]
        public void GH221()
        {
            // This is a perfectly fine file, written by 'file-roller', but
            // SharpZipLib can choke on it because it's not properly handling
            // the headers properly. See GH #221.
            string file = Path.Combine(TestData.DataDir(), "gh221.zip");

            var zipfile = new ZipFile(file);

            var entry = zipfile.GetEntry("221.txt");

            string version = string.Format("{0}", entry.Version);

            Assert.DoesNotThrow(delegate
            {
                zipfile.GetInputStream(entry);
            }, "zip-entry format {0} (788 is our bug)", version);
        }
    }
}

