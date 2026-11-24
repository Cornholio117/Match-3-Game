using System.Collections.Generic;

namespace Game.Core.Board
{
    /// <summary>Two neighbouring cells the player could swap.</summary>
    public readonly struct SwapMove
    {
        public GridPosition A { get; }
        public GridPosition B { get; }

        public SwapMove(GridPosition a, GridPosition b)
        {
            A = a;
            B = b;
        }
    }

    /// <summary>Finds the swaps a player could make on a board. It never changes the board.</summary>
    public static class MoveFinder
    {
        /// <summary>
        /// Fills results with every valid swap, each pair once (a cell with its right and its upper neighbour). The list
        /// is passed in and reused, so no list is allocated per call.
        /// </summary>
        public static void FindValidSwaps(BoardModel board, List<SwapMove> results)
        {
            results.Clear();

            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    var cell = new GridPosition(x, y);
                    TryAdd(board, results, cell, new GridPosition(x + 1, y));
                    TryAdd(board, results, cell, new GridPosition(x, y + 1));
                }
            }
        }

        /// <summary>True when at least one valid swap exists.</summary>
        public static bool HasValidSwap(BoardModel board)
        {
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    var cell = new GridPosition(x, y);
                    if (SwapRules.IsValidSwap(board, cell, new GridPosition(x + 1, y)) ||
                        SwapRules.IsValidSwap(board, cell, new GridPosition(x, y + 1)))
                        return true;
                }
            }

            return false;
        }

        private static void TryAdd(BoardModel board, List<SwapMove> results, GridPosition a, GridPosition b)
        {
            if (SwapRules.IsValidSwap(board, a, b))
                results.Add(new SwapMove(a, b));
        }
    }
}
