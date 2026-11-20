using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Tests.EditMode.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class LineGemTests
    {
        private static bool HasStep(List<BoardStep> steps, BoardStepKind kind, GridPosition cell, PieceKind specialKind)
        {
            foreach (var step in steps)
            {
                if (step.Kind == kind && step.From == cell && step.SpecialKind == specialKind)
                    return true;
            }

            return false;
        }

        private static int Count(List<BoardStep> steps, BoardStepKind kind)
        {
            var count = 0;
            foreach (var step in steps)
            {
                if (step.Kind == kind)
                    count++;
            }

            return count;
        }

        private static bool HasAnyRun(BoardModel board)
        {
            var matches = new List<Match>();
            MatchFinder.Find(board, matches);
            return matches.Count > 0;
        }

        // The board after the player's swap: four blues in a row on the bottom row.
        private static BoardModel FourBlueRow()
        {
            return BoardBuilder.FromRows(
                "RYRYR",
                "BBBBY");
        }

        // The prediction picture: one run of three greens on row y = 1.
        private static BoardModel PredictionPicture()
        {
            return BoardBuilder.FromRows(
                "BYRBB",
                "RBYRY",
                "YRGGG",
                "BYRYB");
        }

        [Test]
        public void Resolve_FourInARow_CreatesAHorizontalLineGemWhereThePlayerMoved()
        {
            var board = FourBlueRow();
            var steps = new List<BoardStep>();

            var result = new TurnResolver().Resolve(board, new ScriptedRandom(0, 1, 0), 5, steps,
                new GridPosition(2, 0), new GridPosition(2, 1));

            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            Assert.That(HasStep(steps, BoardStepKind.Create, new GridPosition(2, 0), PieceKind.LineHorizontal), Is.True);
            Assert.That(board.GetKind(new GridPosition(2, 0)), Is.EqualTo(PieceKind.LineHorizontal));
            Assert.That(board.GetPiece(new GridPosition(2, 0)), Is.EqualTo(PieceColor.Blue));
        }

        [Test]
        public void Resolve_FourInARow_ClearsOnlyTheOtherThree()
        {
            var board = FourBlueRow();
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(board, new ScriptedRandom(0, 1, 0), 5, steps,
                new GridPosition(2, 0), new GridPosition(2, 1));

            Assert.That(result.CellsCleared, Is.EqualTo(3));
            Assert.That(Count(steps, BoardStepKind.Clear), Is.EqualTo(3));
        }

        [Test]
        public void Resolve_FourInAColumn_CreatesAVerticalLineGem()
        {
            var board = BoardBuilder.FromRows(
                "RYR",
                "BGY",
                "YGR",
                "RGY",
                "YGR");
            var steps = new List<BoardStep>();

            new TurnResolver(1).Resolve(board, new ScriptedRandom(0, 1, 0), 5, steps,
                new GridPosition(1, 1), new GridPosition(0, 1));

            Assert.That(HasStep(steps, BoardStepKind.Create, new GridPosition(1, 1), PieceKind.LineVertical), Is.True);
        }

        [Test]
        public void Resolve_CascadeRunWithNoSwap_CreatesTheGemInTheLowerLeftCell()
        {
            var board = FourBlueRow();
            var steps = new List<BoardStep>();

            new TurnResolver(1).Resolve(board, new ScriptedRandom(0, 1, 0), 5, steps);

            Assert.That(HasStep(steps, BoardStepKind.Create, new GridPosition(0, 0), PieceKind.LineHorizontal), Is.True);
        }

        [Test]
        public void Resolve_FiveInARow_MakesAShockwaveGemNotALineGem()
        {
            var board = BoardBuilder.FromRows(
                "RYRYR",
                "BBBBB");
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(board, new ScriptedRandom(0, 1, 0, 1, 0), 5, steps,
                new GridPosition(2, 0), new GridPosition(2, 1));

            // Since the shockwave gem exists, a 5-run makes it instead of a line gem; one of the five stays.
            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            Assert.That(result.CellsCleared, Is.EqualTo(4));
            foreach (var step in steps)
            {
                if (step.Kind == BoardStepKind.Create)
                    Assert.That(step.SpecialKind, Is.EqualTo(PieceKind.Shockwave));
            }
        }

        [Test]
        public void Resolve_PredictionPictureQuestionOne_VerticalLineGemClearsItsColumnToo()
        {
            var board = PredictionPicture();
            board.SetPiece(new GridPosition(3, 1), PieceColor.Green, PieceKind.LineVertical);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            // The run (3 cells) plus the other three cells of column 3.
            Assert.That(result.CellsCleared, Is.EqualTo(6));
            Assert.That(result.SpecialsFired, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_PredictionPictureQuestionTwo_ChainReactionClearsTenCells()
        {
            var board = PredictionPicture();
            board.SetPiece(new GridPosition(3, 1), PieceColor.Green, PieceKind.LineVertical);
            board.SetPiece(new GridPosition(3, 3), PieceColor.Blue, PieceKind.LineHorizontal);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            // 6 cells as before, plus the other 4 cells of the top row fired by the second line gem.
            Assert.That(result.CellsCleared, Is.EqualTo(10));
            Assert.That(result.SpecialsFired, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_HorizontalLineGemInARun_ClearsItsWholeRow()
        {
            var board = BoardBuilder.FromRows(
                "RYRYR",
                "BGGGB",
                "YRYRY");
            board.SetPiece(new GridPosition(2, 1), PieceColor.Green, PieceKind.LineHorizontal);

            var result = new TurnResolver(1).Resolve(board, new SeededRandom(1), 5, new List<BoardStep>());

            Assert.That(result.CellsCleared, Is.EqualTo(5));
        }

        [Test]
        public void Resolve_PreferredCellHoldsASpecial_NewGemGoesToTheNextNormalCell()
        {
            var board = FourBlueRow();
            board.SetPiece(new GridPosition(2, 0), PieceColor.Blue, PieceKind.LineHorizontal);
            var steps = new List<BoardStep>();

            var result = new TurnResolver(1).Resolve(board, new ScriptedRandom(0, 1, 0, 1), 5, steps,
                new GridPosition(2, 0), new GridPosition(2, 1));

            // The existing special fires and clears the row; the new one is made on the first normal cell, (0,0).
            Assert.That(result.SpecialsFired, Is.EqualTo(1));
            Assert.That(HasStep(steps, BoardStepKind.Create, new GridPosition(0, 0), PieceKind.LineHorizontal), Is.True);
        }

        [Test]
        public void Resolve_SameSeed_ProducesTheSameStepsWithSpecialGems()
        {
            var first = ResolveSeededTurnWithAFourRun();
            var second = ResolveSeededTurnWithAFourRun();

            Assert.That(first.Count, Is.EqualTo(second.Count));
            for (var i = 0; i < first.Count; i++)
            {
                Assert.That(first[i].Kind, Is.EqualTo(second[i].Kind), $"step {i}");
                Assert.That(first[i].From, Is.EqualTo(second[i].From), $"step {i}");
                Assert.That(first[i].To, Is.EqualTo(second[i].To), $"step {i}");
                Assert.That(first[i].SpecialKind, Is.EqualTo(second[i].SpecialKind), $"step {i}");
            }
        }

        private static List<BoardStep> ResolveSeededTurnWithAFourRun()
        {
            var board = new BoardModel(8, 8);
            board.Fill(new SeededRandom(31), 5);
            for (var x = 0; x < 4; x++)
                board.SetPiece(new GridPosition(x, 0), PieceColor.Red);

            var steps = new List<BoardStep>();
            new TurnResolver().Resolve(board, new SeededRandom(8), 5, steps, new GridPosition(1, 0), new GridPosition(1, 1));
            return steps;
        }

        [Test]
        public void Resolve_ManySeedsWithFourRuns_AlwaysEndsWithAFullBoardAndNoRuns()
        {
            var resolver = new TurnResolver();

            for (var seed = 0; seed < 100; seed++)
            {
                var board = new BoardModel(8, 8);
                board.Fill(new SeededRandom(seed), 4);
                for (var x = 0; x < 4; x++)
                    board.SetPiece(new GridPosition(x, 0), PieceColor.Red);

                var result = resolver.Resolve(board, new SeededRandom(seed + 500), 4, new List<BoardStep>(),
                    new GridPosition(2, 0), new GridPosition(2, 1));

                Assert.That(HasAnyRun(board) && !result.HitLimit, Is.False, $"seed {seed}");
                foreach (var row in BoardBuilder.ToRows(board))
                    Assert.That(row, Does.Not.Contain("."), $"seed {seed}");
            }
        }
    }
}
