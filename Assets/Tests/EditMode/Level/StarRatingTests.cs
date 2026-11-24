using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Level;
using NUnit.Framework;

namespace Game.Tests.EditMode.Level
{
    public sealed class StarRatingTests
    {
        [TestCase(2999, 0)]
        [TestCase(3000, 1)]
        [TestCase(4499, 1)]
        [TestCase(4500, 2)]
        [TestCase(5999, 2)]
        [TestCase(6000, 3)]
        [TestCase(20000, 3)]
        public void Calculate_ScoreGoal_StarsFollowOneAndAHalfAndTwoTimesTheGoal(int score, int expectedStars)
        {
            var stars = StarRating.Calculate(LevelGoalType.Score, 3000, score, 0, 12);

            Assert.That(stars, Is.EqualTo(expectedStars));
        }

        [TestCase(49, 12, 0)]
        [TestCase(50, 0, 1)]
        [TestCase(50, 2, 1)]
        [TestCase(50, 3, 2)]
        [TestCase(50, 5, 2)]
        [TestCase(50, 6, 3)]
        [TestCase(50, 12, 3)]
        public void Calculate_CollectGoal_StarsFollowTheMovesLeft(int collected, int movesLeft, int expectedStars)
        {
            // 12 moves to start with: 25% left is 3 moves, 50% left is 6 moves.
            var stars = StarRating.Calculate(LevelGoalType.CollectColor, 50, collected, movesLeft, 12);

            Assert.That(stars, Is.EqualTo(expectedStars));
        }

        [Test]
        public void Calculate_FromAWonScoreLevel_UsesTheScore()
        {
            // Three greens cleared in one turn are worth 30 points: exactly 2x a goal of 15.
            var progress = new LevelProgress(LevelGoalType.Score, 15, PieceColor.None, 5, new ScoreRules());
            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(StarRating.Calculate(progress), Is.EqualTo(3));
        }

        [Test]
        public void Calculate_FromAWonCollectLevel_UsesTheMovesLeft()
        {
            var progress = new LevelProgress(LevelGoalType.CollectColor, 3, PieceColor.Green, 10, new ScoreRules());
            progress.ApplyTurn(ThreeGreenClears());

            // 9 of 10 moves are left.
            Assert.That(StarRating.Calculate(progress), Is.EqualTo(3));
        }

        [Test]
        public void Calculate_FromALevelStillInProgress_ReturnsZero()
        {
            var progress = new LevelProgress(LevelGoalType.Score, 1000, PieceColor.None, 5, new ScoreRules());
            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(StarRating.Calculate(progress), Is.EqualTo(0));
        }

        [Test]
        public void Calculate_FromALostLevel_ReturnsZero()
        {
            var progress = new LevelProgress(LevelGoalType.Score, 1000, PieceColor.None, 1, new ScoreRules());
            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(progress.State, Is.EqualTo(LevelState.Lost));
            Assert.That(StarRating.Calculate(progress), Is.EqualTo(0));
        }

        [Test]
        public void StartingMoves_NewProgress_IsTheMovesTheLevelBeganWith()
        {
            var progress = new LevelProgress(LevelGoalType.Score, 1000, PieceColor.None, 12, new ScoreRules());
            progress.ApplyTurn(ThreeGreenClears());

            Assert.That(progress.StartingMoves, Is.EqualTo(12));
            Assert.That(progress.Moves.Remaining, Is.EqualTo(11));
        }

        private static List<BoardStep> ThreeGreenClears()
        {
            var steps = new List<BoardStep>();
            for (var x = 0; x < 3; x++)
                steps.Add(BoardStep.Clear(PieceColor.Green, new GridPosition(x, 0)));

            return steps;
        }
    }
}
