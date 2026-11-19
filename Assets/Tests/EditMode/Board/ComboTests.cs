using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Tests.EditMode.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class ComboTests
    {
        // The prediction picture, after the player swipes from (1,2) to (2,2): two line gems side by side on row y = 2.
        private static BoardModel PredictionPicture()
        {
            var board = BoardBuilder.FromRows(
                "RYRBY",
                "BRYRB",
                "YGGYR",
                "RBYBY",
                "YRGYB");
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.LineHorizontal);
            board.SetPiece(new GridPosition(2, 2), PieceColor.Green, PieceKind.LineHorizontal);
            return board;
        }

        private static TurnResult ResolveCombo(BoardModel board, List<BoardStep> steps, int maxCascades = 1)
        {
            return new TurnResolver(maxCascades).Resolve(board, new SeededRandom(1), 5, steps,
                new GridPosition(1, 2), new GridPosition(2, 2), combo: true);
        }

        [Test]
        public void Resolve_ComboOfTwoLineGems_ClearsTheRowAndTheColumnThroughTheMovedCell()
        {
            var board = PredictionPicture();

            var result = ResolveCombo(board, new List<BoardStep>());

            // Row y = 2 (5 cells) plus the rest of column x = 2 (4 cells), the crossing counted once.
            Assert.That(result.CellsCleared, Is.EqualTo(9));
            Assert.That(result.SpecialsFired, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_ComboWithAThirdLineGemInTheCross_ChainReactionClearsThirteen()
        {
            var board = PredictionPicture();
            board.SetPiece(new GridPosition(2, 0), PieceColor.Green, PieceKind.LineHorizontal);

            var result = ResolveCombo(board, new List<BoardStep>());

            // The 9 cells of the cross, plus the other 4 cells of row y = 0, fired by the third line gem.
            Assert.That(result.CellsCleared, Is.EqualTo(13));
            Assert.That(result.SpecialsFired, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_ComboGems_DoNotFireTheirOwnLines()
        {
            var board = PredictionPicture();

            // If the vertical gem at (1,2) fired its own column, column x = 1 would add 4 more cells.
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.LineVertical);

            var result = ResolveCombo(board, new List<BoardStep>());

            Assert.That(result.CellsCleared, Is.EqualTo(9));
        }

        [Test]
        public void Resolve_ComboWithoutMatchFlag_ClearsNothingWhenThereAreNoRuns()
        {
            var board = PredictionPicture();

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>(),
                new GridPosition(1, 2), new GridPosition(2, 2));

            Assert.That(result.CellsCleared, Is.EqualTo(0));
        }

        [Test]
        public void TrySwap_TwoSpecialGems_IsAcceptedAndResolvedAsACombo()
        {
            var board = PredictionPicture();
            var game = new BoardGame(board, 5, new SeededRandom(4));
            var steps = new List<BoardStep>();

            var outcome = game.TrySwap(new GridPosition(1, 2), new GridPosition(2, 2), steps);

            Assert.That(outcome.Accepted, Is.True);
            Assert.That(outcome.Turn.CellsCleared, Is.GreaterThanOrEqualTo(9));
            Assert.That(outcome.Turn.SpecialsFired, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void TrySwap_AfterACombo_TheBoardIsFullWithNoRunsLeft()
        {
            var game = new BoardGame(PredictionPicture(), 5, new SeededRandom(4));

            game.TrySwap(new GridPosition(1, 2), new GridPosition(2, 2), new List<BoardStep>());

            var matches = new List<Match>();
            MatchFinder.Find(game.Board, matches);
            Assert.That(matches, Is.Empty);
            foreach (var row in BoardBuilder.ToRows(game.Board))
                Assert.That(row, Does.Not.Contain("."));
        }

        [Test]
        public void TrySwap_ManySeeds_ComboAlwaysEndsWithAFullBoardAndNoRuns()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var board = new BoardModel(8, 8);
                board.Fill(new SeededRandom(seed), 4);
                board.SetPiece(new GridPosition(3, 3), PieceColor.Red, PieceKind.LineHorizontal);
                board.SetPiece(new GridPosition(4, 3), PieceColor.Blue, PieceKind.LineVertical);
                var game = new BoardGame(board, 4, new SeededRandom(seed + 900));

                var outcome = game.TrySwap(new GridPosition(3, 3), new GridPosition(4, 3), new List<BoardStep>());

                var matches = new List<Match>();
                MatchFinder.Find(board, matches);
                Assert.That(outcome.Accepted, Is.True, $"seed {seed}");
                Assert.That(matches.Count > 0 && !outcome.Turn.HitLimit, Is.False, $"seed {seed}");
                foreach (var row in BoardBuilder.ToRows(board))
                    Assert.That(row, Does.Not.Contain("."), $"seed {seed}");
            }
        }
    }
}
