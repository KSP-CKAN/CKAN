#if NETFRAMEWORK || WINDOWS

using NUnit.Framework;

using CKAN.GUI;

namespace Tests.GUI.Model
{
    [TestFixture]
    public class ComparisonSelectionTests
    {
        [Test]
        public void Select_PlainClicks_LatestClickIsTheOnlyOne()
        {
            // Arrange
            var sut = new ComparisonSelection();

            // Act
            sut.Select(3, false, false, new int[] { 3 }, 3);
            sut.Select(7, false, false, new int[] { 7 }, 7);

            // Assert
            Assert.AreEqual(7, sut.Reference);
            Assert.IsNull(sut.Other);
            CollectionAssert.AreEqual(new int[] { 7 }, sut.Indices);
        }

        [TestCase(2), // Newer than the reference
         TestCase(8)] // Older than the reference
        public void Select_CtrlClick_AddsOtherOnEitherSide(int other)
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);

            // Act
            sut.Select(other, true, false, new int[] { 5, other }, other);

            // Assert
            Assert.AreEqual(5, sut.Reference);
            Assert.AreEqual(other, sut.Other);
        }

        [Test]
        public void Select_CtrlClickAgain_ReplacesOtherAndKeepsReference()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);
            sut.Select(2, true, false, new int[] { 5, 2 }, 2);

            // Act
            // Between the two, which used to be impossible to pick
            sut.Select(4, true, false, new int[] { 5, 2, 4 }, 4);

            // Assert
            Assert.AreEqual(5, sut.Reference);
            Assert.AreEqual(4, sut.Other);
        }

        [Test]
        public void Select_PlainClickAfterOther_StartsOver()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);
            sut.Select(2, true, false, new int[] { 5, 2 }, 2);

            // Act
            sut.Select(9, false, false, new int[] { 9 }, 9);

            // Assert
            Assert.AreEqual(9, sut.Reference);
            Assert.IsNull(sut.Other);
        }

        [Test]
        public void Select_CtrlClickOnReference_ChangesNothing()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);
            sut.Select(2, true, false, new int[] { 5, 2 }, 2);

            // Act
            sut.Select(5, true, false, new int[] { 5, 2 }, 5);

            // Assert
            Assert.AreEqual(5, sut.Reference);
            Assert.AreEqual(2, sut.Other);
        }

        [Test]
        public void Select_CtrlClickWithNothingSelected_BecomesReference()
        {
            // Arrange
            var sut = new ComparisonSelection();

            // Act
            sut.Select(4, true, false, new int[] { 4 }, 4);

            // Assert
            Assert.AreEqual(4, sut.Reference);
            Assert.IsNull(sut.Other);
        }

        [Test]
        public void Select_ShiftClickRange_OtherIsTheClickedEnd()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);

            // Act
            // The list selects 5 through 9, the click was on 9
            sut.Select(7, false, true, new int[] { 5, 6, 7 }, 9);
            sut.Select(9, false, true, new int[] { 5, 6, 7, 8, 9 }, 9);

            // Assert
            Assert.AreEqual(5, sut.Reference);
            Assert.AreEqual(9, sut.Other);
        }

        [Test]
        public void Select_ShiftClickWithoutFocusInfo_OtherIsFarthestFromReference()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);

            // Act
            sut.Select(2, false, true, new int[] { 5, 4, 3, 2 }, null);

            // Assert
            Assert.AreEqual(5, sut.Reference);
            Assert.AreEqual(2, sut.Other);
        }

        [Test]
        public void Deselect_Other_LeavesReference()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);
            sut.Select(2, true, false, new int[] { 5, 2 }, 2);

            // Act
            sut.Deselect(2);

            // Assert
            Assert.AreEqual(5, sut.Reference);
            Assert.IsNull(sut.Other);
        }

        [Test]
        public void Deselect_Reference_OtherTakesItsPlace()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);
            sut.Select(2, true, false, new int[] { 5, 2 }, 2);

            // Act
            sut.Deselect(5);

            // Assert
            Assert.AreEqual(2, sut.Reference);
            Assert.IsNull(sut.Other);
        }

        [TestCase(new int[] { 5, 2 }, 5,    2),    // Nothing changed
         TestCase(new int[] { 5 },    5,    null), // Plain click on the reference
         TestCase(new int[] { 2 },    2,    null), // Plain click on the other
         TestCase(new int[] { 7 },    7,    null), // Something we did not track
         TestCase(new int[] { },      null, null)] // Click on empty space
        public void Reconcile_ListShowsSomethingElse_FollowsTheList(int[] actual, int? reference, int? other)
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);
            sut.Select(2, true, false, new int[] { 5, 2 }, 2);

            // Act
            sut.Reconcile(actual);

            // Assert
            Assert.AreEqual(reference, sut.Reference);
            Assert.AreEqual(other, sut.Other);
        }

        [Test]
        public void Select_ShiftClickOnReference_ChangesNothing()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(5, false, false, new int[] { 5 }, 5);
            sut.Select(2, true, false, new int[] { 5, 2 }, 2);

            // Act
            // Only the reference is in the list's range, there is no other end
            sut.Select(5, false, true, new int[] { 5 }, 5);

            // Assert
            Assert.AreEqual(5, sut.Reference);
            Assert.AreEqual(2, sut.Other);
        }

        [Test]
        public void Compared_NothingSelected_Null()
        {
            // Arrange
            var sut = new ComparisonSelection();

            // Act / Assert
            Assert.IsNull(sut.Compared(10));
        }

        [Test]
        public void Compared_OneEntry_PairedWithTheNextOlderOne()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(3, false, false, new int[] { 3 }, 3);

            // Act
            var pair = sut.Compared(10);

            // Assert
            Assert.AreEqual((3, (int?)4), pair);
        }

        [Test]
        public void Compared_OneEntryWhichIsTheOldest_NothingOlder()
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(9, false, false, new int[] { 9 }, 9);

            // Act
            var pair = sut.Compared(10);

            // Assert
            Assert.AreEqual((9, (int?)null), pair);
        }

        [TestCase(2, 8),  // Other is older
         TestCase(8, 2)]  // Other is newer
        public void Compared_TwoEntries_NewerFirstWhicheverWasClickedFirst(int reference, int other)
        {
            // Arrange
            var sut = new ComparisonSelection();
            sut.Select(reference, false, false, new int[] { reference }, reference);
            sut.Select(other, true, false, new int[] { reference, other }, other);

            // Act
            var pair = sut.Compared(10);

            // Assert
            Assert.AreEqual((2, (int?)8), pair);
        }
    }
}

#endif
