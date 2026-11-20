using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using Game.Tests.EditMode.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class TurnResolverTests
    {
        // A green run on the floor; everything above it falls one row when it clears.
        private static BoardModel GreenRunOnTheFloor()
        {
            return BoardBuilder.FromRows(
                "BRB",
                "RBR",
                "GGG");
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

        [Test]
        public void Resolve_NoRuns_DoesNothing()
        {
            var board = BoardBuilder.FromRows(
                "RBR",
                "BRB");
            var steps = new List<BoardStep>();

            var result = new TurnResolver().Resolve(board, new SeededRandom(1), 4, steps);

            Assert.That(result.Cascades, Is.EqualTo(0));
            Assert.That(result.CellsCleared, Is.EqualTo(0));
            Assert.That(result.HitLimit, Is.False);
            Assert.That(steps, Is.Empty);
        }

        [Test]
        public void Resolve_OneRun_ClearsDropsAndRefills()
        {
            var board = GreenRunOnTheFloor();
            var steps = new List<BoardStep>();

            // Spawns become Green, Blue, Green (0, 1, 0): no new run, so exactly one wave.
            var result = new TurnResolver().Resolve(board, new ScriptedRandom(0, 1, 0), 5, steps);

            Assert.That(result.Cascades, Is.EqualTo(1));
            Assert.That(result.CellsCleared, Is.EqualTo(3));
            Assert.That(Count(steps, BoardStepKind.Clear), Is.EqualTo(3));
            Assert.That(Count(steps, BoardStepKind.Fall), Is.EqualTo(6));
            Assert.That(Count(steps, BoardStepKind.Spawn), Is.EqualTo(3));
            Assert.That(BoardBuilder.ToRows(board), Is.EqualTo(new[] { "GBG", "BRB", "RBR" }));
        }

        [Test]
        public void Resolve_RefillCreatesARun_CascadesAgain()
        {
            var board = GreenRunOnTheFloor();

            // First spawns are Blue, Blue, Blue (1, 1, 1): a new run on the top row. The second wave's spawns
            // are Green, Red, Green (0, 2, 0): no run, so the loop stops after two waves.
            var result = new TurnResolver().Resolve(board, new ScriptedRandom(1, 1, 1, 0, 2, 0), 5, new List<BoardStep>());

            Assert.That(result.Cascades, Is.EqualTo(2));
            Assert.That(result.CellsCleared, Is.EqualTo(6));
            Assert.That(result.HitLimit, Is.False);
            Assert.That(HasAnyRun(board), Is.False);
        }

        [Test]
        public void Resolve_LimitReached_StopsAndReportsIt()
        {
            var board = GreenRunOnTheFloor();

            // The same forced cascade, but only one wave is allowed, so the new blue run is left on the board.
            var result = new TurnResolver(1).Resolve(board, new ScriptedRandom(1, 1, 1, 0, 2, 0), 5, new List<BoardStep>());

            Assert.That(result.Cascades, Is.EqualTo(1));
            Assert.That(result.HitLimit, Is.True);
            Assert.That(HasAnyRun(board), Is.True);
        }

        [Test]
        public void Resolve_LShape_ClearsEachCellOnce()
        {
            var board = BoardBuilder.FromRows(
                "GRYRY",
                "GYRYR",
                "GGGRY",
                "RYRYR");
            var steps = new List<BoardStep>();

            // Five distinct cells make up the L and the corner belongs to both runs. One of the five becomes a diagonal
            // gem and stays; the other four clear, each exactly once.
            var result = new TurnResolver(1).Resolve(board, new SeededRandom(2), 5, steps);

            Assert.That(result.SpecialsCreated, Is.EqualTo(1));
            Assert.That(result.CellsCleared, Is.EqualTo(4));
            Assert.That(Count(steps, BoardStepKind.Clear), Is.EqualTo(4));
        }

        [Test]
        public void Resolve_SameSeed_ProducesTheSameSteps()
        {
            var first = new List<BoardStep>();
            var second = new List<BoardStep>();

            ResolveSeededTurn(first);
            ResolveSeededTurn(second);

            Assert.That(first.Count, Is.EqualTo(second.Count));
            for (var i = 0; i < first.Count; i++)
            {
                Assert.That(first[i].Kind, Is.EqualTo(second[i].Kind), $"step {i}");
                Assert.That(first[i].Color, Is.EqualTo(second[i].Color), $"step {i}");
                Assert.That(first[i].From, Is.EqualTo(second[i].From), $"step {i}");
                Assert.That(first[i].To, Is.EqualTo(second[i].To), $"step {i}");
            }
        }

        private static void ResolveSeededTurn(List<BoardStep> steps)
        {
            var board = new BoardModel(8, 8);
            board.Fill(new SeededRandom(77), 5);

            // Force a run on the bottom row so there is always something to resolve.
            board.SetPiece(new GridPosition(0, 0), PieceColor.Green);
            board.SetPiece(new GridPosition(1, 0), PieceColor.Green);
            board.SetPiece(new GridPosition(2, 0), PieceColor.Green);

            new TurnResolver().Resolve(board, new SeededRandom(99), 5, steps);
        }

        [Test]
        public void Resolve_ManySeeds_AlwaysEndsWithNoRunsOrAReportedLimit()
        {
            var resolver = new TurnResolver();
            var steps = new List<BoardStep>();

            for (var seed = 0; seed < 100; seed++)
            {
                var board = new BoardModel(8, 8);
                board.Fill(new SeededRandom(seed), 4);
                board.SetPiece(new GridPosition(0, 0), PieceColor.Red);
                board.SetPiece(new GridPosition(1, 0), PieceColor.Red);
                board.SetPiece(new GridPosition(2, 0), PieceColor.Red);
                steps.Clear();

                var result = resolver.Resolve(board, new SeededRandom(seed + 1000), 4, steps);

                Assert.That(HasAnyRun(board) && !result.HitLimit, Is.False, $"seed {seed}");
                Assert.That(result.Cascades, Is.GreaterThanOrEqualTo(1), $"seed {seed}");
            }
        }

        [Test]
        public void Constructor_LimitBelowOne_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new TurnResolver(0));
        }
    }
}
