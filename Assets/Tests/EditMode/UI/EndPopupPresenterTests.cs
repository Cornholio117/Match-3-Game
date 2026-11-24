using System;
using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Level;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class EndPopupPresenterTests
    {
        private sealed class FakeEndView : IEndPopupView
        {
            public event Action RetryClicked;
            public int ShownScore = -1;
            public int ShownProgress = -1;
            public int ShownAmount = -1;
            public int ShowCalls;

            public void ShowResult(int score, int goalProgress, int goalAmount)
            {
                ShownScore = score;
                ShownProgress = goalProgress;
                ShownAmount = goalAmount;
                ShowCalls++;
            }

            public void PressRetry()
            {
                RetryClicked?.Invoke();
            }
        }

        // Remembers whether the popup was already closed at the moment the level was restarted.
        private sealed class FakeRestarter : ILevelRestarter
        {
            private readonly PopupStack _stack;
            private readonly PopupId _popup;
            public int Calls;
            public bool PopupWasOpenAtRestart;

            public FakeRestarter(PopupStack stack, PopupId popup)
            {
                _stack = stack;
                _popup = popup;
            }

            public void Restart()
            {
                Calls++;
                PopupWasOpenAtRestart = _stack.IsOpen(_popup);
            }
        }

        private static LevelProgress ProgressWithSomeScore()
        {
            var progress = new LevelProgress(LevelGoalType.Score, 1000, PieceColor.None, 5, new ScoreRules());
            var steps = new List<BoardStep>();
            for (var x = 0; x < 3; x++)
                steps.Add(BoardStep.Clear(PieceColor.Green, new GridPosition(x, 0)));

            progress.ApplyTurn(steps);
            return progress;
        }

        [Test]
        public void Opened_OwnPopup_ShowsTheScoreAndGoalProgress()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var progress = ProgressWithSomeScore();
            new EndPopupPresenter(view, stack, PopupId.Lose, progress, new FakeRestarter(stack, PopupId.Lose)).Enable();

            stack.Open(PopupId.Lose);

            Assert.That(view.ShownScore, Is.EqualTo(30));
            Assert.That(view.ShownProgress, Is.EqualTo(30));
            Assert.That(view.ShownAmount, Is.EqualTo(1000));
        }

        [Test]
        public void Opened_AnotherPopup_ShowsNothing()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            new EndPopupPresenter(view, stack, PopupId.Lose, ProgressWithSomeScore(), new FakeRestarter(stack, PopupId.Lose)).Enable();

            stack.Open(PopupId.Pause);

            Assert.That(view.ShowCalls, Is.EqualTo(0));
        }

        [Test]
        public void RetryClicked_ClosesThePopupThenRestartsTheLevelOnce()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var restarter = new FakeRestarter(stack, PopupId.Win);
            new EndPopupPresenter(view, stack, PopupId.Win, ProgressWithSomeScore(), restarter).Enable();
            stack.Open(PopupId.Win);

            view.PressRetry();

            Assert.That(stack.IsOpen(PopupId.Win), Is.False);
            Assert.That(restarter.Calls, Is.EqualTo(1));
            Assert.That(restarter.PopupWasOpenAtRestart, Is.False, "the popup must be closed before the restart");
        }

        [Test]
        public void RetryClicked_AfterDisable_DoesNothing()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var restarter = new FakeRestarter(stack, PopupId.Win);
            var presenter = new EndPopupPresenter(view, stack, PopupId.Win, ProgressWithSomeScore(), restarter);
            presenter.Enable();
            presenter.Disable();

            view.PressRetry();

            Assert.That(restarter.Calls, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var progress = ProgressWithSomeScore();
            var restarter = new FakeRestarter(stack, PopupId.Win);

            Assert.Throws<ArgumentNullException>(() => new EndPopupPresenter(null, stack, PopupId.Win, progress, restarter));
            Assert.Throws<ArgumentNullException>(() => new EndPopupPresenter(view, null, PopupId.Win, progress, restarter));
            Assert.Throws<ArgumentNullException>(() => new EndPopupPresenter(view, stack, PopupId.Win, null, restarter));
            Assert.Throws<ArgumentNullException>(() => new EndPopupPresenter(view, stack, PopupId.Win, progress, null));
            Assert.Throws<ArgumentException>(() => new EndPopupPresenter(view, stack, PopupId.None, progress, restarter));
        }
    }
}
