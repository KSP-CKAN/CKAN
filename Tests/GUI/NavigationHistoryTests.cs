namespace Tests.GUI
{
    using System;
    using CKAN;
    using NUnit.Framework;

    [TestFixture]
    public class NavigationHistoryTests
    {
        [Test]
        public void Initially_CannotNavigate()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            // act

            // assert

            Assert.IsFalse(nav.CanNavigateBackward);
            Assert.IsFalse(nav.CanNavigateForward);
        }

        [Test]
        public void After_SingleHistoryItem_CannotNavigate()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            // act

            nav.AddToHistory(42);

            // assert

            Assert.IsFalse(nav.CanNavigateBackward);
            Assert.IsFalse(nav.CanNavigateForward);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void NavigatingBackward_WhenUnable_ThrowsException()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            // act

            nav.NavigateBackward();

            // assert

            Assert.Fail();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void NavigatingForward_WhenUnable_ThrowsException()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            // act

            nav.NavigateForward();

            // assert

            Assert.Fail();
        }

        [Test]
        public void After_MultipleHistoryItems_CanNavigateBackward()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            // act

            nav.AddToHistory(1);
            nav.AddToHistory(2);

            // assert

            Assert.IsTrue(nav.CanNavigateBackward);
        }

        [Test]
        public void While_AtHeadOfHistory_CannotNavigateForward()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            // act

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            // assert

            Assert.IsFalse(nav.CanNavigateForward);
        }

        [Test]
        public void After_NavigatingBackward_CanNavigateForward()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            // act

            nav.NavigateBackward();

            // assert

            Assert.IsTrue(nav.CanNavigateForward);
        }

        [Test]
        public void NavigatingBackward_ProducesCorrectItem()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            // act

            var two = nav.NavigateBackward();
            var one = nav.NavigateBackward();

            // assert

            Assert.AreEqual(2, two);
            Assert.AreEqual(1, one);
        }

        [Test]
        public void NavigatingForward_ProducesCorrectItem()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.NavigateBackward();
            nav.NavigateBackward();

            // act

            var two = nav.NavigateForward();
            var three = nav.NavigateForward();

            // assert

            Assert.AreEqual(2, two);
            Assert.AreEqual(3, three);
        }

        [Test]
        public void After_AddingHistory_CannotNavigateForward()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.NavigateBackward();
            nav.NavigateBackward();

            // act

            nav.AddToHistory(9);

            // assert

            Assert.IsFalse(nav.CanNavigateForward);
        }

        [Test]
        public void After_AddingHistory_ForwardHistoryIsDestroyed()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);
            nav.AddToHistory(4);
            nav.AddToHistory(5);

            nav.NavigateBackward();
            nav.NavigateBackward();
            nav.NavigateBackward();
            nav.NavigateBackward();

            // act

            nav.AddToHistory(9);

            var one = nav.NavigateBackward();
            var navBackAfterOne = nav.CanNavigateBackward;
            var nine = nav.NavigateForward();
            var navForwAfterNine = nav.CanNavigateForward;

            // assert

            Assert.IsFalse(navBackAfterOne);
            Assert.IsFalse(navForwAfterNine);
            Assert.AreEqual(1, one);
            Assert.AreEqual(9, nine);
        }

        [Test]
        public void ReadOnlyMode_BlocksAddingToHistory()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.IsReadOnly = true;

            // act

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.IsReadOnly = false;

            // assert

            Assert.IsFalse(nav.CanNavigateBackward);
            Assert.IsFalse(nav.CanNavigateForward);
        }

        [Test]
        public void ReadOnlyMode_BlocksBackwardNavigation()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.IsReadOnly = true;

            // act

            nav.NavigateBackward();
            nav.NavigateBackward();
            nav.NavigateBackward();

            var canStillNavigate = nav.CanNavigateBackward;

            nav.IsReadOnly = false;

            var two = nav.NavigateBackward();

            // assert

            Assert.True(canStillNavigate);
            Assert.AreEqual(2, two);
        }

        [Test]
        public void ReadOnlyMode_BlocksForwardNavigation()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.NavigateBackward();
            nav.NavigateBackward();

            nav.IsReadOnly = true;

            // act

            nav.NavigateForward();
            nav.NavigateForward();
            nav.NavigateForward();

            var canStillNavigate = nav.CanNavigateForward;

            nav.IsReadOnly = false;

            var two = nav.NavigateForward();

            // assert

            Assert.True(canStillNavigate);
            Assert.AreEqual(2, two);
        }

        [Test]
        public void BackwardsNavigation_DuringReadOnlyMode_ProducesDefaultValue()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.IsReadOnly = true;

            // act

            var zero = nav.NavigateBackward();

            // assert

            Assert.AreEqual(default(int), zero);
        }

        [Test]
        public void ForwardsNavigation_DuringReadOnlyMode_ProducesDefaultValue()
        {
            // arrange

            var nav = new NavigationHistory<int>();

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.NavigateBackward();
            nav.NavigateBackward();

            nav.IsReadOnly = true;

            // act

            var zero = nav.NavigateForward();

            // assert

            Assert.AreEqual(default(int), zero);
        }

        [Test]
        public void AddingToHistory_InvokesChangeEvent()
        {
            // arrange

            var nav = new NavigationHistory<int>();
            var eventCount = 0;

            nav.OnHistoryChange += () =>
            {
                eventCount++;
            };

            // act

            nav.AddToHistory(1);

            // assert

            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void NavigatingBackward_InvokesChangeEvent()
        {
            // arrange

            var nav = new NavigationHistory<int>();
            var eventCount = 0;

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.OnHistoryChange += () =>
            {
                eventCount++;
            };

            // act

            nav.NavigateBackward();

            // assert

            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void NavigatingForward_InvokesChangeEvent()
        {
            // arrange

            var nav = new NavigationHistory<int>();
            var eventCount = 0;

            nav.AddToHistory(1);
            nav.AddToHistory(2);
            nav.AddToHistory(3);

            nav.NavigateBackward();
            nav.NavigateBackward();

            nav.OnHistoryChange += () =>
            {
                eventCount++;
            };

            // act

            nav.NavigateForward();

            // assert

            Assert.AreEqual(1, eventCount);
        }
    }
}
