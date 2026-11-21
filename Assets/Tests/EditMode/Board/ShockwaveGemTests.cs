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
        public void Resolve_FiveInARowCrossedByARun_StillMakesAShockwaveGemNotADiagonal()
        {
            // A T: five greens on the floor and three up column 2. A 5-run always wins over the diagonal rule.
            var board = BoardBuilder.FromRows(
                "RYGYR",
                "YRGRY",
                "GGGGG");
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, steps);

            // No swap, so the gem takes the first free cell of the five-run, (0,0).
            Assert.That(HasCreate(steps, new GridPosition(0, 0), PieceKind.Shockwave), Is.True);
            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            // Seven distinct cells are in the T; one of them became the shockwave gem.
            Assert.That(result.CellsCleared, Is.EqualTo(6));
        }

        // A 7x7 board with no run anywhere: each row is the colour pattern Y R B P shifted by two more than the row
        // below, so neighbours never match in a row or a column.
        private static BoardModel SevenBySeven()
        {
            const string pattern = "YRBPYRBPYRBP";
            var rows = new string[7];
            for (var i = 0; i < 7; i++)
            {
                var y = 6 - i;
                rows[i] = pattern.Substring((2 * y) % 4, 7);
            }

            return BoardBuilder.FromRows(rows);
        }

        // Puts three greens in a row starting at (startX, y) and a shockwave gem on the one at shockX.
        private static BoardModel SevenBySevenWithShockwave(int startX, int y, int shockX)
        {
            var board = SevenBySeven();
            for (var x = startX; x < startX + 3; x++)
                board.SetPiece(new GridPosition(x, y), PieceColor.Green);

            board.SetPiece(new GridPosition(shockX, y), PieceColor.Green, PieceKind.Shockwave);
            return board;
        }

        [Test]
        public void Resolve_ShockwaveGemInTheMiddle_ClearsTheFiveByFiveSquareAroundIt()
        {
            var board = SevenBySevenWithShockwave(2, 3, 3);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            Assert.That(result.CellsCleared, Is.EqualTo(25));
            Assert.That(result.SpecialsFired, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_ShockwaveGemAtTheSideEdge_StillClearsAFullFiveByFive()
        {
            // The gem is on column 0, so a centred square would lose two columns; the square slides in instead.
            var board = SevenBySevenWithShockwave(0, 3, 0);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            Assert.That(result.CellsCleared, Is.EqualTo(25));
        }

        [Test]
        public void Resolve_ShockwaveGemInTheCorner_StillClearsAFullFiveByFive()
        {
            var board = SevenBySevenWithShockwave(0, 0, 0);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            Assert.That(result.CellsCleared, Is.EqualTo(25));
        }

        [Test]
        public void Resolve_ShockwaveGemAtTheEdge_ClearsTheSlidSquareNotTheCentredOne()
        {
            // The gem at (0,3) slides its square to columns 0..4, so a gem there is cleared and one at column 5 is not.
            var board = SevenBySevenWithShockwave(0, 3, 0);
            var steps = new List<BoardStep>();

            new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, steps);

            var clearedColumnFour = false;
            var clearedColumnFive = false;
            foreach (var step in steps)
            {
                if (step.Kind != BoardStepKind.Clear)
                    continue;

                clearedColumnFour |= step.From.X == 4;
                clearedColumnFive |= step.From.X == 5;
            }

            Assert.That(clearedColumnFour, Is.True);
            Assert.That(clearedColumnFive, Is.False);
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

            // On this small board the 5x5 square is the whole board (25 cells), which already holds the line gem's row,
            // so the line gem fires (2 gems fired) without adding any new cell.
            Assert.That(result.SpecialsFired, Is.EqualTo(2));
            Assert.That(result.CellsCleared, Is.EqualTo(25));
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
