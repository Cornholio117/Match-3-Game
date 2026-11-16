using Game.Core.Board;
using NUnit.Framework;

namespace Game.Tests.EditMode.Board
{
    public sealed class SwapRulesTests
    {
        // Picture 3: only swapping the green with the last red makes RRR.
        private static BoardModel PictureThree()
        {
            return BoardBuilder.FromRows(
                "YBYB",
                "RRGR");
        }

        [Test]
        public void IsValidSwap_SwapMakesARun_ReturnsTrue()
        {
            var board = PictureThree();

            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(2, 0), new GridPosition(3, 0)), Is.True);
        }

        [Test]
        public void IsValidSwap_SwapMakesNoRun_ReturnsFalse()
        {
            var board = PictureThree();

            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(0, 1), new GridPosition(1, 1)), Is.False);
        }

        [Test]
        public void IsValidSwap_VerticalSwapMakesNoRun_ReturnsFalse()
        {
            var board = PictureThree();

            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(2, 0), new GridPosition(2, 1)), Is.False);
        }

        [Test]
        public void IsValidSwap_UpDownSwapMakesARowRun_ReturnsTrue()
        {
            var board = BoardBuilder.FromRows(
                "YBG",
                "GGR");

            // Moving the green down from (2,1) to (2,0) completes the bottom row: G G G.
            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(2, 0), new GridPosition(2, 1)), Is.True);
        }

        [Test]
        public void IsValidSwap_SideSwapMakesAColumnRun_ReturnsTrue()
        {
            var board = BoardBuilder.FromRows(
                "RBY",
                "RYB",
                "BRY");

            // Moving the red from (1,0) to (0,0) completes the left column: R R R.
            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(0, 0), new GridPosition(1, 0)), Is.True);
        }

        [Test]
        public void IsValidSwap_CellsNotNeighbours_ReturnsFalse()
        {
            var board = BoardBuilder.FromRows(
                "RRYR");

            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(0, 0), new GridPosition(3, 0)), Is.False);
        }

        [Test]
        public void IsValidSwap_DiagonalCells_ReturnsFalse()
        {
            var board = BoardBuilder.FromRows(
                "RBR",
                "BRB");

            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(0, 0), new GridPosition(1, 1)), Is.False);
        }

        [Test]
        public void IsValidSwap_OutsideTheBoard_ReturnsFalse()
        {
            var board = PictureThree();

            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(3, 0), new GridPosition(4, 0)), Is.False);
        }

        [Test]
        public void IsValidSwap_SameColour_ReturnsFalse()
        {
            var board = BoardBuilder.FromRows(
                "RRB");

            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(0, 0), new GridPosition(1, 0)), Is.False);
        }

        [Test]
        public void IsValidSwap_EmptyCell_ReturnsFalse()
        {
            var board = BoardBuilder.FromRows(
                "RR.R");

            Assert.That(SwapRules.IsValidSwap(board, new GridPosition(2, 0), new GridPosition(3, 0)), Is.False);
        }

        [Test]
        public void IsValidSwap_ValidSwap_LeavesTheBoardUnchanged()
        {
            var board = PictureThree();

            SwapRules.IsValidSwap(board, new GridPosition(2, 0), new GridPosition(3, 0));

            Assert.That(board.GetPiece(new GridPosition(2, 0)), Is.EqualTo(PieceColor.Green));
            Assert.That(board.GetPiece(new GridPosition(3, 0)), Is.EqualTo(PieceColor.Red));
        }
    }
}
