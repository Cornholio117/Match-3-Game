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

        [Test]
        public void StateChanged_Won_OpensTheWinPopup()
        {
            var progress = Level(30, 5);
            var stack = new PopupStack();
            new LevelEndPresenter(progress, stack).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(stack.Top, Is.EqualTo(PopupId.Win));
            Assert.That(stack.Count, Is.EqualTo(1));
        }

        [Test]
        public void StateChanged_Lost_OpensTheLosePopup()
        {
            var progress = Level(1000, 1);
            var stack = new PopupStack();
            new LevelEndPresenter(progress, stack).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(stack.Top, Is.EqualTo(PopupId.Lose));
        }

        [Test]
        public void ApplyTurn_LevelStillInProgress_OpensNothing()
        {
            var progress = Level(1000, 5);
            var stack = new PopupStack();
            new LevelEndPresenter(progress, stack).Enable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(stack.Count, Is.EqualTo(0));
        }

        [Test]
        public void StateChanged_AfterDisable_OpensNothing()
        {
            var progress = Level(30, 5);
            var stack = new PopupStack();
            var presenter = new LevelEndPresenter(progress, stack);
            presenter.Enable();
            presenter.Disable();

            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(stack.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new LevelEndPresenter(null, new PopupStack()));
            Assert.Throws<ArgumentNullException>(() => new LevelEndPresenter(Level(10, 5), null));
        }
    }
}
