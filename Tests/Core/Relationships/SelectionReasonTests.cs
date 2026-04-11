using System;

using NUnit.Framework;

using CKAN;
using Tests.Data;

namespace Tests.Core.Relationships
{
    [TestFixture]
    public class SelectionReasonTests
    {
        [Test]
        public void ToStringAndDescribeWith_VariousReasons_MatchExpected()
        {
            // Arrange
            var modGen = new RandomModuleGenerator(new Random());
            var mod    = modGen.GenerateRandomModule();
            var other  = modGen.GenerateRandomModule();
            var others = new SelectionReason[] { new SelectionReason.Depends(other) };

            // Act / Assert
            Assert.AreEqual("Currently installed",
                            new SelectionReason.Installed().ToString());
            Assert.AreEqual("Currently installed",
                            new SelectionReason.Installed().DescribeWith(others));
            Assert.AreEqual("Requested by user",
                            new SelectionReason.UserRequested().ToString());
            Assert.AreEqual("Requested by user",
                            new SelectionReason.UserRequested().DescribeWith(others));
            Assert.AreEqual("Dependency removed",
                            new SelectionReason.DependencyRemoved().ToString());
            Assert.AreEqual("Dependency removed",
                            new SelectionReason.DependencyRemoved().DescribeWith(others));
            Assert.AreEqual("Auto-installed, depending modules removed",
                            new SelectionReason.NoLongerUsed().ToString());
            Assert.AreEqual("Auto-installed, depending modules removed",
                            new SelectionReason.NoLongerUsed().DescribeWith(others));
            Assert.AreEqual($"Replacing {mod}",
                            new SelectionReason.Replacement(mod).ToString());
            Assert.AreEqual($"Replacing {mod}, {other}",
                            new SelectionReason.Replacement(mod).DescribeWith(others));
            Assert.AreEqual($"Suggested by {mod}",
                            new SelectionReason.Suggested(mod).ToString());
            Assert.AreEqual($"Suggested by {mod}",
                            new SelectionReason.Suggested(mod).DescribeWith(others));
            Assert.AreEqual($"Recommended by {mod}",
                            new SelectionReason.Recommended(mod, 0).ToString());
            Assert.AreEqual($"Recommended by {mod}",
                            new SelectionReason.Recommended(mod, 0).WithIndex(1).ToString());
            Assert.AreEqual($"Recommended by {mod}",
                            new SelectionReason.Recommended(mod, 0).DescribeWith(others));
            Assert.AreEqual($"Dependency of {mod}",
                            new SelectionReason.Depends(mod).ToString());
            Assert.AreEqual($"Dependency of {mod}, {other}",
                            new SelectionReason.Depends(mod).DescribeWith(others));
            Assert.AreEqual($"Dependency of {mod} to satisfy VirtIdent",
                            new SelectionReason.VirtualDepends(mod, "VirtIdent").ToString());
            Assert.AreEqual($"Dependency of {mod}, {other} to satisfy VirtIdent",
                            new SelectionReason.VirtualDepends(mod, "VirtIdent").DescribeWith(others));
        }

        [Test]
        public void Equals_VariousCombinations_Correct()
        {
            // Arrange
            var modGen = new RandomModuleGenerator(new Random());
            var mod    = modGen.GenerateRandomModule();
            var other  = modGen.GenerateRandomModule();

            // Act / Assert
            Assert.IsTrue(new SelectionReason.NoLongerUsed()
                                             .Equals(new SelectionReason.NoLongerUsed()));
            Assert.IsFalse(new SelectionReason.NoLongerUsed()
                                              .Equals(new SelectionReason.DependencyRemoved()));
            Assert.IsTrue(new SelectionReason.Depends(mod)
                                             .Equals(new SelectionReason.Depends(mod)));
            Assert.IsFalse(new SelectionReason.Depends(mod)
                                              .Equals(new SelectionReason.Depends(other)));
            Assert.IsFalse(new SelectionReason.Depends(mod)
                                              .Equals(new SelectionReason.Suggested(mod)));
        }
    }
}
