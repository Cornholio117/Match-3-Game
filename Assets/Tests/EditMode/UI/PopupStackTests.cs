using System;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class PopupStackTests
    {
        [Test]
        public void Open_NewPopup_BecomesTheTopOne()
        {
            var stack = new PopupStack();

            var opened = stack.Open(PopupId.Pause);

            Assert.That(opened, Is.True);
            Assert.That(stack.Top, Is.EqualTo(PopupId.Pause));
            Assert.That(stack.Count, Is.EqualTo(1));
        }

        [Test]
        public void Open_SecondPopup_StacksOnTopOfTheFirst()
        {
            var stack = new PopupStack();
            stack.Open(PopupId.Pause);

            stack.Open(PopupId.Lose);

            Assert.That(stack.Top, Is.EqualTo(PopupId.Lose));
            Assert.That(stack.Count, Is.EqualTo(2));
        }

        [Test]
        public void Open_PopupAlreadyOpen_ReturnsFalseAndKeepsOneCopy()
        {
            var stack = new PopupStack();
            stack.Open(PopupId.Pause);

            var opened = stack.Open(PopupId.Pause);

            Assert.That(opened, Is.False);
            Assert.That(stack.Count, Is.EqualTo(1));
        }

        [Test]
        public void Open_None_Throws()
        {
            var stack = new PopupStack();

            Assert.Throws<ArgumentException>(() => stack.Open(PopupId.None));
        }

        [Test]
        public void CloseTop_NothingOpen_ReturnsFalse()
        {
            var stack = new PopupStack();

            Assert.That(stack.CloseTop(), Is.False);
            Assert.That(stack.Top, Is.EqualTo(PopupId.None));
        }

        [Test]
        public void CloseTop_TwoOpen_ClosesOnlyTheTopOne()
        {
            var stack = new PopupStack();
            stack.Open(PopupId.Pause);
            stack.Open(PopupId.Lose);

            var closed = stack.CloseTop();

            Assert.That(closed, Is.True);
            Assert.That(stack.Top, Is.EqualTo(PopupId.Pause));
            Assert.That(stack.IsOpen(PopupId.Lose), Is.False);
        }

        [Test]
        public void Close_PopupBelowTheTop_RemovesItAndKeepsTheTop()
        {
            var stack = new PopupStack();
            stack.Open(PopupId.Pause);
            stack.Open(PopupId.Lose);

            var closed = stack.Close(PopupId.Pause);

            Assert.That(closed, Is.True);
            Assert.That(stack.Top, Is.EqualTo(PopupId.Lose));
            Assert.That(stack.Count, Is.EqualTo(1));
        }

        [Test]
        public void Close_PopupNotOpen_ReturnsFalse()
        {
            var stack = new PopupStack();
            stack.Open(PopupId.Pause);

            Assert.That(stack.Close(PopupId.Win), Is.False);
            Assert.That(stack.Count, Is.EqualTo(1));
        }

        [Test]
        public void CloseTopOnBack_PausePopup_ClosesIt()
        {
            var stack = new PopupStack();
            stack.Open(PopupId.Pause);

            Assert.That(stack.CloseTopOnBack(), Is.True);
            Assert.That(stack.Count, Is.EqualTo(0));
        }

        [TestCase(PopupId.Win)]
        [TestCase(PopupId.Lose)]
        public void CloseTopOnBack_EndPopup_StaysOpen(PopupId endPopup)
        {
            var stack = new PopupStack();
            stack.Open(endPopup);

            Assert.That(stack.CloseTopOnBack(), Is.False);
            Assert.That(stack.IsOpen(endPopup), Is.True);
        }

        [Test]
        public void CloseTopOnBack_PauseOnTopOfAnEndPopup_ClosesOnlyThePause()
        {
            var stack = new PopupStack();
            stack.Open(PopupId.Win);
            stack.Open(PopupId.Pause);

            Assert.That(stack.CloseTopOnBack(), Is.True);
            Assert.That(stack.Top, Is.EqualTo(PopupId.Win));
        }

        [Test]
        public void CloseTopOnBack_NothingOpen_ReturnsFalse()
        {
            Assert.That(new PopupStack().CloseTopOnBack(), Is.False);
        }

        [Test]
        public void Events_OpenAndClose_ReportTheSamePopup()
        {
            var stack = new PopupStack();
            var opened = PopupId.None;
            var closed = PopupId.None;
            stack.Opened += id => opened = id;
            stack.Closed += id => closed = id;

            stack.Open(PopupId.Win);
            stack.CloseTop();

            Assert.That(opened, Is.EqualTo(PopupId.Win));
            Assert.That(closed, Is.EqualTo(PopupId.Win));
        }

        [Test]
        public void Events_OpenAlreadyOpenPopup_DoesNotRaiseOpenedAgain()
        {
            var stack = new PopupStack();
            stack.Open(PopupId.Pause);
            var raised = 0;
            stack.Opened += _ => raised++;

            stack.Open(PopupId.Pause);

            Assert.That(raised, Is.EqualTo(0));
        }
    }
}
