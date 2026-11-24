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
            public event Action NextLevelClicked;
            public int ShownScore = -1;
            public int ShownProgress = -1;
            public int ShownAmount = -1;
            public int ShownStars = -1;
            public bool ShownCanGoNext;
            public int ShowCalls;

            public void ShowResult(int score, int goalProgress, int goalAmount, int stars, bool canGoNext)
            {
                ShownScore = score;
                ShownProgress = goalProgress;
                ShownAmount = goalAmount;
                ShownStars = stars;
                ShownCanGoNext = canGoNext;
                ShowCalls++;
            }

            public void PressRetry()
            {
                RetryClicked?.Invoke();
            }

            public void PressNextLevel()
            {
                NextLevelClicked?.Invoke();
            }
        }

        // Remembers whether the popup was already closed at the moment the level flow was asked to move on.
        private sealed class FakeFlow : ILevelFlow
        {
            private readonly PopupStack _stack;
            private readonly PopupId _popup;
            public int RestartCalls;
            public int NextCalls;
            public bool PopupWasOpenAtCall;

            public bool CanStartNextLevel { get; set; }

            public FakeFlow(PopupStack stack, PopupId popup)
            {
                _stack = stack;
                _popup = popup;
            }

            public void Restart()
            {
                RestartCalls++;
                PopupWasOpenAtCall = _stack.IsOpen(_popup);
            }

            public void StartNextLevel()
            {
                NextCalls++;
                PopupWasOpenAtCall = _stack.IsOpen(_popup);
            }
        }

        private static List<BoardStep> ThreeGreenClears()
        {
            var steps = new List<BoardStep>();
            for (var x = 0; x < 3; x++)
                steps.Add(BoardStep.Clear(PieceColor.Green, new GridPosition(x, 0)));

            return steps;
        }

        // A level in progress with 30 points scored.
        private static LevelProgress ProgressWithSomeScore()
        {
            var progress = new LevelProgress(LevelGoalType.Score, 1000, PieceColor.None, 5, new ScoreRules());
            progress.ApplyTurn(ThreeGreenClears());
            return progress;
        }

        // A won level: 30 points against a goal of 15 is exactly twice the goal, so 3 stars.
        private static LevelProgress WonWithThreeStars()
        {
            var progress = new LevelProgress(LevelGoalType.Score, 15, PieceColor.None, 5, new ScoreRules());
            progress.ApplyTurn(ThreeGreenClears());
            return progress;
        }

        [Test]
        public void Opened_OwnPopup_ShowsTheScoreAndGoalProgress()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            new EndPopupPresenter(view, stack, PopupId.Lose, ProgressWithSomeScore(), new FakeFlow(stack, PopupId.Lose)).Enable();

            stack.Open(PopupId.Lose);

            Assert.That(view.ShownScore, Is.EqualTo(30));
            Assert.That(view.ShownProgress, Is.EqualTo(30));
            Assert.That(view.ShownAmount, Is.EqualTo(1000));
        }

        [Test]
        public void Opened_WonLevel_ShowsTheStarsAndWhetherTheNextLevelCanStart()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var flow = new FakeFlow(stack, PopupId.Win) { CanStartNextLevel = true };
            new EndPopupPresenter(view, stack, PopupId.Win, WonWithThreeStars(), flow).Enable();

            stack.Open(PopupId.Win);

            Assert.That(view.ShownStars, Is.EqualTo(3));
            Assert.That(view.ShownCanGoNext, Is.True);
        }

        [Test]
        public void Opened_WonLevelButNoNextLevel_HidesTheNextLevelButton()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var flow = new FakeFlow(stack, PopupId.Win) { CanStartNextLevel = false };
            new EndPopupPresenter(view, stack, PopupId.Win, WonWithThreeStars(), flow).Enable();

            stack.Open(PopupId.Win);

            Assert.That(view.ShownCanGoNext, Is.False);
        }

        [Test]
        public void Opened_LostLevel_ShowsNoStarsAndNoNextLevel()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var flow = new FakeFlow(stack, PopupId.Lose) { CanStartNextLevel = true };
            new EndPopupPresenter(view, stack, PopupId.Lose, ProgressWithSomeScore(), flow).Enable();

            stack.Open(PopupId.Lose);

            Assert.That(view.ShownStars, Is.EqualTo(0));
            Assert.That(view.ShownCanGoNext, Is.False);
        }

        [Test]
        public void Opened_AnotherPopup_ShowsNothing()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            new EndPopupPresenter(view, stack, PopupId.Lose, ProgressWithSomeScore(), new FakeFlow(stack, PopupId.Lose)).Enable();

            stack.Open(PopupId.Pause);

            Assert.That(view.ShowCalls, Is.EqualTo(0));
        }

        [Test]
        public void RetryClicked_ClosesThePopupThenRestartsTheLevelOnce()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var flow = new FakeFlow(stack, PopupId.Win);
            new EndPopupPresenter(view, stack, PopupId.Win, WonWithThreeStars(), flow).Enable();
            stack.Open(PopupId.Win);

            view.PressRetry();

            Assert.That(stack.IsOpen(PopupId.Win), Is.False);
            Assert.That(flow.RestartCalls, Is.EqualTo(1));
            Assert.That(flow.PopupWasOpenAtCall, Is.False, "the popup must be closed before the restart");
        }

        [Test]
        public void NextLevelClicked_ClosesThePopupThenStartsTheNextLevelOnce()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var flow = new FakeFlow(stack, PopupId.Win) { CanStartNextLevel = true };
            new EndPopupPresenter(view, stack, PopupId.Win, WonWithThreeStars(), flow).Enable();
            stack.Open(PopupId.Win);

            view.PressNextLevel();

            Assert.That(stack.IsOpen(PopupId.Win), Is.False);
            Assert.That(flow.NextCalls, Is.EqualTo(1));
            Assert.That(flow.PopupWasOpenAtCall, Is.False, "the popup must be closed before the next level starts");
        }

        [Test]
        public void NextLevelClicked_WhenNoNextLevelCanStart_DoesNothing()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var flow = new FakeFlow(stack, PopupId.Win) { CanStartNextLevel = false };
            new EndPopupPresenter(view, stack, PopupId.Win, WonWithThreeStars(), flow).Enable();
            stack.Open(PopupId.Win);

            view.PressNextLevel();

            Assert.That(flow.NextCalls, Is.EqualTo(0));
            Assert.That(stack.IsOpen(PopupId.Win), Is.True);
        }

        [Test]
        public void Buttons_AfterDisable_DoNothing()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var flow = new FakeFlow(stack, PopupId.Win) { CanStartNextLevel = true };
            var presenter = new EndPopupPresenter(view, stack, PopupId.Win, WonWithThreeStars(), flow);
            presenter.Enable();
            presenter.Disable();

            view.PressRetry();
            view.PressNextLevel();

            Assert.That(flow.RestartCalls, Is.EqualTo(0));
            Assert.That(flow.NextCalls, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            var stack = new PopupStack();
            var view = new FakeEndView();
            var progress = ProgressWithSomeScore();
            var flow = new FakeFlow(stack, PopupId.Win);

            Assert.Throws<ArgumentNullException>(() => new EndPopupPresenter(null, stack, PopupId.Win, progress, flow));
            Assert.Throws<ArgumentNullException>(() => new EndPopupPresenter(view, null, PopupId.Win, progress, flow));
            Assert.Throws<ArgumentNullException>(() => new EndPopupPresenter(view, stack, PopupId.Win, null, flow));
            Assert.Throws<ArgumentNullException>(() => new EndPopupPresenter(view, stack, PopupId.Win, progress, null));
            Assert.Throws<ArgumentException>(() => new EndPopupPresenter(view, stack, PopupId.None, progress, flow));
        }
    }
}
