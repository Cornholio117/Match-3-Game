using System;
using Game.Core.Board;

namespace Game.Tests.EditMode.Board
{
    /// <summary>Builds a board from a text picture so tests read as diagrams.</summary>
    /// <remarks>
    /// Rows are written top to bottom, like the screen, so the last row is y = 0. Letters: G green, B blue, R red,
    /// P purple, Y yellow, W white, K pink, '.' empty.
    /// </remarks>
    public static class BoardBuilder
    {
        public static BoardModel FromRows(params string[] rows)
        {
            var height = rows.Length;
            var width = rows[0].Length;
            var board = new BoardModel(width, height);

            for (var row = 0; row < height; row++)
            {
                if (rows[row].Length != width)
                    throw new ArgumentException("Every row of the picture must have the same length.");

                var y = height - 1 - row;
                for (var x = 0; x < width; x++)
                    board.SetPiece(new GridPosition(x, y), ColorOf(rows[row][x]));
            }

            return board;
        }

        private static PieceColor ColorOf(char letter)
        {
            switch (letter)
            {
                case 'G': return PieceColor.Green;
                case 'B': return PieceColor.Blue;
                case 'R': return PieceColor.Red;
                case 'P': return PieceColor.Purple;
                case 'Y': return PieceColor.Yellow;
                case 'W': return PieceColor.White;
                case 'K': return PieceColor.Pink;
                case '.': return PieceColor.None;
                default: throw new ArgumentException($"Unknown colour letter '{letter}'.");
            }
        }
    }
}
