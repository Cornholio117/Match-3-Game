using System;
using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Level;
using Game.Core.UI;
using NUnit.Framework;

namespace Game.Tests.EditMode.UI
{
    public sealed class HudPresenterTests
    {
        // A stand-in for the real HUD: it remembers the last values it was told to show.
        private sealed class FakeHudView : IHudView
        {
            public int Moves = -1;
            public int Score = -1;
            public LevelGoalType GoalType = LevelGoalType.Score;
            public PieceColor GoalColor = PieceColor.None;
            public int GoalCurrent = -1;
            public int GoalTarget = -1;
            public int GoalCalls;

            public void ShowMoves(int remaining)
            {
                Moves = remaining;
            }

            public void ShowScore(int score)
            {
                Score = score;
            }

            public void ShowGoal(LevelGoalType type, PieceColor color, int current, int target)
            {
                GoalType = type;
                GoalColor = color;
                GoalCurrent = current;
                GoalTarget = target;
                GoalCalls++;
            }
        }

        private static List<BoardStep> ClearsOf(PieceColor color, int count)
        {
            var steps = new List<BoardStep>();
            for (var x = 0; x < count; x++)
                steps.Add(BoardStep.Clear(color, new GridPosition(x, 0)));

            for (var x = 0; x < count; x++)
                steps.Add(BoardStep.Spawn(PieceColor.Red, new GridPosition(x, 2)));

            return steps;
        }

        private static LevelProgress ScoreLevel()
        {
            return new LevelProgress(LevelGoalType.Score, 1000, PieceColor.None, 20, new ScoreRules());
        }

        [Test]
        public void Enable_ShowsTheStartingValues()
        {
            var view = new FakeHudView();

            new HudPresenter(ScoreLevel(), view).Enable();

            Assert.That(view.Moves, Is.EqualTo(20));
            Assert.That(view.Score, Is.EqualTo(0));
            Assert.That(view.GoalType, Is.EqualTo(LevelGoalType.Score));
            Assert.That(view.GoalCurrent, Is.EqualTo(0));
            Assert.That(view.GoalTarget, Is.EqualTo(1000));
        }

        [Test]
        public void ApplyTurn_AfterEnable_UpdatesMovesScoreAndGoal()
        {
            var progress = ScoreLevel();
            var view = new FakeHudView();
            new HudPresenter(progress, view).Enable();

            progress.ApplyTurn(ClearsOf(PieceColor.Green, 3));

            Assert.That(view.Moves, Is.EqualTo(19));
            Assert.That(view.Score, Is.EqualTo(30));
            Assert.That(view.GoalCurrent, Is.EqualTo(30));
        }

        [Test]
        public void ApplyTurn_CollectGoal_ShowsTheColourAndTheCollectedCount()
        {
            var progress = new LevelProgress(LevelGoalType.CollectColor, 20, PieceColor.Green, 18, new ScoreRules());
            var view = new FakeHudView();
            new HudPresenter(progress, view).Enable();

            progress.ApplyTurn(ClearsOf(PieceColor.Green, 3));

            Assert.That(view.GoalType, Is.EqualTo(LevelGoalType.CollectColor));
            Assert.That(view.GoalColor, Is.EqualTo(PieceColor.Green));
            Assert.That(view.GoalCurrent, Is.EqualTo(3));
            Assert.That(view.GoalTarget, Is.EqualTo(20));
        }

        [Test]
        public void ApplyTurn_AfterDisable_ChangesNothingOnTheView()
        {
            var progress = ScoreLevel();
            var view = new FakeHudView();
            var presenter = new HudPresenter(progress, view);
            presenter.Enable();
            presenter.Disable();

            progress.ApplyTurn(ClearsOf(PieceColor.Green, 3));

            Assert.That(view.Moves, Is.EqualTo(20));
            Assert.That(view.Score, Is.EqualTo(0));
        }

        [Test]
        public void Enable_CalledTwice_StillUpdatesEachChangeOnce()
        {
            var progress = ScoreLevel();
            var view = new FakeHudView();
            var presenter = new HudPresenter(progress, view);
            presenter.Enable();
            presenter.Enable();
            var callsBefore = view.GoalCalls;

            progress.ApplyTurn(ClearsOf(PieceColor.Green, 3));

            Assert.That(view.GoalCalls - callsBefore, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new HudPresenter(null, new FakeHudView()));
            Assert.Throws<ArgumentNullException>(() => new HudPresenter(ScoreLevel(), null));
        }
    }
}
