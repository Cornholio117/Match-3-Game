using System.Collections.Generic;
using Game.Core.Board;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class BoardGravityTests
    {
        [Test]
        public void Apply_PredictionPicture_GemsFallToTheFloor()
        {
            var board = BoardBuilder.FromRows(
                "R.B.",
                "..YG",
                "G...",
                ".BR.",
                "Y..K");

            BoardGravity.Apply(board, new List<BoardStep>());

            Assert.That(BoardBuilder.ToRows(board), Is.EqualTo(new[]
            {
                "....",
                "....",
                "R.B.",
                "G.YG",
                "YBRK"
            }));
        }

        [Test]
        public void Apply_PredictionPicture_RecordsOneFallPerMovedGem()
        {
            var board = BoardBuilder.FromRows(
                "R.B.",
                "..YG",
                "G...",
                ".BR.",
                "Y..K");
            var steps = new List<BoardStep>();

            var moved = BoardGravity.Apply(board, steps);

            Assert.That(moved, Is.EqualTo(7));
            Assert.That(steps.Count, Is.EqualTo(7));
            Assert.That(steps.TrueForAll(step => step.Kind == BoardStepKind.Fall), Is.True);
        }

        [Test]
        public void Apply_FallStep_RecordsWhereTheGemLeftAndLanded()
        {
            var board = BoardBuilder.FromRows(
                "B",
                ".",
                ".");
            var steps = new List<BoardStep>();

            BoardGravity.Apply(board, steps);

            Assert.That(steps.Count, Is.EqualTo(1));
            Assert.That(steps[0].Color, Is.EqualTo(PieceColor.Blue));
            Assert.That(steps[0].From, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(steps[0].To, Is.EqualTo(new GridPosition(0, 0)));
        }

        [Test]
        public void Apply_NoGaps_MovesNothing()
        {
            var board = BoardBuilder.FromRows(
                "RB",
                "BR");
            var steps = new List<BoardStep>();

            var moved = BoardGravity.Apply(board, steps);

            Assert.That(moved, Is.EqualTo(0));
            Assert.That(steps, Is.Empty);
        }

        [Test]
        public void Apply_EmptyBoard_MovesNothing()
        {
            var board = BoardBuilder.FromRows(
                "..",
                "..");

            Assert.That(BoardGravity.Apply(board, new List<BoardStep>()), Is.EqualTo(0));
        }
    }
}
