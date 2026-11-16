namespace Game.Core.Board
{
    /// <summary>Decides whether a swap between two cells is allowed.</summary>
    public static class SwapRules
    {
        /// <summary>
        /// A swap is valid when both cells are on the board, they are neighbours (not diagonal), they hold
        /// different gems, and the swap creates a run of 3 or more. The board is always left unchanged.
        /// </summary>
        public static bool IsValidSwap(BoardModel board, GridPosition a, GridPosition b)
        {
            if (!a.IsInside(board.Width, board.Height) || !b.IsInside(board.Width, board.Height))
                return false;

            if (!a.IsAdjacent(b))
                return false;

            var first = board.GetPiece(a);
            var second = board.GetPiece(b);
            if (first == PieceColor.None || second == PieceColor.None || first == second)
                return false;

            // Swap, look for a match around both cells, then always swap back: the rules only answer a question.
            board.Swap(a, b);
            var valid = MatchFinder.IsPartOfMatch(board, a) || MatchFinder.IsPartOfMatch(board, b);
            board.Swap(a, b);

            return valid;
        }
    }
}
