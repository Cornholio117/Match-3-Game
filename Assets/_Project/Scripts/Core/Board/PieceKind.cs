namespace Game.Core.Board
{
    /// <summary>
    /// What a gem does besides having a colour. A line gem clears its row or column when it is matched; a diagonal
    /// gem clears both diagonals through its cell.
    /// </summary>
    public enum PieceKind
    {
        Normal = 0,
        LineHorizontal = 1,
        LineVertical = 2,
        Diagonal = 3
    }
}
