using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Tests.EditMode.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class DiagonalGemTests
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

        private static int CountSteps(List<BoardStep> steps, BoardStepKind kind)
        {
            var count = 0;
            foreach (var step in steps)
            {
                if (step.Kind == kind)
                    count++;
            }

            return count;
        }

        // The prediction picture: a T of blues, a bar on the bottom row and a stem up column 2, crossing at (2,0).
        private static BoardModel TShape()
        {
            return BoardBuilder.FromRows(
                "YRBYR",
                "RYBRY",
                "YBBBR");
        }

        // The only run is three greens on row y = 2 (x 0..2); the diagonal gem will sit at (1,2), the middle one.
        private static BoardModel GreenRunOnTheMiddleRow()
        {
            return BoardBuilder.FromRows(
                "RYRBY",
                "YRBYR",
                "GGGYB",
                "BRYRY",
                "RYRBR");
        }

        // Two special gems side by side on row y = 2, as in the combo pictures.
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
        public void Resolve_TShape_CreatesTheDiagonalGemWhereThePlayerMoved()
        {
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(TShape(), new ScriptedRandom(0, 1, 0, 1), 5, steps,
                new GridPosition(3, 0), new GridPosition(3, 1));

            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            Assert.That(HasCreate(steps, new GridPosition(3, 0), PieceKind.Diagonal), Is.True);
        }

        [Test]
        public void Resolve_TShapeInACascade_CreatesTheDiagonalGemOnTheCrossingCell()
        {
            var steps = new List<BoardStep>();

            new TurnResolver(1).Resolve(TShape(), new ScriptedRandom(0, 1, 0, 1), 5, steps);

            Assert.That(HasCreate(steps, new GridPosition(2, 0), PieceKind.Diagonal), Is.True);
        }

        [Test]
        public void Resolve_TShape_ClearsTheOtherFourGemsAndMakesOnlyOneSpecialGem()
        {
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(TShape(), new ScriptedRandom(0, 1, 0, 1), 5, steps);

            Assert.That(result.CellsCleared, Is.EqualTo(4));
            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            Assert.That(CountSteps(steps, BoardStepKind.Create), Is.EqualTo(1));
        }

        [Test]
        public void Resolve_LShape_CreatesTheDiagonalGemOnTheCorner()
        {
            var board = BoardBuilder.FromRows(
                "BYRYR",
                "BRYRY",
                "BBBRY");
            var steps = new List<BoardStep>();

            new TurnResolver(1).Resolve(board, new ScriptedRandom(0, 1, 0, 1), 5, steps);

            Assert.That(HasCreate(steps, new GridPosition(0, 0), PieceKind.Diagonal), Is.True);
        }

        [Test]
        public void Resolve_PlusShape_CreatesTheDiagonalGemOnTheCrossingCell()
        {
            var board = BoardBuilder.FromRows(
                "RYRYR",
                "YRBRY",
                "RBBBR",
                "YRBRY",
                "RYRYR");
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(board, new ScriptedRandom(0, 1, 0, 1), 5, steps);

            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            Assert.That(HasCreate(steps, new GridPosition(2, 2), PieceKind.Diagonal), Is.True);
        }

        [Test]
        public void Resolve_FourRunCrossedByAnotherRun_MakesOnlyTheDiagonalGem()
        {
            // A horizontal run of 4 on the bottom row (x 0..3) crossed by a vertical run of 3 up column 1.
            var board = BoardBuilder.FromRows(
                "YBRYR",
                "RBYRY",
                "BBBBR");
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(board, new ScriptedRandom(0, 1, 0, 1), 5, steps);

            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            Assert.That(CountSteps(steps, BoardStepKind.Create), Is.EqualTo(1));
            Assert.That(HasCreate(steps, new GridPosition(1, 0), PieceKind.Diagonal), Is.True);
        }

        [Test]
        public void Resolve_DiagonalGemInARun_ClearsBothDiagonalsThroughItsCell()
        {
            var board = GreenRunOnTheMiddleRow();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Diagonal);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            // The X through (1,2) on a 5x5 board has 7 cells; the run adds its two other cells, (0,2) and (2,2).
            Assert.That(result.CellsCleared, Is.EqualTo(9));
            Assert.That(result.SpecialsFired, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_DiagonalGemReachesALineGem_TheChainReactionFiresIt()
        {
            var board = GreenRunOnTheMiddleRow();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Diagonal);
            board.SetPiece(new GridPosition(3, 4), PieceColor.Blue, PieceKind.LineHorizontal);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            // The X reaches (3,4); that line gem clears the rest of the top row: 4 more cells on top of the 9.
            Assert.That(result.SpecialsFired, Is.EqualTo(2));
            Assert.That(result.CellsCleared, Is.EqualTo(13));
        }

        [Test]
        public void Resolve_LineGemPlusDiagonalCombo_ClearsTheRowTheColumnAndBothDiagonals()
        {
            var board = SideBySidePicture();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.LineHorizontal);
            board.SetPiece(new GridPosition(2, 2), PieceColor.Green, PieceKind.Diagonal);

            var result = Combo(board);

            // Row (5) + the rest of the column (4) + the rest of both diagonals through (2,2) (8) = 17.
            Assert.That(result.CellsCleared, Is.EqualTo(17));
        }

        [Test]
        public void Resolve_TwoDiagonalGemsCombo_ClearsTheXThroughEachSwappedCell()
        {
            var board = SideBySidePicture();
            board.SetPiece(new GridPosition(1, 2), PieceColor.Green, PieceKind.Diagonal);
            board.SetPiece(new GridPosition(2, 2), PieceColor.Green, PieceKind.Diagonal);

            var result = Combo(board);

            // 7 cells for the X through (1,2) and 9 for the X through (2,2); the two Xs share no cell.
            Assert.That(result.CellsCleared, Is.EqualTo(16));
        }

        [Test]
        public void Resolve_ManySeedsWithATShape_AlwaysEndsWithAFullBoardAndNoRuns()
        {
            var resolver = new TurnResolver();

            for (var seed = 0; seed < 100; seed++)
            {
                var board = new BoardModel(8, 8);
                board.Fill(new SeededRandom(seed), 4);
                for (var x = 1; x <= 3; x++)
                    board.SetPiece(new GridPosition(x, 0), PieceColor.Blue);
                for (var y = 0; y <= 2; y++)
                    board.SetPiece(new GridPosition(2, y), PieceColor.Blue);

                var result = resolver.Resolve(board, new SeededRandom(seed + 300), 4, new List<BoardStep>(),
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
