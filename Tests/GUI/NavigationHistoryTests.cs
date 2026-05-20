#if NETFRAMEWORK || WINDOWS

using NUnit.Framework;

using CKAN.GUI;

namespace Tests.GUI
{
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
        public void After_DuplicateHistoryItem_NoChange()
        {
            // Arrange
            var nav = new NavigationHistory<int>();

            // Act / Assert
            nav.AddToHistory(1);
            Assert.IsFalse(nav.TryGoForward(out int val));
            Assert.AreEqual(default(int), val);
            nav.AddToHistory(2);
            nav.AddToHistory(3);
            Assert.IsTrue(nav.TryGoBackward(out val));
            Assert.AreEqual(2, val);
            nav.AddToHistory(2);
            nav.AddToHistory(2);
            Assert.IsTrue(nav.TryGoForward(out val));
            Assert.AreEqual(3, val);
            Assert.IsFalse(nav.CanNavigateForward);
            Assert.IsTrue(nav.TryGoBackward(out val));
            Assert.AreEqual(2, val);
            Assert.IsTrue(nav.TryGoBackward(out val));
            Assert.AreEqual(1, val);
            Assert.IsTrue(nav.CanNavigateForward);
            Assert.IsFalse(nav.TryGoBackward(out val));
            Assert.AreEqual(default(int), val);
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

            nav.TryGoBackward(out _);

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

            nav.TryGoBackward(out var two);
            nav.TryGoBackward(out var one);

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

            nav.TryGoBackward(out _);
            nav.TryGoBackward(out _);

            // act

            nav.TryGoForward(out var two);
            nav.TryGoForward(out var three);

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

            nav.TryGoBackward(out _);
            nav.TryGoBackward(out _);

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

            nav.TryGoBackward(out _);
            nav.TryGoBackward(out _);
            nav.TryGoBackward(out _);
            nav.TryGoBackward(out _);

            // act

            nav.AddToHistory(9);

            nav.TryGoBackward(out var one);
            var navBackAfterOne = nav.CanNavigateBackward;
            nav.TryGoForward(out var nine);
            var navForwAfterNine = nav.CanNavigateForward;

            // assert

            Assert.IsFalse(navBackAfterOne);
            Assert.IsFalse(navForwAfterNine);
            Assert.AreEqual(1, one);
            Assert.AreEqual(9, nine);
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

            nav.TryGoBackward(out _);

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

            nav.TryGoBackward(out _);
            nav.TryGoBackward(out _);

            nav.OnHistoryChange += () =>
            {
                eventCount++;
            };

            // act

            nav.TryGoForward(out _);

            // assert

            Assert.AreEqual(1, eventCount);
        }
    }
}

#endif
