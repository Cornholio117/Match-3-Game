using System.Collections.Generic;

namespace Game.Core.Board
{
    /// <summary>Lets gems fall straight down into empty cells, one column at a time.</summary>
    public static class BoardGravity
    {
        /// <summary>Applies gravity and records a Fall step for every gem that moved. Returns how many moved.</summary>
        public static int Apply(BoardModel board, List<BoardStep> steps)
        {
            var moved = 0;

            for (var x = 0; x < board.Width; x++)
            {
                // Walk up the column; each gem drops to the lowest free cell, so the order of gems never changes.
                var restingY = 0;
                for (var y = 0; y < board.Height; y++)
                {
                    var from = new GridPosition(x, y);
                    var color = board.GetPiece(from);
                    if (color == PieceColor.None)
                        continue;

                    if (y != restingY)
                    {
                        var to = new GridPosition(x, restingY);
                        board.SetPiece(to, color);
                        board.SetPiece(from, PieceColor.None);
                        steps.Add(BoardStep.Fall(color, from, to));
                        moved++;
                    }

                    restingY++;
                }
            }

            return moved;
        }
    }
}
