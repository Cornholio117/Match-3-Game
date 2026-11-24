using System;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class PausePopupPresenterTests
    {
        // A stand-in for the real popup: no scene needed, the test "presses" the button by calling PressResume.
        private sealed class FakePauseView : IPausePopupView
        {
            public event Action ResumeClicked;

            public void PressResume()
            {
                ResumeClicked?.Invoke();
            }
        }

        [Test]
        public void ResumeClicked_PauseIsOpen_ClosesThePausePopup()
        {
            var stack = new PopupStack();
            var view = new FakePauseView();
            var presenter = new PausePopupPresenter(view, stack);
            presenter.Enable();
            stack.Open(PopupId.Pause);

            view.PressResume();

            Assert.That(stack.IsOpen(PopupId.Pause), Is.False);
        }

        [Test]
        public void ResumeClicked_AnotherPopupIsOnTop_OnlyClosesThePausePopup()
        {
            var stack = new PopupStack();
            var view = new FakePauseView();
            new PausePopupPresenter(view, stack).Enable();
            stack.Open(PopupId.Pause);
            stack.Open(PopupId.Lose);

            view.PressResume();

            Assert.That(stack.IsOpen(PopupId.Pause), Is.False);
            Assert.That(stack.Top, Is.EqualTo(PopupId.Lose));
        }

        [Test]
        public void ResumeClicked_AfterDisable_DoesNothing()
        {
            var stack = new PopupStack();
            var view = new FakePauseView();
            var presenter = new PausePopupPresenter(view, stack);
            presenter.Enable();
            presenter.Disable();
            stack.Open(PopupId.Pause);

            view.PressResume();

            Assert.That(stack.IsOpen(PopupId.Pause), Is.True);
        }

        [Test]
        public void Constructor_NullView_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PausePopupPresenter(null, new PopupStack()));
        }

        [Test]
        public void Constructor_NullStack_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PausePopupPresenter(new FakePauseView(), null));
        }
    }
}
