using System;
using System.Collections.Generic;
using Game.Core.Common;

namespace Game.Core.Board
{
    /// <summary>Fills empty cells with new random gems.</summary>
    public static class BoardRefill
    {
        /// <summary>
        /// Fills every empty cell with a random colour from the first colorCount colours and records a Spawn step
        /// for each. Unlike the first fill, this may create new runs: that is how cascades happen.
        /// </summary>
        public static int Apply(BoardModel board, IRandom random, int colorCount, List<BoardStep> steps)
        {
            if (colorCount < BoardModel.MinColorCount || colorCount > BoardModel.MaxColorCount)
                throw new ArgumentOutOfRangeException(nameof(colorCount));

            var spawned = 0;

            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    var position = new GridPosition(x, y);
                    if (board.GetPiece(position) != PieceColor.None)
                        continue;

                    // Colour values start at 1 (0 is None), so a random 0..colorCount-1 is shifted up by one.
                    var color = (PieceColor)(random.Next(colorCount) + 1);
                    board.SetPiece(position, color);
                    steps.Add(BoardStep.Spawn(color, position));
                    spawned++;
                }
            }

            return spawned;
        }
    }
}
