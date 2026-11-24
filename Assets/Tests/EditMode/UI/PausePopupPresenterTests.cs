using System;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class PausePopupPresenterTests
    {
        // A stand-in for the real popup: no scene needed, the test "presses" the buttons by calling the Press methods.
        private sealed class FakePauseView : IPausePopupView
        {
            public event Action ResumeClicked;
            public event Action LevelSelectClicked;

            public void PressResume()
            {
                ResumeClicked?.Invoke();
            }

            public void PressLevelSelect()
            {
                LevelSelectClicked?.Invoke();
            }
        }

        // Remembers whether the pause popup was already closed at the moment the level select screen was requested.
        private sealed class FakeFlow : ILevelFlow
        {
            private readonly PopupStack _stack;
            public int OpenLevelSelectCalls;
            public bool PauseWasOpenAtCall;

            public bool CanStartNextLevel => false;

            public FakeFlow(PopupStack stack)
            {
                _stack = stack;
            }

            public void Restart()
            {
            }

            public void StartNextLevel()
            {
            }

            public void OpenLevelSelect()
            {
                OpenLevelSelectCalls++;
                PauseWasOpenAtCall = _stack.IsOpen(PopupId.Pause);
            }
        }

        [Test]
        public void ResumeClicked_PauseIsOpen_ClosesThePausePopup()
        {
            var stack = new PopupStack();
            var view = new FakePauseView();
            new PausePopupPresenter(view, stack, new FakeFlow(stack)).Enable();
            stack.Open(PopupId.Pause);

            view.PressResume();

            Assert.That(stack.IsOpen(PopupId.Pause), Is.False);
        }

        [Test]
        public void ResumeClicked_AnotherPopupIsOnTop_OnlyClosesThePausePopup()
        {
            var stack = new PopupStack();
            var view = new FakePauseView();
            new PausePopupPresenter(view, stack, new FakeFlow(stack)).Enable();
            stack.Open(PopupId.Pause);
            stack.Open(PopupId.Lose);

            view.PressResume();

            Assert.That(stack.IsOpen(PopupId.Pause), Is.False);
            Assert.That(stack.Top, Is.EqualTo(PopupId.Lose));
        }

        [Test]
        public void LevelSelectClicked_ClosesThePauseThenOpensTheLevelSelectScreen()
        {
            var stack = new PopupStack();
            var view = new FakePauseView();
            var flow = new FakeFlow(stack);
            new PausePopupPresenter(view, stack, flow).Enable();
            stack.Open(PopupId.Pause);

            view.PressLevelSelect();

            Assert.That(stack.IsOpen(PopupId.Pause), Is.False);
            Assert.That(flow.OpenLevelSelectCalls, Is.EqualTo(1));
            Assert.That(flow.PauseWasOpenAtCall, Is.False, "the popup must be closed before the screen changes");
        }

        [Test]
        public void Buttons_AfterDisable_DoNothing()
        {
            var stack = new PopupStack();
            var view = new FakePauseView();
            var flow = new FakeFlow(stack);
            var presenter = new PausePopupPresenter(view, stack, flow);
            presenter.Enable();
            presenter.Disable();
            stack.Open(PopupId.Pause);

            view.PressResume();
            view.PressLevelSelect();

            Assert.That(stack.IsOpen(PopupId.Pause), Is.True);
            Assert.That(flow.OpenLevelSelectCalls, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            var stack = new PopupStack();

            Assert.Throws<ArgumentNullException>(() => new PausePopupPresenter(null, stack, new FakeFlow(stack)));
            Assert.Throws<ArgumentNullException>(() => new PausePopupPresenter(new FakePauseView(), null, new FakeFlow(stack)));
            Assert.Throws<ArgumentNullException>(() => new PausePopupPresenter(new FakePauseView(), stack, null));
        }
    }
}
