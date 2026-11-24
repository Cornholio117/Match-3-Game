using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class MoveFinderTests
    {
        [Test]
        public void FindValidSwaps_BoardWithOneMove_ReturnsExactlyThatSwap()
        {
            // Swapping the last two gems makes R R R B; no other swap makes a run.
            var board = BoardBuilder.FromRows("RRBR");
            var moves = new List<SwapMove>();

            MoveFinder.FindValidSwaps(board, moves);

            Assert.That(moves.Count, Is.EqualTo(1));
            Assert.That(moves[0].A, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(moves[0].B, Is.EqualTo(new GridPosition(3, 0)));
        }

        [Test]
        public void FindValidSwaps_NoMovesPossible_ReturnsAnEmptyList()
        {
            var board = BoardBuilder.FromRows("RBG");
            var moves = new List<SwapMove>();

            MoveFinder.FindValidSwaps(board, moves);

            Assert.That(moves, Is.Empty);
        }

        [Test]
        public void FindValidSwaps_ListAlreadyHasItems_ClearsItFirst()
        {
            var board = BoardBuilder.FromRows("RBG");
            var moves = new List<SwapMove> { new SwapMove(new GridPosition(0, 0), new GridPosition(1, 0)) };

            MoveFinder.FindValidSwaps(board, moves);

            Assert.That(moves, Is.Empty);
        }

        [Test]
        public void FindValidSwaps_AnyBoard_LeavesTheBoardUnchanged()
        {
            var board = BoardBuilder.FromRows(
                "YRYR",
                "RYRY",
                "GGYG");
            var before = BoardBuilder.ToRows(board);

            MoveFinder.FindValidSwaps(board, new List<SwapMove>());

            Assert.That(BoardBuilder.ToRows(board), Is.EqualTo(before));
        }

        [Test]
        public void FindValidSwaps_TwoSpecialGemsNextToEachOther_CountsAsAMove()
        {
            // Two special gems can always be swapped (that is the combo), even with no run.
            var board = BoardBuilder.FromRows("RB");
            board.SetPiece(new GridPosition(0, 0), PieceColor.Red, PieceKind.LineHorizontal);
            board.SetPiece(new GridPosition(1, 0), PieceColor.Blue, PieceKind.Diagonal);
            var moves = new List<SwapMove>();

            MoveFinder.FindValidSwaps(board, moves);

            Assert.That(moves.Count, Is.EqualTo(1));
        }

        [Test]
        public void HasValidSwap_ReflectsWhetherAMoveExists()
        {
            Assert.That(MoveFinder.HasValidSwap(BoardBuilder.FromRows("RRBR")), Is.True);
            Assert.That(MoveFinder.HasValidSwap(BoardBuilder.FromRows("RBG")), Is.False);
        }

        // Cross-check against a plain brute-force loop on real random boards: same count, and every result is valid.
        [Test]
        public void FindValidSwaps_RandomBoards_MatchesABruteForceCount()
        {
            var moves = new List<SwapMove>();

            for (var seed = 0; seed < 20; seed++)
            {
                var board = new BoardModel(8, 8);
                board.Fill(new SeededRandom(seed), 5);

                MoveFinder.FindValidSwaps(board, moves);

                var expected = 0;
                for (var y = 0; y < 8; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        var a = new GridPosition(x, y);
                        if (SwapRules.IsValidSwap(board, a, new GridPosition(x + 1, y)))
                            expected++;

                        if (SwapRules.IsValidSwap(board, a, new GridPosition(x, y + 1)))
                            expected++;
                    }
                }

                Assert.That(moves.Count, Is.EqualTo(expected), $"seed {seed}");
                foreach (var move in moves)
                    Assert.That(SwapRules.IsValidSwap(board, move.A, move.B), Is.True, $"seed {seed}");
            }
        }
    }
}
