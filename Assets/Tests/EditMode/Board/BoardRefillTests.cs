using System.Collections.Generic;
using Game.Core.Board;
using Game.Core.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class BoardRefillTests
    {
        private static BoardModel PostGravityBoard()
        {
            return BoardBuilder.FromRows(
                "....",
                "....",
                "R.B.",
                "G.YG",
                "YBRK");
        }

        [Test]
        public void Apply_PostGravityBoard_SpawnsElevenAndFillsEveryCell()
        {
            var board = PostGravityBoard();
            var steps = new List<BoardStep>();

            var spawned = BoardRefill.Apply(board, new SeededRandom(5), 4, steps);

            Assert.That(spawned, Is.EqualTo(11));
            Assert.That(steps.Count, Is.EqualTo(11));
            foreach (var row in BoardBuilder.ToRows(board))
                Assert.That(row, Does.Not.Contain("."));
        }

        [Test]
        public void Apply_ExistingGems_AreNotChanged()
        {
            var board = PostGravityBoard();

            BoardRefill.Apply(board, new SeededRandom(5), 4, new List<BoardStep>());

            // Rows are top to bottom: index 4 is y = 0. Only the empty cells may change.
            var rows = BoardBuilder.ToRows(board);
            Assert.That(rows[4], Is.EqualTo("YBRK"));
            Assert.That(rows[3][0], Is.EqualTo('G'));
            Assert.That(rows[3][2], Is.EqualTo('Y'));
            Assert.That(rows[3][3], Is.EqualTo('G'));
            Assert.That(rows[2][0], Is.EqualTo('R'));
            Assert.That(rows[2][2], Is.EqualTo('B'));
        }

        [Test]
        public void Apply_SameSeed_ProducesSameBoard()
        {
            var first = PostGravityBoard();
            var second = PostGravityBoard();

            BoardRefill.Apply(first, new SeededRandom(11), 5, new List<BoardStep>());
            BoardRefill.Apply(second, new SeededRandom(11), 5, new List<BoardStep>());

            Assert.That(BoardBuilder.ToRows(first), Is.EqualTo(BoardBuilder.ToRows(second)));
        }

        [Test]
        public void Apply_FourColours_OnlyUsesTheFirstFour()
        {
            var board = new BoardModel(8, 8);

            BoardRefill.Apply(board, new SeededRandom(3), 4, new List<BoardStep>());

            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                    Assert.That((int)board.GetPiece(new GridPosition(x, y)), Is.InRange(1, 4));
            }
        }

        [Test]
        public void Apply_ScriptedRandom_PlacesTheScriptedColours()
        {
            var board = BoardBuilder.FromRows(
                "..",
                "..");

            // 0 becomes Green (the first colour), 1 becomes Blue; cells are filled column by column, bottom up.
            BoardRefill.Apply(board, new Game.Tests.EditMode.Common.ScriptedRandom(0, 1, 1, 0), 4, new List<BoardStep>());

            Assert.That(BoardBuilder.ToRows(board), Is.EqualTo(new[] { "BG", "GB" }));
        }

        [Test]
        public void Apply_TooFewColours_Throws()
        {
            var board = new BoardModel(2, 2);

            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => BoardRefill.Apply(board, new SeededRandom(1), 2, new List<BoardStep>()));
        }
    }
}
