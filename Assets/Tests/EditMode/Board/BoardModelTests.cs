using Game.Core.Board;
using Game.Core.Common;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class BoardModelTests
    {
        private const int Size = 8;

        private static BoardModel FilledBoard(int seed, int colorCount)
        {
            var board = new BoardModel(Size, Size);
            board.Fill(new SeededRandom(seed), colorCount);
            return board;
        }

        // Simple scan used only to check the fill; the real match finder comes next.
        private static bool HasRunOfThree(BoardModel board)
        {
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    var color = board.GetPiece(new GridPosition(x, y));

                    if (x + 2 < board.Width
                        && color == board.GetPiece(new GridPosition(x + 1, y))
                        && color == board.GetPiece(new GridPosition(x + 2, y)))
                        return true;

                    if (y + 2 < board.Height
                        && color == board.GetPiece(new GridPosition(x, y + 1))
                        && color == board.GetPiece(new GridPosition(x, y + 2)))
                        return true;
                }
            }

            return false;
        }

        [Test]
        public void Constructor_NonPositiveSize_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new BoardModel(0, 8));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new BoardModel(8, 0));
        }

        [Test]
        public void GetPiece_NewBoard_IsEmpty()
        {
            var board = new BoardModel(Size, Size);

            Assert.That(board.GetPiece(new GridPosition(3, 4)), Is.EqualTo(PieceColor.None));
        }

        [Test]
        public void GetPiece_OutsideBoard_Throws()
        {
            var board = new BoardModel(Size, Size);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => board.GetPiece(new GridPosition(8, 0)));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => board.GetPiece(new GridPosition(0, -1)));
        }

        [Test]
        public void Fill_AnyBoard_EveryCellHasAColour()
        {
            var board = FilledBoard(1, 5);

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                    Assert.That(board.GetPiece(new GridPosition(x, y)), Is.Not.EqualTo(PieceColor.None));
            }
        }

        [Test]
        public void Fill_SameSeed_ProducesSameBoard()
        {
            var first = FilledBoard(123, 5);
            var second = FilledBoard(123, 5);

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var position = new GridPosition(x, y);
                    Assert.That(first.GetPiece(position), Is.EqualTo(second.GetPiece(position)));
                }
            }
        }

        [Test]
        public void Fill_DifferentSeeds_ProducesDifferentBoards()
        {
            var first = FilledBoard(1, 5);
            var second = FilledBoard(2, 5);
            var differences = 0;

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var position = new GridPosition(x, y);
                    if (first.GetPiece(position) != second.GetPiece(position))
                        differences++;
                }
            }

            Assert.That(differences, Is.GreaterThan(0));
        }

        [Test]
        public void Fill_FourColours_UsesOnlyTheFirstFour()
        {
            var board = FilledBoard(9, 4);

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                    Assert.That((int)board.GetPiece(new GridPosition(x, y)), Is.InRange(1, 4));
            }
        }

        // Three colours is the hardest case: the most colours get banned, but a choice must always remain.
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(7)]
        public void Fill_ManySeeds_NeverStartsWithARunOfThree(int colorCount)
        {
            for (var seed = 0; seed < 200; seed++)
                Assert.That(HasRunOfThree(FilledBoard(seed, colorCount)), Is.False, $"seed {seed}");
        }

        [TestCase(2)]
        [TestCase(8)]
        public void Fill_ColourCountOutOfRange_Throws(int colorCount)
        {
            var board = new BoardModel(Size, Size);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => board.Fill(new SeededRandom(1), colorCount));
        }
    }
}
