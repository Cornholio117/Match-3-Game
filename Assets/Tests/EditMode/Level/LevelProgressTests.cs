using System;
using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Core.Level;
using Game.Tests.EditMode.Board;
using NUnit.Framework;

namespace Game.Tests.EditMode.Level
{
    public sealed class LevelProgressTests
    {
        // A turn that cleared this many gems of one colour, followed by the refill that always comes with it.
        private static List<BoardStep> Turn(PieceColor color, int count)
        {
            var steps = new List<BoardStep>();
            for (var x = 0; x < count; x++)
                steps.Add(BoardStep.Clear(color, new GridPosition(x, 0)));

            for (var x = 0; x < count; x++)
                steps.Add(BoardStep.Spawn(PieceColor.Red, new GridPosition(x, 2)));

            return steps;
        }

        private static LevelProgress ScoreLevel(int goal, int moves)
        {
            return new LevelProgress(LevelGoalType.Score, goal, PieceColor.None, moves, new ScoreRules());
        }

        private static LevelProgress CollectLevel(PieceColor color, int amount, int moves)
        {
            return new LevelProgress(LevelGoalType.CollectColor, amount, color, moves, new ScoreRules());
        }

        [Test]
        public void ApplyTurn_AnyTurn_UsesOneMove()
        {
            var progress = ScoreLevel(1000, 5);

            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            Assert.That(progress.Moves.Remaining, Is.EqualTo(4));
        }

        [Test]
        public void ApplyTurn_ThreeGems_AddsThirtyToTheScore()
        {
            var progress = ScoreLevel(1000, 5);

            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            Assert.That(progress.Score, Is.EqualTo(30));
            Assert.That(progress.GoalProgress, Is.EqualTo(30));
        }

        [Test]
        public void ApplyTurn_ScoreGoalReached_WinsAndRaisesStateChanged()
        {
            var progress = ScoreLevel(30, 5);
            var seen = LevelState.InProgress;
            progress.StateChanged += state => seen = state;

            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            Assert.That(progress.State, Is.EqualTo(LevelState.Won));
            Assert.That(seen, Is.EqualTo(LevelState.Won));
        }

        [Test]
        public void ApplyTurn_CollectGoal_OnlyCountsGemsOfTheGoalColour()
        {
            var progress = CollectLevel(PieceColor.Green, 5, 10);
            var turn = Turn(PieceColor.Green, 3);
            turn.Add(BoardStep.Clear(PieceColor.Red, new GridPosition(5, 0)));
            turn.Add(BoardStep.Clear(PieceColor.Red, new GridPosition(6, 0)));

            progress.ApplyTurn(turn);

            Assert.That(progress.Collected, Is.EqualTo(3));
            Assert.That(progress.GoalProgress, Is.EqualTo(3));
            Assert.That(progress.State, Is.EqualTo(LevelState.InProgress));
        }

        [Test]
        public void ApplyTurn_CollectGoalReachedOverTwoTurns_Wins()
        {
            var progress = CollectLevel(PieceColor.Green, 5, 10);

            progress.ApplyTurn(Turn(PieceColor.Green, 3));
            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            Assert.That(progress.Collected, Is.EqualTo(6));
            Assert.That(progress.State, Is.EqualTo(LevelState.Won));
        }

        [Test]
        public void ApplyTurn_GoalReachedOnTheLastMove_WinsInsteadOfLosing()
        {
            var progress = ScoreLevel(30, 1);

            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            Assert.That(progress.Moves.Remaining, Is.EqualTo(0));
            Assert.That(progress.State, Is.EqualTo(LevelState.Won));
        }

        [Test]
        public void ApplyTurn_MovesRunOutBeforeTheGoal_Loses()
        {
            var progress = ScoreLevel(1000, 2);

            progress.ApplyTurn(Turn(PieceColor.Green, 3));
            Assert.That(progress.State, Is.EqualTo(LevelState.InProgress));
            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            Assert.That(progress.State, Is.EqualTo(LevelState.Lost));
        }

        [Test]
        public void ApplyTurn_AfterTheLevelEnded_IsIgnored()
        {
            var progress = ScoreLevel(30, 5);
            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            Assert.That(progress.Score, Is.EqualTo(30));
            Assert.That(progress.Moves.Remaining, Is.EqualTo(4));
            Assert.That(progress.State, Is.EqualTo(LevelState.Won));
        }

        [Test]
        public void ApplyTurn_Events_ReportTheNewScoreAndGoalProgress()
        {
            var progress = ScoreLevel(1000, 5);
            var score = -1;
            var goalProgress = -1;
            progress.ScoreChanged += value => score = value;
            progress.GoalProgressChanged += value => goalProgress = value;

            progress.ApplyTurn(Turn(PieceColor.Green, 3));

            Assert.That(score, Is.EqualTo(30));
            Assert.That(goalProgress, Is.EqualTo(30));
        }

        [Test]
        public void ApplyTurn_CollectGoalNothingOfTheColourCleared_RaisesNoGoalProgressEvent()
        {
            var progress = CollectLevel(PieceColor.Green, 5, 10);
            var raised = 0;
            progress.GoalProgressChanged += _ => raised++;

            progress.ApplyTurn(Turn(PieceColor.Red, 3));

            Assert.That(raised, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_GoalAmountNotPositive_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ScoreLevel(0, 5));
        }

        [Test]
        public void Constructor_CollectGoalWithNoColour_Throws()
        {
            Assert.Throws<ArgumentException>(() => CollectLevel(PieceColor.None, 5, 5));
        }

        [Test]
        public void Constructor_NullScoreRules_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new LevelProgress(LevelGoalType.Score, 10, PieceColor.None, 5, null));
        }

        // The real board feeds the real tracker: one swap makes a run of three greens on the bottom row.
        [Test]
        public void ApplyTurn_FedByARealBoardGameSwap_ScoresAndUsesOneMove()
        {
            var board = BoardBuilder.FromRows(
                "YRYR",
                "RYRY",
                "GGYG");
            var game = new BoardGame(board, 4, new SeededRandom(1), 1);
            var progress = ScoreLevel(100000, 20);
            var steps = new List<BoardStep>();

            var outcome = game.TrySwap(new GridPosition(2, 0), new GridPosition(3, 0), steps);
            progress.ApplyTurn(steps);

            Assert.That(outcome.Accepted, Is.True);
            Assert.That(progress.Score, Is.GreaterThanOrEqualTo(30));
            Assert.That(progress.Moves.Remaining, Is.EqualTo(19));
        }
    }
}
