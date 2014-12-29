using System.IO;
using NUnit.Framework;
using Tests;

namespace CKANTests
{
	[TestFixture]
	public class FileIdentifier
	{
		[Test]
		public void IdentifyASCII()
		{
			// Check that we have the zip files to compare against.
			string ascii_file_1 = TestData.DataDir("FileIdentifier/test_ascii.txt");
			string ascii_file_2 = TestData.DataDir("FileIdentifier/test_ascii.tmp");

			Assert.IsTrue(File.Exists(ascii_file_1));
			Assert.IsTrue(File.Exists(ascii_file_2));

			// Check that both files return a tar type.
			Assert.IsTrue(CKAN.FileIdentifier.IdentifyFile(ascii_file_1) == CKAN.FileType.ASCII);
			Assert.IsTrue(CKAN.FileIdentifier.IdentifyFile(ascii_file_2) == CKAN.FileType.ASCII);
		}

		[Test]
		public void IdentifyTar()
		{
			// Check that we have the zip files to compare against.
			string tar_file_1 = TestData.DataDir("FileIdentifier/test_tar.tar");
			string tar_file_2 = TestData.DataDir("FileIdentifier/test_tar.tmp");

			Assert.IsTrue(File.Exists(tar_file_1));
			Assert.IsTrue(File.Exists(tar_file_2));

			// Check that both files return a tar type.
			Assert.IsTrue(CKAN.FileIdentifier.IdentifyFile(tar_file_1) == CKAN.FileType.Tar);
			Assert.IsTrue(CKAN.FileIdentifier.IdentifyFile(tar_file_2) == CKAN.FileType.Tar);
		}

		[Test]
		public void IdentifyTarGz()
		{
			// Check that we have the zip files to compare against.
			string targz_file_1 = TestData.DataDir("FileIdentifier/test_targz.tar.gz");
			string targz_file_2 = TestData.DataDir("FileIdentifier/test_targz.tmp");

			Assert.IsTrue(File.Exists(targz_file_1));
			Assert.IsTrue(File.Exists(targz_file_2));

			// Check that both files return a tar.gz type.
			Assert.IsTrue(CKAN.FileIdentifier.IdentifyFile(targz_file_1) == CKAN.FileType.TarGz);
			Assert.IsTrue(CKAN.FileIdentifier.IdentifyFile(targz_file_2) == CKAN.FileType.TarGz);
		}

		[Test]
		public void IdentifyZip()
		{
			// Check that we have the zip files to compare against.
			string zip_file_1 = TestData.DataDir("FileIdentifier/test_zip.zip");
			string zip_file_2 = TestData.DataDir("FileIdentifier/test_zip.tmp");

			Assert.IsTrue(File.Exists(zip_file_1));
			Assert.IsTrue(File.Exists(zip_file_2));

			// Check that both files return a zip type.
			Assert.IsTrue(CKAN.FileIdentifier.IdentifyFile(zip_file_1) == CKAN.FileType.Zip);
			Assert.IsTrue(CKAN.FileIdentifier.IdentifyFile(zip_file_2) == CKAN.FileType.Zip);
		}
	}
}
