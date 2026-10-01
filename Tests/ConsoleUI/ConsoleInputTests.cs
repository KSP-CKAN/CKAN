using System.Collections.Generic;

using NUnit.Framework;

using CKAN.ConsoleUI.Toolkit;

namespace Tests.ConsoleUI
{
    [TestFixture]
    public class ConsoleInputTests
    {
        // ConsoleInput.Start() is never called because it blocks in Console.ReadKey.

        [Test]
        public void PumpEvent_PostedAction_RunsAndReturnsNull()
        {
            var owner = new object();
            bool ran = false;
            ConsoleInput.Post(() => ran = true, owner);

            var key = ConsoleInput.PumpEvent(owner);

            Assert.IsNull(key);
            Assert.IsTrue(ran);
        }

        [Test]
        public void PumpEvent_MultiplePostedActions_RunInOrder()
        {
            var owner = new object();
            var order = new List<int>();
            ConsoleInput.Post(() => order.Add(1), owner);
            ConsoleInput.Post(() => order.Add(2), owner);
            ConsoleInput.Post(() => order.Add(3), owner);

            ConsoleInput.PumpEvent(owner);
            ConsoleInput.PumpEvent(owner);
            ConsoleInput.PumpEvent(owner);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, order);
        }

        [Test]
        public void PumpEvent_AnotherOwnerPumping_DropsAction()
        {
            bool ran = false;
            ConsoleInput.Post(() => ran = true, new object());

            var key = ConsoleInput.PumpEvent(new object());

            Assert.IsNull(key);
            Assert.IsFalse(ran);
        }
    }
}
