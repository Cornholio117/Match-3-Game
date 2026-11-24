using System;
using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Level;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class LevelEndPresenterTests
    {
        private static List<BoardStep> ThreeGreenClears()
        {
            var steps = new List<BoardStep>();
            for (var x = 0; x < 3; x++)
                steps.Add(BoardStep.Clear(PieceColor.Green, new GridPosition(x, 0)));

            return steps;
        }

        private static LevelProgress Level(int goal, int moves)
        {
            return new LevelProgress(LevelGoalType.Score, goal, PieceColor.None, moves, new ScoreRules());
        }

        private static LevelEndPresenter Presenter(LevelProgress progress, PopupStack stack, PlayerProgress player = null, int levelIndex = 0)
        {
            return new LevelEndPresenter(progress, stack, player ?? new PlayerProgress(5), levelIndex);
        }

        [Test]
        public void StateChanged_Won_OpensTheWinPopup()
        {
            var progress = Level(30, 5);
            var stack = new PopupStack();
            Presenter(progress, stack).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(stack.Top, Is.EqualTo(PopupId.Win));
            Assert.That(stack.Count, Is.EqualTo(1));
        }

        [Test]
        public void StateChanged_Lost_OpensTheLosePopup()
        {
            var progress = Level(1000, 1);
            var stack = new PopupStack();
            Presenter(progress, stack).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(stack.Top, Is.EqualTo(PopupId.Lose));
        }

        [Test]
        public void ApplyTurn_LevelStillInProgress_OpensNothing()
        {
            var progress = Level(1000, 5);
            var stack = new PopupStack();
            Presenter(progress, stack).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(stack.Count, Is.EqualTo(0));
        }

        [Test]
        public void StateChanged_AfterDisable_OpensNothing()
        {
            var progress = Level(30, 5);
            var stack = new PopupStack();
            var presenter = Presenter(progress, stack);
            presenter.Enable();
            presenter.Disable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(stack.Count, Is.EqualTo(0));
        }

        [Test]
        public void StateChanged_Won_RecordsTheStarsForThatLevelAndUnlocksTheNext()
        {
            // 30 points against a goal of 30 is 1 star.
            var progress = Level(30, 5);
            var player = new PlayerProgress(5);
            Presenter(progress, new PopupStack(), player, levelIndex: 2).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(player.GetStars(2), Is.EqualTo(1));
            Assert.That(player.IsUnlocked(3), Is.True);
            Assert.That(player.GetStars(0), Is.EqualTo(0));
        }

        [Test]
        public void StateChanged_Lost_RecordsNothing()
        {
            var progress = Level(1000, 1);
            var player = new PlayerProgress(5);
            Presenter(progress, new PopupStack(), player).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(player.TotalStars, Is.EqualTo(0));
        }

        [Test]
        public void StateChanged_Won_RecordsTheStarsBeforeThePopupOpens()
        {
            // The win popup asks whether the next level is unlocked when it opens, so the result must already be saved.
            var progress = Level(30, 5);
            var stack = new PopupStack();
            var player = new PlayerProgress(5);
            var starsWhenOpened = -1;
            stack.Opened += _ => starsWhenOpened = player.GetStars(0);
            Presenter(progress, stack, player).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(starsWhenOpened, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new LevelEndPresenter(null, new PopupStack(), new PlayerProgress(5), 0));
            Assert.Throws<ArgumentNullException>(() => new LevelEndPresenter(Level(10, 5), null, new PlayerProgress(5), 0));
            Assert.Throws<ArgumentNullException>(() => new LevelEndPresenter(Level(10, 5), new PopupStack(), null, 0));
        }
    }
}
