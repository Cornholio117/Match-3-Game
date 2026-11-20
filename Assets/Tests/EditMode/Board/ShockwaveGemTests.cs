using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Tests.EditMode.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class ShockwaveGemTests
    {
        private static bool HasCreate(List<BoardStep> steps, GridPosition cell, PieceKind kind)
        {
            foreach (var step in steps)
            {
                if (step.Kind == BoardStepKind.Create && step.From == cell && step.SpecialKind == kind)
                    return true;
            }

            return false;
        }

        // Five greens on the floor and nothing else that matches.
        private static BoardModel FiveInARow()
        {
            return BoardBuilder.FromRows(
                "RYRYR",
                "YRYRY",
                "GGGGG");
        }

        // The only run is three greens on row y = 2 (x 0..2); the shockwave gem will sit inside it.
        private static BoardModel GreenRunOnTheMiddleRow()
        {
            return BoardBuilder.FromRows(
                "RYRBY",
                "YRBYR",
                "GGGYB",
                "BRYRY",
                "RYRBR");
        }

        // Two gems side by side on row y = 2 for the combo pictures; no run anywhere.
        private static BoardModel SideBySidePicture()
        {
            return BoardBuilder.FromRows(
                "RYRBY",
                "BRYRB",
                "YGGYR",
                "RBYBY",
                "YRGYB");
        }

        private static TurnResult Combo(BoardModel board)
        {
            return new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>(),
                new GridPosition(1, 2), new GridPosition(2, 2), combo: true);
        }

        [Test]
        public void Resolve_FiveInARow_CreatesAShockwaveGemWhereThePlayerMoved()
        {
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(FiveInARow(), new SeededRandom(1), 5, steps,
                new GridPosition(3, 0), new GridPosition(3, 1));

            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            Assert.That(HasCreate(steps, new GridPosition(3, 0), PieceKind.Shockwave), Is.True);
        }

        [Test]
        public void Resolve_FiveInARow_MakesOnlyTheShockwaveGemNotALineGem()
        {
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(FiveInARow(), new SeededRandom(1), 5, steps);

            Assert.That(result.CellsCleared, Is.EqualTo(4));
            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_FiveInAColumn_CreatesAShockwaveGem()
        {
            var board = BoardBuilder.FromRows(
                "GYRYR",
                "GRYRY",
                "GYRYR",
                "GRYRY",
                "GYRYR");
            var steps = new List<BoardStep>();

            new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, steps);

            Assert.That(HasCreate(steps, new GridPosition(0, 0), PieceKind.Shockwave), Is.True);
        }

        [Test]
        public void Resolve_FiveInARowCrossedByARun_MakesTheDiagonalGemInstead()
        {
            // A T: five greens on the floor and three up column 2. The diagonal rule has priority.
            var board = BoardBuilder.FromRows(
                "RYGYR",
                "YRGRY",
                "GGGGG");
            var steps = new List<BoardStep>();

            new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, steps);

            Assert.That(HasCreate(steps, new GridPosition(2, 0), PieceKind.Diagonal), Is.True);
            Assert.That(HasCreate(steps, new GridPosition(2, 0), PieceKind.Shockwave), Is.False);
        }

        [Test]
        public void Resolve_ShockwaveGemInARun_ClearsTheFiveByFiveSquareAroundIt()
        {
            var board = GreenRunOnTheMiddleRow();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Shockwave);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            // The square around (1,2) reaches x -1..3, so columns 0..3 (4 x 5 cells); column 4 is untouched.
            Assert.That(result.CellsCleared, Is.EqualTo(20));
            Assert.That(result.SpecialsFired, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_ShockwaveGemAtTheBoardEdge_OnlyClearsCellsInsideTheBoard()
        {
            var board = GreenRunOnTheMiddleRow();
            board.SetPiece(new GridPosition(0, 2), PieceColor.Green, PieceKind.Shockwave);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            // x -2..2 inside the board is columns 0..2 (3 x 5 cells).
            Assert.That(result.CellsCleared, Is.EqualTo(15));
        }

        [Test]
        public void Resolve_RadiusOne_ClearsAThreeByThreeSquare()
        {
            var board = GreenRunOnTheMiddleRow();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Shockwave);

            var result = new TurnResolver(1, 1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            // x 0..2, y 1..3 is 9 cells and already contains the whole run.
            Assert.That(result.CellsCleared, Is.EqualTo(9));
        }

        [Test]
        public void Resolve_ShockwaveReachesALineGem_TheChainReactionFiresIt()
        {
            var board = GreenRunOnTheMiddleRow();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Shockwave);
            board.SetPiece(new GridPosition(3, 4), PieceColor.Blue, PieceKind.LineHorizontal);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            // The line gem sits inside the square and clears the rest of its row: only (4,4) is new, so 21.
            Assert.That(result.SpecialsFired, Is.EqualTo(2));
            Assert.That(result.CellsCleared, Is.EqualTo(21));
        }

        [Test]
        public void Resolve_TwoShockwaveGemsCombo_ClearsTheWholeBoard()
        {
            var board = SideBySidePicture();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Shockwave);
            board.SetPiece(new GridPosition(2, 2), PieceColor.Green, PieceKind.Shockwave);

            var result = Combo(board);

            Assert.That(result.CellsCleared, Is.EqualTo(25));
        }

        [Test]
        public void Resolve_ShockwavePlusLineCombo_ClearsAThreeWideRowAndColumn()
        {
            var board = SideBySidePicture();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.LineHorizontal);
            board.SetPiece(new GridPosition(2, 2), PieceColor.Green, PieceKind.Shockwave);

            var result = Combo(board);

            // Rows 1..3 (15 cells) plus columns 1..3 (15 cells), minus the 9 they share = 21.
            Assert.That(result.CellsCleared, Is.EqualTo(21));
        }

        [Test]
        public void Resolve_ShockwavePlusDiagonalCombo_ClearsThreeWideDiagonals()
        {
            var board = SideBySidePicture();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Diagonal);
            board.SetPiece(new GridPosition(2, 2), PieceColor.Green, PieceKind.Shockwave);

            var result = Combo(board);

            // Each band has 13 cells on a 5x5 board and they share 5 near the middle: 13 + 13 - 5 = 21.
            Assert.That(result.CellsCleared, Is.EqualTo(21));
        }

        [Test]
        public void TrySwap_ShockwaveAndNormalLineGemWithNoMatch_IsAcceptedAsACombo()
        {
            var board = SideBySidePicture();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Shockwave);
            board.SetPiece(new GridPosition(2, 2), PieceColor.Blue, PieceKind.LineVertical);
            var game = new BoardGame(board, 5, new SeededRandom(4), 1);

            var outcome = game.TrySwap(new GridPosition(1, 2), new GridPosition(2, 2), new List<BoardStep>());

            Assert.That(outcome.Accepted, Is.True);
            Assert.That(outcome.Turn.CellsCleared, Is.EqualTo(21));
        }

        [Test]
        public void Constructor_RadiusBelowOne_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new TurnResolver(1, 0));
        }

        [Test]
        public void Resolve_ManySeedsWithFiveInARow_AlwaysEndsWithAFullBoardAndNoRuns()
        {
            var resolver = new TurnResolver();

            for (var seed = 0; seed < 100; seed++)
            {
                var board = new BoardModel(8, 8);
                board.Fill(new SeededRandom(seed), 4);
                for (var x = 1; x <= 5; x++)
                    board.SetPiece(new GridPosition(x, 0), PieceColor.Blue);

                var result = resolver.Resolve(board, new SeededRandom(seed + 500), 4, new List<BoardStep>(),
                    new GridPosition(3, 0), new GridPosition(3, 1));

                var matches = new List<Match>();
                MatchFinder.Find(board, matches);
                Assert.That(matches.Count > 0 && !result.HitLimit, Is.False, $"seed {seed}");
                foreach (var row in BoardBuilder.ToRows(board))
                    Assert.That(row, Does.Not.Contain("."), $"seed {seed}");
            }
        }
    }
}
