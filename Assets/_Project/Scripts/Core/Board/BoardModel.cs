using System;
using Game.Core.Common;

namespace Game.Core.Board
{
    /// <summary>The grid of gems. It stores the cells and fills them; swapping and matching live in other classes.</summary>
    public sealed class BoardModel
    {
        public const int MinColorCount = 3;
        public const int MaxColorCount = 7;

        private readonly PieceColor[,] _cells;

        public int Width { get; }
        public int Height { get; }

        public BoardModel(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            _cells = new PieceColor[width, height];
        }

        public PieceColor GetPiece(GridPosition position)
        {
            if (!position.IsInside(Width, Height))
                throw new ArgumentOutOfRangeException(nameof(position));

            return _cells[position.X, position.Y];
        }

        public void SetPiece(GridPosition position, PieceColor color)
        {
            if (!position.IsInside(Width, Height))
                throw new ArgumentOutOfRangeException(nameof(position));

            _cells[position.X, position.Y] = color;
        }

        /// <summary>Exchanges the gems in two cells. It does not check the swap rules; see SwapRules.</summary>
        public void Swap(GridPosition a, GridPosition b)
        {
            var first = GetPiece(a);
            var second = GetPiece(b);
            _cells[a.X, a.Y] = second;
            _cells[b.X, b.Y] = first;
        }

        /// <summary>
        /// Fills every cell with a random colour from the first colorCount colours, without creating any row
        /// or column of three. The same random sequence always gives the same board.
        /// </summary>
        public void Fill(IRandom random, int colorCount)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            if (colorCount < MinColorCount || colorCount > MaxColorCount)
                throw new ArgumentOutOfRangeException(nameof(colorCount));

            // Bottom-left to top-right, so the two cells to the left and the two below are always already filled.
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var bannedByRow = BannedByTwoBehind(x, y, 1, 0);
                    var bannedByColumn = BannedByTwoBehind(x, y, 0, 1);
                    _cells[x, y] = PickColor(random, colorCount, bannedByRow, bannedByColumn);
                }
            }
        }

        /// <summary>
        /// Returns the colour that must not be placed at (x, y) because the two cells behind it (in the given
        /// direction) already match, or None if nothing is banned.
        /// </summary>
        private PieceColor BannedByTwoBehind(int x, int y, int stepX, int stepY)
        {
            var firstX = x - stepX;
            var firstY = y - stepY;
            var secondX = x - 2 * stepX;
            var secondY = y - 2 * stepY;

            // Near the edge there are not two cells behind, so nothing is banned.
            if (secondX < 0 || secondY < 0)
                return PieceColor.None;

            var first = _cells[firstX, firstY];
            var second = _cells[secondX, secondY];

            if (first == second)
                return first;

            return PieceColor.None;
        }

        // At least 3 colours are required, so with at most 2 banned a choice always remains.
        private static PieceColor PickColor(IRandom random, int colorCount, PieceColor bannedA, PieceColor bannedB)
        {
            var allowed = colorCount;
            if (bannedA != PieceColor.None)
                allowed--;

            if (bannedB != PieceColor.None && bannedB != bannedA)
                allowed--;

            // Pick the n-th allowed colour, skipping the banned ones.
            var pick = random.Next(allowed);
            for (var value = 1; value <= colorCount; value++)
            {
                var color = (PieceColor)value;
                if (color == bannedA || color == bannedB)
                    continue;

                if (pick == 0)
                    return color;

                pick--;
            }

            throw new InvalidOperationException("No colour available for the cell.");
        }
    }
}
