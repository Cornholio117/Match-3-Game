using System;
using System.Collections.Generic;
using Game.Core.Common;

namespace Game.Core.Board
{
    /// <summary>What happened when a swap was tried: whether it was allowed, and if so how the turn played out.</summary>
    public readonly struct SwapOutcome
    {
        public bool Accepted { get; }
        public TurnResult Turn { get; }

        public SwapOutcome(bool accepted, TurnResult turn)
        {
            Accepted = accepted;
            Turn = turn;
        }
    }

    /// <summary>One game's board: tries swaps and resolves the turn they cause. The view plays back the recorded steps.</summary>
    public sealed class BoardGame
    {
        private readonly IRandom _random;
        private readonly int _colorCount;
        private readonly TurnResolver _resolver;

        public BoardModel Board { get; }

        public BoardGame(BoardModel board, int colorCount, IRandom random, int maxCascades = TurnResolver.DefaultMaxCascades)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (random == null)
                throw new ArgumentNullException(nameof(random));

            if (colorCount < BoardModel.MinColorCount || colorCount > BoardModel.MaxColorCount)
                throw new ArgumentOutOfRangeException(nameof(colorCount));

            Board = board;
            _colorCount = colorCount;
            _random = random;
            _resolver = new TurnResolver(maxCascades);
        }

        /// <summary>Creates a game on a new board filled with random gems and no starting runs.</summary>
        public static BoardGame CreateFilled(int width, int height, int colorCount, IRandom random, int maxCascades = TurnResolver.DefaultMaxCascades)
        {
            var board = new BoardModel(width, height);
            board.Fill(random, colorCount);
            return new BoardGame(board, colorCount, random, maxCascades);
        }

        /// <summary>
        /// Tries to swap two cells. If the swap is not allowed the board is untouched and steps is left empty;
        /// otherwise the swap is made, the turn is resolved, and steps holds everything that happened after the swap.
        /// </summary>
        public SwapOutcome TrySwap(GridPosition a, GridPosition b, List<BoardStep> steps)
        {
            steps.Clear();

            if (!SwapRules.IsValidSwap(Board, a, b))
                return new SwapOutcome(false, default);

            // Two special gems swapped together trigger their combo instead of needing a match.
            var combo = Board.GetKind(a) != PieceKind.Normal && Board.GetKind(b) != PieceKind.Normal;

            Board.Swap(a, b);
            var turn = _resolver.Resolve(Board, _random, _colorCount, steps, a, b, combo);
            return new SwapOutcome(true, turn);
        }
    }
}
