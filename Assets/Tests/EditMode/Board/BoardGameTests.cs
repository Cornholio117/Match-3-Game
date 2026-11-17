using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Tests.EditMode.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class BoardGameTests
    {
        // Swapping (2,0) with (2,1) completes the bottom row of greens.
        private static BoardModel OneMoveFromARun()
        {
            return BoardBuilder.FromRows(
                "YBG",
                "GGR");
        }

        [Test]
        public void CreateFilled_NewGame_BoardIsFullWithNoRuns()
        {
            var game = BoardGame.CreateFilled(8, 8, 5, new SeededRandom(3));
            var matches = new List<Match>();

            MatchFinder.Find(game.Board, matches);

            Assert.That(matches, Is.Empty);
            foreach (var row in BoardBuilder.ToRows(game.Board))
                Assert.That(row, Does.Not.Contain("."));
        }

        [Test]
        public void CreateFilled_SameSeed_SameBoard()
        {
            var first = BoardGame.CreateFilled(8, 8, 5, new SeededRandom(21));
            var second = BoardGame.CreateFilled(8, 8, 5, new SeededRandom(21));

            Assert.That(BoardBuilder.ToRows(first.Board), Is.EqualTo(BoardBuilder.ToRows(second.Board)));
        }

        [Test]
        public void TrySwap_ValidSwap_IsAcceptedAndResolved()
        {
            var game = new BoardGame(OneMoveFromARun(), 4, new ScriptedRandom(0, 1, 0));
            var steps = new List<BoardStep>();

            var outcome = game.TrySwap(new GridPosition(2, 0), new GridPosition(2, 1), steps);

            Assert.That(outcome.Accepted, Is.True);
            Assert.That(outcome.Turn.Cascades, Is.EqualTo(1));
            Assert.That(outcome.Turn.CellsCleared, Is.EqualTo(3));
            Assert.That(steps.Count, Is.EqualTo(9));
            Assert.That(BoardBuilder.ToRows(game.Board), Is.EqualTo(new[] { "GBG", "YBR" }));
        }

        [Test]
        public void TrySwap_SwapMakesNoRun_IsRejectedAndNothingChanges()
        {
            var game = new BoardGame(OneMoveFromARun(), 4, new SeededRandom(1));
            var steps = new List<BoardStep>();

            var outcome = game.TrySwap(new GridPosition(0, 1), new GridPosition(1, 1), steps);

            Assert.That(outcome.Accepted, Is.False);
            Assert.That(steps, Is.Empty);
            Assert.That(BoardBuilder.ToRows(game.Board), Is.EqualTo(new[] { "YBG", "GGR" }));
        }

        [Test]
        public void TrySwap_CellsNotNeighbours_IsRejected()
        {
            var game = new BoardGame(OneMoveFromARun(), 4, new SeededRandom(1));

            var outcome = game.TrySwap(new GridPosition(0, 0), new GridPosition(2, 0), new List<BoardStep>());

            Assert.That(outcome.Accepted, Is.False);
        }

        [Test]
        public void TrySwap_PreviousStepsInTheList_AreClearedFirst()
        {
            var game = new BoardGame(OneMoveFromARun(), 4, new SeededRandom(1));
            var steps = new List<BoardStep> { BoardStep.Spawn(PieceColor.Red, new GridPosition(0, 0)) };

            game.TrySwap(new GridPosition(0, 1), new GridPosition(1, 1), steps);

            Assert.That(steps, Is.Empty);
        }

        [Test]
        public void Constructor_ColourCountOutOfRange_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new BoardGame(OneMoveFromARun(), 2, new SeededRandom(1)));
        }
    }
}
