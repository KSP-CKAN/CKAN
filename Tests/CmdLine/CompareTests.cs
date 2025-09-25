using System.Linq;

using NUnit.Framework;

using CKAN.CmdLine;

namespace Tests.CmdLine
{
    [TestFixture]
    public class CompareTests
    {
        [TestCase("1", "1", false, ExpectedResult = @"""1"" and ""1"" are the same versions.")]
        [TestCase("1", "2", false, ExpectedResult = @"""1"" is lower than ""2"".")]
        [TestCase("2", "1", false, ExpectedResult = @"""2"" is higher than ""1"".")]
        [TestCase("1", "1", true,  ExpectedResult = "0")]
        [TestCase("1", "2", true,  ExpectedResult = "-1")]
        [TestCase("2", "1", true,  ExpectedResult = "1")]
        public string RunCommand_WithVersions_Correct(string v1, string v2, bool machineReadable)
        {
            // Arrange
            var user = new CapturingUser(false, q => false, (msg, objs) => 0);
            var sut  = new Compare(user);
            var opts = new CompareOptions()
            {
                Left             = v1,
                Right            = v2,
                machine_readable = machineReadable,
            };

            // Act
            sut.RunCommand(opts);
            return user.RaisedMessages.Single();
        }

        [TestCase(null),
         TestCase("x")]
        public void RunCommand_WithoutVersions_PrintsUsage(string? arg1)
        {
            // Arrange
            var user = new CapturingUser(false, q => false, (msg, objs) => 0);
            var sut  = new Compare(user);
            var opts = new CompareOptions() { Left = arg1 };

            // Act
            sut.RunCommand(opts);

            // Assert
            CollectionAssert.AreEqual(new string[]
                                      {
                                          "argument missing, perhaps you forgot it?",
                                          " ",
                                          "Usage: ckan compare [options] version1 version2"
                                      },
                                      user.RaisedErrors);
        }
    }
}
