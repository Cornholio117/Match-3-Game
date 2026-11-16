using System.Collections.Generic;

namespace Game.Core.Board
{
    /// <summary>Finds runs of 3 or more gems of the same colour.</summary>
    public static class MatchFinder
    {
        public const int MinRunLength = 3;

        /// <summary>
        /// Fills results with every run of 3 or more in the board's rows and columns. A gem in an L or T shape
        /// belongs to two runs. The list is passed in and reused, so no list is allocated per call.
        /// </summary>
        public static void Find(BoardModel board, List<Match> results)
        {
            results.Clear();

            for (var y = 0; y < board.Height; y++)
                ScanLine(board, results, 0, y, Direction.Right, board.Width);

            for (var x = 0; x < board.Width; x++)
                ScanLine(board, results, x, 0, Direction.Up, board.Height);
        }

        /// <summary>True when the gem at this cell is part of a run of 3 or more, across or up and down.</summary>
        public static bool IsPartOfMatch(BoardModel board, GridPosition position)
        {
            var color = board.GetPiece(position);
            if (color == PieceColor.None)
                return false;

            var across = 1 + CountSame(board, position, Direction.Left, color) + CountSame(board, position, Direction.Right, color);
            if (across >= MinRunLength)
                return true;

            var upDown = 1 + CountSame(board, position, Direction.Up, color) + CountSame(board, position, Direction.Down, color);
            return upDown >= MinRunLength;
        }

        // Counts how many cells in a row, going one way from the position, hold the given colour.
        private static int CountSame(BoardModel board, GridPosition position, Direction direction, PieceColor color)
        {
            var count = 0;
            var next = position.Move(direction);

            while (next.IsInside(board.Width, board.Height) && board.GetPiece(next) == color)
            {
                count++;
                next = next.Move(direction);
            }

            return count;
        }

        // Walks along one row or column, tracking the current run. Reading one step past the end (as None)
        // closes a run that touches the board's edge.
        private static void ScanLine(BoardModel board, List<Match> results, int startX, int startY, Direction direction, int length)
        {
            var runColor = PieceColor.None;
            var runStart = 0;

            for (var i = 0; i <= length; i++)
            {
                var color = i < length ? board.GetPiece(PositionAt(startX, startY, direction, i)) : PieceColor.None;

                if (color == runColor && color != PieceColor.None)
                    continue;

                var runLength = i - runStart;
                if (runColor != PieceColor.None && runLength >= MinRunLength)
                    results.Add(new Match(runColor, PositionAt(startX, startY, direction, runStart), direction, runLength));

                runColor = color;
                runStart = i;
            }
        }

        private static GridPosition PositionAt(int startX, int startY, Direction direction, int index)
        {
            return direction == Direction.Right
                ? new GridPosition(startX + index, startY)
                : new GridPosition(startX, startY + index);
        }
    }
}
